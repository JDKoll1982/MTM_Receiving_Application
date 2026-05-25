using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service for displaying quality hold warnings when restricted parts are entered.
/// Provides immediate user feedback for MMFSR and MMCSR parts that require quality inspection.
/// </summary>
public class Service_QualityHoldWarning : IService_QualityHoldWarning
{
    private readonly IService_AppSettings _appSettings;
    private readonly IService_InforVisualMockDataCatalog _mockDataCatalog;
    private readonly IService_Window _windowService;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ReceivingSettings _receivingSettings;

    public Service_QualityHoldWarning(
        IService_AppSettings appSettings,
        IService_InforVisualMockDataCatalog mockDataCatalog,
        IService_Window windowService,
        IService_LoggingUtility logger,
        IService_ReceivingSettings receivingSettings
    )
    {
        _appSettings = appSettings;
        _mockDataCatalog = mockDataCatalog;
        _windowService = windowService;
        _logger = logger;
        _receivingSettings = receivingSettings;
    }

    /// <inheritdoc/>
    public bool IsRestrictedPart(string? partID)
    {
        return string.IsNullOrWhiteSpace(ResolveRestrictionType(partID)) is false;
    }

    /// <inheritdoc/>
    public async Task<bool> CheckAndWarnAsync(string? partID, Model_ReceivingLoad? load = null)
    {
        var restrictionType = ResolveRestrictionType(partID);
        if (string.IsNullOrWhiteSpace(restrictionType))
        {
            if (load is not null)
            {
                load.IsQualityHoldRequired = false;
                load.IsQualityHoldAcknowledged = false;
                load.QualityHoldRestrictionType = string.Empty;
            }

            return true; // No warning needed, proceed
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show quality hold warning: XamlRoot is null");
            return true; // Don't block if we can't show dialog
        }

        // Update load if provided
        if (load != null)
        {
            load.IsQualityHoldRequired = true;
            load.IsQualityHoldAcknowledged = false;
            load.QualityHoldRestrictionType = restrictionType;
        }

        // Build warning message
        var message =
            $"⚠️ QUALITY HOLD REQUIRED ⚠️\n\n"
            + $"ACKNOWLEDGMENT 1 of 2\n\n"
            + $"Part ID: {partID}\n"
            + $"Type: {restrictionType}\n\n"
            + $"IMMEDIATE ACTION REQUIRED:\n"
            + $"• Contact Quality NOW\n"
            + $"• Quality MUST inspect and accept this load\n"
            + $"• DO NOT sign any paperwork until Quality accepts\n\n"
            + $"You will be asked to confirm again before saving.\n"
            + $"This is a critical quality control checkpoint.";

        var dialog = new ContentDialog
        {
            Title = "⚠️ QUALITY HOLD - Acknowledgment 1 of 2",
            Content = new TextBlock
            {
                Text = message,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                FontSize = 14,
            },
            PrimaryButtonText = "I Understand - Will Contact Quality",
            CloseButtonText = "Cancel Entry",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        var result = await dialog.ShowAsync();

        bool acknowledged = result == ContentDialogResult.Primary;

        // DO NOT set IsQualityHoldAcknowledged here - this is just the first warning
        // The user must acknowledge again at save time for dual confirmation
        // Only mark as "warned" but not "acknowledged" until final save confirmation

        _logger.LogInfo(
            $"Quality hold warning for part {partID}: {(acknowledged ? "Acknowledged" : "Cancelled")}"
        );

        return acknowledged;
    }

    /// <inheritdoc/>
    public async Task<bool> ConfirmBeforeSaveAsync(
        IReadOnlyList<Model_ReceivingLoad> loadsWithHolds
    )
    {
        ArgumentNullException.ThrowIfNull(loadsWithHolds);

        if (loadsWithHolds.Count == 0)
        {
            return true;
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot == null)
        {
            _logger.LogError("Cannot show quality hold save confirmation: XamlRoot is null");
            return false;
        }

        var restrictedPartsList = string.Join(
            "\n",
            loadsWithHolds.Select(load => $"  • {load.PartID} ({load.QualityHoldRestrictionType})")
        );

        var content =
            $"⚠️ FINAL QUALITY HOLD CONFIRMATION ⚠️\n\n"
            + $"This is your SECOND and FINAL acknowledgment.\n\n"
            + $"The following parts require quality hold:\n\n{restrictedPartsList}\n\n"
            + $"BEFORE YOU PROCEED:\n"
            + $"✓ Have you contacted Quality?\n"
            + $"✓ Has Quality physically inspected these loads?\n"
            + $"✓ Has Quality accepted these loads?\n"
            + $"This is a critical quality control checkpoint.\n"
            + $"DO NOT proceed unless Quality has accepted.";

        var dialog = new ContentDialog
        {
            Title = "⚠️ FINAL QUALITY HOLD CONFIRMATION - Action Required",
            Content = new TextBlock
            {
                Text = content,
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Microsoft.UI.Colors.DarkRed
                ),
            },
            PrimaryButtonText = "✓ YES - Save Now",
            CloseButtonText = "✗ NO - Cancel Save",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private string ResolveRestrictionType(string? partID)
    {
        if (string.IsNullOrWhiteSpace(partID))
        {
            return string.Empty;
        }

        var normalizedPartId = partID.Trim();

        if (_appSettings.GetUseInforVisualMockData())
        {
            var catalog = _mockDataCatalog.GetCatalog();
            var mockPart = catalog.Parts.FirstOrDefault(part =>
                string.Equals(part.PartID, normalizedPartId, StringComparison.OrdinalIgnoreCase)
            );

            mockPart ??= catalog
                .PurchaseOrders.SelectMany(purchaseOrder => purchaseOrder.Parts)
                .FirstOrDefault(part =>
                    string.Equals(part.PartID, normalizedPartId, StringComparison.OrdinalIgnoreCase)
                );

            if (mockPart is not null)
            {
                return mockPart.RequiresQualityHold
                    ? string.IsNullOrWhiteSpace(mockPart.QualityHoldRestrictionType)
                        ? "Quality Hold Required"
                        : mockPart.QualityHoldRestrictionType.Trim()
                    : string.Empty;
            }
        }

        if (normalizedPartId.Contains("MMFSR", StringComparison.OrdinalIgnoreCase))
        {
            return "Sheet Material - Quality Hold Required";
        }

        if (normalizedPartId.Contains("MMCSR", StringComparison.OrdinalIgnoreCase))
        {
            return "Coil Material - Quality Hold Required";
        }

        return string.Empty;
    }
}
