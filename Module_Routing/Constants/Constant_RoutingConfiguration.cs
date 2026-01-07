namespace MTM_Receiving_Application.Module_Routing.Constants;

/// <summary>
/// Configuration key constants for Routing Module
/// Fixes Issue #22: Magic numbers in configuration keys
/// </summary>
public static class Constant_RoutingConfiguration
{
    // CSV Export Configuration
    public const string CsvExportPathNetwork = "RoutingModule:CsvExportPath:Network";
    public const string CsvExportPathLocal = "RoutingModule:CsvExportPath:Local";
    public const string CsvExportPathBaseDirectory = "RoutingModule:CsvExportPath:BaseDirectory";

    // CSV Retry Configuration
    public const string CsvRetryMaxAttempts = "RoutingModule:CsvRetry:MaxAttempts";
    public const string CsvRetryDelayMs = "RoutingModule:CsvRetry:DelayMs";

    // Default Values
    public const int DefaultCsvRetryMaxAttempts = 3;
    public const int DefaultCsvRetryDelayMs = 500;
    public const int DefaultDuplicateDetectionHours = 24;
    public const int DefaultQuickAddRecipientCount = 5;
    public const int DefaultPageLimit = 100;
}
