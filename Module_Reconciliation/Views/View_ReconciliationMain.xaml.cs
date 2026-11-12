using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.Views;

/// <summary>
/// The working page. The code-behind carries view-only work: it loads the opening
/// day and forwards the date picker's change. No business rule lives here
/// (R-MVVM-02).
/// </summary>
public sealed partial class View_ReconciliationMain : Page
{
    /// <summary>The page's view model.</summary>
    public ViewModel_ReconciliationMain ViewModel { get; }

    /// <summary>Creates the page over its resolved view model.</summary>
    public View_ReconciliationMain()
    {
        ViewModel = App.GetService<ViewModel_ReconciliationMain>();

        InitializeComponent();

        DataContext = ViewModel;
        ViewModel.NeedsAttentionRequested += OnNeedsAttentionRequested;
    }

    private void OnNeedsAttentionRequested(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        var window = App.GetService<View_ReconciliationNeedsAttentionWindow>();
        window.Initialize(ViewModel.UnresolvedFindings);
        window.Activate();
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        await ViewModel.LoadSelectedDateAsync();
    }

    private async void OnWorkingDayChanged(
        object sender,
        DatePickerValueChangedEventArgs e
    )
    {
        _ = sender;

        if (e.NewDate == default)
        {
            return;
        }

        // Setting Date from the view model's own SelectedDate raises this event too.
        // Without this guard that push re-enters the load and walks the day backwards.
        DateOnly selected = DateOnly.FromDateTime(e.NewDate.Date);

        if (selected == ViewModel.SelectedDate)
        {
            return;
        }

        await ViewModel.SelectDateAsync(selected);
    }

    private void OnSweepStartChanged(object sender, DatePickerValueChangedEventArgs e)
    {
        _ = sender;

        if (e.NewDate == default)
        {
            return;
        }

        DateOnly start = DateOnly.FromDateTime(e.NewDate.Date);

        if (start != ViewModel.SweepStartDate)
        {
            ViewModel.SweepStartDate = start;
        }
    }

    private void OnSweepEndChanged(object sender, DatePickerValueChangedEventArgs e)
    {
        _ = sender;

        if (e.NewDate == default)
        {
            return;
        }

        DateOnly end = DateOnly.FromDateTime(e.NewDate.Date);

        if (end != ViewModel.SweepEndDate)
        {
            ViewModel.SweepEndDate = end;
        }
    }

    private void OnPageKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        _ = sender;

        switch (e.Key)
        {
            case Windows.System.VirtualKey.Enter when ViewModel.AcceptCommand.CanExecute(null):
                ViewModel.AcceptCommand.Execute(null);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Escape when ViewModel.SkipCommand.CanExecute(null):
                ViewModel.SkipCommand.Execute(null);
                e.Handled = true;
                break;
        }
    }
}
