using System;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Interface for a timer that can be used for scheduling tasks.
    /// Wraps DispatcherQueueTimer to allow mocking in unit tests.
    /// </summary>
    public interface IService_DispatcherTimer
    {
        /// <summary>
        /// Gets or sets the amount of time between timer ticks.
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the timer repeats.
        /// </summary>
        public bool IsRepeating { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the timer is running.
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// Occurs when the timer interval has elapsed.
        /// </summary>
        public event EventHandler<object> Tick;

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start();

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop();
    }
}
