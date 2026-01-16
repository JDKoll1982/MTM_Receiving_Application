# MVVM Pattern Instructions

This file defines MVVM patterns and standards for WinUI 3 development in the MTM Receiving Application.

**Applies To**: `ViewModels/**/*.cs`, `Views/**/*.xaml`, `Views/**/*.xaml.cs`

---

## Core MVVM Principles

1. **Separation of Concerns**: ViewModels contain logic, Views contain UI
2. **Data Binding**: Use `x:Bind` for compile-time binding (not `Binding`)
3. **CommunityToolkit.Mvvm**: Use `ObservableObject`, `RelayCommand`, `Observable` Property` attributes
4. **Dependency Injection**: ALL ViewModels registered in DI container
5. **No Business Logic in Code-Behind**: Only UI-specific logic in `.xaml.cs`
6. **Layer Separation**: ViewModels **NEVER** access DAOs directly. Use Services.

## Architecture Diagram

```mermaid
graph TD
    View[View (XAML)] -->|x:Bind| ViewModel[ViewModel]
    ViewModel -->|Injects| Service[Service Interface]
    Service -->|Injects| DAO[DAO Interface/Class]
    DAO -->|Executes| DB[(Database)]
    
    subgraph "Presentation Layer"
    View
    ViewModel
    end
    
    subgraph "Business Layer"
    Service
    end
    
    subgraph "Data Layer"
    DAO
    DB
    end
```

---

## ViewModel Template

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Receiving;

/// <summary>
/// ViewModel for receiving label entry page.
/// Handles data entry, validation, and saving to database.
/// </summary>
public partial class ReceivingLabelViewModel : BaseViewModel
{
    private readonly IService_ReceivingWorkflow _receivingService;

    #region Constructor & DI

    public ReceivingLabelViewModel(
        IService_ReceivingWorkflow receivingService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        _receivingService = receivingService;
        ReceivingLines = new ObservableCollection<Model_ReceivingLine>();
        LoadInitialDataAsync().ConfigureAwait(false);
    }

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of receiving lines displayed in DataGrid.
    /// </summary>
    public ObservableCollection<Model_ReceivingLine> ReceivingLines { get; }

    /// <summary>
    /// Current line being edited.
    /// </summary>
    [ObservableProperty]
    private Model_ReceivingLine _currentLine = new();

    /// <summary>
    /// Total count of rows in collection.
    /// </summary>
    [ObservableProperty]
    private int _totalRows;

    /// <summary>
    /// Logged-in user's employee number.
    /// </summary>
    [ObservableProperty]
    private int _employeeNumber = 6229;

    #endregion

    #region Commands

    /// <summary>
    /// Adds current line to collection and saves to database.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddLine))]
    private async Task AddLineAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Adding receiving line...";

            // Validate
            if (!ValidateCurrentLine())
                return;

            // Set employee number
            CurrentLine.EmployeeNumber = EmployeeNumber;

            // Save to database via Service (NOT DAO directly)
            var result = await _receivingService.AddReceivingLineAsync(CurrentLine);

            if (result.IsSuccess)
            {
                ReceivingLines.Add(CurrentLine);
                TotalRows = ReceivingLines.Count;
                
                // Reset for next entry
                CurrentLine = new Model_ReceivingLine
                {
                    Date = DateTime.Now,
                    EmployeeNumber = EmployeeNumber
                };
                
                StatusMessage = "Line added successfully";
                AddLineCommand.NotifyCanExecuteChanged();
            }
            else
            {
                _errorHandler.ShowUserError(result.ErrorMessage, "Database Error", nameof(AddLineAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Models.Enums.Enum_ErrorSeverity.Medium,
                callerName: nameof(AddLineAsync),
                controlName: nameof(ReceivingLabelViewModel)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAddLine() => !string.IsNullOrEmpty(CurrentLine?.PartID);

    /// <summary>
    /// Saves all lines to history and clears current session.
    /// </summary>
    [RelayCommand]
    private async Task SaveToHistoryAsync()
    {
        if (ReceivingLines.Count == 0)
        {
            _errorHandler.ShowUserError("No lines to save", "Validation", nameof(SaveToHistoryAsync));
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving to history...";

            // TODO: Implement bulk history save
            await Task.Delay(500); // Placeholder

            ReceivingLines.Clear();
            TotalRows = 0;
            StatusMessage = "Saved to history successfully";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Medium);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadInitialDataAsync()
    {
        // Load user preferences, last session data, etc.
        await Task.CompletedTask;
    }

    private bool ValidateCurrentLine()
    {
        if (string.IsNullOrEmpty(CurrentLine.PartID))
        {
            _errorHandler.ShowUserError("Part ID is required", "Validation", nameof(AddLineAsync));
            return false;
        }

        if (CurrentLine.Quantity <= 0)
        {
            _errorHandler.ShowUserError("Quantity must be greater than 0", "Validation", nameof(AddLineAsync));
            return false;
        }

        return true;
    }

    #endregion
}
```

---

## View (XAML) Template

```xml
<Page
    x:Class="MTM_Receiving_Application.Views.Receiving.ReceivingLabelPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.ViewModels.Receiving"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <!-- DataContext set in code-behind via DI -->

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Header -->
            <RowDefinition Height="Auto"/> <!-- Input Form -->
            <RowDefinition Height="*"/>    <!-- DataGrid -->
            <RowDefinition Height="Auto"/> <!-- Actions -->
        </Grid.RowDefinitions>

        <!-- Header with Status -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="16" Margin="0,0,0,24">
            <TextBlock Text="Receiving Label Entry" 
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="{x:Bind ViewModel.EmployeeNumber, Mode=OneWay, StringFormat='Employee: {0}'}" 
                       Style="{StaticResource BodyTextBlockStyle}" 
                       VerticalAlignment="Center"/>
            <TextBlock Text="{x:Bind ViewModel.TotalRows, Mode=OneWay, StringFormat='Total Rows: {0}'}" 
                       Style="{StaticResource BodyTextBlockStyle}" 
                       VerticalAlignment="Center"/>
            <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" 
                          Width="20" Height="20" 
                          Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay}"/>
        </StackPanel>

        <!-- Input Form (responsive grid) -->
        <Grid Grid.Row="1" Margin="0,0,0,24" ColumnSpacing="12" RowSpacing="12">
            <!-- Column/Row definitions omitted for brevity -->
            
            <!-- Use x:Bind for compile-time binding -->
            <TextBox Header="Material ID" 
                     Text="{x:Bind ViewModel.CurrentLine.PartID, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
            
            <!-- Add Button with Command binding -->
            <Button Content="Add Entry" 
                    Command="{x:Bind ViewModel.AddLineCommand}" 
                    IsEnabled="{x:Bind ViewModel.AddLineCommand.CanExecute(null), Mode=OneWay}"
                    Style="{StaticResource AccentButtonStyle}"/>
        </Grid>

        <!-- DataGrid with ItemsSource binding -->
        <ListView Grid.Row="2" 
                  ItemsSource="{x:Bind ViewModel.ReceivingLines, Mode=OneWay}"
                  SelectionMode="Single">
            <!-- ItemTemplate with x:DataType for strong typing -->
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="vm:Model_ReceivingLine">
                    <Grid>
                        <TextBlock Text="{x:Bind PartID}"/>
                        <!-- More columns... -->
                    </Grid>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- Action Bar -->
        <CommandBar Grid.Row="3" DefaultLabelPosition="Right">
            <AppBarButton Icon="Save" 
                          Label="Save to History" 
                          Command="{x:Bind ViewModel.SaveToHistoryCommand}"/>
        </CommandBar>
    </Grid>
</Page>
```

---

## Code-Behind Template

```csharp
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving;

/// <summary>
/// Receiving label entry page.
/// ViewModel injected via DI, minimal code-behind logic.
/// </summary>
public sealed partial class ReceivingLabelPage : Page
{
    public ReceivingLabelViewModel ViewModel { get; }

    public ReceivingLabelPage()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI container
        ViewModel = App.GetService<ReceivingLabelViewModel>();
        this.DataContext = ViewModel;
    }
}
```

---

## Dependency Injection Setup

### App.xaml.cs Registration

```csharp
services.AddTransient<ReceivingLabelViewModel>();  // ViewModel
services.AddTransient<ReceivingLabelPage>();        // View
```

---

## Data Binding Guidelines

### Use x:Bind (Compile-Time Binding)

✅ **CORRECT**:

```xml
<TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"/>
```

❌ **WRONG** (runtime binding, slower):

```xml
<TextBlock Text="{Binding StatusMessage}"/>
```

### Observable Property Modes

| Mode | Use Case | Example |
|------|----------|---------|
| `OneWay` | Display data (VM → View) | Status messages, read-only data |
| `TwoWay` | User input (View ↔ VM) | TextBox.Text, CheckBox.IsChecked |
| `OneTime` | Set once, never updates | Static configuration |

---

## Command Guidelines

### Use RelayCommand with CanExecute

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private async Task SaveAsync()
{
    // Implementation
}

private bool CanSave() => !IsBusy && HasUnsavedChanges;
```

### Notify CanExecute Changes

```csharp
// When property affecting CanExecute changes
HasUnsavedChanges = true;
SaveCommand.NotifyCanExecuteChanged();
```

---

## Common Mistakes

❌ **Business logic in code-behind**

```csharp
// WRONG: In ReceivingLabelPage.xaml.cs
private void Button_Click(object sender, RoutedEventArgs e)
{
    var result = Dao_ReceivingLine.InsertAsync(line); // NO!
}
```

❌ **ViewModel calling DAO directly**

```csharp
// WRONG: In ReceivingLabelViewModel.cs
[RelayCommand]
private async Task AddLineAsync()
{
    var result = await Dao_ReceivingLine.InsertAsync(line); // NO! Use Service instead.
}
```

✅ **ViewModel calling Service**

```csharp
// CORRECT: In ReceivingLabelViewModel.cs
[RelayCommand]
private async Task AddLineAsync()
{
    var result = await _receivingService.AddReceivingLineAsync(line);
}
```

❌ **Forgetting Mode=TwoWay for input controls**

```xml
<!-- WRONG: Text won't update ViewModel -->
<TextBox Text="{x:Bind ViewModel.CurrentLine.PartID}"/>
```

✅ **Specify TwoWay for input**

```xml
<!-- CORRECT: Updates ViewModel on text change -->
<TextBox Text="{x:Bind ViewModel.CurrentLine.PartID, Mode=TwoWay}"/>
```

---

## Testing ViewModels

```csharp
[Fact]
public async Task AddLineAsync_ValidData_AddsToCollection()
{
    // Arrange
    var mockService = new Mock<IService_ReceivingWorkflow>();
    var mockErrorHandler = new Mock<IService_ErrorHandler>();
    var mockLogger = new Mock<ILoggingService>();
    var viewModel = new ReceivingLabelViewModel(mockService.Object, mockErrorHandler.Object, mockLogger.Object);
    
    viewModel.CurrentLine = new Model_ReceivingLine
    {
        PartID = "MMC0000848",
        Quantity = 100,
        PONumber = 66754
    };

    // Act
    await viewModel.AddLineCommand.ExecuteAsync(null);

    // Assert
    Assert.Single(viewModel.ReceivingLines);
    Assert.Equal("MMC0000848", viewModel.ReceivingLines[0].PartID);
}
```

---

## Quick Reference

| Component | Responsibility | Example |
|-----------|----------------|---------|
| **ViewModel** | Business logic, commands, data | `ReceivingLabelViewModel` |
| **View** | UI layout, data binding | `ReceivingLabelPage.xaml` |
| **Code-Behind** | UI-specific logic only | `ReceivingLabelPage.xaml.cs` |
| **Model** | Data structure | `Model_ReceivingLine` |
| **Service** | Reusable business logic | `Service_ReceivingWorkflow` |
| **DAO** | Database access | `Dao_ReceivingLine` |

**Key Pattern**: View → ViewModel → Service → DAO → Database

---

**Last Updated**: December 27, 2025  
**Framework**: WinUI 3 + CommunityToolkit.Mvvm
