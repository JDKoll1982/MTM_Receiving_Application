using System;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services
{
    /// <summary>
    /// Implementation of IDispatcherService using WinUI DispatcherQueue.
    /// </summary>
    public class Service_Dispatcher : IService_Dispatcher
    {
        private readonly DispatcherQueue _dispatcherQueue;

        public Service_Dispatcher(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        public IService_DispatcherTimer CreateTimer()
        {
            return new Service_DispatcherTimerWrapper(_dispatcherQueue.CreateTimer());
        }

        public bool TryEnqueue(Action callback)
        {
            return _dispatcherQueue.TryEnqueue(() => callback());
        }
    }
}

