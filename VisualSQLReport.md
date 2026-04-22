# Email Button Disable Logic

## Requirement
In both `Module_Volvo` and `Module_Reporting`, the **Send To** and **Send CC** email buttons must be disabled when their respective input fields contain no value.

## Implementation Details

### MVVM Approach
- Use `ICommand` (e.g., `RelayCommand`) with a `CanExecute` predicate bound to each button's `Command` property.
- The `CanExecute` delegate should return `false` when the corresponding `To` or `CC` property is `null`, empty, or whitespace.
- Raise `CommandManager.InvalidateRequerySuggested()` or call `RaiseCanExecuteChanged()` whenever the `To` / `CC` properties change.

### ViewModel Properties
```csharp
// Example pattern for both Module_Volvo and Module_Reporting ViewModels

private string _toAddress;
public string ToAddress
{
    get => _toAddress;
    set
    {
        _toAddress = value;
        OnPropertyChanged();
        SendToCommand.RaiseCanExecuteChanged();
    }
}

private string _ccAddress;
public string CcAddress
{
    get => _ccAddress;
    set
    {
        _ccAddress = value;
        OnPropertyChanged();
        SendCcCommand.RaiseCanExecuteChanged();
    }
}

public RelayCommand SendToCommand { get; }
public RelayCommand SendCcCommand { get; }

// In constructor:
// SendToCommand = new RelayCommand(ExecuteSendTo, () => !string.IsNullOrWhiteSpace(ToAddress));
// SendCcCommand = new RelayCommand(ExecuteSendCc, () => !string.IsNullOrWhiteSpace(CcAddress));
```

### XAML Binding
```xml
<!-- Bind the button Command; IsEnabled is automatically controlled by CanExecute -->
<Button Content="Send To"
        Command="{Binding SendToCommand}" />

<Button Content="Send CC"
        Command="{Binding SendCcCommand}" />
```

> **Note:** Do **not** set `IsEnabled` directly in XAML; rely solely on `CanExecute` to keep the logic in the ViewModel per MVVM standards.

## Affected Modules
| Module | To Button | CC Button |
|---|---|---|
| `Module_Volvo` | Disabled when `ToAddress` is empty | Disabled when `CcAddress` is empty |
| `Module_Reporting` | Disabled when `ToAddress` is empty | Disabled when `CcAddress` is empty |
