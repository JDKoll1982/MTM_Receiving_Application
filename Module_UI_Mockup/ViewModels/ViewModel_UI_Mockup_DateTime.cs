using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace Module_UI_Mockup.ViewModels;

public partial class ViewModel_UI_Mockup_DateTime : ViewModel_Base
{
    [ObservableProperty] private DateTimeOffset _deliveryDate = DateTimeOffset.Now.AddDays(7);
    [ObservableProperty] private DateTimeOffset _startDate = DateTimeOffset.Now;
    [ObservableProperty] private TimeSpan _shiftTime = new TimeSpan(8, 0, 0);
    [ObservableProperty] private ObservableCollection<DateTimeOffset> _selectedDates;

    public ViewModel_UI_Mockup_DateTime()
    {
        _selectedDates = new ObservableCollection<DateTimeOffset> { DateTimeOffset.Now };
        StatusMessage = "Date & Time Pickers";
    }
}
