namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for PO Entry step.
    /// Represents PO number, part selection, and non-PO item flag.
    /// </summary>
    public class POEntryData
    {
        /// <summary>
        /// Purchase Order number (null for non-PO items).
        /// </summary>
        public string? PONumber { get; set; }

        /// <summary>
        /// Selected part information from Infor Visual.
        /// </summary>
        public Model_InforVisualPart? SelectedPart { get; set; }

        /// <summary>
        /// Flag indicating if this is a non-PO item (no PO number).
        /// </summary>
        public bool IsNonPOItem { get; set; }

        /// <summary>
        /// Part ID entered for lookup (for non-PO items or direct lookup).
        /// </summary>
        public string PartID { get; set; } = string.Empty;
    }
}
