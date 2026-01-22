using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_ExternalizationBacklog : Page
{
    public ViewModel_Settings_Volvo_ExternalizationBacklog ViewModel { get; }

    public View_Settings_Volvo_ExternalizationBacklog(ViewModel_Settings_Volvo_ExternalizationBacklog viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
