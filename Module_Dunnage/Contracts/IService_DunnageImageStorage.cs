using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Handles import and cleanup of Dunnage image assets stored under app-managed local storage.
/// </summary>
public interface IService_DunnageImageStorage
{
    Task RefreshConfiguredRootFolderAsync();

    Task SyncLocalCacheAsync();

    Task<string?> GetConfiguredRootFolderAsync();

    Task<Model_Dao_Result<string>> ImportImageAsync(string sourceFilePath, string folderName);

    Task<Model_Dao_Result<string>> ImportTypeImageAsync(string sourceFilePath, string typeName);

    Task<Model_Dao_Result<string>> ImportPartImageAsync(
        string sourceFilePath,
        string typeName,
        string partId
    );

    Task<Model_Dao_Result<string>> CreateRotatedWorkingCopyAsync(string imagePath);

    /// <summary>
    /// Reserves a unique path inside the local temp working folder, keeping the supported
    /// extension of the supplied image. Callers write edited image content to this path so the
    /// original photo is never modified.
    /// </summary>
    string CreateTempWorkingCopyPath(string? sourceImagePath);

    Task<Model_Dao_Result> DeleteImageAsync(string? relativeImagePath);

    string? GetAbsolutePath(string? relativeImagePath);

    string? GetNormalizedFullPath(string? imagePath);
}
