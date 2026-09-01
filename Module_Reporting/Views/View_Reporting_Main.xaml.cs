using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reporting.ViewModels;
using MTM_Receiving_Application.Module_Reporting.Views;

namespace MTM_Receiving_Application.Module_Reporting.Views;

public sealed partial class View_Reporting_Main : Page
{
    public ViewModel_Reporting_Main ViewModel { get; }

    public View_Reporting_Main()
    {
        ViewModel = App.GetService<ViewModel_Reporting_Main>();
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.PreviewRequested += OnPreviewRequested;
        Unloaded += OnUnloaded;
    }

    private async void OnPreviewRequested(object? sender, System.EventArgs e)
    {
        _ = sender;
        _ = e;

        if (App.MainWindow is not MainWindow mainWindow)
        {
            return;
        }

        var previewPage = App.GetService<View_Reporting_PreviewPage>();
        mainWindow.SetContentPage(previewPage, string.Empty);
        await System.Threading.Tasks.Task.CompletedTask;
    }

    private void OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        ViewModel.PreviewRequested -= OnPreviewRequested;
        Unloaded -= OnUnloaded;
    }

    private async void OnHelpClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var helpService = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Help>();
        await helpService.ShowHelpAsync("Reporting.Main");
    }
}
