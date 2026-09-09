#########################################################################################################################
# File Name:      check-reinit.ps1
# Description:    Switches between the device and the simulator, which rebuilds the native interface
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

# Switching between the device and the simulator, and back, tears the native interface down and builds a
# new one each time. That is the one thing a user does that re-initializes it inside a running application,
# so it is worth knowing the device still reads after being let go of and picked back up.
$ErrorActionPreference = "Continue"
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

# A round of recording, reporting what came back
function RecordRound($label, $simulate, $file) {
    if (Test-Path $file) { Remove-Item $file -Force }
    (Fld "m_SimulateToggle").Checked = $simulate
    [System.Windows.Forms.Application]::DoEvents()
    $data.FilePath = $file
    (Fld "m_FileTextBox").Text = [System.IO.Path]::GetFileName($file)
    Call "UpdateStartAvailability"
    (Fld "m_ResultChart").Clear()
    $data.Reset()
    [System.Windows.Forms.Application]::DoEvents()

    $before = $data.NumDataPoints
    Call "SetRunningState"
    Pump 5
    $state  = (Fld "m_State").ToString()
    $status = (Fld "m_StatusLabel").Text
    $points = $data.NumDataPoints - $before
    Call "SetIdleState"
    Pump 1

    $onDisk = 0
    $simulated = ""
    if (Test-Path $file) {
        $onDisk = ([regex]::Matches((Get-Content $file -Raw), "</Data>")).Count
        $xml = New-Object System.Xml.XmlDocument
        try { $xml.Load($file); $simulated = $xml.DocumentElement.GetAttribute("Simulated") } catch { }
    }

    Check ($label + ": ran without error")   (($state -eq "Running") -and ($status -notmatch "Error")) ("state=" + $state + " status='" + $status + "'")
    Check ($label + ": recorded readings")   ($points -gt 0)   ("readings=" + $points)
    Check ($label + ": reached the file")    ($onDisk -gt 0)   ("on disk=" + $onDisk)
    Check ($label + ": recorded its source") ($simulated -eq $(if ($simulate) { "true" } else { "false" })) ("Simulated='" + $simulated + "'")
}

$portCombo = Fld "m_PortComboBox"
Check "a device was discovered" ($portCombo.Items.Count -gt 0) ("devices=" + $portCombo.Items.Count)
$devicePort = 0
if ($portCombo.Items.Count -gt 0) { $devicePort = $portCombo.Items[0].Port }

Write-Host ""
Write-Host "=== 1. DEVICE ==="
$portCombo.SelectedValue = $devicePort
RecordRound "device" $false (Join-Path $WorkRoot "reinit-device1.rng")

Write-Host ""
Write-Host "=== 2. SIMULATOR, WHICH REPLACES THE DEVICE INTERFACE ==="
RecordRound "simulator" $true (Join-Path $WorkRoot "reinit-sim.rng")

Write-Host ""
Write-Host "=== 3. BACK TO THE DEVICE ==="
$portCombo.SelectedValue = $devicePort
RecordRound "device again" $false (Join-Path $WorkRoot "reinit-device2.rng")

Write-Host ""
Write-Host "=== 4. AND ONCE MORE, TO BE SURE IT IS NOT JUST THE SECOND TIME ==="
RecordRound "simulator again" $true (Join-Path $WorkRoot "reinit-sim2.rng")
$portCombo.SelectedValue = $devicePort
RecordRound "device a third time" $false (Join-Path $WorkRoot "reinit-device3.rng")

$form.Close()
[System.Windows.Forms.Application]::DoEvents()
Write-Host ""
Write-Host ("PASS " + $script:pass + "   FAIL " + $script:fail)
