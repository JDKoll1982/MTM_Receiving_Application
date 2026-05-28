using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Reporting.Views;

public sealed partial class View_Reporting_PreviewPage : Page
{
    private readonly IService_HeaderBackNavigation _headerBackNavigation;

    public ViewModel_Reporting_Main ViewModel { get; }

    public View_Reporting_PreviewPage(
        ViewModel_Reporting_Main viewModel,
        IService_HeaderBackNavigation headerBackNavigation
    )
    {
        ViewModel = viewModel;
        _headerBackNavigation = headerBackNavigation;
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _headerBackNavigation.RegisterBackAction(
            NavigateBackToReportingAsync,
            "Back to Report Setup"
        );
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _headerBackNavigation.ClearBackAction();
    }

    private Task NavigateBackToReportingAsync()
    {
        if (App.MainWindow is not MainWindow mainWindow)
        {
            return Task.CompletedTask;
        }

        var reportingMainPage = App.GetService<View_Reporting_Main>();
        mainWindow.SetContentPage(reportingMainPage, "End of Day Reports");
        return Task.CompletedTask;
    }
}
