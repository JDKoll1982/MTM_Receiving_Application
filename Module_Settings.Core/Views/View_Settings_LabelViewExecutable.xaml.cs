using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_LabelViewExecutable : Page
{
    public ViewModel_Settings_LabelViewExecutable ViewModel { get; }

    public View_Settings_LabelViewExecutable(ViewModel_Settings_LabelViewExecutable viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
