using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Read-only card-based material availability board view.
/// </summary>
public sealed partial class View_Tool_MaterialAvailabilityBoard : Page
{
    public ViewModel_Tool_MaterialAvailabilityBoard ViewModel { get; }

    public View_Tool_MaterialAvailabilityBoard(ViewModel_Tool_MaterialAvailabilityBoard viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ShowFuzzyPickerAsync = ShowFuzzyPickerDialogAsync;
        ViewModel.RequestPrintAsync = OpenPrintDocumentAsync;
        ViewModel.ShowIncomingMaterialDetailsAsync = ShowIncomingDetailsDialogAsync;
        ViewModel.ShowWorkOrderDetailsDialogAsync = ShowWorkOrderDetailsDialogAsync;
    }

    private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerDialogAsync(
        IReadOnlyList<Model_FuzzySearchResult> candidates,
        string title
    )
    {
        var dialog = new Dialog_FuzzySearchPicker(
            candidates,
            title,
            subtitle: $"{candidates.Count} possible matches — select the correct one."
        )
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? dialog.SelectedResult : null;
    }

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.SearchCommand.CanExecute(null))
        {
            ViewModel.SearchCommand.Execute(null);
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ClearCommand.CanExecute(null))
        {
            ViewModel.ClearCommand.Execute(null);
        }

        SearchBox.DispatcherQueue?.TryEnqueue(() => SearchBox.Focus(FocusState.Programmatic));
    }

    private async void IncomingDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Model_Tool_MaterialAvailabilityCard card })
        {
            return;
        }

        if (ViewModel.ShowIncomingDetailsCommand.CanExecute(card))
        {
            await ViewModel.ShowIncomingDetailsCommand.ExecuteAsync(card);
        }
    }

    private async void WorkOrderDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Model_Tool_MaterialAvailabilityCard card })
        {
            return;
        }

        if (ViewModel.ShowWorkOrderDetailsCommand.CanExecute(card))
        {
            await ViewModel.ShowWorkOrderDetailsCommand.ExecuteAsync(card);
        }
    }

    private async Task ShowIncomingDetailsDialogAsync(
        ViewModel_Dialog_MaterialAvailabilityIncomingDetails dialogViewModel
    )
    {
        var dialog = new Dialog_MaterialAvailabilityIncomingDetails(dialogViewModel)
        {
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
    }

    private async Task ShowWorkOrderDetailsDialogAsync(
        ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails dialogViewModel
    )
    {
        var dialog = new Dialog_MaterialAvailabilityWorkOrderDetails(dialogViewModel)
        {
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
    }

    private static async Task<Model_Dao_Result<bool>> OpenPrintDocumentAsync(
        Model_FormattedReportDocument document
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(document);

            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"mtm-material-availability-board-{DateTime.Now:yyyyMMdd-HHmmss}.html"
            );
            var printablePage = BuildPrintablePage(document.HtmlFragment);

            await File.WriteAllTextAsync(filePath, printablePage, Encoding.UTF8);

            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });

            return Model_Dao_Result_Factory.Success(true);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Failed to open a print-ready Material Availability Board: {ex.Message}",
                ex
            );
        }
    }

    private static string BuildPrintablePage(string htmlFragment)
    {
        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <title>Material Availability Board</title>
    <style>
        @page { margin: 0.5in; }
        body { margin: 0; background: #ffffff; }
        @media print {
            body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        }
    </style>
    <script>
        window.addEventListener('load', function () {
            window.setTimeout(function () { window.print(); }, 250);
        });
    </script>
</head>
<body>
{{htmlFragment}}
</body>
</html>
""";
    }
}
