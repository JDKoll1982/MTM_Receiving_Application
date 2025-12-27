namespace MTM_Receiving_Application.Models.Receiving
{
    public class Model_SaveResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int RecordsSaved { get; set; }
        public Model_CSVWriteResult? CSVExportResult { get; set; }
    }
}
