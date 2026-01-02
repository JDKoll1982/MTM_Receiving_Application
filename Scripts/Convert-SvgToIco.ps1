# Convert SVG to ICO using built-in .NET libraries
# This script creates a multi-resolution .ico file from an SVG

param(
    [string]$SvgPath = "Assets\app-icon.svg",
    [string]$IcoPath = "Assets\app-icon.ico"
)

Write-Host "Converting SVG to ICO..."

# Check if ImageMagick is available (preferred method)
$magickPath = Get-Command "magick.exe" -ErrorAction SilentlyContinue

if ($magickPath) {
    Write-Host "Using ImageMagick for conversion..."
    
    # Create multiple sizes and combine into ICO
    $sizes = @(16, 32, 48, 64, 128, 256)
    $tempFiles = @()
    
    foreach ($size in $sizes) {
        $tempPng = "Assets\temp_$size.png"
        & magick.exe convert -background none -density 300 $SvgPath -resize "${size}x${size}" $tempPng
        $tempFiles += $tempPng
    }
    
    # Combine all PNGs into single ICO
    & magick.exe convert $tempFiles $IcoPath
    
    # Clean up temp files
    $tempFiles | ForEach-Object { Remove-Item $_ -ErrorAction SilentlyContinue }
    
    Write-Host "ICO file created successfully: $IcoPath"
}
else {
    Write-Host "ImageMagick not found. Installing via winget..."
    
    # Try to install ImageMagick
    try {
        winget install --id ImageMagick.ImageMagick --silent --accept-source-agreements --accept-package-agreements
        Write-Host "ImageMagick installed. Please run this script again."
        exit 1
    }
    catch {
        Write-Host "Could not install ImageMagick automatically."
        Write-Host "Please install ImageMagick from https://imagemagick.org/script/download.php"
        Write-Host "Or use an online converter: https://convertio.co/svg-ico/"
        exit 1
    }
}
