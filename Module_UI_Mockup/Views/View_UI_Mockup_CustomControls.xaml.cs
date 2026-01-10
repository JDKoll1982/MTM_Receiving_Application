using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_CustomControls : Page
{
    public ViewModel_UI_Mockup_CustomControls ViewModel { get; }

    public View_UI_Mockup_CustomControls()
    {
        ViewModel = new ViewModel_UI_Mockup_CustomControls();
        InitializeComponent();
    }
}
