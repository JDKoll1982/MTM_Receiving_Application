# Security & Session - Rotate Encryption Key Confirmation

**SVG File**: `02-security-modal-rotate-key.svg`  
**Parent Page**: Security & Session Settings  
**Type**: ContentDialog (Critical Confirmation)  
**Purpose**: Confirm encryption key rotation with warning

---

## WinUI 3 Implementation

```xml
<ContentDialog
    Title="Confirm Key Rotation"
    PrimaryButtonText="Rotate Key"
    CloseButtonText="Cancel"
    DefaultButton="Close"
    PrimaryButtonStyle="{StaticResource DangerButtonStyle}">
    
    <StackPanel Spacing="16">
        <!-- Warning Banner -->
        <InfoBar 
            Severity="Warning"
            IsOpen="True"
            IsClosable="False"
            Title="This will re-encrypt all sensitive data"/>
        
        <!-- Description -->
        <TextBlock TextWrapping="Wrap" Foreground="{ThemeResource TextFillColorSecondaryBrush}">
            A new encryption key will be generated and all passwords and sensitive data will be re-encrypted.
        </TextBlock>
        
        <!-- Critical warning -->
        <TextBlock 
            Text="This operation cannot be undone."
            Foreground="{ThemeResource SystemFillColorCriticalBrush}"
            FontWeight="SemiBold"/>
    </StackPanel>
</ContentDialog>
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task RotateEncryptionKeyAsync()
{
    var confirm = new ContentDialog
    {
        Title = "Confirm Key Rotation",
        PrimaryButtonText = "Rotate Key",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Close,
        XamlRoot = _xamlRoot
    };
    
    confirm.Content = new StackPanel
    {
        Spacing = 16,
        Children =
        {
            new InfoBar
            {
                Severity = InfoBarSeverity.Warning,
                IsOpen = true,
                IsClosable = false,
                Title = "This will re-encrypt all sensitive data"
            },
            new TextBlock
            {
                Text = "A new encryption key will be generated and all passwords and sensitive data will be re-encrypted.",
                TextWrapping = TextWrapping.Wrap
            },
            new TextBlock
            {
                Text = "This operation cannot be undone.",
                Foreground = Application.Current.Resources["SystemFillColorCriticalBrush"] as Brush,
                FontWeight = FontWeights.SemiBold
            }
        }
    };
    
    confirm.PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle"] as Style;
    
    var result = await confirm.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        IsBusy = true;
        StatusMessage = "Rotating encryption key...";
        
        try
        {
            var rotateResult = await _encryptionService.RotateKeyAsync();
            
            if (rotateResult.IsSuccess)
            {
                StatusMessage = "✓ Encryption key rotated successfully";
                
                // Show success dialog
                var success = new ContentDialog
                {
                    Title = "Key Rotation Complete",
                    Content = $"Successfully re-encrypted {rotateResult.Data} items with new key.",
                    CloseButtonText = "OK",
                    XamlRoot = _xamlRoot
                };
                await success.ShowAsync();
            }
            else
            {
                _errorHandler.ShowUserError(
                    rotateResult.ErrorMessage,
                    "Key Rotation Failed",
                    nameof(RotateEncryptionKeyAsync)
                );
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Critical,
                nameof(RotateEncryptionKeyAsync),
                nameof(SecuritySessionViewModel)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

---

## Encryption Service

```csharp
public class EncryptionService : IEncryptionService
{
    public async Task<Model_Dao_Result<int>> RotateKeyAsync()
    {
        try
        {
            // 1. Generate new key
            var newKey = GenerateAES256Key();
            
            // 2. Get all encrypted items
            var items = await GetAllEncryptedItemsAsync();
            
            var reencryptedCount = 0;
            
            // 3. Re-encrypt each item
            foreach (var item in items)
            {
                var decrypted = DecryptWithOldKey(item.Value);
                var encrypted = EncryptWithNewKey(decrypted, newKey);
                
                await UpdateEncryptedValueAsync(item.Id, encrypted);
                reencryptedCount++;
            }
            
            // 4. Save new key (encrypted with DPAPI)
            await SaveMasterKeyAsync(newKey);
            
            // 5. Log the operation
            await _logger.LogWarningAsync(
                $"Encryption key rotated. {reencryptedCount} items re-encrypted.",
                nameof(RotateKeyAsync)
            );
            
            return Model_Dao_Result<int>.Success(reencryptedCount);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<int>.Failure($"Key rotation failed: {ex.Message}");
        }
    }
    
    private string GenerateAES256Key()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        return Convert.ToBase64String(aes.Key);
    }
}
```

---

## Database Audit Log

```sql
INSERT INTO security_audit_log 
(event_type, user_id, description, severity, created_date)
VALUES 
('ENCRYPTION_KEY_ROTATION', @UserId, 'Master encryption key rotated', 'CRITICAL', CURRENT_TIMESTAMP);
```

---

## Permissions

- **Super Admin only**
- Requires re-authentication before execution
- Logged to audit trail

---

## References
- [AES Encryption in .NET](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes)
- [Data Protection API (DPAPI)](https://learn.microsoft.com/en-us/dotnet/standard/security/how-to-use-data-protection)
