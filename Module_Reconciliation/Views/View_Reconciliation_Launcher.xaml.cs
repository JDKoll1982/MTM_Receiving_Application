using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.Views;

/// <summary>
/// The module's launcher page. It carries only view work: it opens the working page
/// when the mode card is chosen (R-MVVM-02).
/// </summary>
public sealed partial class View_Reconciliation_Launcher : Page
{
    /// <summary>The page's view model.</summary>
    public ViewModel_Reconciliation_Launcher ViewModel { get; }

    /// <summary>Creates the page over its resolved view model.</summary>
    public View_Reconciliation_Launcher()
    {
        ViewModel = App.GetService<ViewModel_Reconciliation_Launcher>();

        InitializeComponent();

        DataContext = ViewModel;
        ViewModel.WorkAReceivingDayRequested += OnWorkAReceivingDayRequested;
    }

    private void OnWorkAReceivingDayRequested(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        if (App.MainWindow is MainWindow mainWindow)
        {
            mainWindow.SetContentPage(
                App.GetService<View_ReconciliationMain>(),
                string.Empty
            );
        }
    }
}
