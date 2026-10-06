<#
.SYNOPSIS
    Installs .NET Framework reference assemblies (targeting packs) that the build machine lacks.

.DESCRIPTION
    Some projects still target .NET Framework 4.0 and 4.6.1. Visual Studio 2022 and the GitHub
    windows-2022 runner image no longer ship those targeting packs, so MSBuild fails with MSB3644.
    This script copies them from Microsoft's official NuGet packages
    (Microsoft.NETFramework.ReferenceAssemblies.*) into the standard Reference Assemblies folder,
    which is exactly what the targeting-pack installer would do. Versions already present are skipped.

    Requires administrator rights (the CI runner has them).

.EXAMPLE
    ./build/Install-TargetingPacks.ps1
#>
[CmdletBinding()]
param(
    [string[]] $Versions = @('v4.0', 'v4.5.2', 'v4.6', 'v4.6.1'),
    [string] $PackageVersion = '1.0.3'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$packageIds = @{
    'v4.0'   = 'net40'
    'v4.5.2' = 'net452'
    'v4.6'   = 'net46'
    'v4.6.1' = 'net461'
}

$referenceRoot = Join-Path ${env:ProgramFiles(x86)} 'Reference Assemblies\Microsoft\Framework\.NETFramework'
$workRoot = Join-Path ([IO.Path]::GetTempPath()) 'targeting-packs'
New-Item -ItemType Directory -Force -Path $workRoot | Out-Null

foreach ($version in $Versions) {
    if (-not $packageIds.ContainsKey($version)) {
        throw "Unknown framework version '$version'. Known: $($packageIds.Keys -join ', ')"
    }

    $target = Join-Path $referenceRoot $version
    if (Test-Path (Join-Path $target 'RedistList\FrameworkList.xml')) {
        Write-Host "$version targeting pack is already installed."
        continue
    }

    $id = "microsoft.netframework.referenceassemblies.$($packageIds[$version])"
    $url = "https://api.nuget.org/v3-flatcontainer/$id/$PackageVersion/$id.$PackageVersion.nupkg"
    # Expand-Archive only accepts the .zip extension.
    $archive = Join-Path $workRoot "$id.$PackageVersion.zip"
    $extracted = Join-Path $workRoot "$id.$PackageVersion"

    Write-Host "Installing $version targeting pack from $url"
    Invoke-WebRequest -Uri $url -OutFile $archive -UseBasicParsing
    Expand-Archive -Path $archive -DestinationPath $extracted -Force

    $source = Join-Path $extracted "build\.NETFramework\$version"
    if (-not (Test-Path (Join-Path $source 'RedistList\FrameworkList.xml'))) {
        throw "Package $id does not contain build\.NETFramework\$version"
    }

    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item -Path (Join-Path $source '*') -Destination $target -Recurse -Force
    Write-Host "$version targeting pack installed to $target"
}
