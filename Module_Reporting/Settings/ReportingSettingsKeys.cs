namespace MTM_Receiving_Application.Module_Reporting.Settings;

public static class ReportingSettingsKeys
{
    public const string Category = "Reporting";

    /// <summary>
    /// Default folder the preview's Save PDF action writes into. A shared network folder so a
    /// finished end-of-day report lands where the rest of the team already looks for it.
    /// </summary>
    public const string DefaultPdfSaveFolder = @"\\mtmanu-fs01\expo drive\RECEIVING\EoD Reports";

    public static class Preview
    {
        public const string RowDisplayMode = "Reporting.Preview.RowDisplayMode";
        public const string ModulePreferencesJson = "Reporting.Preview.ModulePreferencesJson";
    }

    public static class Output
    {
        /// <summary>Folder the Save PDF action writes the report into.</summary>
        public const string PdfSaveFolder = "Reporting.Output.PdfSaveFolder";
    }
}
