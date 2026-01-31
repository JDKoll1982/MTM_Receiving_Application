using System;
using Microsoft.UI.Dispatching;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services
{
    /// <summary>
    /// Implementation of IDispatcherService using WinUI DispatcherQueue.
    /// Lazy-initializes the dispatcher queue on first use to support DI container build before UI thread starts.
    /// </summary>
    public class Service_Dispatcher : IService_Dispatcher
    {
        private DispatcherQueue? _dispatcherQueue;

        private DispatcherQueue DispatcherQueue
        {
            get
            {
                if (_dispatcherQueue == null)
                {
                    _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    if (_dispatcherQueue == null)
                    {
                        throw new InvalidOperationException(
                            "DispatcherQueue is not available. Service_Dispatcher must be used from the UI thread.");
                    }
                }
                return _dispatcherQueue;
            }
        }

        /// <summary>
        /// Parameterless constructor for DI container.
        /// Dispatcher queue is lazy-initialized on first use.
        /// </summary>
        public Service_Dispatcher()
        {
        }

        /// <summary>
        /// Constructor that accepts a dispatcher queue (for testing or explicit initialization).
        /// </summary>
        public Service_Dispatcher(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        public IService_DispatcherTimer CreateTimer()
        {
            return new Service_DispatcherTimerWrapper(DispatcherQueue.CreateTimer());
        }

        public bool TryEnqueue(Action callback)
        {
            return DispatcherQueue.TryEnqueue(() => callback());
        }
    }
}


