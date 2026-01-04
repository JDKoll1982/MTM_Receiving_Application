namespace MTM_Receiving_Application.Module_Core.Models.Core
{
    /// <summary>
    /// Generic version of Model_Dao_Result for returning data
    /// </summary>
    /// <typeparam name="T">Type of data returned</typeparam>
    public class Model_Dao_Result<T> : Model_Dao_Result
    {
        /// <summary>
        /// Data returned by the operation
        /// </summary>
        public T? Data { get; set; }
    }
}

