using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents a DAO validation result (development diagnostics)
/// </summary>
public partial class Model_DaoTestResult : ObservableObject
{
    [ObservableProperty]
    private string _daoName = string.Empty;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private string _operationsTested = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;
}
