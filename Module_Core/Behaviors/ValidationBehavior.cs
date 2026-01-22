using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace MTM_Receiving_Application.Module_Core.Behaviors
{
    /// <summary>
    /// Global pipeline behavior that validates commands using FluentValidation.
    /// Automatically applied to ALL commands in ALL modules.
    /// Validation occurs BEFORE the handler executes.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // If no validators registered for this request type, skip validation
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            // Run all validators in parallel
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Collect all validation failures
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // If any validation failures, throw ValidationException (short-circuits handler execution)
            if (failures.Count > 0)
            {
                throw new ValidationException(failures);
            }

            // Validation passed, proceed to handler
            return await next();
        }
    }
}
