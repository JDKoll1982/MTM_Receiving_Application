using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Models.Receiving
{
    /// <summary>
    /// Represents one load/skid of received material with all associated details.
    /// Implements ObservableObject for data binding in WinUI 3.
    /// </summary>
    public partial class Model_ReceivingLoad : ObservableObject
    {
        [ObservableProperty]
        private Guid _loadID = Guid.NewGuid();

        [ObservableProperty]
        private string _partID = string.Empty;

        [ObservableProperty]
        private string _partType = string.Empty;

        [ObservableProperty]
        private string? _poNumber;  // Nullable for non-PO items

        [ObservableProperty]
        private string _poLineNumber = string.Empty;

        [ObservableProperty]
        private int _loadNumber;

        [ObservableProperty]
        private decimal _weightQuantity;

        [ObservableProperty]
        private string _heatLotNumber = "Not Entered";  // Default for optional field

        [ObservableProperty]
        private int _remainingQuantity;

        [ObservableProperty]
        private int _packagesPerLoad;

        [ObservableProperty]
        private string _packageTypeName = string.Empty;

        [ObservableProperty]
        private decimal _weightPerPackage;

        [ObservableProperty]
        private bool _isNonPOItem;

        [ObservableProperty]
        private DateTime _receivedDate = DateTime.Now;

        /// <summary>
        /// Calculates and updates WeightPerPackage when quantity or package count changes.
        /// </summary>
        partial void OnWeightQuantityChanged(decimal value)
        {
            CalculateWeightPerPackage();
        }

        /// <summary>
        /// Calculates and updates WeightPerPackage when package count changes.
        /// </summary>
        partial void OnPackagesPerLoadChanged(int value)
        {
            CalculateWeightPerPackage();
        }

        /// <summary>
        /// Calculates weight per package: WeightQuantity ÷ PackagesPerLoad.
        /// </summary>
        private void CalculateWeightPerPackage()
        {
            if (PackagesPerLoad > 0)
            {
                WeightPerPackage = Math.Round(WeightQuantity / PackagesPerLoad, 2);
            }
            else
            {
                WeightPerPackage = 0;
            }
        }

        /// <summary>
        /// Display property for review grid showing weight per package with unit.
        /// </summary>
        public string WeightPerPackageDisplay => 
            $"{WeightPerPackage:F2} lbs per {PackageTypeName}";

        /// <summary>
        /// Display property for PO number handling null values.
        /// </summary>
        public string PONumberDisplay => 
            string.IsNullOrEmpty(PoNumber) ? "N/A" : PoNumber;
    }
}
