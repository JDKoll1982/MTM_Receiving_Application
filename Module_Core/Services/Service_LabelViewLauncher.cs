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

        try
        {
            Process.Start(
                new ProcessStartInfo(executablePath!, $"\"{labelFilePath}\"")
                {
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(labelFilePath) ?? string.Empty,
                }
            );

            _logger.LogInfo(
                $"Opened label '{labelFilePath}' with LabelView '{executablePath}'.",
                nameof(Service_LabelViewLauncher)
            );

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to open label '{labelFilePath}' with LabelView '{executablePath}': {ex.Message}",
                ex,
                nameof(Service_LabelViewLauncher)
            );

            return Model_Dao_Result_Factory.Failure(
                $"Failed to open the label in LabelView: {ex.Message}",
                ex
            );
        }
    }
}
