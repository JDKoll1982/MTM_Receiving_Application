using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Models.Enums;
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
        private bool _isSelected;

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
        private int _packagesPerLoad = 1;  // Default to 1 package per load

        [ObservableProperty]
        private string _packageTypeName = Enum_PackageType.Skid.ToString();

        [ObservableProperty]
        private Enum_PackageType _packageType = Enum_PackageType.Skid;

        [ObservableProperty]
        private decimal _weightPerPackage;

        [ObservableProperty]
        private bool _isNonPOItem;

        [ObservableProperty]
        private DateTime _receivedDate = DateTime.Now;

        partial void OnPartIDChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            try
            {
                var upperValue = value.ToUpperInvariant();
                if (upperValue.Contains("MMC"))
                {
                    PackageType = Enum_PackageType.Coil;
                }
                else if (upperValue.Contains("MMF"))
                {
                    PackageType = Enum_PackageType.Sheet;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Model_ReceivingLoad] OnPartIDChanged error: {ex.Message}");
            }
        }

        partial void OnPackageTypeChanged(Enum_PackageType value)
        {
            PackageTypeName = value.ToString();
        }

        partial void OnPackageTypeNameChanged(string value)
        {
            if (Enum.TryParse<Enum_PackageType>(value, out var result))
            {
                if (PackageType != result)
                {
                    PackageType = result;
                }
            }
        }

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
