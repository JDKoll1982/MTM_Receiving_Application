using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services;

/// <summary>
/// Stores the reusable back action shown in the shared main-window header.
/// </summary>
public partial class Service_HeaderBackNavigation : ObservableObject, IService_HeaderBackNavigation
{
    private Func<Task>? _backAction;

    [ObservableProperty]
    private bool _isBackButtonVisible;

    [ObservableProperty]
    private string _backButtonToolTip = "Back";

    public void RegisterBackAction(Func<Task> action, string toolTip = "Back")
    {
        ArgumentNullException.ThrowIfNull(action);

        _backAction = action;
        BackButtonToolTip = string.IsNullOrWhiteSpace(toolTip) ? "Back" : toolTip;
        IsBackButtonVisible = true;
    }

    public async Task ExecuteBackActionAsync()
    {
        if (_backAction is null)
        {
            return;
        }

        await _backAction();
    }

    public void ClearBackAction()
    {
        _backAction = null;
        BackButtonToolTip = "Back";
        IsBackButtonVisible = false;
    }
}
