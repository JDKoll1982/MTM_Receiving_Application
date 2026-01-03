namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// Result of CSV write operation.
    /// </summary>
    public class Model_CSVWriteResult
    {
        public bool LocalSuccess { get; set; }
        public bool NetworkSuccess { get; set; }

        // Combined properties from both definitions
        public string ErrorMessage { get; set; } = string.Empty;
        public string? LocalError { get; set; }
        public string? NetworkError { get; set; }
        public string LocalFilePath { get; set; } = string.Empty;
        public string NetworkFilePath { get; set; } = string.Empty;
        public int RecordsWritten { get; set; }

        public bool IsFullSuccess => LocalSuccess && NetworkSuccess;
        public bool IsPartialSuccess => LocalSuccess && !NetworkSuccess;
        public bool IsFailure => !LocalSuccess;
    }
}
