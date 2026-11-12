using System.Collections.Generic;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.Views;

/// <summary>
/// The Needs Attention surface. It is a modal window rather than a dialog, so the
/// operator keeps the working page behind it and is never bounced out of the module
/// (FR-047, R-MVVM-07).
/// </summary>
public sealed partial class View_ReconciliationNeedsAttentionWindow : Window
{
    /// <summary>The window's view model.</summary>
    public ViewModel_ReconciliationNeedsAttention ViewModel { get; }

    /// <summary>Creates the modal window over its resolved view model.</summary>
    public View_ReconciliationNeedsAttentionWindow()
    {
        ViewModel = App.GetService<ViewModel_ReconciliationNeedsAttention>();

        InitializeComponent();

        ConfigureAsModal();
    }

    /// <summary>Fills the window with the working day's unresolved lines.</summary>
    /// <param name="findings">Every finding the day produced.</param>
    public void Initialize(IReadOnlyList<Model_ReconciliationFinding> findings) =>
        ViewModel.Load(findings);

    private void ConfigureAsModal()
    {
        if (AppWindow.Presenter is not OverlappedPresenter presenter)
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            presenter = (OverlappedPresenter)AppWindow.Presenter;
        }

        presenter.IsModal = true;
        presenter.IsResizable = true;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        Close();
    }
}
