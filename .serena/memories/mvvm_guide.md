# MVVM Guide

Last Updated: 2026-03-21

## ViewModel Requirements

### Must-Have Attributes

- `partial` class (required for CommunityToolkit.Mvvm)
- Inherit from `ViewModel_Shared_Base`
- `[ObservableProperty]` for all bindable properties
- `[RelayCommand]` for all commands

### ViewModel Template

Class naming follows `ViewModel_<Module>_<Feature>` convention (e.g., `ViewModel_Receiving_Workflow`).

```csharp
public partial class ViewModel_Receiving_Workflow : ViewModel_Shared_Base
{
    private readonly IService_MySQL_Receiving _service;

    [ObservableProperty]
    private string _myProperty = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_ReceivingLine> _items;

    public ViewModel_Receiving_Workflow(
        IService_MySQL_Receiving service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _service = service;
        Items = new ObservableCollection<Model_ReceivingLine>();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";

            var result = await _service.GetDataAsync();
            if (result.IsSuccess)
            {
                Items.Clear();
                foreach (var item in result.Data) Items.Add(item);
                StatusMessage = "Loaded successfully";
            }
            else
            {
                _errorHandler.ShowUserError(result.ErrorMessage, "Load Error", nameof(LoadDataAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium,
                nameof(LoadDataAsync), nameof(ViewModel_Receiving_Workflow));
        }
        finally { IsBusy = false; }
    }
}
```

## View (XAML) Requirements

### Use x:Bind (Compile-Time Binding)

```xml
<!-- ✅ CORRECT - x:Bind -->
<TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

<!-- ❌ WRONG - Binding (runtime) -->
<TextBox Text="{Binding SearchText}"/>
```

### Binding Modes

- `OneWay` - Display data (VM → View)
- `TwoWay` - User input (View ↔ VM)
- `OneTime` - Set once, never updates

### View Template

```xml
<Page
    x:Class="MTM_Receiving_Application.Views.Receiving.MyFeatureView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.Receiving">

    <Page.DataContext>
        <viewmodels:MyFeatureViewModel />
    </Page.DataContext>

    <Grid Padding="20">
        <StackPanel Spacing="10">
            <TextBox
                Text="{x:Bind ViewModel.SearchText, Mode=TwoWay}"
                PlaceholderText="Search..." />

            <Button
                Content="Load Data"
                Command="{x:Bind ViewModel.LoadDataCommand}"
                IsEnabled="{x:Bind ViewModel.IsBusy, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}" />

            <ListView ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}" />
        </StackPanel>
    </Grid>
</Page>
```

## Code-Behind Pattern

Code-behind should ONLY contain UI-specific logic (NO business logic).

```csharp
public sealed partial class MyFeatureView : Page
{
    public MyFeatureViewModel ViewModel { get; }

    public MyFeatureView()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<MyFeatureViewModel>();
        this.DataContext = ViewModel;

        // ✅ OK - UI setup
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1400, 900);

        // ❌ FORBIDDEN - Business logic
        // var result = await _service.GetData();
    }
}
```

## Common Mistakes

### ❌ Non-partial ViewModel

```csharp
public class ViewModel_Receiving_Workflow : ViewModel_Shared_Base { } // Won't compile with [ObservableProperty]
```

### ❌ Missing Mode=TwoWay

```xml
<TextBox Text="{x:Bind ViewModel.SearchText}"/> <!-- Won't update ViewModel -->
```

### ❌ Business Logic in Code-Behind

```csharp
private void Button_Click(object sender, RoutedEventArgs e)
{
    var data = _service.GetData(); // NO! Use ViewModel command
}
```
