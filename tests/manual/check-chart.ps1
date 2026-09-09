#########################################################################################################################
# File Name:      check-chart.ps1
# Description:    Checks the chart and the statistics beside it agree on every path through the window
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

# The chart and the statistics beside it describe the same readings, so they have to agree everywhere.
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms

# The window has to be set up the way Program.cs sets it up, before any of it is built. Without this the
# form is drawn without visual styles, and controls that ask the theme how to draw themselves fall back to
# something that looks nothing like the shipped application - the spin buttons on the session length come
# out almost black. A window that does not look like the real one is no good for judging how it looks.
try {
    [System.Windows.Forms.Application]::EnableVisualStyles()
    [System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)
} catch {
    # Already set for this process, which is fine
}
$binDir = $env:RNG_BIN
[System.Reflection.Assembly]::LoadFrom((Join-Path $binDir "Random Number Generator.exe")) | Out-Null
[Environment]::CurrentDirectory = $binDir

$script:pass = 0; $script:fail = 0
function Check($name, $condition, $detail) {
    if ($condition) { $script:pass++; Write-Host ("  PASS  " + $name + "   " + $detail) }
    else            { $script:fail++; Write-Host ("  FAIL  " + $name + "   " + $detail) }
}

$fileA = Join-Path $WorkRoot "chart-a.rng"
$fileB = Join-Path $WorkRoot "chart-b.rng"
foreach ($f in @($fileA, $fileB)) { if (Test-Path $f) { Remove-Item $f -Force } }

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
function Call1($name, $arg) { return $form.GetType().GetMethod($name, $BF).Invoke($form, [object[]]@([string]$arg)) }
function Pump($s) { $d = (Get-Date).AddSeconds($s); while ((Get-Date) -lt $d) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 } }
# The readings the chart is drawing. The trace series carries one extra point: Clear leaves a single
# point at the centre behind so the chart area still draws when there is nothing in it, so that seed is
# taken off the count. The running mean series has no seed and is checked against it.
function Charted() {
    $chart = Fld "m_ResultChart"
    return ($chart.Series[0].Points.Count - 1)
}
function ChartedMean() {
    $chart = Fld "m_ResultChart"
    return $chart.Series[1].Points.Count
}
function Readings() { return $data.NumDataPoints }

$form.Show(); [System.Windows.Forms.Application]::DoEvents()
(Fld "m_SimulateToggle").Checked = $true
$data.FilePath = $fileA
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($fileA)
Call "UpdateStartAvailability"

Write-Host "=== 1. FIRST SESSION ==="
Call "SetRunningState"; Pump 4
Check "chart matches the readings while recording" ((Charted) -eq (Readings)) ("charted=" + (Charted) + " readings=" + (Readings))
Call "SetIdleState"; Pump 1
$afterFirst = Readings
Check "chart matches after Stop" ((Charted) -eq $afterFirst) ("charted=" + (Charted) + " readings=" + $afterFirst)

Write-Host ""
Write-Host "=== 2. SECOND SESSION INTO THE SAME FILE ==="
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
Check "chart kept what the first session recorded" ((Charted) -eq $afterFirst) ("charted=" + (Charted) + " was=" + $afterFirst)
Pump 4
Check "chart still matches the readings" ((Charted) -eq (Readings)) ("charted=" + (Charted) + " readings=" + (Readings))
Call "SetIdleState"; Pump 1
$afterSecond = Readings
Check "both sessions on the chart" ($afterSecond -gt $afterFirst) ("first=" + $afterFirst + " total=" + $afterSecond)

Write-Host ""
Write-Host "=== 3. A DIFFERENT FILE IS CHOSEN ==="
# what browsing to a new file does: end the session, clear the chart, point the data at the file
Call "EndSession" | Out-Null
(Fld "m_ResultChart").Clear()
$data.Reset()
$data.FilePath = $fileB
(Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($fileB)
Call "UpdateStatisticsDisplay"
[System.Windows.Forms.Application]::DoEvents()
Check "chart emptied for the new file"    (((Charted) -eq 0) -and ((ChartedMean) -eq 0)) ("charted=" + (Charted) + " mean series=" + (ChartedMean))
Check "readings emptied with it"          ((Readings) -eq 0) ("readings=" + (Readings))
Call "SetRunningState"; Pump 4
Check "new file charts only its own readings" ((Charted) -eq (Readings)) ("charted=" + (Charted) + " readings=" + (Readings))
Check "nothing left over from the last file"  ((Readings) -lt $afterSecond) ("readings=" + (Readings) + " last file had " + $afterSecond)
Call "SetIdleState"; Pump 1

Write-Host ""
Write-Host "=== 4. AN EXISTING FILE IS REOPENED ==="
(Fld "m_ResultChart").Clear()
$loaded = Call1 "LoadExistingSessionFile" $fileA
Pump 1
Check "existing file loaded"            ($loaded -eq $true) ("returned " + $loaded)
Check "chart drew the loaded readings"  ((Charted) -eq (Readings)) ("charted=" + (Charted) + " readings=" + (Readings))
Check "loaded what the earlier sessions recorded" ((Readings) -eq $afterSecond) ("loaded=" + (Readings) + " recorded=" + $afterSecond)
Call "SetRunningState"
[System.Windows.Forms.Application]::DoEvents()
Check "starting keeps the loaded readings on the chart" ((Charted) -eq $afterSecond) ("charted=" + (Charted))
Pump 3
Check "chart matches the readings as it continues" ((Charted) -eq (Readings)) ("charted=" + (Charted) + " readings=" + (Readings))
Call "SetIdleState"; Pump 1

Write-Host ""
Write-Host "=== 5. CLEAR ==="
$data.Reset()
(Fld "m_ResultChart").Clear()
Call "UpdateStatisticsDisplay"
Check "chart and readings both emptied" (((Charted) -eq 0) -and ((ChartedMean) -eq 0) -and ((Readings) -eq 0)) ("charted=" + (Charted) + " mean series=" + (ChartedMean) + " readings=" + (Readings))

$form.Close()
Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
