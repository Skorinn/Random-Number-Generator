#########################################################################################################################
# File Name:      test-analyze.ps1
# Description:    Drives the Analyse tab, and recovers a session left unterminated by a kill
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

$exe = (Join-Path $env:RNG_BIN "Random Number Generator.exe")
# session A and B were recorded by the execute-tab run
$fileA = Join-Path $WorkRoot "ui-sessionA.rng"
$fileB = Join-Path $WorkRoot "ui-sessionB.rng"
$crash = Join-Path $WorkRoot "ui-crash.rng"
if (Test-Path $crash) { Remove-Item $crash -Force }

Write-Host "=== PART 1: ANALYZE TAB ==="
Check "baseline source file exists" (Test-Path $fileA) ("points=" + $(if (Test-Path $fileA) { ([regex]::Matches([System.IO.File]::ReadAllText($fileA),'<Data ')).Count } else { 0 }))
Check "result source file exists"   (Test-Path $fileB) ("points=" + $(if (Test-Path $fileB) { ([regex]::Matches([System.IO.File]::ReadAllText($fileB),'<Data ')).Count } else { 0 }))

$p = Start-Process $exe -PassThru
$hwnd = Get-MainWindow $p
[void](Wait-ForIdle $hwnd)
function Flds() { return (Read-Fields ([System.Windows.Automation.AutomationElement]::FromHandle($hwnd))) }
function Vals() { return ((Flds).All) }

# switch to the Analyze tab
$tab = ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*SysTabControl32*" } | Select-Object -First 1)
[void](Select-Tab $tab.Handle 1)
Start-Sleep -Seconds 2
$ctrls = Get-Controls $hwnd
$browseButtons = Get-AnalyzeBrowseButtons $hwnd
Check "analyze tab shown"        ($browseButtons.Count -eq 2)                    ("browse buttons=" + $browseButtons.Count)

# the comparison table, as columns of cells read off the rows
function Rows() { return (Get-ComparisonTable $hwnd 4) }
function Column($rows, $index) { return @($rows | ForEach-Object { if ($_.Count -gt $index) { $_[$index] } else { "" } }) }
$NO_VALUE = [string][char]0x2014
$ROWS = 7
# rows 1..5 are the measures that are subtracted from one another; row 0 is a count and row 6 a probability
$MEASURE_ROWS = 1..5
function Cells($column, $rows, $indexes) { return @($indexes | ForEach-Object { (Column $rows $column)[$_] }) }
# the verdict on whether the two sessions differ, which is the question the tab exists to answer
function Verdict() {
    $w = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
    $texts = @(Get-Elements $w | Where-Object { $_.Type -eq "Text" } | ForEach-Object { $_.Name })
    return (@($texts | Where-Object { $_ -match "significant|Load a baseline|Not enough readings" }) -join " ")
}

# the table is there before anything is loaded, with the measures named and no values against them
$rows0 = Rows
Check "comparison table present"  ($rows0.Count -eq $ROWS)                       ("rows=" + $rows0.Count)
Check "measures named"            ((Column $rows0 0) -join '|' -match 'Mean')    ((Column $rows0 0) -join ' | ')
Check "no values before loading"  (@(Column $rows0 1 | Where-Object { $_ -eq $NO_VALUE }).Count -eq $ROWS) ((Column $rows0 1) -join ' | ')
Check "verdict asks for both files" ((Verdict) -match "Load a baseline")         ("verdict: '" + (Verdict) + "'")

# load the baseline (the upper browse button)
Post-Click $browseButtons[0].Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $fileA) }
Start-Sleep -Seconds 3
$v = Vals
$rows1 = Rows
$baseline1 = Column $rows1 1
$result1 = Column $rows1 2
$difference1 = Column $rows1 3
$sBaselineShown = @($v | Where-Object { $_ -like ((Split-Path $fileA -Leaf) + "*") })
Check "baseline file name shown"  ($sBaselineShown.Count -ge 1)                             ("shown as: " + ($sBaselineShown -join " / "))
Check "baseline reading count shown" ($sBaselineShown -join " " -match "[0-9]+ readings")   ("shown as: " + ($sBaselineShown -join " / "))
$nBaselineFile = ([regex]::Matches([System.IO.File]::ReadAllText($fileA),'<Data ')).Count
Check "baseline reading count in the table" ($baseline1[0] -replace ',','' -eq "$nBaselineFile")          ("table says '" + $baseline1[0] + "', file has " + $nBaselineFile)
Check "baseline measures filled"  (@(Cells 1 $rows1 $MEASURE_ROWS | Where-Object { $_ -match '^-?\d+\.\d+$' }).Count -eq 5) ($baseline1 -join ' | ')
Check "baseline tested against 0.5" ($baseline1[6] -match '^(0\.\d+|< 0\.001)$')                          ("p = '" + $baseline1[6] + "'")
Check "result column still empty" (@($result1 | Where-Object { $_ -eq $NO_VALUE }).Count -eq $ROWS)       ($result1 -join ' | ')
Check "no difference from one file" (@($difference1 | Where-Object { $_ -eq $NO_VALUE }).Count -eq $ROWS) ($difference1 -join ' | ')
Check "verdict still asks for both" ((Verdict) -match "Load a baseline")                                  ("verdict: '" + (Verdict) + "'")

# load the result (the lower browse button)
Post-Click $browseButtons[1].Handle
$dlg = Wait-Dialog $p.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $fileB) }
Start-Sleep -Seconds 3
$v2 = Vals
$rows2 = Rows
$result2 = Column $rows2 2
$difference2 = Column $rows2 3
$sResultShown = @($v2 | Where-Object { $_ -like ((Split-Path $fileB -Leaf) + "*") })
Check "result file name shown"    ($sResultShown.Count -ge 1)                                 ("shown as: " + ($sResultShown -join " / "))
Check "result reading count shown" ($sResultShown -join " " -match "[0-9]+ readings")   ("shown as: " + ($sResultShown -join " / "))
Check "result measures filled"    (@(Cells 2 $rows2 $MEASURE_ROWS | Where-Object { $_ -match '^-?\d+\.\d+$' }).Count -eq 5) ($result2 -join ' | ')
Check "result tested against 0.5" ($result2[6] -match '^(0\.\d+|< 0\.001)$')                              ("p = '" + $result2[6] + "'")
Check "differences signed"        (@(Cells 3 $rows2 $MEASURE_ROWS | Where-Object { $_ -match '^[+-]\d+\.\d+$' }).Count -eq 5) ($difference2 -join ' | ')
Check "no difference for a count"  ($difference2[0] -eq $NO_VALUE)                                        ("'" + $difference2[0] + "'")
Check "no difference for a probability" ($difference2[6] -eq $NO_VALUE)                                   ("'" + $difference2[6] + "'")

# the verdict is the answer the tab exists to give, and it has to name a probability rather than just
# assert an outcome
$sVerdict = Verdict
Check "verdict reaches a conclusion" ($sVerdict -match "No significant shift|Significant shift")          ("verdict: '" + $sVerdict + "'")
Check "verdict reports a probability" ($sVerdict -match "chance (0\.\d+|< 0\.001) of the time")           ("verdict: '" + $sVerdict + "'")
Check "verdict names the test"       ($sVerdict -match "Welch")                                           ("verdict: '" + $sVerdict + "'")

# the difference has to be the result less the baseline, not something else that merely looks numeric
$sBaselineMean = (Column $rows2 1)[1]
$bComparable = (($sBaselineMean -match '^-?\d+\.\d+$') -and ($result2[1] -match '^-?\d+\.\d+$') -and ($difference2[1] -match '^[+-]\d+\.\d+$'))
if ($bComparable) {
    $fBaselineMean = [double]$sBaselineMean
    $fResultMean = [double]$result2[1]
    $fShownDifference = [double]$difference2[1]
    # The application subtracts the means it holds, not the six decimals it shows them to, so its answer
    # can differ from this one by up to the last place of each rounded value it was checked against
    $fExpected = $fResultMean - $fBaselineMean
    $fTolerance = 0.0000015
    Check "difference is result minus baseline" ([math]::Abs($fShownDifference - $fExpected) -lt $fTolerance) ("$fResultMean - $fBaselineMean = $fExpected, shown $fShownDifference")
} else {
    Check "difference is result minus baseline" $false ("cells are not numbers: '$sBaselineMean' '" + $result2[1] + "' '" + $difference2[1] + "'")
}

$p.CloseMainWindow() | Out-Null
Start-Sleep -Seconds 5
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force }

Write-Host ""
Write-Host "=== PART 2: CRASH RECOVERY THROUGH THE GUI ==="
# record into a new file, then kill the application while it is still recording
$p2 = Start-Process $exe -PassThru
$h2 = Get-MainWindow $p2
[void](Wait-ForIdle $h2)
function Btn2($t) { return ((Get-Controls $h2) | Where-Object { $_.Class -like "*BUTTON*" -and $_.Text -eq $t -and $_.Visible } | Select-Object -First 1) }
Click-Button (Btn2 "").Handle           # simulate
Start-Sleep -Milliseconds 800
Post-Click (Btn2 "Browse...").Handle
$dlg = Wait-Dialog $p2.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $crash); $m = Wait-Dialog $p2.Id 8; if ($m) { [void](Answer-MessageBox $m "OK") } }
Start-Sleep -Seconds 1
Click-Button (Btn2 "Start").Handle
Start-Sleep -Seconds 5
$sizeBefore = (Get-Item $crash).Length
Stop-Process -Id $p2.Id -Force          # kill it mid-session, as a power loss would
Start-Sleep -Seconds 2
$raw = [System.IO.File]::ReadAllText($crash)
$nCrash = ([regex]::Matches($raw, '<Data ')).Count
Check "file has data after the kill"   ($nCrash -gt 0)                           ("points=" + $nCrash + " bytes=" + $sizeBefore)
Check "session left unterminated"      (-not ($raw -match '</Session>'))         "no closing tag, as expected"

# reopen it in a fresh instance and confirm the recovery is reported
$p3 = Start-Process $exe -PassThru
$h3 = Get-MainWindow $p3
[void](Wait-ForIdle $h3)
function Btn3($t) { return ((Get-Controls $h3) | Where-Object { $_.Class -like "*BUTTON*" -and $_.Text -eq $t -and $_.Visible } | Select-Object -First 1) }
function Flds3()  { return (Read-Fields ([System.Windows.Automation.AutomationElement]::FromHandle($h3))) }
Post-Click (Btn3 "Browse...").Handle
$dlg = Wait-Dialog $p3.Id 15
if ($dlg) { [void](Complete-FileDialog $dlg $crash) }
Start-Sleep -Seconds 4
$f3 = Flds3
$recovered = 0; [void][int]::TryParse($f3.Points, [ref]$recovered)
Check "points recovered into the session" ($recovered -eq $nCrash)               ("recovered=" + $recovered + " expected=" + $nCrash)
Check "recovery reported to the user"  ($f3.Status -match "not closed properly") ("status='" + $f3.Status + "'")

# continue the recovered session and confirm the file ends up valid
Click-Button (Btn3 "Start").Handle
Start-Sleep -Seconds 3
Click-Button (Btn3 "Stop").Handle
Start-Sleep -Seconds 2
$raw2 = [System.IO.File]::ReadAllText($crash)
$n2 = ([regex]::Matches($raw2, '<Data ')).Count
Check "recovered session continued"    ($n2 -gt $nCrash)                         ("$nCrash -> $n2")
$xml = New-Object System.Xml.XmlDocument
try { $xml.LoadXml($raw2); Check "recovered file is valid XML" $true ("points=" + $xml.DocumentElement.ChildNodes.Count) }
catch { Check "recovered file is valid XML" $false $_.Exception.Message }

$p3.CloseMainWindow() | Out-Null
Start-Sleep -Seconds 5
if (-not $p3.HasExited) { Stop-Process -Id $p3.Id -Force }

Write-Host ""
Write-Host ("RESULT: " + $script:pass + " passed, " + $script:fail + " failed")
