using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Behaviors;
using System.Reflection;

namespace MTM_Receiving_Application.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering CQRS infrastructure (MediatR + FluentValidation).
/// </summary>
public static class CqrsInfrastructureExtensions
{
    /// <summary>
    /// Registers MediatR with global pipeline behaviors and FluentValidation auto-discovery.
    /// Pipeline behaviors are executed in order: Logging → Validation → Audit.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCqrsInfrastructure(this IServiceCollection services)
    {
        // MediatR Configuration with Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            // Register all handlers from this assembly
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Register GLOBAL pipeline behaviors (applied to ALL handlers in ALL modules)
            // Order matters: Logging → Validation → Audit → Handler
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        });

        // FluentValidation Auto-discovery
        // Automatically registers all validators found in the assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
