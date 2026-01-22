using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_UiConfiguration : Page
{
    public ViewModel_Settings_Volvo_UiConfiguration ViewModel { get; }

    public View_Settings_Volvo_UiConfiguration(ViewModel_Settings_Volvo_UiConfiguration viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
