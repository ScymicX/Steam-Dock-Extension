param(
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$Version = '0.1.0.0'
)

$ErrorActionPreference = 'Stop'

$repositoryDirectory = Split-Path $PSScriptRoot -Parent
$projectPath = Join-Path $repositoryDirectory 'SteamDockExtension\SteamDockExtension.csproj'
$artifactDirectory = Join-Path $repositoryDirectory 'artifacts\store'
$packageDirectory = Join-Path $artifactDirectory 'packages'
$bundleInputDirectory = Join-Path $artifactDirectory 'bundle-input'

if (Test-Path -LiteralPath $artifactDirectory) {
    $resolvedRepository = (Resolve-Path -LiteralPath $repositoryDirectory).Path
    $resolvedArtifacts = [System.IO.Path]::GetFullPath($artifactDirectory)
    if (-not $resolvedArtifacts.StartsWith($resolvedRepository, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clear output outside the repository: $resolvedArtifacts"
    }

    Remove-Item -LiteralPath $artifactDirectory -Recurse -Force
}

[System.IO.Directory]::CreateDirectory($packageDirectory) | Out-Null
[System.IO.Directory]::CreateDirectory($bundleInputDirectory) | Out-Null

foreach ($architecture in @(
    @{ Platform = 'x64'; Runtime = 'win-x64' },
    @{ Platform = 'ARM64'; Runtime = 'win-arm64' }
)) {
    $architectureOutput = Join-Path $packageDirectory $architecture.Platform
    [System.IO.Directory]::CreateDirectory($architectureOutput) | Out-Null

    & dotnet build $projectPath `
        --configuration Release `
        --runtime $architecture.Runtime `
        -p:Platform=$($architecture.Platform) `
        -p:GenerateAppxPackageOnBuild=true `
        -p:AppxPackageSigningEnabled=false `
        -p:AppxPackageVersion=$Version `
        -p:AppxPackageDir="$architectureOutput\"

    if ($LASTEXITCODE -ne 0) {
        throw "The $($architecture.Platform) package build failed."
    }

    $packages = @(Get-ChildItem -LiteralPath $architectureOutput -Recurse -Filter '*.msix')
    if ($packages.Count -ne 1) {
        throw "Expected one $($architecture.Platform) MSIX package, found $($packages.Count)."
    }

    Copy-Item -LiteralPath $packages[0].FullName -Destination $bundleInputDirectory
}

$windowsKitsDirectory = Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
$makeAppx = Get-ChildItem -LiteralPath $windowsKitsDirectory -Recurse -Filter 'makeappx.exe' |
    Where-Object { $_.Directory.Name -eq 'x64' } |
    Sort-Object { [Version]$_.Directory.Parent.Name } -Descending |
    Select-Object -First 1

if ($null -eq $makeAppx) {
    throw 'makeappx.exe was not found. Install the Windows SDK.'
}

$bundlePath = Join-Path $artifactDirectory "SteamDockExtension_$($Version)_neutral.msixbundle"
& $makeAppx.FullName bundle /d $bundleInputDirectory /p $bundlePath /o
if ($LASTEXITCODE -ne 0) {
    throw 'MSIX bundle creation failed.'
}

Write-Host "Store bundle created: $bundlePath"
