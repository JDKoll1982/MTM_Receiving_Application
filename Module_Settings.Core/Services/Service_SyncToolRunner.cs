using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Default implementation of <see cref="IService_SyncToolRunner"/>.
/// </summary>
public sealed class Service_SyncToolRunner : IService_SyncToolRunner
{
    private readonly IConfiguration _configuration;
    private readonly IService_LoggingUtility _logger;

    /// <inheritdoc/>
    public event Action<string>? OutputReceived;

    /// <summary>
    /// Initializes a new instance of the <see cref="Service_SyncToolRunner"/> class.
    /// </summary>
    public Service_SyncToolRunner(
        IConfiguration configuration,
        IService_LoggingUtility logger
    )
    {
        _configuration =
            configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string DefaultScriptPath => Path.Combine(
        AppContext.BaseDirectory,
        "Database",
        "SyncTool",
        "sync_reference_data.py"
    );

    /// <inheritdoc/>
    public string DefaultPythonPath => "python";

    /// <inheritdoc/>
    public string? ConfiguredScriptPath { get; private set; }

    /// <inheritdoc/>
    public string? ConfiguredPythonPath { get; private set; }

    /// <inheritdoc/>
    public string EffectiveScriptPath =>
        string.IsNullOrWhiteSpace(ConfiguredScriptPath)
            ? DefaultScriptPath
            : ConfiguredScriptPath!;

    /// <inheritdoc/>
    public string EffectivePythonPath =>
        string.IsNullOrWhiteSpace(ConfiguredPythonPath)
            ? DefaultPythonPath
            : ConfiguredPythonPath!;

    /// <inheritdoc/>
    public string? BundledExePath
    {
        get
        {
            var executable = Path.Combine(
                Path.GetDirectoryName(EffectiveScriptPath) ?? string.Empty,
                "sync_reference_data.exe"
            );
            return File.Exists(executable) ? executable : null;
        }
    }

    /// <inheritdoc/>
    public bool IsBundledExeAvailable => BundledExePath is not null;

    /// <inheritdoc/>
    public bool IsScriptAvailable =>
        IsBundledExeAvailable || File.Exists(EffectiveScriptPath);

    /// <inheritdoc/>
    public async Task RefreshConfiguredPathsAsync()
    {
        var root = await Helper_LocalAppConfigFile.ReadRootAsync();
        var databaseConfig = root["DatabaseConfig"] as JsonObject;
        ConfiguredScriptPath = databaseConfig?["SyncToolScriptPath"]?.GetValue<string>();
        ConfiguredPythonPath = databaseConfig?["SyncToolPythonPath"]?.GetValue<string>();
    }

    /// <inheritdoc/>
    public async Task SaveToolPathsAsync(string? scriptPath, string? pythonPath)
    {
        var root = await Helper_LocalAppConfigFile.ReadRootAsync();
        var databaseConfig = root["DatabaseConfig"] as JsonObject ?? new JsonObject();

        if (string.IsNullOrWhiteSpace(scriptPath))
        {
            databaseConfig.Remove("SyncToolScriptPath");
        }
        else
        {
            databaseConfig["SyncToolScriptPath"] = scriptPath.Trim();
        }

        if (string.IsNullOrWhiteSpace(pythonPath))
        {
            databaseConfig.Remove("SyncToolPythonPath");
        }
        else
        {
            databaseConfig["SyncToolPythonPath"] = pythonPath.Trim();
        }

        root["DatabaseConfig"] = databaseConfig;
        await Helper_LocalAppConfigFile.WriteRootAsync(root);

        ConfiguredScriptPath = string.IsNullOrWhiteSpace(scriptPath) ? null : scriptPath.Trim();
        ConfiguredPythonPath = string.IsNullOrWhiteSpace(pythonPath) ? null : pythonPath.Trim();
    }

    /// <inheritdoc/>
    public async Task<int> RunAsync(
        Model_SyncToolRunRequest request,
        string password,
        CancellationToken cancellationToken
    )
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var scriptPath = EffectiveScriptPath;
        var exePath = BundledExePath;
        var useBundledExe = exePath is not null;
        var toolDirectory = Path.GetDirectoryName(scriptPath) ?? AppContext.BaseDirectory;

        // The standalone exe cannot resolve the policy file from its own temp
        // extraction folder, so pass both the policy and backup directory explicitly.
        var policyPath = Path.Combine(toolDirectory, "sync_policy.yml");
        if (!File.Exists(policyPath))
        {
            throw new FileNotFoundException(
                $"SyncTool policy not found at '{policyPath}'. Verify the SyncTool path on this page and that the tool has been copied into the application output.",
                policyPath
            );
        }

        if (!useBundledExe && !File.Exists(scriptPath))
        {
            throw new FileNotFoundException(
                $"SyncTool script not found at '{scriptPath}'. Verify the SyncTool path on this page and that the tool has been copied into the application output.",
                scriptPath
            );
        }

        var (host, port, user) = ResolveMySqlEndpoint();
        var args = BuildArguments(request);
        args.Add("--config");
        args.Add(policyPath);
        args.Add("--host");
        args.Add(host);
        args.Add("--port");
        args.Add(port);
        args.Add("--user");
        args.Add(user);

        var backupDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Database",
            "SyncTool",
            "backups"
        );
        Directory.CreateDirectory(backupDirectory);
        args.Add("--backup-dir");
        args.Add(backupDirectory);

        var startInfo = new ProcessStartInfo
        {
            FileName = useBundledExe ? exePath! : EffectivePythonPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = toolDirectory,
        };

        // Python mode requires the script path as the first argument after the
        // interpreter; the bundled exe is self-contained and needs no script path.
        if (!useBundledExe)
        {
            startInfo.ArgumentList.Add(scriptPath);
        }

        foreach (var argument in args)
        {
            startInfo.ArgumentList.Add(argument);
        }

        // Credentials are passed to the child exclusively through the environment.
        startInfo.Environment["MTM_SYNC_HOST"] = host;
        startInfo.Environment["MTM_SYNC_PORT"] = port;
        startInfo.Environment["MTM_SYNC_USER"] = user;
        startInfo.Environment["MTM_SYNC_PASSWORD"] = password ?? string.Empty;
        startInfo.Environment["MTM_SYNC_NON_INTERACTIVE"] = "1";
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";

        // No external MySQL client tools are required: backup/copy/restore are
        // performed in pure Python inside the bundled executable.

        Emit(
            useBundledExe
                ? $"> {exePath} {DescribeInvocation(request)}"
                : $"> {EffectivePythonPath} {scriptPath} {DescribeInvocation(request)}"
        );
        Emit(string.Empty);

        using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                Emit(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                Emit(e.Data);
            }
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start the SyncTool process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SyncTool run cancelled; terminating child process.", "Settings.SyncTool");
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Process already exited.
            }

            throw;
        }

        var exitCode = process.ExitCode;
        Emit(string.Empty);
        Emit($"[exit code {exitCode}]");
        _logger.LogInfo($"SyncTool finished with exit code {exitCode}.", "Settings.SyncTool");
        return exitCode;
    }

    private void Emit(string text) => OutputReceived?.Invoke(text);

    private static string DescribeInvocation(Model_SyncToolRunRequest request)
    {
        var description = request.Action switch
        {
            Enum_SyncToolAction.Inspect => $"inspect --target {request.Target}",
            Enum_SyncToolAction.WhatIf => $"whatif --target {request.Target}",
            Enum_SyncToolAction.GenerateSql =>
                $"generate-sql --target {request.Target} {request.GenerateSqlOutputPath}",
            Enum_SyncToolAction.Execute =>
                $"execute --target {request.Target}"
                + (request.ConfirmCoreWrite ? " --confirm" : string.Empty)
                + (request.SkipValidation ? " --skip-validation" : string.Empty),
            Enum_SyncToolAction.VerifyCopy =>
                "verify-copy" + (request.KeepCopy ? " --keep-copy" : string.Empty),
            _ => request.Action.ToString(),
        };

        return description;
    }

    private static List<string> BuildArguments(Model_SyncToolRunRequest request)
    {
        var arguments = new List<string>();

        switch (request.Action)
        {
            case Enum_SyncToolAction.Inspect:
                arguments.Add("inspect");
                AddTarget(arguments, request.Target);
                break;

            case Enum_SyncToolAction.WhatIf:
                arguments.Add("whatif");
                AddTarget(arguments, request.Target);
                break;

            case Enum_SyncToolAction.GenerateSql:
                arguments.Add("generate-sql");
                AddTarget(arguments, request.Target);
                arguments.Add(request.GenerateSqlOutputPath ?? string.Empty);
                break;

            case Enum_SyncToolAction.Execute:
                arguments.Add("execute");
                AddTarget(arguments, request.Target);
                if (request.ConfirmCoreWrite)
                {
                    arguments.Add("--confirm");
                }

                if (request.SkipValidation)
                {
                    arguments.Add("--skip-validation");
                }

                break;

            case Enum_SyncToolAction.VerifyCopy:
                arguments.Add("verify-copy");
                if (request.KeepCopy)
                {
                    arguments.Add("--keep-copy");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request),
                    request.Action,
                    "Unsupported SyncTool action."
                );
        }

        arguments.Add("--non-interactive");
        return arguments;
    }

    private static void AddTarget(List<string> arguments, string target)
    {
        arguments.Add("--target");
        arguments.Add(target);
    }

    /// <inheritdoc/>
    public Model_SyncToolReadiness GetReadiness()
    {
        var checks = new List<string>();

        var exePath = BundledExePath;
        var hasExe = exePath is not null;
        checks.Add(
            hasExe
                ? $"✓ Bundled executable found ({exePath})"
                : "✗ Bundled executable (sync_reference_data.exe) not found next to the script path."
        );

        var toolDirectory =
            Path.GetDirectoryName(EffectiveScriptPath) ?? AppContext.BaseDirectory;
        var policyPath = Path.Combine(toolDirectory, "sync_policy.yml");
        var hasPolicy = File.Exists(policyPath);
        checks.Add(
            hasPolicy
                ? $"✓ Sync policy found ({policyPath})"
                : $"✗ Sync policy (sync_policy.yml) not found at '{policyPath}'."
        );

        // Backup/copy/restore are implemented in pure Python (pymysql), so no
        // external mysql/mysqldump CLI tools are required on the machine.

        return new Model_SyncToolReadiness
        {
            IsReady = hasExe && hasPolicy,
            Checks = checks,
        };
    }

    private (string Host, string Port, string User) ResolveMySqlEndpoint()
    {
        var connectionString = _configuration.GetConnectionString("MySql");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The MySql connection string is not configured, so the SyncTool has no server to target."
            );
        }

        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString);
            return (
                builder.Server,
                builder.Port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                builder.UserID
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "The configured MySql connection string could not be parsed for the SyncTool.",
                ex
            );
        }
    }
}
