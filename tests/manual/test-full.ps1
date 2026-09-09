#########################################################################################################################
# File Name:      test-full.ps1
# Description:    Drives the Record tab through a whole session and checks what the window shows
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

$ErrorActionPreference = "Continue"
. "$PSScriptRoot\win32b.ps1"
. "$PSScriptRoot\uia.ps1"

$script:pass = 0; $script:fail = 0
function Check($name, $condition, $detail) {
    if ($condition) { $script:pass++; Write-Host ("  PASS  " + $name + "   " + $detail) }
    else            { $script:fail++; Write-Host ("  FAIL  " + $name + "   " + $detail) }
}

$exe      = (Join-Path $env:RNG_BIN "Random Number Generator.exe")
$fileA    = Join-Path $WorkRoot "ui-sessionA.rng"
$fileB    = Join-Path $WorkRoot "ui-sessionB.rng"
foreach ($f in @($fileA, $fileB)) { if (Test-Path $f) { Remove-Item $f -Force } }

$p = Start-Process $exe -PassThru
$hwnd = Get-MainWindow $p
# the automation window is acquired once the application is up, and reused
[void](Wait-ForIdle $hwnd)   # the device scan takes as long as WMI takes



function F()           { return (Read-Fields ([System.Windows.Automation.AutomationElement]::FromHandle($hwnd))) }


function Btn($text)    { return ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*BUTTON*" -and $_.Text -eq $text -and $_.Visible } | Select-Object -First 1) }
function Pts()         { $t = (F).Points; $n = -1; [void][int]::TryParse($t, [ref]$n); return $n }
$NO_VALUE = [string][char]0x2014

Write-Host "=== 1. INITIAL STATE ==="
$e = F
Check "start disabled without a file" (-not (Btn "Start").Enabled)          ""
Check "browse offered instead"  ((Btn "Browse...").Enabled)                    ""
Check "stop disabled"          (-not (Btn "Stop").Enabled)                      ""
Check "pause disabled"         (-not (Btn "Pause").Enabled)                     ""
Check "data points zero"       (($e.Points) -eq "0")          ("'" + ($e.Points) + "'")
Check "no average before any readings" (($e.Average) -eq $NO_VALUE) ("'" + ($e.Average) + "'")
Check "timer zero"             (($e.Timer) -eq "00:00:00") ("'" + ($e.Timer) + "'")
Check "status says what to do first" (((F).Status) -match "Choose a data file") ("'" + ((F).Status) + "'")

Write-Host ""
Write-Host ""
Write-Host "=== 1b. START WITH NO DEVICE (device mode, nothing attached) ==="
# A start that fails must not cost the user the file they chose for it. Skipped if a real TruRNGpro is
# attached, since then the start legitimately succeeds.
$fileND = Join-Path $WorkRoot "ui-nodevice.rng"
if (Test-Path $fileND) { Remove-Item $fileND -Force }
Post-Click (Btn "Browse...").Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $fileND); $m = Wait-Dialog $p.Id 8; if ($m) { [void](Answer-MessageBox $m "OK") } }
Start-Sleep -Seconds 1
Click-Button (Btn "Start").Handle
Start-Sleep -Seconds 3
if (((F).Status) -match "Error initializing") {
    Check "device failure is reported"   ($true)                                     ("'" + ((F).Status) + "'")
    Check "data file survives the failure" (((F).FileName) -eq (Split-Path $fileND -Leaf)) ("'" + ((F).FileName) + "'")
    Check "start still offered to retry" ((Btn "Start").Enabled)                     ""
    Check "no session was left running"  (-not (Btn "Stop").Enabled)                 ""
    Check "browse still available"       ((Btn "Browse...").Enabled)                 ""
    Check "nothing was recorded"         ((Pts) -eq 0)                               ("points=" + (Pts))
} else {
    Write-Host ("  SKIP  a device answered, so the failure path was not exercised: '" + ((F).Status) + "'")
    Click-Button (Btn "Stop").Handle
    Start-Sleep -Seconds 2
}

Write-Host "=== 2. SIMULATE MODE ==="
Click-Button (Btn "").Handle
Start-Sleep -Milliseconds 900
$lbl = ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*STATIC*" -and ($_.Text -eq "PORT" -or $_.Text -eq "SEED") } | Select-Object -First 1)
Check "port field becomes seed"  ((Get-Text $lbl.Handle) -eq "SEED")            ("label='" + (Get-Text $lbl.Handle) + "'")
$seedCombo = ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*COMBOBOX*" })
Check "seed field present"       ($seedCombo.Count -ge 1)                       ("combo boxes=" + $seedCombo.Count)

Write-Host ""
Write-Host "=== 3. RECORD SESSION A ==="
Post-Click (Btn "Browse...").Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $fileA); $m = Wait-Dialog $p.Id 8; if ($m) { [void](Answer-MessageBox $m "OK") } }
Start-Sleep -Seconds 1
Check "file name displayed"    (((F).FileName) -eq (Split-Path $fileA -Leaf)) ("'" + ((F).FileName) + "'")
Check "file in the window title" ((Get-Text $hwnd) -like ((Split-Path $fileA -Leaf) + "*")) ("title='" + (Get-Text $hwnd) + "'")
Check "status confirms new file" (((F).Status) -match "New file selected")             ("'" + ((F).Status) + "'")
Check "start offered once a file is chosen" ((Btn "Start").Enabled)             ""

Click-Button (Btn "Start").Handle
Start-Sleep -Seconds 4
$ptsRun = Pts
$avgRun = (F).Average
Check "data points climbing"   ($ptsRun -gt 0)                                  ("points=" + $ptsRun)
Check "average near 0.5"       ([math]::Abs([double]$avgRun - 0.5) -lt 0.05)    ("average=" + $avgRun)
Check "mean deviation shown"   (((F).MeanDev) -ne "0.000000")        ("=" + ((F).MeanDev))
Check "standard deviation shown" (((F).StdDev) -ne "0.000000")  ("=" + ((F).StdDev))
Check "session timer running"  (((F).Timer) -ne "00:00:00")           ("=" + ((F).Timer))
Check "elapsed time in the window title" ((Get-Text $hwnd) -match "[0-9]{2}:[0-9]{2}:[0-9]{2}") ("title='" + (Get-Text $hwnd) + "'")
Check "status shows running"   (((F).Status) -match "Running|Session")                 ("'" + ((F).Status) + "'")
Check "start disabled"         (-not (Btn "Start").Enabled)                     ""
Check "stop enabled"           ((Btn "Stop").Enabled)                           ""

Write-Host ""
Write-Host "=== 4. Pause AND Resume ==="
Click-Button (Btn "Pause").Handle
Start-Sleep -Seconds 2
$pA = Pts; Start-Sleep -Seconds 3; $pB = Pts
Check "recording halts"        ($pA -eq $pB)                                    ("points " + $pA + " -> " + $pB)
Check "status shows paused"    (((F).Status) -match "Paused")                          ("'" + ((F).Status) + "'")
Check "button reads Resume"    ($null -ne (Btn "Resume"))                       ""
Click-Button (Btn "Resume").Handle
Start-Sleep -Seconds 3
$pC = Pts
Check "recording resumes"      ($pC -gt $pB)                                    ("points " + $pB + " -> " + $pC)
Check "button reads Pause"     ($null -ne (Btn "Pause"))                        ""

Write-Host ""
Write-Host "=== 5. Stop AND VERIFY FILE A ==="
Click-Button (Btn "Stop").Handle
Start-Sleep -Seconds 2
$ptsFinal = Pts
# the file stays chosen when a session ends, so another session can be recorded into it straight away
Check "start still offered after stopping" ((Btn "Start").Enabled)              ""
Check "stop disabled"          (-not (Btn "Stop").Enabled)                      ""
$rawA = [System.IO.File]::ReadAllText($fileA)
$nA = ([regex]::Matches($rawA, '<Data ')).Count
Check "file point count matches display" ($nA -eq $ptsFinal)                    ("file=" + $nA + " display=" + $ptsFinal)
$xml = New-Object System.Xml.XmlDocument
try { $xml.LoadXml($rawA); Check "file well formed and closed" $true "" } catch { Check "file well formed and closed" $false $_.Exception.Message }
Check "recorded as simulated"  ($rawA -match 'Simulated="true"')                ""
$valsA = [regex]::Matches($rawA, '<Data [^>]*>([^<]+)</Data>') | ForEach-Object { [double]$_.Groups[1].Value }
Check "values are bit averages" ((($valsA | Where-Object { $_ -lt 0.3 -or $_ -gt 0.7 }).Count) -eq 0) ("mean=" + [math]::Round((($valsA | Measure-Object -Average).Average),4))

Write-Host ""
Write-Host "=== 6. CLEAR ==="
Post-Click (Btn "Clear").Handle
[void](Confirm-Dialog $p.Id "Yes")
Start-Sleep -Seconds 2
Check "points reset"   (((F).Points) -eq "0")                            ("'" + ((F).Points) + "'")
Check "average reset"  (((F).Average) -eq $NO_VALUE)             ("'" + ((F).Average) + "'")
Check "timer reset"    (((F).Timer) -eq "00:00:00")                   ("'" + ((F).Timer) + "'")

Write-Host ""
Write-Host "=== 7. REOPEN SESSION A AND APPEND ==="
Post-Click (Btn "Browse...").Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $fileA) }
Start-Sleep -Seconds 3
$ptsLoaded = Pts
Check "existing session loaded" ($ptsLoaded -eq $nA)                            "loaded=$ptsLoaded expected=$nA"
Check "status reports the load" (((F).Status) -match "loaded")                         ("'" + ((F).Status) + "'")
Click-Button (Btn "Start").Handle
Start-Sleep -Seconds 3
Click-Button (Btn "Stop").Handle
Start-Sleep -Seconds 2
$rawA2 = [System.IO.File]::ReadAllText($fileA)
$nA2 = ([regex]::Matches($rawA2, '<Data ')).Count
Check "points appended to the file" ($nA2 -gt $nA)                              "$nA -> $nA2"
$xml2 = New-Object System.Xml.XmlDocument
try { $xml2.LoadXml($rawA2); Check "file still well formed after append" $true ("total points=" + $xml2.DocumentElement.ChildNodes.Count) }
catch { Check "file still well formed after append" $false $_.Exception.Message }

Write-Host ""
Write-Host "=== 8. RECORD SESSION B (for comparison) ==="
Post-Click (Btn "Clear").Handle
[void](Confirm-Dialog $p.Id "Yes")
Start-Sleep -Seconds 1
Post-Click (Btn "Browse...").Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $fileB); $m = Wait-Dialog $p.Id 8; if ($m) { [void](Answer-MessageBox $m "OK") } }
Start-Sleep -Seconds 1
Click-Button (Btn "Start").Handle
Start-Sleep -Seconds 4
Click-Button (Btn "Stop").Handle
Start-Sleep -Seconds 2
Check "second session recorded" (Test-Path $fileB)                              ("points=" + ([regex]::Matches([System.IO.File]::ReadAllText($fileB), '<Data ')).Count)

Write-Host ""
Write-Host "=== 9. RESIZING ==="
# The chart is the point of the window, so growing the window has to grow the chart. Every WinForms
# control reports the same window class, so the chart is found by where it sits: it is the largest thing
# below the statistics band, which is the space the layout leaves it.
function ChartRect() {
    $all = @(Get-Controls $hwnd | Where-Object { $_.Visible })
    # The readout tiles sit directly above the chart, so the first of them marks where the chart starts
    $tile = ($all | Where-Object { $_.Text -eq "ELAPSED" } | Select-Object -First 1)
    if ($null -eq $tile) { return $null }
    $below = $tile.Top + $tile.Height
    return ($all | Where-Object { $_.Top -ge $below } |
            Sort-Object { -($_.Width * $_.Height) } | Select-Object -First 1)
}
# Start from a known modest size rather than whatever the last run left remembered, so there is room to
# grow into on any screen
$screen = [System.Windows.Forms.Screen]::PrimaryScreen.WorkingArea
$startWidth  = [math]::Min(800, $screen.Width)
$startHeight = [math]::Min(640, $screen.Height)
[void][W32]::MoveWindow($hwnd, $screen.Left, $screen.Top, $startWidth, $startHeight, $true)
Start-Sleep -Seconds 2

$before = Get-Rect $hwnd
$chartBefore = ChartRect
Check "chart control found"    ($null -ne $chartBefore)                          $(if ($chartBefore) { $chartBefore.Class } else { "none" })
$newWidth  = [math]::Min($startWidth + 220, $screen.Width)
$newHeight = [math]::Min($startHeight + 180, $screen.Height)
[void][W32]::MoveWindow($hwnd, $screen.Left, $screen.Top, $newWidth, $newHeight, $true)
Start-Sleep -Seconds 2
$after = Get-Rect $hwnd
$chartAfter = ChartRect
Check "window resized"         ((($after.R - $after.L) -gt ($before.R - $before.L)) -and (($after.B - $after.T) -gt ($before.B - $before.T))) ("" + ($before.R-$before.L) + "x" + ($before.B-$before.T) + " -> " + ($after.R-$after.L) + "x" + ($after.B-$after.T))
Check "chart grew with it"     (($chartAfter.Width -gt $chartBefore.Width) -and ($chartAfter.Height -gt $chartBefore.Height)) ("" + $chartBefore.Width + "x" + $chartBefore.Height + " -> " + $chartAfter.Width + "x" + $chartAfter.Height)

# below the minimum the window must refuse to shrink any further
[void][W32]::MoveWindow($hwnd, $before.L, $before.T, 300, 200, $true)
Start-Sleep -Seconds 1
$tiny = Get-Rect $hwnd
Check "minimum size honoured"  ((($tiny.R - $tiny.L) -gt 300) -and (($tiny.B - $tiny.T) -gt 200)) ("" + ($tiny.R-$tiny.L) + "x" + ($tiny.B-$tiny.T))

# put it back to a size worth remembering, and record what it actually became rather than what was asked
# for, so the next run is checked against the size the window really had
[void][W32]::MoveWindow($hwnd, $screen.Left, $screen.Top, $newWidth, $newHeight, $true)
Start-Sleep -Seconds 1
$closedAt = Get-Rect $hwnd
$closedWidth = ($closedAt.R - $closedAt.L)
$closedHeight = ($closedAt.B - $closedAt.T)

Write-Host ""
Write-Host "=== 10. CLOSE ==="
$p.CloseMainWindow() | Out-Null
Start-Sleep -Seconds 6
Check "closed cleanly" ($p.HasExited) $(if ($p.HasExited) { "exit code " + $p.ExitCode } else { "still running" })
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }

Write-Host ""
Write-Host "=== 11. WINDOW SIZE REMEMBERED ==="
$p2 = Start-Process $exe -PassThru
$h2 = Get-MainWindow $p2
[void](Wait-ForIdle $h2)
$reopened = Get-Rect $h2
Check "reopens at the size it was closed at" ((($reopened.R - $reopened.L) -eq $closedWidth) -and (($reopened.B - $reopened.T) -eq $closedHeight)) ("closed at " + $closedWidth + "x" + $closedHeight + ", reopened " + ($reopened.R-$reopened.L) + "x" + ($reopened.B-$reopened.T))
$p2.CloseMainWindow() | Out-Null
Start-Sleep -Seconds 6
if (-not $p2.HasExited) { Stop-Process -Id $p2.Id -Force }

Write-Host ""
Write-Host ("RESULT: " + $script:pass + " passed, " + $script:fail + " failed")
