#########################################################################################################################
# File Name:      check-nodevice.ps1
# Description:    Checks the device path fails rather than appearing to work when nothing is attached
#
# Copyright (c) 2026 Mike Pullen
# Licensed under the MIT License. See LICENSE in the repository root.
#
# Revision History:
#======================================================================================================================
# 2026/09/08 - Mike Pullen - Original implementation.
#########################################################################################################################
# The files a run produces are kept out of the way of the scripts, and out of source control.
$WorkRoot = Join-Path $PSScriptRoot "work"
if (-not (Test-Path $WorkRoot)) { $null = New-Item -ItemType Directory -Path $WorkRoot }

# This suite is about what happens with nothing attached, so a device being present means there is
# nothing for it to test rather than something failing.
if (0 -ne ([System.IO.Ports.SerialPort]::GetPortNames()).Count) {
    Write-Host "SKIPPED: a device is attached, so the no-device path cannot be exercised. Unplug it and run again."
    exit 0
}

# Which build to drive. Set RNG_BIN to test a different one.
if ([string]::IsNullOrEmpty($env:RNG_BIN)) { $env:RNG_BIN = (Resolve-Path (Join-Path $PSScriptRoot "..\..\bin\Release")).Path }

# With nothing attached, the device path has to fail rather than appear to work, and it has to fail
# promptly: a read now tries to open the device again before giving up, and that retry must not turn a
# missing device into a hang.
$ErrorActionPreference = "Continue"
Add-Type -AssemblyName System.Windows.Forms
$binDir = $env:RNG_BIN
[System.Reflection.Assembly]::LoadFrom((Join-Path $binDir "Random Number Generator.exe")) | Out-Null
[Environment]::CurrentDirectory = $binDir

$script:pass = 0; $script:fail = 0
function Check($name, $condition, $detail) {
    if ($condition) { $script:pass++; Write-Host ("  PASS  " + $name + "   " + $detail) }
    else            { $script:fail++; Write-Host ("  FAIL  " + $name + "   " + $detail) }
}

Write-Host "=== 1. NOTHING IS ATTACHED ==="
$ports = [System.IO.Ports.SerialPort]::GetPortNames()
Check "no serial port present" ($ports.Count -eq 0) ("ports=" + $(if ($ports.Count -gt 0) { $ports -join ", " } else { "none" }))

Write-Host ""
Write-Host "=== 2. INITIALIZING AGAINST A DEVICE THAT IS NOT THERE ==="
$readMethod = [RandomNumberGenerator.RNGDeviceTimer].GetMethod("GetRandomBitAverage",
                  [System.Reflection.BindingFlags]"NonPublic,Static")
# The port the device used to be on, and the value the form passes when it has no port to offer
foreach ($port in 4, 0) {
    $t = New-Object RandomNumberGenerator.RNGDeviceTimer
    $init = $t.InitializeDevice($port, $false)
    Check ("port " + $port + " is refused")        ($init -eq $false)      ("InitializeDevice(" + $port + ", simulate=false) returned " + $init)
    Check ("port " + $port + " is not made ready") (-not $t.Initialized)   ""
}

Write-Host ""
Write-Host "=== 3. A READ WITH NO DEVICE FAILS, AND FAILS QUICKLY ==="
# The read reopens the device once before giving up, so a missing device costs one failed open per read.
# That has to stay cheap: the timer asks for a reading every 100ms on the thread that draws the window.
$t = New-Object RandomNumberGenerator.RNGDeviceTimer
$null = $t.InitializeDevice(4, $false)
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$failed = 0
$READS = 20
for ($i = 0; $i -lt $READS; $i++) {
    $a = [object[]]@([double]0.0)
    $ok = $readMethod.Invoke($null, $a)
    if (-not $ok) { $failed++ }
}
$sw.Stop()
$perRead = [math]::Round($sw.Elapsed.TotalMilliseconds / $READS, 2)
Check "every read reports failure" ($failed -eq $READS) ("failed=" + $failed + " of " + $READS)
Check "a failed read stays cheap"  ($perRead -lt 100)   ("$perRead ms per read, against the 100ms the timer allows")

Write-Host ""
Write-Host "=== 4. THE WINDOW WITH NO DEVICE ==="
$ws = New-Object System.Xml.XmlWriterSettings
$ws.Indent = $true; $ws.IndentChars = "`t"
$writer  = New-Object RandomNumberGenerator.RNGXMLWriter($ws)
$reader  = New-Object RandomNumberGenerator.RNGXMLReader
$dataFile= New-Object RandomNumberGenerator.RNGSessionDataFile($writer, $reader)
$timer   = New-Object RandomNumberGenerator.RNGSessionTimer
$data    = New-Object RandomNumberGenerator.RNGSessionData($dataFile, $timer)
$device  = New-Object RandomNumberGenerator.RNGDeviceTimer
$form    = New-Object RandomNumberGenerator.GeneratorForm($data, $device)
$BF = [System.Reflection.BindingFlags]"NonPublic,Instance"
function Fld($name)  { return $form.GetType().GetField($name, $BF).GetValue($form) }
function Call($name) { return $form.GetType().GetMethod($name, $BF).Invoke($form, @()) }
function Pump($s) { $d = (Get-Date).AddSeconds($s); while ((Get-Date) -lt $d) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 } }

$form.Show()
Pump 8
$portCombo = Fld "m_PortComboBox"
Check "no device is listed" ($portCombo.Items.Count -eq 0) ("listed=" + $portCombo.Items.Count)

$file = Join-Path $WorkRoot "nodevice-session.rng"
if (Test-Path $file) { Remove-Item $file -Force }
(Fld "m_SimulateToggle").Checked = $false
$data.FilePath = $file
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call "UpdateStartAvailability"
[System.Windows.Forms.Application]::DoEvents()
Check "Start is offered, since a file was chosen" ((Fld "m_StartButton").Enabled) ""

$sw = [System.Diagnostics.Stopwatch]::StartNew()
Call "SetRunningState"
Pump 3
$sw.Stop()
$state  = (Fld "m_State").ToString()
$status = (Fld "m_StatusLabel").Text
Check "the session did not start"     ("Idle" -eq $state)              ("state=" + $state)
Check "the failure is reported"       ($status -match "Error")         ("status='" + $status + "'")
Check "it names the device"           ($status -match "TruRNGpro")     ""
Check "the data file is kept"         ((Fld "m_FileTextBox").Text -eq [System.IO.Path]::GetFileName($file)) ("'" + (Fld "m_FileTextBox").Text + "'")
Check "Start is offered to retry"     ((Fld "m_StartButton").Enabled)  ""
Check "Stop is not offered"           (-not (Fld "m_StopButton").Enabled) ""
Check "the length is open again"      ((Fld "m_SessionLengthUpDown").Enabled) ""
Check "nothing was recorded"          ($data.NumDataPoints -eq 0)      ("readings=" + $data.NumDataPoints)
Check "no file was written"           (-not (Test-Path $file))         ("exists=" + (Test-Path $file))

Write-Host ""
Write-Host "=== 5. THE SIMULATOR STILL WORKS WITH NOTHING ATTACHED ==="
$simFile = Join-Path $WorkRoot "nodevice-sim.rng"
if (Test-Path $simFile) { Remove-Item $simFile -Force }
(Fld "m_SimulateToggle").Checked = $true
$data.FilePath = $simFile
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($simFile)
Call "UpdateStartAvailability"
[System.Windows.Forms.Application]::DoEvents()
Call "SetRunningState"
Pump 4
Check "the simulated session runs" ("Running" -eq (Fld "m_State").ToString()) ("status='" + (Fld "m_StatusLabel").Text + "'")
Check "it records readings"        ($data.NumDataPoints -gt 0)              ("readings=" + $data.NumDataPoints)
Call "SetIdleState"
Pump 1
Check "and reaches the file"       ((Test-Path $simFile) -and (([regex]::Matches((Get-Content $simFile -Raw), "</Data>")).Count -gt 0)) ("on disk=" + $(if (Test-Path $simFile) { ([regex]::Matches((Get-Content $simFile -Raw), "</Data>")).Count } else { 0 }))

Write-Host ""
Write-Host "=== 6. AND BACK TO THE DEVICE, WHICH IS STILL NOT THERE ==="
(Fld "m_SimulateToggle").Checked = $false
$data.FilePath = $file
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call "UpdateStartAvailability"
[System.Windows.Forms.Application]::DoEvents()
Call "SetRunningState"
Pump 3
Check "still refuses to start"  ("Idle" -eq (Fld "m_State").ToString())      ("state=" + (Fld "m_State").ToString())
Check "and still says why"      ((Fld "m_StatusLabel").Text -match "Error")  ("status='" + (Fld "m_StatusLabel").Text + "'")

$form.Close()
[System.Windows.Forms.Application]::DoEvents()
Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
