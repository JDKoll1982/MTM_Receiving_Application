using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Default implementation of <see cref="IService_DatabaseTargetManager"/>.
/// </summary>
public sealed class Service_DatabaseTargetManager : IService_DatabaseTargetManager
{
    private readonly IConfiguration _configuration;
    private readonly IService_LoggingUtility _logger;

    private static readonly IReadOnlyList<Model_DatabaseTarget> KnownTargets =
        new List<Model_DatabaseTarget>
        {
            new(
                "live",
                "Live (core)",
                "mtm_receiving_application",
                "The production database the SyncTool writes reference data into."
            ),
            new(
                "test",
                "Test",
                "mtm_receiving_application_test",
                "The regular test database used for development."
            ),
            new(
                "sync-copy",
                "SyncTool copy",
                "mtm_receiving_application_backup_test",
                "Disposable verification copy produced by the SyncTool."
            ),
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="Service_DatabaseTargetManager"/> class.
    /// </summary>
    public Service_DatabaseTargetManager(
        IConfiguration configuration,
        IService_LoggingUtility logger
    )
    {
        _configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public IReadOnlyList<Model_DatabaseTarget> Targets => KnownTargets;

    /// <inheritdoc/>
    public string CurrentConnectionString =>
        _configuration.GetConnectionString("MySql") ?? string.Empty;

    /// <inheritdoc/>
    public string CurrentDatabaseName =>
        TryParse(CurrentConnectionString)?.Database ?? "(not configured)";

    /// <inheritdoc/>
    public string CurrentServerDisplay
    {
        get
        {
            var builder = TryParse(CurrentConnectionString);
            return builder is null ? "(unknown)" : $"{builder.Server}:{builder.Port}";
        }
    }

    /// <inheritdoc/>
    public string BuildConnectionString(Model_DatabaseTarget target)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var builder = TryParse(CurrentConnectionString) ?? new MySqlConnectionStringBuilder();
        builder.Database = target.DatabaseName;
        return builder.ConnectionString;
    }

    /// <inheritdoc/>
    public Model_DatabaseTarget? FindByDatabaseName(string? databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return null;
        }

        foreach (var target in KnownTargets)
        {
            if (string.Equals(target.DatabaseName, databaseName, StringComparison.OrdinalIgnoreCase))
            {
                return target;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task SaveTargetAsync(Model_DatabaseTarget target)
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var root = await Helper_LocalAppConfigFile.ReadRootAsync();
        var connectionStrings = root["ConnectionStrings"] as JsonObject ?? new JsonObject();
        connectionStrings["MySql"] = BuildConnectionString(target);
        root["ConnectionStrings"] = connectionStrings;

        await Helper_LocalAppConfigFile.WriteRootAsync(root);

        _logger.LogInfo(
            $"Database target saved to local override: {target.DatabaseName}",
            "Settings.DatabaseConfig"
        );
    }

    /// <inheritdoc/>
    public async Task<string> GetPendingDatabaseNameAsync()
    {
        var connectionString = await ReadOverrideMySqlConnectionStringAsync();
        return TryParse(connectionString)?.Database ?? CurrentDatabaseName;
    }

    /// <inheritdoc/>
    public async Task<bool> GetIsRestartPendingAsync()
    {
        var connectionString = await ReadOverrideMySqlConnectionStringAsync();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        var pendingDatabase = TryParse(connectionString)?.Database;
        if (string.IsNullOrWhiteSpace(pendingDatabase))
        {
            return false;
        }

        return !string.Equals(
            pendingDatabase,
            CurrentDatabaseName,
            StringComparison.OrdinalIgnoreCase
        );
    }

    /// <inheritdoc/>
    public async Task<Model_DatabaseTestResult> TestConnectionAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new Model_DatabaseTestResult(false, "No MySQL connection string configured.");
        }

        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("SELECT DATABASE(), VERSION()", connection);
            await using var reader = await command.ExecuteReaderAsync();

            var database = "?";
            var version = "?";
            if (await reader.ReadAsync())
            {
                database = Convert.ToString(reader[0]) ?? "?";
                version = Convert.ToString(reader[1]) ?? "?";
            }

            return new Model_DatabaseTestResult(
                true,
                $"Connected to '{database}' (MySQL {version})."
            );
        }
        catch (Exception ex)
        {
            return new Model_DatabaseTestResult(false, $"Connection failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Model_DatabaseEnumeration> EnumerateDatabasesAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentConnectionString))
        {
            return new Model_DatabaseEnumeration(
                false,
                Array.Empty<string>(),
                "No MySQL connection string configured."
            );
        }

        try
        {
            await using var connection = new MySqlConnection(CurrentConnectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("SHOW DATABASES", connection);
            await using var reader = await command.ExecuteReaderAsync();

            var databases = new List<string>();
            while (await reader.ReadAsync())
            {
                var name = Convert.ToString(reader[0]);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    databases.Add(name);
                }
            }

            return new Model_DatabaseEnumeration(
                true,
                databases,
                $"Found {databases.Count} database(s) on {CurrentServerDisplay}."
            );
        }
        catch (Exception ex)
        {
            return new Model_DatabaseEnumeration(
                false,
                Array.Empty<string>(),
                $"Could not list databases: {ex.Message}"
            );
        }
    }

    private static MySqlConnectionStringBuilder? TryParse(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        try
        {
            return new MySqlConnectionStringBuilder(connectionString);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> ReadOverrideMySqlConnectionStringAsync()
    {
        var root = await Helper_LocalAppConfigFile.ReadRootAsync();
        if (root["ConnectionStrings"] is JsonObject connectionStrings)
        {
            return connectionStrings["MySql"]?.GetValue<string>();
        }

        return null;
    }
}
