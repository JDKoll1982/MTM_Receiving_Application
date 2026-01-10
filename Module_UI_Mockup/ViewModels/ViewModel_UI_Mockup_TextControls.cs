using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Module_UI_Mockup.ViewModels;

public partial class ViewModel_UI_Mockup_TextControls : ViewModel_Base
{
    [ObservableProperty] private string _inputText = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private double _quantity = 10;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private double _rating = 3;
    [ObservableProperty] private ObservableCollection<string> _partNumbers;

    public ViewModel_UI_Mockup_TextControls()
    {
        _partNumbers = new ObservableCollection<string>
        {
            "ABC-12345-XYZ", "MCC-45678-A", "MMF-98765-B", "PRT-11223-C", "ASM-55667-D"
        };
        StatusMessage = "Text Controls";
    }
}
