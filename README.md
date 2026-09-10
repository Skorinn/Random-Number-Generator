# Random Number Generator

A Windows desktop application for recording and analysing the output of a
[TruRNGpro](https://ubld.it/truerngpro) hardware random number generator.

The application samples the device, records every reading to a session file, charts the results as they
arrive, and compares recorded sessions against one another. A built-in simulator stands in for the device,
so the application can be run and developed without one attached.

## What it does

Each sample reads a block of bits from the device and records the **average of those bits**, which for an
unbiased generator sits around 0.5. Readings are taken ten times a second. The application shows, live:

- the elapsed session time, the number of readings, the running average, how far that average sits from
  0.5, and the standard deviation
- a chart of the individual readings against the running average, marked with the 0.5 an unbiased
  generator is expected to give, and with the axis scaling itself to the data

The window resizes, and the chart takes whatever space is left over. It reopens at the size and on the
screen it was last closed at.

Every reading is written to the session file as it is taken, so a session survives the application being
closed or stopped unexpectedly.

### Session length

A session can be given a length in minutes, and stops itself once it has recorded for that long. The
default of zero records until *Stop* is pressed. Time spent paused does not count towards it.

### Analysis

A recorded session can be loaded as a **baseline** and another as a **result**, and the two are compared
directly: mean, standard deviation, skewness and kurtosis for each, the difference between their means, and
a histogram of the two distributions overlaid. Statistics are calculated with
[Math.NET Numerics](https://numerics.mathdotnet.com/).

### Enough readings to answer

A session that found nothing has only said something if it could have found something. The verdict therefore
states the smallest difference the two sessions could show as significant, and warns when that is coarser
than the shift being looked for — one part in ten thousand, a generator running at 0.5001 rather than 0.5,
which is the order of the effect reported in the published work.

A minute or so of recording from the device is enough to speak to a shift that size. Below that, a real
shift can sit in the readings and never reach significance, and a verdict of *no significant shift* would
be reporting the length of the session rather than anything about the generator. The limit is worked out
from each session's own spread rather than from a count of readings, because a simulated reading averages
far fewer bits than a device one and is correspondingly noisier.

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

Start the application. It opens on the **Record** tab, and the status bar at the foot of the window says
what to do next at each step.

1. **Choose a file.** *Browse* selects the session file. An existing file is loaded and displayed, and
   the session continues in it. A new file is created. The file stays selected when a session ends, so
   pressing *Start* again records another session into the same file, after the readings already in it.
2. **Choose a source.** The toggle switches between the device and the simulator. With the device, pick the
   COM port it is on; the port list updates by itself as devices are connected and removed. With the
   simulator, the port becomes a seed.
3. **Choose a target**, if the session has one.
4. **Set a length**, if the session should stop on its own. *Stop after (min)* is how long to record
   for; zero records until *Stop* is pressed.
5. **Start.** *Pause* suspends recording without ending the session; *Stop* ends it. Whichever button
   carries the action to take next is the emphasised one. *Clear* discards the readings on screen and asks
   before it does.

To compare previous sessions, go to the **Analyse** tab and browse for a baseline file and a result file.
The two are set against each other in one table, measure by measure, with the difference between them, and
the question the tab exists to answer is stated underneath in words: whether the result shifted away from
the baseline by more than the noise in the two of them accounts for.

That verdict is [Welch's t-test](https://en.wikipedia.org/wiki/Welch%27s_t-test) between the two sessions,
which does not assume they are the same length or equally spread — a baseline is usually recorded for far
longer than the run being compared against it. It is two-tailed, so a shift in either direction counts; a
one-tailed test would find a shift toward a target more easily, but only holds when the direction was
predicted before the readings were taken. A shift is reported as significant when it would arise by chance
less than one time in twenty. Each session is also tested on its own against the 0.5 an unbiased generator
would give.

The histogram plots each session as a percentage of its own readings rather than as a count of them, so a
long session and a short one can be compared on the same chart.

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
| `tests/RandomNumberGenerator.Test/` | Unit tests. MSTest and Moq |
| `tests/manual/` | Suites that drive the built application, some needing the device attached |

The application talks to the native DLL through two exported functions, `Initialize` and
`GetRandomBitAverage`, which sit behind an interface implemented by both the real device and the simulator.

## Tests

```
vstest.console.exe "tests\RandomNumberGenerator.Test\bin\Debug\Random Number Generator.Test.dll"
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
- `tests/RandomNumberGenerator.Test/CODING_GUIDELINES_TESTS.md` for the tests
- `TruRNGpro/CODING_GUIDELINES_CPP.md` for the native code

`CLAUDE.md` describes the architecture and the build for anyone, or anything, new to the codebase.

## Third-party code

- [Math.NET Numerics](https://numerics.mathdotnet.com/) (MIT), for the statistics
- [Moq](https://github.com/devlooped/moq) and MSTest, for the tests
- `Externals/CommonControls.dll` and `Externals/DeviceInterfaces.dll` (both MIT), built from
  [CommonControls](https://github.com/Skorinn/CommonControls) and
  [DeviceInterfaces](https://github.com/Skorinn/DeviceInterfaces). They are committed here as binaries
  rather than built alongside this solution.
- `TruRNGpro/rng.h`, a third-party header that wraps the serial port setup and reads for the device. It is
  not covered by this project's licence. Its author states in the file that it carries "No copyright, no
  warranties", which is a disclaimer rather than a formal grant such as CC0, so no particular legal status
  is claimed for it here. That is worth settling with the author if the licensing of the whole ever has to
  be stated precisely.

## Licence

[MIT](LICENSE). Copyright (c) 2022-2026 Mike Pullen.

You may use, modify and redistribute this, including commercially, provided the copyright notice and the
permission notice are kept. It comes with no warranty.
