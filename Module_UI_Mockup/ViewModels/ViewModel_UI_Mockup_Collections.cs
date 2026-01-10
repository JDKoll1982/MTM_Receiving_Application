using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Module_UI_Mockup.ViewModels;

public partial class ViewModel_UI_Mockup_Collections : ViewModel_Base
{
    [ObservableProperty] private ObservableCollection<string> _items;
    [ObservableProperty] private ObservableCollection<string> _gridItems;

    public ViewModel_UI_Mockup_Collections()
    {
        _items = new ObservableCollection<string>
        {
            "Transaction 1 - Received 100 units", "Transaction 2 - Shipped 50 units", "Transaction 3 - Inspected 75 units",
            "Transaction 4 - Stored 200 units", "Transaction 5 - Counted 150 units"
        };

        _gridItems = new ObservableCollection<string>
        {
            "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6"
        };

        StatusMessage = "Collections & Data Display";
    }
}
