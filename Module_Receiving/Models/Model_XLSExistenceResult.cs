namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Result of XLS existence check.
    /// </summary>
    public class Model_XLSExistenceResult
    {
        public bool LocalExists { get; set; }
        public bool NetworkExists { get; set; }
        public bool NetworkAccessible { get; set; }
    }
}
