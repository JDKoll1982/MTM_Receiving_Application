using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Views.Controls;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Customer Pull n' Pack report view.
/// </summary>
public sealed partial class View_Tool_CustomerPullPackReport : Page
{
    private Func<Task>? _showWaitlistQueueAsync;
    private string _customerSearchTextAtFocusGain = string.Empty;

    public ViewModel_Tool_CustomerPullPackReport ViewModel { get; }

    public View_Tool_CustomerPullPackReport(ViewModel_Tool_CustomerPullPackReport viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        ViewModel.ShowWaitlistEditorAsync = ShowWaitlistEditorAsync;
        ViewModel.ShowOverSelectedQuantityConfirmationAsync =
            ShowOverSelectedQuantityConfirmationAsync;
        ViewModel.ShowWaitlistQueueAsync = ShowWaitlistQueueAsync;
        ViewModel.ShowDefaultsAsync = ShowDefaultsAsync;
        ViewModel.ShowPrintPreviewAsync = ShowPrintPreviewAsync;
        ViewModel.ShowFuzzyPickerAsync = ShowFuzzyPickerDialogAsync;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public void ConfigureWaitlistQueueNavigation(Func<Task> showWaitlistQueueAsync)
    {
        ArgumentNullException.ThrowIfNull(showWaitlistQueueAsync);
        _showWaitlistQueueAsync = showWaitlistQueueAsync;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        await ViewModel.LoadUserDefaultsAsync();

        if (string.IsNullOrWhiteSpace(ViewModel.CustomerSearchText) is false)
        {
            await ViewModel.RefreshReportCommand.ExecuteAsync(null);
        }
    }

    private async System.Threading.Tasks.Task<bool> ShowWaitlistEditorAsync(
        ViewModel_Dialog_CustomerPullPackWaitlistEditor dialogViewModel
    )
    {
        var dialog = new Dialog_CustomerPullPackWaitlistEditor(dialogViewModel)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private async Task<bool> ShowOverSelectedQuantityConfirmationAsync(
        decimal quantitySelected,
        decimal quantityToPack
    )
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Selected quantity looks high",
            PrimaryButtonText = "Continue",
            CloseButtonText = "Review Selection",
            DefaultButton = ContentDialogButton.Close,
            Content =
                $"QTY SELECTED is {quantitySelected:0.##}, which is more than 20% above QTY TO PACK at {quantityToPack:0.##}. Make sure you did not select too many locations before continuing.",
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private async Task ShowWaitlistQueueAsync()
    {
        if (_showWaitlistQueueAsync is null)
        {
            ViewModel.ShowStatus(
                "The waitlist page is not available in the current report host.",
                Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            return;
        }

        await _showWaitlistQueueAsync();
    }

    private async Task<Model_CustomerPullPack_UserDefaults?> ShowDefaultsAsync(
        Model_CustomerPullPack_UserDefaults defaults
    )
    {
        var defaultsView = new View_Tool_CustomerPullPackDefaults(defaults);
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Customer Pull n' Pack Defaults",
            PrimaryButtonText = "Save Defaults",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            Content = defaultsView,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? defaultsView.BuildDefaults() : null;
    }

    private async Task ShowPrintPreviewAsync(Model_CustomerPullPack_PrintContext printContext)
    {
        FrameworkElement content =
            printContext.PrintMode == Enum_CustomerPullPackPrintMode.PullList
                ? new View_Tool_CustomerPullPackPullList(printContext)
                : new View_Tool_CustomerPullPackFloorCopy(printContext);

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = printContext.Title,
            PrimaryButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            FullSizeDesired = true,
            Content = content,
        };

        await dialog.ShowAsync();
    }

    private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerDialogAsync(
        System.Collections.Generic.IReadOnlyList<Model_FuzzySearchResult> candidates,
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

    private void OnCustomerSearchBoxGotFocus(object sender, RoutedEventArgs e)
    {
        _customerSearchTextAtFocusGain = ViewModel.CustomerSearchText;
    }

    private async void OnCustomerSearchBoxLostFocus(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResolveCustomerSearchTextOnBlurAsync(_customerSearchTextAtFocusGain);
    }

    private void OnCrystalRequestLineSelectionChanged(
        object sender,
        CrystalRequestLineSelectionChangedEventArgs e
    )
    {
        ViewModel.ApplyCrystalRequestLineSelection(e.SelectedSourceLineKeys);
    }

    private void OnCrystalLocationSelectionChanged(
        object sender,
        CrystalLocationSelectionChangedEventArgs e
    )
    {
        ViewModel.ApplyCrystalLocationSelection(e.GroupKey, e.SelectedLocationIds);
    }
}
