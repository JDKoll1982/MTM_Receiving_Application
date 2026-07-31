using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views;

public sealed partial class View_Receiving_Dialog_LocationReconciliationReview : UserControl
{
    public ViewModel_Receiving_LocationReconciliationReview ViewModel { get; }
    private readonly IService_AdaptiveLayout _adaptiveLayout;

    public View_Receiving_Dialog_LocationReconciliationReview(
        ViewModel_Receiving_LocationReconciliationReview viewModel,
        IService_AdaptiveLayout adaptiveLayout
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(adaptiveLayout);

        ViewModel = viewModel;
        _adaptiveLayout = adaptiveLayout;
        DataContext = ViewModel;
        InitializeComponent();

        Loaded += View_Receiving_Dialog_LocationReconciliationReview_Loaded;
        Unloaded += View_Receiving_Dialog_LocationReconciliationReview_Unloaded;
        SizeChanged += View_Receiving_Dialog_LocationReconciliationReview_SizeChanged;
    }

    private void View_Receiving_Dialog_LocationReconciliationReview_Loaded(
        object sender,
        RoutedEventArgs e
    )
    {
        _ = sender;
        _ = e;
        ApplyAdaptiveLayout();
    }

    private void View_Receiving_Dialog_LocationReconciliationReview_Unloaded(
        object sender,
        RoutedEventArgs e
    )
    {
        _ = sender;
        _ = e;
        SizeChanged -= View_Receiving_Dialog_LocationReconciliationReview_SizeChanged;
    }

    private void View_Receiving_Dialog_LocationReconciliationReview_SizeChanged(
        object sender,
        SizeChangedEventArgs e
    )
    {
        _ = sender;
        _ = e;
        ApplyAdaptiveLayout();
    }

    private void ApplyAdaptiveLayout()
    {
        var state = _adaptiveLayout.ResolveReceivingLayoutState(ActualWidth);
        _ = VisualStateManager.GoToState(this, state, false);
    }
}
