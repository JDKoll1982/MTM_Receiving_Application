using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_LabelPaths : Page
{
    public ViewModel_Settings_Volvo_LabelPaths ViewModel { get; }

    public View_Settings_Volvo_LabelPaths(ViewModel_Settings_Volvo_LabelPaths viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
