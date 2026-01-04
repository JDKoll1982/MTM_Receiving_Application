using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Enums;
using MTM_Receiving_Application.Module_Settings.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.ViewModels;

public partial class ViewModel_Settings_Placeholder : ViewModel_Shared_Base
{
    private readonly IService_SettingsWorkflow _workflowService;

    [ObservableProperty]
    private string _categoryTitle = "Settings";

    [ObservableProperty]
    private string _placeholderMessage = "This settings category is under development.";

    public ViewModel_Settings_Placeholder(
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

