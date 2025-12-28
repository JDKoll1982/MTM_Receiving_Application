namespace MTM_Receiving_Application.Models.Receiving
{
    /// <summary>
    /// Result of CSV existence check.
    /// </summary>
    public class Model_CSVExistenceResult
    {
        public bool LocalExists { get; set; }
        public bool NetworkExists { get; set; }
        public bool NetworkAccessible { get; set; }
    }
}
