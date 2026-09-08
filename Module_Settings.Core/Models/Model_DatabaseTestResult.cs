namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Result of a diagnostic database connection test.
/// </summary>
public sealed record Model_DatabaseTestResult(bool Success, string Message);
