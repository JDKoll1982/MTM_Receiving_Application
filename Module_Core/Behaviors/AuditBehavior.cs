using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MTM_Receiving_Application.Module_Core.Behaviors
{
    /// <summary>
    /// Global pipeline behavior that adds audit context to all requests.
    /// Automatically applied to ALL handlers in ALL modules.
    /// Logs user context, machine name, and timestamp for compliance and troubleshooting.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

        public AuditBehavior(ILogger<AuditBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Capture audit context
            var userId = Environment.UserName; // TODO: Replace with authenticated user from IService_Authentication
            var timestamp = DateTime.UtcNow;
            var machineName = Environment.MachineName;

            _logger.LogInformation(
                "Audit: User {UserId} executing {RequestName} at {Timestamp} from {MachineName}",
                userId,
                requestName,
                timestamp,
                machineName);

            // Proceed to next behavior or handler
            return await next();
        }
    }
}
