using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_LabelPaths : Page
{
    public ViewModel_Settings_Dunnage_LabelPaths ViewModel { get; }

    public View_Settings_Dunnage_LabelPaths(ViewModel_Settings_Dunnage_LabelPaths viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
