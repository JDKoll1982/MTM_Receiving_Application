# MTM Receiving Application - Phase 2: MVVM Features Development

**Purpose**: Build WinUI 3 MVVM features for receiving label management using the infrastructure from Phase 1.

**Prerequisites**: 
- ✅ Phase 1 (Infrastructure) completed
- ✅ All verification items from Phase 1 checked
- WinUI 3 project created
- CommunityToolkit.Mvvm NuGet package installed

**Estimated Time**: 4-6 hours per major feature

---

## Overview

Phase 2 focuses on building the actual receiving label application using MVVM pattern with WinUI 3. This phase is **feature-driven** and integrates with the speckit workflow.

**Key MVVM Components**:
1. Models (already created in Phase 1)
2. ViewModels (with commands and data binding)
3. Views (XAML pages and controls)
4. Services (business logic)
5. Dependency Injection setup

---

## MVVM Development Order

Follow this order when building features to ensure proper dependencies:

### 1. Foundation Setup
- Application startup and dependency injection
- Navigation service
- Main window/shell

### 2. Core Features (in order)
- User authentication/login
- Receiving label entry
- Dunnage label entry
- UPS/FedEx routing labels
- Label preview and printing
- History/search functionality

### 3. Supporting Features
- Settings/preferences
- Export to CSV
- Infor Visual integration

---

## Step 1: Create WinUI 3 Project Structure

### 1.1 Initialize WinUI 3 Project

```powershell
# Navigate to project directory
cd C:\Users\johnk\source\repos\MTM_Receiving_Application

# Create WinUI 3 app (if not already created)
dotnet new winui -n MTM_Receiving_Application -o .
```

### 1.2 Install Required NuGet Packages

```powershell
dotnet add package CommunityToolkit.Mvvm --version 8.2.2
dotnet add package Microsoft.Extensions.DependencyInjection --version 8.0.0
dotnet add package Microsoft.Extensions.Hosting --version 8.0.0
dotnet add package MySql.Data --version 9.4.0
dotnet add package CommunityToolkit.WinUI.UI.Controls --version 7.1.2
```

---

## Step 2: Configure Dependency Injection

### 2.1 Create Service Registration

**File**: `App.xaml.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.ViewModels.Receiving;
using MTM_Receiving_Application.Views.Receiving;

namespace MTM_Receiving_Application;

public partial class App : Application
{
    private IHost _host;

    public App()
    {
        this.InitializeComponent();
        
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
                services.AddSingleton<ILoggingService, LoggingUtility>();
                
                // ViewModels
                services.AddTransient<ReceivingLabelViewModel>();
                services.AddTransient<DunnageLabelViewModel>();
                services.AddTransient<RoutingLabelViewModel>();
                
                // Views
                services.AddTransient<ReceivingLabelPage>();
                services.AddTransient<DunnageLabelPage>();
                services.AddTransient<RoutingLabelPage>();
                
                // Main Window
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _host.Services.GetRequiredService<MainWindow>().Activate();
    }

    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
    }
}
```

---

## Step 3: Create ViewModels (MVVM Pattern)

### 3.1 Base ViewModel

**File**: `ViewModels/Shared/BaseViewModel.cs`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Services.Database;

namespace MTM_Receiving_Application.ViewModels.Shared;

public abstract class BaseViewModel : ObservableObject
{
    protected readonly IService_ErrorHandler _errorHandler;
    protected readonly ILoggingService _logger;

    protected BaseViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
}
```

### 3.2 Receiving Label ViewModel

**File**: `ViewModels/Receiving/ReceivingLabelViewModel.cs`

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Receiving;

public partial class ReceivingLabelViewModel : BaseViewModel
{
    public ReceivingLabelViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        ReceivingLines = new ObservableCollection<Model_ReceivingLine>();
        LoadSampleData(); // For testing
    }

    public ObservableCollection<Model_ReceivingLine> ReceivingLines { get; }

    [ObservableProperty]
    private Model_ReceivingLine _currentLine = new();

    [ObservableProperty]
    private int _totalRows;

    [ObservableProperty]
    private int _employeeNumber = 6229; // From login

    [RelayCommand]
    private async Task AddLineAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Adding receiving line...";

            // Validate
            if (string.IsNullOrEmpty(CurrentLine.PartID))
            {
                _errorHandler.ShowUserError("Part ID is required", "Validation Error", nameof(AddLineAsync));
                return;
            }

            // Set employee number
            CurrentLine.EmployeeNumber = EmployeeNumber;

            // Save to database
            var result = await Dao_ReceivingLine.InsertReceivingLineAsync(CurrentLine);

            if (result.IsSuccess)
            {
                ReceivingLines.Add(CurrentLine);
                TotalRows = ReceivingLines.Count;
                
                CurrentLine = new Model_ReceivingLine(); // Reset for next entry
                StatusMessage = "Line added successfully";
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
                controlName: "ReceivingLabelViewModel"
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveToHistoryAsync()
    {
        // TODO: Implement bulk save to history
        StatusMessage = "Saving to history...";
        await Task.Delay(500); // Placeholder
        ReceivingLines.Clear();
        TotalRows = 0;
        StatusMessage = "Saved to history";
    }

    [RelayCommand]
    private void FillBlankSpaces()
    {
        // TODO: Implement auto-fill logic from last entry
        StatusMessage = "Filled blank spaces";
    }

    [RelayCommand]
    private void SortForPrinting()
    {
        // TODO: Implement sort by Part ID, PO, Heat
        StatusMessage = "Sorted for printing";
    }

    private void LoadSampleData()
    {
        // TODO: Load from database or history
    }
}
```

**Repeat similar pattern for**:
- `DunnageLabelViewModel.cs`
- `RoutingLabelViewModel.cs`

---

## Step 4: Create Views (XAML Pages)

### 4.1 Receiving Label Page

**File**: `Views/Receiving/ReceivingLabelPage.xaml`

```xml
<Page
    x:Class="MTM_Receiving_Application.Views.Receiving.ReceivingLabelPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MTM_Receiving_Application.ViewModels.Receiving"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.DataContext>
        <vm:ReceivingLabelViewModel />
    </Page.DataContext>

    <Grid Padding="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="16" Margin="0,0,0,24">
            <TextBlock Text="Receiving Label Entry" 
                       Style="{StaticResource TitleTextBlockStyle}"/>
            <TextBlock Text="{x:Bind ViewModel.EmployeeNumber, Mode=OneWay, StringFormat='Employee: {0}'}" 
                       Style="{StaticResource BodyTextBlockStyle}" 
                       VerticalAlignment="Center"/>
            <TextBlock Text="{x:Bind ViewModel.TotalRows, Mode=OneWay, StringFormat='Total Rows: {0}'}" 
                       Style="{StaticResource BodyTextBlockStyle}" 
                       VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Input Form -->
        <Grid Grid.Row="1" Margin="0,0,0,24" ColumnSpacing="12" RowSpacing="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <!-- Row 1 -->
            <TextBox Grid.Row="0" Grid.Column="0" 
                     Header="Quantity" 
                     Text="{x:Bind ViewModel.CurrentLine.Quantity, Mode=TwoWay}" 
                     InputScope="Number"/>
            
            <TextBox Grid.Row="0" Grid.Column="1" 
                     Header="Material ID" 
                     Text="{x:Bind ViewModel.CurrentLine.PartID, Mode=TwoWay}"/>
            
            <TextBox Grid.Row="0" Grid.Column="2" 
                     Header="PO Number" 
                     Text="{x:Bind ViewModel.CurrentLine.PONumber, Mode=TwoWay}" 
                     InputScope="Number"/>
            
            <TextBox Grid.Row="0" Grid.Column="3" 
                     Header="Heat" 
                     Text="{x:Bind ViewModel.CurrentLine.Heat, Mode=TwoWay}"/>

            <!-- Row 2 -->
            <CalendarDatePicker Grid.Row="1" Grid.Column="0" 
                                Header="Date" 
                                Date="{x:Bind ViewModel.CurrentLine.Date, Mode=TwoWay}"/>
            
            <TextBox Grid.Row="1" Grid.Column="1" 
                     Header="Initial Location" 
                     Text="{x:Bind ViewModel.CurrentLine.InitialLocation, Mode=TwoWay}"/>
            
            <NumberBox Grid.Row="1" Grid.Column="2" 
                       Header="Coils on Skid (Optional)" 
                       Value="{x:Bind ViewModel.CurrentLine.CoilsOnSkid, Mode=TwoWay}" 
                       SpinButtonPlacementMode="Inline" 
                       Minimum="1"/>
            
            <Button Grid.Row="1" Grid.Column="3" 
                    Content="Add Entry" 
                    Command="{x:Bind ViewModel.AddLineCommand}" 
                    Style="{StaticResource AccentButtonStyle}"
                    VerticalAlignment="Bottom"/>
        </Grid>

        <!-- DataGrid -->
        <ListView Grid.Row="2" 
                  ItemsSource="{x:Bind ViewModel.ReceivingLines, Mode=OneWay}"
                  SelectionMode="Single">
            <ListView.Header>
                <Grid Background="{ThemeResource SystemControlBackgroundAccentBrush}" Padding="12,8">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="80"/>
                        <ColumnDefinition Width="150"/>
                        <ColumnDefinition Width="100"/>
                        <ColumnDefinition Width="100"/>
                        <ColumnDefinition Width="120"/>
                        <ColumnDefinition Width="120"/>
                    </Grid.ColumnDefinitions>
                    <TextBlock Grid.Column="0" Text="Quantity" FontWeight="Bold" Foreground="White"/>
                    <TextBlock Grid.Column="1" Text="Material ID" FontWeight="Bold" Foreground="White"/>
                    <TextBlock Grid.Column="2" Text="PO Number" FontWeight="Bold" Foreground="White"/>
                    <TextBlock Grid.Column="3" Text="Heat" FontWeight="Bold" Foreground="White"/>
                    <TextBlock Grid.Column="4" Text="Date" FontWeight="Bold" Foreground="White"/>
                    <TextBlock Grid.Column="5" Text="Location" FontWeight="Bold" Foreground="White"/>
                </Grid>
            </ListView.Header>
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="vm:Model_ReceivingLine">
                    <Grid Padding="12,8">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="80"/>
                            <ColumnDefinition Width="150"/>
                            <ColumnDefinition Width="100"/>
                            <ColumnDefinition Width="100"/>
                            <ColumnDefinition Width="120"/>
                            <ColumnDefinition Width="120"/>
                        </Grid.ColumnDefinitions>
                        <TextBlock Grid.Column="0" Text="{x:Bind Quantity}"/>
                        <TextBlock Grid.Column="1" Text="{x:Bind PartID}"/>
                        <TextBlock Grid.Column="2" Text="{x:Bind PONumber}"/>
                        <TextBlock Grid.Column="3" Text="{x:Bind Heat}"/>
                        <TextBlock Grid.Column="4" Text="{x:Bind Date, StringFormat='{}{0:MM/dd/yyyy}'}"/>
                        <TextBlock Grid.Column="5" Text="{x:Bind InitialLocation}"/>
                    </Grid>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- Action Buttons -->
        <CommandBar Grid.Row="3" DefaultLabelPosition="Right">
            <AppBarButton Icon="Save" Label="Save to History" Command="{x:Bind ViewModel.SaveToHistoryCommand}"/>
            <AppBarButton Icon="Edit" Label="Fill Blank Spaces" Command="{x:Bind ViewModel.FillBlankSpacesCommand}"/>
            <AppBarButton Icon="Sort" Label="Sort for Printing" Command="{x:Bind ViewModel.SortForPrintingCommand}"/>
        </CommandBar>
    </Grid>
</Page>
```

**Code-Behind** (`ReceivingLabelPage.xaml.cs`):

```csharp
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;

namespace MTM_Receiving_Application.Views.Receiving;

public sealed partial class ReceivingLabelPage : Page
{
    public ReceivingLabelViewModel ViewModel { get; }

    public ReceivingLabelPage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<ReceivingLabelViewModel>();
        this.DataContext = ViewModel;
    }
}
```

---

## Step 5: Speckit Workflow Integration

### 5.1 Create Speckit Configuration

**File**: `.specify/config.json`

```json
{
  "project": {
    "name": "MTM Receiving Application",
    "type": "winui3",
    "framework": ".NET 8.0",
    "pattern": "MVVM"
  },
  "templates": {
    "spec": ".specify/templates/spec-template.md",
    "tasks": ".specify/templates/tasks-template.md"
  },
  "branches": {
    "prefix": "feature/",
    "format": "{shortName}-{number}"
  },
  "specs": {
    "directory": "specs"
  }
}
```

### 5.2 Create Spec Template

**File**: `.specify/templates/spec-template.md`

```markdown
# Feature: {{featureName}}

**Status**: {{status}}  
**Priority**: {{priority}}  
**Branch**: {{branchName}}

---

## User Story

As a **{{userRole}}**,  
I want to **{{userGoal}}**,  
So that **{{userBenefit}}**.

---

## Functional Requirements

### Must Have
- [ ] {{requirement1}}
- [ ] {{requirement2}}

### Nice to Have
- [ ] {{niceToHave1}}

---

## Success Criteria

- [ ] {{criterion1}}
- [ ] {{criterion2}}

---

## Technical Notes

- Architecture: MVVM
- Platform: WinUI 3 (.NET 8.0)
- Database: MySQL 5.7.24
- Pattern: Follow MTM WIP Application patterns

---

## Dependencies

- Phase 1 Infrastructure: ✅ Complete
- Database Tables: {{tables}}
- Stored Procedures: {{storedProcedures}}

---

## Testing Strategy

### Unit Tests
- [ ] ViewModel tests
- [ ] DAO tests

### Integration Tests
- [ ] Database stored procedure tests
- [ ] End-to-end workflow tests

---

## Implementation Notes

*To be filled during planning phase*
```

### 5.3 Using Speckit for Feature Development

**Example Workflow**:

```powershell
# 1. Create new feature specification
/speckit.specify Add receiving label entry functionality with Material ID lookup from Infor Visual

# This will:
# - Create branch: feature/receiving-label-entry-1
# - Generate spec: specs/receiving-label-entry-1/spec.md
# - Create checklist: specs/receiving-label-entry-1/checklists/requirements.md

# 2. Review and refine spec
# Edit specs/receiving-label-entry-1/spec.md as needed

# 3. Create technical plan
/speckit.plan

# This will:
# - Generate tasks.md with MVVM implementation steps
# - Break down into ViewModels, Views, Services tasks

# 4. Implement feature following tasks.md

# 5. Create pull request when done
```

---

## Step 6: Feature Development Order

Develop features in this order (use speckit for each):

### Priority 1: Core Label Entry
1. **Receiving Label Entry** (`/speckit.specify Add receiving label entry form`)
   - ViewModel: `ReceivingLabelViewModel`
   - View: `ReceivingLabelPage`
   - DAO: `Dao_ReceivingLine`
   - SP: `receiving_line_Insert`

2. **Dunnage Label Entry** (`/speckit.specify Add dunnage label tracking`)
   - ViewModel: `DunnageLabelViewModel`
   - View: `DunnageLabelPage`
   - DAO: `Dao_DunnageLine`
   - SP: `dunnage_line_Insert`

3. **Routing Label Entry** (`/speckit.specify Add internal routing labels`)
   - ViewModel: `RoutingLabelViewModel`
   - View: `RoutingLabelPage`
   - DAO: `Dao_RoutingLabel`
   - SP: `routing_label_Insert`

### Priority 2: Data Management
4. **Save to History** (`/speckit.specify Implement save to history archive`)
5. **Auto-Fill from History** (`/speckit.specify Add auto-fill previous values`)
6. **Sort for Printing** (`/speckit.specify Add label sorting functionality`)

### Priority 3: Label Generation
7. **Label Preview** (`/speckit.specify Add label preview before printing`)
8. **CSV Export for LabelView** (`/speckit.specify Export to CSV for LabelView`)
9. **Multi-Coil Label Generation** (`/speckit.specify Auto-generate coil labels`)

### Priority 4: Supporting Features
10. **Search Historical Labels** (`/speckit.specify Add search for past labels`)
11. **Infor Visual Integration** (`/speckit.specify Lookup part details from Infor Visual`)
12. **User Settings** (`/speckit.specify Add user preferences and themes`)

---

## Step 7: GitHub Instruction Files for MVVM

Create `.github/instructions/mvvm-*.instructions.md` files for each MVVM concern.

See **Section 8** below for instruction file templates.

---

## Step 8: Verification Checklist

Before considering Phase 2 complete for a feature:

- [ ] Spec created via `/speckit.specify`
- [ ] Technical plan created via `/speckit.plan`
- [ ] ViewModel implemented with proper MVVM pattern
- [ ] View (XAML) created with data binding
- [ ] ViewModel registered in DI container
- [ ] View registered in DI container
- [ ] Commands use `RelayCommand` / `AsyncRelayCommand`
- [ ] Error handling uses `Service_ErrorHandler`
- [ ] Logging uses `ILoggingService`
- [ ] DAO methods return `Model_Dao_Result<T>`
- [ ] All database calls are async
- [ ] UI updates via `INotifyPropertyChanged` (ObservableObject)
- [ ] Feature tested manually end-to-end
- [ ] Pull request created and reviewed

---

## Next Steps After Feature Completion

1. **Merge feature branch** to master
2. **Update CHANGELOG.md**
3. **Move to next priority feature** using speckit
4. **Repeat until all Priority 1-3 features complete**

---

## Final Setup: Core Documentation

**Only after ALL features complete**, create these core files:

### AGENTS.md

**File**: `AGENTS.md`

See companion file for template.

### copilot-instructions.md

**File**: `.github/copilot-instructions.md`

See companion file for template.

---

**Phase 2 MVVM Development**: Feature-driven, speckit-integrated, quality-focused! 🚀

Use `/speckit.specify` for each new feature to maintain consistency and documentation.
