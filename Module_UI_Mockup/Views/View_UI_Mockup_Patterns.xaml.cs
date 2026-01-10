using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_Patterns : Page
{
    public ViewModel_UI_Mockup_Patterns ViewModel { get; }

    public View_UI_Mockup_Patterns()
    {
        ViewModel = new ViewModel_UI_Mockup_Patterns();
        InitializeComponent();
    }
}
