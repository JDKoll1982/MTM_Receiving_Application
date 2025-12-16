using System;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Data.Authentication;

namespace MTM_Receiving_Application.Services.Authentication
{
    /// <summary>
    /// Service implementation for session management and timeout monitoring.
    /// Uses DispatcherTimer to check for session timeouts every 60 seconds.
    /// </summary>
    public class Service_SessionManager : IService_SessionManager
    {
        private readonly Dao_User _daoUser;
        private readonly DispatcherQueue _dispatcherQueue;
        private DispatcherQueueTimer? _timeoutTimer;
        private const int TimerIntervalSeconds = 60;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="daoUser">User data access object for activity logging</param>
        /// <param name="dispatcherQueue">UI dispatcher for timer operations</param>
        public Service_SessionManager(Dao_User daoUser, DispatcherQueue dispatcherQueue)
        {
            _daoUser = daoUser ?? throw new ArgumentNullException(nameof(daoUser));
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        // ====================================================================
        // Properties
        // ====================================================================

        /// <inheritdoc/>
        public Model_UserSession? CurrentSession { get; private set; }

        // ====================================================================
        // Session Management
        // ====================================================================

        /// <inheritdoc/>
        public Model_UserSession CreateSession(
            Model_User user, 
            Model_WorkstationConfig workstationConfig, 
            string authenticationMethod)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (workstationConfig == null) throw new ArgumentNullException(nameof(workstationConfig));

            CurrentSession = new Model_UserSession
            {
                User = user,
                WorkstationName = workstationConfig.ComputerName,
                WorkstationType = workstationConfig.WorkstationType,
                AuthenticationMethod = authenticationMethod,
                LoginTimestamp = DateTime.Now,
                LastActivityTimestamp = DateTime.Now,
                TimeoutDuration = workstationConfig.TimeoutDuration
            };

            return CurrentSession;
        }

        /// <inheritdoc/>
        public void UpdateLastActivity()
        {
            if (CurrentSession != null)
            {
                CurrentSession.UpdateLastActivity();
            }
        }

        // ====================================================================
        // Timeout Monitoring
        // ====================================================================

        /// <inheritdoc/>
        public void StartTimeoutMonitoring()
        {
            if (CurrentSession == null)
            {
                throw new InvalidOperationException("Cannot start timeout monitoring without an active session");
            }

            // Stop existing timer if running
            StopTimeoutMonitoring();

            // Create new timer
            _timeoutTimer = _dispatcherQueue.CreateTimer();
            _timeoutTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _timeoutTimer.IsRepeating = true;
            _timeoutTimer.Tick += OnTimerTick;
            _timeoutTimer.Start();

            System.Diagnostics.Debug.WriteLine($"Session timeout monitoring started. Timeout: {CurrentSession.TimeoutDuration.TotalMinutes} minutes");
        }

        /// <inheritdoc/>
        public void StopTimeoutMonitoring()
        {
            if (_timeoutTimer != null)
            {
                _timeoutTimer.Stop();
                _timeoutTimer.Tick -= OnTimerTick;
                _timeoutTimer = null;
                
                System.Diagnostics.Debug.WriteLine("Session timeout monitoring stopped");
            }
        }

        /// <inheritdoc/>
        public bool IsSessionTimedOut()
        {
            return CurrentSession?.IsTimedOut ?? false;
        }

        /// <inheritdoc/>
        public async Task EndSessionAsync(string reason)
        {
            if (CurrentSession == null) return;

            try
            {
                // Log session end
                await _daoUser.LogUserActivityAsync(
                    reason,
                    CurrentSession.User.WindowsUsername,
                    CurrentSession.WorkstationName,
                    $"Session ended. Duration: {(DateTime.Now - CurrentSession.LoginTimestamp).TotalMinutes:F1} minutes");

                System.Diagnostics.Debug.WriteLine($"Session ended. Reason: {reason}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log session end: {ex.Message}");
            }
            finally
            {
                // Clean up
                StopTimeoutMonitoring();
                CurrentSession = null;
            }
        }

        // ====================================================================
        // Events
        // ====================================================================

        /// <inheritdoc/>
        public event EventHandler<SessionTimedOutEventArgs>? SessionTimedOut;

        // ====================================================================
        // Private Methods
        // ====================================================================

        /// <summary>
        /// Timer tick handler - checks for session timeout.
        /// </summary>
        private void OnTimerTick(DispatcherQueueTimer sender, object args)
        {
            if (CurrentSession == null) return;

            if (IsSessionTimedOut())
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Session timeout detected. Idle time: {CurrentSession.TimeSinceLastActivity.TotalMinutes:F1} minutes");

                // Raise timeout event
                SessionTimedOut?.Invoke(this, new SessionTimedOutEventArgs
                {
                    Session = CurrentSession,
                    IdleDuration = CurrentSession.TimeSinceLastActivity
                });

                // Stop monitoring (app will close)
                StopTimeoutMonitoring();
            }
        }
    }
}
