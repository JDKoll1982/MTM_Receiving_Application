# Dialog and Modal Patterns

## Existing Dialog Infrastructure

### Dialog Types

The application uses WinUI 3 `ContentDialog` for all modal interactions.

### Existing Dialog Examples

**Simple ContentDialogs (No ViewModel)**:

- `AddMultipleRowsDialog` - Simple input dialog with NumberBox
  - Location: `Views/Dunnage/Dialogs/`
  - Pattern: Inherits ContentDialog, uses code-behind for simple logic
  - Properties exposed directly on dialog class
  - PrimaryButtonClick event handler in code-behind

**Complex ContentDialogs (With ViewModel)**:

- `Shared_SharedTerminalLoginDialog`
  - Has dedicated ViewModel: `Shared_SharedTerminalLoginViewModel`
  - ViewModel injected via `App.GetService<T>()`
  - Location: `Views/Shared/`
  
- `Shared_NewUserSetupDialog`
  - Has dedicated ViewModel: `Shared_NewUserSetupViewModel`
  - Multi-step wizard pattern within dialog
  - Complex validation and state management

- `Dunnage_QuickAddTypeDialog`
  - Implements `INotifyPropertyChanged` directly
  - Mix of code-behind and property change notifications
  - Has dedicated ViewModel: `Dunnage_AddTypeDialogViewModel`

### Dialog Display Pattern

**XamlRoot Requirement**:
All ContentDialogs must have `XamlRoot` set before showing.

**Current Pattern**:

```csharp
var dialog = new ContentDialog
{
    Title = "Title",
    Content = "Message",
    PrimaryButtonText = "OK",
    CloseButtonText = "Cancel",
    XamlRoot = App.MainWindow.Content.XamlRoot  // REQUIRED
};

var result = await dialog.ShowAsync();
```

**Window Service**:
`IService_Window` provides `GetXamlRoot()` method:

- Returns `App.MainWindow?.Content?.XamlRoot`
- Returns nullable `XamlRoot?`

### Button Handlers

- `PrimaryButtonClick` event for primary action
- `CloseButtonClick` event for cancel/close
- `ContentDialogClosing` event for validation before close
- `ContentDialogButtonClickEventArgs.Deferral` for async operations

## Registration Requirements

### DI Registration Pattern

Dialogs are registered in `App.xaml.cs`:

**Transient Registration** (New instance each time):

```csharp
services.AddTransient<Views.Shared.Shared_SharedTerminalLoginDialog>();
services.AddTransient<Views.Dunnage.Dialogs.AddToInventoriedListDialog>();
```

**ViewModel Registration** (if dialog has ViewModel):

```csharp
services.AddTransient<Shared_SharedTerminalLoginViewModel>();
services.AddTransient<Dunnage_AddTypeDialogViewModel>();
```

### Dialog Creation Pattern

```csharp
// In consuming code (ViewModel or code-behind)
var dialog = App.GetService<MyDialog>();
dialog.XamlRoot = _windowService.GetXamlRoot();
var result = await dialog.ShowAsync();
```

## Help System Implications

For a centralized help dialog:

1. Must inherit from `ContentDialog`
2. Should have dedicated ViewModel (`HelpDialogViewModel`)
3. Must be registered as Transient in DI
4. ViewModel should be Transient in DI
5. XamlRoot must be set via `IService_Window.GetXamlRoot()`
6. Should accept help content via property or method
7. Should support rich content rendering (TextBlock, RichTextBlock, or custom controls)
