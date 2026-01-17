using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Facade for external modules to access core settings.
/// </summary>
public class Service_SettingsCoreFacade : IService_SettingsCoreFacade
{
    private readonly IMediator _mediator;
    private readonly ISettingsMetadataRegistry _registry;

    public Service_SettingsCoreFacade(IMediator mediator, ISettingsMetadataRegistry registry)
    {
        _mediator = mediator;
        _registry = registry;
    }

    public Task<Model_Dao_Result<Model_SettingsValue>> GetSettingAsync(string category, string key, int? userId = null)
    {
        return _mediator.Send(new GetSettingQuery(category, key, userId));
    }

    public Task<Model_Dao_Result> SetSettingAsync(string category, string key, string value, int? userId = null)
    {
        return _mediator.Send(new SetSettingCommand(category, key, value, userId));
    }

    public Task<Model_Dao_Result> ResetSettingAsync(string category, string key, int? userId = null)
    {
        return _mediator.Send(new ResetSettingCommand(category, key, userId));
    }

    public async Task InitializeDefaultsAsync(int? userId = null)
    {
        foreach (var definition in _registry.GetAll())
        {
            if (definition.Scope == Enum_SettingsScope.User && !userId.HasValue)
            {
                continue;
            }

            await _mediator.Send(new GetSettingQuery(definition.Category, definition.Key, userId));
        }
    }
}
