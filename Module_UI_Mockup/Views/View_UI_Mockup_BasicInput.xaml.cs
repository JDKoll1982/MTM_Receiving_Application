using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

/// <summary>
/// Tab 1 - Basic Input Controls demonstration page.
/// Showcases buttons, toggles, checkboxes, sliders, and switches.
/// </summary>
public sealed partial class View_UI_Mockup_BasicInput : Page
{
    public ViewModel_UI_Mockup_BasicInput ViewModel { get; }

    public View_UI_Mockup_BasicInput()
    {
        ViewModel = new ViewModel_UI_Mockup_BasicInput();
        InitializeComponent();
    }
}
