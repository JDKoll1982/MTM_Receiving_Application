using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using System.Diagnostics;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_Defaults : Page
{
    public ViewModel_Settings_Receiving_Defaults ViewModel { get; }

    public View_Settings_Receiving_Defaults(ViewModel_Settings_Receiving_Defaults viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
        
        // Set the settings window for the folder picker
        Loaded += (s, e) =>
        {
            SetSettingsWindow();
            // Log the current values
            Debug.WriteLine($"[Receiving Defaults] DefaultReceivingMode: '{viewModel.DefaultReceivingMode}'");
            Debug.WriteLine($"[Receiving Defaults] CsvSaveLocation (legacy/repurpose): '{viewModel.CsvSaveLocation}'");
        };
    }

    private void SetSettingsWindow()
    {
        // Get the settings window (stored as static instance in View_Settings_CoreWindow)
        var settingsWindow = View_Settings_CoreWindow.GetInstance();
        if (settingsWindow != null)
        {
            ViewModel.SetSettingsWindow(settingsWindow);
        }
    }
}
