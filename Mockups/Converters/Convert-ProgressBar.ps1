# Convert-ProgressBar.ps1
# Converts XAML ProgressBar to Penpot shapes

function Convert-ProgressBar {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
    
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [int]$X = 0,
    [int]$Y = 0,
    [int]$Width = 300,
    [int]$Height = 4,
    
    [string]$ParentId,
    [string]$FrameId,
    [string]$PageId,
    
    [int]$Value = 0,
    [int]$Maximum = 100
)

# Background track
$json = @"
{
  "id": "$Id",
  "name": "$Name",
  "type": "rect",
  "x": $X,
  "y": $Y,
  "width": $Width,
  "height": $Height,
  "rotation": 0,
  "selrect": {
    "x": $X,
    "y": $Y,
    "width": $Width,
    "height": $Height,
    "x1": $X,
    "y1": $Y,
    "x2": $($X + $Width),
    "y2": $($Y + $Height)
  },
  "points": [
    {"x": $X, "y": $Y},
    {"x": $($X + $Width), "y": $Y},
    {"x": $($X + $Width), "y": $($Y + $Height)},
    {"x": $X, "y": $($Y + $Height)}
  ],
  "transform": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "transformInverse": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "parentId": "$ParentId",
  "frameId": "$FrameId",
  "pageId": "$PageId",
  "r1": 2,
  "r2": 2,
  "r3": 2,
  "r4": 2,
  "fills": [{"fillColor": "#E0E0E0"}],
  "strokes": []
}
"@

    return $json
}
