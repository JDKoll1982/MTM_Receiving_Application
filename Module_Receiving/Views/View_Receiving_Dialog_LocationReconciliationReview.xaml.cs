using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views;

public sealed partial class View_Receiving_Dialog_LocationReconciliationReview : UserControl
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
}
