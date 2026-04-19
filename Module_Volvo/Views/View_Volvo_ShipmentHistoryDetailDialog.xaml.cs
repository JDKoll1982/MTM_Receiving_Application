using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Dialog showing formatted shipment history details.
/// </summary>
public sealed partial class View_Volvo_ShipmentHistoryDetailDialog : ContentDialog
{
    public ViewModel_Volvo_ShipmentHistoryDetailDialog ViewModel { get; }

    public View_Volvo_ShipmentHistoryDetailDialog()
    {
        ViewModel = App.GetService<ViewModel_Volvo_ShipmentHistoryDetailDialog>();
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
    }

    /// <summary>
    /// Loads the prepared dialog model into the dialog viewmodel.
    /// </summary>
    /// <param name="dialogModel"></param>
    public void Initialize(Model_VolvoShipmentHistoryDetailDialog dialogModel)
    {
        ViewModel.Initialize(dialogModel);
    }
}
