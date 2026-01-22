using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_Theme : Page
{
    public ViewModel_Settings_Theme ViewModel { get; }

    public View_Settings_Theme(ViewModel_Settings_Theme viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
