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
}
