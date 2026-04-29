using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Settings;

namespace MTM_Receiving_Application.Module_Core.Services;

public class Service_LabelViewLauncher : IService_LabelViewLauncher
{
    private static readonly string DocumentsFolder = Environment.GetFolderPath(
        Environment.SpecialFolder.MyDocuments
    );

    private readonly IService_LoggingUtility _logger;
    private readonly IService_SettingsCoreFacade _settingsCore;

    public Service_LabelViewLauncher(
        IService_LoggingUtility logger,
        IService_SettingsCoreFacade settingsCore
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
    }

    public string DefaultExecutablePath => CoreSettingsKeys.LabelView.DefaultExecutablePath;

    public bool IsExecutablePathValid(string executablePath)
    {
        return string.IsNullOrWhiteSpace(executablePath) is false
            && File.Exists(executablePath)
            && string.Equals(
                Path.GetFileName(executablePath),
                CoreSettingsKeys.LabelView.ExecutableFileName,
                StringComparison.OrdinalIgnoreCase
            );
    }

    public bool IsLabelFilePathValid(string labelFilePath)
    {
        return string.IsNullOrWhiteSpace(labelFilePath) is false
            && File.Exists(labelFilePath)
            && string.Equals(
                Path.GetExtension(labelFilePath),
                ".lbl",
                StringComparison.OrdinalIgnoreCase
            );
    }

    public async Task<string?> ResolveExecutablePathAsync()
    {
        var savedResult = await _settingsCore.GetSettingAsync(
            CoreSettingsKeys.SystemCategory,
            CoreSettingsKeys.LabelView.ExecutablePath
        );

        var savedPath = savedResult.IsSuccess ? savedResult.Data?.Value : null;
        if (IsExecutablePathValid(savedPath ?? string.Empty))
        {
            return savedPath;
        }

        return IsExecutablePathValid(DefaultExecutablePath) ? DefaultExecutablePath : null;
    }

    public async Task<Model_Dao_Result> LaunchLabelAsync(string labelFilePath)
    {
        if (!IsLabelFilePathValid(labelFilePath))
        {
            return Model_Dao_Result_Factory.Failure(
                "The selected label file path is missing, inaccessible, or does not point to a .lbl file."
            );
        }

        var executablePath = await ResolveExecutablePathAsync();
        if (!IsExecutablePathValid(executablePath ?? string.Empty))
        {
            return Model_Dao_Result_Factory.Failure(
                "LabelView executable could not be resolved from the saved path or the default installation path."
            );
        }

        var workingDirectory = ResolveFolderToOpen(labelFilePath);

        try
        {
            Process.Start(
                new ProcessStartInfo(labelFilePath)
                {
                    UseShellExecute = true,
                    Verb = "open",
                    WorkingDirectory = workingDirectory,
                }
            );

            _logger.LogInfo(
                $"Opened label '{labelFilePath}' through the Windows shell after validating LabelView '{executablePath}'.",
                nameof(Service_LabelViewLauncher)
            );

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to open label '{labelFilePath}' through the Windows shell after validating LabelView '{executablePath}': {ex.Message}",
                ex,
                nameof(Service_LabelViewLauncher)
            );

            return Model_Dao_Result_Factory.Failure(
                $"Failed to open the label in LabelView: {ex.Message}",
                ex
            );
        }
    }

    public Task<Model_Dao_Result> OpenFolderForPathAsync(string? configuredPath)
    {
        var explorerArguments = ResolveExplorerArguments(configuredPath, out var workingDirectory);

        try
        {
            Process.Start(
                new ProcessStartInfo("explorer.exe", explorerArguments)
                {
                    UseShellExecute = true,
                    WorkingDirectory = workingDirectory,
                }
            );

            _logger.LogInfo(
                $"Opened Explorer with arguments '{explorerArguments}' for configured path '{configuredPath ?? string.Empty}'.",
                nameof(Service_LabelViewLauncher)
            );

            return Task.FromResult(Model_Dao_Result_Factory.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to open Explorer with arguments '{explorerArguments}' for configured path '{configuredPath ?? string.Empty}': {ex.Message}",
                ex,
                nameof(Service_LabelViewLauncher)
            );

            return Task.FromResult(
                Model_Dao_Result_Factory.Failure($"Failed to open File Explorer: {ex.Message}", ex)
            );
        }
    }

    private static string ResolveExplorerArguments(
        string? configuredPath,
        out string workingDirectory
    )
    {
        var sanitizedPath = configuredPath?.Trim().Trim('"') ?? string.Empty;

        if (File.Exists(sanitizedPath))
        {
            workingDirectory = Path.GetDirectoryName(sanitizedPath) ?? DocumentsFolder;
            return $"/select,\"{sanitizedPath}\"";
        }

        workingDirectory = ResolveFolderToOpen(configuredPath);
        return $"\"{workingDirectory}\"";
    }

    private static string ResolveFolderToOpen(string? configuredPath)
    {
        var sanitizedPath = configuredPath?.Trim().Trim('"') ?? string.Empty;
        if (string.IsNullOrWhiteSpace(sanitizedPath))
        {
            return DocumentsFolder;
        }

        if (Directory.Exists(sanitizedPath))
        {
            return sanitizedPath;
        }

        if (File.Exists(sanitizedPath))
        {
            return Path.GetDirectoryName(sanitizedPath) ?? DocumentsFolder;
        }

        var parentDirectory = Path.GetDirectoryName(sanitizedPath);
        if (
            string.IsNullOrWhiteSpace(parentDirectory) is false
            && Directory.Exists(parentDirectory)
        )
        {
            return parentDirectory;
        }

        return DocumentsFolder;
    }
}
