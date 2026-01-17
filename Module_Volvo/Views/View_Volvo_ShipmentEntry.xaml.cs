using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Page for Volvo shipment entry
/// </summary>
public sealed partial class View_Volvo_ShipmentEntry : Page
{
    public ViewModel_Volvo_ShipmentEntry ViewModel { get; }

    public View_Volvo_ShipmentEntry()
    {
        ViewModel = App.GetService<ViewModel_Volvo_ShipmentEntry>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private async void AddPartButton_Click(object sender, RoutedEventArgs e)
    {
        await ShowAddPartDialogAsync();
    }

    private async void RemoveSelectedPartButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedPart == null)
        {
            var noSelectionDialog = new ContentDialog
            {
                Title = "No Part Selected",
                Content = "Please select a part from the grid to remove.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await noSelectionDialog.ShowAsync();
            return;
        }

        var confirmDialog = new ContentDialog
        {
            Title = "Remove Part",
            Content = $"Are you sure you want to remove {ViewModel.SelectedPart.PartNumber} from this shipment?",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.Parts.Remove(ViewModel.SelectedPart);
        }
    }

    private async void ReportDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Model_VolvoShipmentLine line)
        {
            await ViewModel.ToggleDiscrepancyCommand.ExecuteAsync(line);
        }
    }

    private async void ViewDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Model_VolvoShipmentLine line)
        {
            await ShowViewDiscrepancyDialogAsync(line);
        }
    }

    private async void RemoveDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        var xamlRoot = this.XamlRoot;
        if (xamlRoot == null)
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
            XamlRoot = xamlRoot
        };

        var confirmResult = await confirmDialog.ShowAsync();
        if (confirmResult == ContentDialogResult.Primary)
        {
            line.HasDiscrepancy = false;
        }
    }

    private async Task ShowViewDiscrepancyDialogAsync(Model_VolvoShipmentLine line)
    {
        var xamlRoot = this.XamlRoot;
        if (xamlRoot == null)
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
            XamlRoot = xamlRoot
        };

        await dialog.ShowAsync();
    }

    private async Task ShowAddPartDialogAsync()
    {
        var xamlRoot = this.XamlRoot;
        if (xamlRoot == null)
        {
            return;
        }

        // Load all parts
        await ViewModel.LoadAllPartsForDialogAsync();
        
        var allParts = new List<Model_VolvoPart>(ViewModel.AvailableParts);
        var filteredParts = new List<Model_VolvoPart>(allParts);

        // Create error message display
        var errorMessage = new TextBlock
        {
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 0, 0, 12)
        };

        // Create search box
        var searchBox = new TextBox
        {
            PlaceholderText = "Search part numbers...",
            Margin = new Thickness(0, 0, 0, 12)
        };

        // Create ListView for parts
        var partsListView = new ListView
        {
            ItemsSource = filteredParts,
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 300,
            Margin = new Thickness(0, 0, 0, 12),
            DisplayMemberPath = "PartNumber"  // Show PartNumber property for each item
        };

        // Create received skids input
        var receivedSkidsBox = new TextBox
        {
            Header = "Received Skids",
            PlaceholderText = "Enter quantity (1-99)",
            InputScope = new InputScope { Names = { new InputScopeName(InputScopeNameValue.Number) } }
        };

        // Search handler with fuzzy filtering
        searchBox.TextChanged += (s, args) =>
        {
            var searchText = searchBox.Text?.ToLower() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                partsListView.ItemsSource = allParts;
                return;
            }

            // Fuzzy search: matches if characters appear in order
            var filtered = allParts.Where(part =>
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

            partsListView.ItemsSource = filtered;
        };

        // Build dialog content
        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(errorMessage);  // Error message at top (hidden by default)
        content.Children.Add(searchBox);
        
        var partsHeader = new TextBlock
        {
            Text = "Available Parts",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        };
        content.Children.Add(partsHeader);
        content.Children.Add(partsListView);
        content.Children.Add(receivedSkidsBox);

        var dialog = new ContentDialog
        {
            Title = "Add Part to Shipment",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        dialog.PrimaryButtonClick += async (s, args) =>
        {
            var deferral = args.GetDeferral();
            
            try
            {
                // Hide previous error
                errorMessage.Visibility = Visibility.Collapsed;

                // Validate selection
                if (partsListView.SelectedItem is not Model_VolvoPart selectedPart)
                {
                    args.Cancel = true;
                    errorMessage.Text = "Please select a part from the list.";
                    errorMessage.Visibility = Visibility.Visible;
                    return;
                }

                // Validate skid count
                if (!int.TryParse(receivedSkidsBox.Text, out int skidCount) || skidCount < 1 || skidCount > 99)
                {
                    args.Cancel = true;
                    errorMessage.Text = "Received skid count must be a number between 1 and 99.";
                    errorMessage.Visibility = Visibility.Visible;
                    return;
                }

                // Check for duplicate
                if (ViewModel.Parts.Any(p => p.PartNumber.Equals(selectedPart.PartNumber, StringComparison.OrdinalIgnoreCase)))
                {
                    args.Cancel = true;
                    errorMessage.Text = $"Part {selectedPart.PartNumber} is already in this shipment. Remove it first to update the quantity.";
                    errorMessage.Visibility = Visibility.Visible;
                    return;
                }

                // Add the part - validation passed
                var calculatedPieces = selectedPart.QuantityPerSkid * skidCount;
                var newLine = new Model_VolvoShipmentLine
                {
                    PartNumber = selectedPart.PartNumber,
                    QuantityPerSkid = selectedPart.QuantityPerSkid,
                    ReceivedSkidCount = skidCount,
                    CalculatedPieceCount = calculatedPieces,
                    HasDiscrepancy = false,
                    ExpectedSkidCount = null,
                    DiscrepancyNote = string.Empty
                };

                ViewModel.Parts.Add(newLine);
                await ViewModel.Logger.LogInfoAsync($"User added part {selectedPart.PartNumber}, {skidCount} skids ({calculatedPieces} pcs)");
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }
}
