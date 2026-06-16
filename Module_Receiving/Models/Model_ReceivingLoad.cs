using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    /// <summary>
    /// Represents one load/skid of received material with all associated details.
    /// Implements ObservableObject for data binding in WinUI 3.
    /// </summary>
    public partial class Model_ReceivingLoad : ObservableObject
    {
        private static readonly Regex CanonicalPoNumberPattern = new(
            @"^(?:PO-)?(?<digits>\d{1,6})(?<suffix>[Bb]?)$",
            RegexOptions.IgnoreCase
        );

        [ObservableProperty]
        private Guid _loadID = Guid.NewGuid();

        [ObservableProperty]
        private int? _labelDataRecordID;

        [ObservableProperty]
        private int? _historyRecordID;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _partID = string.Empty;

        [ObservableProperty]
        private string _partType = string.Empty;

        [ObservableProperty]
        private string? _poNumber; // Nullable for non-PO items

        [ObservableProperty]
        private string _poLineNumber = string.Empty;

        [ObservableProperty]
        private string _selectedPartSourcePONumber = string.Empty;

        [ObservableProperty]
        private int _loadNumber;

        [ObservableProperty]
        private decimal _weightQuantity = 0;

        [ObservableProperty]
        private string _heatLotNumber = string.Empty; // Default to empty, set to "Nothing Entered" on save if blank

        [ObservableProperty]
        private string _initialLocation = string.Empty; // Optional, validated only when populated

        [ObservableProperty]
        private int _remainingQuantity;

        [ObservableProperty]
        private int _packagesPerLoad = 0; // Default to 0 (blank)

        [ObservableProperty]
        private string _packageTypeName = nameof(Enum_PackageType.Skid);

        [ObservableProperty]
        private Enum_PackageType _packageType = Enum_PackageType.Skid;

        [ObservableProperty]
        private decimal _weightPerPackage;

        [ObservableProperty]
        private bool _isNonPOItem;

        [ObservableProperty]
        private DateTime _receivedDate = DateTime.Now;

        [ObservableProperty]
        private string? _userId;

        [ObservableProperty]
        private string _userSetCustomerName = string.Empty;

        [ObservableProperty]
        private string _userSetVariable = string.Empty;

        [ObservableProperty]
        [JsonIgnore]
        private Visibility _userSetVariableFieldVisibility = Visibility.Collapsed;

        [ObservableProperty]
        [JsonIgnore]
        private string _userSetVariableFieldHeaderText = "User Set Variable";

        [ObservableProperty]
        [JsonIgnore]
        private string _userSetVariableFieldPlaceholderText = "Enter variable value";

        [ObservableProperty]
        [JsonIgnore]
        private string _userSetVariableAccessibilityName = "User Set Variable";

        [ObservableProperty]
        private int _employeeNumber;

        [ObservableProperty]
        private bool _isQualityHoldRequired;

        [ObservableProperty]
        private bool _isQualityHoldAcknowledged;

        [ObservableProperty]
        private bool _isReprint;

        [ObservableProperty]
        private string _qualityHoldRestrictionType = string.Empty;

        // Additional fields from Infor Visual / PO data
        [ObservableProperty]
        private string _partDescription = string.Empty;

        [ObservableProperty]
        private string _unitOfMeasure = "EA";

        [ObservableProperty]
        private decimal _qtyOrdered;

        [ObservableProperty]
        private string _poVendor = string.Empty;

        [ObservableProperty]
        private string _poStatus = string.Empty;

        [ObservableProperty]
        private DateTime? _poDueDate;

        public string PackageTypeDisplayName =>
            string.IsNullOrWhiteSpace(PackageTypeName) ? PackageType.ToString() : PackageTypeName;

        partial void OnPartIDChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            // If PackagesPerLoad is 0 (default/blank), set it to 1 when PartID is entered
            if (PackagesPerLoad == 0)
            {
                PackagesPerLoad = 1;
            }

            try
            {
                var upperValue = value.ToUpperInvariant();
                if (upperValue.Contains("MMC"))
                {
                    PackageType = Enum_PackageType.Coil;
                    PartType = "Coil"; // Set part type for validation
                }
                else if (upperValue.Contains("MMF"))
                {
                    PackageType = Enum_PackageType.Sheet;
                    PartType = "Sheet"; // Set part type for validation
                }
                else
                {
                    // Default for non-MMC/MMF parts (tubes, etc.)
                    PartType = "Standard";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Model_ReceivingLoad] OnPartIDChanged error: {ex.Message}"
                );
            }
        }

        partial void OnPackageTypeChanged(Enum_PackageType value)
        {
            PackageTypeName = value.ToString();
            OnPropertyChanged(nameof(PackageTypeDisplayName));
        }

        partial void OnPoNumberChanged(string? value)
        {
            var normalizedPoNumber = NormalizePoNumber(value);
            if (string.Equals(value, normalizedPoNumber, StringComparison.Ordinal))
            {
                return;
            }

            PoNumber = normalizedPoNumber;
        }

        partial void OnHeatLotNumberChanged(string value)
        {
            var normalizedHeatLotNumber = NormalizeHeatLotNumber(value);
            if (string.Equals(value, normalizedHeatLotNumber, StringComparison.Ordinal))
            {
                return;
            }

            HeatLotNumber = normalizedHeatLotNumber;
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

            OnPropertyChanged(nameof(PackageTypeDisplayName));
        }

        /// <summary>
        /// Calculates and updates WeightPerPackage when quantity or package count changes.
        /// </summary>
        partial void OnWeightQuantityChanged(decimal value)
        {
            CalculateWeightPerPackage();
        }

        partial void OnWeightPerPackageChanged(decimal value)
        {
            OnPropertyChanged(nameof(WeightPerPackageDisplay));
            OnPropertyChanged(nameof(WeightPerPackageValueDisplay));
        }

        /// <summary>
        /// Calculates and updates WeightPerPackage when package count changes.
        /// </summary>
        partial void OnPackagesPerLoadChanged(int value)
        {
            CalculateWeightPerPackage();
        }

        partial void OnUnitOfMeasureChanged(string value)
        {
            OnPropertyChanged(nameof(WeightPerPackageLabel));
            OnPropertyChanged(nameof(WeightPerPackageDisplay));
            OnPropertyChanged(nameof(WeightPerPackageValueDisplay));
        }

        /// <summary>
        /// Calculates the per-package quantity by dividing the entered quantity across packages.
        /// </summary>
        private void CalculateWeightPerPackage()
        {
            if (PackagesPerLoad > 0)
            {
                WeightPerPackage = Math.Ceiling(WeightQuantity / PackagesPerLoad);
            }
            else
            {
                WeightPerPackage = 0;
            }
        }

        /// <summary>
        /// Gets the label shown for the per-package quantity in Guided Mode.
        /// </summary>
        public string WeightPerPackageLabel => $"{GetDisplayUnitOfMeasure()} per Package";

        /// <summary>
        /// Gets the formatted per-package quantity value with the Infor Visual unit type.
        /// </summary>
        public string WeightPerPackageValueDisplay =>
            $"{WeightPerPackage:F0} {GetDisplayUnitOfMeasure()}";

        /// <summary>
        /// Display property for review grid showing per-package quantity with unit.
        /// </summary>
        public string WeightPerPackageDisplay =>
            $"{WeightPerPackage:F0} {GetDisplayUnitOfMeasure()} per {PackageTypeName}";

        /// <summary>
        /// Display property for PO number handling null values.
        /// </summary>
        public string PONumberDisplay => string.IsNullOrEmpty(PoNumber) ? "N/A" : PoNumber;

        /// <summary>
        /// Display property for load number (used in DataTemplates where x:Bind cannot call into the parent ViewModel).
        /// </summary>
        public string LoadDisplayText => $"Load {LoadNumber}";

        private string GetDisplayUnitOfMeasure()
        {
            return string.IsNullOrWhiteSpace(UnitOfMeasure)
                ? "Units"
                : UnitOfMeasure.Trim().ToUpperInvariant();
        }

        private static string? NormalizePoNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmedValue = value.Trim();
            var match = CanonicalPoNumberPattern.Match(trimmedValue);
            if (!match.Success)
            {
                return trimmedValue;
            }

            var digits = match.Groups["digits"].Value;
            var suffix = match.Groups["suffix"].Value.ToUpperInvariant();
            return $"PO-{digits.PadLeft(6, '0')}{suffix}";
        }

        private static string NormalizeHeatLotNumber(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }
    }
}
