using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Data.Authentication;

namespace MTM_Receiving_Application.Module_Core.Services.Authentication
{
    /// <summary>
    /// Service implementation for session management and timeout monitoring.
    /// Uses IDispatcherService to check for session timeouts every 60 seconds.
    /// </summary>
    public class Service_UserSessionManager : IService_UserSessionManager
    {
        private readonly Dao_User _daoUser;
        private readonly IService_Dispatcher _dispatcherService;
        private IService_DispatcherTimer? _timeoutTimer;
        private const int TimerIntervalSeconds = 60;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="daoUser">User data access object for activity logging</param>
        /// <param name="dispatcherService">Dispatcher service for timer operations</param>
        public Service_UserSessionManager(Dao_User daoUser, IService_Dispatcher dispatcherService)
        {
            _daoUser = daoUser ?? throw new ArgumentNullException(nameof(daoUser));
            _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
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
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (workstationConfig == null)
            {
                throw new ArgumentNullException(nameof(workstationConfig));
            }

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
            CurrentSession?.UpdateLastActivity();
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
            _timeoutTimer = _dispatcherService.CreateTimer();
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
            if (CurrentSession?.User == null)
            {
                return;
            }

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
        public event EventHandler<Model_SessionTimedOutEventArgs>? SessionTimedOut;

        // ====================================================================
        // Private Methods
        // ====================================================================

        /// <summary>
        /// Timer tick handler - checks for session timeout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTimerTick(object? sender, object args)
        {
            if (CurrentSession == null)
            {
                return;
            }

            if (IsSessionTimedOut())
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Session timeout detected. Idle time: {CurrentSession.TimeSinceLastActivity.TotalMinutes:F1} minutes");

                // Raise timeout event
                SessionTimedOut?.Invoke(this, new Model_SessionTimedOutEventArgs
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

