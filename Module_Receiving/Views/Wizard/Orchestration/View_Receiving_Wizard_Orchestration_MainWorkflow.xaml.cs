using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Orchestration;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Orchestration;

/// <summary>
/// Main orchestration view for the Wizard workflow.
/// Manages step navigation (Step 1 ? Step 2 ? Step 3).
/// ENHANCED: Integrated with ViewModel for validation and state management.
/// </summary>
public sealed partial class View_Receiving_Wizard_Orchestration_MainWorkflow : Page
{
    public ViewModel_Receiving_Wizard_Orchestration_MainWorkflow? ViewModel => DataContext as ViewModel_Receiving_Wizard_Orchestration_MainWorkflow;

    public View_Receiving_Wizard_Orchestration_MainWorkflow()
    {
        InitializeComponent();
        
        // Set ViewModel as DataContext (DI injection)
        DataContext = App.GetService<ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();
        
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Navigate to Step 1 on initial load
        NavigateToStep(1);
        
        // Subscribe to ViewModel CurrentStep changes to update UI
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(ViewModel.CurrentStep))
                {
                    var stepNumber = ViewModel.CurrentStep switch
                    {
                        Module_Receiving.Models.Enums.Enum_Receiving_State_WorkflowStep.OrderAndPartSelection => 1,
                        Module_Receiving.Models.Enums.Enum_Receiving_State_WorkflowStep.LoadDetailsEntry => 2,
                        Module_Receiving.Models.Enums.Enum_Receiving_State_WorkflowStep.ReviewAndSave => 3,
                        _ => 1
                    };
                    NavigateToStep(stepNumber);
                }
            };
        }
    }

    private Frame? GetHostFrame()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is Frame frame)
            {
                return frame;
            }
            parent = (parent as FrameworkElement)?.Parent;
        }
        return null;
    }

    private async void OnNavigateNext(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: ViewModel is null in OnNavigateNext!");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"OnNavigateNext: Current Step = {ViewModel.CurrentStep}");

        // Collect data from current step FIRST
        await CollectCurrentStepDataAsync();

        System.Diagnostics.Debug.WriteLine($"After Collect: PO={ViewModel.PoNumber}, Part={ViewModel.PartNumber}, LoadCount={ViewModel.LoadCount}");

        // CRITICAL: Validate and update Step1Valid flag BEFORE checking CanExecute
        await ViewModel.ValidateCurrentStepCommand.ExecuteAsync(null);
        
        System.Diagnostics.Debug.WriteLine($"After Validate: Step1Valid={ViewModel.Step1Valid}, Step2Valid={ViewModel.Step2Valid}");

        // Now the CanExecute should return true if validation passed
        if (ViewModel.GoToNextStepCommand.CanExecute(null))
        {
            System.Diagnostics.Debug.WriteLine("GoToNextStepCommand.CanExecute = true, executing...");
            await ViewModel.GoToNextStepCommand.ExecuteAsync(null);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"GoToNextStepCommand.CanExecute = FALSE! Step1Valid={ViewModel.Step1Valid}");
            
            // Get error handler from App (ViewModel base class doesn't expose it publicly)
            var errorHandler = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_ErrorHandler>();
            
            // Show validation error to user
            await errorHandler.ShowUserErrorAsync(
                "Please complete all required fields before proceeding.\n\n" +
                $"PO Number: {(string.IsNullOrWhiteSpace(ViewModel.PoNumber) ? "? Missing" : "? " + ViewModel.PoNumber)}\n" +
                $"Part Number: {(string.IsNullOrWhiteSpace(ViewModel.PartNumber) ? "? Missing" : "? " + ViewModel.PartNumber)}\n" +
                $"Load Count: {(ViewModel.LoadCount > 0 && ViewModel.LoadCount <= 99 ? "? " + ViewModel.LoadCount : "? Invalid")}",
                "Validation Error",
                nameof(OnNavigateNext));
        }
    }

    private async void OnNavigatePrevious(object sender, RoutedEventArgs e)
    {
        if (ViewModel == null) return;

        // Use ViewModel navigation command
        if (ViewModel.GoToPreviousStepCommand.CanExecute(null))
        {
            await ViewModel.GoToPreviousStepCommand.ExecuteAsync(null);
        }
    }

    private void OnCancelWorkflow(object sender, RoutedEventArgs e)
    {
        // Navigate back to Hub Mode Selection (Page, not UserControl)
        GetHostFrame()?.Navigate(typeof(Hub.View_Receiving_Hub_Display_ModeSelection));
    }

    /// <summary>
    /// Collects data from child ViewModels in Step 1 and updates the Orchestration ViewModel.
    /// </summary>
    private async Task CollectCurrentStepDataAsync()
    {
        if (ViewModel == null)
        {
            System.Diagnostics.Debug.WriteLine("ERROR: ViewModel is null in CollectCurrentStepDataAsync!");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"CollectCurrentStepDataAsync: Frame content type = {StepContentFrame.Content?.GetType().Name}");

        // Get the current content from StepContentFrame
        if (StepContentFrame.Content is Step1.View_Receiving_Wizard_Display_Step1Container step1Container)
        {
            System.Diagnostics.Debug.WriteLine("Step1Container found!");
            
            // Access child ViewModels from Step1Container's named controls
            var poViewModel = step1Container.PONumberEntry.DataContext as ViewModel_Receiving_Wizard_Display_PONumberEntry;
            var partViewModel = step1Container.PartSelection.DataContext as ViewModel_Receiving_Wizard_Display_PartSelection;
            var loadCountViewModel = step1Container.LoadCountEntry.DataContext as ViewModel_Receiving_Wizard_Display_LoadCountEntry;

            System.Diagnostics.Debug.WriteLine($"Child ViewModels - PO: {poViewModel != null}, Part: {partViewModel != null}, LoadCount: {loadCountViewModel != null}");

            if (poViewModel != null && partViewModel != null && loadCountViewModel != null)
            {
                // Update orchestration ViewModel with child ViewModel data
                ViewModel.PoNumber = poViewModel.PoNumber;
                ViewModel.PartNumber = partViewModel.SelectedPartFromPo?.PartNumber ?? string.Empty;
                ViewModel.LoadCount = loadCountViewModel.LoadCount;
                
                System.Diagnostics.Debug.WriteLine($"Collected Data: PO={ViewModel.PoNumber}, Part={ViewModel.PartNumber}, LoadCount={ViewModel.LoadCount}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: One or more child ViewModels is null!");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Current step is NOT Step1Container, it's: {StepContentFrame.Content?.GetType().Name}");
        }
        
        await Task.CompletedTask;
    }

    private void NavigateToStep(int stepNumber)
    {
        UpdateStepIndicators(stepNumber);
        UpdateNavigationButtons(stepNumber);

        switch (stepNumber)
        {
            case 1:
                StepContentFrame.Navigate(typeof(Step1.View_Receiving_Wizard_Display_Step1Container));
                break;
            case 2:
                StepContentFrame.Navigate(typeof(Step2.View_Receiving_Wizard_Display_Step2Container));
                break;
            case 3:
                StepContentFrame.Navigate(typeof(Step3.View_Receiving_Wizard_Display_Step3Container));
                break;
        }
    }

    private void UpdateStepIndicators(int currentStep)
    {
        var accentBrush = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        var inactiveBrush = (SolidColorBrush)Application.Current.Resources["ControlFillColorDefaultBrush"];
        var inactiveBorderBrush = (SolidColorBrush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
        var activeTextBrush = new SolidColorBrush(Microsoft.UI.Colors.White);
        var inactiveTextBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"];

        // Update Step 1
        if (currentStep >= 1)
        {
            Step1Circle.Background = accentBrush;
            Step1Circle.BorderBrush = accentBrush;
            Step1Circle.BorderThickness = new Thickness(0);
            ((TextBlock)Step1Circle.Child).Foreground = activeTextBrush;
        }
        else
        {
            Step1Circle.Background = inactiveBrush;
            Step1Circle.BorderBrush = inactiveBorderBrush;
            Step1Circle.BorderThickness = new Thickness(2);
            ((TextBlock)Step1Circle.Child).Foreground = inactiveTextBrush;
        }

        // Update Step 2
        if (currentStep >= 2)
        {
            Step2Circle.Background = accentBrush;
            Step2Circle.BorderBrush = accentBrush;
            Step2Circle.BorderThickness = new Thickness(0);
            ((TextBlock)Step2Circle.Child).Foreground = activeTextBrush;
        }
        else
        {
            Step2Circle.Background = inactiveBrush;
            Step2Circle.BorderBrush = inactiveBorderBrush;
            Step2Circle.BorderThickness = new Thickness(2);
            ((TextBlock)Step2Circle.Child).Foreground = inactiveTextBrush;
        }

        // Update Step 3
        if (currentStep >= 3)
        {
            Step3Circle.Background = accentBrush;
            Step3Circle.BorderBrush = accentBrush;
            Step3Circle.BorderThickness = new Thickness(0);
            ((TextBlock)Step3Circle.Child).Foreground = activeTextBrush;
        }
        else
        {
            Step3Circle.Background = inactiveBrush;
            Step3Circle.BorderBrush = inactiveBorderBrush;
            Step3Circle.BorderThickness = new Thickness(2);
            ((TextBlock)Step3Circle.Child).Foreground = inactiveTextBrush;
        }
    }

    private void UpdateNavigationButtons(int currentStep)
    {
        PreviousButton.IsEnabled = currentStep > 1;
        NextButton.IsEnabled = currentStep < 3;
    }
}
