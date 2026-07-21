using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Tracks which placeholder scanner page is active inside the module host.
/// </summary>
public partial class Service_ScannerNavigation : ObservableObject, IService_ScannerNavigation
{
    [ObservableProperty]
    private Enum_ScannerPage _currentPage = Enum_ScannerPage.Workbench;

    /// <inheritdoc />
    public void ShowWorkbench()
    {
        CurrentPage = Enum_ScannerPage.Workbench;
    }

    /// <inheritdoc />
    public void ShowHistory()
    {
        CurrentPage = Enum_ScannerPage.History;
    }

    /// <inheritdoc />
    public void ShowSettings()
    {
        CurrentPage = Enum_ScannerPage.Settings;
    }
}