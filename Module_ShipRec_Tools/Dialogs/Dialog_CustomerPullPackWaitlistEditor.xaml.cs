using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Requester-side confirmation dialog for Customer Pull n' Pack waitlist creation and update flows.
/// </summary>
public sealed partial class Dialog_CustomerPullPackWaitlistEditor : ContentDialog
{
    public ViewModel_Dialog_CustomerPullPackWaitlistEditor ViewModel { get; }

    public Dialog_CustomerPullPackWaitlistEditor(
        ViewModel_Dialog_CustomerPullPackWaitlistEditor viewModel
    )
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        Title = ViewModel.DialogTitle;
        PrimaryButtonText = ViewModel.PrimaryButtonText;
        CloseButtonText = "Cancel";
        DefaultButton = ContentDialogButton.Primary;
    }
}
