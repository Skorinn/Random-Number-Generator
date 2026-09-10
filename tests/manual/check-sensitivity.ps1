#########################################################################################################################
# File Name:      check-sensitivity.ps1
# Description:    Checks the analysis warns when sessions are too short to show the shift being looked for
#
# Copyright (c) 2026 Mike Pullen
# Licensed under the MIT License. See LICENSE in the repository root.
#
# Revision History:
#======================================================================================================================
# 2026/09/09 - Mike Pullen - Original implementation.
#########################################################################################################################
# Drives the analysis tab through all three verdicts: a shift found, nothing found from sessions long
# enough to have found one, and nothing found from sessions too short to have found anything. The third is
# the one the warning exists for.
$ErrorActionPreference = "Continue"
Add-Type -AssemblyName System.Windows.Forms
try {
    [System.Windows.Forms.Application]::EnableVisualStyles()
    [System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false)
} catch { }
# Which build to drive. Set RNG_BIN to test a different one.
if ([string]::IsNullOrEmpty($env:RNG_BIN)) { $env:RNG_BIN = (Resolve-Path (Join-Path $PSScriptRoot "..\..\bin\Release")).Path }
$binDir = $env:RNG_BIN
[System.Reflection.Assembly]::LoadFrom((Join-Path $binDir "Random Number Generator.exe")) | Out-Null
[Environment]::CurrentDirectory = $binDir

$WorkRoot = Join-Path $PSScriptRoot "work"
if (-not (Test-Path $WorkRoot)) { $null = New-Item -ItemType Directory -Path $WorkRoot }

$script:pass = 0; $script:fail = 0
function Check($name, $condition, $detail) {
    if ($condition) { $script:pass++; Write-Host ("  PASS  " + $name + "   " + $detail) }
    else            { $script:fail++; Write-Host ("  FAIL  " + $name + "   " + $detail) }
}

# A session file with a known count, centre and spread. Readings alternate either side of the centre so the
# spread is exactly what was asked for, which makes the detectable shift predictable.
function WriteSession($path, $count, $centre, $spread) {
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('<?xml version="1.0" encoding="utf-8"?>')
    [void]$sb.AppendLine('<Session Simulated="true" Target="-1">')
    for ($i = 0; $i -lt $count; $i++) {
        $offset = if (0 -eq ($i % 2)) { $spread } else { -$spread }
        $v = ($centre + $offset).ToString("F12", [System.Globalization.CultureInfo]::InvariantCulture)
        [void]$sb.AppendLine("`t<Data Time=""00:00:00"">$v</Data>")
    }
    [void]$sb.Append('</Session>')
    [System.IO.File]::WriteAllText($path, $sb.ToString())
}

# The spread a real device reading has: the average of 32768 bytes of bits
$DEVICE_SPREAD = 0.00098

$ws = New-Object System.Xml.XmlWriterSettings
$writer  = New-Object RandomNumberGenerator.RNGXMLWriter($ws)
$reader  = New-Object RandomNumberGenerator.RNGXMLReader
$dataFile= New-Object RandomNumberGenerator.RNGSessionDataFile($writer, $reader)
$timer   = New-Object RandomNumberGenerator.RNGSessionTimer
$data    = New-Object RandomNumberGenerator.RNGSessionData($dataFile, $timer)
$device  = New-Object RandomNumberGenerator.RNGDeviceTimer
$form    = New-Object RandomNumberGenerator.GeneratorForm($data, $device)
$BF = [System.Reflection.BindingFlags]"NonPublic,Instance"
function Fld($name) { return $form.GetType().GetField($name, $BF).GetValue($form) }
function Call1($name, $arg) { return $form.GetType().GetMethod($name, $BF).Invoke($form, [object[]]@([string]$arg)) }
function Call2($name, $a, $b) { return $form.GetType().GetMethod($name, $BF).Invoke($form, [object[]]@([string]$a, [bool]$b)) }
function Verdict() { return (Fld "m_VerdictLabel").Text }
function VerdictColour() { return (Fld "m_VerdictLabel").ForeColor }

$form.Show(); [System.Windows.Forms.Application]::DoEvents()

function LoadPair($baseline, $result) {
    $b = Call1 "LoadBaselineFile" $baseline
    Call2 "OnBaselineLoadCompleted" $baseline $b | Out-Null
    $r = Call1 "LoadResultFile" $result
    Call2 "OnResultLoadCompleted" $result $r | Out-Null
    [System.Windows.Forms.Application]::DoEvents()
}

Write-Host "=== 1. TOO SHORT TO SAY ANYTHING - the case the warning exists for ==="
$shortA = Join-Path $WorkRoot "sens-short-a.rng"
$shortB = Join-Path $WorkRoot "sens-short-b.rng"
WriteSession $shortA 90 0.5 $DEVICE_SPREAD
WriteSession $shortB 90 0.5 $DEVICE_SPREAD
LoadPair $shortA $shortB
$v = Verdict
Write-Host ("  verdict: " + $v)
Check "warns that the sessions are too short" ($v -match "too short to conclude") ""
Check "states the limit"                      ($v -match "can only show a difference of") ""
Check "names the shift being looked for"      ($v -match "0.000100") ""
Check "tells the user what to do"             ($v -match "Record for longer") ""
Check "coloured as a warning"                 ((VerdictColour).ToArgb() -eq ([RandomNumberGenerator.StatusPalette]::WarningText).ToArgb()) ("colour=" + (VerdictColour))

Write-Host ""
Write-Host "=== 2. LONG ENOUGH, NOTHING FOUND - the ordinary outcome ==="
$longA = Join-Path $WorkRoot "sens-long-a.rng"
$longB = Join-Path $WorkRoot "sens-long-b.rng"
WriteSession $longA 1500 0.5 $DEVICE_SPREAD
WriteSession $longB 1500 0.5 $DEVICE_SPREAD
LoadPair $longA $longB
$v = Verdict
Write-Host ("  verdict: " + $v)
Check "reports no significant shift"      ($v -match "No significant shift\.") ""
Check "does not warn"                     (-not ($v -match "too short")) ""
Check "says the shift would have been found" ($v -match "would have been found") ""
Check "coloured as ordinary"              ((VerdictColour).ToArgb() -eq ([RandomNumberGenerator.UiPalette]::MutedText).ToArgb()) ("colour=" + (VerdictColour))

Write-Host ""
Write-Host "=== 3. A SHIFT FOUND - the limit is reported, not warned about ==="
$shiftA = Join-Path $WorkRoot "sens-shift-a.rng"
$shiftB = Join-Path $WorkRoot "sens-shift-b.rng"
WriteSession $shiftA 1500 0.5 $DEVICE_SPREAD
WriteSession $shiftB 1500 0.5005 $DEVICE_SPREAD
LoadPair $shiftA $shiftB
$v = Verdict
Write-Host ("  verdict: " + $v)
Check "reports a significant shift"  ($v -match "Significant shift:") ""
Check "still states the limit"       ($v -match "can show a difference of") ""
Check "does not warn"                (-not ($v -match "too short")) ""
Check "coloured as notable"          ((VerdictColour).ToArgb() -eq ([RandomNumberGenerator.UiPalette]::CardText).ToArgb()) ("colour=" + (VerdictColour))

$form.Close()
[System.Windows.Forms.Application]::DoEvents()
Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
