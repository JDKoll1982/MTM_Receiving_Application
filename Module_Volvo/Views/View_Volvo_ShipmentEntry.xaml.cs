using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using AppInfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_Volvo.Views;

public sealed partial class View_Volvo_ShipmentEntry : Page
{
    private bool _suppressSkidsTextChanged;

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
        await ViewModel.WaitForPendingAutoSaveAsync();
        await ShowAddPartDialogAsync();
    }

    private async void ReportDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Model_VolvoShipmentLine line)
        {
            await ViewModel.ToggleDiscrepancyCommand.ExecuteAsync(line);
        }
    }

    private async void RemoveDiscrepancyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        var xamlRoot = XamlRoot;
        if (xamlRoot == null)
        {
            return;
        }

        var confirmDialog = new ContentDialog
        {
            Title = "Remove Discrepancy",
            Content = "Remove the discrepancy for this card?",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(confirmDialog, xamlRoot);
        var result = await confirmDialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var clearResult = await ViewModel.ClearDiscrepancyAsync(line);
        if (!clearResult.IsSuccess)
        {
            ViewModel.ShowStatus(
                clearResult.ErrorMessage ?? "Failed to remove discrepancy.",
                AppInfoBarSeverity.Warning
            );
        }
    }

    private async void RemovePartButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        var xamlRoot = XamlRoot;
        if (xamlRoot == null)
        {
            return;
        }

        var confirmDialog = new ContentDialog
        {
            Title = "Remove Part",
            Content = $"Remove {line.PartNumber} from the active queue?",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(confirmDialog, xamlRoot);
        var result = await confirmDialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        var removeResult = await ViewModel.RemovePartCardAsync(line);
        if (!removeResult.IsSuccess)
        {
            ViewModel.ShowStatus(
                removeResult.ErrorMessage ?? "Failed to remove part.",
                AppInfoBarSeverity.Warning
            );
        }
    }

    private async void PartNumberButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        await ViewModel.WaitForPendingAutoSaveAsync();
        await ShowPartNumberEditDialogAsync(line);
    }

    private void ReceivedSkidCountTextBox_BeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    )
    {
        args.Cancel = args.NewText.Any(character => !char.IsDigit(character));
    }

    private async void ReceivedSkidCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressSkidsTextChanged)
        {
            return;
        }

        if (sender is not TextBox textBox || textBox.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        var trimmedText = textBox.Text?.Trim() ?? string.Empty;
        if (!int.TryParse(trimmedText, out var parsedValue) || parsedValue <= 0)
        {
            return;
        }

        var previousValue = line.ReceivedSkidCount;
        if (parsedValue == previousValue)
        {
            return;
        }

        var saveResult = await ViewModel.UpdateReceivedSkidsAsync(line, parsedValue);
        if (!saveResult.IsSuccess)
        {
            line.ReceivedSkidCount = previousValue;
            _suppressSkidsTextChanged = true;
            textBox.Text = previousValue.ToString();
            _suppressSkidsTextChanged = false;
            ViewModel.ShowStatus(
                saveResult.ErrorMessage ?? "Failed to save Skids.",
                AppInfoBarSeverity.Warning
            );
        }
    }

    private void ReceivedSkidCountTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.Tag is not Model_VolvoShipmentLine line)
        {
            return;
        }

        var previousValue = line.ReceivedSkidCount;
        var trimmedText = textBox.Text?.Trim() ?? string.Empty;
        if (!int.TryParse(trimmedText, out var parsedValue) || parsedValue <= 0)
        {
            _suppressSkidsTextChanged = true;
            textBox.Text = previousValue.ToString();
            _suppressSkidsTextChanged = false;
        }
    }

    private async Task ShowPartNumberEditDialogAsync(Model_VolvoShipmentLine line)
    {
        var xamlRoot = XamlRoot;
        if (xamlRoot == null)
        {
            return;
        }

        await ViewModel.LoadAllPartsForDialogAsync();

        var dialog = new View_Volvo_PartNumberEditDialog { XamlRoot = xamlRoot };
        dialog.Initialize(
            new Model_VolvoPartNumberEditDialog
            {
                CurrentPartNumber = line.PartNumber,
                AvailableParts = ViewModel.AvailableParts.ToList(),
                ExistingPartNumbers = ViewModel
                    .Parts.Where(part => !ReferenceEquals(part, line))
                    .Select(part => part.PartNumber)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase),
            }
        );

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary || dialog.SelectedPart == null)
        {
            return;
        }

        var updateResult = await ViewModel.UpdatePartNumberAsync(line, dialog.SelectedPart);
        if (!updateResult.IsSuccess)
        {
            ViewModel.ShowStatus(
                updateResult.ErrorMessage ?? "Failed to update the part number.",
                AppInfoBarSeverity.Warning
            );
        }
    }

    private async Task ShowAddPartDialogAsync()
    {
        var xamlRoot = XamlRoot;
        if (xamlRoot == null)
        {
            return;
        }

        await ViewModel.LoadAllPartsForDialogAsync();

        var allParts = new List<Model_VolvoPart>(ViewModel.AvailableParts);
        var errorMessage = new TextBlock
        {
            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 0, 0, 8),
        };

        var searchBox = new TextBox
        {
            PlaceholderText = "Search part numbers...",
            Margin = new Thickness(0, 0, 0, 8),
        };

        var receivedSkidsBox = new TextBox
        {
            Header = "Received Skids",
            PlaceholderText = "Enter quantity (1-99)",
        };

        var locationBox = new TextBox
        {
            Header = "Location",
            PlaceholderText = "Auto-set from Infor Visual",
            IsReadOnly = true,
            IsTabStop = false,
        };

        var partsListView = new ListView
        {
            ItemsSource = allParts,
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 220,
            DisplayMemberPath = "PartNumber",
        };

        partsListView.SelectionChanged += async (_, _) =>
        {
            if (partsListView.SelectedItem is not Model_VolvoPart selectedPart)
            {
                locationBox.Text = string.Empty;
                return;
            }

            locationBox.Text = await ViewModel.ResolvePartLocationAsync(selectedPart.PartNumber);
        };

        searchBox.TextChanged += (_, _) =>
        {
            var searchText = searchBox.Text?.Trim() ?? string.Empty;
            partsListView.ItemsSource = string.IsNullOrWhiteSpace(searchText)
                ? allParts
                : allParts
                    .Where(part =>
                        part.PartNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
        };

        var content = new StackPanel { Width = 420, Spacing = 8 };
        content.Children.Add(errorMessage);
        content.Children.Add(searchBox);
        content.Children.Add(receivedSkidsBox);
        content.Children.Add(
            new TextBlock
            {
                Text = "Available Parts",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            }
        );
        content.Children.Add(partsListView);

        var dialog = new ContentDialog
        {
            Title = "Add Part to Shipment",
            Content = content,
            PrimaryButtonText = "Add",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);

        dialog.PrimaryButtonClick += async (_, args) =>
        {
            var deferral = args.GetDeferral();
            try
            {
                errorMessage.Visibility = Visibility.Collapsed;

                if (partsListView.SelectedItem is not Model_VolvoPart selectedPart)
                {
                    args.Cancel = true;
                    errorMessage.Text = "Please select a part from the list.";
                    errorMessage.Visibility = Visibility.Visible;
                    return;
                }

                if (
                    !int.TryParse(receivedSkidsBox.Text?.Trim(), out var skidCount)
                    || skidCount <= 0
                )
                {
                    args.Cancel = true;
                    errorMessage.Text =
                        "Received skid count must be a whole number greater than 0.";
                    errorMessage.Visibility = Visibility.Visible;
                    return;
                }

                var addResult = await ViewModel.AddPartFromDialogAsync(
                    selectedPart,
                    skidCount,
                    locationBox.Text
                );

                if (!addResult.IsSuccess)
                {
                    args.Cancel = true;
                    errorMessage.Text = addResult.ErrorMessage ?? "Failed to add the part.";
                    errorMessage.Visibility = Visibility.Visible;
                }
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }
}
