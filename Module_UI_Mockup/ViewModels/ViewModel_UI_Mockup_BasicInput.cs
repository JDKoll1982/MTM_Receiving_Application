using CommunityToolkit.Mvvm.ComponentModel;

namespace Module_UI_Mockup.ViewModels;

public partial class ViewModel_UI_Mockup_BasicInput : ViewModel_Base
{
    [ObservableProperty] private bool _isToggled;
    [ObservableProperty] private bool _isChecked = true;
    [ObservableProperty] private double _sliderValue = 50;
    [ObservableProperty] private bool _isSwitchOn = true;

    public ViewModel_UI_Mockup_BasicInput()
    {
        StatusMessage = "Basic Input Controls";
    }
}
