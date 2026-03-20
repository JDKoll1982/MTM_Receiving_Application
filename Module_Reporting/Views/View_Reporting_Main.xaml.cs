using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reporting.ViewModels;

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
    }

    private async void OnPreviewRequested(object? sender, System.EventArgs e)
    {
        var dialog = new View_Reporting_PreviewDialog(ViewModel) { XamlRoot = XamlRoot };

        await dialog.ShowAsync();
    }
}
