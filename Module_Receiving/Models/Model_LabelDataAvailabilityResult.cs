namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of label-data availability check.
    /// </summary>
    public class Model_LabelDataAvailabilityResult
    {
        public bool LabelQueueAvailable { get; set; }
        public bool ArchiveQueueAvailable { get; set; }
        public bool ArchiveQueueAccessible { get; set; }
    }
}
