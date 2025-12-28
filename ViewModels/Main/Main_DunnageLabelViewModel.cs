using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Main;

/// <summary>
/// ViewModel for Dunnage Label entry page
/// </summary>
public partial class Main_DunnageLabelViewModel : Shared_BaseViewModel
{
    public Main_DunnageLabelViewModel(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
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
