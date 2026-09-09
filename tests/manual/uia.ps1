#########################################################################################################################
# File Name:      uia.ps1
# Description:    UI Automation helpers for reading the application's fields and status bar
#
# Copyright (c) 2026 Mike Pullen
# Licensed under the MIT License. See LICENSE in the repository root.
#
# Revision History:
#======================================================================================================================
# 2026/09/08 - Mike Pullen - Original implementation.
#########################################################################################################################
# Reading the application's displayed values. UI Automation is used because GetWindowText does not
# retrieve text from another process's non-caption windows.
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

function Get-UiaWindow($processId) {
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $cond = New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $processId)
    for ($i = 0; $i -lt 30; $i++) {
        $w = $root.FindFirst([System.Windows.Automation.TreeScope]::Children, $cond)
        if ($null -ne $w) { return $w }
        Start-Sleep -Milliseconds 500
    }
    return $null
}

# All the displayed values, in tree order
function Get-Values($win) {
    $out = @()
    $all = $win.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($e in $all) {
        if ($e.Current.ControlType.ProgrammaticName -eq "ControlType.Pane") { $out += $e.Current.Name }
    }
    return $out
}

# The status message now lives in a status bar rather than a text box, so it comes back as a Text element
# inside a ToolBar rather than as the widest Pane. It is found by being the lowest thing on the window: the
# bar is docked to the bottom edge, so nothing else sits below it.
function Get-StatusText($win) {
    # Find the status bar itself rather than guessing from position. Guessing picked up whatever else
    # happened to be wide and low on the window, which at one point was the analysis verdict.
    $barCondition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::StatusBar)
    $bar = $win.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $barCondition)
    if ($null -eq $bar) { return "" }

    # The bar carries a severity dot as well as the message. The message is the wide one; the dot is a
    # glyph a few pixels across.
    $best = ""; $bestWidth = 0
    $all = $bar.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($e in $all) {
        $r = $e.Current.BoundingRectangle
        $w = ($r.Right - $r.Left)
        if ($w -gt $bestWidth) { $bestWidth = $w; $best = $e.Current.Name }
    }
    return $best
}

# Every element with its position, so fields can be tied to the label that names them
function Get-Elements($win) {
    $out = @()
    $all = $win.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($e in $all) {
        $r = $e.Current.BoundingRectangle
        $out += [pscustomobject]@{
            Name = $e.Current.Name
            Type = ($e.Current.ControlType.ProgrammaticName -replace 'ControlType\.','')
            L = $r.Left; T = $r.Top; R = $r.Right; B = $r.Bottom
        }
    }
    return $out
}

# Each label sits directly above the field it names, so the field is the closest one below it
function Field-Of($elements, $labelText) {
    $label = ($elements | Where-Object { $_.Type -eq "Text" -and $_.Name -eq $labelText } | Select-Object -First 1)
    if ($null -eq $label) { return $null }
    $lcx = ($label.L + $label.R) / 2
    $best = $null; $bestD = [double]::MaxValue
    foreach ($e in ($elements | Where-Object { $_.Type -eq "Pane" -or $_.Type -eq "ComboBox" })) {
        $gap = $e.T - $label.B
        if ($gap -lt -2 -or $gap -gt 30) { continue }             # directly below the label
        # the label sits over the field it names, which can be much wider than the label itself
        if ($lcx -lt ($e.L - 10) -or $lcx -gt ($e.R + 10)) { continue }
        $dx = [math]::Abs((($e.L + $e.R) / 2) - $lcx)
        $d = $gap + $dx / 100
        if ($d -lt $bestD) { $bestD = $d; $best = $e }
    }
    if ($null -eq $best) { return $null }
    return $best.Name
}

function Read-Fields($win) {
    $e = Get-Elements $win
    return [pscustomobject]@{
        Timer     = (Field-Of $e "ELAPSED")
        Average   = (Field-Of $e "MEAN")
        Points    = (Field-Of $e "READINGS")
        MeanDev   = (Field-Of $e "DEVIATION FROM 0.5")
        StdDev    = (Field-Of $e "STD DEVIATION")
        FileName  = (Field-Of $e "DATA FILE")
        Seed      = (Field-Of $e "SEED")
        Target    = (Field-Of $e "TARGET VALUE")
        Status    = (Get-StatusText $win)
        All       = ($e | Where-Object { $_.Type -eq "Pane" } | ForEach-Object { $_.Name })
    }
}

# The device scan runs when the application starts and takes as long as WMI takes, so wait for it to
# finish rather than guessing at a duration
function Wait-ForIdle($hwnd, $timeoutSec = 60) {
    $deadline = (Get-Date).AddSeconds($timeoutSec)
    while ((Get-Date) -lt $deadline) {
        $w = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
        $status = Get-StatusText $w
        if ($status -and $status -notmatch 'Checking attached devices') { return $status }
        Start-Sleep -Milliseconds 500
    }
    return "<still scanning after $timeoutSec seconds>"
}

# The analysis comparison is one table rather than mirrored fields. Its rows come back as DataItem
# elements whose children hold the measure name and the three values, in column order.
function Read-ComparisonRows($win) {
    $rows = @()
    $all = $win.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($e in $all) {
        if ($e.Current.ControlType.ProgrammaticName -ne "ControlType.DataItem") { continue }
        $cells = @()
        $kids = $e.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.Condition]::TrueCondition)
        foreach ($k in $kids) { $cells += $k.Current.Name }
        # A row with no children of its own still carries the measure name
        if ($cells.Count -eq 0) { $cells = @($e.Current.Name) }
        $rows += ,$cells
    }
    return $rows
}
