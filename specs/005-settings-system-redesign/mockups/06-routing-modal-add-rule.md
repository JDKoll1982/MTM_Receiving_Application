# Routing - Add/Edit Routing Rule

**SVG File**: `06-routing-modal-add-rule.svg`
**Parent Page**: Routing Settings
**Type**: ContentDialog
**Purpose**: Create or modify automatic routing rules

---

## WinUI 3 Implementation

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Dialogs.RoutingRuleDialog"
    Title="Add Routing Rule"
    PrimaryButtonText="Save"
    CloseButtonText="Cancel"
    DefaultButton="Primary">

    <StackPanel Spacing="16" MinWidth="400">
        <!-- Match Type -->
        <ComboBox
            Header="Match Type"
            SelectedItem="{x:Bind RoutingRule.MatchType, Mode=TwoWay}"
            Width="360">
            <ComboBoxItem Content="Part Number" IsSelected="True"/>
            <ComboBoxItem Content="Vendor"/>
            <ComboBoxItem Content="PO Type"/>
            <ComboBoxItem Content="Part Category"/>
        </ComboBox>

        <!-- Pattern -->
        <TextBox
            Header="Pattern"
            Text="{x:Bind RoutingRule.Pattern, Mode=TwoWay}"
            PlaceholderText="e.g., VOL-*, VENDOR-123, etc."
            Width="360">
            <TextBox.Description>
                <TextBlock Text="Use * for wildcard matching"/>
            </TextBox.Description>
        </TextBox>

        <!-- Destination -->
        <ComboBox
            Header="Destination Location"
            ItemsSource="{x:Bind Locations}"
            SelectedItem="{x:Bind RoutingRule.DestinationLocation, Mode=TwoWay}"
            DisplayMemberPath="Name"
            Width="360"/>

        <!-- Priority -->
        <NumberBox
            Header="Priority"
            Value="{x:Bind RoutingRule.Priority, Mode=TwoWay}"
            Minimum="1"
            Maximum="100"
            SpinButtonPlacementMode="Inline"
            Width="150">
            <NumberBox.Description>
                <TextBlock Text="Lower numbers have higher priority"/>
            </NumberBox.Description>
        </NumberBox>
    </StackPanel>
</ContentDialog>
```

---

## Model

```csharp
public partial class Model_RoutingRule : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _matchType = "Part Number";

    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private string _destinationLocation = string.Empty;

    [ObservableProperty]
    private int _priority = 1;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;
}
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task AddRoutingRuleAsync()
{
    var dialog = new RoutingRuleDialog(Locations)
    {
        XamlRoot = _xamlRoot
    };

    var result = await dialog.ShowAsync();

    if (result == ContentDialogResult.Primary)
    {
        var saveResult = await _routingService.SaveRoutingRuleAsync(dialog.RoutingRule);

        if (saveResult.IsSuccess)
        {
            await LoadRoutingRulesAsync();
            StatusMessage = $"Routing rule for '{dialog.RoutingRule.Pattern}' added";
        }
    }
}
```

---

## Pattern Matching Logic

```csharp
public class RoutingRuleMatcher
{
    public static bool Matches(string pattern, string value)
    {
        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(value, regexPattern, RegexOptions.IgnoreCase);
    }

    public static string GetDestination(List<Model_RoutingRule> rules, string partNumber)
    {
        // Sort by priority (lower = higher priority)
        var sortedRules = rules
            .Where(r => r.IsActive)
            .OrderBy(r => r.Priority)
            .ToList();

        foreach (var rule in sortedRules)
        {
            if (rule.MatchType == "Part Number" && Matches(rule.Pattern, partNumber))
            {
                return rule.DestinationLocation;
            }
        }

        return null; // No match, use fallback
    }
}
```

---

## Examples

| Match Type | Pattern | Matches |
|------------|---------|---------|
| Part Number | `VOL-*` | VOL-12345, VOL-ABC |
| Part Number | `*-BOLT` | M8-BOLT, 10MM-BOLT |
| Vendor | `VENDOR-123` | Exact match only |
| PO Type | `STOCK` | STOCK POs |

---

## Database Schema

```sql
CREATE TABLE routing_home_locations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    match_type VARCHAR(50) NOT NULL,
    pattern VARCHAR(100) NOT NULL,
    destination_location VARCHAR(100) NOT NULL,
    priority INT NOT NULL DEFAULT 1,
    is_active BOOLEAN DEFAULT 1,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY unique_pattern (match_type, pattern)
);
```

---

## Validation

```csharp
private bool ValidateRoutingRule()
{
    if (string.IsNullOrWhiteSpace(RoutingRule.Pattern))
    {
        ShowError("Pattern is required");
        return false;
    }

    if (string.IsNullOrWhiteSpace(RoutingRule.DestinationLocation))
    {
        ShowError("Destination location is required");
        return false;
    }

    if (RoutingRule.Priority < 1 || RoutingRule.Priority > 100)
    {
        ShowError("Priority must be between 1 and 100");
        return false;
    }

    return true;
}
```

---

## References

- [Regex Wildcard Patterns](https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference)
