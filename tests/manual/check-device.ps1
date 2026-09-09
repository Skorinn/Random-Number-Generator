#########################################################################################################################
# File Name:      check-device.ps1
# Description:    Exercises the real device: discovery, the native reads, and sessions recorded from it
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

# Which build to drive. Set RNG_BIN to test a different one.
if ([string]::IsNullOrEmpty($env:RNG_BIN)) { $env:RNG_BIN = (Resolve-Path (Join-Path $PSScriptRoot "..\..\bin\Release")).Path }

# Exercises the real TruRNGpro rather than the simulator: the native P/Invoke layer, device discovery,
# and a session recorded from the hardware. Everything the suite has run so far went through the
# simulator, so the device path itself has never been under test.
$ErrorActionPreference = "Continue"
Add-Type -AssemblyName System.Windows.Forms
$binDir = $env:RNG_BIN
[System.Reflection.Assembly]::LoadFrom((Join-Path $binDir "Random Number Generator.exe")) | Out-Null
[Environment]::CurrentDirectory = $binDir

# This suite needs the hardware. Without it there is nothing to test rather than something failing, so it
# says so and stops instead of reporting the absent device as a fault.
if (0 -eq ([System.IO.Ports.SerialPort]::GetPortNames()).Count) {
    Write-Host "SKIPPED: no serial device attached, so there is nothing for this suite to exercise."
    exit 0
}

$script:pass = 0; $script:fail = 0
function Check($name, $condition, $detail) {
    if ($condition) { $script:pass++; Write-Host ("  PASS  " + $name + "   " + $detail) }
    else            { $script:fail++; Write-Host ("  FAIL  " + $name + "   " + $detail) }
}

$file = Join-Path $WorkRoot "device-session.rng"
if (Test-Path $file) { Remove-Item $file -Force }

Write-Host "=== 1. THE DEVICE IS THERE ==="
$ports = [System.IO.Ports.SerialPort]::GetPortNames()
Check "a serial port is attached" ($ports.Count -gt 0) ("ports=" + ($ports -join ", "))
$expectedPort = 0
if ($ports.Count -gt 0) { $expectedPort = [int]($ports[0] -replace '\D','') }
Write-Host ("  device expected on COM" + $expectedPort)

Write-Host ""
Write-Host "=== 2. DEVICE DISCOVERY FINDS IT ==="
# The form is what the discovery thread reports back through, so a real one is built for it to fill in
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
Pump 8   # the discovery thread goes out to WMI, which takes as long as it takes

$portCombo = Fld "m_PortComboBox"
$listed = @()
foreach ($item in $portCombo.Items) { $listed += ($item.Description + " [port " + $item.Port + "]") }
Check "the device is listed"        ($portCombo.Items.Count -gt 0) ("listed: " + ($listed -join " | "))
Check "listed on the port it is on" (($listed -join " ") -match ("port " + $expectedPort + "\]")) ("looking for port " + $expectedPort)

Write-Host ""
Write-Host "=== 3. THE NATIVE LAYER READS FROM IT ==="
# Straight at the P/Invoke, with no session around it. The read is an internal static on the device
# timer, so it is reached the same way a private member is: by name, through the type description.
$readMethod = [RandomNumberGenerator.RNGDeviceTimer].GetMethod("GetRandomBitAverage",
                  [System.Reflection.BindingFlags]"NonPublic,Static")
Check "the native read is there" ($null -ne $readMethod) "GetRandomBitAverage"

$deviceTimer = New-Object RandomNumberGenerator.RNGDeviceTimer
$initialized = $deviceTimer.InitializeDevice($expectedPort, $false)
Check "device initialized"      ($initialized -eq $true)   ("InitializeDevice(" + $expectedPort + ", simulate=false) returned " + $initialized)
Check "reports itself as ready" ($deviceTimer.Initialized) ""

if ($initialized -and $readMethod) {
    $readings = @()
    $failed = 0
    for ($i = 0; $i -lt 40; $i++) {
        $readArgs = [object[]]@([double]0.0)
        $ok = $readMethod.Invoke($null, $readArgs)
        $value = $readArgs[0]
        if ($ok -and ($value -ne [double]::MaxValue)) { $readings += $value } else { $failed++ }
        Start-Sleep -Milliseconds 100
    }
    Check "every read succeeded" ($failed -eq 0)         ("failed=" + $failed + " of 40")
    Check "readings came back"   ($readings.Count -gt 0) ("readings=" + $readings.Count)
    if ($readings.Count -gt 0) {
        $min = ($readings | Measure-Object -Minimum).Minimum
        $max = ($readings | Measure-Object -Maximum).Maximum
        $avg = ($readings | Measure-Object -Average).Average
        $distinct = ($readings | Select-Object -Unique).Count
        Check "readings are bit averages"  (($min -ge 0.0) -and ($max -le 1.0)) ("min=" + $min + " max=" + $max)
        Check "readings sit around a half" (([math]::Abs($avg - 0.5)) -lt 0.05) ("mean=" + $avg)
        Check "readings actually vary"     ($distinct -gt 1)                    ("distinct=" + $distinct + " of " + $readings.Count)
    }
}

Write-Host ""
Write-Host "=== 3b. A PORT WITH NOTHING ON IT ==="
# The no-device failure cannot be reached through the window while a device is attached, because the
# port list only offers ports it found something on. The layer below it can still be asked for one.
$NO_SUCH_PORT = 99
$badTimer = New-Object RandomNumberGenerator.RNGDeviceTimer
$badInit = $badTimer.InitializeDevice($NO_SUCH_PORT, $false)
Check "a port with nothing on it is refused" ($badInit -eq $false) ("InitializeDevice(" + $NO_SUCH_PORT + ", simulate=false) returned " + $badInit)
Check "and is not reported as ready"        (-not $badTimer.Initialized) ""

Write-Host ""
Write-Host "=== 4. A SESSION RECORDED FROM THE DEVICE ==="
$portCombo.SelectedValue = $expectedPort
(Fld "m_SimulateToggle").Checked = $false
$data.FilePath = $file
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call "UpdateStartAvailability"
[System.Windows.Forms.Application]::DoEvents()
Check "the device port is selected" ($portCombo.SelectedValue -eq $expectedPort) ("selected=" + $portCombo.SelectedValue)

Call "SetRunningState"
Pump 6
$state = (Fld "m_State").ToString()
Check "session running from the device" ("Running" -eq $state)      ("state=" + $state + " status='" + (Fld "m_StatusLabel").Text + "'")
Check "readings recorded"               ($data.NumDataPoints -gt 0) ("readings=" + $data.NumDataPoints)
Check "mean is a bit average"           (($data.CurrentAverage -gt 0.4) -and ($data.CurrentAverage -lt 0.6)) ("mean=" + $data.CurrentAverage)
Call "SetIdleState"
Pump 1

$onDisk = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "the session reached the file" ($onDisk -gt 0) ("readings on disk=" + $onDisk)
$xml = New-Object System.Xml.XmlDocument
$wellFormed = $true
try { $xml.Load($file) } catch { $wellFormed = $false }
Check "file well formed"             $wellFormed ("root=" + $(if ($wellFormed) { $xml.DocumentElement.Name } else { "unparseable" }))
Check "recorded as a device session" ($wellFormed -and ($xml.DocumentElement.GetAttribute("Simulated") -eq "false")) ("Simulated='" + $(if ($wellFormed) { $xml.DocumentElement.GetAttribute("Simulated") } else { "" }) + "'")

Write-Host ""
Write-Host "=== 5. A SECOND DEVICE SESSION APPENDS ==="
Call "SetRunningState"
Pump 5
Check "second session started" ("Running" -eq (Fld "m_State").ToString()) ("status='" + (Fld "m_StatusLabel").Text + "'")
Call "SetIdleState"
Pump 1
$onDisk2 = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "appended, first session kept" ($onDisk2 -gt $onDisk) ("before=" + $onDisk + " after=" + $onDisk2)
$xml2 = New-Object System.Xml.XmlDocument
$ok2 = $true
try { $xml2.Load($file) } catch { $ok2 = $false }
Check "still one well formed session" ($ok2 -and ($xml2.DocumentElement.ChildNodes.Count -eq $onDisk2)) ("children=" + $(if ($ok2) { $xml2.DocumentElement.ChildNodes.Count } else { 0 }))

Write-Host ""
Write-Host "=== 6. A TIMED SESSION FROM THE DEVICE ==="
(Fld "m_SessionLengthUpDown").Value = 1
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
$deadline = (Get-Date).AddSeconds(120)
while (((Get-Date) -lt $deadline) -and ("Running" -eq (Fld "m_State").ToString())) {
    [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20
}
Check "stopped on its own"    ("Idle" -eq (Fld "m_State").ToString())             ("state=" + (Fld "m_State").ToString())
Check "stopped at one minute" ((Fld "m_SessionTimerTextBox").Text -eq "00:01:00") ("shown=" + (Fld "m_SessionTimerTextBox").Text)
Check "says why it stopped"   ((Fld "m_StatusLabel").Text -match "stopped after 1 minute") ("status='" + (Fld "m_StatusLabel").Text + "'")
$onDisk3 = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "timed session on disk" ($onDisk3 -gt $onDisk2) ("before=" + $onDisk2 + " after=" + $onDisk3)

# A minute of readings at ten a second, allowing for the ones lost to starting and stopping
$recorded = $onDisk3 - $onDisk2
$expectedReadings = 600
$rate = [math]::Round($recorded / 60.0, 2)
Check "a minute of readings recorded" (($recorded -gt ($expectedReadings * 0.8)) -and ($recorded -lt ($expectedReadings * 1.1))) ("recorded=" + $recorded + " in 60s = " + $rate + "/s, the timer asks for 10/s")

Write-Host ""
Write-Host "=== 7. THE RECORDED FILE ANALYSES ==="
$analysis = New-Object RandomNumberGenerator.StatisticalAnalysis
$points = $analysis.LoadResultFile($file)
Check "analysis loaded the file" ($points.Count -eq $onDisk3) ("analysed=" + $points.Count + " file=" + $onDisk3)
$stats = $analysis.LoadedFileStats
Check "mean of the device readings" (([math]::Abs($stats.Mean - 0.5)) -lt 0.01) ("mean=" + $stats.Mean)
Check "the readings have spread"    ($stats.StandardDeviation -gt 0)             ("std dev=" + $stats.StandardDeviation)
$UNBIASED_MEAN = 0.5
$expected = [RandomNumberGenerator.SignificanceTest]::CompareWithExpected($points, $UNBIASED_MEAN)
Check "tested against an unbiased generator" ($expected.Valid) ("p=" + $expected.Probability + " significant=" + $expected.Significant)

$form.Close()
[System.Windows.Forms.Application]::DoEvents()

Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
