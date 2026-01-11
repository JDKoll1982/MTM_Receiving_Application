# Volvo Integration - Manual Sync Trigger

**SVG File**: `07-volvo-modal-sync.svg`  
**Parent Page**: Volvo Integration Settings  
**Type**: ContentDialog (Confirmation with Progress)  
**Purpose**: Trigger manual master data synchronization

---

## WinUI 3 Implementation

```xml
<ContentDialog
    Title="Trigger Manual Sync"
    PrimaryButtonText="Start Sync"
    CloseButtonText="Cancel"
    DefaultButton="Primary">
    
    <StackPanel Spacing="12">
        <InfoBar 
            Severity="Informational"
            IsOpen="True"
            IsClosable="False"
            Title="This will immediately synchronize part master data"/>
        
        <TextBlock TextWrapping="Wrap" Foreground="{ThemeResource TextFillColorSecondaryBrush}">
            Volvo master data will be downloaded and updated in the local database. 
            This may take several minutes depending on the number of parts.
        </TextBlock>
    </StackPanel>
</ContentDialog>
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task TriggerManualSyncAsync()
{
    var confirm = new ContentDialog
    {
        Title = "Trigger Manual Sync",
        PrimaryButtonText = "Start Sync",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Primary,
        XamlRoot = _xamlRoot
    };
    
    confirm.Content = new StackPanel
    {
        Spacing = 12,
        Children =
        {
            new InfoBar
            {
                Severity = InfoBarSeverity.Informational,
                IsOpen = true,
                IsClosable = false,
                Title = "This will immediately synchronize part master data"
            },
            new TextBlock
            {
                Text = "Volvo master data will be downloaded and updated in the local database. This may take several minutes depending on the number of parts.",
                TextWrapping = TextWrapping.Wrap
            }
        }
    };
    
    var result = await confirm.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        await ExecuteSyncWithProgressAsync();
    }
}

private async Task ExecuteSyncWithProgressAsync()
{
    // Show progress dialog
    var progressDialog = new ContentDialog
    {
        Title = "Synchronizing Master Data",
        CloseButtonText = "Cancel",
        XamlRoot = _xamlRoot
    };
    
    var progressBar = new ProgressBar
    {
        IsIndeterminate = true,
        Margin = new Thickness(0, 12, 0, 12)
    };
    
    var statusText = new TextBlock
    {
        Text = "Downloading part data...",
        TextWrapping = TextWrapping.Wrap
    };
    
    var stack = new StackPanel
    {
        Spacing = 12,
        Children = { progressBar, statusText }
    };
    
    progressDialog.Content = stack;
    
    // Start sync in background
    var syncTask = Task.Run(async () =>
    {
        var result = await _volvoMasterDataService.SyncAsync(
            progress: new Progress<string>(msg =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    statusText.Text = msg;
                });
            })
        );
        
        return result;
    });
    
    // Show dialog (non-blocking)
    var dialogTask = progressDialog.ShowAsync();
    
    // Wait for sync to complete
    var syncResult = await syncTask;
    
    // Close progress dialog
    progressDialog.Hide();
    
    // Show result
    if (syncResult.IsSuccess)
    {
        LastSyncStatusMessage = $"{DateTime.Now:MMMM dd, yyyy hh:mm tt} - {syncResult.Data} parts updated";
        
        var success = new ContentDialog
        {
            Title = "Sync Complete",
            Content = $"Successfully synchronized {syncResult.Data} parts from Volvo master data.",
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };
        await success.ShowAsync();
    }
    else
    {
        _errorHandler.ShowUserError(
            syncResult.ErrorMessage,
            "Sync Failed",
            nameof(TriggerManualSyncAsync)
        );
    }
}
```

---

## Master Data Service with Progress

```csharp
public class VolvoMasterDataService : IVolvoMasterDataService
{
    public async Task<Model_Dao_Result<int>> SyncAsync(IProgress<string> progress = null)
    {
        try
        {
            progress?.Report("Connecting to Volvo data source...");
            
            var parts = await FetchVolvoPartsAsync();
            
            progress?.Report($"Processing {parts.Count} parts...");
            
            int updatedCount = 0;
            
            for (int i = 0; i < parts.Count; i++)
            {
                await UpdatePartAsync(parts[i]);
                updatedCount++;
                
                if (i % 100 == 0)
                {
                    progress?.Report($"Updated {updatedCount} of {parts.Count} parts...");
                }
            }
            
            progress?.Report("Sync complete!");
            
            return Model_Dao_Result<int>.Success(updatedCount);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<int>.Failure($"Sync failed: {ex.Message}");
        }
    }
}
```

---

## Progress Reporting

The sync operation reports progress through `IProgress<string>`:

1. **Connecting** - Establishing connection
2. **Processing X parts** - Total count
3. **Updated X of Y** - Progress updates every 100 parts
4. **Sync complete** - Finished

---

## References

- [IProgress<T> Interface](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1)
- [ProgressBar Control](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.progressbar)
