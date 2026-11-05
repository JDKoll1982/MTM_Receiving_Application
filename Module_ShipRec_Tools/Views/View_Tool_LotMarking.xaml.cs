using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Windows.System;
using WinRT.Interop;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Heat / Lot Management tool view. Business logic stays in the ViewModel; this file only supplies
/// the palette and confirmation dialogs, the two modal windows, the print hand-off, the help call,
/// and the Enter key on the search box.
/// </summary>
public sealed partial class View_Tool_LotMarking : Page
{
    public ViewModel_Tool_LotMarking ViewModel { get; }

    public View_Tool_LotMarking(ViewModel_Tool_LotMarking viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ConfirmRealignAsync = ConfirmRealignDialogAsync;
        ViewModel.RequestPrintAsync = PrintMarkingsDocumentAsync;
    }

    private async void OnHelpClick(object sender, RoutedEventArgs e)
    {
        var helpService = App.GetService<IService_Help>();
        await helpService.ShowHelpAsync("ShipRecTools.LotMarking", XamlRoot);
    }

    private async void OnShowPaletteClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Dialog_LotMarkingPalette(
            ViewModel.LegendShapes,
            ViewModel.LegendColors,
            ViewModel.PaletteSummary
        )
        {
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Opens the marking editor for the lot the user picked. The button that reaches this is only
    /// enabled while a chosen lot has a recorded marking, so there is always something to edit.
    /// </summary>
    private async void OnChangeMarkingClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.CanChangeSelectedMarking || App.MainWindow is not { } owner)
        {
            return;
        }

        var window = new View_Tool_LotMarkingChangeWindow(
            ViewModel,
            WindowNative.GetWindowHandle(owner)
        );

        await window.ShowModalAsync();
    }

    /// <summary>
    /// Opens the list of part numbers that receive markings. Editing that list changes which lots the
    /// page answers for, so the list is read again on the way out.
    /// </summary>
    private async void OnManagePartsClick(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow is not { } owner)
        {
            return;
        }

        var window = new View_Tool_LotMarkingPartsWindow(
            ViewModel,
            WindowNative.GetWindowHandle(owner)
        );

        await window.ShowModalAsync();
        await ViewModel.ReloadAfterPartScopeChangeAsync();
    }

    private async void OnPrintClick(object sender, RoutedEventArgs e) =>
        await ViewModel.PrintMarkingsAsync();

    /// <summary>
    /// Opens the Filter window. The window reports whether Apply was pressed, and the choices are only
    /// saved and the list only re-read when it was, so cancelling changes nothing and costs no read.
    /// </summary>
    private async void OnFiltersClick(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow is not { } owner)
        {
            return;
        }

        var window = new View_Tool_LotMarkingFilterWindow(
            ViewModel,
            WindowNative.GetWindowHandle(owner)
        );

        if (await window.ShowModalAsync())
        {
            await ViewModel.ApplyFiltersAsync();
        }
    }

    private async Task<bool> ConfirmRealignDialogAsync(Model_Tool_LotMarkingRealignPreview preview)
    {
        var dialog = new Dialog_LotMarkingRealignConfirm(preview) { XamlRoot = XamlRoot };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private static Task<Model_Dao_Result<bool>> PrintMarkingsDocumentAsync(
        Model_FormattedReportDocument document
    )
    {
        var html = Helper_PrintDocumentHtml.BuildPage(document, autoPrint: true);
        return Helper_HtmlReportExport.WriteAndOpenAsync("heat-lot-markings", html);
    }

    private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.SearchMarkingsCommand.CanExecute(null))
        {
            ViewModel.SearchMarkingsCommand.Execute(null);
        }
    }
}
