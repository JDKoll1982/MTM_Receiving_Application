using System;

namespace MTM_Receiving_Application.Module_Core.Models.Systems
{
    /// <summary>
    /// Represents the current active user session with timeout tracking.
    /// Maintained in memory during application lifetime.
    /// </summary>
    public class Model_UserSession
    {
        private static Func<DateTime> NowProvider { get; } = static () => DateTime.Now;

        // ====================================================================
        // User Information
        // ====================================================================

        /// <summary>
        /// Authenticated user for this session
        /// </summary>
        public Model_User? User { get; set; }

        // ====================================================================
        // Session Metadata
        // ====================================================================

        /// <summary>
        /// Computer name where session is active (from Environment.MachineName)
        /// </summary>
        public string WorkstationName { get; set; } = string.Empty;

        /// <summary>
        /// Workstation type: "personal_workstation" or "shared_terminal"
        /// </summary>
        public string WorkstationType { get; set; } = string.Empty;

        /// <summary>
        /// Authentication method used: "windows_auto" or "pin_login"
        /// </summary>
        public string AuthenticationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when user logged in
        /// </summary>
        public DateTime LoginTimestamp { get; set; }

        /// <summary>
        /// Timestamp of last user interaction (mouse, keyboard, window focus)
        /// Updated by UpdateLastActivity() method
        /// </summary>
        public DateTime LastActivityTimestamp { get; set; }

        // ====================================================================
        // Session Management
        // ====================================================================

        /// <summary>
        /// Session timeout duration based on workstation type
        /// 30 minutes for personal workstations, 15 minutes for shared terminals
        /// </summary>
        public TimeSpan TimeoutDuration { get; set; }

        /// <summary>
        /// Time elapsed since last user activity
        /// </summary>
        public TimeSpan TimeSinceLastActivity => NowProvider() - LastActivityTimestamp;

        /// <summary>
        /// Indicates if session has exceeded timeout duration
        /// </summary>
        public bool IsTimedOut => TimeSinceLastActivity >= TimeoutDuration;

        /// <summary>
        /// Indicates if user has configured ERP access (copied from User model)
        /// </summary>
        public bool HasErpAccess => User?.HasErpAccess ?? false;

        // ====================================================================
        // Methods
        // ====================================================================

        /// <summary>
        /// Updates the last activity timestamp to current time.
        /// Called on any user interaction event (mouse, keyboard, window focus).
        /// </summary>
        public void UpdateLastActivity()
        {
            LastActivityTimestamp = NowProvider();
        }

        // ====================================================================
        // Constructor
        // ====================================================================

        /// <summary>
        /// Default constructor
        /// </summary>
        public Model_UserSession()
        {
            LoginTimestamp = NowProvider();
            LastActivityTimestamp = NowProvider();
        }

        /// <summary>
        /// Constructor that requires an authenticated user.
        /// </summary>
        /// <param name="user">Authenticated user for this session.</param>
        public Model_UserSession(Model_User user) : this()
        {
            ArgumentNullException.ThrowIfNull(user);
            User = user;
        }
    }
}

