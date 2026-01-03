using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Contracts.Services.Navigation;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ViewModels.Settings;

namespace MTM_Receiving_Application.Views.Settings;

public sealed partial class Settings_WorkflowView : Page
{
    public Settings_WorkflowViewModel ViewModel { get; }
    private readonly IService_SettingsWorkflow _workflowService;
    private readonly IService_Navigation _navigationService;

    public Settings_WorkflowView()
    {
        ViewModel = App.GetService<Settings_WorkflowViewModel>();
        _workflowService = App.GetService<IService_SettingsWorkflow>();
        _navigationService = App.GetService<IService_Navigation>();
        InitializeComponent();
        DataContext = ViewModel;

        _workflowService.StepChanged += OnWorkflowStepChanged;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set placeholder content for category views
        var receivingVM = ReceivingPlaceholderView.ViewModel;
        receivingVM.SetCategory("Receiving Settings", "Configure receiving workflows, label formats, and validation rules.");

        var shippingVM = ShippingPlaceholderView.ViewModel;
        shippingVM.SetCategory("UPS / FedEx Settings", "Configure carrier integration, tracking, and shipping label settings.");

        var volvoVM = VolvoPlaceholderView.ViewModel;
        volvoVM.SetCategory("Volvo Settings", "Configure Volvo-specific integration and customer requirements.");

        var adminVM = AdministrativePlaceholderView.ViewModel;
        adminVM.SetCategory("Administrative Settings", "Manage system settings, database configuration, and user permissions.");
    }

    private void OnWorkflowStepChanged(object? sender, Enum_SettingsWorkflowStep step)
    {
        switch (step)
        {
            case Enum_SettingsWorkflowStep.DunnageTypes:
                _navigationService.NavigateTo(AdminFrame, typeof(DunnageModule.Views.View_Dunnage_AdminTypesView));
                break;
            case Enum_SettingsWorkflowStep.DunnageInventory:
                _navigationService.NavigateTo(AdminFrame, typeof(DunnageModule.Views.View_Dunnage_AdminInventoryView));
                break;
            case Enum_SettingsWorkflowStep.ModeSelection:
            case Enum_SettingsWorkflowStep.ReceivingSettings:
            case Enum_SettingsWorkflowStep.DunnageSettings:
            case Enum_SettingsWorkflowStep.ShippingSettings:
            case Enum_SettingsWorkflowStep.VolvoSettings:
            case Enum_SettingsWorkflowStep.AdministrativeSettings:
                _navigationService.ClearNavigation(AdminFrame);
                break;
        }
    }
}
