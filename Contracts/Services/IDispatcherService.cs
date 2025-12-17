using System;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Interface for dispatcher operations.
    /// Wraps DispatcherQueue to allow mocking in unit tests.
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// Creates a new timer on the dispatcher thread.
        /// </summary>
        /// <returns>A new IDispatcherTimer instance</returns>
        IDispatcherTimer CreateTimer();

        /// <summary>
        /// Enqueues a function for execution on the dispatcher thread.
        /// </summary>
        /// <param name="callback">The function to execute</param>
        /// <returns>True if the function was enqueued; otherwise, false.</returns>
        bool TryEnqueue(Action callback);
    }
}
