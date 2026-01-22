using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MTM_Receiving_Application.Infrastructure.Logging;

/// <summary>
/// Configurations for Serilog logging.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog from the application configuration.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog configuration builder.</param>
    /// <param name="appConfiguration">The application configuration (appsettings).</param>
    public static void Configure(LoggerConfiguration loggerConfiguration, IConfiguration appConfiguration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(appConfiguration);

        // Configure Serilog from appsettings.json
        // Supports environment-specific configuration (Development, Staging, Production)
        loggerConfiguration.ReadFrom.Configuration(appConfiguration);
    }
}
