<#
.SYNOPSIS
    Installs the MSI silently, checks what it installed, runs the UI smoke test against the installed app and uninstalls it.

.DESCRIPTION
    1. msiexec /i /qn, then finds the installed FleetAudit.App.exe under Program Files.
    2. Every .exe, .dll, .exe.config and data file of the MSBuild output (-ReferenceDirectory) must be installed,
       and the installer must add its Desktop and Start menu shortcuts.
    3. Runs Invoke-UiSmokeTest.ps1 against the installed app.
    4. msiexec /x /qn, then checks that the files and shortcuts are gone.

    The msiexec logs go to -OutputDirectory. Needs administrator rights (the MSI installs per machine);
    GitHub-hosted Windows runners have them. Run it with Windows PowerShell 5.1, like the smoke test.

.EXAMPLE
    ./build/Test-Installer.ps1 -MsiPath FleetAudit.Installer\Release\FleetAudit.Installer.msi
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $MsiPath,
    [string] $ReferenceDirectory = 'FleetAudit.App\bin\Release',
    [string] $OutputDirectory = 'artifacts\installer'
)

$ErrorActionPreference = 'Stop'

$MsiPath = (Resolve-Path $MsiPath).Path
$ReferenceDirectory = (Resolve-Path $ReferenceDirectory).Path
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$OutputDirectory = (Resolve-Path $OutputDirectory).Path

function Invoke-Msiexec([string] $Action, [string] $LogName) {
    $log = Join-Path $OutputDirectory $LogName
    $process = Start-Process msiexec.exe -ArgumentList $Action, "`"$MsiPath`"", '/qn', '/norestart', '/l*v', "`"$log`"" -Wait -PassThru
    if ($process.ExitCode -notin 0, 3010) {
        throw "msiexec $Action failed with exit code $($process.ExitCode); see $log."
    }
    Write-Host "msiexec $Action succeeded."
}

function Find-InstalledApp {
    Get-ChildItem -Path ${env:ProgramFiles(x86)}, $env:ProgramFiles -Filter 'FleetAudit.App.exe' -Recurse -Depth 3 -ErrorAction SilentlyContinue |
        Select-Object -First 1
}

function Get-Shortcuts {
    'Desktop', 'CommonDesktopDirectory', 'Programs', 'CommonPrograms' |
        ForEach-Object { Get-ChildItem -Path ([Environment]::GetFolderPath($_)) -Filter '*.lnk' -ErrorAction SilentlyContinue } |
        ForEach-Object { $_.FullName }
}

# Files the app needs at run time, relative to its folder: what MSBuild put next to the exe, minus build-only files.
function Get-RuntimeFiles([string] $Directory) {
    Get-ChildItem -Path $Directory -File -Recurse |
        Where-Object { $_.Extension -in '.exe', '.dll', '.config', '.pdf', '.xlsx' -and $_.Name -notlike '*.dll.config' } |
        ForEach-Object { $_.FullName.Substring($Directory.Length).TrimStart('\') } |
        Sort-Object
}

if (Find-InstalledApp) { throw 'FleetAudit.App.exe is already installed; uninstall it first.' }
$shortcutsBefore = @(Get-Shortcuts)

Invoke-Msiexec '/i' 'install.log'
try {
    $app = Find-InstalledApp
    if (-not $app) { throw 'FleetAudit.App.exe was not found under Program Files after the installation.' }
    $installDirectory = $app.DirectoryName
    Write-Host "Installed to $installDirectory"

    $missing = @(Get-RuntimeFiles $ReferenceDirectory | Where-Object { -not (Test-Path (Join-Path $installDirectory $_)) })
    Get-ChildItem -Path $installDirectory -File -Recurse |
        ForEach-Object { $_.FullName.Substring($installDirectory.Length).TrimStart('\') } |
        Sort-Object |
        Set-Content -Path (Join-Path $OutputDirectory 'installed-files.txt')
    if ($missing) {
        throw "The installer does not install $($missing.Count) file(s) of the build output:`n  $($missing -join "`n  ")"
    }
    Write-Host 'All run-time files of the build output are installed.'

    $newShortcuts = @(Get-Shortcuts | Where-Object { $_ -notin $shortcutsBefore })
    if ($newShortcuts.Count -lt 2) { throw "Expected Desktop and Start menu shortcuts, found: $($newShortcuts -join ', ')" }
    Write-Host "Shortcuts: $($newShortcuts.Count)"

    & (Join-Path $PSScriptRoot 'Invoke-UiSmokeTest.ps1') -AppPath $app.FullName -OutputDirectory (Join-Path $OutputDirectory 'screenshots')
}
finally {
    Invoke-Msiexec '/x' 'uninstall.log'
}

if (Find-InstalledApp) { throw 'FleetAudit.App.exe is still installed after the uninstallation.' }
$leftShortcuts = @(Get-Shortcuts | Where-Object { $_ -notin $shortcutsBefore })
if ($leftShortcuts) { throw "Shortcuts left after the uninstallation: $($leftShortcuts -join ', ')" }
Write-Host 'Installer test passed.'
