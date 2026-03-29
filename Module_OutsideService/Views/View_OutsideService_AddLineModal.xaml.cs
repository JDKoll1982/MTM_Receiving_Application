using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.Models;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Modal dialog used to add a new Outside Service request line.
/// </summary>
public sealed partial class View_OutsideService_AddLineModal : ContentDialog
{
    private readonly List<NumberBox> _packageBoxes = new();
    private bool _isPartValidated;

    public View_OutsideService_AddLineModal()
    {
        InitializeComponent();
        RebuildPackageRows(1, null);
    }

    public Func<string, Task<bool>>? ValidatePartAsync { get; set; }

    public Func<
        string,
        Task<Model_OutsideServicePartMatchSuggestion?>
    >? ResolvePartMatchAsync { get; set; }

    public Model_OutsideServiceRequestLine? CreatedLine { get; private set; }

    private async void PartIdBox_LostFocus(object sender, RoutedEventArgs e)
    {
        await ValidatePartStatusAsync();
    }

    private async void PartMatchHelperButton_Click(object sender, RoutedEventArgs e)
    {
        if (ResolvePartMatchAsync is null || string.IsNullOrWhiteSpace(PartIdBox.Text))
        {
            return;
        }

        var suggestion = await ResolvePartMatchAsync(PartIdBox.Text.Trim());
        if (suggestion is null)
        {
            return;
        }

        PartIdBox.Text = suggestion.PartId;
        PartMatchStatusText.Text = "Matched part ready";
        PartMatchReasonText.Text = suggestion.MatchReason;
        _isPartValidated = true;
    }

    private void PackageCountBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        var packageCount = (int)Math.Max(1, sender.Value);
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
            var packageCount = (int)Math.Max(1, PackageCountBox.Value);

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
                if (ResolvePartMatchAsync is not null)
                {
                    var suggestion = await ResolvePartMatchAsync(partId);
                    if (suggestion is not null)
                    {
                        partId = suggestion.PartId;
                        PartIdBox.Text = partId;
                        _isPartValidated = true;
                    }
                }

                if (!_isPartValidated)
                {
                    args.Cancel = true;
                    ShowError("Select a valid part before saving the line.");
                    return;
                }
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
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
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

    private async Task<bool> ValidatePartStatusAsync()
    {
        if (ValidatePartAsync is null || string.IsNullOrWhiteSpace(PartIdBox.Text))
        {
            _isPartValidated = false;
            PartMatchStatusText.Text = "Validation required";
            PartMatchReasonText.Text =
                "Enter a part and use the helper when the typed value does not exactly match Infor Visual.";
            return false;
        }

        var isValid = await ValidatePartAsync(PartIdBox.Text.Trim());
        _isPartValidated = isValid;
        PartMatchStatusText.Text = isValid ? "Matched part ready" : "No exact match found";
        PartMatchReasonText.Text = isValid
            ? "The part is ready to return to the request entry screen."
            : "Use the Part Match Helper to apply a suggested part or correct the value manually.";
        return isValid;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
