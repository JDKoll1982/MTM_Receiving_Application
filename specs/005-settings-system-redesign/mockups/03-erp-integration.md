# ERP Integration Settings - WinUI 3 Controls

**SVG File**: `03-erp-integration.svg`  
**Category**: ERP Integration  
**Purpose**: Infor Visual database connection and performance configuration

---

## WinUI 3 Controls Used

### 1. **Database Connection Section**

#### SQL Server Name - `TextBox`

```xml
<TextBox 
    Header="SQL Server Name"
    Text="{x:Bind ViewModel.InforVisualServer, Mode=TwoWay}"
    PlaceholderText="Enter server name"
    Width="350"
    HorizontalAlignment="Left">
    <TextBox.Description>
        <TextBlock Text="Server hosting Infor Visual database"/>
    </TextBox.Description>
</TextBox>
```

**Properties**:

- Data type: `string`
- Default: `"VISUAL"`
- Permission: Super Admin

---

#### Database Name - `TextBox`

```xml
<TextBox 
    Header="Database Name"
    Text="{x:Bind ViewModel.InforVisualDatabase, Mode=TwoWay}"
    PlaceholderText="Enter database name"
    Width="350"
    HorizontalAlignment="Left">
    <TextBox.Description>
        <TextBlock Text="Infor Visual database name"/>
    </TextBox.Description>
</TextBox>
```

**Properties**:

- Data type: `string`
- Default: `"MTMFG"`
- Permission: Super Admin

---

#### Warehouse/Site Code - `TextBox`

```xml
<TextBox 
    Header="Warehouse/Site Code"
    Text="{x:Bind ViewModel.InforVisualWarehouse, Mode=TwoWay}"
    PlaceholderText="e.g., 002"
    Width="150"
    HorizontalAlignment="Left"
    MaxLength="10">
    <TextBox.Description>
        <TextBlock Text="Default site reference for queries"/>
    </TextBox.Description>
</TextBox>
```

**Properties**:

- Data type: `string`
- Default: `"002"`
- Permission: Admin
- Validation: Required for all ERP queries

---

#### Use Windows Authentication - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Use Windows Authentication"
    IsOn="{x:Bind ViewModel.InforVisualUseWindowsAuth, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Connect using current Windows credentials"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Super Admin
- **Locked**: Cannot be disabled (security requirement)

---

#### Test Connection Button - `Button` with Status Icon

```xml
<StackPanel Orientation="Horizontal" Spacing="12">
    <Button 
        Content="Test Connection"
        Command="{x:Bind ViewModel.TestConnectionCommand}"
        Style="{StaticResource AccentButtonStyle}"
        Background="{ThemeResource SystemFillColorSuccessBrush}"
        Width="160"
        Height="36"/>
    
    <!-- Status Indicator -->
    <StackPanel Orientation="Horizontal" Spacing="8" VerticalAlignment="Center">
        <FontIcon 
            Glyph="&#xE73E;"
            FontSize="16"
            Foreground="{ThemeResource SystemFillColorSuccessBrush}"
            Visibility="{x:Bind ViewModel.IsConnected, Mode=OneWay}"/>
        <TextBlock 
            Text="Connected"
            Foreground="{ThemeResource SystemFillColorSuccessBrush}"
            Visibility="{x:Bind ViewModel.IsConnected, Mode=OneWay}"/>
    </StackPanel>
</StackPanel>
```

**Command Implementation**:

```csharp
[RelayCommand]
private async Task TestConnectionAsync()
{
    IsBusy = true;
    StatusMessage = "Testing connection...";
    
    var result = await _inforVisualService.TestConnectionAsync();
    
    IsConnected = result.IsSuccess;
    
    if (result.IsSuccess)
    {
        StatusMessage = "✓ Connected successfully";
    }
    else
    {
        _errorHandler.ShowUserError(
            result.ErrorMessage,
            "Connection Failed",
            nameof(TestConnectionAsync)
        );
        StatusMessage = "Connection failed";
    }
    
    IsBusy = false;
}
```

---

### 2. **Performance & Caching Section**

#### Query Timeout - `NumberBox`

```xml
<NumberBox 
    Header="Query Timeout (seconds)"
    Value="{x:Bind ViewModel.InforVisualQueryTimeoutSeconds, Mode=TwoWay}"
    Minimum="5"
    Maximum="120"
    SpinButtonPlacementMode="Inline"
    SmallChange="5"
    LargeChange="15"
    Width="150"
    HorizontalAlignment="Left">
    <NumberBox.Description>
        <TextBlock Text="Maximum time for ERP queries (5-120 seconds)"/>
    </NumberBox.Description>
</NumberBox>
```

**Properties**:

- Data type: `int`
- Range: 5-120 seconds
- Default: `30`
- Permission: Developer

---

#### Enable Query Caching - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable Query Caching"
    IsOn="{x:Bind ViewModel.InforVisualEnableCaching, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Cache frequently accessed ERP data"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Developer

---

#### Cache Duration - `NumberBox`

```xml
<NumberBox 
    Header="Cache Duration (minutes)"
    Value="{x:Bind ViewModel.InforVisualCacheDurationMinutes, Mode=TwoWay}"
    Minimum="1"
    Maximum="60"
    SpinButtonPlacementMode="Inline"
    Width="150"
    HorizontalAlignment="Left"
    IsEnabled="{x:Bind ViewModel.InforVisualEnableCaching, Mode=OneWay}">
    <NumberBox.Description>
        <TextBlock Text="How long to cache ERP data (1-60 minutes)"/>
    </NumberBox.Description>
</NumberBox>
```

**Properties**:

- Data type: `int`
- Range: 1-60 minutes
- Default: `15`
- Permission: Developer
- **Disabled** when caching is off

---

#### Clear Cache Button - `Button`

```xml
<Button 
    Content="Clear Cache Now"
    Command="{x:Bind ViewModel.ClearCacheCommand}"
    Width="140"
    IsEnabled="{x:Bind ViewModel.InforVisualEnableCaching, Mode=OneWay}"/>
```

---

### 3. **Read-Only Warning - InfoBar**

```xml
<InfoBar 
    Severity="Warning"
    IsOpen="True"
    IsClosable="False"
    Title="⚠ READ-ONLY DATABASE"
    Width="920">
    <InfoBar.Message>
        <StackPanel Spacing="4">
            <TextBlock TextWrapping="Wrap">
                The Infor Visual database is configured for READ-ONLY access. Only SELECT queries are permitted.
            </TextBlock>
            <TextBlock TextWrapping="Wrap">
                All write operations (INSERT/UPDATE/DELETE) are blocked to prevent data corruption in the ERP system.
            </TextBlock>
            <TextBlock FontWeight="SemiBold" Margin="0,4,0,0">
                Connection String: ApplicationIntent=ReadOnly
            </TextBlock>
        </StackPanel>
    </InfoBar.Message>
</InfoBar>
```

**Alternative using Border**:

```xml
<Border 
    Background="#fff4e5"
    BorderBrush="#f0ad4e"
    BorderThickness="1"
    CornerRadius="4"
    Padding="20"
    Width="920">
    <StackPanel Spacing="8">
        <TextBlock 
            Text="⚠ READ-ONLY DATABASE"
            FontWeight="SemiBold"
            Foreground="#856404"/>
        <TextBlock 
            TextWrapping="Wrap"
            Foreground="#856404">
            The Infor Visual database is configured for READ-ONLY access...
        </TextBlock>
    </StackPanel>
</Border>
```

---

## Connection String Builder

```csharp
public string GetInforVisualConnectionString()
{
    var builder = new SqlConnectionStringBuilder
    {
        DataSource = InforVisualServer,
        InitialCatalog = InforVisualDatabase,
        IntegratedSecurity = InforVisualUseWindowsAuth,
        ApplicationIntent = ApplicationIntent.ReadOnly,
        TrustServerCertificate = true,
        ConnectTimeout = 15,
        ApplicationName = "MTM_Receiving_Application"
    };
    
    return builder.ConnectionString;
}
```

---

## ViewModel

```csharp
public partial class ErpIntegrationViewModel : BaseViewModel
{
    // Connection
    [ObservableProperty]
    private string _inforVisualServer = "VISUAL";
    
    [ObservableProperty]
    private string _inforVisualDatabase = "MTMFG";
    
    [ObservableProperty]
    private string _inforVisualWarehouse = "002";
    
    [ObservableProperty]
    private bool _inforVisualUseWindowsAuth = false;
    
    [ObservableProperty]
    private bool _isConnected = false;
    
    // Performance
    [ObservableProperty]
    private int _inforVisualQueryTimeoutSeconds = 30;
    
    [ObservableProperty]
    private bool _inforVisualEnableCaching = true;
    
    [ObservableProperty]
    private int _inforVisualCacheDurationMinutes = 15;
    
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        // Implementation above
    }
    
    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        await _cacheService.ClearInforVisualCacheAsync();
        StatusMessage = "Cache cleared";
    }
}
```

---

## Validation

### Warehouse Code Validation

```csharp
partial void OnInforVisualWarehouseChanged(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        ValidationErrors["InforVisualWarehouse"] = "Warehouse code is required";
        return;
    }
    
    // Must be 3-digit code
    if (!Regex.IsMatch(value, @"^\d{3}$"))
    {
        ValidationErrors["InforVisualWarehouse"] = "Must be 3-digit code (e.g., 002)";
        return;
    }
    
    ValidationErrors.Remove("InforVisualWarehouse");
}
```

---

## Cache Service Implementation

```csharp
public class InforVisualCacheService : IInforVisualCacheService
{
    private readonly MemoryCache _cache;
    private readonly TimeSpan _defaultDuration;
    
    public InforVisualCacheService(int durationMinutes)
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _defaultDuration = TimeSpan.FromMinutes(durationMinutes);
    }
    
    public async Task<T?> GetOrFetchAsync<T>(
        string key,
        Func<Task<T>> fetchFunc)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }
        
        var value = await fetchFunc();
        
        _cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _defaultDuration
        });
        
        return value;
    }
    
    public void Clear()
    {
        if (_cache is MemoryCache mc)
        {
            mc.Compact(1.0);
        }
    }
}
```

---

## Usage Example

```csharp
// Query with caching
public async Task<Model_Dao_Result<List<Model_PurchaseOrder>>> GetPurchaseOrdersAsync()
{
    if (InforVisualEnableCaching)
    {
        var cached = await _cacheService.GetOrFetchAsync(
            "PurchaseOrders_Active",
            async () => await FetchPurchaseOrdersFromDatabaseAsync()
        );
        
        return Model_Dao_Result<List<Model_PurchaseOrder>>.Success(cached);
    }
    
    return await FetchPurchaseOrdersFromDatabaseAsync();
}
```

---

## Permissions

| Setting | User | Operator | Admin | Developer | Super Admin |
|---------|------|----------|-------|-----------|-------------|
| Server Name | ❌ | ❌ | ❌ | ❌ | ✅ |
| Database Name | ❌ | ❌ | ❌ | ❌ | ✅ |
| Warehouse Code | ❌ | ❌ | ✅ | ✅ | ✅ |
| Windows Auth | ❌ | ❌ | ❌ | ❌ | ✅ (Locked) |
| Test Connection | ❌ | ❌ | ✅ | ✅ | ✅ |
| Query Timeout | ❌ | ❌ | ❌ | ✅ | ✅ |
| Enable Caching | ❌ | ❌ | ❌ | ✅ | ✅ |
| Cache Duration | ❌ | ❌ | ❌ | ✅ | ✅ |
| Clear Cache | ❌ | ❌ | ❌ | ✅ | ✅ |

---

## Accessibility

```xml
<TextBox 
    AutomationProperties.Name="SQL Server Name"
    AutomationProperties.HelpText="Enter the server hosting Infor Visual database"/>

<Button 
    AutomationProperties.Name="Test Database Connection"
    AutomationProperties.HelpText="Verify connection to Infor Visual ERP system"/>
```

---

## References

- [SQL Server Connection Strings](https://www.connectionstrings.com/sql-server/)
- [ApplicationIntent=ReadOnly](https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.applicationintent)
- [WinUI 3 Gallery - InfoBar](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/InfoBarPage.xaml)
