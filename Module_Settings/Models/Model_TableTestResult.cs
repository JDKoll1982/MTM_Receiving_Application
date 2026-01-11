using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents a database table validation result (development diagnostics)
/// </summary>
public partial class Model_TableTestResult : ObservableObject
{
    [ObservableProperty]
    private string _tableName = string.Empty;

    [ObservableProperty]
    private string _details = string.Empty;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private string _statusText = string.Empty;
}
