# Receiving Settings - WinUI 3 Controls

**SVG File**: `04-receiving.svg`
**Category**: Receiving
**Purpose**: Workflow configuration and package type management

---

## WinUI 3 Controls Used

### 1. **Workflow Settings Section**

All workflow toggles use `ToggleSwitch` control:

#### Auto-verify Purchase Orders

```xml
<ToggleSwitch
    Header="Auto-verify Purchase Orders"
    IsOn="{x:Bind ViewModel.ReceivingAutoVerifyPO, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Automatically validate PO against Infor Visual on entry"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

#### Require Package Type Selection

```xml
<ToggleSwitch
    Header="Require Package Type Selection"
    IsOn="{x:Bind ViewModel.ReceivingRequirePackageType, Mode=TwoWay}"/>
```

#### Allow Partial Receiving

```xml
<ToggleSwitch
    Header="Allow Partial Receiving"
    IsOn="{x:Bind ViewModel.ReceivingAllowPartialReceiving, Mode=TwoWay}"/>
```

#### Show Over-Receiving Warnings

```xml
<ToggleSwitch
    Header="Show Over-Receiving Warnings"
    IsOn="{x:Bind ViewModel.ReceivingShowOverReceivingWarnings, Mode=TwoWay}"/>
```

---

### 2. **Package Types - ListView with DataTemplate**

```xml
<ListView
    ItemsSource="{x:Bind ViewModel.PackageTypes}"
    SelectedItem="{x:Bind ViewModel.SelectedPackageType, Mode=TwoWay}"
    SelectionMode="Single"
    Width="420"
    Height="300"
    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
    BorderThickness="1"
    CornerRadius="4">

    <ListView.Header>
        <Grid Background="{ThemeResource LayerFillColorDefaultBrush}"
              Padding="12,8"
              ColumnDefinitions="180,100,*">
            <TextBlock Text="Package Type" FontWeight="SemiBold" Grid.Column="0"/>
            <TextBlock Text="Code" FontWeight="SemiBold" Grid.Column="1"/>
            <TextBlock Text="Active" FontWeight="SemiBold" Grid.Column="2"/>
        </Grid>
    </ListView.Header>

    <ListView.ItemTemplate>
        <DataTemplate x:DataType="local:Model_PackageType">
            <Grid Padding="12,8" ColumnDefinitions="180,100,*">
                <TextBlock Text="{x:Bind Name}" Grid.Column="0"/>
                <TextBlock Text="{x:Bind Code}" Grid.Column="1"
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                <FontIcon Grid.Column="2"
                          Glyph="{x:Bind IsActive, Converter={StaticResource BoolToCheckmarkConverter}}"
                          FontSize="16"
                          Foreground="{x:Bind IsActive, Converter={StaticResource BoolToColorConverter}}"/>
            </Grid>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```

**Model**:

```csharp
public partial class Model_PackageType : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private bool _isActive = true;
}
```

---

### 3. **Action Buttons - CommandBar Alternative**

```xml
<StackPanel Orientation="Horizontal" Spacing="12" Margin="0,12,0,0">
    <Button
        Content="Add New"
        Command="{x:Bind ViewModel.AddPackageTypeCommand}"
        Style="{StaticResource AccentButtonStyle}"
        Width="120"/>

    <Button
        Content="Edit Selected"
        Command="{x:Bind ViewModel.EditPackageTypeCommand}"
        Width="120"
        IsEnabled="{x:Bind ViewModel.SelectedPackageType, Converter={StaticResource NullToBoolConverter}}"/>

    <Button
        Content="Delete"
        Command="{x:Bind ViewModel.DeletePackageTypeCommand}"
        Width="120"
        Foreground="{ThemeResource SystemFillColorCriticalBrush}"
        BorderBrush="{ThemeResource SystemFillColorCriticalBrush}"
        IsEnabled="{x:Bind ViewModel.SelectedPackageType, Converter={StaticResource NullToBoolConverter}}"/>
</StackPanel>
```

**Alternative using CommandBar**:

```xml
<CommandBar DefaultLabelPosition="Right">
    <AppBarButton Icon="Add" Label="Add New" Command="{x:Bind ViewModel.AddPackageTypeCommand}"/>
    <AppBarButton Icon="Edit" Label="Edit" Command="{x:Bind ViewModel.EditPackageTypeCommand}"/>
    <AppBarSeparator/>
    <AppBarButton Icon="Delete" Label="Delete" Command="{x:Bind ViewModel.DeletePackageTypeCommand}"/>
</CommandBar>
```

---

### 4. **Default Values Section**

#### Default Package Type - `ComboBox`

```xml
<ComboBox
    Header="Default Package Type"
    ItemsSource="{x:Bind ViewModel.ActivePackageTypes}"
    SelectedItem="{x:Bind ViewModel.ReceivingDefaultPackageType, Mode=TwoWay}"
    DisplayMemberPath="Name"
    Width="250"
    HorizontalAlignment="Left">
    <ComboBox.Description>
        <TextBlock Text="Pre-selected type when creating new packages"/>
    </ComboBox.Description>
</ComboBox>
```

#### Default Receiver Name - `TextBox`

```xml
<TextBox
    Header="Default Receiver Name"
    Text="{x:Bind ViewModel.ReceivingDefaultReceiverName, Mode=TwoWay}"
    PlaceholderText="Enter receiver name"
    Width="300"
    HorizontalAlignment="Left">
    <TextBox.Description>
        <TextBlock Text="Name auto-populated in receiver field"/>
    </TextBox.Description>
</TextBox>
```

---

## ViewModel

```csharp
public partial class ReceivingSettingsViewModel : BaseViewModel
{
    private readonly IService_PackageType _packageTypeService;

    // Workflow
    [ObservableProperty]
    private bool _receivingAutoVerifyPO = true;

    [ObservableProperty]
    private bool _receivingRequirePackageType = true;

    [ObservableProperty]
    private bool _receivingAllowPartialReceiving = true;

    [ObservableProperty]
    private bool _receivingShowOverReceivingWarnings = true;

    // Package Types
    [ObservableProperty]
    private ObservableCollection<Model_PackageType> _packageTypes = new();

    [ObservableProperty]
    private Model_PackageType? _selectedPackageType;

    [ObservableProperty]
    private ObservableCollection<Model_PackageType> _activePackageTypes = new();

    // Defaults
    [ObservableProperty]
    private Model_PackageType? _receivingDefaultPackageType;

    [ObservableProperty]
    private string _receivingDefaultReceiverName = string.Empty;

    public ReceivingSettingsViewModel(IService_PackageType packageTypeService)
    {
        _packageTypeService = packageTypeService;
    }

    public override async Task OnNavigatedToAsync(object parameter)
    {
        await LoadPackageTypesAsync();
    }

    private async Task LoadPackageTypesAsync()
    {
        var result = await _packageTypeService.GetAllPackageTypesAsync();

        if (result.IsSuccess)
        {
            PackageTypes.Clear();
            foreach (var pt in result.Data)
            {
                PackageTypes.Add(pt);
            }

            // Filter active types for dropdown
            ActivePackageTypes = new ObservableCollection<Model_PackageType>(
                PackageTypes.Where(pt => pt.IsActive)
            );
        }
    }

    [RelayCommand]
    private async Task AddPackageTypeAsync()
    {
        var dialog = new PackageTypeDialog();
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.PackageType != null)
        {
            var saveResult = await _packageTypeService.SavePackageTypeAsync(dialog.PackageType);

            if (saveResult.IsSuccess)
            {
                await LoadPackageTypesAsync();
            }
        }
    }

    [RelayCommand]
    private async Task EditPackageTypeAsync()
    {
        if (SelectedPackageType == null) return;

        var dialog = new PackageTypeDialog(SelectedPackageType);
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            await _packageTypeService.SavePackageTypeAsync(dialog.PackageType);
            await LoadPackageTypesAsync();
        }
    }

    [RelayCommand]
    private async Task DeletePackageTypeAsync()
    {
        if (SelectedPackageType == null) return;

        var confirm = new ContentDialog
        {
            Title = "Confirm Delete",
            Content = $"Delete package type '{SelectedPackageType.Name}'?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
        {
            await _packageTypeService.DeletePackageTypeAsync(SelectedPackageType.Id);
            await LoadPackageTypesAsync();
        }
    }
}
```

---

## Package Type Dialog (ContentDialog)

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Dialogs.PackageTypeDialog"
    Title="Package Type"
    PrimaryButtonText="Save"
    CloseButtonText="Cancel"
    DefaultButton="Primary">

    <StackPanel Spacing="16" MinWidth="400">
        <TextBox
            Header="Name"
            Text="{x:Bind PackageType.Name, Mode=TwoWay}"
            PlaceholderText="e.g., Box"/>

        <TextBox
            Header="Code"
            Text="{x:Bind PackageType.Code, Mode=TwoWay}"
            PlaceholderText="e.g., BOX"
            CharacterCasing="Upper"
            MaxLength="10"/>

        <ToggleSwitch
            Header="Active"
            IsOn="{x:Bind PackageType.IsActive, Mode=TwoWay}"/>
    </StackPanel>
</ContentDialog>
```

---

## Converters

### BoolToCheckmarkConverter

```csharp
public class BoolToCheckmarkConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? "\uE73E" : "\uE711"; // Checkmark : Circle
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

### BoolToColorConverter

```csharp
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isActive = (bool)value;
        return isActive
            ? Application.Current.Resources["SystemFillColorSuccessBrush"]
            : Application.Current.Resources["SystemFillColorCriticalBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

---

## Database Schema

```sql
CREATE TABLE dunnage_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    code VARCHAR(10) NOT NULL UNIQUE,
    is_active BOOLEAN NOT NULL DEFAULT 1,
    created_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    modified_date DATETIME ON UPDATE CURRENT_TIMESTAMP
);

-- Seed data
INSERT IGNORE INTO dunnage_types (name, code, is_active) VALUES
('Box', 'BOX', 1),
('Pallet', 'PLT', 1),
('Crate', 'CRT', 1),
('Skid', 'SKD', 1),
('Bag', 'BAG', 0),
('Other', 'OTH', 1);
```

---

## Validation Rules

### Workflow Settings

- **Auto-verify PO**: Available to all users
- **Require Package Type**: Operator+ can disable
- **Allow Partial Receiving**: Admin+ can disable
- **Show Warnings**: Available to all users

### Package Types

- **Name**: Required, max 50 characters, unique
- **Code**: Required, max 10 characters, uppercase, unique
- **Cannot delete** if in use by existing packages
- **Cannot deactivate** if set as default

```csharp
partial void OnReceivingDefaultPackageTypeChanged(Model_PackageType? value)
{
    if (value != null && !value.IsActive)
    {
        _errorHandler.ShowUserError(
            "Cannot set inactive package type as default",
            "Validation Error",
            nameof(OnReceivingDefaultPackageTypeChanged)
        );

        ReceivingDefaultPackageType = ActivePackageTypes.FirstOrDefault();
    }
}
```

---

## Permissions

| Setting | User | Operator | Admin | Developer | Super Admin |
|---------|------|----------|-------|-----------|-------------|
| Auto-verify PO | ✅ | ✅ | ✅ | ✅ | ✅ |
| Require Package Type | ❌ | ✅ | ✅ | ✅ | ✅ |
| Allow Partial | ❌ | ❌ | ✅ | ✅ | ✅ |
| Show Warnings | ✅ | ✅ | ✅ | ✅ | ✅ |
| View Package Types | ✅ | ✅ | ✅ | ✅ | ✅ |
| Add Package Type | ❌ | ❌ | ✅ | ✅ | ✅ |
| Edit Package Type | ❌ | ❌ | ✅ | ✅ | ✅ |
| Delete Package Type | ❌ | ❌ | ❌ | ✅ | ✅ |
| Default Package Type | ❌ | ✅ | ✅ | ✅ | ✅ |
| Default Receiver Name | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## Accessibility

```xml
<ListView
    AutomationProperties.Name="Package Types List"
    AutomationProperties.HelpText="List of available package types for receiving operations"/>

<Button
    AutomationProperties.Name="Add Package Type"
    AutomationProperties.HelpText="Create new package type"/>
```

---

## References

- [WinUI 3 Gallery - ListView](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ListViewPage.xaml)
- [WinUI 3 Gallery - ContentDialog](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ContentDialogPage.xaml)
- [WinUI 3 Gallery - CommandBar](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/CommandBarPage.xaml)
