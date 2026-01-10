using CommunityToolkit.Mvvm.ComponentModel;

namespace Module_UI_Mockup.ViewModels;

public partial class ViewModel_UI_Mockup_Status : ViewModel_Base
{
    [ObservableProperty] private double _progressValue = 65;

    public ViewModel_UI_Mockup_Status()
    {
        StatusMessage = "Status & Feedback Controls";
    }
}
