# Convert-TextBlock.ps1
# Converts XAML TextBlock to Penpot text shape

function Convert-TextBlock {
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
    
    [string]$Text = "Text",
    [int]$FontSize = 14,
    [string]$FontWeight = "Normal",
    [string]$Foreground = "#000000"
)

# Adjust height based on font size
if ($FontSize -gt 0) {
    $Height = [int]($FontSize * 1.5)
}

# Real Penpot Text Structure
$json = @"
{
  "id": "$Id",
  "name": "$Name",
  "type": "text",
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
  "transform": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "transformInverse": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "parentId": "$ParentId",
  "frameId": "$FrameId",
  "pageId": "$PageId",
  "content": {
    "type": "root",
    "children": [
      {
        "type": "paragraph-set",
        "children": [
          {
            "type": "paragraph",
            "children": [
              {
                "type": "text",
                "text": "$Text",
                "fontFamily": "Roboto",
                "fontWeight": "400",
                "fontSize": "${FontSize}px",
                "fontStyle": "normal",
                "fillColor": "#000000"
              }
            ]
          }
        ]
      }
    ]
  },
  "fills": [{"fillColor": "#000000"}],
  "strokes": []
}
"@

    return $json
}
