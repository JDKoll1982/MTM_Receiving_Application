using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
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

    /// <summary>Executes the command attached to a flyout button's Tag, then closes the flyout.</summary>
    private void DateFilterActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (
            sender is FrameworkElement { Tag: System.Windows.Input.ICommand command }
            && command.CanExecute(null)
        )
        {
            command.Execute(null);
        }

        DateRangeFlyout?.Hide();
    }

    /// <summary>Applies a quick date-type preset and closes the flyout.</summary>
    private async void DateTypeActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string dateType })
        {
            await ViewModel.ApplyDateTypeAsync(dateType);
        }

        DateRangeFlyout?.Hide();
    }
}
