# Convert-TextBox.ps1
# Converts XAML TextBox to Penpot rectangle shape

function Convert-TextBox {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
    
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [int]$X = 0,
    [int]$Y = 0,
    [int]$Width = 200,
    [int]$Height = 32,
    
    [string]$ParentId,
    [string]$FrameId,
    [string]$PageId,
    
    [string]$PlaceholderText = "",
    [int]$MaxWidth = 0
)

if ($MaxWidth -gt 0) { $Width = $MaxWidth }

# TextBox = white background with border
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
  "fills": [{"fillColor": "#FFFFFF"}],
  "strokes": [{"strokeColor": "#8A8A8A", "strokeWidth": 1}]
}
"@

    return $json
}
