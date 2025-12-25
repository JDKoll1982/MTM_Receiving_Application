using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service contract for user session management and timeout monitoring.
    /// </summary>
    public interface IService_UserSessionManager
    {
        /// <summary>
        /// Gets the current active user session.
        /// </summary>
        Model_UserSession? CurrentSession { get; }

        /// <summary>
        /// Creates a new user session.
        /// </summary>
        /// <param name="user">Authenticated user</param>
        /// <param name="workstationConfig">Workstation configuration</param>
        /// <param name="authenticationMethod">Method used for authentication</param>
        /// <returns>The created session</returns>
        Model_UserSession CreateSession(
            Model_User user, 
            Model_WorkstationConfig workstationConfig, 
            string authenticationMethod);

        /// <summary>
        /// Updates the last activity timestamp to prevent timeout.
        /// </summary>
        void UpdateLastActivity();

        /// <summary>
        /// Starts the session timeout monitoring timer.
        /// </summary>
        void StartTimeoutMonitoring();

        /// <summary>
        /// Stops the session timeout monitoring timer.
        /// </summary>
        void StopTimeoutMonitoring();

        /// <summary>
        /// Checks if the current session has timed out.
        /// </summary>
        /// <returns>True if timed out, false otherwise</returns>
        bool IsSessionTimedOut();

        /// <summary>
        /// Ends the current session and logs the reason.
        /// </summary>
        /// <param name="reason">Reason for ending session</param>
        Task EndSessionAsync(string reason);

        /// <summary>
        /// Event raised when the session times out.
        /// </summary>
        event EventHandler<SessionTimedOutEventArgs> SessionTimedOut;
    }

    public class SessionTimedOutEventArgs : EventArgs
    {
        public Model_UserSession Session { get; set; } = null!;
        public TimeSpan IdleDuration { get; set; }
    }
}
