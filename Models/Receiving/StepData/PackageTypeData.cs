using System.Collections.Generic;

namespace MTM_Receiving_Application.Models.Receiving.StepData
{
    /// <summary>
    /// Data transfer object for Package Type Entry step.
    /// Represents package types and counts per load with preference management.
    /// </summary>
    public class PackageTypeData
    {
        /// <summary>
        /// List of loads with package type and count values.
        /// </summary>
        public List<Model_ReceivingLoad> Loads { get; set; } = new();

        /// <summary>
        /// Available package type options (e.g., Coils, Sheets, Skids, Custom).
        /// </summary>
        public List<string> AvailablePackageTypes { get; set; } = new() { "Coils", "Sheets", "Skids", "Custom" };

        /// <summary>
        /// Selected package type from dropdown.
        /// </summary>
        public string SelectedPackageType { get; set; } = string.Empty;

        /// <summary>
        /// Custom package type name (when "Custom" is selected).
        /// </summary>
        public string CustomPackageTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating if selected type should be saved as default preference.
        /// </summary>
        public bool IsSaveAsDefault { get; set; }

        /// <summary>
        /// Flag indicating if custom type input is visible.
        /// </summary>
        public bool IsCustomTypeVisible { get; set; }
    }
}
