# service-template.md

Last Updated: 2026-01-24

This is a template for creating a Service class in the MTM Receiving Application using MVVM architecture and CommunityToolkit.Mvvm.

```csharp
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.[MODULE].Contracts.Services;

public interface IService_[SERVICE_NAME]
{
    Task<Model_Dao_Result> LoadAsync();
}

namespace MTM_Receiving_Application.[MODULE].Services;

public class Service_[SERVICE_NAME] : IService_[SERVICE_NAME]
{
    private readonly Dao_[ENTITY] _dao;

    public Service_[SERVICE_NAME](Dao_[ENTITY] dao)
    {
        _dao = dao;
    }

    public async Task<Model_Dao_Result> LoadAsync()
    {
        return await _dao.LoadAsync();
    }
}
```
