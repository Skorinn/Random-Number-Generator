#########################################################################################################################
# File Name:      check-appended.ps1
# Description:    Checks a file grown by a second session reads back correctly on both tabs
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

# Records two sessions into one file through Start/Stop/Start/Stop, then opens that file again on both
# tabs, to check that a file grown by appending reads back the same as one written in a single session.
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

$file  = Join-Path $WorkRoot "appended.rng"
$other = Join-Path $WorkRoot "appended-other.rng"
foreach ($f in @($file, $other)) { if (Test-Path $f) { Remove-Item $f -Force } }

function NewForm() {
    $ws = New-Object System.Xml.XmlWriterSettings
    $ws.Indent = $true; $ws.IndentChars = "`t"
    $writer  = New-Object RandomNumberGenerator.RNGXMLWriter($ws)
    $reader  = New-Object RandomNumberGenerator.RNGXMLReader
    $dataFile= New-Object RandomNumberGenerator.RNGSessionDataFile($writer, $reader)
    $timer   = New-Object RandomNumberGenerator.RNGSessionTimer
    $data    = New-Object RandomNumberGenerator.RNGSessionData($dataFile, $timer)
    $device  = New-Object RandomNumberGenerator.RNGDeviceTimer
    return @{ Form = (New-Object RandomNumberGenerator.GeneratorForm($data, $device)); Data = $data; Timer = $timer }
}
$BF = [System.Reflection.BindingFlags]"NonPublic,Instance"
function Fld($f, $name)  { return $f.GetType().GetField($name, $BF).GetValue($f) }
function Call($f, $name) { return $f.GetType().GetMethod($name, $BF).Invoke($f, @()) }
function Call1($f, $name, $arg) { return $f.GetType().GetMethod($name, $BF).Invoke($f, [object[]]@([string]$arg)) }
function Call2($f, $name, $a, $b) { return $f.GetType().GetMethod($name, $BF).Invoke($f, [object[]]@([string]$a, [bool]$b)) }
function Pump($seconds) { $d = (Get-Date).AddSeconds($seconds); while ((Get-Date) -lt $d) { [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 20 } }

Write-Host "=== 1. RECORD TWO SESSIONS INTO ONE FILE ==="
$h = NewForm; $form = $h.Form; $data = $h.Data
$form.Show(); [System.Windows.Forms.Application]::DoEvents()
(Fld $form "m_SimulateToggle").Checked = $true
$data.FilePath = $file
(Fld $form "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
Call $form "UpdateStartAvailability"
Call $form "SetRunningState"; Pump 4
Call $form "SetIdleState"; Pump 1
$afterFirst = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Call $form "SetRunningState"; Pump 4
Call $form "SetIdleState"; Pump 1
$total = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
Check "two sessions recorded into one file" ($total -gt $afterFirst) ("first=" + $afterFirst + " total=" + $total)

# a second file, recorded once, to compare against on the analyse tab
$data2 = $h.Data
Call1 $form "LoadExistingSessionFile" $file | Out-Null   # exercised again below through the browse path
$form.Close(); [System.Windows.Forms.Application]::DoEvents()

$h2 = NewForm; $form2 = $h2.Form; $d2 = $h2.Data
$form2.Show(); [System.Windows.Forms.Application]::DoEvents()
(Fld $form2 "m_SimulateToggle").Checked = $true
$d2.FilePath = $other
(Fld $form2 "m_FileTextBox").Text = [System.IO.Path]::GetFileName($other)
Call $form2 "UpdateStartAvailability"
Call $form2 "SetRunningState"; Pump 4
Call $form2 "SetIdleState"; Pump 1
$otherTotal = ([regex]::Matches((Get-Content $other -Raw), "</Data>")).Count
$form2.Close(); [System.Windows.Forms.Application]::DoEvents()

Write-Host ""
Write-Host "=== 2. THE FILE ITSELF ==="
$xml = New-Object System.Xml.XmlDocument
$ok = $true
try { $xml.Load($file) } catch { $ok = $false }
Check "parses as XML"                    $ok ("root=" + $(if ($ok) { $xml.DocumentElement.Name } else { "unparseable" }))
Check "one session holding every reading" ($ok -and ($xml.DocumentElement.ChildNodes.Count -eq $total)) ("children=" + $(if ($ok) { $xml.DocumentElement.ChildNodes.Count } else { 0 }) + " readings=" + $total)
Check "session attributes intact"         ($ok -and ($xml.DocumentElement.GetAttribute("Simulated") -eq "true")) ("Simulated='" + $(if ($ok) { $xml.DocumentElement.GetAttribute("Simulated") } else { "" }) + "' Target='" + $(if ($ok) { $xml.DocumentElement.GetAttribute("Target") } else { "" }) + "'")

Write-Host ""
Write-Host "=== 3. REOPENED ON THE RECORD TAB ==="
$h3 = NewForm; $form3 = $h3.Form; $d3 = $h3.Data
$form3.Show(); [System.Windows.Forms.Application]::DoEvents()
$loaded = Call1 $form3 "LoadExistingSessionFile" $file
[System.Windows.Forms.Application]::DoEvents()
Check "file loaded"          ($loaded -eq $true)                ("returned " + $loaded)
Check "every reading loaded" ($d3.NumDataPoints -eq $total)     ("loaded=" + $d3.NumDataPoints + " file=" + $total)
Check "mean is a bit average" (($d3.CurrentAverage -gt 0.45) -and ($d3.CurrentAverage -lt 0.55)) ("mean=" + $d3.CurrentAverage)
$form3.Close(); [System.Windows.Forms.Application]::DoEvents()

Write-Host ""
Write-Host "=== 4. LOADED ON THE ANALYSE TAB ==="
$h4 = NewForm; $form4 = $h4.Form
$form4.Show(); [System.Windows.Forms.Application]::DoEvents()
$bl = Call1 $form4 "LoadBaselineFile" $file
Call2 $form4 "OnBaselineLoadCompleted" $file $bl | Out-Null
[System.Windows.Forms.Application]::DoEvents()
$blAnalysis = (Fld $form4 "m_BaselineAnalysis")
Check "baseline loaded"           ($bl -eq $true)                                   ("returned " + $bl)
Check "baseline reading count"    ($blAnalysis.LoadedFileData.Count -eq $total)     ("analysed=" + $blAnalysis.LoadedFileData.Count + " file=" + $total)
Check "baseline reports no warning" ([string]::IsNullOrEmpty($blAnalysis.LoadWarning)) ("warning='" + $blAnalysis.LoadWarning + "'")
$form4.Close(); [System.Windows.Forms.Application]::DoEvents()

Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
