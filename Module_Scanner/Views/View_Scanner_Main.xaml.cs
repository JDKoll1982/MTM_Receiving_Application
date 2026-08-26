using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Main container page for the scanner module.
/// Hosts the Workbench, History, and Settings views using DI-injected subviews.
/// </summary>
public sealed partial class View_Scanner_Main : Page
{
    public ViewModel_Scanner_Main ViewModel { get; }

    public View_Scanner_Main(
        ViewModel_Scanner_Main viewModel,
        View_Scanner_Workbench workbenchView,
        View_Scanner_History historyView,
        View_Scanner_Settings settingsView
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(workbenchView);
        ArgumentNullException.ThrowIfNull(historyView);
        ArgumentNullException.ThrowIfNull(settingsView);

        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;

        WorkbenchHost.Content = workbenchView;
        HistoryHost.Content = historyView;
        SettingsHost.Content = settingsView;
    }
}
