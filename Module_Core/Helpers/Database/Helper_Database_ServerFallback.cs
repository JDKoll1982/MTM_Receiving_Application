using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Core.Helpers.Database;

/// <summary>
/// Picks the MySQL server the app talks to at startup: the server in
/// <c>ConnectionStrings:MySql</c> when it answers, otherwise the spare server (localhost by default).
/// </summary>
/// <remarks>
/// The deployment tool has always resolved its server this way, and the app now uses the same rule, so
/// a PC that cannot see the shop server still starts instead of hanging on connect timeouts.
/// The probe is a TCP connect with a short budget; it never proves credentials, only reachability, so a
/// wrong password still surfaces as a sign-in failure instead of a silent retarget.
/// </remarks>
public static class Helper_Database_ServerFallback
{
    /// <summary>
    /// Configuration key that names the spare server. Optional; defaults to localhost.
    /// </summary>
    public const string FallbackServerConfigurationKey = "AppSettings:MySqlFallbackServer";

    /// <summary>
    /// Server used when the configured one does not answer and no spare was configured.
    /// </summary>
    public const string DefaultFallbackServer = "127.0.0.1";

    /// <summary>
    /// How long the probe may take before the server counts as unavailable.
    /// </summary>
    public const int ProbeTimeoutMilliseconds = 2000;

    /// <summary>
    /// Builds the configuration values that should override <c>ConnectionStrings:MySql</c>.
    /// </summary>
    /// <returns>
    /// An empty dictionary when the configured server already answers (or when no fallback is
    /// reachable), so nothing is retargeted without a reason.
    /// </returns>
    public static IReadOnlyDictionary<string, string?> BuildConfigurationOverride(
        IConfiguration configuration,
        Func<string, int, bool>? canReach = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var configured = configuration.GetConnectionString("MySql");
        var fallbackServer = configuration[FallbackServerConfigurationKey];
        var resolved = ResolveConnectionString(configured, fallbackServer, canReach);

        return resolved is null
            ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["ConnectionStrings:MySql"] = resolved,
            };
    }

    /// <summary>
    /// Returns a connection string aimed at the spare server, or <c>null</c> when the configured
    /// server is reachable or no spare server answers.
    /// </summary>
    public static string? ResolveConnectionString(
        string? configuredConnectionString,
        string? fallbackServer = null,
        Func<string, int, bool>? canReach = null)
    {
        if (string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return null;
        }

        MySqlConnectionStringBuilder builder;
        try
        {
            builder = new MySqlConnectionStringBuilder(configuredConnectionString);
        }
        catch (Exception exception) when (IsUnparsable(exception))
        {
            // An unparsable connection string is reported by the normal connection path.
            return null;
        }

        var probe = canReach ?? CanReach;
        var primaryServer = builder.Server;
        var port = (int)builder.Port;

        if (string.IsNullOrWhiteSpace(primaryServer) || probe(primaryServer, port))
        {
            return null;
        }

        var spareServer = string.IsNullOrWhiteSpace(fallbackServer)
            ? DefaultFallbackServer
            : fallbackServer.Trim();

        if (
            string.Equals(spareServer, primaryServer, StringComparison.OrdinalIgnoreCase)
            || probe(spareServer, port) is false
        )
        {
            return null;
        }

        builder.Server = spareServer;
        return builder.ConnectionString;
    }

    /// <summary>
    /// Names the server a connection string points at, for logging.
    /// </summary>
    public static string? DescribeTargetServer(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            return string.IsNullOrWhiteSpace(builder.Server)
                ? null
                : $"{builder.Server}:{builder.Port}";
        }
        catch (Exception exception) when (IsUnparsable(exception))
        {
            return null;
        }
    }

    /// <summary>
    /// MySql.Data reports a malformed connection string with its own exception type, so the
    /// shape check covers the three ways a string can fail to parse.
    /// </summary>
    private static bool IsUnparsable(Exception exception) =>
        exception is ArgumentException or FormatException or MySqlException;

    private static bool CanReach(string server, int port)
    {
        try
        {
            using var client = new TcpClient();
            using var cancellation = new CancellationTokenSource(ProbeTimeoutMilliseconds);
            client.ConnectAsync(server, port, cancellation.Token).AsTask().GetAwaiter().GetResult();
            return client.Connected;
        }
        catch (Exception exception)
            when (exception is SocketException
                or OperationCanceledException
                or AggregateException
                or ArgumentException)
        {
            return false;
        }
    }
}
