using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_VisualAutomation.Dialogs;
using MTM_Receiving_Application.Module_VisualAutomation.ViewModels;

namespace MTM_Receiving_Application.Module_VisualAutomation.Views;

/// <summary>
/// Test harness for Infor VISUAL Purchase Receipt Entry automation (VMRCVENT.EXE). The cards on this page
/// stay short - they show only Pass or Fail - and the reasons plus the values sent to VISUAL live in two
/// modal dialogs opened from the icon buttons beside Reset.
/// </summary>
public sealed partial class View_VisualAutomation_MacroTester : Page
{
    public View_VisualAutomation_MacroTester()
    {
        ViewModel = App.GetService<ViewModel_VisualAutomation_MacroTester>();
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ProbeInputsRequested += OnProbeInputsRequested;
    }

    public ViewModel_VisualAutomation_MacroTester ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.Initialize();

    private async void OnDetailsClick(object sender, RoutedEventArgs e)
    {
        if (XamlRoot is null)
        {
            return;
        }

        var dialog = new Dialog_VisualProbeDetails(
            ViewModel.BuildDetails(),
            ViewModel.LogEntries.Count
        )
        {
            XamlRoot = XamlRoot,
        };

        await dialog.ShowAsync();
    }

    private async void OnInputsClick(object sender, RoutedEventArgs e) =>
        await ShowInputsDialogAsync();

    private async void OnProbeInputsRequested(object? sender, EventArgs e) =>
        await ShowInputsDialogAsync();

    private async Task ShowInputsDialogAsync()
    {
        if (XamlRoot is null)
        {
            return;
        }

        var dialog = new Dialog_VisualProbeInputs(ViewModel.Inputs) { XamlRoot = XamlRoot };

        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            ViewModel.Inputs = dialog.Inputs;
            ViewModel.RefreshInputsSummary();
        }
    }
}
