using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reprint.Models;
using MTM_Receiving_Application.Module_Reprint.ViewModels;

namespace MTM_Receiving_Application.Module_Reprint.Views;

/// <summary>
/// Landing page for the Reprint Labels feature. Hosts three mode cards; selecting a mode opens
/// that module's reprint sub-page.
/// </summary>
public sealed partial class View_Reprint_Main : Page
{
    public ViewModel_Reprint_Main ViewModel { get; }

    public View_Reprint_Main(ViewModel_Reprint_Main viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.ModeSelected += OnModeSelected;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ModeSelected -= OnModeSelected;
    }

    private void OnModeSelected(object? sender, Enum_ReprintMode mode)
    {
        if (App.MainWindow is not MainWindow mainWindow)
        {
            return;
        }

        var moduleViewModel = mode switch
        {
            Enum_ReprintMode.Receiving => (ViewModel_Reprint_ModuleBase)
                App.GetService<ViewModel_Reprint_Receiving>(),
            Enum_ReprintMode.Dunnage => App.GetService<ViewModel_Reprint_Dunnage>(),
            _ => App.GetService<ViewModel_Reprint_Volvo>(),
        };

        var page = new View_Reprint_ModulePage(moduleViewModel);
        mainWindow.SetContentPage(page, $"Reprint Labels - {mode}");
    }
}
