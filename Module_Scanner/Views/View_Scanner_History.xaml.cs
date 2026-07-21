using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Placeholder scanner history page.
/// </summary>
public sealed partial class View_Scanner_History : Page
{
    public ViewModel_Scanner_History ViewModel { get; }

    public View_Scanner_History(ViewModel_Scanner_History viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}