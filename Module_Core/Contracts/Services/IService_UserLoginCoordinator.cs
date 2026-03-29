using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Coordinates authenticated-session startup, logout, and manual re-login flows.
/// </summary>
public interface IService_UserLoginCoordinator
{
    /// <summary>
    /// Creates the active session for an already-authenticated user and initializes session-bound services.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="workstationConfig">The workstation configuration used for timeout behavior.</param>
    /// <param name="authenticationMethod">The authentication method used for the login.</param>
    /// <returns>The created user session.</returns>
    Task<Model_Dao_Result<Model_UserSession>> InitializeAuthenticatedSessionAsync(
        Model_User user,
        Model_WorkstationConfig workstationConfig,
        string authenticationMethod
    );

    /// <summary>
    /// Ends the current session and prompts for a manual login so a different user can sign in.
    /// </summary>
    /// <param name="logoutReason">The reason to record for the logout.</param>
    /// <returns>The newly created session when login succeeds.</returns>
    Task<Model_Dao_Result<Model_UserSession>> LogOutAndPromptForLoginAsync(string logoutReason);
}
