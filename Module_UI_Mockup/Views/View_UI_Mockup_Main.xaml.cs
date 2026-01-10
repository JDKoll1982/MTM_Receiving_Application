using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

/// <summary>
/// Welcome/Main landing page for the UI Mockup Module.
/// Displays gallery overview and quick navigation cards to all control categories.
/// </summary>
public sealed partial class View_UI_Mockup_Main : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// </summary>
    public ViewModel_UI_Mockup_Main ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the View_UI_Mockup_Main class.
    /// </summary>
    public View_UI_Mockup_Main()
    {
        ViewModel = new ViewModel_UI_Mockup_Main();
        InitializeComponent();
    }

    private void QuickNavCard_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Get tab index from the clicked card
        if (sender is Microsoft.UI.Xaml.Controls.Button button && button.DataContext is QuickNavItem navItem)
        {
            var tabIndex = navItem.Title switch
            {
                "Basic Input" => 1,
                "Text Controls" => 2,
                "Collections" => 3,
                "Navigation" => 4,
                "Dialogs & Flyouts" => 5,
                "Date & Time" => 6,
                "Media & Visual" => 7,
                "Layout & Panels" => 8,
                "Status & Feedback" => 9,
                "Advanced" => 10,
                "App Patterns" => 11,
                "Custom Controls" => 12,
                _ => 0
            };

            // Store in ViewModel for window to pick up
            ViewModel.RequestedTabIndex = tabIndex;
        }
    }
}
