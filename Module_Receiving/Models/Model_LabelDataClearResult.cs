namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of label-data clear operation.
    /// </summary>
    public class Model_LabelDataClearResult
    {
        public bool LabelQueueCleared { get; set; }
        public bool ArchiveQueueCleared { get; set; }
        public string? LabelQueueError { get; set; }
        public string? ArchiveQueueError { get; set; }
    }
}
