using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.Services.Navigation;
using MTM_Receiving_Application.Module_Settings.Enums;
using MTM_Receiving_Application.Module_Settings.Interfaces;
using MTM_Receiving_Application.Module_Settings.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Views;

public sealed partial class View_Settings_Workflow : Page
{
    public ViewModel_Settings_Workflow ViewModel { get; }
    private readonly IService_SettingsWorkflow _workflowService;
    private readonly IService_Navigation _navigationService;
    private readonly IService_Focus _focusService;

    public View_Settings_Workflow()
    {
        ViewModel = App.GetService<ViewModel_Settings_Workflow>();
        _workflowService = App.GetService<IService_SettingsWorkflow>();
        _navigationService = App.GetService<IService_Navigation>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();
        DataContext = ViewModel;

        _workflowService.StepChanged += OnWorkflowStepChanged;
        Loaded += OnLoaded;

        _focusService.AttachFocusOnVisibility(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // TODO: Initialize settings views if needed
        // The actual ViewModels have their own initialization logic
    }

    private void OnWorkflowStepChanged(object? sender, Enum_SettingsWorkflowStep step)
    {
        switch (step)
        {
            case Enum_SettingsWorkflowStep.DunnageTypes:
                _navigationService.NavigateTo(AdminFrame, typeof(Module_Dunnage.Views.View_Dunnage_AdminTypesView));
                break;
            case Enum_SettingsWorkflowStep.DunnageInventory:
                // Navigate to inventory page when implemented
                break;
        }
    }
}

