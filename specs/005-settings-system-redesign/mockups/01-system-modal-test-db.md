# System Settings - Database Connection Test

**SVG File**: `01-system-modal-test-db.svg`  
**Parent Page**: System Settings  
**Type**: ContentDialog (Informational)  
**Purpose**: Display MySQL database connection test results

---

## WinUI 3 Implementation

```xml
<ContentDialog
    Title="Database Connection Test"
    CloseButtonText="OK"
    DefaultButton="Close">
    
    <StackPanel Spacing="12">
        <!-- Status indicator -->
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
        
        <!-- Connection details -->
        <StackPanel Spacing="8" Visibility="{x:Bind TestResult.IsSuccess, Mode=OneWay}">
            <TextBlock>
                <Run Text="Host: "/><Run Text="{x:Bind TestResult.Host}" FontWeight="SemiBold"/>
            </TextBlock>
            <TextBlock>
                <Run Text="Database: "/><Run Text="{x:Bind TestResult.DatabaseName}" FontWeight="SemiBold"/>
            </TextBlock>
            <TextBlock>
                <Run Text="Connection time: "/><Run Text="{x:Bind TestResult.ConnectionTimeMs}" FontWeight="SemiBold"/><Run Text=" ms"/>
            </TextBlock>
        </StackPanel>
        
        <!-- Error details -->
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
public partial class Model_DatabaseTestResult : ObservableObject
{
    [ObservableProperty]
    private bool _isSuccess;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private string _host = string.Empty;
    
    [ObservableProperty]
    private string _databaseName = string.Empty;
    
    [ObservableProperty]
    private int _connectionTimeMs;
    
    [ObservableProperty]
    private string _errorMessage = string.Empty;
}
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task TestDatabaseConnectionAsync()
{
    IsBusy = true;
    StatusMessage = "Testing database connection...";
    
    try
    {
        var stopwatch = Stopwatch.StartNew();
        var connectionString = Helper_Database_Variables.GetConnectionString();
        
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        stopwatch.Stop();
        
        var result = new Model_DatabaseTestResult
        {
            IsSuccess = true,
            StatusMessage = "Connection Successful",
            Host = $"{connection.DataSource}:{connection.Port}",
            DatabaseName = connection.Database,
            ConnectionTimeMs = (int)stopwatch.ElapsedMilliseconds
        };
        
        IsConnected = true;
        StatusMessage = "✓ Connected to MySQL database";
        
        // Show result dialog
        var dialog = new DatabaseTestDialog(result)
        {
            XamlRoot = _xamlRoot
        };
        await dialog.ShowAsync();
    }
    catch (MySqlException ex)
    {
        var result = new Model_DatabaseTestResult
        {
            IsSuccess = false,
            StatusMessage = "Connection Failed",
            ErrorMessage = GetFriendlyMySqlError(ex)
        };
        
        IsConnected = false;
        StatusMessage = "Connection failed";
        
        var dialog = new DatabaseTestDialog(result)
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
            nameof(TestDatabaseConnectionAsync),
            nameof(SystemSettingsViewModel)
        );
    }
    finally
    {
        IsBusy = false;
    }
}

private string GetFriendlyMySqlError(MySqlException ex)
{
    return ex.Number switch
    {
        0 => "Unable to connect to MySQL server. Check if server is running.",
        1042 => "Cannot resolve hostname. Check server address.",
        1045 => "Access denied. Check username and password.",
        1049 => $"Database '{ex.Message}' does not exist.",
        2002 => "Cannot connect to MySQL server on specified host.",
        2003 => "Cannot connect to MySQL server. Connection refused.",
        _ => $"MySQL Error {ex.Number}: {ex.Message}"
    };
}
```

---

## Common MySQL Connection Errors

| Error Code | Message | User-Friendly |
|------------|---------|---------------|
| 0 | Connection failure | MySQL server may be stopped |
| 1042 | Cannot resolve host | Invalid server address |
| 1045 | Access denied | Invalid username/password |
| 1049 | Unknown database | Database does not exist |
| 2002 | Connection refused | Server not listening on port |
| 2003 | Can't connect | Firewall or network issue |

---

## References

- [MySqlConnection Class](https://dev.mysql.com/doc/connector-net/en/connector-net-connections.html)
- [MySQL Error Codes](https://dev.mysql.com/doc/mysql-errors/8.0/en/server-error-reference.html)
