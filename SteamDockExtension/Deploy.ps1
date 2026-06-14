[CmdletBinding()]
param(
    [string] $ManifestPath,

    [switch] $Prepare,

    [string] $PackageName = 'SteamDockExtension',

    [string] $ProcessName = 'SteamDockExtension'
)

$ErrorActionPreference = 'Stop'

if ($Prepare) {
    $packages = @(Get-AppxPackage -Name $PackageName -ErrorAction SilentlyContinue)
    foreach ($package in $packages) {
        Write-Host "Removing previous development package $($package.PackageFullName)..."
        Remove-AppxPackage -Package $package.PackageFullName -ErrorAction Stop
    }

    $processes = @(Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)
    if ($processes.Count -gt 0) {
        Write-Host "Stopping running $ProcessName instance..."
        $processes | Stop-Process -Force -ErrorAction Stop
        $processes | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue
    }

    return
}

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    throw 'ManifestPath is required when registering the extension.'
}

$manifest = Get-Item -LiteralPath $ManifestPath
[xml] $manifestXml = Get-Content -LiteralPath $manifest.FullName
$PackageName = $manifestXml.Package.Identity.Name

Write-Host "Registering $PackageName from $($manifest.DirectoryName)..."

try {
    Add-AppxPackage `
        -Register $manifest.FullName `
        -ForceApplicationShutdown `
        -ErrorAction Stop
}
catch {
    throw "Deployment failed. Enable Windows Developer Mode and retry. $($_.Exception.Message)"
}

$package = Get-AppxPackage -Name $PackageName -ErrorAction SilentlyContinue
if ($null -eq $package) {
    throw "Windows did not report '$PackageName' as registered after deployment."
}

Write-Host "Deployed $($package.PackageFullName)"
