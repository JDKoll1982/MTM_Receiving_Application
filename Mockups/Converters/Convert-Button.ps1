# Convert-Button.ps1
# Converts XAML Button to Penpot rectangle shape

function Convert-Button {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
        
        [Parameter(Mandatory=$true)]
        [string]$Name,
        
        [int]$X = 0,
        [int]$Y = 0,
        [int]$Width = 120,
        [int]$Height = 32,
        
        [string]$ParentId,
        [string]$FrameId,
        [string]$PageId,
        
        [string]$Content = "Button",
        [string]$Style = "Default",
        [string]$Background = ""
    )

# Determine color based on style
$fillColor = switch ($Style) {
    "AccentButtonStyle" { "#0078D4" }
    "Accent" { "#0078D4" }
    default { 
        if ($Background) { $Background } 
        else { "#F3F3F3" }
    }
}

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
  "r1": 4,
  "r2": 4,
  "r3": 4,
  "r4": 4,
  "fills": [{"fillColor": "$fillColor"}],
  "strokes": []
}
"@

    return $json
}
