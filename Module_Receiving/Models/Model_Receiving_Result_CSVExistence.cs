namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of CSV existence check.
    /// </summary>
    public class Model_Receiving_Result_CSVExistence
    {
        public bool LocalExists { get; set; }
        public bool NetworkExists { get; set; }
        public bool NetworkAccessible { get; set; }
    }
}
