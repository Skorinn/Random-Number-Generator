#########################################################################################################################
# File Name:      run-all.ps1
# Description:    Runs every suite that can run against whatever hardware is attached right now
#
# Copyright (c) 2026 Mike Pullen
# Licensed under the MIT License. See LICENSE in the repository root.
#
# Revision History:
#======================================================================================================================
# 2026/09/08 - Mike Pullen - Original implementation.
#########################################################################################################################

# The suites that need a person to unplug something are left out of this: they are run by hand. Everything
# else runs, and the ones that need the device say so and skip when it is not there.
param(
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Continue"

$binDir = Join-Path $PSScriptRoot "..\..\bin\$Configuration"
if (-not (Test-Path $binDir)) {
    Write-Host "No $Configuration build at $binDir. Build the solution first."
    exit 1
}
$env:RNG_BIN = (Resolve-Path $binDir).Path
Write-Host ("Driving " + $env:RNG_BIN)

$deviceAttached = (([System.IO.Ports.SerialPort]::GetPortNames()).Count -gt 0)
Write-Host ("Device attached: " + $deviceAttached)
Write-Host ""

# test-analyze reads the two session files test-full records, so the order of these matters
$suites = @(
    "test-full.ps1",
    "test-analyze.ps1",
    "check-chart.ps1",
    "inproc-length.ps1",
    "check-appended.ps1",
    "check-sensitivity.ps1",
    "check-device.ps1",
    "check-reinit.ps1",
    "check-nodevice.ps1"
)

$results = @()
foreach ($suite in $suites) {
    Write-Host ("===== " + $suite + " =====")

    # Each suite runs in its own process. The suites report with Write-Host, which does not come back
    # through the pipeline when they are called directly, and a separate process also gives each one the
    # native device interface to itself: initializing it more than once in a process is what produced a
    # run of read failures that turned out to be the harness rather than the application.
    $output = pwsh -NoProfile -File (Join-Path $PSScriptRoot $suite) 2>&1 |
              Where-Object { $_ -notmatch "bytes from trueRNG|Open failed" }
    $output | ForEach-Object { Write-Host $_ }

    # Each suite ends with either a count or a line saying it was skipped. The lines are looked at one at a
    # time rather than as one string, because the counts share their wording with the per-check lines above
    # them and only the summary line carries digits where the count belongs.
    $verdict = "no result"
    foreach ($line in $output) {
        $text = [string]$line
        if ($text -match "SKIPPED") {
            $verdict = "skipped"
        }
        elseif ($text -match "^PASS\s+(\d+)\s+FAIL\s+(\d+)\s*$") {
            $verdict = "$($Matches[1]) passed, $($Matches[2]) failed"
        }
        elseif ($text -match "^RESULT:\s+(\d+) passed, (\d+) failed") {
            $verdict = "$($Matches[1]) passed, $($Matches[2]) failed"
        }
    }
    $results += [pscustomobject]@{ Suite = $suite; Result = $verdict }
    Write-Host ""
}

Write-Host "========================= SUMMARY ========================="
$results | Format-Table -AutoSize | Out-String -Width 100 | Write-Host

$failed = @($results | Where-Object { $_.Result -match ", ([1-9]\d*) failed" })
if ($failed.Count -gt 0) {
    Write-Host ("Suites with failures: " + (($failed | ForEach-Object { $_.Suite }) -join ", "))
    exit 1
}

if (-not $deviceAttached) {
    Write-Host "The device suites were skipped. Attach the device and run again to cover the device path."
}
Write-Host "The suites needing a cable pulled by hand are not run here: check-unplug.ps1, and soak-device.ps1 for a long run."
exit 0
