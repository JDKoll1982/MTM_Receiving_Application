# Routing Settings - WinUI 3 Controls

**SVG File**: `06-routing.svg`  
**Category**: Routing  
**Purpose**: Automatic routing rules and location management

---

## WinUI 3 Controls Used

### 1. **Auto-Routing Section**

#### Enable Automatic Routing - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable Automatic Routing"
    IsOn="{x:Bind ViewModel.RoutingEnableAutoRouting, Mode=TwoWay}"/>
```

#### Routing Priority - `ComboBox`

```xml
<ComboBox 
    Header="Routing Priority"
    SelectedItem="{x:Bind ViewModel.RoutingPriority, Mode=TwoWay}"
    Width="300">
    <ComboBoxItem Content="Part Number Match"/>
    <ComboBoxItem Content="Vendor Match"/>
    <ComboBoxItem Content="PO Type"/>
    <ComboBoxItem Content="Custom Rules"/>
</ComboBox>
```

#### Allow Manual Override - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Allow Manual Override"
    IsOn="{x:Bind ViewModel.RoutingAllowOverride, Mode=TwoWay}"/>
```

---

### 2. **Default Locations**

#### Fallback Location - `TextBox` or `ComboBox`

```xml
<ComboBox 
    Header="Fallback Location"
    ItemsSource="{x:Bind ViewModel.Locations}"
    SelectedItem="{x:Bind ViewModel.RoutingFallbackLocation, Mode=TwoWay}"
    DisplayMemberPath="Name"
    Width="280"/>
```

#### Show Confirmation - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Show Routing Confirmation"
    IsOn="{x:Bind ViewModel.RoutingShowConfirmation, Mode=TwoWay}"/>
```

---

### 3. **Manage Rules Button**

```xml
<Button 
    Content="Manage Routing Rules"
    Command="{x:Bind ViewModel.ManageRoutingRulesCommand}"
    Style="{StaticResource AccentButtonStyle}"
    Background="#004e8c"
    Width="180"
    Height="36"/>
```

**Command opens routing rules page or dialog**

---

## ViewModel

```csharp
public partial class RoutingSettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _routingEnableAutoRouting = true;
    
    [ObservableProperty]
    private string _routingPriority = "Part Number Match";
    
    [ObservableProperty]
    private bool _routingAllowOverride = true;
    
    [ObservableProperty]
    private string _routingFallbackLocation = "RECEIVING-HOLD";
    
    [ObservableProperty]
    private bool _routingShowConfirmation = true;
    
    [RelayCommand]
    private void ManageRoutingRules()
    {
        _navigationService.NavigateTo(typeof(RoutingRulesPage).FullName);
    }
}
```

---

## References

- [WinUI 3 Gallery - ComboBox](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ComboBoxPage.xaml)
