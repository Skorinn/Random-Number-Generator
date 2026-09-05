# Random Number Generator

A Windows desktop application for recording and analysing the output of a
[TruRNGpro](https://ubld.it/truerngpro) hardware random number generator.

The application samples the device, records every reading to a session file, charts the results as they
arrive, and compares recorded sessions against one another. A built-in simulator stands in for the device,
so the application can be run and developed without one attached.

## What it does

Each sample reads a block of bits from the device and records the **average of those bits**, which for an
unbiased generator sits around 0.5. Readings are taken ten times a second. The application shows, live:

- the running average, the number of data points, the deviation from the statistical mean, and the standard
  deviation
- a chart of the individual readings against the running average, with the axis scaling itself to the data
- the elapsed session time

Every reading is written to the session file as it is taken, so a session survives the application being
closed or stopped unexpectedly.

### Analysis

A recorded session can be loaded as a **baseline** and another as a **result**, and the two are compared
directly: mean, standard deviation, skewness and kurtosis for each, the difference between their means, and
a histogram of the two distributions overlaid. Statistics are calculated with
[Math.NET Numerics](https://numerics.mathdotnet.com/).

### Targets

A session can record a target value of `0` or `1` — the outcome the operator is attempting to influence —
or `None`. The target is stored in the session file alongside the data.

## Requirements

- Windows
- Visual Studio 2022 with the **.NET desktop** and **Desktop development with C++** workloads
- .NET Framework 4.8
- A TruRNGpro on a USB serial port — optional, as the simulator can be used instead

## Building

Build the solution rather than the individual projects: the C# application depends on the native DLL, and
the post-build steps that place the DLL where the application and the tests can find it rely on the
solution directory.

```
msbuild RandomNumberGenerator.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

MSBuild is not usually on the path; it is under the Visual Studio installation, for example
`C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe`.

NuGet packages are not committed, so restore them on a fresh clone. The projects use `packages.config`, so
`dotnet restore` will not work:

```
nuget restore RandomNumberGenerator.sln
```

The application is built to `bin\Debug\Random Number Generator.exe` and copied to `bin\`.

## Running

Start the application, then:

1. **Choose a source.** Leave *Simulate* unchecked and pick the COM port the device is on, or check it and
   enter a seed for the simulator. The port list updates by itself as devices are connected and removed.
2. **Choose a file.** *Browse* selects the session file. An existing file is loaded and displayed, and the
   session continues in it. A new file is created.
3. **Choose a target**, if the session has one.
4. **Start.** *Pause* suspends recording without ending the session; *Stop* ends it.

To analyse previous sessions, browse for a baseline file and a result file in the comparison section.

### Recovering a session

A file left unfinished — by the application being stopped while recording, or by the machine losing power —
is recovered when it is next opened. The readings it holds are loaded, the file is reported as having been
recovered, and recording continues in it. Only complete readings are recovered; a reading that was still
being written when the application stopped is discarded rather than being loaded as a smaller number.

## Session files

Sessions are XML, written as the data is recorded:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Session Simulated="false" Target="-1">
	<Data Time="00:00:00">0.499969482421875</Data>
	<Data Time="00:00:00">0.500091552734375</Data>
</Session>
```

`Simulated` records whether the readings came from the simulator. `Target` is `0`, `1`, or `-1` for a
session with no target. `Time` is the elapsed session time when the reading was taken. Values are written
in the invariant culture, so a file written on one machine reads back as the same numbers on another.

The file is shared for reading while a session is in progress, so it can be inspected or backed up without
stopping the recording.

## Layout

| Project | |
|---|---|
| `RandomNumberGenerator.csproj` | The application. Windows Forms, .NET Framework 4.8, AnyCPU |
| `TruRNGpro/TruRNGpro.vcxproj` | Native DLL holding the device interface and the simulator. x64 |
| `RandomNumberGenerator.Test/` | Unit tests. MSTest and Moq |

The application talks to the native DLL through two exported functions, `Initialize` and
`GetRandomBitAverage`, which sit behind an interface implemented by both the real device and the simulator.

## Tests

```
vstest.console.exe "RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll"
```

`vstest.console.exe` is under the Visual Studio installation, in
`Common7\IDE\CommonExtensions\Microsoft\TestWindow`. A single test or class can be selected:

```
vstest.console.exe "...\Random Number Generator.Test.dll" /Tests:WriteDataPoint_ValidWriter_Success
vstest.console.exe "...\Random Number Generator.Test.dll" /TestCaseFilter:"FullyQualifiedName~RNGXMLWriterTests"
```

Build the whole solution before running them, as the tests need the native DLL that the solution build puts
in place.

## Contributing

The coding standards are written down and are followed throughout:

- `CODING_GUIDELINES.md` for the application
- `RandomNumberGenerator.Test/CODING_GUIDELINES_TESTS.md` for the tests
- `TruRNGpro/CODING_GUIDELINES_CPP.md` for the native code

`CLAUDE.md` describes the architecture and the build for anyone, or anything, new to the codebase.

## Third-party code

- [Math.NET Numerics](https://numerics.mathdotnet.com/) (MIT), for the statistics
- [Moq](https://github.com/devlooped/moq) and MSTest, for the tests
- `Externals/CommonControls.dll` and `Externals/DeviceInterfaces.dll`, built from
  [CommonControls](https://github.com/Skorinn/CommonControls) and
  [DeviceInterfaces](https://github.com/Skorinn/DeviceInterfaces). They are committed here as binaries
  rather than built alongside this solution, and the terms they are offered under are stated in their own
  repositories.
- `TruRNGpro/rng.h`, a third-party header that wraps the serial port setup and reads for the device. It is
  not covered by this project's licence. Its author states in the file that it carries "No copyright, no
  warranties", which is a disclaimer rather than a formal grant such as CC0, so no particular legal status
  is claimed for it here. That is worth settling with the author if the licensing of the whole ever has to
  be stated precisely.

## Licence

[MIT](LICENSE). Copyright (c) 2022-2026 Mike Pullen.

You may use, modify and redistribute this, including commercially, provided the copyright notice and the
permission notice are kept. It comes with no warranty.
