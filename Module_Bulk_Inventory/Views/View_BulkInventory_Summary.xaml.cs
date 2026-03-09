using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Bulk_Inventory.ViewModels;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Views;

/// <summary>
/// Post-automation summary: shows outcome badges and failed-row re-push checklist.
/// </summary>
public sealed partial class View_BulkInventory_Summary : Page
{
    public ViewModel_BulkInventory_Summary? ViewModel { get; set; }

    public View_BulkInventory_Summary()
    {
        InitializeComponent();
    }
}
