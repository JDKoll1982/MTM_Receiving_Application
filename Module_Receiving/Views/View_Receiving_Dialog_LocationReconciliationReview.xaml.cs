using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Receiving.Views;

public sealed partial class View_Receiving_Dialog_LocationReconciliationReview : ContentDialog
{
    public ViewModel_Receiving_LocationReconciliationReview ViewModel { get; }

    public View_Receiving_Dialog_LocationReconciliationReview(
        ViewModel_Receiving_LocationReconciliationReview viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }

    public void Initialize(Model_ReceivingLocationReconciliationSummary previewSummary)
    {
        ViewModel.Initialize(previewSummary);
    }

    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var desiredWidth = Math.Ceiling(Math.Max(RootGrid.DesiredSize.Width, 1180) + 32);
        var availableWidth = Math.Max(1120, XamlRoot.Size.Width - 32);
        var availableHeight = Math.Max(680, XamlRoot.Size.Height - 48);

        Width = Math.Min(desiredWidth, availableWidth);
        MinWidth = Math.Min(1180, availableWidth);
        MinHeight = Math.Min(680, availableHeight);
        MaxHeight = availableHeight;
    }
}
