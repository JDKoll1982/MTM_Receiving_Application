namespace MTM_Receiving_Application.Models.Receiving
{
    public class Model_CSVWriteResult
    {
        public bool LocalSuccess { get; set; }
        public bool NetworkSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string LocalFilePath { get; set; } = string.Empty;
        public string NetworkFilePath { get; set; } = string.Empty;
    }
}
