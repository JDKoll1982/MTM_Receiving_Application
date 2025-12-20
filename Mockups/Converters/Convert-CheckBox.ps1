# Convert-CheckBox.ps1
# Converts XAML CheckBox to Penpot shape

function Convert-CheckBox {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
    
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [int]$X = 0,
    [int]$Y = 0,
    [int]$Width = 150,
    [int]$Height = 20,
    
    [string]$ParentId,
    [string]$FrameId,
    [string]$PageId,
    
    [string]$Content = "CheckBox",
    [bool]$IsChecked = $false
)

$fillColor = if ($IsChecked) { "#0078D4" } else { "#FFFFFF" }

$json = @"
{
  "id": "$Id",
  "name": "$Name",
  "type": "rect",
  "x": $X,
  "y": $Y,
  "width": 20,
  "height": 20,
  "rotation": 0,
  "selrect": {
    "x": $X,
    "y": $Y,
    "width": 20,
    "height": 20,
    "x1": $X,
    "y1": $Y,
    "x2": $($X + 20),
    "y2": $($Y + 20)
  },
  "points": [
    {"x": $X, "y": $Y},
    {"x": $($X + 20), "y": $Y},
    {"x": $($X + 20), "y": $($Y + 20)},
    {"x": $X, "y": $($Y + 20)}
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
  "fills": [{"fillColor": "$fillColor"}],
  "strokes": [{"strokeColor": "#8A8A8A", "strokeWidth": 1}]
}
"@

    return $json
}
