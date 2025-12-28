using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Receiving
{
    /// <summary>
    /// Represents a purchase order retrieved from Infor Visual (SQL Server).
    /// Read-only data from external system.
    /// </summary>
    public class Model_InforVisualPO
    {
        public string PONumber { get; set; } = string.Empty;

        public string Vendor { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<Model_InforVisualPart> Parts { get; set; } = new();

        /// <summary>
        /// Indicates whether this PO has any parts associated with it.
        /// </summary>
        public bool HasParts => Parts?.Count > 0;

        /// <summary>
        /// Returns a user-friendly status description.
        /// </summary>
        public string StatusDescription => Status switch
        {
            "R" => "Open",
            "C" => "Closed",
            "O" => "Open",
            "P" => "Partially Received",
            "X" => "Cancelled",
            "F" => "Firm",
            "" => "Unknown",
            _ => $"Status: {Status}"
        };

        /// <summary>
        /// Indicates if the PO is closed or cancelled.
        /// </summary>
        public bool IsClosed => Status == "C" || Status == "X";
    }
}
