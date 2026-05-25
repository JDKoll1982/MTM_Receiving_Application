using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Customer Pull n' Pack report view.
/// </summary>
public sealed partial class View_Tool_CustomerPullPackReport : Page
{
    public ViewModel_Tool_CustomerPullPackReport ViewModel { get; }

    public View_Tool_CustomerPullPackReport(ViewModel_Tool_CustomerPullPackReport viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        ViewModel.ShowWaitlistEditorAsync = ShowWaitlistEditorAsync;
        InitializeComponent();
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

    private void OnDemandLineSelectionClick(object sender, RoutedEventArgs e)
    {
        if (
            sender is not CheckBox checkBox
            || checkBox.DataContext is not Model_CustomerPullPack_DemandLine demandLine
        )
        {
            return;
        }

        ViewModel.SelectDemandLineCommand.Execute(demandLine);
    }
}
