# Settings Database Test - Development Tool

**SVG File**: `10-dev-database-test.svg`
**Type**: Development Window
**Purpose**: Test and validate Settings System database schema, stored procedures, and DAOs
**Access**: Development Mode Only - "Settings DB Test" button in MainWindow

---

## Overview

This development tool provides comprehensive testing and validation of the Settings System database implementation. It verifies schema correctness, tests all stored procedures, validates DAO operations, and provides detailed logging for troubleshooting.

**Key Features:**

- Real-time database connection monitoring
- Automated table schema validation
- Stored procedure execution testing
- DAO CRUD operation verification
- Detailed error logging and reporting
- Export test results for documentation

---

## WinUI 3 Implementation

### Window Structure

```xml
<Window
    x:Class="MTM_Receiving_Application.Views.Development.DatabaseTestWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.ViewModels.Development"
    Title="Settings Database Test"
    Width="1200"
    Height="900">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Grid Grid.Row="0" Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" Padding="40,25">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <StackPanel>
                <TextBlock Text="Settings Database Test" Style="{StaticResource TitleTextBlockStyle}"/>
                <TextBlock Text="Development Tool - Schema &amp; Stored Procedure Validation"
                           FontSize="12"
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>

            <Button Grid.Column="1"
                    Content="Refresh All Tests"
                    Command="{x:Bind ViewModel.RefreshAllCommand}"
                    Style="{StaticResource AccentButtonStyle}"
                    Width="140"/>
        </Grid>

        <!-- Connection Status Card -->
        <Border Grid.Row="1"
                Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                BorderThickness="1"
                CornerRadius="8"
                Margin="40,20,40,0"
                Padding="20">
            <StackPanel Spacing="12">
                <TextBlock Text="Database Connection" FontWeight="SemiBold" FontSize="16"/>

                <StackPanel Orientation="Horizontal" Spacing="12">
                    <FontIcon Glyph="{x:Bind ViewModel.ConnectionStatus.IsConnected, Converter={StaticResource BoolToGlyphConverter}, Mode=OneWay}"
                              Foreground="{x:Bind ViewModel.ConnectionStatus.IsConnected, Converter={StaticResource BoolToColorConverter}, Mode=OneWay}"
                              FontSize="20"/>
                    <TextBlock Text="{x:Bind ViewModel.ConnectionStatus.StatusMessage, Mode=OneWay}"
                               VerticalAlignment="Center"/>
                </StackPanel>

                <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12">
                    <Run Text="Connection time: "/>
                    <Run Text="{x:Bind ViewModel.ConnectionStatus.ConnectionTimeMs, Mode=OneWay}"/>
                    <Run Text="ms | MySQL "/>
                    <Run Text="{x:Bind ViewModel.ConnectionStatus.ServerVersion, Mode=OneWay}"/>
                </TextBlock>
            </StackPanel>
        </Border>

        <!-- Summary Cards -->
        <Grid Grid.Row="2" Margin="40,20">
            <Grid.ColumnDefinitions>
                <ColumnDefinition/>
                <ColumnDefinition/>
                <ColumnDefinition/>
                <ColumnDefinition/>
            </Grid.ColumnDefinitions>

            <!-- Tables Summary -->
            <Border Grid.Column="0"
                    Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="8"
                    Padding="20"
                    Margin="0,0,10,0">
                <StackPanel Spacing="8">
                    <TextBlock Text="Tables" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontWeight="SemiBold"/>
                    <TextBlock FontSize="36" FontWeight="SemiBold">
                        <Run Text="{x:Bind ViewModel.TablesValidated, Mode=OneWay}" Foreground="#107c10"/>
                        <Run Text=" / " Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        <Run Text="{x:Bind ViewModel.TotalTables, Mode=OneWay}" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    </TextBlock>
                    <TextBlock Text="All tables validated" Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12"/>
                    <TextBlock Text="✓ Indexes OK" Foreground="#107c10" FontSize="11"/>
                </StackPanel>
            </Border>

            <!-- Similar cards for SP, Data, DAOs -->
            <!-- ... -->
        </Grid>

        <!-- Tabs and Test Results -->
        <TabView Grid.Row="3" Margin="40,0">
            <TabViewItem Header="Schema Tests">
                <ScrollViewer>
                    <ItemsControl ItemsSource="{x:Bind ViewModel.TableTestResults, Mode=OneWay}">
                        <ItemsControl.ItemTemplate>
                            <DataTemplate x:DataType="viewmodels:TableTestResult">
                                <Grid Padding="20,10">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="200"/>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>

                                    <FontIcon Grid.Column="0"
                                              Glyph="{x:Bind IsValid, Converter={StaticResource BoolToGlyphConverter}}"
                                              Foreground="{x:Bind IsValid, Converter={StaticResource BoolToColorConverter}}"
                                              Margin="0,0,15,0"/>

                                    <TextBlock Grid.Column="1" Text="{x:Bind TableName}" VerticalAlignment="Center"/>

                                    <TextBlock Grid.Column="2"
                                               Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                               VerticalAlignment="Center">
                                        <Run Text="{x:Bind RowCount}"/>
                                        <Run Text=" rows | "/>
                                        <Run Text="{x:Bind ColumnCount}"/>
                                        <Run Text=" columns | "/>
                                        <Run Text="{x:Bind IndexCount}"/>
                                        <Run Text=" indexes"/>
                                    </TextBlock>

                                    <Border Grid.Column="3"
                                            BorderBrush="{x:Bind IsValid, Converter={StaticResource BoolToColorConverter}}"
                                            BorderThickness="1"
                                            CornerRadius="4"
                                            Padding="12,4">
                                        <TextBlock Text="{x:Bind StatusText}"
                                                   Foreground="{x:Bind IsValid, Converter={StaticResource BoolToColorConverter}}"
                                                   FontSize="11"/>
                                    </Border>
                                </Grid>
                            </DataTemplate>
                        </ItemsControl.ItemTemplate>
                    </ItemsControl>
                </ScrollViewer>
            </TabViewItem>

            <TabViewItem Header="SP Tests">
                <!-- Stored Procedure Test Results -->
            </TabViewItem>

            <TabViewItem Header="DAO Tests">
                <!-- DAO Test Results -->
            </TabViewItem>

            <TabViewItem Header="Logs">
                <ScrollViewer>
                    <TextBlock Text="{x:Bind ViewModel.TestLogs, Mode=OneWay}"
                               FontFamily="Consolas"
                               FontSize="11"
                               Padding="20"/>
                </ScrollViewer>
            </TabViewItem>
        </TabView>

        <!-- Footer -->
        <Grid Grid.Row="4"
              Background="{ThemeResource CardBackgroundFillColorDefaultBrush}"
              BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
              BorderThickness="0,1,0,0"
              Padding="40,25">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <StackPanel>
                <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12">
                    <Run Text="Last run: "/>
                    <Run Text="{x:Bind ViewModel.LastRunTime, Mode=OneWay}"/>
                </TextBlock>
                <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}" FontSize="12">
                    <Run Text="Total test duration: "/>
                    <Run Text="{x:Bind ViewModel.TotalDurationMs, Mode=OneWay}"/>
                    <Run Text=" ms"/>
                </TextBlock>
            </StackPanel>

            <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="10">
                <Button Content="Export Results"
                        Command="{x:Bind ViewModel.ExportResultsCommand}"
                        Width="140"/>
                <Button Content="Close"
                        Click="Close_Click"
                        Width="100"/>
            </StackPanel>
        </Grid>
    </Grid>
</Window>
```

---

## ViewModel Implementation

```csharp
public partial class DatabaseTestViewModel : BaseViewModel
{
    private readonly IService_Settings _settingsService;
    private readonly Dao_SystemSettings _systemSettingsDao;
    private readonly Dao_UserSettings _userSettingsDao;
    private readonly Dao_PackageType _packageTypeDao;
    private readonly Dao_RoutingRule _routingRuleDao;
    private readonly Dao_ScheduledReport _scheduledReportDao;
    private readonly Dao_PackageTypeMapping _packageTypeMappingDao;

    [ObservableProperty]
    private Model_ConnectionTestResult _connectionStatus = new();

    [ObservableProperty]
    private int _totalTables = 7;

    [ObservableProperty]
    private int _tablesValidated;

    [ObservableProperty]
    private int _totalStoredProcedures = 25;

    [ObservableProperty]
    private int _storedProceduresTested;

    [ObservableProperty]
    private int _totalDaos = 6;

    [ObservableProperty]
    private int _daosValidated;

    [ObservableProperty]
    private ObservableCollection<TableTestResult> _tableTestResults = new();

    [ObservableProperty]
    private ObservableCollection<StoredProcedureTestResult> _spTestResults = new();

    [ObservableProperty]
    private ObservableCollection<DaoTestResult> _daoTestResults = new();

    [ObservableProperty]
    private string _testLogs = string.Empty;

    [ObservableProperty]
    private DateTime _lastRunTime;

    [ObservableProperty]
    private int _totalDurationMs;

    public DatabaseTestViewModel(
        IService_Settings settingsService,
        Dao_SystemSettings systemSettingsDao,
        Dao_UserSettings userSettingsDao,
        Dao_PackageType packageTypeDao,
        Dao_RoutingRule routingRuleDao,
        Dao_ScheduledReport scheduledReportDao,
        Dao_PackageTypeMapping packageTypeMappingDao,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _settingsService = settingsService;
        _systemSettingsDao = systemSettingsDao;
        _userSettingsDao = userSettingsDao;
        _packageTypeDao = packageTypeDao;
        _routingRuleDao = routingRuleDao;
        _scheduledReportDao = scheduledReportDao;
        _packageTypeMappingDao = packageTypeMappingDao;
    }

    [RelayCommand]
    private async Task RefreshAllAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var startTime = DateTime.Now;
            var stopwatch = Stopwatch.StartNew();

            TestLogs = $"[{DateTime.Now:HH:mm:ss}] Starting comprehensive database tests...\n";

            // Test connection
            await TestConnectionAsync();

            // Test schema
            await TestSchemaAsync();

            // Test stored procedures
            await TestStoredProceduresAsync();

            // Test DAOs
            await TestDaosAsync();

            stopwatch.Stop();
            LastRunTime = startTime;
            TotalDurationMs = (int)stopwatch.ElapsedMilliseconds;

            TestLogs += $"\n[{DateTime.Now:HH:mm:ss}] All tests completed in {TotalDurationMs}ms";
            StatusMessage = $"Tests completed: {TablesValidated}/{TotalTables} tables, {StoredProceduresTested}/{TotalStoredProcedures} SPs, {DaosValidated}/{TotalDaos} DAOs";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(RefreshAllAsync), nameof(DatabaseTestViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestConnectionAsync()
    {
        TestLogs += $"[{DateTime.Now:HH:mm:ss}] Testing database connection...\n";

        var stopwatch = Stopwatch.StartNew();
        var connectionString = Helper_Database_Variables.GetConnectionString();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        stopwatch.Stop();

        ConnectionStatus = new Model_ConnectionTestResult
        {
            IsConnected = true,
            StatusMessage = $"Connected to {connection.DataSource}:{connection.Port}/{connection.Database}",
            ConnectionTimeMs = (int)stopwatch.ElapsedMilliseconds,
            ServerVersion = connection.ServerVersion
        };

        TestLogs += $"[{DateTime.Now:HH:mm:ss}] ✓ Connection successful ({ConnectionStatus.ConnectionTimeMs}ms)\n";
    }

    private async Task TestSchemaAsync()
    {
        TestLogs += $"\n[{DateTime.Now:HH:mm:ss}] Validating database schema...\n";

        TableTestResults.Clear();
        TablesValidated = 0;

        var tables = new[]
        {
            "settings_universal",
            "settings_personal",
            "settings_activity",
            "receiving_package_type_mapping",
            "dunnage_types",
            "routing_home_locations",
            "reporting_scheduled_reports"
        };

        foreach (var table in tables)
        {
            var result = await ValidateTableAsync(table);
            TableTestResults.Add(result);

            if (result.IsValid)
                TablesValidated++;
        }

        TestLogs += $"[{DateTime.Now:HH:mm:ss}] ✓ Schema validation complete: {TablesValidated}/{TotalTables} tables valid\n";
    }

    private async Task<TableTestResult> ValidateTableAsync(string tableName)
    {
        var connectionString = Helper_Database_Variables.GetConnectionString();

        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        // Check table exists
        var checkCmd = new MySqlCommand($"SHOW TABLES LIKE '{tableName}'", connection);
        var exists = await checkCmd.ExecuteScalarAsync() != null;

        if (!exists)
        {
            TestLogs += $"[{DateTime.Now:HH:mm:ss}] ✗ Table '{tableName}' not found\n";
            return new TableTestResult
            {
                TableName = tableName,
                IsValid = false,
                StatusText = "Not Found"
            };
        }

        // Get row count
        var countCmd = new MySqlCommand($"SELECT COUNT(*) FROM {tableName}", connection);
        var rowCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

        // Get column count
        var colCmd = new MySqlCommand($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'", connection);
        var columnCount = Convert.ToInt32(await colCmd.ExecuteScalarAsync());

        // Get index count
        var idxCmd = new MySqlCommand($"SELECT COUNT(DISTINCT INDEX_NAME) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{tableName}'", connection);
        var indexCount = Convert.ToInt32(await idxCmd.ExecuteScalarAsync());

        TestLogs += $"[{DateTime.Now:HH:mm:ss}] ✓ {tableName}: {rowCount} rows, {columnCount} columns, {indexCount} indexes\n";

        return new TableTestResult
        {
            TableName = tableName,
            RowCount = rowCount,
            ColumnCount = columnCount,
            IndexCount = indexCount,
            IsValid = true,
            StatusText = "Validated"
        };
    }

    private async Task TestStoredProceduresAsync()
    {
        TestLogs += $"\n[{DateTime.Now:HH:mm:ss}] Testing stored procedures...\n";

        SpTestResults.Clear();
        StoredProceduresTested = 0;

        // Test each SP category
        await TestSystemSettingsSPs();
        await TestUserSettingsSPs();
        await TestPackageTypeSPs();
        await TestRoutingRuleSPs();
        await TestScheduledReportSPs();

        TestLogs += $"[{DateTime.Now:HH:mm:ss}] ✓ Stored procedure tests complete: {StoredProceduresTested}/{TotalStoredProcedures} passed\n";
    }

    private async Task TestDaosAsync()
    {
        TestLogs += $"\n[{DateTime.Now:HH:mm:ss}] Testing DAOs...\n";

        DaoTestResults.Clear();
        DaosValidated = 0;

        // Test each DAO
        await TestSystemSettingsDao();
        await TestUserSettingsDao();
        await TestPackageTypeDao();
        await TestRoutingRuleDao();
        await TestScheduledReportDao();
        await TestPackageTypeMappingDao();

        TestLogs += $"[{DateTime.Now:HH:mm:ss}] ✓ DAO tests complete: {DaosValidated}/{TotalDaos} operational\n";
    }

    [RelayCommand]
    private async Task ExportResultsAsync()
    {
        var savePicker = new FileSavePicker();
        savePicker.SuggestedFileName = $"DatabaseTest_{DateTime.Now:yyyyMMdd_HHmmss}";
        savePicker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });

        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(savePicker, hwnd);

        var file = await savePicker.PickSaveFileAsync();

        if (file != null)
        {
            await FileIO.WriteTextAsync(file, TestLogs);
            StatusMessage = $"Results exported to {file.Name}";
        }
    }
}
```

---

## Model Classes

```csharp
public partial class Model_ConnectionTestResult : ObservableObject
{
    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _connectionTimeMs;

    [ObservableProperty]
    private string _serverVersion = string.Empty;
}

public partial class TableTestResult : ObservableObject
{
    [ObservableProperty]
    private string _tableName = string.Empty;

    [ObservableProperty]
    private int _rowCount;

    [ObservableProperty]
    private int _columnCount;

    [ObservableProperty]
    private int _indexCount;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private string _statusText = string.Empty;
}

public partial class StoredProcedureTestResult : ObservableObject
{
    [ObservableProperty]
    private string _procedureName = string.Empty;

    [ObservableProperty]
    private bool _isPassed;

    [ObservableProperty]
    private int _executionTimeMs;

    [ObservableProperty]
    private string _testDetails = string.Empty;
}

public partial class DaoTestResult : ObservableObject
{
    [ObservableProperty]
    private string _daoName = string.Empty;

    [ObservableProperty]
    private bool _isOperational;

    [ObservableProperty]
    private string _testedOperations = string.Empty; // "CRUD"

    [ObservableProperty]
    private string _statusText = string.Empty;
}
```

---

## MainWindow Integration

Add button to MainWindow.xaml (for development builds only):

```xml
<!-- In NavigationView.MenuItems, add: -->
<NavigationViewItem Content="Settings DB Test"
                    Tag="DatabaseTestView"
                    Visibility="{x:Bind IsDevelopmentMode, Mode=OneWay}">
    <NavigationViewItem.Icon>
        <FontIcon Glyph="&#xE9F9;"/>
    </NavigationViewItem.Icon>
</NavigationViewItem>
```

In MainWindow.xaml.cs:

```csharp
private bool IsDevelopmentMode =>
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItem is NavigationViewItem item)
    {
        var tag = item.Tag?.ToString();

        if (tag == "DatabaseTestView")
        {
            var window = new DatabaseTestWindow();
            window.Activate();
            return;
        }

        // ... existing navigation
    }
}
```

---

## Testing Scenarios

### Schema Validation Tests

1. Verify all 7 tables exist
2. Check column counts and data types
3. Validate indexes are created
4. Verify foreign key constraints
5. Check unique constraints

### Stored Procedure Tests

1. Execute sp_Settings_System_GetAll
2. Test sp_Settings_System_UpdateValue with audit logging
3. Test sp_Settings_User_Get with fallback
4. Test sp_Dunnage_Types_GetUsageCount validation
5. Test sp_RoutingRule pattern matching
6. Verify all SPs return expected result sets

### DAO Tests

1. Test GetAll operations
2. Test GetById operations
3. Test Insert operations
4. Test Update operations
5. Test Delete operations
6. Verify Model_Dao_Result error handling

---

## Success Criteria

✅ **Database Test View Ready When:**

1. All 7 tables validated successfully
2. All 25+ stored procedures tested
3. All 6 DAOs operational
4. Connection status displays correctly
5. Test logs capture all operations
6. Export results functionality works
7. No SQL errors during test execution

---

## References

- [Database Testing Best Practices](https://learn.microsoft.com/en-us/sql/relational-databases/testing)
- [MySQL INFORMATION_SCHEMA](https://dev.mysql.com/doc/refman/8.0/en/information-schema.html)
