using System;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Services
{
    /// <summary>
    /// Implementation of IDispatcherService using WinUI DispatcherQueue.
    /// </summary>
    public class DispatcherService : IDispatcherService
    {
        private readonly DispatcherQueue _dispatcherQueue;

        public DispatcherService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        public IDispatcherTimer CreateTimer()
        {
            return new DispatcherTimerWrapper(_dispatcherQueue.CreateTimer());
        }

        public bool TryEnqueue(Action callback)
        {
            return _dispatcherQueue.TryEnqueue(() => callback());
        }
    }
}
