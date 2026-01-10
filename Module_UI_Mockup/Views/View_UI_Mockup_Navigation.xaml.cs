using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_Navigation : Page
{
    public ViewModel_UI_Mockup_Navigation ViewModel { get; }

    public View_UI_Mockup_Navigation()
    {
        ViewModel = new ViewModel_UI_Mockup_Navigation();
        InitializeComponent();
    }
}
