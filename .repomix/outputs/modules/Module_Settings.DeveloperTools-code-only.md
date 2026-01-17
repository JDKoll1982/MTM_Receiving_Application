# User Provided Header
MTM Receiving Application - Module_Settings.DeveloperTools Code-Only Export

# Files

## File: Module_Settings.DeveloperTools/Data/Dao_SettingsDiagnostics.cs
```csharp
public class Dao_SettingsDiagnostics
⋮----
public Task<Model_Dao_Result<List<string>>> GetTablesAsync()
⋮----
return Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
reader => reader.GetString(reader.GetOrdinal("table_name")));
⋮----
public Task<Model_Dao_Result<List<string>>> GetStoredProceduresAsync()
⋮----
reader => reader.GetString(reader.GetOrdinal("procedure_name")));
```

## File: Module_Settings.DeveloperTools/Models/Model_SettingsDbDaoResult.cs
```csharp
public class Model_SettingsDbDaoResult
```

## File: Module_Settings.DeveloperTools/Models/Model_SettingsDbProcedureResult.cs
```csharp
public class Model_SettingsDbProcedureResult
```

## File: Module_Settings.DeveloperTools/Models/Model_SettingsDbTableResult.cs
```csharp
public class Model_SettingsDbTableResult
```

## File: Module_Settings.DeveloperTools/Models/Model_SettingsDbTestReport.cs
```csharp
public class Model_SettingsDbTestReport
```

## File: Module_Settings.DeveloperTools/Services/RunSettingsDbTestCommand.cs
```csharp

```

## File: Module_Settings.DeveloperTools/Services/RunSettingsDbTestCommandHandler.cs
```csharp
public class RunSettingsDbTestCommandHandler : IRequestHandler<RunSettingsDbTestCommand, Model_Dao_Result<Model_SettingsDbTestReport>>
⋮----
private readonly Dao_SettingsDiagnostics _diagnosticsDao;
private readonly Dao_SettingsCoreSystem _systemDao;
private readonly Dao_SettingsCoreUser _userDao;
private readonly Dao_SettingsCoreAudit _auditDao;
⋮----
public async Task<Model_Dao_Result<Model_SettingsDbTestReport>> Handle(RunSettingsDbTestCommand request, CancellationToken cancellationToken)
⋮----
var report = new Model_SettingsDbTestReport
⋮----
var stopwatch = Stopwatch.StartNew();
await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
await connection.OpenAsync(cancellationToken);
stopwatch.Stop();
⋮----
var tablesResult = await _diagnosticsDao.GetTablesAsync();
var proceduresResult = await _diagnosticsDao.GetStoredProceduresAsync();
⋮----
var exists = tableSet.Contains(table);
report.TableResults.Add(new Model_SettingsDbTableResult
⋮----
report.TablesValidated = report.TableResults.Count(r => r.IsValid);
⋮----
var exists = procSet.Contains(proc);
report.ProcedureResults.Add(new Model_SettingsDbProcedureResult
⋮----
report.ProceduresValidated = report.ProcedureResults.Count(r => r.IsValid);
var systemDaoResult = await _systemDao.GetByCategoryAsync("System");
report.DaoResults.Add(new Model_SettingsDbDaoResult
⋮----
var userDaoResult = await _userDao.GetByCategoryAsync(1, "User");
⋮----
var auditDaoResult = await _auditDao.GetByUserAsync(1);
⋮----
report.DaosValidated = report.DaoResults.Count(r => r.IsValid);
return Model_Dao_Result_Factory.Success(report);
```

## File: Module_Settings.DeveloperTools/ViewModels/ViewModel_SettingsDeveloperTools_DatabaseTest.cs
```csharp
public partial class ViewModel_SettingsDeveloperTools_DatabaseTest : ViewModel_Shared_Base
⋮----
private readonly IMediator _mediator;
⋮----
private async Task RunAllTestsAsync()
⋮----
var result = await _mediator.Send(new RunSettingsDbTestCommand());
⋮----
_errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(RunAllTestsAsync), nameof(ViewModel_SettingsDeveloperTools_DatabaseTest));
```

## File: Module_Settings.DeveloperTools/Views/View_SettingsDeveloperTools_DatabaseTest.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Settings.DeveloperTools.Views.View_SettingsDeveloperTools_DatabaseTest"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:viewmodels="using:MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels"
    xmlns:models="using:MTM_Receiving_Application.Module_Settings.DeveloperTools.Models">

    <Grid Padding="20" RowSpacing="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <StackPanel Orientation="Horizontal" Spacing="12">
            <TextBlock Text="Settings Database Test" Style="{StaticResource TitleTextBlockStyle}" />
            <Button Content="Run Tests"
                    Command="{x:Bind ViewModel.RunAllTestsCommand}" />
        </StackPanel>

        <Pivot Grid.Row="1">
            <PivotItem Header="Summary">
                <StackPanel Spacing="8">
                    <TextBlock Text="Connection" Style="{StaticResource SubtitleTextBlockStyle}" />
                    <TextBlock Text="{x:Bind ViewModel.ConnectionStatus, Mode=OneWay}" />
                    <TextBlock Text="Server Version:" />
                    <TextBlock Text="{x:Bind ViewModel.ServerVersion, Mode=OneWay}" />
                    <TextBlock Text="Tables Validated:" />
                    <TextBlock Text="{x:Bind ViewModel.TablesValidated, Mode=OneWay}" />
                    <TextBlock Text="Procedures Validated:" />
                    <TextBlock Text="{x:Bind ViewModel.ProceduresValidated, Mode=OneWay}" />
                    <TextBlock Text="DAOs Validated:" />
                    <TextBlock Text="{x:Bind ViewModel.DaosValidated, Mode=OneWay}" />
                </StackPanel>
            </PivotItem>
            <PivotItem Header="Tables">
                <ItemsControl ItemsSource="{x:Bind ViewModel.TableResults, Mode=OneWay}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate x:DataType="models:Model_SettingsDbTableResult">
                            <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,4,0,4">
                                <TextBlock Text="{x:Bind TableName}" Width="240" />
                                <TextBlock Text="{x:Bind Details}" />
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </PivotItem>
            <PivotItem Header="Stored Procedures">
                <ItemsControl ItemsSource="{x:Bind ViewModel.ProcedureResults, Mode=OneWay}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate x:DataType="models:Model_SettingsDbProcedureResult">
                            <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,4,0,4">
                                <TextBlock Text="{x:Bind ProcedureName}" Width="320" />
                                <TextBlock Text="{x:Bind Details}" />
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </PivotItem>
            <PivotItem Header="DAOs">
                <ItemsControl ItemsSource="{x:Bind ViewModel.DaoResults, Mode=OneWay}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate x:DataType="models:Model_SettingsDbDaoResult">
                            <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,4,0,4">
                                <TextBlock Text="{x:Bind DaoName}" Width="240" />
                                <TextBlock Text="{x:Bind Details}" />
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </PivotItem>
        </Pivot>
    </Grid>
</Page>
```

## File: Module_Settings.DeveloperTools/Views/View_SettingsDeveloperTools_DatabaseTest.xaml.cs
```csharp
public sealed partial class View_SettingsDeveloperTools_DatabaseTest : Page
⋮----
private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await ViewModel.RunAllTestsCommand.ExecuteAsync(null);
```
