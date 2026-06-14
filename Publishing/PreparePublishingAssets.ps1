param(
    [string]$SourceDirectory = (Join-Path $PSScriptRoot '..\..\Afbeelding Ontwikkeling')
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$sourceDirectory = (Resolve-Path -LiteralPath $SourceDirectory).Path
$storeDirectory = Join-Path $PSScriptRoot 'Store'
$githubDirectory = Join-Path $PSScriptRoot 'GitHub'

[System.IO.Directory]::CreateDirectory($storeDirectory) | Out-Null
[System.IO.Directory]::CreateDirectory($githubDirectory) | Out-Null

Copy-Item `
    -LiteralPath (Join-Path $sourceDirectory 'Winget Github Banner.png') `
    -Destination (Join-Path $githubDirectory 'SteamDockExtension-Banner.png') `
    -Force

function Export-Screenshot {
    param(
        [string]$SourcePath,
        [string]$DestinationPath,
        [int]$Width,
        [int]$Height,
        [double]$MaximumScale = 2.5
    )

    $source = [System.Drawing.Bitmap]::FromFile($SourcePath)
    $canvas = [System.Drawing.Bitmap]::new(
        $Width,
        $Height,
        [System.Drawing.Imaging.PixelFormat]::Format24bppRgb)
    $canvas.SetResolution(96, 96)

    $graphics = [System.Drawing.Graphics]::FromImage($canvas)
    try {
        $graphics.Clear([System.Drawing.Color]::FromArgb(12, 8, 28))
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

        $scale = [Math]::Min($Width / $source.Width, $Height / $source.Height)
        $scale = [Math]::Min($scale, $MaximumScale)
        $drawWidth = [int][Math]::Round($source.Width * $scale)
        $drawHeight = [int][Math]::Round($source.Height * $scale)
        $x = [int][Math]::Floor(($Width - $drawWidth) / 2)
        $y = [int][Math]::Floor(($Height - $drawHeight) / 2)

        $graphics.DrawImage($source, $x, $y, $drawWidth, $drawHeight)
        $canvas.Save($DestinationPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $canvas.Dispose()
        $source.Dispose()
    }
}

$screenshots = Join-Path $sourceDirectory 'Screenshots'

Export-Screenshot `
    (Join-Path $screenshots 'Fullscreen Dock and friends.png') `
    (Join-Path $storeDirectory '01-dock-library-and-friends.png') `
    1920 1080

Export-Screenshot `
    (Join-Path $screenshots 'Library Dock.png') `
    (Join-Path $storeDirectory '02-steam-library.png') `
    1920 1080

Export-Screenshot `
    (Join-Path $screenshots 'Settings.png') `
    (Join-Path $storeDirectory '03-extension-settings.png') `
    1920 1080

Export-Screenshot `
    (Join-Path $screenshots 'Friends.png') `
    (Join-Path $storeDirectory '04-steam-chat.png') `
    1080 1920
