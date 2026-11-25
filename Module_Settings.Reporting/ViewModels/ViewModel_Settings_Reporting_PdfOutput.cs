using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

/// <summary>
/// Settings page for the folder the Reporting preview saves its PDF into, and for the
/// Save PDF behavior that follows it.
/// </summary>
public partial class ViewModel_Settings_Reporting_PdfOutput : ViewModel_Shared_Base
{
    private readonly IService_ReportingSettings _reportingSettings;

    [ObservableProperty]
    private string _pdfSaveFolder = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    public ViewModel_Settings_Reporting_PdfOutput(
        IService_ReportingSettings reportingSettings,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _reportingSettings =
            reportingSettings ?? throw new ArgumentNullException(nameof(reportingSettings));
        Title = "Report PDF Output";

        _ = LoadSettingsAsync();
    }

    /// <summary>The folder new reports land in when nothing else has been saved.</summary>
    public string DefaultFolder => ReportingSettingsKeys.DefaultPdfSaveFolder;

    /// <summary>Restores the shipped default instead of the last saved folder.</summary>
    [RelayCommand]
    private void UseDefaultFolder()
    {
        PdfSaveFolder = DefaultFolder;
        StatusMessage = "Default folder restored. Click Save to keep it.";
    }

    /// <summary>Opens File Explorer at the folder currently typed in the box.</summary>
    [RelayCommand]
    private void OpenFolder()
    {
        var folder = PdfSaveFolder?.Trim() ?? string.Empty;
        if (folder.Length == 0)
        {
            StatusMessage = "Enter a folder path first.";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
            StatusMessage = Directory.Exists(folder)
                ? "Opened the report folder."
                : "Explorer opened, but that folder could not be reached from this PC.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to open the report folder '{folder}': {ex.Message}", ex);
            StatusMessage = $"Could not open the folder: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var folder = PdfSaveFolder?.Trim() ?? string.Empty;
        if (folder.Length == 0)
        {
            StatusMessage = "A report output folder is required.";
            return;
        }

        try
        {
            IsBusy = true;

            var saveResult = await _reportingSettings.SavePdfSaveFolderAsync(folder);
            if (!saveResult.IsSuccess)
            {
                StatusMessage = saveResult.ErrorMessage ?? "Failed to save the report folder.";
                return;
            }

            PdfSaveFolder = folder;
            StatusMessage = Directory.Exists(folder)
                ? "Report folder saved."
                : "Report folder saved. It is not reachable from this PC right now; Save PDF will report that when used.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_Reporting_PdfOutput)
            );
            StatusMessage = "Failed to save the report folder.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;
            PdfSaveFolder = await _reportingSettings.GetPdfSaveFolderAsync();
            StatusMessage = "Report folder loaded.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_Reporting_PdfOutput)
            );
            PdfSaveFolder = DefaultFolder;
            StatusMessage = "Failed to load the report folder. The default is shown.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
