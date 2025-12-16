using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service contract for session management and timeout monitoring.
    /// Tracks active user sessions and handles inactivity timeouts.
    /// </summary>
    public interface IService_SessionManager
    {
        /// <summary>
        /// Gets the current active user session.
        /// Null if no user is logged in.
        /// </summary>
        Model_UserSession? CurrentSession { get; }

        /// <summary>
        /// Creates a new user session after successful authentication.
        /// Sets timeout duration based on workstation type (30 min personal, 15 min shared).
        /// </summary>
        /// <param name="user">Authenticated user</param>
        /// <param name="workstationConfig">Workstation configuration</param>
        /// <param name="authenticationMethod">Authentication method used (windows_auto, pin_login)</param>
        /// <returns>Created session</returns>
        Model_UserSession CreateSession(Model_User user, Model_WorkstationConfig workstationConfig, string authenticationMethod);

        /// <summary>
        /// Updates the last activity timestamp to current time.
        /// Called on any user interaction (mouse, keyboard, window focus).
        /// Resets the inactivity timer.
        /// </summary>
        void UpdateLastActivity();

        /// <summary>
        /// Starts monitoring for session timeout.
        /// Checks every 60 seconds if session has exceeded timeout duration.
        /// Raises SessionTimedOut event when timeout detected.
        /// </summary>
        void StartTimeoutMonitoring();

        /// <summary>
        /// Stops timeout monitoring and cleans up resources.
        /// Called during application shutdown.
        /// </summary>
        void StopTimeoutMonitoring();

        /// <summary>
        /// Checks if the current session has timed out.
        /// </summary>
        /// <returns>True if session exceeded timeout duration</returns>
        bool IsSessionTimedOut();

        /// <summary>
        /// Ends the current session and logs the activity.
        /// Called when user closes application or session times out.
        /// </summary>
        /// <param name="reason">Reason for session end (manual_close, session_timeout)</param>
        Task EndSessionAsync(string reason);

        /// <summary>
        /// Event raised when session timeout is detected.
        /// Application should close when this event fires.
        /// </summary>
        event EventHandler<SessionTimedOutEventArgs>? SessionTimedOut;
    }

    /// <summary>
    /// Event arguments for SessionTimedOut event.
    /// </summary>
    public class SessionTimedOutEventArgs : EventArgs
    {
        public Model_UserSession Session { get; set; } = null!;
        public TimeSpan IdleDuration { get; set; }
    }
}
