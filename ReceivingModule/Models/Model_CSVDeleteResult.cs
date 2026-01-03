namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// Result of CSV delete operation.
    /// </summary>
    public class Model_CSVDeleteResult
    {
        public bool LocalDeleted { get; set; }
        public bool NetworkDeleted { get; set; }
        public string? LocalError { get; set; }
        public string? NetworkError { get; set; }
    }
}
