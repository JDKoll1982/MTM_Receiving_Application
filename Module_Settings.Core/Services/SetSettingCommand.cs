using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Command to set a core setting value.
/// </summary>
public sealed record SetSettingCommand(string Category, string Key, string Value, int? UserId = null) : IRequest<Model_Dao_Result>;
