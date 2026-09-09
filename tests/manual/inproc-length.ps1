#########################################################################################################################
# File Name:      inproc-length.ps1
# Description:    Checks the session length and the file surviving a session, driving the form directly
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

# Drives the real form, with its real event wiring, in this process. The application's own controls do
# not answer text messages sent from outside it, so the session length is set the way the form sees it
# rather than by typing into the window.
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms

$binDir = $env:RNG_BIN
[System.Reflection.Assembly]::LoadFrom((Join-Path $binDir "Random Number Generator.exe")) | Out-Null
[Environment]::CurrentDirectory = $binDir

$script:pass = 0; $script:fail = 0
function Check($name, $condition, $detail) {
    if ($condition) { $script:pass++; Write-Host ("  PASS  " + $name + "   " + $detail) }
    else            { $script:fail++; Write-Host ("  FAIL  " + $name + "   " + $detail) }
}

$file = Join-Path $WorkRoot "inproc-length.rng"
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
function Fld($name) { return $form.GetType().GetField($name, $BF).GetValue($form) }
function Call($name) { return $form.GetType().GetMethod($name, $BF).Invoke($form, @()) }
function State() { return (Fld "m_State").ToString() }
function StatusText() { return (Fld "m_StatusLabel").Text }

$form.Show()
[System.Windows.Forms.Application]::DoEvents()
Start-Sleep -Milliseconds 500

Write-Host "=== 0. SETUP ==="
# Simulate rather than a device, and point the session at a new file, the way the form does after browsing
(Fld "m_SimulateToggle").Checked = $true
[System.Windows.Forms.Application]::DoEvents()
$data.FilePath = $file
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call "UpdateStartAvailability"
[System.Windows.Forms.Application]::DoEvents()
Check "Start offered with a file chosen" ((Fld "m_StartButton").Enabled) ""
Check "no length set by default" ((Fld "m_SessionLengthUpDown").Value -eq 0) ("value=" + (Fld "m_SessionLengthUpDown").Value)

Write-Host ""
Write-Host "=== 1. A LENGTH OF ZERO KEEPS RECORDING ==="
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
Check "running" ("Running" -eq (State)) ("state=" + (State) + " status='" + (StatusText) + "'")
$deadline = (Get-Date).AddSeconds(8)
while ((Get-Date) -lt $deadline) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 }
Check "still running after 8s with no length" ("Running" -eq (State)) ("elapsed=" + $timer.ElapsedSeconds + "s state=" + (State))
Check "length locked while running" (-not (Fld "m_SessionLengthUpDown").Enabled) ""
$pointsFirst = $data.NumDataPoints
Check "readings taken" ($pointsFirst -gt 0) ("readings=" + $pointsFirst)

Call "SetIdleState"
[System.Windows.Forms.Application]::DoEvents()
Start-Sleep -Milliseconds 500
$diskFirst = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "file stays chosen after Stop" ((Fld "m_FileTextBox").Text -eq [System.IO.Path]::GetFileName($file)) ("'" + (Fld "m_FileTextBox").Text + "'")
Check "Start offered again"          ((Fld "m_StartButton").Enabled)  ""
Check "length offered again"         ((Fld "m_SessionLengthUpDown").Enabled) ""
Check "first session on disk"        ($diskFirst -gt 0) ("readings=" + $diskFirst)

Write-Host ""
Write-Host "=== 2. A SECOND SESSION APPENDS TO THE SAME FILE ==="
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
Check "second session started" ("Running" -eq (State)) ("state=" + (State) + " status='" + (StatusText) + "'")
$deadline = (Get-Date).AddSeconds(5)
while ((Get-Date) -lt $deadline) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 }
Call "SetIdleState"
[System.Windows.Forms.Application]::DoEvents()
Start-Sleep -Milliseconds 500
$diskSecond = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "second session appended, first kept" ($diskSecond -gt $diskFirst) ("before=" + $diskFirst + " after=" + $diskSecond)
$xml = New-Object System.Xml.XmlDocument
$wellFormed = $true
try { $xml.Load($file) } catch { $wellFormed = $false }
Check "file still parses as XML" $wellFormed ("root=" + $(if ($wellFormed) { $xml.DocumentElement.Name } else { "unparseable" }))
if ($wellFormed) { Check "every reading is in the one session" ($xml.DocumentElement.ChildNodes.Count -eq $diskSecond) ("children=" + $xml.DocumentElement.ChildNodes.Count) }

Write-Host ""
Write-Host "=== 3. A LENGTH OF ONE MINUTE STOPS THE SESSION ==="
(Fld "m_SessionLengthUpDown").Value = 1
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
Check "running with a length set" ("Running" -eq (State)) ("state=" + (State))

$deadline = (Get-Date).AddSeconds(90)
while (((Get-Date) -lt $deadline) -and ("Running" -eq (State))) {
    [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20
}
$elapsed = $timer.ElapsedSeconds
$shown   = (Fld "m_SessionTimerTextBox").Text
$status  = StatusText
Check "stopped on its own"      ("Idle" -eq (State))        ("state=" + (State))
Check "stopped at one minute"   ($shown -eq "00:01:00")       ("shown=" + $shown + " elapsed=" + $elapsed + "s")
Check "says why it stopped"     ($status -match "Session stopped after 1 minute") ("status='" + $status + "'")
Check "length offered again"    ((Fld "m_SessionLengthUpDown").Enabled) ""
Check "Start offered again"     ((Fld "m_StartButton").Enabled) ""
Check "Stop no longer offered"  (-not (Fld "m_StopButton").Enabled) ""

$pointsAfter = $data.NumDataPoints
$deadline = (Get-Date).AddSeconds(3)
while ((Get-Date) -lt $deadline) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 }
Check "no longer recording" ($data.NumDataPoints -eq $pointsAfter) ("readings=" + $pointsAfter + " -> " + $data.NumDataPoints)
$diskThird = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "timed session on disk" ($diskThird -gt $diskSecond) ("before=" + $diskSecond + " after=" + $diskThird)

Write-Host ""
Write-Host "=== 4. CLOSE ==="
$closed = $true
try { $form.Close(); [System.Windows.Forms.Application]::DoEvents() } catch { $closed = $false; Write-Host ("    " + $_.Exception.Message) }
Check "closed without an exception" $closed ""

Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
