using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_System : Page
{
    public ViewModel_Settings_System ViewModel { get; }

    public View_Settings_System(ViewModel_Settings_System viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
