namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// Represents a part/line item on a purchase order from Infor Visual.
    /// Read-only data from external system.
    /// </summary>
    public class Model_InforVisualPart
    {
        public string PartID { get; set; } = string.Empty;

        public string POLineNumber { get; set; } = string.Empty;

        public string PartType { get; set; } = string.Empty;

        public decimal QtyOrdered { get; set; }

        public string UnitOfMeasure { get; set; } = "EA";

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Remaining quantity available to receive (Ordered - Received).
        /// Whole number only (no decimals).
        /// </summary>
        public int RemainingQuantity { get; set; }

        /// <summary>
        /// Display text for UI showing part ID, description, and line number.
        /// </summary>
        public string DisplayText =>
            $"{PartID} - {Description} (Line {POLineNumber})";
    }
}
