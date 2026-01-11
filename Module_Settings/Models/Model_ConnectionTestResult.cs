using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents the result of a database connection test (development diagnostics)
/// </summary>
public partial class Model_ConnectionTestResult : ObservableObject
{
    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = string.Empty;

    [ObservableProperty]
    private int _connectionTimeMs;

    [ObservableProperty]
    private string _serverVersion = string.Empty;
}
