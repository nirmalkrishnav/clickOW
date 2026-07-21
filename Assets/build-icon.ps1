#requires -Version 5.1
# Regenerates Assets\tray.ico as a clean, multi-resolution Windows icon.
# Small sizes are stored as 32-bpp BMP (DIB) entries for maximum shell
# compatibility; the 256x256 entry is stored as PNG (the standard for large
# icon frames). Run from the repo root:  powershell -File Assets\build-icon.ps1
Add-Type -AssemblyName System.Drawing

$ringColor = [System.Drawing.Color]::FromArgb(255, 0x3C, 0xA5, 0xFE)
$dotColor  = [System.Drawing.Color]::FromArgb(255, 0xFF, 0x4D, 0x4D)

function New-IconBitmap {
    param([int]$Size)

    $bmp = New-Object System.Drawing.Bitmap($Size, $Size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # Ring: centerline radius = 0.375*S, stroke width = 0.0833*S
    $stroke = [Math]::Max(1.0, $Size * 0.0833)
    $ringR  = $Size * 0.375
    $cx = $Size / 2.0
    $cy = $Size / 2.0
    $pen = New-Object System.Drawing.Pen($ringColor, [float]$stroke)
    $g.DrawEllipse($pen, [float]($cx - $ringR), [float]($cy - $ringR), [float]($ringR * 2), [float]($ringR * 2))

    # Center dot: radius = 0.156*S
    $dotR = $Size * 0.156
    $brush = New-Object System.Drawing.SolidBrush($dotColor)
    $g.FillEllipse($brush, [float]($cx - $dotR), [float]($cy - $dotR), [float]($dotR * 2), [float]($dotR * 2))

    $pen.Dispose(); $brush.Dispose(); $g.Dispose()
    return $bmp
}

# --- Build the ICO container ------------------------------------------------
$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$entries = @()

foreach ($s in $sizes) {
    $bmp = New-IconBitmap -Size $s
    if ($s -ge 256) {
        # PNG-encoded frame
        $ms = New-Object System.IO.MemoryStream
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $data = $ms.ToArray()
        $ms.Dispose()
    }
    else {
        # 32-bpp BMP (DIB) frame: BITMAPINFOHEADER + BGRA pixels (bottom-up) + AND mask
        $w = $s; $h = $s
        $hdr = New-Object System.IO.MemoryStream
        $bw = New-Object System.IO.BinaryWriter($hdr)
        $bw.Write([int]40)          # biSize
        $bw.Write([int]$w)          # biWidth
        $bw.Write([int]($h * 2))    # biHeight (image + mask)
        $bw.Write([int16]1)         # biPlanes
        $bw.Write([int16]32)        # biBitCount
        $bw.Write([int]0)           # biCompression
        $bw.Write([int]0)           # biSizeImage
        $bw.Write([int]0); $bw.Write([int]0)  # ppm x/y
        $bw.Write([int]0); $bw.Write([int]0)  # colors used/important

        # Pixel data, bottom-up
        for ($y = $h - 1; $y -ge 0; $y--) {
            for ($x = 0; $x -lt $w; $x++) {
                $p = $bmp.GetPixel($x, $y)
                $bw.Write([byte]$p.B); $bw.Write([byte]$p.G); $bw.Write([byte]$p.R); $bw.Write([byte]$p.A)
            }
        }
        # AND mask (all zero: alpha channel drives transparency), 32-bit row aligned
        $maskRow = [int][Math]::Floor((($w + 31) / 32)) * 4
        $zeros = New-Object byte[] ($maskRow * $h)
        $bw.Write($zeros)

        $bw.Flush()
        $data = $hdr.ToArray()
        $bw.Dispose(); $hdr.Dispose()
    }
    $entries += [pscustomobject]@{ Size = $s; Data = $data }
    $bmp.Dispose()
}

# ICONDIR + ICONDIRENTRY[] + image data
$out = New-Object System.IO.MemoryStream
$w = New-Object System.IO.BinaryWriter($out)
$w.Write([int16]0)                 # reserved
$w.Write([int16]1)                 # type = icon
$w.Write([int16]$entries.Count)    # count

$offset = 6 + (16 * $entries.Count)
foreach ($e in $entries) {
    $dim = if ($e.Size -ge 256) { 0 } else { $e.Size }
    $w.Write([byte]$dim)           # width
    $w.Write([byte]$dim)           # height
    $w.Write([byte]0)              # color count
    $w.Write([byte]0)              # reserved
    $w.Write([int16]1)             # planes
    $w.Write([int16]32)            # bit count
    $w.Write([int]$e.Data.Length)  # bytes in resource
    $w.Write([int]$offset)         # offset
    $offset += $e.Data.Length
}
foreach ($e in $entries) { $w.Write($e.Data) }
$w.Flush()

$target = Join-Path $PSScriptRoot 'tray.ico'
[System.IO.File]::WriteAllBytes($target, $out.ToArray())
$w.Dispose(); $out.Dispose()

Write-Output ("Wrote {0} ({1} sizes, {2} bytes)" -f $target, $entries.Count, (Get-Item $target).Length)
