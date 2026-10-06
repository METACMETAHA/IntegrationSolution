<#
.SYNOPSIS
    Windows UI Automation helpers for driving IntegrationSolution.ShellGUI (dot-source this file).

.DESCRIPTION
    Shared by Invoke-UiSmokeTest.ps1 and Capture-ReadmeScreenshots.ps1. Before dot-sourcing, the caller
    defines $AppPath, $OutputDirectory, $ResourceDirectory and $TimeoutSeconds.

    ASCII only on purpose: Windows PowerShell reads BOM-less scripts as ANSI, so expected texts are read
    from the .resx files instead of being written here.
#>

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms, System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class UiAutomationNative
{
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();
}
'@
[UiAutomationNative]::SetProcessDPIAware() | Out-Null

$AppPath = (Resolve-Path $AppPath).Path
$ResourceDirectory = (Resolve-Path $ResourceDirectory).Path
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory = (Resolve-Path $OutputDirectory).Path

$AutomationElement = [System.Windows.Automation.AutomationElement]
$TreeScope = [System.Windows.Automation.TreeScope]
$ControlType = [System.Windows.Automation.ControlType]

# Order of LocalizationService.SupportedLanguages, i.e. of the items in the language selector.
$LanguageIndex = @{ en = 0; uk = 1; ru = 2 }

# Where FileLanguagePreferenceStore keeps the chosen language.
$LanguagePreferenceFile = Join-Path $env:LOCALAPPDATA 'IntegrationSolution\ui-language.txt'

$script:Step = 0
$script:Shell = $null

function Get-Translation([string] $Culture, [string] $Key) {
    $file = if ($Culture -eq 'en') { 'Strings.resx' } else { "Strings.$Culture.resx" }
    $document = New-Object System.Xml.XmlDocument
    $document.Load((Join-Path $ResourceDirectory $file))
    $node = $document.SelectSingleNode("/root/data[@name='$Key']/value")
    if (-not $node) { throw "Key '$Key' is missing from $file" }
    return $node.InnerText
}

# Polls $Probe until it returns something. UI Automation exceptions while the UI is still changing count as "not yet".
function Wait-For([string] $What, [scriptblock] $Probe, [int] $Seconds = $TimeoutSeconds) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    do {
        $result = $null
        try { $result = & $Probe }
        catch [System.Windows.Automation.ElementNotAvailableException] { }
        catch [System.InvalidOperationException] { }
        if ($result) { return $result }
        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)
    throw "Timed out after $Seconds s waiting for: $What"
}

function New-PropertyCondition($Property, $Value) {
    return New-Object System.Windows.Automation.PropertyCondition($Property, $Value)
}

function Find-ById($Root, [string] $AutomationId) {
    return $Root.FindFirst($TreeScope::Descendants, (New-PropertyCondition $AutomationElement::AutomationIdProperty $AutomationId))
}

function Find-AllByType($Root, $Type) {
    return $Root.FindAll($TreeScope::Descendants, (New-PropertyCondition $AutomationElement::ControlTypeProperty $Type))
}

function Get-Pattern($Element, $Pattern) {
    return $Element.GetCurrentPattern($Pattern::Pattern)
}

function Select-Element($Element) {
    (Get-Pattern $Element ([System.Windows.Automation.SelectionItemPattern])).Select()
}

function Invoke-Element($Element) {
    (Get-Pattern $Element ([System.Windows.Automation.InvokePattern])).Invoke()
}

function Save-Bitmap([System.Drawing.Rectangle] $Bounds, [string] $Path) {
    $bitmap = New-Object System.Drawing.Bitmap $Bounds.Width, $Bounds.Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($Bounds.Location, [System.Drawing.Point]::Empty, $Bounds.Size)
        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Screenshot: $Path"
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

# Full-screen screenshot, numbered in the order taken.
function Save-Screenshot([string] $Label) {
    $script:Step++
    Save-Bitmap ([System.Windows.Forms.SystemInformation]::VirtualScreen) (Join-Path $OutputDirectory ('{0:D2}-{1}.png' -f $script:Step, $Label))
}

# Screenshot of the main window only (including open drop-downs and flyouts inside it).
function Save-WindowScreenshot([string] $Name) {
    $rect = $script:Shell.Window.Current.BoundingRectangle
    $screen = [System.Windows.Forms.SystemInformation]::VirtualScreen
    $left = [Math]::Max([int]$rect.Left, $screen.Left)
    $top = [Math]::Max([int]$rect.Top, $screen.Top)
    $right = [Math]::Min([int]$rect.Right, $screen.Right)
    $bottom = [Math]::Min([int]$rect.Bottom, $screen.Bottom)
    Save-Bitmap (New-Object System.Drawing.Rectangle $left, $top, ($right - $left), ($bottom - $top)) (Join-Path $OutputDirectory "$Name.png")
}

function Get-AutomationTreeLines([int] $MaxDepth = 25) {
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $lines = New-Object System.Collections.Generic.List[string]

    function Visit($Element, [int] $Depth) {
        if ($Depth -gt $MaxDepth -or $lines.Count -gt 5000) { return }
        try {
            $info = $Element.Current
            $lines.Add(('{0}{1} name="{2}" id="{3}" class="{4}" offscreen={5}' -f ('  ' * $Depth), $info.ControlType.ProgrammaticName, $info.Name, $info.AutomationId, $info.ClassName, $info.IsOffscreen))
            $child = $walker.GetFirstChild($Element)
            while ($child) {
                Visit $child ($Depth + 1)
                $child = $walker.GetNextSibling($child)
            }
        }
        catch { $lines.Add(('  ' * $Depth) + "<error: $($_.Exception.Message)>") }
    }

    if ($script:Shell -and -not $script:Shell.Process.HasExited) {
        $condition = New-PropertyCondition $AutomationElement::ProcessIdProperty $script:Shell.Process.Id
        foreach ($window in $AutomationElement::RootElement.FindAll($TreeScope::Children, $condition)) {
            Visit $window 0
        }
    }
    return $lines
}

function Write-AutomationTree([string] $Path) {
    [System.IO.File]::WriteAllLines($Path, [string[]]@(Get-AutomationTreeLines))
    Write-Host "UI Automation tree: $Path"
}

function Start-Shell([switch] $KeepWindowState) {
    Write-Host "Starting $AppPath"
    $process = Start-Process -FilePath $AppPath -WorkingDirectory (Split-Path $AppPath) -PassThru
    $script:Shell = [pscustomobject]@{ Process = $process; Window = $null }

    $window = Wait-For -What 'the main window with the Settings button' -Probe {
        if ($process.HasExited) { throw "The application exited during startup with code $($process.ExitCode)." }
        $process.Refresh()
        if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
            $candidate = $AutomationElement::FromHandle($process.MainWindowHandle)
            if (Find-ById $candidate 'SettingsButton') { $candidate }
        }
    }
    $script:Shell.Window = $window

    if (-not $KeepWindowState) {
        try {
            (Get-Pattern $window ([System.Windows.Automation.WindowPattern])).SetWindowVisualState([System.Windows.Automation.WindowVisualState]::Maximized)
        }
        catch { Write-Warning "Could not maximize the window: $($_.Exception.Message)" }
    }
    [UiAutomationNative]::SetForegroundWindow([IntPtr]$window.Current.NativeWindowHandle) | Out-Null
    Start-Sleep -Seconds 2
}

function Stop-Shell {
    $process = $script:Shell.Process
    if (-not $process.HasExited) {
        $process.CloseMainWindow() | Out-Null
        if (-not $process.WaitForExit(20000)) {
            Write-Warning 'The application did not close in time; killing it.'
            $process.Kill()
            $process.WaitForExit()
        }
    }
    Write-Host "The application exited with code $($process.ExitCode)."
    $script:Shell = $null
}

function Stop-ShellIfRunning {
    if ($script:Shell -and -not $script:Shell.Process.HasExited) {
        $script:Shell.Process.Kill()
    }
}

function Assert-Language([string] $Culture) {
    $expected = Get-Translation $Culture 'Shell_SettingsButton'
    $window = $script:Shell.Window
    Wait-For -What "Settings button text '$expected' ($Culture)" -Seconds 15 -Probe {
        $button = Find-ById $window 'SettingsButton'
        if ($button -and $button.Current.Name -eq $expected) { $button }
    } | Out-Null
    Write-Host "OK: the UI is in '$Culture' (Settings button reads '$expected')."
}

function Find-LanguageSelector {
    $selector = Find-ById $script:Shell.Window 'LanguageSelector'
    if ($selector -and -not $selector.Current.IsOffscreen) { return $selector }
    return $null
}

function Toggle-Settings {
    Invoke-Element (Find-ById $script:Shell.Window 'SettingsButton')
}

function Open-Settings {
    Toggle-Settings
    Wait-For -What 'the language selector in the Settings flyout' -Probe { Find-LanguageSelector } | Out-Null
    Start-Sleep -Seconds 1 # flyout animation
}

function Close-Settings {
    Toggle-Settings
    try {
        Wait-For -What 'the Settings flyout to close' -Seconds 5 -Probe { -not (Find-LanguageSelector) } | Out-Null
    }
    catch { Write-Warning $_.Exception.Message }
    Start-Sleep -Seconds 1 # flyout animation
}

function Expand-LanguageSelector {
    $selector = Wait-For -What 'the language selector' -Probe { Find-LanguageSelector }
    (Get-Pattern $selector ([System.Windows.Automation.ExpandCollapsePattern])).Expand()
    return $selector
}

function Select-Language([string] $Culture) {
    $index = $LanguageIndex[$Culture]
    $selector = Expand-LanguageSelector

    $item = Wait-For -What "language item #$index" -Probe {
        $items = Find-AllByType $selector $ControlType::ListItem
        if ($items.Count -gt $index) { $items[$index] }
    }

    Write-Host "Selecting language item #$index '$($item.Current.Name)'"
    Select-Element $item
    try { (Get-Pattern $selector ([System.Windows.Automation.ExpandCollapsePattern])).Collapse() }
    catch { } # the drop-down usually closes by itself after a selection
}
