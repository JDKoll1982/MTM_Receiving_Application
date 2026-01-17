using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Command to reset a core setting to its default.
/// </summary>
public sealed record ResetSettingCommand(string Category, string Key, int? UserId = null) : IRequest<Model_Dao_Result>;
