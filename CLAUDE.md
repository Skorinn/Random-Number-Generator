# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Windows Forms desktop app (.NET Framework 4.8, C#) that reads bit averages from a TruRNGpro USB hardware
random number generator (or a built-in simulator), records them to XML session files, and charts/analyzes
the results. The device access layer is a native C++ DLL (`TruRNGpro`) called through P/Invoke.

Three projects in `RandomNumberGenerator.sln`:
- `RandomNumberGenerator.csproj` — WinForms app, AnyCPU (64-bit; `Prefer32Bit=false`)
- `TruRNGpro/TruRNGpro.vcxproj` — native C++ DLL, **x64 only** (solution maps `Any CPU` → `x64`)
- `RandomNumberGenerator.Test/` — MSTest + Moq unit tests

## Build and test

MSBuild/vstest are not on PATH; use the full VS paths (adjust the VS edition/version if different):

```powershell
$msbuild = "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
$vstest  = "C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

# Restore (packages/ is gitignored; packages.config style, so NuGet CLI or VS restore — not `dotnet restore`)
nuget restore RandomNumberGenerator.sln

# Build everything (do NOT use `dotnet build` — old-style projects plus a vcxproj)
& $msbuild RandomNumberGenerator.sln /p:Configuration=Debug /p:Platform="Any CPU"

# Run all tests
& $vstest "RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll"

# Run a single test / class / category
& $vstest "RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll" /Tests:WriteDataPoint_ValidWriter_Success
& $vstest "RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll" /TestCaseFilter:"FullyQualifiedName~RNGXMLWriterTests"
& $vstest "RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll" /TestCaseFilter:"TestCategory=Component"
```

Build ordering matters: post-build events copy the app output to `<sln>\bin\`, and the test project copies
`<sln>\bin\TruRNGpro.dll` into its own output. Build the whole solution (not just the test project) or any
test that reaches the P/Invoke layer will fail to load the native DLL.

App output: `bin\Debug\Random Number Generator.exe` (also copied to `<sln>\bin\`).

## Architecture

### Composition root
`Program.cs` builds the whole object graph by hand and injects it — there is no DI container:

```
RNGXMLWriter + RNGXMLReader → RNGSessionDataFile → ┐
                              RNGSessionTimer    → ┴→ RNGSessionData → ┐
                                                      RNGDeviceTimer → ┴→ GeneratorForm
```

Nearly every class has a matching `I…` interface (`IRNGSessionData`, `IRNGSessionDataFile`,
`IRNGSessionFileWriter`/`IRNGSessionFileReader`, `IRNGSessionTimer`, `IRNGDeviceTimer`, `IXMLDataPoint`,
`IRNGDevice`, `IGeneratorForm`). These exist so the tests can mock collaborators — when adding a
collaborator, add the interface, inject it through the constructor, and null-validate it there.
`IGeneratorForm` derives from `ISynchronizeInvoke` specifically so `DeviceUpdateThread` can be tested
without a real form.

### Acquisition data flow (the hot path)
`RNGDeviceTimer` (WinForms `Timer`, 100 ms, so ticks on the UI thread) → P/Invoke
`TruRNGpro.dll!GetRandomBitAverage` → `DeviceReadCallbackDelegate` → `GeneratorForm.RecordReadResult` →
`RNGSessionData.AddDataPoint` → (a) `RecordDataPoint` updates the rolling window + stats and fires
`DataPointAddedCallback` → `GeneratorForm.OnDataPointAdded` → `RNGChart.AddPoint`; (b) `WritePendingData`
flushes to `RNGSessionDataFile` → `RNGXMLWriter` once `WRITE_FILE_INTERVAL` points are pending.

`double.MaxValue` is the sentinel for a failed device read: it aborts the run, forces the interface to be
re-initialized, and shows an error in the status box. `TargetValues.NO_VALUE_SET` (-2) is the "no target"
sentinel.

### Session data model
`RNGSessionData` keeps a bounded `ConcurrentQueue<double>` (`DataWindowSize`, default 6144, overwritten from
`RNGChart.MaxDataSize` at form construction). Stats (average, mean deviation, standard deviation) are
recomputed on every point; the queue is walked under `m_DataLock` even though it is concurrent, and scalars
are published with `Interlocked.Exchange`.

### File format and append semantics
Session files are XML: a single `<Session Simulated="true" Target="0">` element containing
`<Data Time="hh:mm:ss">0.123</Data>` children. Element/attribute names live in `XMLConstants` — use them
rather than string literals.

Loading an existing file (`RNGSessionData.LoadSession` → `RNGSessionDataFile.LoadSession` →
`RNGXMLReader.LoadFile`) streams data points in batches through `LoadDataPointsBatch`, which fires the same
`DataPointAddedCallback` so the chart fills in as it loads. Afterwards `RNGXMLWriter.PrepareForAppend`
rewrites the file **textually** — truncating at the last `</Session>`, or converting a self-closing
`<Session … />` into an open tag — so writing can resume. Changes to the session element shape must keep
that text surgery in sync.

### GUI
`GeneratorForm` is a state machine over `RngGuiStates` (Idle / Running / Paused / Terminating); the
`Set*State()` methods own all control enable/disable and are the right place to hook new UI state.
`GeneratorForm.Designer.cs` is designer-generated — prefer editing it through the VS designer.

Device discovery is separate: `DeviceUpdateThread` (a static class queued on the thread pool) enumerates
`Win32_USBControllerDevice`/`Win32_PnPEntity` via WMI, regex-matches `USB…Serial…COM<n>`, and pushes a
`BindingList<IRNGDevice>` back through `IGeneratorForm`. It is re-triggered from `WndProc` on
`WM_DEVICECHANGE`, and checks `Terminating` between items so shutdown is not blocked.

The analysis UI (baseline vs. result comparison) uses `StatisticalAnalysis` (MathNet.Numerics
`DescriptiveStatistics`) and `HistogramChart`; both charts derive from
`System.Windows.Forms.DataVisualization.Charting.Chart`.

### Threading rules
- `RNGDeviceTimer` uses `System.Windows.Forms.Timer` → its tick is already on the UI thread.
- `RNGSessionTimer` uses `System.Timers.Timer` (deliberately, for reliable ticks) → it fires on a pool
  thread, so anything touching controls goes through `InvokeRequired`/`Invoke`.
- Background work (device enumeration, file loading) marshals UI updates the same way; the `On*Completed`
  callbacks are the established pattern.

### Native layer
`TruRNGproMain.cpp` exports `Initialize(int iPort, bool bSimulate)` and `GetRandomBitAverage(double&)`.
`Initialize` picks `TruRNGpro` (real device, via the third-party `rng.h`) or `RNGSimulator` (seeded Mersenne
twister — the seed comes from the "Seed" box that replaces "Port" in simulate mode) behind the
`RNGInterface` base class. Note the two `DllImport`s in `RNGDeviceTimer.cs` declare different calling
conventions (`Winapi` vs `Cdecl`).

`Externals/CommonControls.dll` and `Externals/DeviceInterfaces.dll` are checked-in binaries with no source
here (`DeviceInterfaces` supplies the `USBDeviceNotification` constants used in `WndProc`).

## Coding conventions

`CODING_GUIDELINES.md` (app), `RandomNumberGenerator.Test/CODING_GUIDELINES_TESTS.md` (tests), and
`TruRNGpro/CODING_GUIDELINES_CPP.md` (native) are authoritative and are actually followed throughout. The
non-obvious rules new code is expected to match:

- Standardized file header block on every file (name, description, copyright, revision history).
- Modified Hungarian notation: `m_` + type char for members (`m_sFilePath`, `m_iPort`, `m_bValid`,
  `m_Timer` for objects), and `s`/`i`/`f`/`b` prefixes on locals.
- `#region` blocks in a fixed order: Type definitions, Constructors, Event Handlers, Methods, Properties,
  Constants, Data Members.
- XML doc comments on public members, with `IN`/`OUT`/`INOUT` on every `<param>` and `<exception>` tags for
  everything explicitly thrown.
- No function calls inside conditionals — assign to a `bool b…` first. Yoda-style comparisons
  (`if (null == x)`, `if (false == bStatus)`). Explicit parentheses in compound/boolean expressions.
- `string.Empty` over `""`; string interpolation over concatenation; named `const` locals instead of bare
  `true`/`false`/magic numbers at call sites; `_ = sender;` to discard unused event parameters.
- Errors bubble up: throw with a user-facing message (leading space is the house style) rather than
  swallowing and returning `false`; the form catches them and renders via `SetStatusBoxError`.
- Tests: `[ClassName]Tests`, `[Method]_[Scenario]_[ExpectedResult]`, banner-commented Arrange/Act/Assert
  sections, Moq for collaborators, `[TestCategory("Component")]`.
