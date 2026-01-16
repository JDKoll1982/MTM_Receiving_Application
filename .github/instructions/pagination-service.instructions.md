<instruction>
<description>Guidelines for using the Pagination Service in the MTM Receiving Application</description>
<file>c:\Users\johnk\source\repos\MTM_Receiving_Application\.github\instructions\pagination-service.instructions.md</file>
<applyTo>**/*ViewModel.cs</applyTo>
</instruction>

# Pagination Service Instructions

When implementing pagination in ViewModels, follow these guidelines:

1. **Injection**: Always inject `IService_Pagination` via constructor injection.
2. **State Management**:
    - Maintain a "Master" collection (all items) and a "Display" collection (current page items).
    - The `IService_Pagination` manages the logic, but the ViewModel must update the `ObservableCollection` bound to the View.
3. **Event Handling**:
    - Subscribe to `PageChanged` event in the constructor.
    - Unsubscribe in `Dispose` or if the ViewModel is transient and not disposed, ensure no memory leaks (though transient VMs are usually fine).
4. **UI Binding**:
    - Expose `CurrentPage`, `TotalPages`, `HasNextPage`, `HasPreviousPage` properties in the ViewModel.
    - Update these properties in the `PageChanged` handler.
5. **Filtering**:
    - If filtering is applied, update the source of the `IService_Pagination` with the filtered list, then refresh the display.

## Example

```csharp
public partial class MyViewModel : BaseViewModel
{
    private readonly IService_Pagination _paginationService;
    
    [ObservableProperty]
    private ObservableCollection<MyItem> _items; // Bound to DataGrid

    [ObservableProperty]
    private int _currentPage;

    [ObservableProperty]
    private int _totalPages;

    public MyViewModel(IService_Pagination paginationService)
    {
        _paginationService = paginationService;
        _paginationService.PageChanged += (s, e) => UpdateDisplay();
    }

    public void LoadData(List<MyItem> data)
    {
        _paginationService.SetSource(data);
        // UpdateDisplay is called automatically by SetSource via PageChanged? 
        // Check implementation. If not, call UpdateDisplay().
        // Current implementation calls OnPageChanged inside SetSource.
    }

    private void UpdateDisplay()
    {
        var pageItems = _paginationService.GetCurrentPageItems<MyItem>();
        Items.Clear();
        foreach(var item in pageItems) Items.Add(item);
        
        CurrentPage = _paginationService.CurrentPage;
        TotalPages = _paginationService.TotalPages;
        
        // Notify commands
        NextPageCommand.NotifyCanExecuteChanged();
        PreviousPageCommand.NotifyCanExecuteChanged();
    }
}
```
