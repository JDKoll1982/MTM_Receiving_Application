# ERP Integration - Connection Test Result

**SVG File**: `03-erp-modal-test-connection.svg`  
**Parent Page**: ERP Integration Settings  
**Type**: ContentDialog (Informational)  
**Purpose**: Display Infor Visual connection test results

---

## WinUI 3 Implementation

```xml
<ContentDialog
    Title="Connection Test"
    CloseButtonText="OK"
    DefaultButton="Close">
    
    <StackPanel Spacing="12">
        <!-- Success/Failure indicator -->
        <Grid ColumnDefinitions="Auto,*">
            <FontIcon 
                Grid.Column="0"
                Glyph="{x:Bind TestResult.IsSuccess, Converter={StaticResource BoolToGlyphConverter}}"
                FontSize="24"
                Foreground="{x:Bind TestResult.IsSuccess, Converter={StaticResource BoolToColorConverter}}"
                Margin="0,0,12,0"/>
            
            <TextBlock 
                Grid.Column="1"
                Text="{x:Bind TestResult.StatusMessage}"
                FontSize="16"
                FontWeight="SemiBold"
                VerticalAlignment="Center"
                Foreground="{x:Bind TestResult.IsSuccess, Converter={StaticResource BoolToColorConverter}}"/>
        </Grid>
        
        <!-- Connection details (success only) -->
        <StackPanel Spacing="8" Visibility="{x:Bind TestResult.IsSuccess, Mode=OneWay}">
            <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                <Run Text="Server: "/><Run Text="{x:Bind TestResult.ServerName}" FontWeight="SemiBold"/>
            </TextBlock>
            <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                <Run Text="Database: "/><Run Text="{x:Bind TestResult.DatabaseName}" FontWeight="SemiBold"/>
            </TextBlock>
            <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                <Run Text="Connection time: "/><Run Text="{x:Bind TestResult.ConnectionTimeMs}" FontWeight="SemiBold"/><Run Text=" ms"/>
            </TextBlock>
            <TextBlock Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                <Run Text="Authentication: "/><Run Text="{x:Bind TestResult.AuthenticationMode}" FontWeight="SemiBold"/>
            </TextBlock>
        </StackPanel>
        
        <!-- Error details (failure only) -->
        <Border 
            Background="#fff4e5"
            BorderBrush="#f0ad4e"
            BorderThickness="1"
            CornerRadius="4"
            Padding="12"
            Visibility="{x:Bind TestResult.IsSuccess, Converter={StaticResource InverseBoolToVisibilityConverter}, Mode=OneWay}">
            <TextBlock 
                Text="{x:Bind TestResult.ErrorMessage}"
                TextWrapping="Wrap"
                Foreground="#856404"/>
        </Border>
    </StackPanel>
</ContentDialog>
```

---

## Test Result Model

```csharp
public partial class Model_ConnectionTestResult : ObservableObject
{
    [ObservableProperty]
    private bool _isSuccess;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private string _serverName = string.Empty;
    
    [ObservableProperty]
    private string _databaseName = string.Empty;
    
    [ObservableProperty]
    private int _connectionTimeMs;
    
    [ObservableProperty]
    private string _authenticationMode = string.Empty;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
}
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task TestConnectionAsync()
{
    IsBusy = true;
    StatusMessage = "Testing connection...";
    
    try
    {
        var stopwatch = Stopwatch.StartNew();
        var testResult = await _inforVisualService.TestConnectionAsync();
        stopwatch.Stop();
        
        var result = new Model_ConnectionTestResult
        {
            IsSuccess = testResult.IsSuccess,
            ServerName = InforVisualServer,
            DatabaseName = InforVisualDatabase,
            ConnectionTimeMs = (int)stopwatch.ElapsedMilliseconds,
            AuthenticationMode = InforVisualUseWindowsAuth 
                ? $"Windows ({Environment.UserDomainName}\\{Environment.UserName})"
                : "SQL Server"
        };
        
        if (testResult.IsSuccess)
        {
            result.StatusMessage = "Connection Successful";
            IsConnected = true;
            StatusMessage = "✓ Connected to Infor Visual";
        }
        else
        {
            result.StatusMessage = "Connection Failed";
            result.ErrorMessage = testResult.ErrorMessage;
            IsConnected = false;
            StatusMessage = "Connection failed";
        }
        
        // Show result dialog
        var dialog = new ConnectionTestDialog(result)
        {
            XamlRoot = _xamlRoot
        };
        await dialog.ShowAsync();
    }
    catch (Exception ex)
    {
        _errorHandler.HandleException(
            ex,
            Enum_ErrorSeverity.Medium,
            nameof(TestConnectionAsync),
            nameof(ErpIntegrationViewModel)
        );
    }
    finally
    {
        IsBusy = false;
    }
}
```

---

## Service Implementation

```csharp
public class InforVisualService : IInforVisualService
{
    public async Task<Model_Dao_Result> TestConnectionAsync()
    {
        try
        {
            var connectionString = GetConnectionString();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Test query
            using var command = new SqlCommand("SELECT @@VERSION", connection);
            var version = await command.ExecuteScalarAsync();
            
            return Model_Dao_Result.Success();
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result.Failure(GetFriendlyErrorMessage(ex));
        }
        catch (Exception ex)
        {
            return Model_Dao_Result.Failure($"Connection error: {ex.Message}");
        }
    }
    
    private string GetFriendlyErrorMessage(SqlException ex)
    {
        return ex.Number switch
        {
            -1 => "Connection timeout. Server may be unreachable.",
            -2 => "Timeout expired during connection attempt.",
            2 => "Server not found or not accessible.",
            4060 => "Database does not exist or access denied.",
            18456 => "Login failed. Check authentication settings.",
            _ => $"SQL Error {ex.Number}: {ex.Message}"
        };
    }
}
```

---

## Common Connection Errors

| Error Code | Message | User-Friendly |
|------------|---------|---------------|
| -1 | Timeout | Server may be offline or unreachable |
| 2 | Network error | Cannot find server VISUAL |
| 4060 | Invalid database | Database MTMFG does not exist |
| 18456 | Login failed | Invalid credentials or permissions |
| 18452 | SQL Auth disabled | Windows Authentication required |

---

## References

- [SqlConnection.Open](https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlconnection.open)
- [SQL Server Error Codes](https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors)
