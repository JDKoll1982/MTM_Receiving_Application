using System;

namespace MTM_Receiving_Application.Module_Reporting.Helpers;

/// <summary>
/// Single source for the per-module accent colors used by the report preview, the formatted
/// email body, and the PDF attachment.
/// </summary>
public static class Helper_ReportingModuleAccent
{
    private const string ReceivingBackground = "#0B6157";
    private const string ReceivingForeground = "#FFFFFF";
    private const string DunnageBackground = "#8A5E00";
    private const string DunnageForeground = "#FFFFFF";
    private const string VolvoBackground = "#1E4F7A";
    private const string VolvoForeground = "#FFFFFF";
    private const string DefaultBackground = "#4A5568";
    private const string DefaultForeground = "#FFFFFF";

    /// <summary>
    /// Returns the header background and foreground colors for <paramref name="moduleName"/>.
    /// </summary>
    public static (string AccentBackground, string AccentForeground) GetAccent(string? moduleName)
    {
        return moduleName switch
        {
            "Receiving" => (ReceivingBackground, ReceivingForeground),
            "Dunnage" => (DunnageBackground, DunnageForeground),
            "Volvo" => (VolvoBackground, VolvoForeground),
            _ => (DefaultBackground, DefaultForeground),
        };
    }
}
