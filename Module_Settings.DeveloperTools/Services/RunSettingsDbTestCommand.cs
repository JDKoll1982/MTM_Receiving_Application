using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Services;

/// <summary>
/// Command to run settings database tests.
/// </summary>
public sealed record RunSettingsDbTestCommand() : IRequest<Model_Dao_Result<Model_SettingsDbTestReport>>;
