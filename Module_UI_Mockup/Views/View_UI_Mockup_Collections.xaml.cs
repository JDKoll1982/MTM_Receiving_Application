using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_Collections : Page
{
    public ViewModel_UI_Mockup_Collections ViewModel { get; }

    public View_UI_Mockup_Collections()
    {
        ViewModel = new ViewModel_UI_Mockup_Collections();
        InitializeComponent();
    }
}
