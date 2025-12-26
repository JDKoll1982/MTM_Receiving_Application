# Pagination Service

The Pagination Service (`IService_Pagination`) provides a reusable way to handle pagination logic for collections across the application.

## Features
- Generic implementation supports any item type
- Configurable page size
- Navigation methods (Next, Previous, First, Last, GoTo)
- State properties (CurrentPage, TotalPages, TotalItems)
- Event notification on page changes

## Usage

### 1. Inject the Service
Inject `IService_Pagination` into your ViewModel.

```csharp
public class MyViewModel : BaseViewModel
{
    private readonly IService_Pagination _paginationService;

    public MyViewModel(IService_Pagination paginationService)
    {
        _paginationService = paginationService;
        _paginationService.PageChanged += OnPageChanged;
    }
}
```

### 2. Set the Source
Provide the full collection of items to the service.

```csharp
var allItems = await _service.GetDataAsync();
_paginationService.SetSource(allItems);
```

### 3. Bind to Current Page Items
Update your observable collection whenever the page changes.

```csharp
private void OnPageChanged(object sender, EventArgs e)
{
    var pageItems = _paginationService.GetCurrentPageItems<MyModel>();
    DisplayItems.Clear();
    foreach(var item in pageItems) DisplayItems.Add(item);
    
    // Update UI properties
    CurrentPage = _paginationService.CurrentPage;
    TotalPages = _paginationService.TotalPages;
}
```

### 4. Navigation Commands
Expose commands in your ViewModel that call service methods.

```csharp
[RelayCommand]
private void NextPage() => _paginationService.NextPage();

[RelayCommand]
private void PreviousPage() => _paginationService.PreviousPage();
```
