# Security & Session Settings - WinUI 3 Controls

**SVG File**: `02-security-session.svg`
**Category**: Security & Session
**Purpose**: Authentication, session management, and encryption configuration

---

## WinUI 3 Controls Used

### 1. **Authentication Section**

#### Require Authentication - `ToggleSwitch`

```xml
<ToggleSwitch
    Header="Require User Authentication"
    IsOn="{x:Bind ViewModel.RequireAuth, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Force users to sign in before accessing the application"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Admin
- Database: `settings_universal.RequireAuth`

---

#### Remember Credentials - `ToggleSwitch`

```xml
<ToggleSwitch
    Header="Remember User Credentials"
    IsOn="{x:Bind ViewModel.RememberUserCredentials, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Allow users to stay signed in across sessions"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Admin

---

### 2. **Session Management Section**

#### Idle Timeout - `NumberBox`

```xml
<NumberBox
    Header="Idle Timeout (minutes)"
    Value="{x:Bind ViewModel.SessionIdleTimeoutMinutes, Mode=TwoWay}"
    Minimum="5"
    Maximum="120"
    SpinButtonPlacementMode="Inline"
    SmallChange="5"
    LargeChange="15"
    Width="150"
    HorizontalAlignment="Left">
    <NumberBox.Description>
        <TextBlock Text="Auto-logout after period of inactivity (5-120 minutes)"/>
    </NumberBox.Description>
</NumberBox>
```

**Properties**:

- Data type: `int`
- Range: 5-120 minutes
- Default: `30`
- Permission: Admin
- Validation: Must be multiple of 5

---

#### Show Timeout Warning - `ToggleSwitch`

```xml
<ToggleSwitch
    Header="Show Timeout Warning"
    IsOn="{x:Bind ViewModel.SessionShowWarning, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Display warning 2 minutes before automatic logout"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Admin

---

### 3. **Data Encryption Section**

#### Master Encryption Key - `PasswordBox`

```xml
<PasswordBox
    Header="Master Encryption Key"
    Password="{x:Bind ViewModel.EncryptionMasterKey, Mode=TwoWay}"
    PasswordRevealMode="Peek"
    Width="350"
    HorizontalAlignment="Left"
    IsPasswordRevealButtonEnabled="True">
    <PasswordBox.Description>
        <TextBlock Text="AES-256 encryption key for sensitive data"/>
    </PasswordBox.Description>
</PasswordBox>
```

**Properties**:

- Data type: `string` (encrypted)
- Default: System-generated key
- Permission: Super Admin
- Validation: Must be valid AES-256 key
- **Security**: Stored encrypted with DPAPI

**Important**: PasswordRevealMode options:

- `Hidden` - No reveal button
- `Peek` - Hold to reveal (default)
- `Visible` - Always visible (not recommended for sensitive data)

---

#### Encrypt Passwords - `ToggleSwitch`

```xml
<ToggleSwitch
    Header="Encrypt Stored Passwords"
    IsOn="{x:Bind ViewModel.EncryptPasswords, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Use AES-256 + DPAPI for password storage"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Super Admin
- **Locked**: Cannot be disabled

---

#### Rotate Encryption Key - `Button`

```xml
<Button
    Content="Rotate Encryption Key"
    Command="{x:Bind ViewModel.RotateEncryptionKeyCommand}"
    Style="{StaticResource DefaultButtonStyle}"
    BorderBrush="{ThemeResource SystemFillColorCriticalBrush}"
    Foreground="{ThemeResource SystemFillColorCriticalBrush}"
    Width="160"/>
```

**Behavior**:

- Opens confirmation dialog
- Re-encrypts all sensitive data
- Generates new key
- Cannot be undone
- Requires Super Admin permission

---

### 4. **Warning/Info Card - Custom Control**

```xml
<Border
    Background="#fff4e5"
    BorderBrush="#f0ad4e"
    BorderThickness="1"
    CornerRadius="4"
    Padding="16"
    Width="400">
    <StackPanel Spacing="8">
        <TextBlock
            Text="⚠ Security Notice"
            FontWeight="SemiBold"
            Foreground="#856404"/>
        <TextBlock
            TextWrapping="Wrap"
            Foreground="#856404">
            <Run Text="Changing the encryption key will re-encrypt all"/>
            <LineBreak/>
            <Run Text="sensitive data. This operation cannot be undone."/>
        </TextBlock>
    </StackPanel>
</Border>
```

**Alternative using InfoBar**:

```xml
<InfoBar
    Severity="Warning"
    IsOpen="True"
    IsClosable="False"
    Title="Security Notice"
    Message="Changing the encryption key will re-encrypt all sensitive data. This operation cannot be undone."/>
```

---

## Layout Structure

```xml
<Grid ColumnDefinitions="*,*" ColumnSpacing="40" Padding="40,20">
    <!-- Left Column -->
    <StackPanel Grid.Column="0" Spacing="32">
        <!-- Authentication Section -->
        <StackPanel Spacing="16">
            <TextBlock Text="Authentication"
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
            <ToggleSwitch ... />
            <ToggleSwitch ... />
        </StackPanel>

        <!-- Session Management Section -->
        <StackPanel Spacing="16">
            <TextBlock Text="Session Management"
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
            <NumberBox ... />
            <ToggleSwitch ... />
        </StackPanel>
    </StackPanel>

    <!-- Right Column -->
    <StackPanel Grid.Column="1" Spacing="16">
        <TextBlock Text="Data Encryption"
                   Style="{StaticResource SubtitleTextBlockStyle}"
                   Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
        <PasswordBox ... />
        <ToggleSwitch ... />
        <Button ... />
        <InfoBar ... />
    </StackPanel>
</Grid>
```

---

## ViewModel

```csharp
public partial class SecuritySessionViewModel : BaseViewModel
{
    // Authentication
    [ObservableProperty]
    private bool _requireAuth = true;

    [ObservableProperty]
    private bool _rememberUserCredentials = true;

    // Session Management
    [ObservableProperty]
    private int _sessionIdleTimeoutMinutes = 30;

    [ObservableProperty]
    private bool _sessionShowWarning = true;

    // Encryption
    [ObservableProperty]
    private string _encryptionMasterKey = string.Empty;

    [ObservableProperty]
    private bool _encryptPasswords = true;

    [RelayCommand]
    private async Task RotateEncryptionKeyAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Confirm Key Rotation",
            Content = "This will re-encrypt all sensitive data with a new key. Continue?",
            PrimaryButtonText = "Rotate Key",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            await _encryptionService.RotateKeyAsync();
        }
    }
}
```

---

## Validation

### Session Timeout Validation

```csharp
partial void OnSessionIdleTimeoutMinutesChanged(int value)
{
    // Enforce 5-minute increments
    if (value % 5 != 0)
    {
        SessionIdleTimeoutMinutes = (value / 5) * 5;
    }

    // Enforce range
    if (value < 5) SessionIdleTimeoutMinutes = 5;
    if (value > 120) SessionIdleTimeoutMinutes = 120;
}
```

### Encryption Key Validation

```csharp
partial void OnEncryptionMasterKeyChanged(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        ValidationErrors["EncryptionMasterKey"] = "Encryption key is required";
        return;
    }

    if (!IsValidAES256Key(value))
    {
        ValidationErrors["EncryptionMasterKey"] = "Invalid AES-256 key format";
        return;
    }

    ValidationErrors.Remove("EncryptionMasterKey");
}

private bool IsValidAES256Key(string key)
{
    // Validate AES-256 key format (32 bytes/256 bits)
    return key.Length == 44 && Convert.TryFromBase64String(key, new byte[32], out _);
}
```

---

## Security Implementation

### Password Encryption Service

```csharp
public class EncryptionService : IEncryptionService
{
    public string EncryptPassword(string plaintext, string masterKey)
    {
        // 1. Use AES-256 with master key
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(masterKey);
        aes.GenerateIV();

        var encrypted = aes.CreateEncryptor().TransformFinalBlock(
            Encoding.UTF8.GetBytes(plaintext), 0, plaintext.Length);

        // 2. Apply DPAPI for additional Windows security
        var protectedData = ProtectedData.Protect(
            encrypted,
            null,
            DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(protectedData);
    }
}
```

### Session Timeout Monitor

```csharp
public class SessionTimeoutService : ISessionTimeoutService
{
    private DispatcherTimer _idleTimer;
    private DateTime _lastActivity;

    public void StartMonitoring(int timeoutMinutes, bool showWarning)
    {
        _idleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _idleTimer.Tick += CheckIdleTimeout;
        _idleTimer.Start();

        // Monitor user activity
        App.Current.Window.Content.PointerMoved += ResetIdleTimer;
        App.Current.Window.Content.KeyDown += ResetIdleTimer;
    }

    private void CheckIdleTimeout(object sender, object e)
    {
        var elapsed = DateTime.Now - _lastActivity;

        if (elapsed.TotalMinutes >= _timeoutMinutes - 2 && _showWarning)
        {
            ShowWarningDialog();
        }
        else if (elapsed.TotalMinutes >= _timeoutMinutes)
        {
            LogoutUser();
        }
    }
}
```

---

## Permissions

| Setting | User | Operator | Admin | Developer | Super Admin |
|---------|------|----------|-------|-----------|-------------|
| Require Auth | ❌ | ❌ | ✅ | ✅ | ✅ |
| Remember Credentials | ❌ | ❌ | ✅ | ✅ | ✅ |
| Idle Timeout | ❌ | ❌ | ✅ | ✅ | ✅ |
| Show Warning | ❌ | ❌ | ✅ | ✅ | ✅ |
| Master Key | ❌ | ❌ | ❌ | ❌ | ✅ |
| Encrypt Passwords | ❌ | ❌ | ❌ | ❌ | ✅ (Locked) |
| Rotate Key | ❌ | ❌ | ❌ | ❌ | ✅ |

---

## Accessibility

```xml
<!-- PasswordBox -->
<PasswordBox
    AutomationProperties.Name="Master Encryption Key"
    AutomationProperties.HelpText="Enter AES-256 encryption key for sensitive data storage"/>

<!-- Button -->
<Button
    AutomationProperties.Name="Rotate Encryption Key"
    AutomationProperties.HelpText="Generate new encryption key and re-encrypt all sensitive data"/>

<!-- InfoBar -->
<InfoBar
    AutomationProperties.LiveSetting="Polite"
    AutomationProperties.Name="Security Warning"/>
```

---

## References

- [WinUI 3 Gallery - PasswordBox](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/PasswordBoxPage.xaml)
- [WinUI 3 Gallery - InfoBar](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/InfoBarPage.xaml)
- [Data Encryption (DPAPI)](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection)
