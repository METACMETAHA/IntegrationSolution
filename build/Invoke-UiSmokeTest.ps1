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

    Run it with Windows PowerShell 5.1 in an interactive desktop session; GitHub-hosted Windows runners have one.

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
. (Join-Path $PSScriptRoot 'UiAutomation.ps1')

# A clean profile: no saved language.
Remove-Item -LiteralPath $LanguagePreferenceFile -Force -ErrorAction SilentlyContinue

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
    Stop-ShellIfRunning
}
