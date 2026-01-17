using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MTM_Receiving_Application.Module_Volvo.Views;

public sealed partial class VolvoShipmentEditDialog : ContentDialog
{
    public Model_VolvoShipment Shipment { get; set; } = null!;
    public ObservableCollection<Model_VolvoShipmentLine> Lines { get; private set; }
    public ObservableCollection<Model_VolvoPart> AvailableParts { get; set; }
    
    private List<Model_VolvoPart> _allParts = new();
    private bool _addPartPanelOpen = false;

    public VolvoShipmentEditDialog()
    {
        InitializeComponent();
        Lines = new ObservableCollection<Model_VolvoShipmentLine>();
        AvailableParts = new ObservableCollection<Model_VolvoPart>();

        // Wire up button events
        ToggleAddPartButton.Click += (s, e) => ToggleAddPartPanel();
        ConfirmAddPartButton.Click += (s, e) => ConfirmAddPart();
        CancelAddPartButton.Click += (s, e) => CloseAddPartPanel();
        RemovePartButton.Click += (s, e) => RemoveSelectedLine();

        // Set DataGrid ItemsSource
        PartsDataGrid.ItemsSource = Lines;
    }

    public void LoadShipment(Model_VolvoShipment shipment, ObservableCollection<Model_VolvoShipmentLine> lines, ObservableCollection<Model_VolvoPart> availableParts)
    {
        Shipment = shipment;
        AvailableParts = availableParts;
        _allParts = new List<Model_VolvoPart>(availableParts);

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
        
        // Debug: Log part count
        System.Diagnostics.Debug.WriteLine($"[EditDialog] Loaded {_allParts.Count} parts for selection");
        
        // IMPORTANT: Set ItemsSource AFTER parts are loaded
        if (_allParts.Count > 0)
        {
            AddPartListView.ItemsSource = null; // Clear first
            AddPartListView.ItemsSource = _allParts;
            System.Diagnostics.Debug.WriteLine($"[EditDialog] ListView ItemsSource set with {_allParts.Count} parts");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[EditDialog] WARNING: No parts available to load!");
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

    private void ToggleAddPartPanel()
    {
        _addPartPanelOpen = !_addPartPanelOpen;
        
        if (_addPartPanelOpen)
        {
            // Open panel
            AddPartPanel.Visibility = Visibility.Visible;
            AddPartIcon.Symbol = Symbol.Remove;
            AddPartButtonText.Text = "Cancel";
            
            // Reset fields
            PartSearchBox.Text = string.Empty;
            AddPartQuantityBox.Text = string.Empty;
            AddPartListView.SelectedItem = null;
            AddPartErrorMessage.Visibility = Visibility.Collapsed;
            AddPartListView.ItemsSource = _allParts;
        }
        else
        {
            CloseAddPartPanel();
        }
    }

    private void CloseAddPartPanel()
    {
        _addPartPanelOpen = false;
        AddPartPanel.Visibility = Visibility.Collapsed;
        AddPartIcon.Symbol = Symbol.Add;
        AddPartButtonText.Text = "Add Part";
        AddPartErrorMessage.Visibility = Visibility.Collapsed;
    }

    private void OnPartSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = PartSearchBox.Text?.ToLower() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            AddPartListView.ItemsSource = _allParts;
            return;
        }

        // Fuzzy search: matches if characters appear in order
        var filtered = _allParts.Where(part =>
        {
            var partNumber = part.PartNumber.ToLower();
            int searchIndex = 0;
            
            foreach (char c in partNumber)
            {
                if (searchIndex < searchText.Length && c == searchText[searchIndex])
                {
                    searchIndex++;
                }
            }
            
            return searchIndex == searchText.Length || partNumber.Contains(searchText);
        }).ToList();

        AddPartListView.ItemsSource = filtered;
    }

    private void ConfirmAddPart()
    {
        // Hide previous error
        AddPartErrorMessage.Visibility = Visibility.Collapsed;

        // Validate selection
        if (AddPartListView.SelectedItem is not Model_VolvoPart selectedPart)
        {
            AddPartErrorMessage.Text = "Please select a part from the list.";
            AddPartErrorMessage.Visibility = Visibility.Visible;
            return;
        }

        // Validate skid count
        if (!int.TryParse(AddPartQuantityBox.Text, out int skidCount) || skidCount < 1 || skidCount > 99)
        {
            AddPartErrorMessage.Text = "Received skid count must be a number between 1 and 99.";
            AddPartErrorMessage.Visibility = Visibility.Visible;
            return;
        }

        // Check for duplicate
        if (Lines.Any(p => p.PartNumber.Equals(selectedPart.PartNumber, StringComparison.OrdinalIgnoreCase)))
        {
            AddPartErrorMessage.Text = $"Part {selectedPart.PartNumber} is already in this shipment. Remove it first to update the quantity.";
            AddPartErrorMessage.Visibility = Visibility.Visible;
            return;
        }

        // Add the part - validation passed
        var calculatedPieces = selectedPart.QuantityPerSkid * skidCount;
        var newLine = new Model_VolvoShipmentLine
        {
            ShipmentId = Shipment.Id,
            PartNumber = selectedPart.PartNumber,
            QuantityPerSkid = selectedPart.QuantityPerSkid,
            ReceivedSkidCount = skidCount,
            CalculatedPieceCount = calculatedPieces,
            HasDiscrepancy = false,
            ExpectedSkidCount = null,
            DiscrepancyNote = string.Empty
        };

        Lines.Add(newLine);
        CloseAddPartPanel();
    }

    private void RemoveSelectedLine()
    {
        // Clear any previous errors
        ValidationErrorBar.IsOpen = false;

        if (PartsDataGrid.SelectedItem is not Model_VolvoShipmentLine selectedLine)
        {
            ValidationErrorBar.Message = "Please select a part from the grid to remove.";
            ValidationErrorBar.IsOpen = true;
            return;
        }

        // Prevent deleting the last part
        if (Lines.Count <= 1)
        {
            ValidationErrorBar.Title = "Cannot Remove Part";
            ValidationErrorBar.Message = "A shipment must have at least one part. Add another part before removing this one.";
            ValidationErrorBar.IsOpen = true;
            return;
        }

        // Remove the part (no confirmation needed - user can use Cancel button to undo)
        Lines.Remove(selectedLine);
        
        // Show success message briefly
        ValidationErrorBar.Severity = InfoBarSeverity.Success;
        ValidationErrorBar.Title = "Part Removed";
        ValidationErrorBar.Message = $"{selectedLine.PartNumber} has been removed from the shipment.";
        ValidationErrorBar.IsOpen = true;
        
        // Auto-hide success message after 3 seconds
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) =>
        {
            ValidationErrorBar.IsOpen = false;
            ValidationErrorBar.Severity = InfoBarSeverity.Error; // Reset to error
            ValidationErrorBar.Title = string.Empty;
            timer.Stop();
        };
        timer.Start();
    }

    private async void ReportDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        if (line.HasDiscrepancy)
        {
            line.HasDiscrepancy = false;
            return;
        }

        var expectedSkidsBox = new NumberBox
        {
            Header = "Expected Skids",
            Minimum = 1,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
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

    private async void ViewDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        if (!line.HasDiscrepancy)
        {
            return;
        }

        var content = new StackPanel { Spacing = 12 };
        
        content.Children.Add(new TextBlock
        {
            Text = $"Part: {line.PartNumber}",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            FontSize = 16
        });

        content.Children.Add(new TextBlock
        {
            Text = $"Received Skids: {line.ReceivedSkidCount}",
            FontSize = 14
        });

        content.Children.Add(new TextBlock
        {
            Text = $"Expected Skids: {line.ExpectedSkidCount:F2}",
            FontSize = 14
        });

        content.Children.Add(new TextBlock
        {
            Text = $"Received Pieces: {line.CalculatedPieceCount}",
            FontSize = 14
        });

        if (line.ExpectedPieceCount.HasValue)
        {
            content.Children.Add(new TextBlock
            {
                Text = $"Expected Pieces: {line.ExpectedPieceCount.Value}",
                FontSize = 14
            });

            if (line.PieceDifference.HasValue)
            {
                var diff = line.PieceDifference.Value;
                var diffText = diff > 0 ? $"+{diff}" : diff.ToString();
                content.Children.Add(new TextBlock
                {
                    Text = $"Difference: {diffText} pieces",
                    FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = diff < 0 ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red) : 
                                 diff > 0 ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange) :
                                 new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(line.DiscrepancyNote))
        {
            content.Children.Add(new TextBlock
            {
                Text = "Note:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4),
                FontSize = 14
            });

            content.Children.Add(new TextBox
            {
                Text = line.DiscrepancyNote,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 60,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)
            });
        }

        var dialog = new ContentDialog
        {
            Title = "Discrepancy Details",
            Content = content,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async void RemoveDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

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
    }
}
