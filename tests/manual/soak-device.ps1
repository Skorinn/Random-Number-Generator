#########################################################################################################################
# File Name:      soak-device.ps1
# Description:    Records from the device for a given number of minutes, watching for read failures
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

# A clean device soak: one form, one device interface, one long session, the way a user records. Nothing
# else initializes the native layer, so a read failure here is the application's rather than the harness's.
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

$minutes = 2
if ($args.Count -gt 0) { $minutes = [int]$args[0] }

$file = Join-Path $WorkRoot "soak-device.rng"
if (Test-Path $file) { Remove-Item $file -Force }

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

$form.Show()
$d = (Get-Date).AddSeconds(8)
while ((Get-Date) -lt $d) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 }

$portCombo = Fld "m_PortComboBox"
Check "a device was discovered" ($portCombo.Items.Count -gt 0) ("devices=" + $portCombo.Items.Count)
if ($portCombo.Items.Count -eq 0) { $form.Close(); exit 1 }

# Record from the device, for the length asked for, exactly as pressing Start does
(Fld "m_SimulateToggle").Checked = $false
$data.FilePath = $file
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call "UpdateStartAvailability"
(Fld "m_SessionLengthUpDown").Value = $minutes
[System.Windows.Forms.Application]::DoEvents()

Write-Host ("  recording from the device for " + $minutes + " minute(s)...")
$started = Get-Date
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
Check "session started" ("Running" -eq (Fld "m_State").ToString()) ("status='" + (Fld "m_StatusLabel").Text + "'")

# Watch it run, noting the first moment anything goes wrong rather than only the state at the end
$firstError = ""
$errorAt = 0
$deadline = (Get-Date).AddSeconds(($minutes * 60) + 60)
while (((Get-Date) -lt $deadline) -and ("Running" -eq (Fld "m_State").ToString())) {
    [System.Windows.Forms.Application]::DoEvents()
    $status = (Fld "m_StatusLabel").Text
    if (($status -match "Error") -and ("" -eq $firstError)) {
        $firstError = $status
        $errorAt = [int]((Get-Date) - $started).TotalSeconds
    }
    Start-Sleep -Milliseconds 20
}
$ranFor = [int]((Get-Date) - $started).TotalSeconds
$shown = (Fld "m_SessionTimerTextBox").Text
$status = (Fld "m_StatusLabel").Text

Check "no read error during the session" ("" -eq $firstError) ("first error at " + $errorAt + "s: '" + $firstError + "'")
Check "ran the whole length"             ($shown -eq ("00:{0:00}:00" -f $minutes)) ("shown=" + $shown + " wall clock=" + $ranFor + "s")
Check "stopped because it was time"      ($status -match "stopped after") ("status='" + $status + "'")

$onDisk = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
$expected = $minutes * 60 * 10
$rate = [math]::Round($onDisk / ($minutes * 60.0), 2)
Write-Host ("  recorded " + $onDisk + " readings in " + ($minutes * 60) + "s = " + $rate + "/s (the timer asks for 10/s)")
Check "readings were recorded throughout" ($onDisk -gt ($expected * 0.3)) ("recorded=" + $onDisk + " of a possible " + $expected)

$xml = New-Object System.Xml.XmlDocument
$ok = $true
try { $xml.Load($file) } catch { $ok = $false }
Check "file well formed" ($ok -and ($xml.DocumentElement.ChildNodes.Count -eq $onDisk)) ("children=" + $(if ($ok) { $xml.DocumentElement.ChildNodes.Count } else { 0 }))

$analysis = New-Object RandomNumberGenerator.StatisticalAnalysis
$points = $analysis.LoadResultFile($file)
$stats = $analysis.LoadedFileStats
Check "mean of a long device run" (([math]::Abs($stats.Mean - 0.5)) -lt 0.005) ("mean=" + $stats.Mean + " over " + $points.Count + " readings")

$form.Close()
[System.Windows.Forms.Application]::DoEvents()
Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
