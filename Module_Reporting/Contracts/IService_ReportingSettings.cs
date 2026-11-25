using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Module_Reporting.Contracts;

public interface IService_ReportingSettings
{
    Task<Model_ReportingPreviewSettings> GetPreviewSettingsAsync(int? userId = null);

    Task<Model_Dao_Result> SavePreviewSettingsAsync(
        Model_ReportingPreviewSettings settings,
        int? userId = null
    );

    /// <summary>
    /// Returns the folder the preview's Save PDF action writes into. Falls back to
    /// <see cref="Settings.ReportingSettingsKeys.DefaultPdfSaveFolder"/> when nothing is stored,
    /// so the report always has a destination.
    /// </summary>
    Task<string> GetPdfSaveFolderAsync(int? userId = null);

    /// <summary>Saves the folder the Save PDF action writes into.</summary>
    /// <param name="folderPath">Folder path, local or UNC.</param>
    /// <param name="userId">Optional user ID for a user-scoped override.</param>
    Task<Model_Dao_Result> SavePdfSaveFolderAsync(string folderPath, int? userId = null);
}
