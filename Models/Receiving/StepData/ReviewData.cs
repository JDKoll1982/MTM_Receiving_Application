using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for Review step.
    /// Represents final review of all loads before saving.
    /// </summary>
    public class ReviewData
    {
        /// <summary>
        /// List of all loads in the current session for review and editing.
        /// </summary>
        public List<Model_ReceivingLoad> Loads { get; set; } = new();

        /// <summary>
        /// Flag indicating if user can add another part/PO.
        /// </summary>
        public bool CanAddAnotherPart { get; set; } = true;
    }
}
