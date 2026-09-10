# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Windows Forms desktop app (.NET Framework 4.8, C#) that reads bit averages from a TruRNGpro USB hardware
random number generator (or a built-in simulator), records them to XML session files, and charts/analyzes
the results. The device access layer is a native C++ DLL (`TruRNGpro`) called through P/Invoke.

Three projects in `RandomNumberGenerator.sln`:
- `RandomNumberGenerator.csproj` — WinForms app, AnyCPU (64-bit; `Prefer32Bit=false`)
- `TruRNGpro/TruRNGpro.vcxproj` — native C++ DLL, **x64 only** (solution maps `Any CPU` → `x64`)
- `tests/RandomNumberGenerator.Test/` — MSTest + Moq unit tests

Both kinds of test live under `tests/`: the unit tests above, and `tests/manual/`, which drives the built
application and is described in its own README.

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
& $vstest "tests\RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll"

# Run a single test / class / category
& $vstest "tests\RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll" /Tests:WriteDataPoint_ValidWriter_Success
& $vstest "tests\RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll" /TestCaseFilter:"FullyQualifiedName~RNGXMLWriterTests"
& $vstest "tests\RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll" /TestCaseFilter:"TestCategory=Component"
```

Build ordering matters. The native project copies `TruRNGpro.dll` to `<sln>\bin\`, and both the app and the
test project copy it from there into their own output, because a native DLL is looked for beside the
executable that loads it. Build the whole solution — building either managed project alone leaves the DLL
missing and anything reaching the P/Invoke layer fails with `DllNotFoundException`.

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
`RNGChart.MaxDataSize` — 1,024,000 — at form construction). Stats (average, mean deviation, standard
deviation) are recomputed by walking the whole window: once per point while recording, but only once per
batch in `LoadDataPointsBatch`, which is what keeps loading a large file from taking time that grows with
the square of its size. The queue is walked under `m_DataLock` even though it is concurrent, and scalars are
published with `Interlocked.Exchange`.

Recording therefore costs a walk of the window per reading, reaching about 10% of the 100 ms interval once
the window is full — around 28 hours of recording. Removing that needs rolling sums, which change the
computed values and are deliberately not done; `MaxPoint`/`MinPoint` return `double.NaN` when there is no
data rather than throwing.

### File format and append semantics
Session files are XML: a single `<Session Simulated="true" Target="0">` element containing
`<Data Time="hh:mm:ss">0.123</Data>` children. Element/attribute names live in `XMLConstants` — use them
rather than string literals.

Values are written and parsed with the invariant culture, so a file written where the decimal separator is a
comma reads back as the same numbers elsewhere. `Target="-1"` means no target; `TargetValues.NO_VALUE_SET`
(-2) is internal and is never written to a file.

Loading an existing file (`RNGSessionData.LoadSession` → `RNGSessionDataFile.LoadSession` →
`RNGXMLReader.LoadFile`) streams data points in batches through `LoadDataPointsBatch`, which fires the same
`DataPointAddedCallback` so the chart fills in as it loads. Afterwards `RNGXMLWriter.PrepareForAppend`
edits the file **textually** so writing can resume, handling three shapes:

1. a closed session, where the closing tag and the whitespace before it are truncated away;
2. an empty session that closed itself, where `<Session … />` is reopened;
3. a session left unterminated by the application stopping, where anything after the last complete
   `</Data>` is discarded so a half-written element cannot be appended to.

Only the ends of the file are read rather than the whole of it, because these files grow by roughly 2MB an
hour. Changes to the session element shape must keep that text surgery in sync.

Starting a session goes down the same road. `RNGSessionDataFile.StartSession` asks whether the file already
holds readings and, if it does, prepares it for appending rather than calling `WriteSessionStart`, which
opens the file from the beginning and would throw them away. That could not happen while ending a session
also gave up the chosen file, because the only way back to a file was to load it; the file now stays chosen
after a session ends, so pressing Start again has to be safe on its own. `RNGXMLWriter.WriteSessionEnd`
keeps `FilePath` for the same reason — what ends there is the session, not the choice of file.

A second session appended this way joins the session element already in the file rather than opening one of
its own, so the recorded `Simulated` and `Target` stay the first session's. `CheckTargetChanged` is what
keeps that honest: it asks before recording against a target the file disagrees with.

A file of the third shape is what an interrupted session leaves, and it is recovered rather than rejected:
the reader keeps every complete data point, refuses a partially written one, and reports the recovery
through `LastError`, which the form shows to the user. A file that closes its session and still fails to
parse is damaged rather than unfinished and is rejected as before. The two are told apart by the shape of
the file, not by the parse error.

### GUI
`GeneratorForm` is a state machine over `RngGuiStates` (Idle / Running / Paused / Terminating); the
`Set*State()` methods own all control enable/disable, which button `SetPrimaryButton` emphasises, and the
window title, and are the right place to hook new UI state.
`GeneratorForm.Designer.cs` is designer-generated — prefer editing it through the VS designer.

Each tab is a `TableLayoutPanel` of bands: fixed-height rows for the settings and the readouts, and a
percent row underneath that the chart fills, so the chart takes whatever space is left and grows with the
window. Nothing is positioned absolutely at the tab level any more, and the anchors that used to be set on
a fixed-size dialog are gone. The window is sizable with a `MinimumSize`, and `RestoreWindowPlacement` /
`SaveWindowPlacement` remember its geometry in `Properties.Settings` — a saved position for a screen that
is no longer attached is ignored rather than opening the window off-screen.

Status messages go to a `StatusStrip` docked to the form, not to the tab.

A session can be given a length in minutes, zero meaning it records until Stop is pressed.
`CheckSessionLength` is called from `RecordReadResult` as each reading arrives, so the stop happens on the
thread that owns the form and needs no marshalling, and it measures `IRNGSessionTimer.ElapsedSeconds` rather
than counting readings: the device delivers about nine readings a second against the ten the timer asks for,
so readings are not a clock. The session timer does not run while a session is paused, so a pause does not
spend the length.

The chart and the statistics beside it are drawn from the same readings, so they are cleared together and at
the same moment — when a file is chosen, in `FileBrowseButton_Click`. Starting a session leaves both alone,
which is what lets a second session into the same file carry on from the first. Clearing one without the
other, which is what starting a session used to do, leaves an empty chart beside a count of several hundred.

Colour comes from two palettes, both of them properties rather than fields so that they report the scheme
in force now: `UiPalette` for the chrome, every value of it taken from `SystemColors`, and `StatusPalette`
for severity. Take text from the same pair as the surface behind it — `Card`/`CardText`, `Ground`/
`GroundText` — because a high contrast scheme can render a mismatched pair as one colour on itself.
`GeneratorForm.ApplyTheme` colours everything the designer laid out and is called again from
`OnSystemColorsChanged`, so anything added to the window belongs there rather than in the designer. The
only fixed colours are `Trace` and `Average`, which are data rather than chrome: they tell two series
apart, so they have to differ from each other rather than agree with the window. Severity has no system
colour to take, so under a high contrast scheme `StatusPalette` stands aside and the wording carries it.

Errors raised while analysing are visible on the tab that raised them. `SetStatusBoxState` takes its
colours from `StatusPalette`, which
is where the severity scheme lives for both the form and `DeviceUpdateThread`. Messages start at the first
word: the leading space that used to pad them was for a text box with no padding of its own.

Statistics are shown in borderless read-only `TextBox` readouts rather than sunken fields — read-only so
they read as output, but still text boxes so a value can be selected and copied. A measure that nothing has
been measured for shows `m_sNO_VALUE` rather than a zero. Number formats are named per kind:
`m_sVALUE_FORMAT` for anything in a unit range, `m_sMOMENT_FORMAT` for the unbounded moments, and the two
`…DIFFERENCE_FORMAT` variants, which carry an explicit sign.

Device discovery is separate: `DeviceUpdateThread` (a static class queued on the thread pool) enumerates
`Win32_USBControllerDevice`/`Win32_PnPEntity` via WMI, regex-matches `USB…Serial…COM<n>`, and pushes a
`BindingList<IRNGDevice>` back through `IGeneratorForm`. It is re-triggered from `WndProc` on
`WM_DEVICECHANGE`, and checks `Terminating` between items so shutdown is not blocked.

The analysis UI (baseline vs. result comparison) uses `StatisticalAnalysis` (MathNet.Numerics
`DescriptiveStatistics`) and `HistogramChart`; both charts derive from
`System.Windows.Forms.DataVisualization.Charting.Chart`.

The analysis tab exists to answer one question: **did the result session shift by more than noise?**
`SignificanceTest` answers it — `CompareMeans` is Welch's t-test between the two sessions (unequal sizes and
spreads, because a baseline is usually recorded for far longer than the run compared against it), and
`CompareWithExpected` tests one session against the 0.5 an unbiased generator gives. Both are two-tailed: a
one-tailed test would find a shift toward a target more easily, but only holds when the direction was
predicted before the readings were taken, which the application cannot know. Every path that cannot produce
a number - fewer than two readings, or readings with no spread - returns `Valid = false` rather than a
probability, and `Significant` is false whenever the test did not run.

A verdict of nothing found means nothing at all unless a shift worth finding could have been seen, so the
analysis states what it could have seen. `SignificanceResult.DetectableDifference` is the half width of the
confidence interval for the pair, which is the test read the other way round: instead of asking whether the
shift that happened beats the noise, it asks how large a shift would have to be before it could. It comes
back with the test, worked out in `BuildResult` from the same critical value the probability comes from, so
the readings are walked once and the limit cannot disagree with the test it is quoted beside. Below
`SHIFT_OF_INTEREST` - one part in ten thousand, the order of the effect reported in the published work - the
sessions cannot speak to the question, and the verdict says so in the warning colour rather than reporting a
null result that reads as evidence of absence.

The three verdicts are different weights, which is what `VerdictWeights` carries. A shift found is notable
whatever the sensitivity, because it cleared the limit by being found at all. Nothing found from sessions
that could have found something is the ordinary outcome. Nothing found from sessions that could not is the
one that misleads, and is the only one that warns.

Note that the device and the simulator have different noise floors: a device reading averages 262,144 bits
and a simulated one 16,384, so the simulated spread is about four times wider and needs about sixteen times
the readings to pin its mean down as finely. That is why the limit is worked out from each session's own
spread rather than from a count of readings.

The verdict is stated in words under the table rather than left to be read off the numbers, and is
emphasised only when there is a shift to notice. `HistogramChart` plots each session as a percentage of its
own readings, not as counts: on counts the longer session stands taller in every bin and hides the shift the
comparison exists to show.

The comparison is a single `ListView` — measure, baseline, result, difference — rather than the two
mirrored sets of fields it used to be. `BuildComparisonTable` makes the rows once and `UpdateComparisonTable`
rewrites their values; the row order there and the `m_iSKEWNESS_ROW` constant that tells the bounded
measures from the unbounded ones have to be kept in step. The list does not come back through UI Automation,
so anything driving it from outside has to read it with `LVM_GETITEMTEXT`.

`HistogramChart.Plot` chooses its own bin count from the sample size unless one is passed; a fixed hundred
bins turned a short session into a row of one-pixel spikes rather than a distribution. Axis labels are
printed to the precision the tick interval needs, because the default prints the full double and the labels
collide.

### Threading rules
- `RNGDeviceTimer` uses `System.Windows.Forms.Timer` → its tick is already on the UI thread.
- `RNGSessionTimer` uses `System.Timers.Timer` (deliberately, for reliable ticks) → it fires on a pool
  thread, so anything touching controls goes through `InvokeRequired`/`Invoke`.
- Background work (device enumeration, file loading) marshals UI updates the same way; the `On*Completed`
  callbacks are the established pattern.
- `RNGSessionTimer.Start` times each session from nothing; `Stop` deliberately leaves the time it reached on
  display, so how long a session ran can still be read once it has ended. Resuming a paused session goes
  through `Enabled` rather than `Start`, so a pause does not reset the clock.

### Native layer
`TruRNGproMain.cpp` exports `Initialize(int iPort, bool bSimulate)` and `GetRandomBitAverage(double&)`.
`Initialize` picks `TruRNGpro` (real device, via the third-party `rng.h`) or `RNGSimulator` (seeded Mersenne
twister — the seed comes from the "Seed" box that replaces "Port" in simulate mode) behind the
`RNGInterface` base class. Note the two `DllImport`s in `RNGDeviceTimer.cs` declare different calling
conventions (`Winapi` vs `Cdecl`).

Both exports return a C++ `bool`, which is one byte, so both `DllImport`s marshal the return — and the
`bSimulate` argument — as `UnmanagedType.I1`. Left to itself the marshaller expects the four byte Windows
`BOOL` and reads three bytes of whatever the call left behind along with the answer, which made a native
false come back as true: initializing against a port with nothing on it reported success, and the failure
only surfaced as a read error a moment later.

`TruRNGpro::GetBitAverage` reopens the device and reads once more before reporting failure. A failed read
leaves the third-party interface flagged `bad` and it never clears, so one momentary fault on the USB port
ended the session and every read after it failed too. The device itself is fine afterwards — opening it
again and carrying on works — so that is what happens, and only a device that will not open again is
reported as a failure, which is what an unplugged one does. Every reading is still an average of the same
number of bits whether it took one attempt or two.

`Externals/CommonControls.dll` and `Externals/DeviceInterfaces.dll` are checked-in binaries with no source
here (`DeviceInterfaces` supplies the `USBDeviceNotification` constants used in `WndProc`).

## Coding conventions

`CODING_GUIDELINES.md` (app), `tests/RandomNumberGenerator.Test/CODING_GUIDELINES_TESTS.md` (tests), and
`TruRNGpro/CODING_GUIDELINES_CPP.md` (native) are authoritative and are actually followed throughout. The
non-obvious rules new code is expected to match:

- Standardized file header block on every file: name, description, copyright, the MIT licence pointer, and
  revision history. The templates in the three guidelines documents are the ones to copy. The name in the
  header has to match the file it is in; two headers naming the file they were copied from have already had
  to be corrected.
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
- Errors bubble up: throw with a user-facing message rather than swallowing and returning `false`; the form
  catches them and renders via `SetStatusBoxError`. Messages start at the first word — the leading space
  that used to be the house style was padding for a status box that had none, and the status bar that
  replaced it has real padding. A failure that puts the form back to idle must report itself **after**
  `SetIdleState`, not before: that method ends by resetting the status bar, so a message set on the way in
  is wiped. Reporting it too early is why clicking Start with no data file used to do nothing whatsoever.
- Tests: `[ClassName]Tests`, `[Method]_[Scenario]_[ExpectedResult]`, banner-commented Arrange/Act/Assert
  sections, Moq for collaborators, `[TestCategory("Component")]`.

## Licence

MIT, in `LICENSE`. Every source file carries the copyright line and a pointer to it. `TruRNGpro/rng.h` is
third-party and is not covered by it; leave its header alone.

## Releasing

`.github/workflows/release.yml`, started by hand from the Actions tab and from `master` only. It refuses to
run unless `RELEASE` is typed into the confirmation box, the version reads like `1.2.3` and has not been
released before; it stops if the tests fail or the package is missing the application or the native
library. What it produces is a **draft** release, so nothing is tagged or published without someone
pressing publish.

The unit tests do not build the form's event wiring, so a fault in it passes them: the crash that made
loading a session file impossible was only found by driving the built application. Worth doing for changes
that touch `GeneratorForm`. The interface rework turned up four more the same way — start-up values still
in a format that had been replaced, a button row clipped by a band an inch too short, comparison columns
that did not fill their table, and a Pause button that was clickable before any session existed. All four
passed a clean unit run.

There are now unit tests that build a `GeneratorForm` over mocked collaborators and drive `SetRunningState`
and `SetIdleState` through reflection, which covers the session lifecycle the earlier suite never reached.
They still do not run a message loop, so the designer's event wiring is exercised only by running the
application.

The device path cannot be reached by the simulator at all, and three faults were found there that a full
green run did not show: the bool marshalling, the read that never recovered, and readings from a previous
file left in the statistics. Changes touching `RNGDeviceTimer`, `TruRNGpro.h` or the P/Invoke boundary are
worth running against real hardware — with the device attached, with it absent, and with it pulled part way
through a session.
