using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Dedicated queue page for Customer Pull n' Pack waitlist work.
/// </summary>
public sealed partial class View_Tool_CustomerPullPackQueue : Page
{
    public ViewModel_Tool_CustomerPullPackQueue ViewModel { get; }

    public View_Tool_CustomerPullPackQueue(ViewModel_Tool_CustomerPullPackQueue viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        ViewModel.ShowPrintPreviewAsync = ShowPrintPreviewAsync;
        InitializeComponent();
    }

    private async System.Threading.Tasks.Task ShowPrintPreviewAsync(
        Model_CustomerPullPack_PrintContext printContext
    )
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
}
