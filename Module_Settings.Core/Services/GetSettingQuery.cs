using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Query to retrieve a core setting.
/// </summary>
/// <param name="Category"></param>
/// <param name="Key"></param>
/// <param name="UserId"></param>
public sealed record GetSettingQuery(string Category, string Key, int? UserId = null) : IRequest<Model_Dao_Result<Model_SettingsValue>>;
