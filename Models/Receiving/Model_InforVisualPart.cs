namespace MTM_Receiving_Application.Models.Receiving
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
        
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Display text for UI showing part ID, description, and line number.
        /// </summary>
        public string DisplayText => 
            $"{PartID} - {Description} (Line {POLineNumber})";
    }
}
