using System;

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual
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
        /// Default receiving location from Infor Visual for this part.
        /// </summary>
        public string DefaultLocationId { get; set; } = string.Empty;

        /// <summary>
        /// Remaining quantity available to receive (Ordered - Received).
        /// Whole number only (no decimals).
        /// </summary>
        public int RemainingQuantity { get; set; }

        /// <summary>
        /// Selected line-level due date from Infor Visual for this part.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets whether this part requires a quality hold acknowledgment.
        /// </summary>
        public bool RequiresQualityHold { get; set; }

        /// <summary>
        /// Gets or sets the quality hold restriction description for this part.
        /// </summary>
        public string QualityHoldRestrictionType { get; set; } = string.Empty;

        /// <summary>
        /// Display text for UI showing part ID, description, and line number.
        /// </summary>
        public string DisplayText => $"{PartID} - {Description} (Line {POLineNumber})";
    }
}
