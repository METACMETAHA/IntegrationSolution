<#
.SYNOPSIS
    End-to-end smoke test of the UI language switching, with a screenshot after every step.

.DESCRIPTION
    1. Starts the shell on a clean profile and checks that it comes up in English (the default).
    2. Switches the language in the Settings flyout to Ukrainian, then Russian, and checks that
       the UI text follows each switch without a restart.
    3. Restarts the shell, checks that the last choice was remembered, and switches back to English.

    Screenshots go to -OutputDirectory, so the UI can be reviewed without a Windows machine.
    On failure the UI Automation tree is dumped next to them (automation-tree.txt).

    Uses Windows UI Automation (UIAutomationClient), which ships with Windows. Run it with
    Windows PowerShell 5.1 in an interactive desktop session; GitHub-hosted Windows runners have one.
    The script is ASCII only on purpose: Windows PowerShell reads BOM-less scripts as ANSI, so expected
    texts are read from the .resx files instead of being written here.

.EXAMPLE
    ./build/Invoke-UiSmokeTest.ps1 -AppPath IntegrationSolution.ShellGUI\bin\Release\IntegrationSolution.ShellGUI.exe
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $AppPath,
    [string] $OutputDirectory = 'artifacts\screenshots',
    [string] $ResourceDirectory = 'IntegrationSolution.Localization\Resources',
    [int] $TimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms, System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

public static class SmokeTestNative
{
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();
}
'@
[SmokeTestNative]::SetProcessDPIAware() | Out-Null

$AppPath = (Resolve-Path $AppPath).Path
$ResourceDirectory = (Resolve-Path $ResourceDirectory).Path
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory = (Resolve-Path $OutputDirectory).Path

$AutomationElement = [System.Windows.Automation.AutomationElement]
$TreeScope = [System.Windows.Automation.TreeScope]

# Order of LocalizationService.SupportedLanguages, i.e. of the items in the language selector.
$LanguageIndex = @{ en = 0; uk = 1; ru = 2 }

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

function Find-ById($Root, [string] $AutomationId) {
    $condition = New-Object System.Windows.Automation.PropertyCondition($AutomationElement::AutomationIdProperty, $AutomationId)
    return $Root.FindFirst($TreeScope::Descendants, $condition)
}

function Save-Screenshot([string] $Label) {
    $script:Step++
    $path = Join-Path $OutputDirectory ('{0:D2}-{1}.png' -f $script:Step, $Label)
    $bounds = [System.Windows.Forms.SystemInformation]::VirtualScreen
    $bitmap = New-Object System.Drawing.Bitmap $bounds.Width, $bounds.Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Screenshot: $path"
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
    }
}

function Write-AutomationTree([string] $Path) {
    $walker = [System.Windows.Automation.TreeWalker]::ControlViewWalker
    $lines = New-Object System.Collections.Generic.List[string]

    function Visit($Element, [int] $Depth) {
        if ($Depth -gt 25 -or $lines.Count -gt 5000) { return }
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
        $condition = New-Object System.Windows.Automation.PropertyCondition($AutomationElement::ProcessIdProperty, $script:Shell.Process.Id)
        foreach ($window in $AutomationElement::RootElement.FindAll($TreeScope::Children, $condition)) {
            Visit $window 0
        }
    }
    [System.IO.File]::WriteAllLines($Path, $lines)
    Write-Host "UI Automation tree: $Path"
}

function Start-Shell {
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

    try {
        $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).SetWindowVisualState([System.Windows.Automation.WindowVisualState]::Maximized)
    }
    catch { Write-Warning "Could not maximize the window: $($_.Exception.Message)" }
    [SmokeTestNative]::SetForegroundWindow([IntPtr]$window.Current.NativeWindowHandle) | Out-Null
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
    $window = $script:Shell.Window
    $selector = Find-ById $window 'LanguageSelector'
    if ($selector -and -not $selector.Current.IsOffscreen) { return $selector }
    return $null
}

function Toggle-Settings {
    $button = Find-ById $script:Shell.Window 'SettingsButton'
    $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
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

function Select-Language([string] $Culture) {
    $index = $LanguageIndex[$Culture]
    $selector = Wait-For -What 'the language selector' -Probe { Find-LanguageSelector }
    $selector.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()

    $listItem = New-Object System.Windows.Automation.PropertyCondition($AutomationElement::ControlTypeProperty, [System.Windows.Automation.ControlType]::ListItem)
    $item = Wait-For -What "language item #$index" -Probe {
        $items = $selector.FindAll($TreeScope::Descendants, $listItem)
        if ($items.Count -gt $index) { $items[$index] }
    }

    Write-Host "Selecting language item #$index '$($item.Current.Name)'"
    $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    try { $selector.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse() }
    catch { } # the drop-down usually closes by itself after a selection
}

try {
    # 1. First start on a clean profile: English by default.
    Start-Shell
    Assert-Language 'en'
    Save-Screenshot 'en-main'
    Open-Settings
    Save-Screenshot 'en-settings'

    # 2. Runtime switching, no restart.
    foreach ($culture in 'uk', 'ru') {
        Select-Language $culture
        Assert-Language $culture
        Save-Screenshot "$culture-settings"
        Close-Settings
        Save-Screenshot "$culture-main"
        if ($culture -ne 'ru') { Open-Settings }
    }
    Stop-Shell

    # 3. The choice survives a restart; switching back to English works too.
    Start-Shell
    Assert-Language 'ru'
    Save-Screenshot 'ru-after-restart'
    Open-Settings
    Select-Language 'en'
    Assert-Language 'en'
    Save-Screenshot 'en-settings-after-switch-back'
    Close-Settings
    Save-Screenshot 'en-main-after-switch-back'
    Stop-Shell

    Write-Host 'UI smoke test passed.'
}
catch {
    Write-Host "UI smoke test failed: $($_.Exception.Message)"
    try { Save-Screenshot 'failure' } catch { Write-Warning "No failure screenshot: $($_.Exception.Message)" }
    try { Write-AutomationTree (Join-Path $OutputDirectory 'automation-tree.txt') } catch { Write-Warning "No automation tree: $($_.Exception.Message)" }
    throw
}
finally {
    if ($script:Shell -and -not $script:Shell.Process.HasExited) {
        $script:Shell.Process.Kill()
    }
}
