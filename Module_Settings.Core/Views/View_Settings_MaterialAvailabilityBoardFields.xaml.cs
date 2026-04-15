using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Settings page for Material Availability work-order field visibility and print selection.
/// </summary>
public sealed partial class View_Settings_MaterialAvailabilityBoardFields : Page
{
    public ViewModel_Settings_MaterialAvailabilityBoardFields ViewModel { get; }

    public View_Settings_MaterialAvailabilityBoardFields(
        ViewModel_Settings_MaterialAvailabilityBoardFields viewModel
    )
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
    }
}
