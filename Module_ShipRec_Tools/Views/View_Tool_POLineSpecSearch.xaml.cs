using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// PO line binary/spec search tool view.
/// </summary>
public sealed partial class View_Tool_POLineSpecSearch : Page
{
    private readonly Dictionary<string, DataGridColumn> _columnByKey;

    public ViewModel_Tool_POLineSpecSearch ViewModel { get; }

    public View_Tool_POLineSpecSearch(ViewModel_Tool_POLineSpecSearch viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        _columnByKey = new Dictionary<string, DataGridColumn>(StringComparer.OrdinalIgnoreCase)
        {
            ["PONumber"] = ColumnPONumber,
            ["POLineNumber"] = ColumnPOLineNumber,
            ["PartId"] = ColumnPartId,
            ["VendorName"] = ColumnVendorName,
            ["VendorId"] = ColumnVendorId,
            ["VendorPartId"] = ColumnVendorPartId,
            ["QtyOrdered"] = ColumnQtyOrdered,
            ["TotalQtyReceived"] = ColumnTotalQtyReceived,
            ["PoStatus"] = ColumnPoStatus,
            ["SpecExcerpt"] = ColumnSpecExcerpt,
            ["MatchScore"] = ColumnMatchScore,
        };

        ViewModel.ShowOptionsDialogAsync = ShowOptionsDialogAsync;
        ViewModel.VisibleColumnsChanged += ApplyColumnVisibility;

        ApplyColumnVisibility(Model_Tool_POLineSpecSearchOptions.DefaultVisibleColumnKeys);
    }

    private async void OnHelpClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var helpService = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Help>();
        await helpService.ShowHelpAsync("ShipRecTools.POLineSpecSearch", XamlRoot);
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

        SearchBox.DispatcherQueue?.TryEnqueue(() =>
        {
            SearchBox.Focus(FocusState.Programmatic);
        });
    }

    private async System.Threading.Tasks.Task<Model_Tool_POLineSpecSearchOptions?> ShowOptionsDialogAsync(
        Model_Tool_POLineSpecSearchOptions currentOptions
    )
    {
        var dialog = new Dialog_POLineSpecSearchOptions(currentOptions)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? dialog.SelectedOptions : null;
    }

    private void ApplyColumnVisibility(IReadOnlyCollection<string> visibleColumnKeys)
    {
        var visible = visibleColumnKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        visible.Add("SpecExcerpt");

        foreach (var pair in _columnByKey)
        {
            pair.Value.Visibility = visible.Contains(pair.Key) ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async void ShowFullSpecTextButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: Model_Tool_POLineSpecSearchResult row })
        {
            return;
        }

        var dialog = new Dialog_POLineSpecTextViewer(row)
        {
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
    }
}
