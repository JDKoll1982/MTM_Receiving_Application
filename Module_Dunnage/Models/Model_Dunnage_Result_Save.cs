namespace MTM_Receiving_Application.Module_Dunnage.Models
{
    /// <summary>
    /// Result object for Dunnage save operations.
    /// Module-specific implementation for Dunnage save functionality.
    /// </summary>
    public class Model_Dunnage_Result_Save
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool DatabaseSaved { get; set; }
        public bool CSVSaved { get; set; }

        public static Model_Dunnage_Result_Save Success(bool dbSaved, bool csvSaved)
        {
            return new Model_Dunnage_Result_Save
            {
                IsSuccess = true,
                DatabaseSaved = dbSaved,
                CSVSaved = csvSaved
            };
        }

        public static Model_Dunnage_Result_Save Failure(string errorMessage)
        {
            return new Model_Dunnage_Result_Save
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
