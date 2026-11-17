using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Record Days tab of Receiving Analytics. The range, the ranking and the three item-type lists
/// live in the view model; this code-behind only exposes it to the compiled bindings.
/// </summary>
public sealed partial class View_Tool_ReceivingAnalyticsTopDays : UserControl
{
    public ViewModel_Tool_ReceivingAnalyticsTopDays ViewModel { get; }

    public View_Tool_ReceivingAnalyticsTopDays(ViewModel_Tool_ReceivingAnalyticsTopDays viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
