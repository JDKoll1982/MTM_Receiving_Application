# Settings Mode Selection - WinUI 3 Controls Mapping

**SVG File**: `settings-mode-selection.svg`  
**Purpose**: Main settings page showing 9 category cards for navigating to different settings sections

---

## WinUI 3 Implementation Guide

### Page Structure

```xml
<Page>
    <ScrollViewer>
        <Grid Padding="16" RowSpacing="12">
            <!-- Header, Search, Cards Grid -->
        </Grid>
    </ScrollViewer>
</Page>
```

### Control Breakdown

#### 1. **Header Section** (Lines 22-28 in SVG)

**WinUI 3 Controls**:

- `SymbolIcon` - Settings gear icon at top (Symbol="Setting")
- `TextBlock` - "Settings" title
  - Style: `TitleTextBlockStyle` or custom
  - FontSize: 32
  - FontWeight: SemiBold
- `TextBlock` - Subtitle description
  - Style: `BodyTextBlockStyle`
  - FontSize: 14
  - Foreground: Gray

**XAML Example**:

```xml
<StackPanel HorizontalAlignment="Center" Spacing="8">
    <SymbolIcon Symbol="Setting" FontSize="24" 
                Foreground="{ThemeResource AccentFillColorDefaultBrush}"/>
    <TextBlock Text="Settings" 
               Style="{StaticResource TitleTextBlockStyle}"
               TextAlignment="Center"/>
    <TextBlock Text="Configure application preferences and system settings"
               Style="{StaticResource BodyTextBlockStyle}"
               TextAlignment="Center"
               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
</StackPanel>
```

---

#### 2. **Search Bar** (Lines 31-36 in SVG)

**WinUI 3 Control**: `AutoSuggestBox` or `TextBox`

**Recommended**: `AutoSuggestBox` for real-time filtering

**XAML Example**:

```xml
<AutoSuggestBox 
    PlaceholderText="Search settings..."
    Width="500"
    HorizontalAlignment="Center"
    QueryIcon="Find"
    QuerySubmitted="SearchBox_QuerySubmitted"
    TextChanged="SearchBox_TextChanged"/>
```

**Alternative** (Simple TextBox):

```xml
<TextBox 
    PlaceholderText="Search settings..."
    Width="500"
    HorizontalAlignment="Center">
    <TextBox.Resources>
        <Style TargetType="TextBox">
            <Setter Property="CornerRadius" Value="22"/>
        </Style>
    </TextBox.Resources>
</TextBox>
```

---

#### 3. **Category Cards Grid** (Lines 39-146 in SVG)

**WinUI 3 Controls**: `GridView` or `ItemsRepeater` with custom layout

**Recommended**: `ItemsRepeater` with `UniformGridLayout` for modern approach

**Layout**:

- 3 columns × 3 rows
- Card size: 280×180 pixels
- Spacing: 20 pixels between cards
- Total grid: 880×600 pixels

**XAML Example (ItemsRepeater)**:

```xml
<ItemsRepeater ItemsSource="{x:Bind ViewModel.SettingsCategories}">
    <ItemsRepeater.Layout>
        <UniformGridLayout 
            MinItemWidth="280" 
            MinItemHeight="180"
            ItemsStretch="None"
            MinColumnSpacing="20"
            MinRowSpacing="20"/>
    </ItemsRepeater.Layout>
    <ItemsRepeater.ItemTemplate>
        <DataTemplate x:DataType="local:SettingsCategoryModel">
            <!-- Card content below -->
        </DataTemplate>
    </ItemsRepeater.ItemTemplate>
</ItemsRepeater>
```

**XAML Example (GridView Alternative)**:

```xml
<GridView 
    ItemsSource="{x:Bind ViewModel.SettingsCategories}"
    SelectionMode="Single"
    IsItemClickEnabled="True"
    ItemClick="CategoryCard_Click">
    <GridView.ItemsPanel>
        <ItemsPanelTemplate>
            <ItemsWrapGrid 
                Orientation="Horizontal" 
                MaximumRowsOrColumns="3"
                ItemWidth="280"
                ItemHeight="180"/>
        </ItemsPanelTemplate>
    </GridView.ItemsPanel>
</GridView>
```

---

#### 4. **Individual Category Cards** (9 cards total)

**WinUI 3 Controls per card**:

- `Border` - Card container with shadow and rounded corners
- `StackPanel` - Vertical layout for icon, title, description, count
- `FontIcon` or `SymbolIcon` - Category icon
- `TextBlock` (×3) - Title, subtitle, item count
- `Rectangle` - Accent bar at bottom

**Card Structure**:

```xml
<Border 
    Width="280" 
    Height="180" 
    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
    BorderThickness="1"
    CornerRadius="8"
    Shadow="{ThemeResource CardShadow}">
    
    <StackPanel VerticalAlignment="Center" Spacing="8" Padding="16">
        <!-- Icon Circle Background -->
        <Border 
            Width="40" 
            Height="40" 
            CornerRadius="20"
            Background="{x:Bind AccentColor, Converter={StaticResource ColorToOpacityBrush}}"
            HorizontalAlignment="Center">
            <SymbolIcon 
                Symbol="{x:Bind IconSymbol}"
                Foreground="{x:Bind AccentColor}"/>
        </Border>
        
        <!-- Title -->
        <TextBlock 
            Text="{x:Bind Title}"
            Style="{StaticResource SubtitleTextBlockStyle}"
            TextAlignment="Center"
            HorizontalAlignment="Center"/>
        
        <!-- Subtitle -->
        <TextBlock 
            Text="{x:Bind Subtitle}"
            Style="{StaticResource CaptionTextBlockStyle}"
            Foreground="{ThemeResource TextFillColorSecondaryBrush}"
            TextAlignment="Center"
            HorizontalAlignment="Center"/>
        
        <!-- Setting Count -->
        <TextBlock 
            Text="{x:Bind SettingCount, Converter={StaticResource CountToStringConverter}}"
            Style="{StaticResource CaptionTextBlockStyle}"
            Foreground="{ThemeResource TextFillColorTertiaryBrush}"
            TextAlignment="Center"
            HorizontalAlignment="Center"
            Margin="0,8,0,0"/>
        
        <!-- Accent Bar -->
        <Rectangle 
            Width="80" 
            Height="2" 
            Fill="{x:Bind AccentColor}"
            Opacity="0.3"
            HorizontalAlignment="Center"/>
    </StackPanel>
</Border>
```

---

#### 5. **Card Icons** (Category-specific symbols)

| Category | SVG Icon | WinUI 3 Symbol | Alternative |
|----------|----------|----------------|-------------|
| **System Settings** | Horizontal lines | `Symbol="Setting"` | `\uE713` |
| **Security & Session** | Lock | `Symbol="ProtectedDocument"` | `\uE8C0` |
| **ERP Integration** | Database connector | `Symbol="Database"` | `FontIcon Glyph="\uEa2D"` |
| **Receiving** | Package box | `Symbol="Package"` | `FontIcon Glyph="\uE7B8"` |
| **Dunnage** | Box/container | `Symbol="AllApps"` | `FontIcon Glyph="\uE71D"` |
| **Routing** | Arrow path | `Symbol="MoveToFolder"` | `FontIcon Glyph="\uE8DE"` |
| **Volvo** | Circle with V | Custom `PathIcon` | `FontIcon Glyph="\uE774"` |
| **Reporting** | Document/list | `Symbol="Page"` | `FontIcon Glyph="\uE8A5"` |
| **User Preferences** | Person icon | `Symbol="Contact"` | `\uE77B` |

**Custom Icon Example (PathIcon)**:

```xml
<PathIcon 
    Data="M 10,5 L 15,15 L 5,15 Z"
    Foreground="{x:Bind AccentColor}"/>
```

---

#### 6. **Footer Legend** (Line 149-158 in SVG)

**WinUI 3 Control**: `StackPanel` with `TextBlock` elements

**XAML Example**:

```xml
<StackPanel 
    Orientation="Horizontal" 
    Spacing="16" 
    HorizontalAlignment="Left"
    Margin="16,24,0,0">
    <TextBlock Text="Role Access:" FontWeight="SemiBold"/>
    <TextBlock Text="User" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
    <TextBlock Text="Operator" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
    <TextBlock Text="Admin" Foreground="{ThemeResource TextFillColorPrimaryBrush}"/>
    <TextBlock Text="Developer" Foreground="{ThemeResource AccentTextFillColorPrimaryBrush}"/>
    <TextBlock Text="Super Admin" Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
</StackPanel>
```

---

## Color Scheme (WinUI 3 Theme Resources)

### Card Accent Colors

Each category uses a distinct accent color (mapped to WinUI 3 theme colors):

| Category | SVG Color | WinUI 3 Resource |
|----------|-----------|------------------|
| System Settings | `#0078d4` | `SystemAccentColor` |
| Security & Session | `#d83b01` | `SystemFillColorCriticalBrush` |
| ERP Integration | `#107c10` | `SystemFillColorSuccessBrush` |
| Receiving | `#8764b8` | Custom purple |
| Dunnage | `#ca5010` | Custom orange |
| Routing | `#004e8c` | Custom dark blue |
| Volvo | `#00188f` | Custom Volvo blue |
| Reporting | `#038387` | Custom teal |
| User Preferences | `#5c2d91` | Custom purple |

### Background Colors

- Page background: `ApplicationPageBackgroundThemeBrush` or `#f3f3f3`
- Card background: `CardBackgroundFillColorDefaultBrush` (white in light theme)
- Card border: `CardStrokeColorDefaultBrush` (`#e0e0e0`)

---

## Interaction States

### Hover State

```xml
<Border.Resources>
    <SolidColorBrush x:Key="CardHoverBackground" Color="{ThemeResource LayerFillColorDefaultBrush}"/>
</Border.Resources>
<VisualStateManager.VisualStateGroups>
    <VisualStateGroup x:Name="CommonStates">
        <VisualState x:Name="PointerOver">
            <VisualState.Setters>
                <Setter Target="CardBorder.Background" Value="{StaticResource CardHoverBackground}"/>
                <Setter Target="CardBorder.Translation" Value="0,-4,0"/>
            </VisualState.Setters>
        </VisualState>
    </VisualStateGroup>
</VisualStateManager.VisualStateGroups>
```

### Click/Pressed State

```xml
<VisualState x:Name="Pressed">
    <VisualState.Setters>
        <Setter Target="CardBorder.Translation" Value="0,2,0"/>
        <Setter Target="CardBorder.Opacity" Value="0.8"/>
    </VisualState.Setters>
</VisualState>
```

---

## Shadow Effect (WinUI 3)

**ThemeShadow** for elevation:

```xml
<Border.Shadow>
    <ThemeShadow />
</Border.Shadow>
<Border.Translation>0,0,8</Border.Translation>
```

**Alternative (Drop Shadow)**:

```xml
<Border>
    <Border.Resources>
        <DropShadow x:Key="CardShadow" 
                    BlurRadius="8" 
                    Opacity="0.1" 
                    Offset="0,2,0"/>
    </Border.Resources>
</Border>
```

---

## Data Model

```csharp
public class Model_SettingsCategory : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;
    
    [ObservableProperty]
    private string _subtitle = string.Empty;
    
    [ObservableProperty]
    private string _iconGlyph = "\uE713";
    
    [ObservableProperty]
    private Color _accentColor = Colors.Blue;
    
    [ObservableProperty]
    private int _settingCount = 0;
    
    [ObservableProperty]
    private Type? _targetPageType;
    
    [ObservableProperty]
    private Enum_PermissionLevel _requiredPermission = Enum_PermissionLevel.User;
}
```

---

## Navigation Implementation

```csharp
private void CategoryCard_Click(object sender, ItemClickEventArgs e)
{
    if (e.ClickedItem is Model_SettingsCategory category && category.TargetPageType != null)
    {
        _navigationService.NavigateTo(
            category.TargetPageType.FullName, 
            category
        );
    }
}
```

---

## Accessibility Considerations

1. **AutomationProperties.Name** on each card:

   ```xml
   <Border AutomationProperties.Name="{x:Bind Title}">
   ```

2. **Keyboard Navigation**: Ensure cards are focusable

   ```xml
   <Border IsTabStop="True" AllowFocusOnInteraction="True">
   ```

3. **Screen Reader Support**: Add descriptive labels

   ```xml
   <AutomationProperties.HelpText>
       Navigate to {x:Bind Title} settings section
   </AutomationProperties.HelpText>
   ```

4. **High Contrast Mode**: Use theme resources that adapt automatically

---

## Performance Optimizations

1. **x:Bind** instead of Binding for better performance
2. **Incremental Loading** if categories exceed 20 items
3. **Data Virtualization** with `ItemsRepeater`
4. **Image Caching** if using custom icons

---

## Responsive Design

### Adaptive Grid Layout

```xml
<ItemsRepeater.Layout>
    <UniformGridLayout 
        MinItemWidth="280"
        MinItemHeight="180"
        MinColumnSpacing="20"
        MinRowSpacing="20"
        ItemsStretch="None"/>
</ItemsRepeater.Layout>
```

**Behavior**:

- Window width > 1200px: 3 columns
- Window width 800-1200px: 2 columns
- Window width < 800px: 1 column (stacked)

---

## Complete Page Example

See `SettingsPageTemplate.xaml` in the templates folder for the full implementation combining all controls.

---

## References

- [WinUI 3 Gallery - GridView](https://github.com/microsoft/WinUI-Gallery/tree/main/WinUIGallery/Samples/ControlPages/GridViewPage.xaml)
- [WinUI 3 Gallery - ItemsRepeater](https://github.com/microsoft/WinUI-Gallery/tree/main/WinUIGallery/Samples/ControlPages/ItemsRepeaterPage.xaml)
- [WinUI 3 Cards Design](https://learn.microsoft.com/en-us/windows/apps/design/controls/cards)
- [WinUI 3 Icons](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font)
