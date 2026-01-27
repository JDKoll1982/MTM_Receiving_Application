using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Represents the current data entry session with accumulated loads from potentially multiple parts.
    /// Persisted to JSON for session restoration across application restarts.
    /// </summary>
    public class Model_ReceivingSession
    {
        public Guid SessionID { get; set; } = Guid.NewGuid();

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<Model_ReceivingLoad> Loads { get; set; } = new();

        public bool IsNonPO { get; set; }

        public string? PoNumber { get; set; }

        public Model_User? User { get; set; }

        // Transient properties (not persisted to JSON)

        /// <summary>
        /// Total number of loads across all parts in this session.
        /// </summary>
        public int TotalLoadsCount => Loads?.Count ?? 0;

        /// <summary>
        /// Sum of all load weights/quantities in this session.
        /// </summary>
        public decimal TotalWeightQuantity => Loads?.Sum(l => l.WeightQuantity) ?? 0;

        /// <summary>
        /// List of unique part IDs in this session.
        /// </summary>
        public List<string> UniqueParts =>
            Loads?.Select(l => l.PartID).Distinct().ToList() ?? new List<string>();

        /// <summary>
        /// Indicates whether this session has any loads.
        /// </summary>
        public bool HasLoads => Loads?.Count > 0;
    }
}

