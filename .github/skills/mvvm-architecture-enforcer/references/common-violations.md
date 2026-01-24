# Common MVVM Violations and Fixes

Last Updated: 2026-01-24

This document provides quick before/after examples for common architecture violations.

## Violation: ViewModel calls DAO directly

### Wrong

```csharp
public partial class ViewModel_Bad : ViewModel_Shared_Base
{
    public async Task LoadAsync()
    {
        var result = await _dao.GetSomethingAsync();
    }
}
```

### Correct

```csharp
public partial class ViewModel_Good : ViewModel_Shared_Base
{
    private readonly IService_Something _service;

    public ViewModel_Good(IService_Something service, IService_ErrorHandler errorHandler)
        : base(errorHandler)
    {
        _service = service;
    }

    public async Task LoadAsync()
    {
        var result = await _service.GetSomethingAsync();
    }
}
```

## Violation: runtime binding in XAML

### Wrong

```xml
<TextBox Text="{Binding SearchText}" />
```

### Correct

```xml
<TextBox Text="{x:Bind ViewModel.SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

## Violation: non-partial ViewModel

### Wrong

```csharp
public class ViewModel_Bad : ViewModel_Shared_Base
{
}
```

### Correct

```csharp
public partial class ViewModel_Good : ViewModel_Shared_Base
{
}
```
