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

    /// <summary>
    /// The workbench's own ViewModel, so the module header can offer the search-by switch. It is the same
    /// instance the workbench page is bound to — resolving another one from the container would give a
    /// second, disconnected copy of the mode.
    /// </summary>
    public ViewModel_Scanner_Workbench WorkbenchViewModel { get; }

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

        // Assigned before InitializeComponent so the header's two-way binding has its source in place.
        WorkbenchViewModel = workbenchView.ViewModel;

        InitializeComponent();
        DataContext = ViewModel;

        WorkbenchHost.Content = workbenchView;
        HistoryHost.Content = historyView;
        SettingsHost.Content = settingsView;
    }

    private async void OnHelpClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var helpService = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Help>();
        await helpService.ShowHelpAsync("Scanner.Main");
    }
}
