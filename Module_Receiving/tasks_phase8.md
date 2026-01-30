# Phase 8: Settings Module - Task List

**Phase:** 8 of 8  
**Status:** ⏳ PENDING  
**Priority:** MEDIUM - Configuration and user preferences  
**Dependencies:** Phase 7 (Integration) must be complete

---

## 📊 **Phase 8 Overview**

**Goal:** Implement Module_Settings.Receiving for receiving-specific configuration and user preferences

**Settings Categories:**
- **User Preferences:** Default values, last-used settings, UI preferences
- **System Configuration:** File paths, connection strings, export settings
- **Reference Data Management:** Manage package types, locations, part preferences

**Status:**
- ⏳ Settings DAOs: 0/1 complete
- ⏳ Settings ViewModels: 0/2 complete
- ⏳ Settings Views: 0/2 complete
- ⏳ Integration: 0/2 complete

**Completion:** 0/7 tasks (0%)

**Estimated Total Time:** 8-10 hours

---

## 🎯 **Settings Module Structure**

```
Module_Settings.Receiving/
├── Data/
│   └── Dao_Settings_Repository_ReceivingPreferences.cs
│
├── ViewModels/
│   ├── ViewModel_Settings_Receiving_Display_UserPreferences.cs
│   └── ViewModel_Settings_Receiving_Display_ReferenceDataManager.cs
│
└── Views/
    ├── View_Settings_Receiving_Display_UserPreferences.xaml[.cs]
    └── View_Settings_Receiving_Display_ReferenceDataManager.xaml[.cs]
```

---

## ⏳ **Settings DAOs (1 task)**

### Task 8.1: Dao_Settings_Repository_ReceivingPreferences

**Priority:** P1 - HIGH  
**File:** `Module_Settings.Receiving/Data/Dao_Settings_Repository_ReceivingPreferences.cs`  
**Dependencies:** Database table `tbl_Receiving_Settings` created  
**Estimated Time:** 2 hours

**Responsibilities:**
- CRUD operations for user preferences
- CRUD operations for system settings
- Load default values
- Save user-specific settings

**Database Table:**
```sql
CREATE TABLE tbl_Receiving_Settings (
    SettingId INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(500),
    SettingType NVARCHAR(50), -- 'User' or 'System'
    UserId INT NULL, -- NULL for system-wide settings
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(100)
);
```

**Key Methods:**

```csharp
public class Dao_Settings_Repository_ReceivingPreferences
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;
    
    public Dao_Settings_Repository_ReceivingPreferences(
        string connectionString, 
        IService_LoggingUtility logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }
    
    // Get user preference
    public async Task<Model_Dao_Result<string>> GetUserPreferenceAsync(
        int userId, 
        string settingKey)
    {
        // Query: SELECT SettingValue FROM tbl_Receiving_Settings 
        //        WHERE UserId = @userId AND SettingKey = @settingKey
    }
    
    // Save user preference
    public async Task<Model_Dao_Result> SaveUserPreferenceAsync(
        int userId, 
        string settingKey, 
        string settingValue)
    {
        // UPSERT: If exists, UPDATE; else INSERT
    }
    
    // Get system setting
    public async Task<Model_Dao_Result<string>> GetSystemSettingAsync(string settingKey)
    {
        // Query: SELECT SettingValue FROM tbl_Receiving_Settings 
        //        WHERE UserId IS NULL AND SettingKey = @settingKey
    }
    
    // Save system setting
    public async Task<Model_Dao_Result> SaveSystemSettingAsync(
        string settingKey, 
        string settingValue)
    {
        // UPSERT system setting
    }
    
    // Get all user preferences
    public async Task<Model_Dao_Result<Dictionary<string, string>>> GetAllUserPreferencesAsync(int userId)
    {
        // Query all user settings, return as dictionary
    }
    
    // Get all system settings
    public async Task<Model_Dao_Result<Dictionary<string, string>>> GetAllSystemSettingsAsync()
    {
        // Query all system settings, return as dictionary
    }
}
```

**Setting Keys:**

**User Preferences:**
- `User.LastUsedPartNumber`
- `User.LastUsedPackageType`
- `User.LastUsedLocation`
- `User.DefaultLoadCount`
- `User.PreferredWorkflowMode` (Wizard, Manual, Edit)

**System Settings:**
- `System.CSVExportPathLocal`
- `System.CSVExportPathNetwork`
- `System.MaxLoadCount` (default: 999)
- `System.DefaultUnitOfMeasure` (default: LBS)
- `System.AutoSaveEnabled` (true/false)
- `System.SessionTimeoutMinutes` (default: 60)

**Acceptance Criteria:**
- [ ] DAO implements all CRUD methods
- [ ] User preference methods work
- [ ] System setting methods work
- [ ] Returns `Model_Dao_Result`
- [ ] No exceptions thrown (returns errors)
- [ ] Logging for all operations

---

## ⏳ **Settings ViewModels (2 tasks)**

### Task 8.2: ViewModel_Settings_Receiving_Display_UserPreferences

**Priority:** P1 - HIGH  
**File:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Display_UserPreferences.cs`  
**Dependencies:** Task 8.1 (DAO), IMediator  
**Estimated Time:** 3 hours

**Responsibilities:**
- Display user preferences UI
- Load current user preferences
- Save user preference changes
- Reset to defaults

**Key Properties:**

```csharp
public partial class ViewModel_Settings_Receiving_Display_UserPreferences : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    
    [ObservableProperty]
    private string _lastUsedPartNumber = string.Empty;
    
    [ObservableProperty]
    private string _lastUsedPackageType = string.Empty;
    
    [ObservableProperty]
    private string _lastUsedLocation = string.Empty;
    
    [ObservableProperty]
    private int _defaultLoadCount = 1;
    
    [ObservableProperty]
    private Enum_Receiving_Mode_WorkflowMode _preferredWorkflowMode = Enum_Receiving_Mode_WorkflowMode.Wizard;
    
    [ObservableProperty]
    private bool _hasUnsavedChanges = false;
}
```

**Key Commands:**

```csharp
[RelayCommand]
private async Task LoadUserPreferencesAsync()
{
    // Query user preferences via MediatR
    var query = new QueryRequest_Settings_Receiving_Get_UserPreferences 
    { 
        UserId = _currentUserId 
    };
    
    var result = await _mediator.Send(query);
    
    if (result.IsSuccess)
    {
        LastUsedPartNumber = result.Data.GetValueOrDefault("User.LastUsedPartNumber", "");
        LastUsedPackageType = result.Data.GetValueOrDefault("User.LastUsedPackageType", "");
        // ... load all preferences
    }
}

[RelayCommand]
private async Task SaveUserPreferencesAsync()
{
    // Save preferences via MediatR
    var command = new CommandRequest_Settings_Receiving_Save_UserPreferences
    {
        UserId = _currentUserId,
        Preferences = new Dictionary<string, string>
        {
            ["User.LastUsedPartNumber"] = LastUsedPartNumber,
            ["User.LastUsedPackageType"] = LastUsedPackageType,
            ["User.LastUsedLocation"] = LastUsedLocation,
            ["User.DefaultLoadCount"] = DefaultLoadCount.ToString(),
            ["User.PreferredWorkflowMode"] = PreferredWorkflowMode.ToString()
        }
    };
    
    var result = await _mediator.Send(command);
    
    if (result.IsSuccess)
    {
        HasUnsavedChanges = false;
        _errorHandler.ShowUserInfo("Preferences saved successfully", "Settings", nameof(SaveUserPreferencesAsync));
    }
}

[RelayCommand]
private async Task ResetToDefaultsAsync()
{
    // Reset all preferences to default values
    LastUsedPartNumber = string.Empty;
    LastUsedPackageType = string.Empty;
    LastUsedLocation = string.Empty;
    DefaultLoadCount = 1;
    PreferredWorkflowMode = Enum_Receiving_Mode_WorkflowMode.Wizard;
    
    await SaveUserPreferencesAsync();
}
```

**Acceptance Criteria:**
- [ ] Loads user preferences on initialization
- [ ] Saves preference changes
- [ ] Tracks unsaved changes
- [ ] Reset to defaults works
- [ ] Shows save success/failure messages

---

### Task 8.3: ViewModel_Settings_Receiving_Display_ReferenceDataManager

**Priority:** P2 - MEDIUM  
**File:** `Module_Settings.Receiving/ViewModels/ViewModel_Settings_Receiving_Display_ReferenceDataManager.cs`  
**Dependencies:** Task 8.1, IMediator  
**Estimated Time:** 2 hours

**Responsibilities:**
- Manage package types (add, edit, delete)
- Manage receiving locations (add, edit, delete)
- Manage part preferences (default settings per part)

**Key Properties:**

```csharp
[ObservableProperty]
private ObservableCollection<Model_Receiving_TableEntitys_PackageType> _packageTypes = new();

[ObservableProperty]
private ObservableCollection<Model_Receiving_TableEntitys_Location> _locations = new();

[ObservableProperty]
private ObservableCollection<Model_Receiving_TableEntitys_PartPreference> _partPreferences = new();

[ObservableProperty]
private Model_Receiving_TableEntitys_PackageType? _selectedPackageType;

[ObservableProperty]
private bool _isAddingNewPackageType = false;
```

**Key Commands:**

```csharp
[RelayCommand]
private async Task LoadReferenceDataAsync();

[RelayCommand]
private async Task AddPackageTypeAsync();

[RelayCommand]
private async Task EditPackageTypeAsync(Model_Receiving_TableEntitys_PackageType packageType);

[RelayCommand]
private async Task DeletePackageTypeAsync(Model_Receiving_TableEntitys_PackageType packageType);

[RelayCommand]
private async Task AddLocationAsync();

[RelayCommand]
private async Task EditLocationAsync(Model_Receiving_TableEntitys_Location location);

[RelayCommand]
private async Task DeleteLocationAsync(Model_Receiving_TableEntitys_Location location);
```

**Acceptance Criteria:**
- [ ] Loads all reference data
- [ ] Add/Edit/Delete package types
- [ ] Add/Edit/Delete locations
- [ ] Validation for required fields
- [ ] Confirmation before delete

---

## ⏳ **Settings Views (2 tasks)**

### Task 8.4: View_Settings_Receiving_Display_UserPreferences

**Priority:** P1 - HIGH  
**Files:**
- `Module_Settings.Receiving/Views/View_Settings_Receiving_Display_UserPreferences.xaml`
- `Module_Settings.Receiving/Views/View_Settings_Receiving_Display_UserPreferences.xaml.cs`  
**Dependencies:** Task 8.2 (ViewModel)  
**Estimated Time:** 2 hours

**UI Layout:**
- Form with user preference inputs
- Save/Reset buttons
- Unsaved changes indicator

**XAML Structure:**

```xaml
<Page x:Class="MTM_Receiving_Application.Module_Settings.Receiving.Views.View_Settings_Receiving_Display_UserPreferences">
    <Grid Padding="20">
        <StackPanel Spacing="20" MaxWidth="600">
            <TextBlock Text="User Preferences" Style="{StaticResource TitleTextBlockStyle}" />
            
            <!-- Last Used Values Section -->
            <StackPanel Spacing="10">
                <TextBlock Text="Last Used Values" Style="{StaticResource SubtitleTextBlockStyle}" />
                
                <TextBox 
                    Header="Last Used Part Number"
                    Text="{x:Bind ViewModel.LastUsedPartNumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
                
                <TextBox 
                    Header="Last Used Package Type"
                    Text="{x:Bind ViewModel.LastUsedPackageType, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
                
                <TextBox 
                    Header="Last Used Location"
                    Text="{x:Bind ViewModel.LastUsedLocation, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
            </StackPanel>
            
            <!-- Default Values Section -->
            <StackPanel Spacing="10">
                <TextBlock Text="Default Values" Style="{StaticResource SubtitleTextBlockStyle}" />
                
                <NumberBox 
                    Header="Default Load Count"
                    Value="{x:Bind ViewModel.DefaultLoadCount, Mode=TwoWay}"
                    Minimum="1"
                    Maximum="999" />
                
                <ComboBox 
                    Header="Preferred Workflow Mode"
                    SelectedItem="{x:Bind ViewModel.PreferredWorkflowMode, Mode=TwoWay}"
                    ItemsSource="{x:Bind ViewModel.AvailableWorkflowModes, Mode=OneWay}" />
            </StackPanel>
            
            <!-- Unsaved Changes Indicator -->
            <InfoBar 
                IsOpen="{x:Bind ViewModel.HasUnsavedChanges, Mode=OneWay}"
                Severity="Warning"
                Title="Unsaved Changes"
                Message="You have unsaved changes. Click 'Save' to apply." />
            
            <!-- Action Buttons -->
            <StackPanel Orientation="Horizontal" Spacing="10">
                <Button 
                    Content="Save" 
                    Command="{x:Bind ViewModel.SaveUserPreferencesCommand}"
                    Style="{StaticResource AccentButtonStyle}" />
                
                <Button 
                    Content="Reset to Defaults" 
                    Command="{x:Bind ViewModel.ResetToDefaultsCommand}" />
            </StackPanel>
        </StackPanel>
    </Grid>
</Page>
```

**Acceptance Criteria:**
- [ ] All preference inputs displayed
- [ ] Save button works
- [ ] Reset button works
- [ ] Unsaved changes indicator shows
- [ ] Form validation works

---

### Task 8.5: View_Settings_Receiving_Display_ReferenceDataManager

**Priority:** P2 - MEDIUM  
**Files:**
- `Module_Settings.Receiving/Views/View_Settings_Receiving_Display_ReferenceDataManager.xaml`
- `Module_Settings.Receiving/Views/View_Settings_Receiving_Display_ReferenceDataManager.xaml.cs`  
**Dependencies:** Task 8.3 (ViewModel)  
**Estimated Time:** 2.5 hours

**UI Layout:**
- TabView with tabs: Package Types, Locations, Part Preferences
- ListView for each reference data type
- Add/Edit/Delete buttons
- Inline editing support

**XAML Structure:**

```xaml
<Page x:Class="MTM_Receiving_Application.Module_Settings.Receiving.Views.View_Settings_Receiving_Display_ReferenceDataManager">
    <Grid Padding="20">
        <TabView>
            <!-- Package Types Tab -->
            <TabViewItem Header="Package Types">
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto" />
                        <RowDefinition Height="*" />
                    </Grid.RowDefinitions>
                    
                    <CommandBar Grid.Row="0">
                        <AppBarButton 
                            Icon="Add" 
                            Label="Add Package Type"
                            Command="{x:Bind ViewModel.AddPackageTypeCommand}" />
                    </CommandBar>
                    
                    <ListView 
                        Grid.Row="1"
                        ItemsSource="{x:Bind ViewModel.PackageTypes, Mode=OneWay}"
                        SelectedItem="{x:Bind ViewModel.SelectedPackageType, Mode=TwoWay}">
                        
                        <ListView.ItemTemplate>
                            <DataTemplate x:DataType="models:Model_Receiving_TableEntitys_PackageType">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="*" />
                                        <ColumnDefinition Width="Auto" />
                                    </Grid.ColumnDefinitions>
                                    
                                    <StackPanel Grid.Column="0">
                                        <TextBlock Text="{x:Bind TypeName}" FontWeight="SemiBold" />
                                        <TextBlock Text="{x:Bind Description}" FontSize="12" />
                                    </StackPanel>
                                    
                                    <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="5">
                                        <Button Content="Edit" />
                                        <Button Content="Delete" />
                                    </StackPanel>
                                </Grid>
                            </DataTemplate>
                        </ListView.ItemTemplate>
                    </ListView>
                </Grid>
            </TabViewItem>
            
            <!-- Locations Tab -->
            <TabViewItem Header="Locations">
                <!-- Similar structure for locations -->
            </TabViewItem>
            
            <!-- Part Preferences Tab -->
            <TabViewItem Header="Part Preferences">
                <!-- Similar structure for part preferences -->
            </TabViewItem>
        </TabView>
    </Grid>
</Page>
```

**Acceptance Criteria:**
- [ ] TabView shows all reference data types
- [ ] Add/Edit/Delete buttons work
- [ ] ListView displays data
- [ ] Inline editing works
- [ ] Confirmation before delete

---

## ⏳ **Integration Tasks (2 tasks)**

### Task 8.6: Register Settings Module in DI Container

**Priority:** P0 - CRITICAL  
**File:** `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`  
**Dependencies:** All Settings DAOs, ViewModels, Views implemented  
**Estimated Time:** 1 hour

**Registration Code:**

```csharp
private static IServiceCollection AddSettingsReceivingModule(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("MySql")
        ?? throw new InvalidOperationException("MySql connection string not found");
    
    // DAO
    services.AddSingleton(sp =>
    {
        var logger = sp.GetRequiredService<IService_LoggingUtility>();
        return new Dao_Settings_Repository_ReceivingPreferences(connectionString, logger);
    });
    
    // ViewModels
    services.AddTransient<ViewModel_Settings_Receiving_Display_UserPreferences>();
    services.AddTransient<ViewModel_Settings_Receiving_Display_ReferenceDataManager>();
    
    // Views
    services.AddTransient<View_Settings_Receiving_Display_UserPreferences>();
    services.AddTransient<View_Settings_Receiving_Display_ReferenceDataManager>();
    
    return services;
}

// In AddModuleServices method:
public static IServiceCollection AddModuleServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddReceivingModule(configuration);
    services.AddSettingsReceivingModule(configuration); // ← Add this
    services.AddDunnageModule(configuration);
    // ... other modules
}
```

**Acceptance Criteria:**
- [ ] Settings DAO registered
- [ ] Settings ViewModels registered
- [ ] Settings Views registered
- [ ] Build succeeds
- [ ] DI resolution works

---

### Task 8.7: Integrate Settings into Main Navigation

**Priority:** P1 - HIGH  
**File:** Main app navigation (Shell or MainWindow)  
**Dependencies:** Task 8.6  
**Estimated Time:** 1 hour

**Navigation Integration:**

1. Add "Settings" menu item to main navigation
2. Navigate to Settings → Receiving → User Preferences
3. Navigate to Settings → Receiving → Reference Data

**Navigation Routes:**

```csharp
["SettingsReceivingUserPreferences"] = typeof(View_Settings_Receiving_Display_UserPreferences),
["SettingsReceivingReferenceData"] = typeof(View_Settings_Receiving_Display_ReferenceDataManager),
```

**Acceptance Criteria:**
- [ ] Settings menu item added
- [ ] Navigation to User Preferences works
- [ ] Navigation to Reference Data works
- [ ] Back navigation works

---

## 📊 **Phase 8 Summary**

**Total Tasks:** 7  
**Total Estimated Time:** 8-10 hours  

**Task Breakdown:**
- DAOs: 1 task (2 hours)
- ViewModels: 2 tasks (5 hours)
- Views: 2 tasks (4.5 hours)
- Integration: 2 tasks (2 hours)

**Settings Module Features:**
- ✅ User-specific preferences
- ✅ System-wide settings
- ✅ Reference data management (Package Types, Locations)
- ✅ Part-specific default settings
- ✅ Reset to defaults capability
- ✅ Unsaved changes tracking

**Key Technologies:**
- MediatR for CRUD operations
- FluentValidation for settings validation
- ObservableCollection for reference data lists
- TabView for multi-category settings
- InfoBar for unsaved changes indicator

**Database Tables:**
- `tbl_Receiving_Settings` - User and system settings
- `tbl_Receiving_PackageType` - Package types reference data
- `tbl_Receiving_Location` - Receiving locations reference data
- `tbl_Receiving_PartPreference` - Part-specific preferences

**Success Criteria:**
- All settings persist to database
- User preferences load automatically
- Reference data management works
- Settings integrate with main workflow
- No impact on existing functionality

---

**Status:** ⏳ PENDING - Awaiting Phase 7 completion

**FINAL PHASE** - After Phase 8 completion, Module_Receiving is 100% complete! 🎉
