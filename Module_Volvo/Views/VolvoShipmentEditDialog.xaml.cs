using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.Models;
using System;
using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Volvo.Views;

public sealed partial class VolvoShipmentEditDialog : ContentDialog
{
    public Model_VolvoShipment Shipment { get; set; } = null!;
    public ObservableCollection<Model_VolvoShipmentLine> Lines { get; private set; }
    public ObservableCollection<Model_VolvoPart> AvailableParts { get; set; }

    public VolvoShipmentEditDialog()
    {
        InitializeComponent();
        Lines = new ObservableCollection<Model_VolvoShipmentLine>();
        AvailableParts = new ObservableCollection<Model_VolvoPart>();

        // Wire up button events
        AddPartButton.Click += (s, e) => AddNewLine();
        RemovePartButton.Click += (s, e) => RemoveSelectedLine();

        // Set DataGrid ItemsSource
        PartsDataGrid.ItemsSource = Lines;
    }

    public void LoadShipment(Model_VolvoShipment shipment, ObservableCollection<Model_VolvoShipmentLine> lines, ObservableCollection<Model_VolvoPart> availableParts)
    {
        Shipment = shipment;
        AvailableParts = availableParts;

        // Populate header fields
        ShipmentDatePicker.Date = shipment.ShipmentDate;
        ShipmentNumberBox.Text = shipment.ShipmentNumber.ToString();
        PONumberBox.Text = shipment.PONumber ?? string.Empty;
        ReceiverNumberBox.Text = shipment.ReceiverNumber ?? string.Empty;
        NotesBox.Text = shipment.Notes ?? string.Empty;

        // Load lines
        Lines.Clear();
        foreach (var line in lines)
        {
            Lines.Add(line);
        }
    }

    public Model_VolvoShipment GetUpdatedShipment()
    {
        Shipment.ShipmentDate = ShipmentDatePicker.Date?.DateTime ?? DateTime.Now;
        Shipment.PONumber = string.IsNullOrWhiteSpace(PONumberBox.Text) ? null : PONumberBox.Text;
        Shipment.ReceiverNumber = string.IsNullOrWhiteSpace(ReceiverNumberBox.Text) ? null : ReceiverNumberBox.Text;
        Shipment.Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text;

        return Shipment;
    }

    public ObservableCollection<Model_VolvoShipmentLine> GetUpdatedLines()
    {
        return Lines;
    }

    private void AddNewLine()
    {
        Lines.Add(new Model_VolvoShipmentLine
        {
            ShipmentId = Shipment.Id,
            PartNumber = string.Empty,
            ReceivedSkidCount = 0,
            CalculatedPieceCount = 0,
            HasDiscrepancy = false,
            ExpectedSkidCount = null,
            DiscrepancyNote = null
        });
    }

    private void RemoveSelectedLine()
    {
        if (PartsDataGrid.SelectedItem is Model_VolvoShipmentLine selectedLine)
        {
            Lines.Remove(selectedLine);
        }
    }

    private async void OnDiscrepancyButtonClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        if (XamlRoot == null)
        {
            return;
        }

        if (line.HasDiscrepancy)
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Remove Discrepancy",
                Content = "Remove the discrepancy for this line?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            var confirmResult = await confirmDialog.ShowAsync();
            if (confirmResult == ContentDialogResult.Primary)
            {
                line.HasDiscrepancy = false;
                line.ExpectedSkidCount = null;
                line.DiscrepancyNote = null;
            }

            return;
        }

        var expectedSkidsBox = new NumberBox
        {
            Header = "Expected Skids",
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            Value = line.ExpectedSkidCount ?? 1
        };

        var noteBox = new TextBox
        {
            Header = "Discrepancy Note",
            PlaceholderText = "Explain discrepancy",
            Text = line.DiscrepancyNote ?? string.Empty
        };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(expectedSkidsBox);
        panel.Children.Add(noteBox);

        var dialog = new ContentDialog
        {
            Title = "Report Discrepancy",
            Content = panel,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (expectedSkidsBox.Value < 1 || string.IsNullOrWhiteSpace(noteBox.Text))
        {
            return;
        }

        line.HasDiscrepancy = true;
        line.ExpectedSkidCount = expectedSkidsBox.Value;
        line.DiscrepancyNote = noteBox.Text.Trim();
    }
}
