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
/// ViewModel for Dunnage Label entry page
/// </summary>
public partial class DunnageLabelViewModel : BaseViewModel
{
    public DunnageLabelViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        DunnageLines = new ObservableCollection<Model_DunnageLine>();
        _currentLine = new Model_DunnageLine();
    }

    public ObservableCollection<Model_DunnageLine> DunnageLines { get; }

    [ObservableProperty]
    private Model_DunnageLine _currentLine;

    [RelayCommand]
    private async Task AddLineAsync()
    {
        // TODO: Implement when ready
        await Task.CompletedTask;
    }
}
