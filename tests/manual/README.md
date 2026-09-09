# Manual test suites

These drive the built application rather than its classes. The unit tests in `tests/RandomNumberGenerator.Test`
cover the layers underneath the window; nothing in them builds the form's event wiring, and nothing in them
can reach the device at all. Everything here exists because a fault got past a clean unit run.

They are run by hand, not by the release workflow: several need the TruRNGpro attached, one needs someone to
pull the cable, and the rest drive a real window and take a minute or two each.

## Running them

Build the solution first — these drive `bin\Release\Random Number Generator.exe` unless told otherwise.

```powershell
# Everything that can run against whatever is attached right now
.\tests\manual\run-all.ps1

# Against the Debug build instead
.\tests\manual\run-all.ps1 -Configuration Debug

# One suite on its own
.\tests\manual\check-chart.ps1

# A different build again
$env:RNG_BIN = "D:\somewhere\else\bin\Release"; .\tests\manual\check-device.ps1
```

Each suite prints one `PASS`/`FAIL` line per check and a count at the end. Session files and screenshots go
to `tests\manual\work`, which is not in source control.

`test-analyze.ps1` reads the two session files `test-full.ps1` records, so run that one first — `run-all.ps1`
already does.

## What each one covers

| Suite | Needs the device | Covers |
| --- | --- | --- |
| `test-full.ps1` | no | The Record tab end to end: the initial state, simulate mode, recording, pause and resume, stopping, clearing, reopening a file and appending to it, resizing, closing, and the window geometry being remembered. Also the no-device failure path, which it skips when a device answers. |
| `test-analyze.ps1` | no | The Analyse tab: the comparison table, the significance verdict and its wording, the histogram, and recovering a session file left unterminated by killing the application mid-write. |
| `check-chart.ps1` | no | That the chart and the statistics beside it agree on every path — recording, a second session into the same file, a different file being chosen, an existing file being reopened, and Clear. |
| `inproc-length.ps1` | no | The session length, and the data file surviving a session ending. Drives the form directly, so it covers the state machine rather than the pixels. |
| `check-appended.ps1` | no | That a file grown by a second session reads back correctly on the Record tab and analyses correctly on the Analyse tab. |
| `check-device.ps1` | **yes** | Device discovery, the native reads, a session recorded from the hardware, a second session appended to it, a timed session, and the recorded file analysing. Also that a port with nothing on it is refused. |
| `check-reinit.ps1` | **yes** | Switching between the device and the simulator and back, which tears down the native interface and builds a new one each time. |
| `check-nodevice.ps1` | **no device** | That the device path fails rather than appearing to work when nothing is attached, that a failed read stays cheap, and that the simulator still works. Run this one with the device unplugged. |
| `soak-device.ps1` | **yes** | Records from the device for a given number of minutes, watching for read failures. `.\soak-device.ps1 5` for five minutes. |
| `check-unplug.ps1` | **yes, then pulled** | The device going away while a session is recording. Prompts for the cable to be pulled and then put back, and reports what the window did. |
| `shot.ps1` | no | Saves screenshots of the window in each of its states, for looking at rather than asserting on. |

The suites that need hardware say `SKIPPED` and stop when it is not there, rather than reporting the absent
device as a fault.

## Why the device ones matter

The simulator cannot reach the device path, and three faults were found there that a full green unit run did
not show:

- the native functions return a one byte C++ `bool` while the P/Invoke assumed the four byte Windows `BOOL`,
  so a native false came back as true and a device that failed to initialize reported success;
- a failed read left the third-party interface flagged `bad` and it never cleared, so one momentary fault on
  the USB port ended the session and every read after it failed too;
- readings from a previously recorded file were left in the statistics when a new file was chosen.

Changes touching `RNGDeviceTimer`, `TruRNGpro.h` or the P/Invoke boundary are worth running against real
hardware — attached, absent, and pulled part way through a session.

## The two helper files

`win32b.ps1` and `uia.ps1` are dot-sourced by the suites that drive a real window. The application's own
text boxes do not answer text messages sent from another process, so fields are read through UI Automation
where that works and over Win32 where it does not — the comparison table is a `ListView` that UI Automation
does not return at all, and is read with `LVM_GETITEMTEXT` into memory allocated in the other process.

The suites that do not drive a window instead load the executable as an assembly and build the form in
process, reaching its private members through reflection. That covers the state machine and the wiring
between the form and the session data, but it still does not run a message loop, so it does not prove the
designer's event wiring.
