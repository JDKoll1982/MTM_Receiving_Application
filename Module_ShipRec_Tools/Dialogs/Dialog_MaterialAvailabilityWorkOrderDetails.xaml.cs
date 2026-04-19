using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Dialog showing categorized work-order details for one next-run association.
/// </summary>
public sealed partial class Dialog_MaterialAvailabilityWorkOrderDetails : ContentDialog
{
    public ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails ViewModel { get; }

    public Dialog_MaterialAvailabilityWorkOrderDetails(
        ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails viewModel
    )
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        Title = ViewModel.Heading;
    }
}
