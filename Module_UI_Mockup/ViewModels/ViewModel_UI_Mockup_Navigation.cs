using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Module_UI_Mockup.ViewModels;

public partial class ViewModel_UI_Mockup_Navigation : ViewModel_Base
{
    [ObservableProperty]
    private ObservableCollection<string> _breadcrumbItems;

    public ViewModel_UI_Mockup_Navigation()
    {
        _breadcrumbItems = new ObservableCollection<string>
        {
            "Home", "Documents", "Work", "Project"
        };
        StatusMessage = "Navigation Controls";
    }
}
