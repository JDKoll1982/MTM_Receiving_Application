using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Read-only shipment history detail content hosted inside a dedicated modal window.
/// </summary>
public sealed partial class View_Volvo_ShipmentHistoryDetailDialog : Page
{
    public ViewModel_Volvo_ShipmentHistoryDetailDialog ViewModel { get; }

    public event EventHandler? CloseRequested;

    public View_Volvo_ShipmentHistoryDetailDialog()
    {
        ViewModel = App.GetService<ViewModel_Volvo_ShipmentHistoryDetailDialog>();
        InitializeComponent();
    }

    private async void OnHelpClick(object sender, RoutedEventArgs e)
    {
        var helpService = App.GetService<IService_Help>();
        await helpService.ShowHelpAsync("Volvo.ShipmentHistoryDetail", XamlRoot);
    }

    /// <summary>
    /// Loads the prepared window model into the page ViewModel.
    /// </summary>
    /// <param name="dialogModel"></param>
    public void Initialize(Model_VolvoShipmentHistoryDetailDialog dialogModel)
    {
        ViewModel.Initialize(dialogModel);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
