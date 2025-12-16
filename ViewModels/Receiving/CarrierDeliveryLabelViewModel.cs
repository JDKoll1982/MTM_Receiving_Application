using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Receiving;

/// <summary>
/// ViewModel for Routing Label entry page
/// </summary>
public partial class RoutingLabelViewModel : BaseViewModel
{
    public RoutingLabelViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        RoutingLabels = new ObservableCollection<Model_RoutingLabel>();
        _currentLabel = new Model_RoutingLabel();
    }

    public ObservableCollection<Model_RoutingLabel> RoutingLabels { get; }

    [ObservableProperty]
    private Model_RoutingLabel _currentLabel;

    [RelayCommand]
    private async Task AddLabelAsync()
    {
        // TODO: Implement when ready
        await Task.CompletedTask;
    }
}
