# Convert-FontIcon.ps1
# Converts XAML FontIcon to Penpot circle shape

function Convert-FontIcon {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
    
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [int]$X = 0,
    [int]$Y = 0,
    [int]$FontSize = 16,
    
    [string]$ParentId,
    [string]$FrameId,
    [string]$PageId,
    
    [string]$Glyph = "",
    [string]$Foreground = "#0078D4"
)

# Icon as circle with size based on FontSize
$diameter = $FontSize * 2
$cx = $X + $diameter / 2
$cy = $Y + $diameter / 2

$json = @"
{
  "id": "$Id",
  "name": "$Name",
  "type": "circle",
  "x": $X,
  "y": $Y,
  "width": $diameter,
  "height": $diameter,
  "rotation": 0,
  "selrect": {
    "x": $X,
    "y": $Y,
    "width": $diameter,
    "height": $diameter,
    "x1": $X,
    "y1": $Y,
    "x2": $($X + $diameter),
    "y2": $($Y + $diameter)
  },
  "transform": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "transformInverse": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "parentId": "$ParentId",
  "frameId": "$FrameId",
  "pageId": "$PageId",
  "fills": [{"fillColor": "$Foreground"}],
  "strokes": []
}
"@

    return $json
}
