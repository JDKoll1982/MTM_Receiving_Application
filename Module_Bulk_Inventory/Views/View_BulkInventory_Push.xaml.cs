using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Bulk_Inventory.ViewModels;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Views;

/// <summary>
/// Full-screen automation-progress overlay.
/// Wires F6 (skip row) and Escape (cancel) keyboard shortcuts to the ViewModel.
/// </summary>
public sealed partial class View_BulkInventory_Push : Page
{
    public ViewModel_BulkInventory_Push? ViewModel { get; set; }

    public View_BulkInventory_Push()
    {
        InitializeComponent();
    }

    private void SkipRow_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.RequestSkipCurrentRow();
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        base.OnKeyDown(e);

        if (ViewModel is null)
            return;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.F6:
                ViewModel.RequestSkipCurrentRow();
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Escape:
                ViewModel.CancelPushCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
