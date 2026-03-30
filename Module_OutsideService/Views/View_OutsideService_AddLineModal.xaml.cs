using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_OutsideService.Models;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Modal dialog used to add a new Outside Service request line.
/// </summary>
public sealed partial class View_OutsideService_AddLineModal : ContentDialog
{
    private readonly List<NumberBox> _packageBoxes = new();
    private bool _isApplyingDraftState;
    private bool _isPartValidated;
    private bool _isInitializing = true;

    public View_OutsideService_AddLineModal(AddLineDraftState? draftState = null)
    {
        InitializeComponent();
        Loaded += View_OutsideService_AddLineModal_Loaded;
        RebuildPackageRows(1, null);
        ApplyDraftState(draftState);
        _isInitializing = false;
    }

    public sealed class AddLineDraftState
    {
        public string PartId { get; set; } = string.Empty;

        public int PackageCount { get; set; } = 1;

        public List<double> PackageQuantities { get; set; } = new();

        public bool IsPartValidated { get; set; }

        public string PartMatchStatusText { get; set; } = "Validation required";

        public string PartMatchReasonText { get; set; } =
            "The helper opens automatically if the part is not found.";
    }

    public Func<string, Task<bool>>? ValidatePartAsync { get; set; }

    public Model_OutsideServiceRequestLine? CreatedLine { get; private set; }

    public string? PendingPartMatchValue { get; private set; }

    public bool RequiresPartMatch => !string.IsNullOrWhiteSpace(PendingPartMatchValue);

    public AddLineDraftState DraftState => CreateDraftState();

    private void View_OutsideService_AddLineModal_Loaded(object sender, RoutedEventArgs e)
    {
        AttachPackageCountInputHandler();
    }

    private async void PartIdBox_LostFocus(object sender, RoutedEventArgs e)
    {
        await ValidatePartStatusAsync(openPartMatchHelper: true);
    }

    private void PartIdBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isApplyingDraftState)
        {
            return;
        }

        PendingPartMatchValue = null;
        _isPartValidated = false;
        PartMatchStatusText.Text = "Validation required";
        PartMatchReasonText.Text = string.IsNullOrWhiteSpace(PartIdBox.Text)
            ? "Enter a part ID to continue."
            : "The helper opens automatically if the part is not found.";
    }

    private void PackageCountBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (_isInitializing || PackageRowsPanel is null)
        {
            return;
        }

        var packageCount = NormalizePackageCount(sender.Value);
        var existingValues = _packageBoxes.ConvertAll(box => box.Value);
        RebuildPackageRows(packageCount, existingValues);
    }

    private async void AddLineDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs args
    )
    {
        var deferral = args.GetDeferral();
        try
        {
            ErrorText.Visibility = Visibility.Collapsed;
            var partId = PartIdBox.Text?.Trim() ?? string.Empty;
            var packageCount = NormalizePackageCount(PackageCountBox.Value);

            if (string.IsNullOrWhiteSpace(partId))
            {
                args.Cancel = true;
                ShowError("Part ID is required.");
                return;
            }

            if (!_isPartValidated)
            {
                _isPartValidated = await ValidatePartStatusAsync();
            }

            if (!_isPartValidated)
            {
                args.Cancel = true;
                ShowError("Leave the Part ID field to choose a valid match.");
                return;
            }

            var packages = new List<Model_OutsideServiceRequestPackage>();
            for (var index = 0; index < _packageBoxes.Count; index++)
            {
                var value = _packageBoxes[index].Value;
                if (value <= 0)
                {
                    args.Cancel = true;
                    ShowError("Each package quantity must be greater than zero.");
                    return;
                }

                packages.Add(
                    new Model_OutsideServiceRequestPackage
                    {
                        PackageSequence = index + 1,
                        PackageQuantity = Convert.ToDecimal(value, CultureInfo.InvariantCulture),
                    }
                );
            }

            if (packages.Count != packageCount)
            {
                args.Cancel = true;
                ShowError("The number of package rows must match the package count.");
                return;
            }

            CreatedLine = new Model_OutsideServiceRequestLine
            {
                PartId = partId,
                PackageCount = packageCount,
                Packages = packages,
                LinePhase = Enum_OutsideServiceLinePhase.Initialize,
            };
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void RebuildPackageRows(int packageCount, IReadOnlyList<double>? existingValues)
    {
        PackageRowsPanel.Children.Clear();
        _packageBoxes.Clear();

        for (var index = 0; index < packageCount; index++)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }
            );
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            row.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );

            var label = new TextBlock
            {
                Text = $"Package {index + 1}",
                VerticalAlignment = VerticalAlignment.Center,
            };

            var numberBox = new NumberBox
            {
                Minimum = 0,
                SmallChange = 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
                Value =
                    existingValues is not null && index < existingValues.Count
                        ? existingValues[index]
                        : 0,
            };

            Grid.SetColumn(label, 0);
            Grid.SetColumn(numberBox, 2);
            row.Children.Add(label);
            row.Children.Add(numberBox);

            PackageRowsPanel.Children.Add(row);
            _packageBoxes.Add(numberBox);
        }
    }

    private void ApplyDraftState(AddLineDraftState? draftState)
    {
        if (draftState is null)
        {
            PartMatchStatusText.Text = "Validation required";
            PartMatchReasonText.Text = "The helper opens automatically if the part is not found.";
            return;
        }

        _isApplyingDraftState = true;
        try
        {
            PartIdBox.Text = draftState.PartId;
            PackageCountBox.Value = draftState.PackageCount;
            RebuildPackageRows(draftState.PackageCount, draftState.PackageQuantities);
            _isPartValidated = draftState.IsPartValidated;
            PartMatchStatusText.Text = draftState.PartMatchStatusText;
            PartMatchReasonText.Text = draftState.PartMatchReasonText;
        }
        finally
        {
            _isApplyingDraftState = false;
        }
    }

    private AddLineDraftState CreateDraftState()
    {
        return new AddLineDraftState
        {
            PartId = PartIdBox.Text?.Trim() ?? string.Empty,
            PackageCount = NormalizePackageCount(PackageCountBox.Value),
            PackageQuantities = _packageBoxes.ConvertAll(box => box.Value),
            IsPartValidated = _isPartValidated,
            PartMatchStatusText = PartMatchStatusText.Text,
            PartMatchReasonText = PartMatchReasonText.Text,
        };
    }

    private void AttachPackageCountInputHandler()
    {
        var inputBox = FindDescendant<TextBox>(PackageCountBox);
        if (inputBox is null)
        {
            return;
        }

        inputBox.TextChanging -= PackageCountInputBox_TextChanging;
        inputBox.TextChanging += PackageCountInputBox_TextChanging;
    }

    private void PackageCountInputBox_TextChanging(
        TextBox sender,
        TextBoxTextChangingEventArgs args
    )
    {
        if (_isInitializing || _isApplyingDraftState || PackageRowsPanel is null)
        {
            return;
        }

        if (!TryParsePackageCount(sender.Text, out var packageCount))
        {
            return;
        }

        if (packageCount == _packageBoxes.Count)
        {
            return;
        }

        var existingValues = _packageBoxes.ConvertAll(box => box.Value);
        RebuildPackageRows(packageCount, existingValues);
    }

    private async Task<bool> ValidatePartStatusAsync(bool openPartMatchHelper = false)
    {
        var partId = PartIdBox.Text?.Trim() ?? string.Empty;

        if (ValidatePartAsync is null || string.IsNullOrWhiteSpace(partId))
        {
            PendingPartMatchValue = null;
            _isPartValidated = false;
            PartMatchStatusText.Text = "Validation required";
            PartMatchReasonText.Text = "Enter a part ID to continue.";
            return false;
        }

        var isValid = await ValidatePartAsync(partId);
        _isPartValidated = isValid;
        PendingPartMatchValue = isValid ? null : partId;
        PartMatchStatusText.Text = isValid ? "Matched part ready" : "No exact match found";
        PartMatchReasonText.Text = isValid
            ? "The part is ready to save."
            : "Choose a suggested part to continue.";

        if (!isValid && openPartMatchHelper)
        {
            Hide();
        }

        return isValid;
    }

    private static int NormalizePackageCount(double rawValue)
    {
        if (double.IsNaN(rawValue) || double.IsInfinity(rawValue))
        {
            return 1;
        }

        return Math.Max(1, Convert.ToInt32(Math.Truncate(rawValue), CultureInfo.InvariantCulture));
    }

    private static bool TryParsePackageCount(string? rawText, out int packageCount)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            packageCount = 1;
            return false;
        }

        if (
            double.TryParse(
                rawText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsedValue
            )
            || double.TryParse(
                rawText,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out parsedValue
            )
        )
        {
            packageCount = NormalizePackageCount(parsedValue);
            return true;
        }

        packageCount = 1;
        return false;
    }

    private static T? FindDescendant<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is null)
        {
            return null;
        }

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var index = 0; index < childrenCount; index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
