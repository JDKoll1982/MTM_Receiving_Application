using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents a stored procedure validation result (development diagnostics)
/// </summary>
public partial class Model_StoredProcedureTestResult : ObservableObject
{
    [ObservableProperty]
    private string _procedureName = string.Empty;

    [ObservableProperty]
    private bool _isValid;

    [ObservableProperty]
    private int _executionTimeMs;

    [ObservableProperty]
    private string _testDetails = string.Empty;
}
