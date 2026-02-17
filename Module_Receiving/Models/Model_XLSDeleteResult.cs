namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of XLS delete operation.
    /// </summary>
    public class Model_XLSDeleteResult
    {
        public bool LocalDeleted { get; set; }
        public bool NetworkDeleted { get; set; }
        public string? LocalError { get; set; }
        public string? NetworkError { get; set; }
    }
}
