using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for Weight/Quantity Entry step.
    /// Represents weight/quantity values for all loads.
    /// </summary>
    public class WeightQuantityData
    {
        /// <summary>
        /// List of loads with weight/quantity values.
        /// </summary>
        public List<Model_ReceivingLoad> Loads { get; set; } = new();

        /// <summary>
        /// Warning message if same-day receiving detected.
        /// </summary>
        public string WarningMessage { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating if a warning exists.
        /// </summary>
        public bool HasWarning { get; set; }

        /// <summary>
        /// PO quantity information for display (ordered quantity).
        /// </summary>
        public string POQuantityInfo { get; set; } = string.Empty;
    }
}
