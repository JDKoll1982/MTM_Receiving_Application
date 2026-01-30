namespace MTM_Receiving_Application.Module_Core.Models.Core
{
    /// <summary>
    /// Result of CSV existence check.
    /// Shared model used across all modules for CSV existence checks.
    /// </summary>
    public class Model_Dunnage_Result_CSVExistence
    {
        public bool LocalExists { get; set; }
        public bool NetworkExists { get; set; }
        public bool NetworkAccessible { get; set; }
    }
}
