using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.ViewModels.Settings;

public partial class Settings_PlaceholderViewModel : Shared_BaseViewModel
{
    private readonly IService_SettingsWorkflow _workflowService;

    [ObservableProperty]
    private string _categoryTitle = "Settings";

    [ObservableProperty]
    private string _placeholderMessage = "This settings category is under development.";

    public Settings_PlaceholderViewModel(
        IService_SettingsWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _workflowService = workflowService;
    }

    public void SetCategory(string title, string message)
    {
        CategoryTitle = title;
        PlaceholderMessage = message;
    }

    [RelayCommand]
    private void Back()
    {
        _workflowService.GoToStep(Enum_SettingsWorkflowStep.ModeSelection);
    }
}
