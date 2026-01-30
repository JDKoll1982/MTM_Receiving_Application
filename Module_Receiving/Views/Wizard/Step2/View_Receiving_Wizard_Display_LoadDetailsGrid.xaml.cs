using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step2;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step2;

/// <summary>
/// UserControl for load details data grid entry.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_LoadDetailsGrid : UserControl
{
    public ViewModel_Receiving_Wizard_Display_LoadDetailsGrid ViewModel { get; }

    /// <summary>
    /// Initializes the Load Details Grid view with DI.
    /// </summary>
    public View_Receiving_Wizard_Display_LoadDetailsGrid(ViewModel_Receiving_Wizard_Display_LoadDetailsGrid viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Receiving_Wizard_Display_LoadDetailsGrid()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_LoadDetailsGrid>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    /// <summary>
    /// Handles Tab key to navigate to next cell in grid instead of next control outside grid.
    /// </summary>
    private void OnTabKeyPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = TryNavigateToNextCell(forward: true);
    }

    /// <summary>
    /// Handles Shift+Tab key to navigate to previous cell in grid.
    /// </summary>
    private void OnShiftTabKeyPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = TryNavigateToNextCell(forward: false);
    }

    /// <summary>
    /// Attempts to move focus to the next/previous editable cell in the grid.
    /// Returns true if focus moved within grid, false to allow default Tab behavior.
    /// </summary>
    private bool TryNavigateToNextCell(bool forward)
    {
        // Get currently focused element
        var focusedElement = FocusManager.GetFocusedElement(this.XamlRoot) as FrameworkElement;
        if (focusedElement == null) return false;

        // Find the next focusable element in tab order
        var nextElement = forward
            ? FocusManager.FindNextElement(FocusNavigationDirection.Next, new FindNextElementOptions { SearchRoot = LoadsRepeater })
            : FocusManager.FindNextElement(FocusNavigationDirection.Previous, new FindNextElementOptions { SearchRoot = LoadsRepeater });

        if (nextElement is Control nextControl)
        {
            // Check if next element is still within the grid
            if (IsDescendantOf(nextControl, LoadsRepeater))
            {
                nextControl.Focus(FocusState.Keyboard);
                return true; // Handled - stay within grid
            }
        }

        // Allow default Tab behavior (exit grid)
        return false;
    }

    /// <summary>
    /// Checks if element is a descendant of parent.
    /// </summary>
    private bool IsDescendantOf(DependencyObject element, DependencyObject parent)
    {
        var current = element;
        while (current != null)
        {
            if (current == parent) return true;
            current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}

