<#
.SYNOPSIS
    Stamps a version into the MSI installer project (FleetAudit.Installer.vdproj).

.DESCRIPTION
    In the "Product" section of the .vdproj, sets ProductVersion to the given version and generates a new ProductCode
    and PackageCode. UpgradeCode stays the same: a major upgrade needs a new ProductCode whenever the version changes.

    The version must follow the MSI ProductVersion rules: three numeric fields, major and minor up to 255,
    build up to 65535. Each of the three keys must occur exactly once in the section, otherwise nothing is written.

    The project file is written back as UTF-8 with a BOM, which is how Visual Studio saves it, and keeps its line endings.

.PARAMETER Version
    The version as 'v1.2.3' or '1.2.3', for example a release tag.

.PARAMETER ProjectPath
    The installer project to change.

.EXAMPLE
    ./build/Set-InstallerVersion.ps1 -Version v1.2.3
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,
    [string] $ProjectPath = 'FleetAudit.Installer\FleetAudit.Installer.vdproj'
)

$ErrorActionPreference = 'Stop'

if (-not ($Version.Trim() -match '^[vV]?([0-9]{1,9})\.([0-9]{1,9})\.([0-9]{1,9})$')) {
    throw "Invalid version '$Version': expected three numeric fields such as 'v1.2.3' or '1.2.3'."
}
$major = [int] $Matches[1]
$minor = [int] $Matches[2]
$build = [int] $Matches[3]
if ($major -gt 255 -or $minor -gt 255 -or $build -gt 65535) {
    throw "Invalid version '$Version': the MSI ProductVersion allows major and minor up to 255 and build up to 65535."
}
$productVersion = "$major.$minor.$build"

# .NET file methods resolve relative paths against the process directory, not the PowerShell location.
$path = (Resolve-Path -LiteralPath $ProjectPath).Path
$utf8Bom = New-Object System.Text.UTF8Encoding $true
$text = [System.IO.File]::ReadAllText($path, $utf8Bom)

# The "Product" section holds no nested sections, so it ends at the first line that starts with a closing brace.
$sections = [regex]::Matches($text, '(?ms)^[ \t]*"Product"[ \t]*\r?\n[ \t]*\{[ \t]*\r?\n(.*?)^[ \t]*\}')
if ($sections.Count -ne 1) {
    throw "Expected one `"Product`" section in $path, found $($sections.Count)."
}
$body = $sections[0].Groups[1]
$section = $body.Value

$new = [ordered]@{
    ProductVersion = $productVersion
    ProductCode    = [guid]::NewGuid().ToString('B').ToUpperInvariant()
    PackageCode    = [guid]::NewGuid().ToString('B').ToUpperInvariant()
}
$old = @{}
foreach ($key in $new.Keys) {
    $pattern = '(?m)^([ \t]*"' + $key + '"[ \t]*=[ \t]*")8:([^"\r\n]*)(")'
    $found = [regex]::Matches($section, $pattern)
    if ($found.Count -ne 1) {
        throw "Expected `"$key`" exactly once in the Product section of $path, found $($found.Count)."
    }
    $old[$key] = $found[0].Groups[2].Value
    $section = [regex]::Replace($section, $pattern, '${1}8:' + $new[$key] + '${3}')
}

$text = $text.Substring(0, $body.Index) + $section + $text.Substring($body.Index + $body.Length)
[System.IO.File]::WriteAllText($path, $text, $utf8Bom)

Write-Host "Updated $path"
Write-Host "  ProductVersion: $($old.ProductVersion) -> $($new.ProductVersion)"
Write-Host "  ProductCode:    $($old.ProductCode) -> $($new.ProductCode)"
Write-Host "  PackageCode:    $($old.PackageCode) -> $($new.PackageCode)"
