namespace MTM_Receiving_Application.Module_Core.Models.Core
{
    /// <summary>
    /// Result object returned after writing to CSV file(s).
    /// Shared model used across all modules for CSV write operations.
    /// </summary>
    public class Model_Core_Result_CSVWrite
    {
        public bool LocalSuccess { get; set; }
        public bool NetworkSuccess { get; set; }
        public string? LocalFilePath { get; set; }
        public string? NetworkFilePath { get; set; }
        public string? LocalErrorMessage { get; set; }
        public string? NetworkErrorMessage { get; set; }
        public string? ErrorMessage { get; set; } // General error message
        public string? NetworkError { get; set; } // Alias for NetworkErrorMessage
        public int RecordsWritten { get; set; }

        public bool IsSuccess => LocalSuccess || NetworkSuccess;
        public bool IsComplete => LocalSuccess && NetworkSuccess;
    }
}
