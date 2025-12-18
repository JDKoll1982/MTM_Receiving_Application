namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for Load Entry step.
    /// Represents the number of loads to create for the selected part.
    /// </summary>
    public class LoadEntryData
    {
        /// <summary>
        /// Number of loads/skids to create for this part.
        /// Must be >= 1.
        /// </summary>
        public int NumberOfLoads { get; set; } = 1;

        /// <summary>
        /// Selected part information (display only).
        /// </summary>
        public string SelectedPartInfo { get; set; } = string.Empty;
    }
}
