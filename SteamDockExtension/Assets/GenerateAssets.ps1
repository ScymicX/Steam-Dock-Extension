param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

$source = [System.Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $SourcePath))
$assetsDirectory = $PSScriptRoot
$repositoryDirectory = Split-Path (Split-Path $assetsDirectory -Parent) -Parent
$storePublishingDirectory = Join-Path $repositoryDirectory 'Publishing\Store'
[System.IO.Directory]::CreateDirectory($storePublishingDirectory) | Out-Null

function Export-Png {
    param(
        [string]$Name,
        [int]$Width,
        [int]$Height,
        [double]$LogoScale = 1.0,
        [string]$OutputDirectory = $assetsDirectory
    )

    $bitmap = [System.Drawing.Bitmap]::new(
        $Width,
        $Height,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bitmap.SetResolution(96, 96)

    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality

        $side = [int][Math]::Round([Math]::Min($Width, $Height) * $LogoScale)
        $x = [int][Math]::Floor(($Width - $side) / 2)
        $y = [int][Math]::Floor(($Height - $side) / 2)
        $destination = [System.Drawing.Rectangle]::new($x, $y, $side, $side)

        $graphics.DrawImage(
            $source,
            $destination,
            0,
            0,
            $source.Width,
            $source.Height,
            [System.Drawing.GraphicsUnit]::Pixel)
    }
    finally {
        $graphics.Dispose()
    }

    $path = Join-Path $OutputDirectory $Name
    try {
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $bitmap.Dispose()
    }

    return $path
}

function Export-Ico {
    param(
        [string]$Name,
        [int[]]$Sizes
    )

    $images = foreach ($size in $Sizes) {
        $bitmap = [System.Drawing.Bitmap]::new(
            $size,
            $size,
            [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
            $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.DrawImage($source, 0, 0, $size, $size)
        }
        finally {
            $graphics.Dispose()
        }

        $stream = [System.IO.MemoryStream]::new()
        try {
            $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
            [PSCustomObject]@{
                Size = $size
                Data = $stream.ToArray()
            }
        }
        finally {
            $stream.Dispose()
            $bitmap.Dispose()
        }
    }

    $outputPath = Join-Path $assetsDirectory $Name
    $file = [System.IO.File]::Create($outputPath)
    $writer = [System.IO.BinaryWriter]::new($file)
    try {
        $writer.Write([UInt16]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]$images.Count)

        $offset = 6 + (16 * $images.Count)
        foreach ($image in $images) {
            $dimension = if ($image.Size -ge 256) { 0 } else { $image.Size }
            $writer.Write([Byte]$dimension)
            $writer.Write([Byte]$dimension)
            $writer.Write([Byte]0)
            $writer.Write([Byte]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]32)
            $writer.Write([UInt32]$image.Data.Length)
            $writer.Write([UInt32]$offset)
            $offset += $image.Data.Length
        }

        foreach ($image in $images) {
            $writer.Write($image.Data)
        }
    }
    finally {
        $writer.Dispose()
        $file.Dispose()
    }
}

try {
    Export-Png 'Square44x44Logo.png' 44 44
    Export-Png 'Square44x44Logo.scale-200.png' 88 88
    Export-Png 'SmallTile.png' 71 71
    Export-Png 'SmallTile.scale-200.png' 142 142
    Export-Png 'Square150x150Logo.png' 150 150
    Export-Png 'Square150x150Logo.scale-200.png' 300 300
    Export-Png 'LargeTile.png' 310 310
    Export-Png 'LargeTile.scale-200.png' 620 620
    Export-Png 'Wide310x150Logo.png' 310 150 0.8
    Export-Png 'Wide310x150Logo.scale-200.png' 620 300 0.8
    Export-Png 'SplashScreen.png' 620 300 0.8
    Export-Png 'SplashScreen.scale-200.png' 1240 600 0.8
    Export-Png 'StoreLogo.png' 50 50
    Export-Png 'StoreLogo.scale-200.png' 100 100
    Export-Png 'CommandPaletteIcon.png' 256 256
    Export-Png 'StoreListingTile.png' 300 300 1.0 $storePublishingDirectory
    Export-Png 'LockScreenLogo.scale-200.png' 48 48

    foreach ($size in 16, 24, 32, 48, 256) {
        Export-Png "Square44x44Logo.targetsize-$size`_altform-unplated.png" $size $size
    }

    Export-Ico 'SteamDockExtension.ico' @(16, 24, 32, 48, 64, 128, 256)
}
finally {
    $source.Dispose()
}
