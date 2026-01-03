using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Main;

/// <summary>
/// ViewModel for Carrier Delivery Label entry page (UPS/FedEx/USPS shipping info)
/// </summary>
public partial class Main_CarrierDeliveryLabelViewModel : Shared_BaseViewModel
{
    public Main_CarrierDeliveryLabelViewModel(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        CarrierDeliveryLabels = new ObservableCollection<Model_CarrierDeliveryLabel>();
        _currentLabel = new Model_CarrierDeliveryLabel();
    }

    public ObservableCollection<Model_CarrierDeliveryLabel> CarrierDeliveryLabels { get; }

    [ObservableProperty]
    private Model_CarrierDeliveryLabel _currentLabel;

    [RelayCommand]
    private async Task AddLabelAsync()
    {
        // TODO: Implement when ready
        await Task.CompletedTask;
    }
}
