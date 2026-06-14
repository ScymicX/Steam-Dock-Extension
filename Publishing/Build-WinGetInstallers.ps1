param(
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version = '0.1.0',

    [ValidateSet('x64', 'arm64')]
    [string[]]$Platforms = @('x64', 'arm64'),

    [switch]$PublishOnly
)

$ErrorActionPreference = 'Stop'

$repositoryDirectory = Split-Path $PSScriptRoot -Parent
$projectDirectory = Join-Path $repositoryDirectory 'SteamDockExtension'
$projectPath = Join-Path $projectDirectory 'SteamDockExtension.csproj'
$templatePath = Join-Path $projectDirectory 'setup-template.iss'
$artifactDirectory = Join-Path $repositoryDirectory 'artifacts\winget'
$installerDirectory = Join-Path $artifactDirectory 'installers'

$innoSetup = @(
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe')
) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

if (-not $PublishOnly -and -not $innoSetup) {
    throw 'Inno Setup 6 was not found. Install it with: winget install JRSoftware.InnoSetup'
}

[System.IO.Directory]::CreateDirectory($installerDirectory) | Out-Null
$template = Get-Content -LiteralPath $templatePath -Raw

foreach ($platform in $Platforms) {
    $runtime = "win-$platform"
    $publishDirectory = Join-Path $artifactDirectory "publish\$platform"
    $setupPath = Join-Path $artifactDirectory "setup-$platform.iss"

    & dotnet publish $projectPath `
        --configuration Release `
        --runtime $runtime `
        --self-contained true `
        --output $publishDirectory `
        -p:Platform=$(if ($platform -eq 'arm64') { 'ARM64' } else { 'x64' }) `
        -p:WindowsPackageType=None `
        -p:EnableMsixTooling=false `
        -p:GenerateAppxPackageOnBuild=false `
        -p:PublishProfile= `
        -p:PublishSingleFile=false

    if ($LASTEXITCODE -ne 0) {
        throw "The $platform publish failed."
    }

    if ($PublishOnly) {
        continue
    }

    $architecture = if ($platform -eq 'arm64') { 'arm64' } else { 'x64compatible' }
    $setup = $template.
        Replace('__VERSION__', $Version).
        Replace('__PLATFORM__', $platform).
        Replace('__OUTPUT_DIRECTORY__', $installerDirectory).
        Replace('__PUBLISH_DIRECTORY__', $publishDirectory).
        Replace('__ICON_PATH__', (Join-Path $projectDirectory 'Assets\SteamDockExtension.ico')).
        Replace('__ARCHITECTURES_ALLOWED__', $architecture).
        Replace('__ARCHITECTURES_64BIT__', $architecture)

    Set-Content -LiteralPath $setupPath -Value $setup -Encoding utf8
    & $innoSetup $setupPath
    if ($LASTEXITCODE -ne 0) {
        throw "The $platform installer build failed."
    }
}

if (-not $PublishOnly) {
    Get-ChildItem -LiteralPath $installerDirectory -Filter '*.exe' |
        Select-Object Name, Length, FullName
}
