<#
.SYNOPSIS
    Installs the MSI silently, checks what it installed, runs the UI smoke test against the installed app and uninstalls it.

.DESCRIPTION
    1. msiexec /i /qn, then finds the installed FleetAudit.App.exe under Program Files.
    2. Every .exe, .dll, .exe.config and data file of the MSBuild output (-ReferenceDirectory) must be installed,
       and the installer must add its Desktop and Start menu shortcuts and an Apps & Features entry.
    3. Runs Invoke-UiSmokeTest.ps1 against the installed app.
    4. msiexec /x /qn, then checks that the files, the shortcuts and the Apps & Features entry are gone.

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

function Get-MsiProperty([string] $Name) {
    $installer = New-Object -ComObject WindowsInstaller.Installer
    $database = $installer.GetType().InvokeMember('OpenDatabase', 'InvokeMethod', $null, $installer, @($MsiPath, 0))
    $view = $database.GetType().InvokeMember('OpenView', 'InvokeMethod', $null, $database, @("SELECT Value FROM Property WHERE Property = '$Name'"))
    [void] $view.GetType().InvokeMember('Execute', 'InvokeMethod', $null, $view, $null)
    $record = $view.GetType().InvokeMember('Fetch', 'InvokeMethod', $null, $view, $null)
    $value = $record.GetType().InvokeMember('StringData', 'GetProperty', $null, $record, @(1))
    [void] $view.GetType().InvokeMember('Close', 'InvokeMethod', $null, $view, $null)
    # Release the database so that msiexec can open the package.
    foreach ($object in $record, $view, $database, $installer) { [void] [Runtime.InteropServices.Marshal]::ReleaseComObject($object) }
    $value
}

# The Apps & Features entry of the package, or $null.
function Get-UninstallEntry {
    'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall', 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall' |
        ForEach-Object { Join-Path $_ $productCode } |
        Where-Object { Test-Path $_ } |
        ForEach-Object { Get-ItemProperty $_ } |
        Select-Object -First 1
}

function Find-InstalledApp {
    Get-ChildItem -Path ${env:ProgramFiles(x86)}, $env:ProgramFiles -Filter 'FleetAudit.App.exe' -Recurse -Depth 3 -ErrorAction SilentlyContinue |
        Select-Object -First 1
}

# The Desktop and Start menu shortcuts named after the product.
function Get-Shortcuts {
    'Desktop', 'CommonDesktopDirectory', 'Programs', 'CommonPrograms' |
        ForEach-Object { Get-ChildItem -LiteralPath ([Environment]::GetFolderPath($_)) -Filter '*.lnk' -ErrorAction SilentlyContinue } |
        Where-Object { $_.BaseName -eq $productName } |
        ForEach-Object { $_.FullName }
}

# Files the app needs at run time, relative to its folder: what MSBuild put next to the exe, minus build-only files.
function Get-RuntimeFiles([string] $Directory) {
    Get-ChildItem -Path $Directory -File -Recurse |
        Where-Object { $_.Extension -in '.exe', '.dll', '.config', '.pdf', '.xlsx' -and $_.Name -notlike '*.dll.config' } |
        ForEach-Object { $_.FullName.Substring($Directory.Length).TrimStart('\') } |
        Sort-Object
}

$productCode = Get-MsiProperty 'ProductCode'
$productName = Get-MsiProperty 'ProductName'
Write-Host "Testing $productName $(Get-MsiProperty 'ProductVersion') $productCode"

if (Find-InstalledApp) { throw 'FleetAudit.App.exe is already installed; uninstall it first.' }
$shortcutsBefore = @(Get-Shortcuts)

Invoke-Msiexec '/i' 'install.log'
try {
    $app = Find-InstalledApp
    if (-not $app) { throw 'FleetAudit.App.exe was not found under Program Files after the installation.' }
    $installDirectory = $app.DirectoryName
    Write-Host "Installed to $installDirectory"

    $missing = @(Get-RuntimeFiles $ReferenceDirectory | Where-Object { -not (Test-Path -LiteralPath (Join-Path $installDirectory $_)) })
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

    $entry = Get-UninstallEntry
    if (-not $entry) { throw "Apps & Features has no entry for $productCode." }
    Write-Host "Apps & Features: $($entry.DisplayName) $($entry.DisplayVersion), $($entry.Publisher)"

    & (Join-Path $PSScriptRoot 'Invoke-UiSmokeTest.ps1') -AppPath $app.FullName -OutputDirectory (Join-Path $OutputDirectory 'screenshots')
}
finally {
    Invoke-Msiexec '/x' 'uninstall.log'
}

if (Find-InstalledApp) { throw 'FleetAudit.App.exe is still installed after the uninstallation.' }
$leftShortcuts = @(Get-Shortcuts | Where-Object { $_ -notin $shortcutsBefore })
if ($leftShortcuts) { throw "Shortcuts left after the uninstallation: $($leftShortcuts -join ', ')" }
if (Get-UninstallEntry) { throw "Apps & Features still lists $productCode after the uninstallation." }
Write-Host 'Installer test passed.'
