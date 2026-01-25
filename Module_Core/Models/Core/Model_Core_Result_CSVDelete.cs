namespace MTM_Receiving_Application.Module_Core.Models.Core
{
    /// <summary>
    /// Result object returned after deleting CSV file(s).
    /// Shared model used across all modules for CSV delete operations.
    /// </summary>
    public class Model_Core_Result_CSVDelete
    {
        public bool LocalDeleted { get; set; }
        public bool NetworkDeleted { get; set; }
        public string? LocalErrorMessage { get; set; }
        public string? NetworkErrorMessage { get; set; }
        public string? LocalError { get; set; } // Alias for LocalErrorMessage
        public string? NetworkError { get; set; } // Alias for NetworkErrorMessage

        public bool IsSuccess => LocalDeleted || NetworkDeleted;
        public bool IsComplete => LocalDeleted && NetworkDeleted;
    }
}
