# Pagination Service Standards

**Category**: UI Services
**Last Updated**: December 26, 2025
**Applies To**: `IService_Pagination`, `Service_Pagination`

## Overview

The `Service_Pagination` provides in-memory pagination logic for collections. It decouples pagination state and logic from ViewModels.

## Responsibilities

1. **State Management**: Track `CurrentPage`, `PageSize`, `TotalItems`, `TotalPages`.
2. **Navigation Logic**: `NextPage`, `PreviousPage`, `FirstPage`, `LastPage`, `GoToPage`.
3. **Data Slicing**: Return the subset of items for the current page.
4. **Events**: Notify when the page changes via `PageChanged` event.

## Implementation Pattern

The service holds a reference to the full source collection (as `IEnumerable<object>`) and slices it on demand.

```csharp
public void SetSource<T>(IEnumerable<T> source)
{
    _source = source?.Cast<object>() ?? Enumerable.Empty<object>();
    _currentPage = 1;
    OnPageChanged();
}

public IEnumerable<T> GetCurrentPageItems<T>()
{
    return _source
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize)
        .Cast<T>();
}
```

## Usage in ViewModels

1. Inject `IService_Pagination`.
2. Expose `PagedItems` (ObservableCollection) in the ViewModel.
3. Subscribe to `PageChanged` event to update `PagedItems`.
4. Bind UI commands (Next/Prev) to ViewModel commands that call Service methods.

```csharp
// ViewModel Setup
_paginationService.PageChanged += (s, e) => UpdatePagedItems();
_paginationService.SetSource(allItems);

private void UpdatePagedItems()
{
    PagedItems.Clear();
    foreach(var item in _paginationService.GetCurrentPageItems<MyModel>())
        PagedItems.Add(item);
}
```

## Registration

- Register as **Transient**. Each ViewModel needing pagination should get its own instance.
