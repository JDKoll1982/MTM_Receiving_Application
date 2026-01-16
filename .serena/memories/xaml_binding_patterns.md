# XAML Binding Patterns

## Overview

This memory documents the standard XAML binding patterns used throughout the MTM Receiving Application, with emphasis on the help system integration and best practices.

## Fundamental Binding Rules

### 1. Always Use x:Bind (Compile-Time Binding)

✅ **CORRECT** - Compile-time binding with type safety:

```xml
<TextBox Text="{x:Bind ViewModel.PropertyName, Mode=TwoWay}"/>
```

❌ **WRONG** - Runtime binding (slower, no compile-time checking):

```xml
<TextBox Text="{Binding PropertyName, Mode=TwoWay}"/>
```

### 2. Always Specify Mode

Required modes:

- `Mode=OneWay` - Read-only display (default for most properties)
- `Mode=TwoWay` - User input fields
- `Mode=OneTime` - Static content that never changes

✅ **CORRECT**:

```xml
<TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}"/>
<TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}"/>
<TextBlock Text="{x:Bind ViewModel.AppVersion, Mode=OneTime}"/>
```

### 3. No Business Logic in Code-Behind

Code-behind should only:

- Inject ViewModel via constructor
- Call ViewModel methods in event handlers
- Handle pure UI concerns (animations, scrolling)

✅ **CORRECT**:

```csharp
public MyPage()
{
    ViewModel = App.GetService<MyViewModel>();
    InitializeComponent();
}
```

❌ **WRONG** - Business logic in code-behind:

```csharp
private void Button_Click(object sender, RoutedEventArgs e)
{
    var data = _service.GetData(); // Business logic belongs in ViewModel
}
```

## Help System Binding Patterns

### Pattern 1: Tooltips (Static Content)

**Use Case**: Button and control tooltips that never change

**Implementation**:

```xml
<Button 
    Content="Save"
    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.Save'), Mode=OneTime}"/>
    
<Button 
    Content="Refresh"
    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.Refresh'), Mode=OneTime}"/>
```

**ViewModel Method**:

```csharp
public string GetTooltip(string name) => _helpService.GetTooltip($"Tooltip.{name}");
```

**Key Points**:

- Use `Mode=OneTime` for performance (content never changes)
- Prefix convention: `Tooltip.Button.<ActionName>` or `Tooltip.Field.<FieldName>`
- Method returns string directly (no async, no null checks in XAML)

### Pattern 2: Placeholders (Static Content)

**Use Case**: TextBox placeholder text

**Implementation**:

```xml
<TextBox 
    PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.PONumber'), Mode=OneTime}"
    Text="{x:Bind ViewModel.PONumber, Mode=TwoWay}"/>
    
<TextBox 
    PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Field.Location'), Mode=OneTime}"
    Text="{x:Bind ViewModel.Location, Mode=TwoWay}"/>
```

**ViewModel Method**:

```csharp
public string GetPlaceholder(string name) => _helpService.GetPlaceholder($"Placeholder.{name}");
```

**Key Points**:

- Use `Mode=OneTime` (static content)
- Prefix convention: `Placeholder.Field.<FieldName>`
- Keep placeholder text short and actionable

### Pattern 3: Contextual Tips (Dynamic Content)

**Use Case**: Tips that change based on workflow step or user state

**Implementation**:

```xml
<InfoBar
    IsOpen="{x:Bind ViewModel.ShowTip, Mode=OneWay}"
    Severity="Informational"
    Message="{x:Bind ViewModel.CurrentTip, Mode=OneWay}">
    <InfoBar.ActionButton>
        <Button Command="{x:Bind ViewModel.DismissTipCommand}" Content="Dismiss"/>
    </InfoBar.ActionButton>
</InfoBar>
```

**ViewModel Properties**:

```csharp
[ObservableProperty]
private bool _showTip = true;

public string CurrentTip => _helpService.GetTip($"{WorkflowName}.{CurrentStepName}");

[RelayCommand]
private void DismissTip()
{
    ShowTip = false;
    // Optionally persist dismissal via help service
}
```

**Key Points**:

- Use `Mode=OneWay` for reactive updates
- Computed property pattern for dynamic content
- Leverage `[ObservableProperty]` for reactive state

### Pattern 4: Help Dialog (Command Binding)

**Use Case**: Contextual help dialog triggered by user action

**Implementation**:

```xml
<Button 
    Command="{x:Bind ViewModel.ShowHelpCommand}"
    ToolTipService.ToolTip="{x:Bind ViewModel.GetTooltip('Button.StepHelp'), Mode=OneTime}">
    <SymbolIcon Symbol="Help"/>
</Button>
```

**ViewModel Command**:

```csharp
[RelayCommand]
private async Task ShowHelpAsync()
{
    await _helpService.ShowContextualHelpAsync(CurrentStep);
}
```

**Key Points**:

- Use `RelayCommand` for async operations
- Service handles dialog creation and display
- No XAML for dialog content (handled by service)

### Pattern 5: InfoBar Messages (Dynamic Binding)

**Use Case**: Status messages, validation errors, warnings

**Implementation**:

```xml
<InfoBar
    IsOpen="{x:Bind ViewModel.ShowInfoBar, Mode=OneWay}"
    Severity="{x:Bind ViewModel.InfoBarSeverity, Mode=OneWay}"
    Title="{x:Bind ViewModel.InfoBarTitle, Mode=OneWay}"
    Message="{x:Bind ViewModel.InfoBarMessage, Mode=OneWay}"/>
```

**ViewModel Properties**:

```csharp
[ObservableProperty]
private bool _showInfoBar;

[ObservableProperty]
private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;

[ObservableProperty]
private string _infoBarTitle = string.Empty;

[ObservableProperty]
private string _infoBarMessage = string.Empty;

private void ShowWarning(string messageKey)
{
    var content = _helpService.GetHelpContent($"InfoBar.{messageKey}");
    if (content != null)
    {
        InfoBarTitle = content.Title;
        InfoBarMessage = content.Content;
        InfoBarSeverity = content.Severity switch
        {
            Enum_HelpSeverity.Warning => InfoBarSeverity.Warning,
            Enum_HelpSeverity.Critical => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational
        };
        ShowInfoBar = true;
    }
}
```

## Common XAML Patterns

### Loading States

```xml
<ProgressRing 
    IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
    Width="40" Height="40"/>
    
<TextBlock 
    Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
    Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay}"/>
```

### Button Enable/Disable

```xml
<Button 
    Content="Save"
    Command="{x:Bind ViewModel.SaveCommand}"
    IsEnabled="{x:Bind ViewModel.CanSave, Mode=OneWay}"/>
```

**ViewModel**:

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync() { }

private bool CanSave => !IsBusy && !string.IsNullOrEmpty(RequiredField);

partial void OnRequiredFieldChanged(string value)
{
    SaveCommand.NotifyCanExecuteChanged();
}
```

### ListView/GridView Binding

```xml
<ListView
    ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"
    SelectedItem="{x:Bind ViewModel.SelectedItem, Mode=TwoWay}">
    <ListView.ItemTemplate>
        <DataTemplate x:DataType="models:Model_MyItem">
            <StackPanel>
                <TextBlock Text="{x:Bind Title}"/>
                <TextBlock Text="{x:Bind Description}" Foreground="Gray"/>
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

**Key Points**:

- Use `ObservableCollection<T>` in ViewModel
- Specify `x:DataType` in DataTemplate for compile-time binding
- `SelectedItem` uses `TwoWay` for user selection tracking

### Visibility Converters

```xml
xmlns:converters="using:MTM_Receiving_Application.Converters"

<Page.Resources>
    <converters:Converter_BooleanToVisibility x:Key="BoolToVisibility"/>
    <converters:Converter_EmptyStringToVisibility x:Key="EmptyStringToVisibility"/>
</Page.Resources>

<TextBlock 
    Text="No items found"
    Visibility="{x:Bind ViewModel.HasNoItems, Mode=OneWay, Converter={StaticResource BoolToVisibility}}"/>
    
<Button 
    Content="Clear"
    Visibility="{x:Bind ViewModel.SearchText, Mode=OneWay, Converter={StaticResource EmptyStringToVisibility}}"/>
```

## DataTemplate Patterns

### Inline DataTemplates (Small Content)

```xml
<ComboBox ItemsSource="{x:Bind ViewModel.Options, Mode=OneWay}">
    <ComboBox.ItemTemplate>
        <DataTemplate x:DataType="models:Model_Option">
            <TextBlock Text="{x:Bind DisplayName}"/>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

### Resource DataTemplates (Reusable)

```xml
<Page.Resources>
    <DataTemplate x:Key="ItemCardTemplate" x:DataType="models:Model_Item">
        <Border Style="{StaticResource CardBorderStyle}">
            <StackPanel Padding="12">
                <TextBlock Text="{x:Bind Title}" Style="{StaticResource SubtitleTextBlockStyle}"/>
                <TextBlock Text="{x:Bind Description}" Style="{StaticResource CaptionTextBlockStyle}"/>
            </StackPanel>
        </Border>
    </DataTemplate>
</Page.Resources>

<GridView 
    ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"
    ItemTemplate="{StaticResource ItemCardTemplate}"/>
```

## Event Handling Patterns

### Command Binding (Preferred)

✅ **CORRECT** - Use commands for all user actions:

```xml
<Button Command="{x:Bind ViewModel.SaveCommand}" Content="Save"/>
```

### Event Handler (Only for UI-Specific Logic)

✅ **ACCEPTABLE** - For scroll, loaded, size changed events:

```xml
<Page Loaded="OnPageLoaded">
```

```csharp
private async void OnPageLoaded(object sender, RoutedEventArgs e)
{
    await ViewModel.LoadDataCommand.ExecuteAsync(null);
}
```

❌ **AVOID** - Don't use events for business logic:

```xml
<Button Click="Button_Click"/> <!-- Use Command instead -->
```

## Performance Best Practices

### 1. Use OneTime for Static Content

```xml
<!-- Static content that never changes -->
<TextBlock Text="{x:Bind ViewModel.AppName, Mode=OneTime}"/>
<TextBlock Text="{x:Bind ViewModel.GetTooltip('Button.Help'), Mode=OneTime}"/>
```

### 2. Use OneWay for Read-Only Display

```xml
<!-- Display-only properties -->
<TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}"/>
<ListView ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"/>
```

### 3. Use TwoWay Only for User Input

```xml
<!-- User input fields only -->
<TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}"/>
<CheckBox IsChecked="{x:Bind ViewModel.IsEnabled, Mode=TwoWay}"/>
```

### 4. Avoid Unnecessary Updates

```csharp
// Good - Update multiple properties then notify once
private void UpdateState()
{
    _title = "New Title";
    _message = "New Message";
    OnPropertyChanged(nameof(Title));
    OnPropertyChanged(nameof(Message));
}

// Better - Use computed properties when possible
public string FullName => $"{FirstName} {LastName}";
partial void OnFirstNameChanged(string value) => OnPropertyChanged(nameof(FullName));
partial void OnLastNameChanged(string value) => OnPropertyChanged(nameof(FullName));
```

## Common Mistakes to Avoid

### ❌ Missing Mode

```xml
<!-- WRONG - Mode not specified -->
<TextBox Text="{x:Bind ViewModel.Name}"/>

<!-- CORRECT -->
<TextBox Text="{x:Bind ViewModel.Name, Mode=TwoWay}"/>
```

### ❌ Using Binding Instead of x:Bind

```xml
<!-- WRONG - Runtime binding -->
<TextBlock Text="{Binding Title}"/>

<!-- CORRECT - Compile-time binding -->
<TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}"/>
```

### ❌ Business Logic in Code-Behind

```csharp
// WRONG
private void Button_Click(object sender, RoutedEventArgs e)
{
    var result = _service.ProcessData(Input.Text);
    Output.Text = result;
}

// CORRECT
private void Button_Click(object sender, RoutedEventArgs e)
{
    ViewModel.ProcessCommand.Execute(null);
}
```

### ❌ Not Using x:DataType in DataTemplates

```xml
<!-- WRONG - Runtime binding in template -->
<DataTemplate>
    <TextBlock Text="{Binding Name}"/>
</DataTemplate>

<!-- CORRECT - Compile-time binding -->
<DataTemplate x:DataType="models:Model_Item">
    <TextBlock Text="{x:Bind Name}"/>
</DataTemplate>
```

## Help System Migration Checklist

When migrating hard-coded help content to the service:

1. **Identify Content Type**:
   - Tooltip → `GetTooltip("Button.XXX")` or `GetTooltip("Field.XXX")`
   - Placeholder → `GetPlaceholder("Field.XXX")`
   - Tip → `GetTip("Workflow.View")`
   - Help Dialog → `ShowHelpCommand`

2. **Add to Service**:
   - Add content to `Service_Help.cs` initialization methods
   - Use appropriate key prefix and naming convention

3. **Update ViewModel**:
   - Inject `IService_Help` in constructor
   - Add helper methods: `GetTooltip()`, `GetPlaceholder()`, etc.
   - Add `ShowHelpCommand` if contextual help needed

4. **Update XAML**:
   - Replace hard-coded strings with bindings
   - Use `Mode=OneTime` for tooltips and placeholders
   - Use `Mode=OneWay` for dynamic content

5. **Test**:
   - Verify content displays correctly
   - Check that commands trigger help dialogs
   - Ensure no null reference exceptions

## Related Memories

- `help_system_architecture` - Help system design and components
- `service_infrastructure` - Service registration and DI patterns
- `mvvm-viewmodels.instructions` - ViewModel patterns and best practices
- `mvvm-views.instructions` - XAML view patterns and standards
