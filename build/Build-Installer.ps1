<#
.SYNOPSIS
    Builds the MSI installer (FleetAudit.Installer.vdproj) with Visual Studio.

.DESCRIPTION
    MSBuild skips .vdproj projects (MSB4078), so the installer is built with devenv.com, which needs the
    "Microsoft Visual Studio Installer Projects 2022" extension. Build FleetAudit.sln with MSBuild first:
    devenv then finds the project outputs up to date and only packages them.

.EXAMPLE
    ./build/Build-Installer.ps1 -Configuration Release
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [string] $LogPath = 'artifacts\logs\installer-build.log'
)

$ErrorActionPreference = 'Stop'

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vs = & $vswhere -latest -products * -property installationPath | Select-Object -First 1
if (-not $vs) { throw 'Visual Studio was not found.' }
Write-Host "Visual Studio: $vs"

# Command-line builds of .vdproj projects fail with HRESULT 8000000A unless out-of-process builds are disabled.
$disableOutOfProcBuild = Join-Path $vs 'Common7\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe'
if (-not (Test-Path $disableOutOfProcBuild)) {
    throw 'The Microsoft Visual Studio Installer Projects extension is not installed.'
}
Push-Location (Split-Path $disableOutOfProcBuild)
try { & $disableOutOfProcBuild } finally { Pop-Location }

New-Item -ItemType Directory -Force -Path (Split-Path $LogPath) | Out-Null
Remove-Item -LiteralPath $LogPath -ErrorAction SilentlyContinue

$devenv = Join-Path $vs 'Common7\IDE\devenv.com'
& $devenv FleetAudit.sln /Build "$Configuration|Any CPU" /Project 'FleetAudit.Installer\FleetAudit.Installer.vdproj' /Out $LogPath
$exitCode = $LASTEXITCODE

# devenv can exit with 0 although a project failed, so the summary line is checked too.
$summary = Select-String -Path $LogPath -Pattern 'Build: \d+ succeeded, (\d+) failed' | Select-Object -Last 1
$failed = if ($summary) { [int] $summary.Matches[0].Groups[1].Value } else { -1 }
$msi = "FleetAudit.Installer\$Configuration\FleetAudit.Installer.msi"
if ($exitCode -ne 0 -or $failed -ne 0 -or -not (Test-Path $msi)) {
    throw "The installer build failed (devenv exit code $exitCode, failed projects $failed); see $LogPath."
}
Write-Host "Built $msi ($([math]::Round((Get-Item $msi).Length / 1MB, 1)) MB)"
