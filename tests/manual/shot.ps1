#########################################################################################################################
# File Name:      shot.ps1
# Description:    Saves screenshots of the window in each of its states
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

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\win32b.ps1"
. "$PSScriptRoot\uia.ps1"
Add-Type -AssemblyName System.Drawing

Add-Type @'
using System;
using System.Runtime.InteropServices;
public class Shot {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RC r);
    [StructLayout(LayoutKind.Sequential)] public struct RC { public int L, T, R, B; }
}
'@

function Save-Window($hwnd, $path) {
    $r = New-Object Shot+RC
    [void][Shot]::GetWindowRect($hwnd, [ref]$r)
    $w = $r.R - $r.L; $h = $r.B - $r.T
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $hdc = $g.GetHdc()
    [void][Shot]::PrintWindow($hwnd, $hdc, 2)      # PW_RENDERFULLCONTENT
    $g.ReleaseHdc($hdc)
    $g.Dispose()
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Host ("  saved {0}  ({1} x {2})" -f (Split-Path $path -Leaf), $w, $h)
}

$p = Start-Process (Join-Path $env:RNG_BIN "Random Number Generator.exe") -PassThru
$hwnd = Get-MainWindow $p
[void](Wait-ForIdle $hwnd)
Start-Sleep -Seconds 1

Save-Window $hwnd (Join-Path $WorkRoot "ui-execute.png")

# put some data on screen so the chart and statistics are not empty
$chk = ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*BUTTON*" -and $_.Text -eq "" } | Select-Object -First 1)
Click-Button $chk.Handle
Start-Sleep -Milliseconds 800
Save-Window $hwnd (Join-Path $WorkRoot "ui-simulate.png")
$file = Join-Path $WorkRoot "ui-shot.rng"
if (Test-Path $file) { Remove-Item $file -Force }
Post-Click ((Get-Controls $hwnd) | Where-Object { $_.Text -eq "Browse..." } | Select-Object -First 1).Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $file); $m = Wait-Dialog $p.Id 8; if ($m) { [void](Answer-MessageBox $m "OK") } }
Start-Sleep -Seconds 1
Click-Button ((Get-Controls $hwnd) | Where-Object { $_.Text -eq "Start" } | Select-Object -First 1).Handle
Start-Sleep -Seconds 6
Save-Window $hwnd (Join-Path $WorkRoot "ui-execute-running.png")
Click-Button ((Get-Controls $hwnd) | Where-Object { $_.Text -eq "Stop" } | Select-Object -First 1).Handle
Start-Sleep -Seconds 2

# the analyze tab, with a baseline and a result loaded
$tab = ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*SysTabControl32*" } | Select-Object -First 1)
[void](Select-Tab $tab.Handle 1)
Start-Sleep -Seconds 2
$browse = Get-AnalyzeBrowseButtons $hwnd
if ($browse.Count -eq 2) {
    Post-Click $browse[0].Handle
    $dlg = Wait-Dialog $p.Id 15
    if ($dlg) { [void](Complete-FileDialog $dlg (Join-Path $WorkRoot "ui-sessionA.rng")) }
    Start-Sleep -Seconds 3
    Post-Click $browse[1].Handle
    $dlg = Wait-Dialog $p.Id 15
    if ($dlg) { [void](Complete-FileDialog $dlg (Join-Path $WorkRoot "ui-sessionB.rng")) }
    Start-Sleep -Seconds 3
}
Save-Window $hwnd (Join-Path $WorkRoot "ui-analyze.png")

Stop-Process -Id $p.Id -Force
