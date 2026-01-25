using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using System;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service for displaying quality hold warnings when restricted parts are entered.
/// Provides immediate user feedback for MMFSR and MMCSR parts that require quality inspection.
/// </summary>
public class Service_QualityHoldWarning : IService_Receiving_Infrastructure_QualityHoldWarning
{
    private readonly IService_Window _windowService;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_Receiving_Infrastructure_Settings _receivingSettings;

    public Service_QualityHoldWarning(
        IService_Window windowService,
        IService_LoggingUtility logger,
        IService_Receiving_Infrastructure_Settings receivingSettings)
    {
        _windowService = windowService;
        _logger = logger;
        _receivingSettings = receivingSettings;
    }

    /// <inheritdoc/>
    public bool IsRestrictedPart(string? partID)
    {
        if (string.IsNullOrWhiteSpace(partID))
        {
            return false;
        }

        // Check for restricted part patterns: MMFSR or MMCSR
        return partID.Contains("MMFSR", StringComparison.OrdinalIgnoreCase) ||
               partID.Contains("MMCSR", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<bool> CheckAndWarnAsync(string? partID, Model_ReceivingLoad? load = null)
    {
        if (!IsRestrictedPart(partID))
        {
            return true; // No warning needed, proceed
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show quality hold warning: XamlRoot is null");
            return true; // Don't block if we can't show dialog
        }

        // Determine restriction type
        string restrictionType = partID!.Contains("MMFSR", StringComparison.OrdinalIgnoreCase)
            ? "Sheet Material - Quality Hold Required"
            : "Coil Material - Quality Hold Required";

        // Update load if provided
        if (load != null)
        {
            load.IsQualityHoldRequired = true;
            load.QualityHoldRestrictionType = restrictionType;
        }

        // Build warning message
        var message = $"⚠️ QUALITY HOLD REQUIRED ⚠️\n\n" +
                     $"ACKNOWLEDGMENT 1 of 2\n\n" +
                     $"Part ID: {partID}\n" +
                     $"Type: {restrictionType}\n\n" +
                     $"IMMEDIATE ACTION REQUIRED:\n" +
                     $"• Contact Quality NOW\n" +
                     $"• Quality MUST inspect and accept this load\n" +
                     $"• DO NOT sign any paperwork until Quality accepts\n\n" +
                     $"You will be asked to confirm again before saving.\n" +
                     $"This is a critical quality control checkpoint.";

        var dialog = new ContentDialog
        {
            Title = "⚠️ QUALITY HOLD - Acknowledgment 1 of 2",
            Content = new TextBlock
            {
                Text = message,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                FontSize = 14
            },
            PrimaryButtonText = "I Understand - Will Contact Quality",
            CloseButtonText = "Cancel Entry",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        
        bool acknowledged = result == ContentDialogResult.Primary;
        
        // DO NOT set IsQualityHoldAcknowledged here - this is just the first warning
        // The user must acknowledge again at save time for dual confirmation
        // Only mark as "warned" but not "acknowledged" until final save confirmation
        
        _logger.LogInfo($"Quality hold warning for part {partID}: {(acknowledged ? "Acknowledged" : "Cancelled")}");
        
        return acknowledged;
    }
}
