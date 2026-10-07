Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$target=Join-Path $root 'src\AulaMarkdown\Assets'
New-Item -ItemType Directory -Path $target -Force | Out-Null
function Color([string]$s) { [System.Drawing.ColorTranslator]::FromHtml($s) }
function Brush([string]$s) { [System.Drawing.SolidBrush]::new((Color $s)) }
function Points($values) { [System.Drawing.PointF[]]($values | ForEach-Object { [System.Drawing.PointF]::new($_[0],$_[1]) }) }
$pngs=@()
foreach($size in @(16,24,32,48,64,128,256)) {
 $bmp=[System.Drawing.Bitmap]::new($size,$size)
 $g=[System.Drawing.Graphics]::FromImage($bmp)
 $g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
 $g.ScaleTransform(($size/256.0),($size/256.0))
 $shape=[System.Drawing.Drawing2D.GraphicsPath]::new()
 $shape.AddArc(8,8,104,104,180,90);$shape.AddArc(144,8,104,104,270,90);$shape.AddArc(144,144,104,104,0,90);$shape.AddArc(8,144,104,104,90,90);$shape.CloseFigure()
 $blue=Brush '#1769c2';$cream=Brush '#fffaf0';$teal=Brush '#a6ede3';$coral=Brush '#ee765d'
 $g.FillPath($blue,$shape)
 $g.FillPolygon($cream,(Points @(@(38,55),@(119,69),@(119,204),@(38,189))))
 $g.FillPolygon($teal,(Points @(@(137,69),@(218,55),@(218,189),@(137,204))))
 $pen=[System.Drawing.Pen]::new((Color '#1769c2'),10);$pen.StartCap='Round';$pen.EndCap='Round'
 $g.DrawLine($pen,55,91,101,99);$g.DrawLine($pen,55,119,101,127);$g.DrawLine($pen,55,147,89,153)
 $g.FillPolygon($coral,(Points @(@(169,87),@(195,82),@(195,137),@(182,126),@(169,141))))
 $pen.Color=Color '#16747c';$g.DrawLine($pen,158,161,198,155)
 $stream=[System.IO.MemoryStream]::new();$bmp.Save($stream,[System.Drawing.Imaging.ImageFormat]::Png)
 $pngs+=,@{Size=$size;Bytes=$stream.ToArray()}
 if($size -eq 256){[System.IO.File]::WriteAllBytes((Join-Path $target 'AulaMarkdown.png'),$stream.ToArray())}
 $stream.Dispose();$g.Dispose();$bmp.Dispose();$shape.Dispose();$pen.Dispose();$blue.Dispose();$cream.Dispose();$teal.Dispose();$coral.Dispose()
}
$ico=[System.IO.File]::Create((Join-Path $target 'AulaMarkdown.ico'))
$writer=[System.IO.BinaryWriter]::new($ico)
$writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$pngs.Count)
$offset=6+16*$pngs.Count
foreach($p in $pngs){$dimension=if($p.Size -eq 256){0}else{$p.Size};$writer.Write([byte]$dimension);$writer.Write([byte]$dimension);$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$p.Bytes.Length);$writer.Write([uint32]$offset);$offset+=$p.Bytes.Length}
foreach($p in $pngs){$writer.Write([byte[]]$p.Bytes)}
$writer.Dispose();$ico.Dispose()
