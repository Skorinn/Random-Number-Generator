#########################################################################################################################
# File Name:      check-unplug.ps1
# Description:    Checks what happens when the device is pulled part way through a session
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

# The one path neither the attached nor the unattached state can reach on its own: the device going away
# while a session is recording. It is the path the read recovery rewrote, so it is worth seeing directly.
# The cable has to be pulled by hand, so this waits for it, and it records every change to the status bar
# rather than sampling it once: pulling the cable also sets the window scanning for devices, and that
# scan puts its own message up for as long as it runs.
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

$file = Join-Path $WorkRoot "unplug-session.rng"
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
function State()  { return (Fld "m_State").ToString() }
function Status() { return (Fld "m_StatusLabel").Text }
function Ports()  { return ([System.IO.Ports.SerialPort]::GetPortNames()).Count }

# Pumps the window for a while, logging every change to the status bar as it goes
$script:timeline = @()
$script:lastStatus = ""
$script:clock = [System.Diagnostics.Stopwatch]::StartNew()
function PumpLogging($seconds, $stopWhen = $null) {
    $end = (Get-Date).AddSeconds($seconds)
    while ((Get-Date) -lt $end) {
        [System.Windows.Forms.Application]::DoEvents()
        $s = Status
        if ($s -ne $script:lastStatus) {
            $script:lastStatus = $s
            $at = [math]::Round($script:clock.Elapsed.TotalSeconds, 1)
            $script:timeline += [pscustomobject]@{ At = $at; State = (State); Status = $s }
        }
        if ($null -ne $stopWhen) { if (& $stopWhen) { return $true } }
        Start-Sleep -Milliseconds 20
    }
    return $false
}
function ShowTimeline($from) {
    foreach ($e in $script:timeline) {
        if ($e.At -ge $from) { Write-Host ("    " + $e.At.ToString("0.0").PadLeft(6) + "s  [" + $e.State.PadRight(7) + "] " + $e.Status) }
    }
}

$form.Show()
[System.Windows.Forms.Application]::DoEvents()

Write-Host "=== 0. THE DEVICE HAS TO BE ATTACHED TO START WITH ==="
if (0 -eq (Ports)) {
    Write-Host ""
    Write-Host "***********************************************************"
    Write-Host "*  PLUG THE CABLE IN to begin. Waiting up to 120 seconds. *"
    Write-Host "***********************************************************"
    Write-Host ""
    $null = PumpLogging 120 { (Ports) -gt 0 }
}
Check "the device is attached" ((Ports) -gt 0) ("ports=" + (Ports))
if (0 -eq (Ports)) { $form.Close(); exit 1 }

# Let the window find it
$null = PumpLogging 10
$portCombo = Fld "m_PortComboBox"
Check "the window found it" ($portCombo.Items.Count -gt 0) ("listed=" + $portCombo.Items.Count)
if ($portCombo.Items.Count -eq 0) { $form.Close(); exit 1 }
$portCombo.SelectedValue = $portCombo.Items[0].Port

(Fld "m_SimulateToggle").Checked = $false
$data.FilePath = $file
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call "UpdateStartAvailability"
[System.Windows.Forms.Application]::DoEvents()

Write-Host ""
Write-Host "=== 1. RECORDING FROM THE DEVICE ==="
Call "SetRunningState"
$null = PumpLogging 6
Check "recording, with the cable still in" (("Running" -eq (State)) -and ($data.NumDataPoints -gt 0)) ("readings=" + $data.NumDataPoints + " state=" + (State))
if ("Running" -ne (State)) {
    Write-Host "  the session was not running when the cable was still in, so there is nothing to interrupt"
    ShowTimeline 0
    $form.Close(); exit 1
}
$healthyReadings = $data.NumDataPoints
$promptedAt = [math]::Round($script:clock.Elapsed.TotalSeconds, 1)

Write-Host ""
Write-Host "***********************************************************"
Write-Host "*  PULL THE USB CABLE NOW. Waiting up to 120 seconds.     *"
Write-Host "***********************************************************"
Write-Host ""

$stopped = PumpLogging 120 { ("Running" -ne (State)) -or (0 -eq (Ports)) }
$portGoneAt = -1
if (0 -eq (Ports)) { $portGoneAt = [math]::Round($script:clock.Elapsed.TotalSeconds, 1) }
if ($portGoneAt -lt 0) {
    Write-Host "  the cable was never pulled, so there is nothing to report on"
    $form.Close(); exit 2
}

# Keep watching after it stops, so what the user is finally left looking at is what gets judged, not
# whatever happened to be up while the device scan was still running
$null = PumpLogging 25
$stoppedAt = -1
foreach ($e in $script:timeline) { if (($e.At -ge $promptedAt) -and ("Idle" -eq $e.State) -and ($stoppedAt -lt 0)) { $stoppedAt = $e.At } }

Write-Host "=== 2. WHAT THE WINDOW SAID, FROM THE MOMENT THE CABLE WENT ==="
ShowTimeline $promptedAt
Write-Host ""

$settled = Status
Check "the session stopped"        ("Idle" -eq (State))               ("state=" + (State))
Check "it stopped promptly"        (($stoppedAt -lt 0) -or (($stoppedAt - $portGoneAt) -lt 5)) ("cable at " + $portGoneAt + "s, idle at " + $stoppedAt + "s")
Check "the reason is what is left on screen" ($settled -match "Error") ("status='" + $settled + "'")
Check "and it names the device"    ($settled -match "TruRNGpro")      ""

Write-Host ""
Write-Host "=== 3. WHAT WAS KEPT ==="
Check "the data file is still chosen" ((Fld "m_FileTextBox").Text -eq [System.IO.Path]::GetFileName($file)) ("'" + (Fld "m_FileTextBox").Text + "'")
Check "Start is offered again"        ((Fld "m_StartButton").Enabled)      ""
Check "Stop is not offered"           (-not (Fld "m_StopButton").Enabled)  ""
Check "the length is open again"      ((Fld "m_SessionLengthUpDown").Enabled) ""

$onDisk = 0
if (Test-Path $file) { $onDisk = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count }
Check "the readings taken before the cable went were kept" ($onDisk -ge $healthyReadings) ("on disk=" + $onDisk + ", taken before the pull=" + $healthyReadings)
$xml = New-Object System.Xml.XmlDocument
$wellFormed = $true
try { $xml.Load($file) } catch { $wellFormed = $false }
Check "the file is well formed" $wellFormed ("root=" + $(if ($wellFormed) { $xml.DocumentElement.Name } else { "unparseable" }))
Check "and holds every reading"  ($wellFormed -and ($xml.DocumentElement.ChildNodes.Count -eq $onDisk)) ("children=" + $(if ($wellFormed) { $xml.DocumentElement.ChildNodes.Count } else { 0 }))
$analysis = New-Object RandomNumberGenerator.StatisticalAnalysis
$loaded = $analysis.LoadResultFile($file)
Check "the interrupted file still analyses" ($loaded.Count -eq $onDisk) ("analysed=" + $loaded.Count + " of " + $onDisk)

Write-Host ""
Write-Host "=== 4. STARTING AGAIN WITH THE DEVICE STILL GONE ==="
Call "SetRunningState"
$null = PumpLogging 4
Check "it refuses rather than half starting" ("Idle" -eq (State))      ("state=" + (State))
Check "and says why"                         ((Status) -match "Error") ("status='" + (Status) + "'")

Write-Host ""
Write-Host "***********************************************************"
Write-Host "*  PLUG THE CABLE BACK IN. Waiting up to 120 seconds.     *"
Write-Host "***********************************************************"
Write-Host ""
$back = PumpLogging 120 { (Ports) -gt 0 }

if ($back) {
    $null = PumpLogging 12   # the window re-scans when Windows tells it the devices changed
    Write-Host "=== 5. PICKING BACK UP ==="
    $portCombo = Fld "m_PortComboBox"
    Check "the device is found again" ($portCombo.Items.Count -gt 0) ("listed=" + $portCombo.Items.Count)
    if ($portCombo.Items.Count -gt 0) {
        $portCombo.SelectedValue = $portCombo.Items[0].Port
        $before = $onDisk
        Call "SetRunningState"
        $null = PumpLogging 6
        Check "recording picks back up" ("Running" -eq (State)) ("readings=" + $data.NumDataPoints + " status='" + (Status) + "'")
        Call "SetIdleState"
        $null = PumpLogging 2
        $after = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
        Check "it appended to the interrupted file" ($after -gt $before) ("before=" + $before + " after=" + $after)
        $xml2 = New-Object System.Xml.XmlDocument
        $ok2 = $true
        try { $xml2.Load($file) } catch { $ok2 = $false }
        Check "still one well formed session" ($ok2 -and ($xml2.DocumentElement.ChildNodes.Count -eq $after)) ("children=" + $(if ($ok2) { $xml2.DocumentElement.ChildNodes.Count } else { 0 }))
    }
} else {
    Write-Host "  the cable did not come back, so the recovery half was not exercised"
}

$form.Close()
[System.Windows.Forms.Application]::DoEvents()
Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
