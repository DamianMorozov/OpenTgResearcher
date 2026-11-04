$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetDirectory = Join-Path $PSScriptRoot '../../Presentation/OpenTgResearcherDesktop/Assets'
$frames = [Collections.Generic.List[byte[]]]::new()
$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)

# Рисуем ту же геометрию, что у общего XAML-знака: круг 80, рамка 2 и буквы Segoe UI Semibold 28
foreach ($size in $sizes) {
    $bitmap = [Drawing.Bitmap]::new($size * 4, $size * 4)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $background = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#202020'))
    $foreground = [Drawing.SolidBrush]::new([Drawing.Color]::White)
    $border = [Drawing.Pen]::new([Drawing.ColorTranslator]::FromHtml('#60CDFF'), 2)
    $font = [Drawing.Font]::new('Segoe UI Semibold', 28, [Drawing.FontStyle]::Regular, [Drawing.GraphicsUnit]::Pixel)
    $format = [Drawing.StringFormat]::new()
    $stream = [IO.MemoryStream]::new()
    try {
        $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.TextRenderingHint = [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $graphics.Clear([Drawing.Color]::Transparent)
        $graphics.ScaleTransform($size * 4 / 80, $size * 4 / 80)
        $graphics.FillEllipse($background, 1, 1, 78, 78)
        $graphics.DrawEllipse($border, 1, 1, 78, 78)
        $format.Alignment = [Drawing.StringAlignment]::Center
        $format.LineAlignment = [Drawing.StringAlignment]::Center
        $graphics.DrawString('OTR', $font, $foreground, [Drawing.RectangleF]::new(2, 2, 76, 76), $format)
        $resized = [Drawing.Bitmap]::new($size, $size)
        $output = [Drawing.Graphics]::FromImage($resized)
        try {
            $output.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $output.DrawImage($bitmap, 0, 0, $size, $size)
            $resized.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
            $frames.Add($stream.ToArray())
            if ($size -eq 256) {
                $manifestIcon = [Drawing.Bitmap]::new($resized, 44, 44)
                try { $manifestIcon.Save((Join-Path $assetDirectory 'Otr.png'), [Drawing.Imaging.ImageFormat]::Png) }
                finally { $manifestIcon.Dispose() }
            }
        }
        finally { $output.Dispose(); $resized.Dispose() }
    }
    finally {
        $stream.Dispose(); $format.Dispose(); $font.Dispose(); $border.Dispose()
        $foreground.Dispose(); $background.Dispose(); $graphics.Dispose(); $bitmap.Dispose()
    }
}

# Сохраняем размеры отдельно, чтобы Windows выбрала чёткий значок для масштаба панели задач
$file = [IO.File]::Create((Join-Path $assetDirectory 'Otr.ico'))
$writer = [IO.BinaryWriter]::new($file)
try {
    $writer.Write([ushort]0); $writer.Write([ushort]1); $writer.Write([ushort]$frames.Count)
    $offset = 6 + 16 * $frames.Count
    for ($index = 0; $index -lt $frames.Count; $index++) {
        $dimension = if ($sizes[$index] -eq 256) { 0 } else { $sizes[$index] }
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([ushort]1); $writer.Write([ushort]32)
        $writer.Write([uint]$frames[$index].Length); $writer.Write([uint]$offset)
        $offset += $frames[$index].Length
    }
    foreach ($frame in $frames) { $writer.Write($frame) }
}
finally { $writer.Dispose(); $file.Dispose() }
Write-Output 'Созданы Otr.ico (9 размеров) и Otr.png (44x44)'
