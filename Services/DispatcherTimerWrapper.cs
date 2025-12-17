using System;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Services
{
    /// <summary>
    /// Wrapper for DispatcherQueueTimer.
    /// </summary>
    public class DispatcherTimerWrapper : IDispatcherTimer
    {
        private readonly DispatcherQueueTimer _timer;

        public DispatcherTimerWrapper(DispatcherQueueTimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _timer.Tick += (s, e) => Tick?.Invoke(this, e);
        }

        public TimeSpan Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool IsRepeating
        {
            get => _timer.IsRepeating;
            set => _timer.IsRepeating = value;
        }

        public bool IsRunning => _timer.IsRunning;

        public event EventHandler<object>? Tick;

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();
    }
}
