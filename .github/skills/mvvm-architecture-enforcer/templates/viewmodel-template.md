# viewmodel-template.md

Last Updated: 2026-01-24

This is a template for creating a ViewModel class in the MTM Receiving Application using MVVM architecture and CommunityToolkit.Mvvm.

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.[MODULE].ViewModels;

public partial class ViewModel_[FEATURE_NAME] : ViewModel_Shared_Base
{
    private readonly IService_[SERVICE_NAME] _service;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private ObservableCollection<Model_[ENTITY]> _items = new();

    public ViewModel_[FEATURE_NAME](
        IService_[SERVICE_NAME] service,
        IService_ErrorHandler errorHandler) : base(errorHandler)
    {
        _service = service;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading...";

            var result = await _service.LoadAsync();
            if (!result.IsSuccess)
            {
                _errorHandler.ShowUserError(result.ErrorMessage, "Load Error", nameof(LoadAsync));
                StatusMessage = "Failed";
                return;
            }

            StatusMessage = "Loaded";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(LoadAsync), nameof(ViewModel_[FEATURE_NAME]));
            StatusMessage = "Error";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```
