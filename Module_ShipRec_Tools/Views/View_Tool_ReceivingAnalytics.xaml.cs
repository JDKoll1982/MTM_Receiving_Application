using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Receiving Analytics tool view. All business logic lives in the ViewModel;
/// this code-behind wires the HTML export delegate, closes the filter popover,
/// and sets the DataContext.
/// </summary>
public sealed partial class View_Tool_ReceivingAnalytics : Page
{
    public ViewModel_Tool_ReceivingAnalytics ViewModel { get; }

    public View_Tool_ReceivingAnalytics(ViewModel_Tool_ReceivingAnalytics viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.RequestExportAsync = OpenExportDocumentAsync;
    }

    /// <summary>
    /// Applies the popover filter selections: closes the flyout and re-runs the load.
    /// </summary>
    private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
    {
        if (FiltersButton.Flyout is FlyoutBase flyout)
        {
            flyout.Hide();
        }

        ViewModel.LoadCommand.Execute(null);
    }

    private static async Task<Model_Dao_Result<bool>> OpenExportDocumentAsync(
        Model_FormattedReportDocument document
    )
    {
        var html = Helper_PrintDocumentHtml.BuildPage(document, autoPrint: false);
        return await Helper_HtmlReportExport.WriteAndOpenAsync("receiving-analytics", html);
    }
}
