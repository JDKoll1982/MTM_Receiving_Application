using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.ReceivingModule.Models
{
    /// <summary>
    /// UI-specific model for the quick-select heat number feature.
    /// Displays heat numbers with checkboxes for applying to multiple loads.
    /// </summary>
    public partial class Model_HeatCheckboxItem : ObservableObject
    {
        [ObservableProperty]
        private string _heatLotNumber = string.Empty;

        [ObservableProperty]
        private bool _isChecked;

        [ObservableProperty]
        private int _firstLoadNumber;

        /// <summary>
        /// Display text showing heat number and which load it came from.
        /// </summary>
        public string DisplayText => 
            $"{HeatLotNumber} (from Load {FirstLoadNumber})";
    }
}
