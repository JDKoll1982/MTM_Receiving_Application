using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Shell page for the Outside Service module.
/// </summary>
public sealed partial class View_OutsideService_Main : Page
{
    public ViewModel_OutsideService_Main ViewModel { get; }

    public View_OutsideService_Main(
        ViewModel_OutsideService_Main viewModel,
        View_OutsideService_RequestEntry requestEntryView,
        View_OutsideService_Waitlist waitlistView,
        View_OutsideService_Setup setupView,
        View_OutsideService_CompleteHistory historyView
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(requestEntryView);
        ArgumentNullException.ThrowIfNull(waitlistView);
        ArgumentNullException.ThrowIfNull(setupView);
        ArgumentNullException.ThrowIfNull(historyView);

        ViewModel = viewModel;
        InitializeComponent();

        RequestEntryHost.Content = requestEntryView;
        WaitlistHost.Content = waitlistView;
        SetupHost.Content = setupView;
        HistoryHost.Content = historyView;

        ViewModel.UpdateSelectedWaitlistLine(waitlistView.ViewModel.SelectedLine);

        waitlistView.ViewModel.SelectedLineChanged += line =>
        {
            ViewModel.UpdateSelectedWaitlistLine(line);
        };

        requestEntryView.ViewModel.RequestSaved += async _ =>
        {
            await waitlistView.ViewModel.RefreshCommand.ExecuteAsync(null);
            await historyView.ViewModel.RefreshCommand.ExecuteAsync(null);
            ViewModel.UpdateSelectedWaitlistLine(waitlistView.ViewModel.SelectedLine);
            ViewModel.ShowWaitlistCommand.Execute(null);
        };

        waitlistView.ViewModel.SetupRequested += async line =>
        {
            ViewModel.UpdateSelectedWaitlistLine(line);
            await setupView.ViewModel.LoadLineAsync(line);
            ViewModel.ShowSetupCommand.Execute(null);
        };

        setupView.ViewModel.ReturnRequested += () =>
        {
            if (setupView.ViewModel.CurrentLine?.IsComplete == true)
            {
                ViewModel.ShowHistoryCommand.Execute(null);
            }
            else
            {
                ViewModel.ShowWaitlistCommand.Execute(null);
            }
        };

        setupView.ViewModel.LineSaved += async () =>
        {
            await waitlistView.ViewModel.RefreshCommand.ExecuteAsync(null);
            await historyView.ViewModel.RefreshCommand.ExecuteAsync(null);
            ViewModel.UpdateSelectedWaitlistLine(waitlistView.ViewModel.SelectedLine);
        };

        ViewModel.PropertyChanged += async (_, args) =>
        {
            if (
                args.PropertyName == nameof(ViewModel_OutsideService_Main.CurrentSection)
                && ViewModel.IsHistoryVisible
            )
            {
                historyView.ViewModel.ResetFiltersToDefaults();
                await historyView.ViewModel.RefreshCommand.ExecuteAsync(null);
            }
        };

        Loaded += async (_, _) =>
        {
            await waitlistView.ViewModel.LoadCommand.ExecuteAsync(null);
            await historyView.ViewModel.LoadCommand.ExecuteAsync(null);
            ViewModel.UpdateSelectedWaitlistLine(waitlistView.ViewModel.SelectedLine);
        };
    }
}
