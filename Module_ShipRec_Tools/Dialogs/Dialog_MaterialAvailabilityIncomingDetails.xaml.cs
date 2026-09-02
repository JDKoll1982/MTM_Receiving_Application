using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Dialog showing incoming-material detail lines for one material availability card.
/// </summary>
public sealed partial class Dialog_MaterialAvailabilityIncomingDetails : ContentDialog
{
    public ViewModel_Dialog_MaterialAvailabilityIncomingDetails ViewModel { get; }

    public Dialog_MaterialAvailabilityIncomingDetails(
        ViewModel_Dialog_MaterialAvailabilityIncomingDetails viewModel
    )
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        Title = ViewModel.Heading;
    }

    }
