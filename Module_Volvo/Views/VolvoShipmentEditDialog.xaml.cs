using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Views;

public sealed partial class VolvoShipmentEditDialog : ContentDialog
{
    public Model_VolvoShipment Shipment { get; set; } = null!;
    public ObservableCollection<Model_VolvoShipmentLine> Lines { get; private set; }
    public ObservableCollection<Model_VolvoPart> AvailableParts { get; set; }

    public bool IsReadOnlyMode { get; private set; }

    public bool IsEditableMode => !IsReadOnlyMode;

    public Visibility ActionControlsVisibility =>
        IsReadOnlyMode ? Visibility.Collapsed : Visibility.Visible;

    private List<Model_VolvoPart> _allParts = new();
    private bool _addPartPanelOpen = false;
    private readonly Func<string, Task<string>> _resolvePartLocationAsync;

    public VolvoShipmentEditDialog(Func<string, Task<string>> resolvePartLocationAsync)
    {
        _resolvePartLocationAsync =
            resolvePartLocationAsync
            ?? throw new ArgumentNullException(nameof(resolvePartLocationAsync));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
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

    
    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }
        var availableWidth = System.Math.Max(1180, XamlRoot.Size.Width - 32);
        var availableHeight = System.Math.Max(680, XamlRoot.Size.Height - 48);

        // Avoid pre-measuring the root grid here. This dialog hosts a CommunityToolkit DataGrid,
        // and forcing a measure before the dialog is attached to the visual tree can trigger
        // DataGrid column sizing code that relies on unsupported GetForCurrentView APIs.
        Width = availableWidth;
        MinWidth = System.Math.Min(1180, availableWidth);
        MinHeight = System.Math.Min(680, availableHeight);
        MaxHeight = availableHeight;
    }

    public void ConfigureMode(bool isReadOnly)
    {
        IsReadOnlyMode = isReadOnly;
        Title = isReadOnly ? "Archived Shipment" : "Edit Shipment";
        PrimaryButtonText = isReadOnly ? string.Empty : "Save Changes";
        CloseButtonText = isReadOnly ? "Close" : "Cancel";
        DefaultButton = isReadOnly ? ContentDialogButton.Close : ContentDialogButton.Primary;

        if (isReadOnly)
        {
            ValidationErrorBar.IsOpen = false;
            AddPartPanel.Visibility = Visibility.Collapsed;
        }

        Bindings.Update();
    }

    public void LoadShipment(
        Model_VolvoShipment shipment,
        ObservableCollection<Model_VolvoShipmentLine> lines,
        ObservableCollection<Model_VolvoPart> availableParts
    )
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
        System.Diagnostics.Debug.WriteLine(
            $"[EditDialog] Loaded {_allParts.Count} parts for selection"
        );

        // IMPORTANT: Set ItemsSource AFTER parts are loaded
        if (_allParts.Count > 0)
        {
            AddPartListView.ItemsSource = null; // Clear first
            AddPartListView.ItemsSource = _allParts;
            System.Diagnostics.Debug.WriteLine(
                $"[EditDialog] ListView ItemsSource set with {_allParts.Count} parts"
            );
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
        Shipment.ReceiverNumber = string.IsNullOrWhiteSpace(ReceiverNumberBox.Text)
            ? null
            : ReceiverNumberBox.Text;
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
            AddPartLocationBox.Text = string.Empty;
            AddPartListView.SelectedItem = null;
            AddPartErrorMessage.Visibility = Visibility.Collapsed;
            AddPartListView.ItemsSource = _allParts;
            DispatcherQueue.TryEnqueue(() => PartSearchBox.Focus(FocusState.Programmatic));
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
        AddPartLocationBox.Text = string.Empty;
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
        var filtered = _allParts
            .Where(part =>
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
            })
            .ToList();

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
        if (
            !int.TryParse(AddPartQuantityBox.Text, out int skidCount)
            || skidCount < 1
            || skidCount > 99
        )
        {
            AddPartErrorMessage.Text = "Received skid count must be a number between 1 and 99.";
            AddPartErrorMessage.Visibility = Visibility.Visible;
            return;
        }

        // Check for duplicate
        if (
            Lines.Any(p =>
                p.PartNumber.Equals(selectedPart.PartNumber, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            AddPartErrorMessage.Text =
                $"Part {selectedPart.PartNumber} is already in this shipment. Remove it first to update the quantity.";
            AddPartErrorMessage.Visibility = Visibility.Visible;
            return;
        }

        // Add the part - validation passed
        var calculatedPieces = selectedPart.QuantityPerSkid * skidCount;
        var newLine = new Model_VolvoShipmentLine
        {
            ShipmentId = Shipment.Id,
            PartNumber = selectedPart.PartNumber,
            Location = AddPartLocationBox.Text?.Trim() ?? string.Empty,
            QuantityPerSkid = selectedPart.QuantityPerSkid,
            ReceivedSkidCount = skidCount,
            CalculatedPieceCount = calculatedPieces,
            HasDiscrepancy = false,
            ExpectedSkidCount = null,
            DiscrepancyNote = string.Empty,
        };

        Lines.Add(newLine);
        CloseAddPartPanel();
    }

    private async void AddPartListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AddPartListView.SelectedItem is not Model_VolvoPart selectedPart)
        {
            AddPartLocationBox.Text = string.Empty;
            return;
        }

        AddPartLocationBox.Text = await _resolvePartLocationAsync(selectedPart.PartNumber);
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
            ValidationErrorBar.Message =
                "A shipment must have at least one part. Add another part before removing this one.";
            ValidationErrorBar.IsOpen = true;
            return;
        }

        // Remove the part (no confirmation needed - user can use Cancel button to undo)
        Lines.Remove(selectedLine);

        // Show success message briefly
        ValidationErrorBar.Severity = InfoBarSeverity.Success;
        ValidationErrorBar.Title = "Part Removed";
        ValidationErrorBar.Message =
            $"{selectedLine.PartNumber} has been removed from the shipment.";
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
            Value = line.ExpectedSkidCount ?? 1,
        };

        var noteBox = new TextBox
        {
            Header = "Discrepancy Note",
            PlaceholderText = "Explain discrepancy",
            Text = line.DiscrepancyNote ?? string.Empty,
        };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(expectedSkidsBox);
        panel.Children.Add(noteBox);
        var validationText = new TextBlock
        {
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Red),
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed,
        };
        panel.Children.Add(validationText);

        Flyout? flyout = null;
        var saveButton = new Button
        {
            Content = "Save",
            Style = Application.Current.Resources["AccentButtonStyle"] as Style,
            MinWidth = 96,
        };
        var cancelButton = new Button { Content = "Cancel", MinWidth = 96 };

        saveButton.Click += (_, _) =>
        {
            if (expectedSkidsBox.Value < 1 || string.IsNullOrWhiteSpace(noteBox.Text))
            {
                validationText.Text =
                    "Expected skids must be greater than zero and a discrepancy note is required.";
                validationText.Visibility = Visibility.Visible;
                return;
            }

            line.HasDiscrepancy = true;
            line.ExpectedSkidCount = expectedSkidsBox.Value;
            line.DiscrepancyNote = noteBox.Text.Trim();
            flyout?.Hide();
        };

        cancelButton.Click += (_, _) => flyout?.Hide();

        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };
        actionPanel.Children.Add(saveButton);
        actionPanel.Children.Add(cancelButton);

        var layoutPanel = CreateFlyoutLayout("Report Discrepancy", panel, actionPanel);

        flyout = CreateFlyout(layoutPanel);
        flyout.ShowAt(button);

        await Task.CompletedTask;
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

        content.Children.Add(
            new TextBlock
            {
                Text = $"Part: {line.PartNumber}",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
            }
        );

        content.Children.Add(
            new TextBlock { Text = $"Received Skids: {line.ReceivedSkidCount}", FontSize = 14 }
        );

        content.Children.Add(
            new TextBlock { Text = $"Expected Skids: {line.ExpectedSkidCount:F2}", FontSize = 14 }
        );

        content.Children.Add(
            new TextBlock { Text = $"Received Pieces: {line.CalculatedPieceCount}", FontSize = 14 }
        );

        if (line.ExpectedPieceCount.HasValue)
        {
            content.Children.Add(
                new TextBlock
                {
                    Text = $"Expected Pieces: {line.ExpectedPieceCount.Value}",
                    FontSize = 14,
                }
            );

            if (line.PieceDifference.HasValue)
            {
                var diff = line.PieceDifference.Value;
                var diffText = diff > 0 ? $"+{diff}" : diff.ToString();
                content.Children.Add(
                    new TextBlock
                    {
                        Text = $"Difference: {diffText} pieces",
                        FontSize = 14,
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                        Foreground =
                            diff < 0
                                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                    Microsoft.UI.Colors.Green
                                )
                            : diff > 0
                                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                    Microsoft.UI.Colors.Red
                                )
                            : new Microsoft.UI.Xaml.Media.SolidColorBrush(
                                Microsoft.UI.Colors.Green
                            ),
                    }
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(line.DiscrepancyNote))
        {
            content.Children.Add(
                new TextBlock
                {
                    Text = "Note:",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 8, 0, 4),
                    FontSize = 14,
                }
            );

            content.Children.Add(
                new TextBox
                {
                    Text = line.DiscrepancyNote,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    MinHeight = 60,
                    Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Transparent
                    ),
                }
            );
        }

        Flyout? flyout = null;
        var closeButton = new Button
        {
            Content = "Close",
            HorizontalAlignment = HorizontalAlignment.Right,
            MinWidth = 96,
        };
        closeButton.Click += (_, _) => flyout?.Hide();

        flyout = CreateFlyout(CreateFlyoutLayout("Discrepancy Details", content, closeButton));
        flyout.ShowAt(button);

        await Task.CompletedTask;
    }

    private async void RemoveDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        Flyout? flyout = null;
        var message = new TextBlock
        {
            Text = "Remove the discrepancy for this line?",
            TextWrapping = TextWrapping.Wrap,
        };
        var removeButton = new Button
        {
            Content = "Remove",
            Style = Application.Current.Resources["AccentButtonStyle"] as Style,
            MinWidth = 96,
        };
        var cancelButton = new Button { Content = "Cancel", MinWidth = 96 };

        removeButton.Click += (_, _) =>
        {
            line.HasDiscrepancy = false;
            line.ExpectedSkidCount = null;
            line.DiscrepancyNote = null;
            flyout?.Hide();
        };
        cancelButton.Click += (_, _) => flyout?.Hide();

        var actionPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };
        actionPanel.Children.Add(removeButton);
        actionPanel.Children.Add(cancelButton);

        flyout = CreateFlyout(CreateFlyoutLayout("Remove Discrepancy", message, actionPanel));
        flyout.ShowAt(button);

        await Task.CompletedTask;
    }

    private static Flyout CreateFlyout(UIElement content)
    {
        return new Flyout
        {
            Placement = Microsoft
                .UI
                .Xaml
                .Controls
                .Primitives
                .FlyoutPlacementMode
                .BottomEdgeAlignedLeft,
            ShouldConstrainToRootBounds = true,
            Content = new Border
            {
                Width = 420,
                MaxHeight = 620,
                Padding = new Thickness(16),
                BorderThickness = new Thickness(1),
                BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.LightGray),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.White),
                Child = content,
            },
        };
    }

    private static StackPanel CreateFlyoutLayout(string title, UIElement content, UIElement footer)
    {
        var layoutPanel = new StackPanel { Spacing = 12 };
        layoutPanel.Children.Add(
            new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
            }
        );
        layoutPanel.Children.Add(content);
        layoutPanel.Children.Add(footer);
        return layoutPanel;
    }
}
