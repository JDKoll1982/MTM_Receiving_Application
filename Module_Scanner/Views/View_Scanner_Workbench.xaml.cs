using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Placeholder scanner workbench page.
/// </summary>
public sealed partial class View_Scanner_Workbench : Page
{
    public ViewModel_Scanner_Workbench ViewModel { get; }

    public View_Scanner_Workbench(ViewModel_Scanner_Workbench viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}