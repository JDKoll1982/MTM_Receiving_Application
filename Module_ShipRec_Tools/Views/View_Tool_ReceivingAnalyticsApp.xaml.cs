using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// App Data tab of Receiving Analytics. Every filter, bucket and series lives in the view
/// model; this code-behind only exposes it to the compiled bindings.
/// </summary>
public sealed partial class View_Tool_ReceivingAnalyticsApp : UserControl
{
    public ViewModel_Tool_ReceivingAnalyticsApp ViewModel { get; }

    public View_Tool_ReceivingAnalyticsApp(ViewModel_Tool_ReceivingAnalyticsApp viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}
