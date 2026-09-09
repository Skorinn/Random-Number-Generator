#########################################################################################################################
# File Name:      win32b.ps1
# Description:    Win32 helpers for driving the application's controls without physical input
#
# Copyright (c) 2026 Mike Pullen
# Licensed under the MIT License. See LICENSE in the repository root.
#
# Revision History:
#======================================================================================================================
# 2026/09/08 - Mike Pullen - Original implementation.
#########################################################################################################################
# Win32 helpers for driving the application's controls directly, without physical input
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public class W32 {
    public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr h, EnumProc cb, IntPtr p);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr p);
    [DllImport("user32.dll")] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowTextW(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern int GetWindowTextLengthW(IntPtr h);
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr SendMessageW(IntPtr h, uint msg, IntPtr w, string l);
    [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, uint msg, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h, uint msg, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] public static extern bool IsWindowEnabled(IntPtr h);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint acc, bool inherit, uint pid);
    [DllImport("kernel32.dll")] public static extern IntPtr VirtualAllocEx(IntPtr p, IntPtr addr, uint size, uint type, uint prot);
    [DllImport("kernel32.dll")] public static extern bool VirtualFreeEx(IntPtr p, IntPtr addr, uint size, uint type);
    [DllImport("kernel32.dll")] public static extern bool ReadProcessMemory(IntPtr p, IntPtr addr, byte[] buf, uint size, out UIntPtr read);
    [DllImport("kernel32.dll")] public static extern bool WriteProcessMemory(IntPtr p, IntPtr addr, byte[] buf, uint size, out UIntPtr written);
    [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr h, int x, int y, int w, int hgt, bool repaint);
    // Memory in the other process is reserved and committed in one call. Committing alone is not enough
    // when the address is left to Windows to choose: the region has to be reserved as well, or the
    // allocation can fail and the read comes back empty from a window that was perfectly fine.
    const uint MEM_RESERVE_COMMIT = 0x1000 | 0x2000;
    const uint PAGE_READWRITE = 0x04;

    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }

    public static System.Collections.Generic.List<IntPtr> Children(IntPtr parent) {
        var list = new System.Collections.Generic.List<IntPtr>();
        EnumChildWindows(parent, (h, p) => { list.Add(h); return true; }, IntPtr.Zero);
        return list;
    }
    public static System.Collections.Generic.List<IntPtr> TopLevel() {
        var list = new System.Collections.Generic.List<IntPtr>();
        EnumWindows((h, p) => { list.Add(h); return true; }, IntPtr.Zero);
        return list;
    }
    public static string ClassOf(IntPtr h) { var sb = new StringBuilder(256); GetClassName(h, sb, 256); return sb.ToString(); }
    public static string TextOf(IntPtr h) { int n = GetWindowTextLengthW(h); var sb = new StringBuilder(n + 2); GetWindowTextW(h, sb, n + 2); return sb.ToString(); }

    // Reads a control's text out of another process. GetWindowText does not send WM_GETTEXT across
    // processes for non-caption windows, so it has to be sent explicitly with a buffer in that process.
    public static string CrossProcessText(IntPtr h) {
        uint pid; GetWindowThreadProcessId(h, out pid);
        IntPtr proc = OpenProcess(0x0008 | 0x0010 | 0x0020, false, pid);
        if (proc == IntPtr.Zero) return null;
        int len = (int)SendMessage(h, 0x000E, IntPtr.Zero, IntPtr.Zero);   // WM_GETTEXTLENGTH
        int bytes = (len + 1) * 2;
        IntPtr mem = VirtualAllocEx(proc, IntPtr.Zero, (uint)bytes, MEM_RESERVE_COMMIT, PAGE_READWRITE);
        if (mem == IntPtr.Zero) { CloseHandle(proc); return null; }
        SendMessage(h, 0x000D, (IntPtr)(len + 1), mem);                    // WM_GETTEXT
        byte[] buf = new byte[bytes]; UIntPtr read;
        ReadProcessMemory(proc, mem, buf, (uint)bytes, out read);
        VirtualFreeEx(proc, mem, 0, 0x8000);
        CloseHandle(proc);
        int end = Array.IndexOf(Encoding.Unicode.GetString(buf).ToCharArray(), '\0');
        string s = Encoding.Unicode.GetString(buf);
        if (end >= 0) { s = s.Substring(0, end); }
        return s;
    }

    // Reads a list view's cells out of the target process. The comparison table does not come back through
    // UI Automation at all, so the rows are asked for with LVM_GETITEMTEXT, which needs both the request
    // structure and the buffer it writes into to live in the process that owns the control.
    public static string[][] ListViewCells(IntPtr lv, int columns) {
        int rows = (int)SendMessage(lv, 0x1004, IntPtr.Zero, IntPtr.Zero);      // LVM_GETITEMCOUNT
        if (rows <= 0) return new string[0][];

        uint pid; GetWindowThreadProcessId(lv, out pid);
        IntPtr proc = OpenProcess(0x0008 | 0x0010 | 0x0020, false, pid);
        if (proc == IntPtr.Zero) return new string[0][];

        const int ITEM_SIZE = 88;          // sizeof(LVITEMW) on x64
        const int TEXT_CHARS = 512;
        int total = ITEM_SIZE + (TEXT_CHARS * 2);
        IntPtr mem = VirtualAllocEx(proc, IntPtr.Zero, (uint)total, MEM_RESERVE_COMMIT, PAGE_READWRITE);
        if (mem == IntPtr.Zero) { CloseHandle(proc); return new string[0][]; }

        var result = new string[rows][];
        byte[] item = new byte[ITEM_SIZE];
        byte[] text = new byte[TEXT_CHARS * 2];
        UIntPtr moved;
        for (int r = 0; r < rows; r++) {
            result[r] = new string[columns];
            for (int c = 0; c < columns; c++) {
                Array.Clear(item, 0, ITEM_SIZE);
                BitConverter.GetBytes(c).CopyTo(item, 8);                        // iSubItem
                BitConverter.GetBytes((long)(mem.ToInt64() + ITEM_SIZE)).CopyTo(item, 24);  // pszText
                BitConverter.GetBytes(TEXT_CHARS).CopyTo(item, 32);              // cchTextMax
                WriteProcessMemory(proc, mem, item, (uint)ITEM_SIZE, out moved);
                SendMessage(lv, 0x1073, (IntPtr)r, mem);                         // LVM_GETITEMTEXTW
                ReadProcessMemory(proc, (IntPtr)(mem.ToInt64() + ITEM_SIZE), text, (uint)text.Length, out moved);
                string s = Encoding.Unicode.GetString(text);
                int end = s.IndexOf('\0');
                result[r][c] = (end >= 0) ? s.Substring(0, end) : s;
            }
        }
        VirtualFreeEx(proc, mem, 0, 0x8000);
        CloseHandle(proc);
        return result;
    }

    // Reads a tab header rectangle out of the target process
    public static int[] TabItemRect(IntPtr tab, int index) {
        uint pid; GetWindowThreadProcessId(tab, out pid);
        IntPtr proc = OpenProcess(0x0008 | 0x0010 | 0x0020, false, pid);   // VM_OPERATION | VM_READ | VM_WRITE
        if (proc == IntPtr.Zero) return null;
        IntPtr mem = VirtualAllocEx(proc, IntPtr.Zero, 16, MEM_RESERVE_COMMIT, PAGE_READWRITE);
        if (mem == IntPtr.Zero) { CloseHandle(proc); return null; }
        SendMessage(tab, 0x130A, (IntPtr)index, mem);                       // TCM_GETITEMRECT
        byte[] buf = new byte[16]; UIntPtr read;
        ReadProcessMemory(proc, mem, buf, 16, out read);
        VirtualFreeEx(proc, mem, 0, 0x8000);                                // RELEASE
        CloseHandle(proc);
        return new int[] { BitConverter.ToInt32(buf,0), BitConverter.ToInt32(buf,4), BitConverter.ToInt32(buf,8), BitConverter.ToInt32(buf,12) };
    }
}
'@

function Get-MainWindow($proc) {
    for ($i = 0; $i -lt 40; $i++) {
        $proc.Refresh()
        if ($proc.MainWindowHandle -ne [IntPtr]::Zero) { return $proc.MainWindowHandle }
        Start-Sleep -Milliseconds 500
    }
    return [IntPtr]::Zero
}

function Get-Controls($hwnd) {
    $out = @()
    foreach ($h in [W32]::Children($hwnd)) {
        # Screen position, so controls that now share a caption can still be told apart
        $r = New-Object W32+RECT
        [void][W32]::GetWindowRect($h, [ref]$r)
        # Buttons now carry access keys, so the ampersand that marks one is dropped from Text and the
        # original kept beside it. Everything matching on a caption keeps working.
        $raw = [W32]::TextOf($h)
        $out += [pscustomobject]@{
            Handle  = $h
            Class   = ([W32]::ClassOf($h) -replace 'WindowsForms10\.','' -replace '\.app.*','')
            Text    = ($raw -replace '&','')
            RawText = $raw
            Visible = [W32]::IsWindowVisible($h)
            Enabled = [W32]::IsWindowEnabled($h)
            Left    = $r.L
            Top     = $r.T
            Width   = ($r.R - $r.L)
            Height  = ($r.B - $r.T)
        }
    }
    return $out
}

# The two browse buttons on the analyze tab, baseline first then result. They and the one on the execute
# tab all read "Browse...", so they are picked out by being the ones on screen while the analyze tab is
# showing, ordered down the window rather than by enumeration order.
function Get-AnalyzeBrowseButtons($hwnd) {
    return @(Get-Controls $hwnd |
             Where-Object { $_.Class -like "*BUTTON*" -and $_.Text -eq "Browse..." -and $_.Visible } |
             Sort-Object Top)
}

# Answers a confirmation put up by a button press, if one appears. Clear now asks before discarding.
function Confirm-Dialog($procId, $buttonText = "Yes", $timeoutSec = 5) {
    $dlg = Wait-Dialog $procId $timeoutSec
    if ($dlg) {
        [void](Answer-MessageBox $dlg $buttonText)
        Start-Sleep -Milliseconds 500
        return $true
    }
    return $false
}

function Click-Button($h) {
    [void][W32]::SendMessage($h, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero)   # BM_CLICK
    Start-Sleep -Milliseconds 500
}

function Set-Text($h, $text) {
    [void][W32]::SendMessageW($h, 0x000C, [IntPtr]::Zero, $text)           # WM_SETTEXT
    Start-Sleep -Milliseconds 200
}

function Get-Text($h) {
    $t = [W32]::CrossProcessText($h)
    if ([string]::IsNullOrEmpty($t)) { return [W32]::TextOf($h) }
    return $t
}

function Click-Point($h, $x, $y) {
    $lp = [IntPtr](($y -shl 16) -bor ($x -band 0xFFFF))
    [void][W32]::PostMessage($h, 0x0201, [IntPtr]1, $lp)                   # WM_LBUTTONDOWN
    Start-Sleep -Milliseconds 80
    [void][W32]::PostMessage($h, 0x0202, [IntPtr]0, $lp)                   # WM_LBUTTONUP
    Start-Sleep -Milliseconds 600
}

function Select-Tab($tabHandle, $index) {
    $r = [W32]::TabItemRect($tabHandle, $index)
    if ($null -eq $r) { return $false }
    $cx = [int](($r[0] + $r[2]) / 2)
    $cy = [int](($r[1] + $r[3]) / 2)
    Click-Point $tabHandle $cx $cy
    return $true
}

function Post-Click($h) {
    # BM_CLICK posted rather than sent, so a modal dialog does not block the caller
    [void][W32]::PostMessage($h, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero)
}

function Wait-Dialog($procId, $timeoutSec = 15) {
    $deadline = (Get-Date).AddSeconds($timeoutSec)
    while ((Get-Date) -lt $deadline) {
        foreach ($h in [W32]::TopLevel()) {
            if (-not [W32]::IsWindowVisible($h)) { continue }
            if ([W32]::ClassOf($h) -ne "#32770") { continue }
            $wpid = 0
            [void][W32]::GetWindowThreadProcessId($h, [ref]$wpid)
            if ($wpid -eq $procId) { return $h }
        }
        Start-Sleep -Milliseconds 300
    }
    return [IntPtr]::Zero
}

function Complete-FileDialog($dlg, $path) {
    # The file name edit sits inside the dialog's combo box; fall back to any edit
    $kids = Get-Controls $dlg
    $edit = ($kids | Where-Object { $_.Class -eq "Edit" -and $_.Visible } | Select-Object -First 1)
    if ($null -eq $edit) { return $false }
    Set-Text $edit.Handle $path
    Start-Sleep -Milliseconds 300
    $btn = ($kids | Where-Object { $_.Class -eq "Button" -and ($_.Text -replace '&','') -in @("Save","Open") } | Select-Object -First 1)
    if ($null -eq $btn) { return $false }
    Click-Button $btn.Handle
    Start-Sleep -Milliseconds 800
    return $true
}

function Wait-Message($procId, $timeoutSec = 20) {
    # Waits for any MessageBox belonging to the process and returns its handle
    return (Wait-Dialog $procId $timeoutSec)
}

function Answer-MessageBox($dlg, $buttonText) {
    $kids = Get-Controls $dlg
    $btn = ($kids | Where-Object { $_.Class -eq "Button" -and ($_.Text -replace '&','') -eq $buttonText } | Select-Object -First 1)
    if ($null -eq $btn) { return $false }
    Click-Button $btn.Handle
    Start-Sleep -Milliseconds 600
    return $true
}

function Get-Rect($h) {
    $r = New-Object W32+RECT
    [void][W32]::GetWindowRect($h, [ref]$r)
    return $r
}

# Finds the edit box that belongs to a label, by choosing the nearest one on screen
function Find-FieldFor($controls, $labelText) {
    $label = ($controls | Where-Object { $_.Class -like "*STATIC*" -and $_.Text -eq $labelText } | Select-Object -First 1)
    if ($null -eq $label) { return $null }
    $lr = Get-Rect $label.Handle
    $lcx = ($lr.L + $lr.R) / 2; $lcy = ($lr.T + $lr.B) / 2
    $best = $null; $bestD = [double]::MaxValue
    foreach ($e in ($controls | Where-Object { $_.Class -like "*EDIT*" -or $_.Class -like "*COMBOBOX*" })) {
        $r = Get-Rect $e.Handle
        $cx = ($r.L + $r.R) / 2; $cy = ($r.T + $r.B) / 2
        $d = [math]::Sqrt([math]::Pow($cx - $lcx, 2) + [math]::Pow(($cy - $lcy) * 4, 2))   # weight vertical distance
        if ($d -lt $bestD) { $bestD = $d; $best = $e }
    }
    return $best
}

# The comparison table on the analyze tab, as rows of cells. It is a list view rather than a set of
# fields, and it does not come back through UI Automation, so it is read over Win32.
function Get-ComparisonTable($hwnd, $columns = 4) {
    $lv = ((Get-Controls $hwnd) | Where-Object { $_.Class -like "*SysListView32*" -and $_.Visible } | Select-Object -First 1)
    if ($null -eq $lv) { return @() }
    return [W32]::ListViewCells($lv.Handle, $columns)
}
