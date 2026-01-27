using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Orchestration;

/// <summary>
/// Main orchestration view for the Wizard workflow.
/// Manages step navigation (Step 1 ? Step 2 ? Step 3).
/// </summary>
public sealed partial class View_Receiving_Wizard_Orchestration_MainWorkflow : Page
{
    private int _currentStepNumber = 1;

    public View_Receiving_Wizard_Orchestration_MainWorkflow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Navigate to Step 1 on initial load
        NavigateToStep(1);
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

    private void OnNavigateNext(object sender, RoutedEventArgs e)
    {
        if (_currentStepNumber < 3)
        {
            NavigateToStep(_currentStepNumber + 1);
        }
    }

    private void OnNavigatePrevious(object sender, RoutedEventArgs e)
    {
        if (_currentStepNumber > 1)
        {
            NavigateToStep(_currentStepNumber - 1);
        }
    }

    private void OnCancelWorkflow(object sender, RoutedEventArgs e)
    {
        // Navigate back to Hub Mode Selection (Page, not UserControl)
        GetHostFrame()?.Navigate(typeof(Hub.View_Receiving_Hub_Display_ModeSelection));
    }

    private void NavigateToStep(int stepNumber)
    {
        _currentStepNumber = stepNumber;
        UpdateNavigationButtons();
        UpdateStepIndicators();

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


    private void UpdateStepIndicators()
    {
        var accentBrush = (SolidColorBrush)Application.Current.Resources["AccentFillColorDefaultBrush"];
        var inactiveBrush = (SolidColorBrush)Application.Current.Resources["ControlFillColorDefaultBrush"];
        var inactiveBorderBrush = (SolidColorBrush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
        var activeTextBrush = new SolidColorBrush(Microsoft.UI.Colors.White);
        var inactiveTextBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"];

        // Update Step 1
        if (_currentStepNumber >= 1)
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
        if (_currentStepNumber >= 2)
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
        if (_currentStepNumber >= 3)
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

    private void UpdateNavigationButtons()
    {
        PreviousButton.IsEnabled = _currentStepNumber > 1;
        NextButton.IsEnabled = _currentStepNumber < 3;
    }
}
