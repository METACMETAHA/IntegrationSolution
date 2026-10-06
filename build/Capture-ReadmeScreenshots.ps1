<#
.SYNOPSIS
    Captures the English screenshots used in README.md (plus Ukrainian/Russian ones for the localization section).

.DESCRIPTION
    Starts the shell on a clean profile (English), sets a fixed window size, and walks through the screens
    that need no SAP/Wialon data. Each screenshot is cropped to the main window and saved under a fixed name,
    so README links stay stable. Run it with Windows PowerShell 5.1 in an interactive desktop session.

.EXAMPLE
    ./build/Capture-ReadmeScreenshots.ps1 -AppPath IntegrationSolution.ShellGUI\bin\Release\IntegrationSolution.ShellGUI.exe
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $AppPath,
    [string] $OutputDirectory = 'docs\screenshots',
    [string] $ResourceDirectory = 'IntegrationSolution.Localization\Resources',
    [int] $TimeoutSeconds = 60,
    [int] $Width = 1440,
    [int] $Height = 810,
    [switch] $DumpAutomationTree
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'UiAutomation.ps1')

function Set-ScreenResolution([int] $ScreenWidth, [int] $ScreenHeight) {
    try {
        Set-DisplayResolution -Width $ScreenWidth -Height $ScreenHeight -Force
    }
    catch { Write-Warning "Could not change the screen resolution: $($_.Exception.Message)" }
    $screen = [System.Windows.Forms.SystemInformation]::VirtualScreen
    Write-Host "Screen: $($screen.Width)x$($screen.Height)"
}

function Set-WindowBounds {
    $window = $script:Shell.Window
    (Get-Pattern $window ([System.Windows.Automation.WindowPattern])).SetWindowVisualState([System.Windows.Automation.WindowVisualState]::Normal)
    Start-Sleep -Milliseconds 500
    $transform = Get-Pattern $window ([System.Windows.Automation.TransformPattern])
    $transform.Resize($Width, $Height)
    $transform.Move(0, 0)
    [UiAutomationNative]::SetForegroundWindow([IntPtr]$window.Current.NativeWindowHandle) | Out-Null
    Start-Sleep -Seconds 1
    Write-Host "Window: $($window.Current.BoundingRectangle)"
}

# Keeps the mouse away from the window, so no tooltip or hover effect ends up in a screenshot.
function Park-Mouse {
    $screen = [System.Windows.Forms.SystemInformation]::VirtualScreen
    [System.Windows.Forms.Cursor]::Position = New-Object System.Drawing.Point ($screen.Right - 1), ($screen.Bottom - 1)
}

function Save-Shot([string] $Name, [int] $SettleMilliseconds = 1500) {
    Park-Mouse
    Start-Sleep -Milliseconds $SettleMilliseconds # page transitions and animations
    Save-WindowScreenshot $Name
}

# The hamburger menu has two lists: the main items (Home, Operations) and the options items (Help).
function Select-MenuItem([int] $ListIndex, [int] $ItemIndex) {
    $window = $script:Shell.Window
    $item = Wait-For -What "menu list #$ListIndex item #$ItemIndex" -Probe {
        $lists = @(Find-AllByType $window $ControlType::List | Where-Object { $_.Current.AutomationId -like '*ListView*' })
        if ($lists.Count -gt $ListIndex) {
            $items = $lists[$ListIndex].FindAll($TreeScope::Children, (New-PropertyCondition $AutomationElement::ControlTypeProperty $ControlType::ListItem))
            if ($items.Count -gt $ItemIndex) { $items[$ItemIndex] }
        }
    }
    Write-Host "Selecting menu item '$($item.Current.Name)'"
    Select-Element $item
}

function Select-SettingsTab([string] $HeaderKey) {
    $header = Get-Translation 'en' $HeaderKey
    $tab = Wait-For -What "the '$header' tab" -Probe {
        $script:Shell.Window.FindFirst($TreeScope::Descendants, (New-Object System.Windows.Automation.AndCondition(
            (New-PropertyCondition $AutomationElement::ControlTypeProperty $ControlType::TabItem),
            (New-PropertyCondition $AutomationElement::NameProperty $header))))
    }
    Select-Element $tab
}

Remove-Item -LiteralPath $LanguagePreferenceFile -Force -ErrorAction SilentlyContinue

try {
    Set-ScreenResolution 1920 1080
    Start-Shell -KeepWindowState
    Set-WindowBounds
    if ($DumpAutomationTree) { Get-AutomationTreeLines -MaxDepth 30 | ForEach-Object { Write-Host $_ } }
    Assert-Language 'en'

    # The shell opens on the data-analysis wizard.
    Save-Shot 'wizard-loading-files'

    Open-Settings
    Expand-LanguageSelector | Out-Null
    Save-Shot 'settings-language' 1000
    Select-Language 'en' # closes the drop-down without changing anything

    Select-SettingsTab 'Settings_ExcelHeadersTab'
    Save-Shot 'settings-excel-headers'
    Select-SettingsTab 'Settings_GeneralTab'
    Close-Settings

    Select-MenuItem 0 0
    Save-Shot 'home'

    Select-MenuItem 1 0
    Save-Shot 'help'

    # Back to the wizard, then the same screen in the other languages.
    Select-MenuItem 0 1
    foreach ($culture in 'uk', 'ru') {
        Open-Settings
        Select-Language $culture
        Assert-Language $culture
        Close-Settings
        Save-Shot "wizard-loading-files-$culture" 2500
    }

    Open-Settings
    Select-Language 'en'
    Assert-Language 'en'
    Close-Settings
    Stop-Shell
    Remove-Item -LiteralPath $LanguagePreferenceFile -Force -ErrorAction SilentlyContinue

    Write-Host "Screenshots saved to $OutputDirectory"
}
catch {
    Write-Host "Capturing screenshots failed: $($_.Exception.Message)"
    try { Get-AutomationTreeLines -MaxDepth 30 | ForEach-Object { Write-Host $_ } } catch { }
    throw
}
finally {
    Stop-ShellIfRunning
}
