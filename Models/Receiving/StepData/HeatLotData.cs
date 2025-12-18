using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for Heat/Lot Entry step.
    /// Represents heat/lot numbers for all loads with unique value management.
    /// </summary>
    public class HeatLotData
    {
        /// <summary>
        /// List of loads with heat/lot number values.
        /// </summary>
        public List<Model_ReceivingLoad> Loads { get; set; } = new();

        /// <summary>
        /// List of unique heat numbers with checkbox state for bulk assignment.
        /// </summary>
        public List<Model_HeatCheckboxItem> UniqueHeatNumbers { get; set; } = new();
    }
}
