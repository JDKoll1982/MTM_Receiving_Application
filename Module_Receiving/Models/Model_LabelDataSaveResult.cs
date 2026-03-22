namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of label-data save operation.
    /// </summary>
    public class Model_LabelDataSaveResult
    {
        public bool LabelQueueSuccess { get; set; }
        public bool ArchiveQueueSuccess { get; set; }

        // Combined properties from both definitions
        public string ErrorMessage { get; set; } = string.Empty;
        public string? LabelQueueError { get; set; }
        public string? ArchiveQueueError { get; set; }
        public string LabelQueuePath { get; set; } = string.Empty;
        public string ArchiveQueuePath { get; set; } = string.Empty;
        public int RecordsWritten { get; set; }

        public bool IsFullSuccess => LabelQueueSuccess && ArchiveQueueSuccess;
        public bool IsPartialSuccess => LabelQueueSuccess && !ArchiveQueueSuccess;
        public bool IsFailure => !LabelQueueSuccess;
    }
}
