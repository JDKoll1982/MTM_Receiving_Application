using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Validators;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service for Quality Hold detection and two-step acknowledgment workflow
/// User Requirements:
///   - Configurable part patterns (not hardcoded MMFSR/MMCSR)
///   - Two-step acknowledgment dialogs ("Acknowledgment 1 of 2", "Acknowledgment 2 of 2")
///   - Hard block on save without acknowledgment
/// </summary>
public class Service_Receiving_QualityHoldDetection : IService_Receiving_QualityHoldDetection
{
    private readonly Validator_Receiving_Shared_ValidateIf_RestrictedPart _restrictedPartValidator;
    private readonly IService_Window _windowService;
    private readonly IService_LoggingUtility _logger;

    public Service_Receiving_QualityHoldDetection(
        Validator_Receiving_Shared_ValidateIf_RestrictedPart restrictedPartValidator,
        IService_Window windowService,
        IService_LoggingUtility logger)
    {
        _restrictedPartValidator = restrictedPartValidator;
        _windowService = windowService;
        _logger = logger;
    }

    /// <summary>
    /// Detects if a part number matches restricted part patterns and shows first acknowledgment dialog
    /// Step 1 of 2: User acknowledgment on part selection
    /// </summary>
    public async Task<(bool IsRestricted, bool UserAcknowledged, string? MatchedPattern, string? RestrictionType)> DetectAndShowFirstWarningAsync(string partNumber)
    {
        try
        {
            _logger.LogInfo($"Checking quality hold for part: {partNumber}");

            // Check if part matches restricted patterns
            var (isRestricted, matchedPattern, restrictionType) = await _restrictedPartValidator.ValidateAsync(partNumber);

            if (!isRestricted)
            {
                _logger.LogInfo($"Part {partNumber} does not require quality hold");
                return (false, false, null, null);
            }

            _logger.LogWarning($"Part {partNumber} requires quality hold - Pattern: {matchedPattern}, Type: {restrictionType}");

            // Show first acknowledgment dialog (Step 1 of 2)
            var userAcknowledged = await ShowFirstAcknowledgmentDialogAsync(partNumber, matchedPattern!, restrictionType!);

            return (true, userAcknowledged, matchedPattern, restrictionType);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in DetectAndShowFirstWarningAsync: {ex.Message}", ex);
            return (false, false, null, null);
        }
    }

    /// <summary>
    /// Shows final acknowledgment dialog before save
    /// Step 2 of 2: Final acknowledgment required before save
    /// </summary>
    public async Task<bool> ShowFinalAcknowledgmentDialogAsync(string partNumber, int loadNumber, string restrictionType)
    {
        try
        {
            _logger.LogInfo($"Showing final acknowledgment for part {partNumber}, load {loadNumber}");

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog - XamlRoot is null");
                return false;
            }

            var dialog = new ContentDialog
            {
                Title = "?? Quality Hold - Final Acknowledgment (2 of 2)",
                Content = BuildFinalAcknowledgmentMessage(partNumber, loadNumber, restrictionType),
                PrimaryButtonText = "I Acknowledge",
                CloseButtonText = "Cancel Save",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();

            var acknowledged = result == ContentDialogResult.Primary;
            _logger.LogInfo($"Final acknowledgment result: {(acknowledged ? "Acknowledged" : "Cancelled")}");

            return acknowledged;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error showing final acknowledgment dialog: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Validates if a part requires quality hold (without showing dialogs)
    /// </summary>
    public async Task<bool> IsRestrictedPartAsync(string partNumber)
    {
        try
        {
            var (isRestricted, _, _) = await _restrictedPartValidator.ValidateAsync(partNumber);
            return isRestricted;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in IsRestrictedPartAsync: {ex.Message}", ex);
            return false;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Shows the first acknowledgment dialog (Step 1 of 2)
    /// </summary>
    private async Task<bool> ShowFirstAcknowledgmentDialogAsync(string partNumber, string matchedPattern, string restrictionType)
    {
        try
        {
            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogError("Cannot show dialog - XamlRoot is null");
                return false;
            }

            var dialog = new ContentDialog
            {
                Title = "?? Quality Hold Required - Acknowledgment (1 of 2)",
                Content = BuildFirstAcknowledgmentMessage(partNumber, matchedPattern, restrictionType),
                PrimaryButtonText = "I Acknowledge",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();

            var acknowledged = result == ContentDialogResult.Primary;
            _logger.LogInfo($"First acknowledgment result: {(acknowledged ? "Acknowledged" : "Cancelled")}");

            return acknowledged;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error showing first acknowledgment dialog: {ex.Message}", ex);
            return false;
        }
    }

    /// <summary>
    /// Builds the message for the first acknowledgment dialog
    /// </summary>
    private string BuildFirstAcknowledgmentMessage(string partNumber, string matchedPattern, string restrictionType)
    {
        return $"?? QUALITY HOLD REQUIRED\n\n" +
               $"Part Number: {partNumber}\n" +
               $"Pattern Matched: {matchedPattern}\n" +
               $"Restriction Type: {restrictionType}\n\n" +
               $"This part requires quality hold acknowledgment.\n\n" +
               $"This is STEP 1 OF 2.\n" +
               $"You will be required to acknowledge again before saving.\n\n" +
               $"Do you acknowledge this quality hold requirement?";
    }

    /// <summary>
    /// Builds the message for the final acknowledgment dialog
    /// </summary>
    private string BuildFinalAcknowledgmentMessage(string partNumber, int loadNumber, string restrictionType)
    {
        return $"?? FINAL QUALITY HOLD ACKNOWLEDGMENT\n\n" +
               $"Load Number: {loadNumber}\n" +
               $"Part Number: {partNumber}\n" +
               $"Restriction Type: {restrictionType}\n\n" +
               $"This is STEP 2 OF 2 - FINAL ACKNOWLEDGMENT.\n\n" +
               $"By clicking 'I Acknowledge', you confirm that:\n" +
               $"• You understand this part requires special quality handling\n" +
               $"• You have reviewed the quality hold requirements\n" +
               $"• You are authorized to proceed with receiving this part\n\n" +
               $"?? SAVE WILL BE BLOCKED if you do not acknowledge.\n\n" +
               $"Do you provide final acknowledgment?";
    }

    #endregion
}
