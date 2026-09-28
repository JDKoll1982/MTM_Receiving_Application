using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Shared.Views;

namespace MTM_Receiving_Application.Module_Core.Services.Help;

/// <summary>
/// Service for providing contextual help content throughout the application
/// </summary>
public class Service_Help : IService_Help
{
    private readonly IService_Window _windowService;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_Dispatcher _dispatcher;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Model_HelpContent> _helpContentCache;
    private readonly HashSet<string> _dismissedTips;

    public Service_Help(
        IService_Window windowService,
        IService_LoggingUtility logger,
        IService_Dispatcher dispatcher,
        IServiceProvider serviceProvider
    )
    {
        _windowService = windowService;
        _logger = logger;
        _dispatcher = dispatcher;
        _serviceProvider = serviceProvider;
        _helpContentCache = new Dictionary<string, Model_HelpContent>();
        _dismissedTips = new HashSet<string>();

        InitializeHelpContent();
    }

    #region Show Help Methods

    public async Task ShowHelpAsync(
        string helpKey,
        Microsoft.UI.Xaml.XamlRoot? xamlRoot = null
    )
    {
        try
        {
            var content = GetHelpContent(helpKey);
            if (content == null)
            {
                await _logger.LogWarningAsync($"Help content not found for key: {helpKey}");
                return;
            }

            await ShowHelpAsync(content, xamlRoot);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error showing help for key {helpKey}: {ex.Message}");
        }
    }

    public async Task ShowHelpAsync(
        Model_HelpContent content,
        Microsoft.UI.Xaml.XamlRoot? xamlRoot = null
    )
    {
        try
        {
            if (content == null)
            {
                return;
            }

            // Must execute on UI thread since we're showing a dialog
            var tcs = new TaskCompletionSource<bool>();

            _dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    var dialog = (View_Shared_HelpDialog?)
                        _serviceProvider.GetService(typeof(View_Shared_HelpDialog));
                    if (dialog == null)
                    {
                        await _logger.LogErrorAsync(
                            "Failed to retrieve HelpDialog from DI container"
                        );
                        tcs.SetResult(false);
                        return;
                    }

                    dialog.SetHelpContent(content);
                    dialog.XamlRoot = xamlRoot ?? _windowService.GetXamlRoot();

                    if (dialog.XamlRoot == null)
                    {
                        await _logger.LogErrorAsync("XamlRoot is null, cannot show help dialog");
                        tcs.SetResult(false);
                        return;
                    }

                    await dialog.ShowAsync();
                    await _logger.LogInfoAsync($"Displayed help for: {content.Key}");
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync($"Error in UI thread: {ex.Message}");
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error showing help for {content?.Key}: {ex.Message}");
        }
    }

    public async Task ShowContextualHelpAsync(Enum_DunnageWorkflowStep step)
    {
        var key = step switch
        {
            Enum_DunnageWorkflowStep.ModeSelection => "Dunnage.ModeSelection",
            Enum_DunnageWorkflowStep.ImagePartSearch => "Dunnage.PartSelection",
            Enum_DunnageWorkflowStep.TypeSelection => "Dunnage.TypeSelection",
            Enum_DunnageWorkflowStep.PartSelection => "Dunnage.PartSelection",
            Enum_DunnageWorkflowStep.QuantityEntry => "Dunnage.QuantityEntry",
            Enum_DunnageWorkflowStep.DetailsEntry => "Dunnage.DetailsEntry",
            Enum_DunnageWorkflowStep.Review => "Dunnage.Review",
            Enum_DunnageWorkflowStep.ManualEntry => "Dunnage.ManualEntry",
            Enum_DunnageWorkflowStep.EditMode => "Dunnage.EditMode",
            _ => "Dunnage.ModeSelection",
        };

        await ShowHelpAsync(key);
    }

    public async Task ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step)
    {
        var key = step switch
        {
            Enum_ReceivingWorkflowStep.ModeSelection => "Receiving.ModeSelection",
            Enum_ReceivingWorkflowStep.POEntry => "Receiving.POEntry",
            Enum_ReceivingWorkflowStep.PartSelection => "Receiving.PartSelection",
            Enum_ReceivingWorkflowStep.LoadEntry => "Receiving.LoadEntry",
            Enum_ReceivingWorkflowStep.WeightQuantityEntry => "Receiving.WeightQuantity",
            Enum_ReceivingWorkflowStep.HeatLotEntry => "Receiving.HeatLot",
            Enum_ReceivingWorkflowStep.PackageTypeEntry => "Receiving.PackageType",
            Enum_ReceivingWorkflowStep.Review => "Receiving.Review",
            Enum_ReceivingWorkflowStep.ManualEntry => "Receiving.ManualEntry",
            Enum_ReceivingWorkflowStep.EditMode => "Receiving.EditMode",
            Enum_ReceivingWorkflowStep.Saving => "Receiving.Saving",
            Enum_ReceivingWorkflowStep.Complete => "Receiving.Complete",
            Enum_ReceivingWorkflowStep.ReconciliationReview => "Receiving.ReconciliationReview",
            _ => "Receiving.ModeSelection",
        };

        await ShowHelpAsync(key);
    }

    #endregion

    #region Content Retrieval Methods

    public Model_HelpContent? GetHelpContent(string key)
    {
        return _helpContentCache.TryGetValue(key, out var content) ? content : null;
    }

    public List<Model_HelpContent> GetHelpByCategory(string category)
    {
        return _helpContentCache
            .Values.Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Title)
            .ToList();
    }

    public List<Model_HelpContent> SearchHelp(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<Model_HelpContent>();
        }

        var term = searchTerm.ToLower();
        return _helpContentCache
            .Values.Where(c =>
                c.Title.ToLower().Contains(term) || c.Content.ToLower().Contains(term)
            )
            .OrderByDescending(c => c.Title.ToLower().Contains(term))
            .ThenBy(c => c.Title)
            .Take(20)
            .ToList();
    }

    public async Task<bool> IsDismissedAsync(string helpKey)
    {
        await Task.CompletedTask;
        return _dismissedTips.Contains(helpKey);
    }

    public async Task SetDismissedAsync(string helpKey, bool isDismissed)
    {
        await Task.CompletedTask;
        if (isDismissed)
        {
            _dismissedTips.Add(helpKey);
        }
        else
        {
            _dismissedTips.Remove(helpKey);
        }
    }

    #endregion

    #region Legacy Methods (for backwards compatibility)

    public string GetDunnageWorkflowHelp(Enum_DunnageWorkflowStep step)
    {
        var key = step switch
        {
            Enum_DunnageWorkflowStep.ModeSelection => "Dunnage.ModeSelection",
            Enum_DunnageWorkflowStep.ImagePartSearch => "Dunnage.PartSelection",
            Enum_DunnageWorkflowStep.TypeSelection => "Dunnage.TypeSelection",
            Enum_DunnageWorkflowStep.PartSelection => "Dunnage.PartSelection",
            Enum_DunnageWorkflowStep.QuantityEntry => "Dunnage.QuantityEntry",
            Enum_DunnageWorkflowStep.DetailsEntry => "Dunnage.DetailsEntry",
            Enum_DunnageWorkflowStep.Review => "Dunnage.Review",
            Enum_DunnageWorkflowStep.ManualEntry => "Dunnage.ManualEntry",
            Enum_DunnageWorkflowStep.EditMode => "Dunnage.EditMode",
            _ => "",
        };

        var content = GetHelpContent(key);
        return content?.Content ?? string.Empty;
    }

    public string GetReceivingWorkflowHelp(Enum_ReceivingWorkflowStep step)
    {
        var key = step switch
        {
            Enum_ReceivingWorkflowStep.ModeSelection => "Receiving.ModeSelection",
            Enum_ReceivingWorkflowStep.ManualEntry => "Receiving.ManualEntry",
            Enum_ReceivingWorkflowStep.EditMode => "Receiving.EditMode",
            Enum_ReceivingWorkflowStep.POEntry => "Receiving.POEntry",
            Enum_ReceivingWorkflowStep.PartSelection => "Receiving.PartSelection",
            Enum_ReceivingWorkflowStep.LoadEntry => "Receiving.LoadEntry",
            Enum_ReceivingWorkflowStep.WeightQuantityEntry => "Receiving.WeightQuantity",
            Enum_ReceivingWorkflowStep.HeatLotEntry => "Receiving.HeatLot",
            Enum_ReceivingWorkflowStep.PackageTypeEntry => "Receiving.PackageType",
            Enum_ReceivingWorkflowStep.Review => "Receiving.Review",
            Enum_ReceivingWorkflowStep.Saving => "Receiving.Saving",
            Enum_ReceivingWorkflowStep.Complete => "Receiving.Complete",
            Enum_ReceivingWorkflowStep.ReconciliationReview => "Receiving.ReconciliationReview",
            _ => "",
        };

        var content = GetHelpContent(key);
        return content?.Content ?? string.Empty;
    }

    public string GetTip(string viewName)
    {
        var content = GetHelpContent($"Tip.{viewName}");
        return content?.Content ?? string.Empty;
    }

    public string GetPlaceholder(string fieldName)
    {
        var content = GetHelpContent($"Placeholder.{fieldName}");
        return content?.Content ?? string.Empty;
    }

    public string GetTooltip(string elementName)
    {
        var content = GetHelpContent($"Tooltip.{elementName}");
        return content?.Content ?? string.Empty;
    }

    public string GetInfoBarMessage(string messageKey)
    {
        var content = GetHelpContent($"InfoBar.{messageKey}");
        return content?.Content ?? string.Empty;
    }

    #endregion

    #region Content Initialization

    private void InitializeHelpContent()
    {
        // Dunnage Workflow Help Content
        InitializeDunnageHelp();

        // Receiving Workflow Help Content
        InitializeReceivingHelp();

        // Module page help content
        InitializeModuleHelp();

        // Dialog help content
        InitializeDialogHelp();

        // Admin Help Content
        InitializeAdminHelp();

        // Tooltips
        InitializeTooltips();

        // Placeholders
        InitializePlaceholders();

        // Tips
        InitializeTips();

        // InfoBar Messages
        InitializeInfoBarMessages();
    }

    private void InitializeDunnageHelp()
    {
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.ModeSelection",
                Title = "Select Entry Mode",
                Content =
                    "Choose how you want to enter dunnage receiving data:\n\n"
                    + "• Guided Wizard - Step-by-step process for single transactions. Best for occasional entries or when you need guidance.\n\n"
                    + "• Manual Entry - Bulk grid entry for multiple transactions. Ideal for processing many items at once.\n\n"
                    + "• Edit Mode - Review and edit historical data. Use to correct past entries or view receiving history.",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "ViewDashboard",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>
                {
                    "Dunnage.TypeSelection",
                    "Dunnage.ManualEntry",
                    "Dunnage.EditMode",
                },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.TypeSelection",
                Title = "Select Dunnage Type",
                Content =
                    "Choose the type of dunnage material you are receiving:\n\n"
                    + "• Each type has specific specifications and requirements\n"
                    + "• Use the search function to quickly find types\n"
                    + "• Navigate with pagination controls or quick access buttons\n\n"
                    + "If you don't see the type you need, contact your administrator to add new types through the Admin section.",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "PackageVariant",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Dunnage.PartSelection", "Admin.Types" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.PartSelection",
                Title = "Select Part/Specification",
                Content =
                    "Select the specific part or specification for the dunnage type:\n\n"
                    + "• Parts are organized by dunnage type\n"
                    + "• Use filters to find specific parts quickly\n"
                    + "• Parts marked with inventory status are tracked in the inventory system\n\n"
                    + "**Inventory Status:**\n"
                    + "• Green badge - Part is in inventory management system\n"
                    + "• No badge - Part is not tracked in inventory\n\n"
                    + "If you don't see the part you need, use the Quick Add button to create it.",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "ShapeOutline",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Dunnage.QuantityEntry", "Admin.Parts" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.QuantityEntry",
                Title = "Enter Quantity",
                Content =
                    "Specify the quantity of dunnage items received:\n\n"
                    + "• Enter whole numbers only\n"
                    + "• The system will generate individual labels for each item\n"
                    + "• You can adjust the quantity later if needed\n\n"
                    + "**Label Generation:**\n"
                    + "Each item will receive a unique label with sequential numbering (e.g., 1 of 10, 2 of 10, etc.)",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "Numeric",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Dunnage.DetailsEntry", "Dunnage.Review" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.DetailsEntry",
                Title = "Enter Additional Details",
                Content =
                    "Provide additional information for the dunnage receiving:\n\n"
                    + "**PO Number (Optional):**\n"
                    + "• Enter the purchase order number if applicable\n"
                    + "• Format: Any alphanumeric format accepted\n\n"
                    + "**Location (Required):**\n"
                    + "• Specify where the dunnage is stored\n"
                    + "• Use standard warehouse location codes\n"
                    + "• Example: A-12-B, DOCK-2, etc.\n\n"
                    + "**Specifications:**\n"
                    + "• Additional spec fields may appear based on the dunnage type\n"
                    + "• Required fields are marked with an asterisk (*)\n"
                    + "• Values must be within specified min/max ranges",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "TextBox",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Dunnage.Review" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.Review",
                Title = "Review & Save",
                Content =
                    "Review your entries before saving:\n\n"
                    + "**Single View:**\n"
                    + "• Review details in card format\n"
                    + "• Navigate through entries individually\n\n"
                    + "**Table View:**\n"
                    + "• See all entries in a grid\n"
                    + "• Quickly spot any errors or inconsistencies\n\n"
                    + "**Actions:**\n"
                    + "• Remove - Delete an entry from the session\n"
                    + "• Edit - Return to previous steps to modify\n"
                    + "• Save - Commit all entries to database and generate labels\n\n"
                    + "After saving, labels will be generated automatically and ready for printing.",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "CheckCircle",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Dunnage.ModeSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.ManualEntry",
                Title = "Manual Entry Mode",
                Content =
                    "Bulk entry mode for processing multiple dunnage items:\n\n"
                    + "**Features:**\n"
                    + "• Add multiple rows at once (up to 100)\n"
                    + "• Auto-fill from last entry for the same part\n"
                    + "• Fill blank spaces with data from previous row\n"
                    + "• Sort entries for optimal label printing\n\n"
                    + "**Tips:**\n"
                    + "• Use Tab key to move between cells quickly\n"
                    + "• Copy/paste values between cells\n"
                    + "• Use bulk operations to speed up data entry\n"
                    + "• Sort by Part ID before saving for sequential label numbers",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "Table",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Dunnage.ModeSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.EditMode",
                Title = "Edit Historical Data",
                Content =
                    "Review and edit past dunnage receiving entries:\n\n"
                    + "**Data Sources:**\n"
                    + "• Session Memory - Unsaved work from current session\n"
                    + "• Recent Labels - From most recent saved label dataset\n"
                    + "• Historical Loads - From database by date range\n\n"
                    + "**Date Filtering:**\n"
                    + "• Quick filters: Today, Last 7 days, This week/month/quarter/year\n"
                    + "• Custom date range with start and end dates\n\n"
                    + "**Editing:**\n"
                    + "• Select entries to modify\n"
                    + "• Make changes directly in the grid\n"
                    + "• Save updates back to database\n\n"
                    + " Changes to historical data may affect inventory counts and reports.",
                Category = "Dunnage Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "PencilBox",
                Severity = Enum_HelpSeverity.Warning,
                RelatedKeys = new List<string> { "Dunnage.ModeSelection" },
            }
        );
    }

    private void InitializeReceivingHelp()
    {
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.ModeSelection",
                Title = "Select Entry Mode",
                Content =
                    "Choose how you want to enter material receiving data:\n\n"
                    + "• Guided Wizard - Step-by-step process with PO integration\n"
                    + "• Manual Entry - Bulk grid entry for multiple line items\n"
                    + "• Edit Mode - Review and modify historical receiving data",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "ViewDashboard",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.POEntry", "Receiving.ManualEntry" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.POEntry",
                Title = "Enter Purchase Order",
                Content =
                    "Enter the PO number to receive material:\n\n"
                    + "**PO Number Format:**\n"
                    + "• With prefix: PO-066868\n"
                    + "• Without prefix: 66868\n"
                    + "• System accepts both formats\n\n"
                    + "**Infor Visual Integration:**\n"
                    + "The system connects to the Infor Visual ERP system (VISUAL database) to:\n"
                    + "• Validate PO exists and is open\n"
                    + "• Retrieve available line items\n"
                    + "• Populate part IDs and descriptions\n"
                    + "• Check ordered quantities\n\n"
                    + "**Part ID Search:**\n"
                    + "Alternatively, search by Part ID (e.g., MMC-001, MMF-456) to find associated POs.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "FileDocument",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.PartSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.WeightQuantity",
                Title = "Weight vs. Quantity",
                Content =
                    "Choose whether to enter by weight or piece count:\n\n"
                    + "**Use Weight When:**\n"
                    + "• Material is sold/received by weight\n"
                    + "• Dealing with raw materials, bars, sheets\n"
                    + "• Unit of measure is LBS, KG, etc.\n\n"
                    + "**Use Quantity When:**\n"
                    + "• Material is counted by pieces\n"
                    + "• Dealing with discrete items\n"
                    + "• Unit of measure is EA (each), PCS (pieces)\n\n"
                    + "**Important:**\n"
                    + "The selection should match the PO line item's unit of measure from Infor Visual.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "Scale",
                Severity = Enum_HelpSeverity.Warning,
                RelatedKeys = new List<string> { "Receiving.HeatLot" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.HeatLot",
                Title = "Heat/Lot Numbers",
                Content =
                    "Enter heat or lot number for material traceability:\n\n"
                    + "**What is a Heat/Lot Number?**\n"
                    + "• Unique identifier from the manufacturer\n"
                    + "• Tracks material batch or production run\n"
                    + "• Required for quality control and traceability\n\n"
                    + "**Format:**\n"
                    + "• Alphanumeric, varies by supplier\n"
                    + "• Example: H123456, LOT-2024-001\n\n"
                    + "**When Required:**\n"
                    + "• Metals and alloys\n"
                    + "• Materials with certification requirements\n"
                    + "• Quality-critical components\n\n"
                    + "Leave blank if not applicable to the material.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "Barcode",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.PackageType" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.Review",
                Title = "Review & Generate Labels",
                Content =
                    "Final review before saving and printing labels:\n\n"
                    + "**Review Process:**\n"
                    + "• Verify all details are correct\n"
                    + "• Check quantities and weights\n"
                    + "• Confirm heat/lot numbers\n\n"
                    + "**Saving:**\n"
                    + "• Data is saved to MySQL database\n"
                    + "• CSV file is exported for records\n"
                    + "• Labels are generated automatically\n\n"
                    + "**Label Printing:**\n"
                    + "• Labels display all receiving information\n"
                    + "• Include barcodes for scanning\n"
                    + "• Print directly from the review screen\n\n"
                    + "After saving, you can reprint labels from the Edit Mode if needed.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "CheckCircle",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.ModeSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.PartSelection",
                Title = "Select Part",
                Content =
                    "Select the part number from the list of items on the Purchase Order:\n\n"
                    + "• List shows all open lines on the PO\n"
                    + "• Verify part description matches material\n"
                    + "• Check ordered quantity vs. received quantity\n\n"
                    + "If the part is not listed, verify the PO number or check with purchasing.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "ShapeOutline",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.LoadEntry" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.LoadEntry",
                Title = "Enter Load Details",
                Content =
                    "Enter the number of loads (pallets/containers) being received:\n\n"
                    + "• Enter total number of identical loads\n"
                    + "• System will generate one label per load\n"
                    + "• If loads have different quantities/weights, enter them separately\n\n"
                    + "Example: Receiving 3 pallets of 1000 lbs each -> Enter 3 loads.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "Pallet",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.WeightQuantity" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.PackageType",
                Title = "Select Package Type",
                Content =
                    "Choose the type of packaging for this load:\n\n"
                    + "• Standard types: Box, Pallet, Crate, Bundle, etc.\n"
                    + "• Custom types: Select 'Custom' to enter a new type\n"
                    + "• Preferences: System remembers your last choice for this part\n\n"
                    + "Correct package type helps with inventory identification and storage.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "PackageVariant",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.Review" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.ManualEntry",
                Title = "Manual Entry Mode",
                Content =
                    "Bulk entry mode for processing multiple receiving items:\n\n"
                    + "• Add multiple rows for different parts/POs\n"
                    + "• Copy/paste functionality supported\n"
                    + "• Ideal for mixed loads or high-volume receiving\n\n"
                    + "Ensure all required fields (PO, Part, Qty) are filled before saving.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "Table",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.ModeSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.EditMode",
                Title = "Edit Historical Data",
                Content =
                    "Review and edit past receiving entries:\n\n"
                    + "• Filter by date range or search by PO/Part\n"
                    + "• Modify quantities, weights, or heat/lot numbers\n"
                    + "• Reprint labels for past receipts\n\n"
                    + "Changes are logged and updated in the database.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "PencilBox",
                Severity = Enum_HelpSeverity.Warning,
                RelatedKeys = new List<string> { "Receiving.ModeSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.Saving",
                Title = "Saving Your Receiving",
                Content =
                    "The app is saving your receiving entry. Do not close the app or use the Back button until it finishes.\n\n"
                    + "**What is happening:**\n"
                    + "• Database record is being written to MySQL\n"
                    + "• Label data is being queued for printing\n"
                    + "• A CSV backup is exported for records\n\n"
                    + "**If a step fails:**\n"
                    + "• The completion screen shows the specific error\n"
                    + "• Contact your supervisor or IT support",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "ContentSave",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.Review" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.Complete",
                Title = "Receiving Complete",
                Content =
                    "Your receiving entry saved successfully.\n\n"
                    + "**What was saved:**\n"
                    + "• Label Data - queued for printing\n"
                    + "• Database - permanent record stored\n\n"
                    + "**Next steps:**\n"
                    + "• Start New Entry - begin another receiving transaction\n"
                    + "• Mode Selection - return to choose a different entry mode\n"
                    + "• Reprint labels later from Edit Mode\n\n"
                    + "If any step shows a failure, review the error message and contact your supervisor.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "CheckCircle",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.ModeSelection", "Receiving.Review" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.ReconciliationReview",
                Title = "Reconcile Saved Locations",
                Content =
                    "Review saved loads against InforVisual to verify their storage locations.\n\n"
                    + "**What this screen does:**\n"
                    + "• Compares InforVisual transaction history with current inventory\n"
                    + "• Shows suggested locations where quantities match exactly\n"
                    + "• Lets you confirm or adjust each saved load's location\n\n"
                    + "**Why:**\n"
                    + "• Keeps inventory locations accurate in InforVisual\n"
                    + "• Prevents misplaced stock\n"
                    + "• Uses read-only data from InforVisual",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "MapMarker",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.ModeSelection" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.NonPOEntry",
                Title = "Non-PO Reference",
                Content =
                    "No PO number was entered, so the app asks for a reference reason for this receiving batch.\n\n"
                    + "**Reference / Reason:**\n"
                    + "• Enter a short reason (e.g. Stock Replenishment, Internal Move, Sample Run)\n"
                    + "• Stored with the batch and shown on the Review page\n\n"
                    + "**Previously Saved Entries:**\n"
                    + "• Pick a saved entry to reuse it\n"
                    + "• Remove an entry with the trash icon\n\n"
                    + "**Save for next time:**\n"
                    + "• Keep the reference so it is offered again later\n\n"
                    + "Use This Reference confirms the entry; Cancel closes the dialog.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Info,
                Icon = "TextBox",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.POEntry" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Receiving.EditModeColumnChooser",
                Title = "Choose Visible Columns",
                Content =
                    "Choose which columns appear in Edit Mode. Related fields are grouped together so the list is easier to scan and update.\n\n"
                    + "**Groups:**\n"
                    + "• Part and receiving details\n"
                    + "• Purchase order details\n"
                    + "• Packaging and traceability\n"
                    + "• Quality hold\n"
                    + "• Audit and ownership\n\n"
                    + "**Buttons:**\n"
                    + "• Select All / Clear All - toggle every optional column\n"
                    + "• Reset - restore the original column visibility\n"
                    + "• Apply - save your visibility choices\n"
                    + "• Cancel - keep the existing grid unchanged\n\n"
                    + "Some columns are always shown and cannot be hidden.",
                Category = "Receiving Workflow",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "ViewColumn",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Receiving.EditMode" },
            }
        );
    }

    private void InitializeModuleHelp()
    {
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Reporting.Main",
                Title = "End of Day Reports",
                Content =
                    "Generate end-of-day reports for one or more modules.\n\n"
                    + "**Date range:**\n"
                    + "• Pick a quick range or set Start/End dates\n"
                    + "• Check availability to see which modules have data\n\n"
                    + "**Select modules:**\n"
                    + "• Choose which modules to include\n"
                    + "• Generate a combined preview for the selected modules",
                Category = "Reporting",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "FileChart",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Reprint.Main",
                Title = "Reprint Labels",
                Content =
                    "Reprint Receiving, Dunnage, or Volvo labels from history.\n\n"
                    + "• Pick a module to open its reprint page\n"
                    + "• Review the history and check the rows to reprint\n"
                    + "• Click Reprint Selected to move rows back into the active label queue",
                Category = "Reprint",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "Printer",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.Main",
                Title = "Ship/Rec Tools",
                Content =
                    "Utilities for shipping and receiving operations.\n\n"
                    + "• Choose a tool from the selection screen\n"
                    + "• Tools include Outside Service History, Material Availability Board, PO Line Spec Search, Dunnage Book, Welded Coils, Receiving Analytics, and Delivery Schedule",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "Tools",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.ShipmentEntry",
                Title = "Volvo Dunnage Requisition",
                Content =
                    "Create and submit Volvo dunnage requisitions.\n\n"
                    + "• Enter shipment line details for each part\n"
                    + "• Add parts, report discrepancies, and track quantities\n"
                    + "• Entries are auto-saved as you work\n"
                    + "• Complete the shipment when all lines are entered",
                Category = "Volvo",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "Truck",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.History",
                Title = "Volvo Shipment History",
                Content =
                    "Review past Volvo shipments and requisitions.\n\n"
                    + "• Recent shipments load automatically (last 30 days)\n"
                    + "• Filter by date or search terms\n"
                    + "• View details, edit, or delete a shipment",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "History",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Scanner.Main",
                Title = "Scanner",
                Content =
                    "Scanner workbench, history, and settings.\n\n"
                    + "• Workbench - run scanner send/sweep operations\n"
                    + "• History - review past scan results\n"
                    + "• Settings - configure scanner behavior",
                Category = "Scanner",
                HelpType = Enum_HelpType.Info,
                Icon = "ScanHelper",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.ToolSelection",
                Title = "Ship/Rec Tools",
                Content =
                    "Choose a utility from the tool cards.\n\n"
                    + "• Outside Service History\n"
                    + "• Material Availability Board\n"
                    + "• PO Line Spec Search\n"
                    + "• Dunnage Book\n"
                    + "• Welded Coils\n"
                    + "• Receiving Analytics\n"
                    + "• Delivery Schedule",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "ViewGrid",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.OutsideServiceHistory",
                Title = "Outside Service History",
                Content =
                    "Look up dispatch history for parts sent to outside service providers.\n\n"
                    + "• Search by part number or vendor name\n"
                    + "• Review dispatch and receipt history (read-only)\n"
                    + "• Use the fuzzy picker when the search returns similar matches",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "TruckClock",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.MaterialAvailabilityBoard",
                Title = "Material Availability Board",
                Content =
                    "Read-only card view of material availability by location.\n\n"
                    + "• Search by location or part\n"
                    + "• Cards show on-hand availability and details\n"
                    + "• Open work-order or incoming detail dialogs from a card",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "ViewDashboard",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.POLineSpecSearch",
                Title = "PO Line Spec Search",
                Content =
                    "Search purchase-order line specification text.\n\n"
                    + "• Enter a search term to find matching PO lines\n"
                    + "• Configure which spec columns to match in Search Options\n"
                    + "• Open a row to view the full spec text",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "Magnify",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.DunnageBook",
                Title = "Dunnage Book",
                Content =
                    "Generate the dunnage book report for the selected criteria.\n\n"
                    + "• Configure the book (dates, types, parts)\n"
                    + "• Preview the generated pages\n"
                    + "• Print or save as PDF",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "BookOpen",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.WeldedCoils",
                Title = "Welded Coils",
                Content =
                    "Maintain welded-coil inventory.\n\n"
                    + "• Search and add coils\n"
                    + "• Toggle active status per coil\n"
                    + "• Delete selected coils when no longer needed",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "CircleMultiple",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.ReceivingAnalytics",
                Title = "Receiving Analytics",
                Content =
                    "Review receiving statistics and trends.\n\n"
                    + "• Filter by date, location, or part\n"
                    + "• View summary cards and charts\n"
                    + "• Export results when needed",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "ChartLine",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRecTools.DeliverySchedule",
                Title = "Delivery Schedule",
                Content =
                    "Review and export the delivery schedule.\n\n"
                    + "• Filter the schedule as needed\n"
                    + "• Open details for a scheduled delivery\n"
                    + "• Export results when needed",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "TruckDelivery",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );
    }

    private void InitializeDialogHelp()
    {
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Shared.TerminalLogin",
                Title = "Shared Terminal Login",
                Content =
                    "Log in with your Windows username and 4-digit PIN.\n\n"
                    + "• Username - your application username\n"
                    + "• PIN - your 4-digit personal PIN\n"
                    + "• After 3 failed attempts the terminal locks\n\n"
                    + "If you do not know your PIN, ask your supervisor to reset it.",
                Category = "Shared",
                HelpType = Enum_HelpType.Info,
                Icon = "Lock",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Shared.NewUserSetup",
                Title = "New User Entry",
                Content =
                    "Create your account the first time you use the app.\n\n"
                    + "• Name - first and last name\n"
                    + "• Employee number - your badge/employee ID\n"
                    + "• Department / Shift - pick from the lists\n"
                    + "• PIN - choose a 4-digit PIN and confirm it\n"
                    + "• Infor Visual credentials - your ERP sign-in used for lookups\n\n"
                    + "The Create Account button enables once all required fields are valid.",
                Category = "Shared",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "AccountPlus",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.NonPOEntry",
                Title = "Non-PO Reference",
                Content =
                    "No PO number was entered, so a reference reason is requested.\n\n"
                    + "• Enter a short reason (e.g. Stock Replenishment, Return Dunnage, Internal Use)\n"
                    + "• Stored with the load and shown on the review page\n"
                    + "• Pick a saved entry to reuse it, or remove one with the trash icon\n"
                    + "• Save for next time keeps the reference for later",
                Category = "Dunnage",
                HelpType = Enum_HelpType.Info,
                Icon = "TextBox",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.EditModeColumnChooser",
                Title = "Choose Visible Columns",
                Content =
                    "Choose which columns appear in Dunnage Edit Mode.\n\n"
                    + "• Fields are grouped so the list is easy to scan\n"
                    + "• Use Select All / Clear All / Reset to adjust quickly\n"
                    + "• Apply saves your visibility choices; Cancel keeps the current layout\n"
                    + "• Some columns are always shown and cannot be hidden",
                Category = "Dunnage",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "ViewColumn",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.AddMultipleRows",
                Title = "Add Multiple Rows",
                Content =
                    "Add several blank rows to Manual Entry at once.\n\n"
                    + "• Enter the number of rows to add (1 or more)\n"
                    + "• Click Add (primary button) to insert them\n"
                    + "• Fill in each new row as usual",
                Category = "Dunnage",
                HelpType = Enum_HelpType.Info,
                Icon = "TablePlus",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.SelectExistingSpecs",
                Title = "Select Existing Specs",
                Content =
                    "Copy saved details from an existing part to speed up entry.\n\n"
                    + "• Search or pick a part whose saved details you want to reuse\n"
                    + "• Part ID, inventory type, and home location can still be changed after copying",
                Category = "Dunnage",
                HelpType = Enum_HelpType.Info,
                Icon = "ContentCopy",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Shared.IconSelector",
                Title = "Select Icon",
                Content =
                    "Pick a Material icon to represent the item.\n\n"
                    + "• Search icons by name (e.g. box, truck, pallet)\n"
                    + "• Show all icons reveals the full library\n"
                    + "• Select an icon and confirm your choice",
                Category = "Shared",
                HelpType = Enum_HelpType.Info,
                Icon = "ShapeOutline",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.ImagePartSearch",
                Title = "Image Part Search",
                Content =
                    "Find a part by its image instead of a part ID.\n\n"
                    + "• Browse the part image catalog\n"
                    + "• Pick the image that matches the material on the floor\n"
                    + "• The matching part is loaded into the workflow",
                Category = "Dunnage",
                HelpType = Enum_HelpType.Info,
                Icon = "ImageSearch",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Dunnage.PartInfoModal",
                Title = "Part Information",
                Content =
                    "Read-only summary of the selected part.\n\n"
                    + "• Stored part metadata and inventory settings\n"
                    + "• Spec values saved for this part\n"
                    + "• No edits are made from this view",
                Category = "Dunnage",
                HelpType = Enum_HelpType.Info,
                Icon = "Information",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.EmailPreview",
                Title = "Email Preview",
                Content =
                    "Review the prepared Volvo requisition email before it is sent.\n\n"
                    + "• Confirm recipients and the email body\n"
                    + "• Copy Email Body copies the formatted text to the clipboard",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "Email",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.GeneratedLabelData",
                Title = "Generated Label Data",
                Content =
                    "Review the queued Volvo generated-label rows.\n\n"
                    + "• Shows part, quantity, and skid for each row\n"
                    + "• Clear Label Data removes all queued rows",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "Label",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.PartNumberEdit",
                Title = "Change Part Number",
                Content =
                    "Replace the part number on this shipment card.\n\n"
                    + "• Search for the replacement part number\n"
                    + "• Select the correct match and save\n"
                    + "• The card updates to the new part",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "Pencil",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.ShipmentHistoryDetail",
                Title = "Shipment History Detail",
                Content =
                    "Read-only details for an archived Volvo shipment.\n\n"
                    + "• Review the shipment and its line items\n"
                    + "• Details cannot be changed here",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "ClipboardText",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.ShipmentEdit",
                Title = "Edit Shipment",
                Content =
                    "Edit a Volvo shipment's line items.\n\n"
                    + "• Add or remove part rows\n"
                    + "• Edit quantities and details inline\n"
                    + "• Archived shipments open read-only",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "PencilBox",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Volvo.PartAddEdit",
                Title = "Add / Edit Volvo Part",
                Content =
                    "Maintain the Volvo part master.\n\n"
                    + "• Enter the part number and description\n"
                    + "• Only the fields used by shipment entry are required\n"
                    + "• Save writes the change to the part catalog",
                Category = "Volvo",
                HelpType = Enum_HelpType.Info,
                Icon = "PackageVariant",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Reporting.Preview",
                Title = "Report Preview",
                Content =
                    "Review the combined report preview before finalizing.\n\n"
                    + "• Verify each module section looks correct\n"
                    + "• Use the preview to confirm formatting and data",
                Category = "Reporting",
                HelpType = Enum_HelpType.Info,
                Icon = "FileEye",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Scanner.ManageItems",
                Title = "Manage Items",
                Content =
                    "Rearrange the scanner batch before sending.\n\n"
                    + "• Add or duplicate an item\n"
                    + "• Move items up/down to set the order\n"
                    + "• Apply saves your changes to the batch",
                Category = "Scanner",
                HelpType = Enum_HelpType.Info,
                Icon = "FormatListBulleted",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Reprint.ColumnChooser",
                Title = "Choose Visible Columns",
                Content =
                    "Choose which columns appear in the reprint history grid.\n\n"
                    + "• The selection checkbox column is always shown\n"
                    + "• Apply saves your visibility choices; Cancel keeps the current layout",
                Category = "Reprint",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "ViewColumn",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRec.POLineSpecTextViewer",
                Title = "PO Line Spec Text",
                Content =
                    "Read-only full specification text for a PO line.\n\n"
                    + "• Displays the complete stored spec text\n"
                    + "• Use Close when finished",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "FileDocument",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRec.POLineSpecSearchOptions",
                Title = "PO Line Spec Search Options",
                Content =
                    "Choose which spec columns to search and show.\n\n"
                    + "• Enable the fields you want to match on\n"
                    + "• Apply runs the search with the selected options",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "Magnify",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRec.MaterialAvailabilityWorkOrder",
                Title = "Work Order Availability",
                Content =
                    "Material availability for a work order.\n\n"
                    + "• Shows the work-order details and required material\n"
                    + "• Use Close when finished",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "ClipboardCheck",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRec.MaterialAvailabilityIncoming",
                Title = "Incoming Material Availability",
                Content =
                    "Material availability for incoming shipments.\n\n"
                    + "• Shows the incoming order and expected material\n"
                    + "• Use Close when finished",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "TruckDelivery",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "ShipRec.DunnageBookPreview",
                Title = "Dunnage Book Preview",
                Content =
                    "Preview the dunnage book report before output.\n\n"
                    + "• Review the generated pages\n"
                    + "• Print / Save as PDF writes the report",
                Category = "ShipRec Tools",
                HelpType = Enum_HelpType.Info,
                Icon = "BookOpen",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string>(),
            }
        );
    }

    private void InitializeAdminHelp()
    {
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Admin.Types",
                Title = "Manage Dunnage Types",
                Content =
                    "Create, edit, and delete dunnage types:\n\n"
                    + "**Creating Types:**\n"
                    + "• Choose a descriptive, unique name\n"
                    + "• Select an appropriate icon for visual identification\n"
                    + "• Define specification fields (dimensions, materials, etc.)\n\n"
                    + "**Specification Fields:**\n"
                    + "• Text fields - For descriptions, notes\n"
                    + "• Number fields - For measurements, quantities\n"
                    + "• Set min/max ranges for validation\n"
                    + "• Mark fields as required or optional\n\n"
                    + "**Deleting Types:**\n"
                    + " Warning: Deleting a type will affect:\n"
                    + "• All associated parts\n"
                    + "• Historical receiving records\n"
                    + "• Inventory counts\n\n"
                    + "System will show impact analysis before allowing deletion.",
                Category = "Admin",
                HelpType = Enum_HelpType.Tutorial,
                Icon = "Cog",
                Severity = Enum_HelpSeverity.Warning,
                RelatedKeys = new List<string> { "Admin.Parts" },
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Admin.Parts",
                Title = "Manage Dunnage Parts",
                Content =
                    "Create and manage dunnage part specifications:\n\n"
                    + "**Part Information:**\n"
                    + "• Part must be associated with a dunnage type\n"
                    + "• Enter dimensions and specifications as required\n"
                    + "• Add descriptive notes for identification\n\n"
                    + "**Inventory Management:**\n"
                    + "• Mark parts for inventory tracking\n"
                    + "• Set reorder points and quantities\n"
                    + "• Track current stock levels\n\n"
                    + "**Search & Filtering:**\n"
                    + "• Search by part name or description\n"
                    + "• Filter by dunnage type\n"
                    + "• Sort by any column\n\n"
                    + "Parts can be created during receiving using Quick Add feature.",
                Category = "Admin",
                HelpType = Enum_HelpType.Info,
                Icon = "Shape",
                Severity = Enum_HelpSeverity.Info,
                RelatedKeys = new List<string> { "Admin.Types", "Dunnage.PartSelection" },
            }
        );
    }

    private void InitializeTooltips()
    {
        // Navigation tooltips
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FirstPage",
                Content = "First Page",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.PreviousPage",
                Content = "Previous Page",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.NextPage",
                Content = "Next Page",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.LastPage",
                Content = "Last Page",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Mode selection quick access
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.QuickGuidedWizard",
                Content = "Skip mode selection and go directly to Guided Wizard",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.QuickManualEntry",
                Content = "Skip mode selection and go directly to Manual Entry",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.QuickEditMode",
                Content = "Skip mode selection and go directly to Edit Mode",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Workflow actions
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.ReturnToModeSelection",
                Content = "Return to mode selection (clears current work)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.ShowHelp",
                Content = "Click for help about current step",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.Refresh",
                Content = "Refresh parts list",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Manual entry operations
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.AddMultipleRows",
                Content = "Add multiple rows at once (up to 100)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.AutoFill",
                Content = "Auto-fill from last entry for this Part ID",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FillBlanks",
                Content = "Copy PO, Location, and specs from last row to empty fields",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SortForPrinting",
                Content = "Sort by Part ID â†’ PO â†’ Type",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Edit mode data sources
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.LoadSessionMemory",
                Content = "Load unsaved loads from current session",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.LoadRecentCSV",
                Content = "Load from most recent saved label dataset",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.LoadHistoricalData",
                Content = "Load historical loads from database",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Admin operations
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.CreateType",
                Content = "Create a new dunnage type",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.EditType",
                Content = "Edit selected type",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.DeleteType",
                Content = "Delete selected type with impact analysis",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.ReturnToAdmin",
                Content = "Return to admin main navigation",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Admin Parts
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.CreatePart",
                Content = "Create a new dunnage part",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.EditPart",
                Content = "Edit selected part",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.DeletePart",
                Content = "Delete selected part",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Edit Mode
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.RemoveSelectedRows",
                Content = "Remove all selected rows",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FilterLastWeek",
                Content = "Show loads from last 7 days",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FilterToday",
                Content = "Show loads from today only",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FilterThisWeek",
                Content = "Show loads from this week (Mon-Sun)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FilterThisMonth",
                Content = "Show loads from this month",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FilterThisQuarter",
                Content = "Show loads from this quarter",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.FilterShowAll",
                Content = "Show all loads from last year",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SaveChanges",
                Content = "Save all changes to database",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SelectAll",
                Content = "Select or deselect all rows",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.RemoveRow",
                Content = "Remove selected rows",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.ClearFilters",
                Content = "Remove all date filters",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.GoToPage",
                Content = "Go to specified page",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SaveAndFinish",
                Content = "Save all changes and finish",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Manual Entry
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.AddRow",
                Content = "Add a new row to the grid",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Quick Add Dialog
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.HelpDunnageTypes",
                Content = "Click for help about dunnage types",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.TypeName",
                Content = "Enter a unique descriptive name for this dunnage type",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SelectIcon",
                Content = "Click to browse and select an icon from the icon library",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.SpecName",
                Content = "Enter a unique name for this specification field",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.SpecType",
                Content = "Select the data type for this field",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.MinValue",
                Content = "Minimum allowed value (optional)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.MaxValue",
                Content = "Maximum allowed value (optional)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.Unit",
                Content = "Unit of measurement to display (optional)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Field.Required",
                Content = "Check if this field must be filled when creating parts",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.AddSpec",
                Content = "Add this specification to the list below",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.List.Specs",
                Content = "List of specifications defined for this type",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.RemoveSpec",
                Content = "Remove this specification",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Review View
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.AddAnother",
                Content = "Add another load to the current batch",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SaveAndGenerate",
                Content = "Save all entries to database and generate labels",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Workflow View
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.ModeSelection",
                Content = "Return to mode selection (clears current work)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.StepHelp",
                Content = "Click for help about the current step",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.BackStep",
                Content = "Go back to previous step",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.NextStep",
                Content = "Proceed to next step",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SaveAndReview",
                Content = "Save entry and proceed to review",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.StartNew",
                Content = "Start a new receiving entry",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.ResetCSV",
                Content = "Clear the CSV data and start over",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Receiving PO Entry
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.LoadPO",
                Content = "Load purchase order details from Infor Visual",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SwitchToNonPO",
                Content = "Switch to non-PO entry mode for items without purchase orders",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.LookupPart",
                Content = "Look up part details in the system",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tooltip.Button.SwitchToPO",
                Content = "Switch to PO entry mode",
                HelpType = Enum_HelpType.Tip,
            }
        );
    }

    private void InitializePlaceholders()
    {
        // Dunnage placeholders
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.Quantity",
                Content = "Enter quantity...",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.PONumber",
                Content = "Enter PO number (optional)...",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.Location",
                Content = "Enter location...",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.PartSelection",
                Content = "Choose a part...",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.HeatLotNumber",
                Content = "Enter Heat or Lot Number",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Receiving placeholders
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.PONumberReceiving",
                Content = "Enter PO (e.g., 66868 or PO-066868)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.PartID",
                Content = "Enter Part ID (e.g., MMC-001, MMF-456)",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.Weight",
                Content = "Enter weight...",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.QuantityNumber",
                Content = "Enter whole number",
                HelpType = Enum_HelpType.Tip,
            }
        );

        // Date placeholders
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.StartDate",
                Content = "Start Date",
                HelpType = Enum_HelpType.Tip,
            }
        );
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Placeholder.Field.EndDate",
                Content = "End Date",
                HelpType = Enum_HelpType.Tip,
            }
        );
    }

    private void InitializeTips()
    {
        // Dunnage Tips
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Dunnage.QuantityEntry",
                Content =
                    "You can adjust the quantity later if needed. The system will generate individual labels for each item.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Dunnage.PartSelection",
                Content =
                    "Use the search function to quickly find parts. Parts with a green badge are tracked in inventory.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Dunnage.ManualEntry",
                Content =
                    "Use Tab key to navigate between cells quickly. Sort entries by Part ID before saving for sequential labels.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Dunnage.TypeSelection",
                Content = "Choose a dunnage type from the available options below.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Dunnage.DetailsEntry",
                Content =
                    "Provide additional information such as PO number and location for this dunnage receiving.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        // Receiving Tips
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Receiving.POEntry",
                Content =
                    "Enter the purchase order number to load part details from Infor Visual ERP.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Receiving.ManualEntry",
                Content =
                    "Use the manual entry grid for bulk data entry. You can paste from Excel or use Auto-Fill for common data.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "Tip.Receiving.PartSelection",
                Content =
                    "Select the part from the list or search by Part ID. The package type is auto-detected based on part specifications.",
                HelpType = Enum_HelpType.Tip,
                Icon = "Lightbulb",
            }
        );
    }

    private void InitializeInfoBarMessages()
    {
        AddHelpContent(
            new Model_HelpContent
            {
                Key = "InfoBar.InventoryWarning",
                Content =
                    "This part is not in the inventoried dunnage list. Add it to inventory for better tracking.",
                HelpType = Enum_HelpType.Warning,
                Severity = Enum_HelpSeverity.Warning,
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "InfoBar.LocationRequired",
                Content = "Location is required before proceeding to the next step.",
                HelpType = Enum_HelpType.Warning,
                Severity = Enum_HelpSeverity.Warning,
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "InfoBar.SaveSuccess",
                Content = "Successfully saved all entries. Labels are ready for printing.",
                HelpType = Enum_HelpType.Info,
                Severity = Enum_HelpSeverity.Info,
            }
        );

        AddHelpContent(
            new Model_HelpContent
            {
                Key = "InfoBar.WeightQuantitySelection",
                Content =
                    "Choose whether to enter by weight or piece count based on the PO line item unit of measure.",
                HelpType = Enum_HelpType.Info,
                Severity = Enum_HelpSeverity.Info,
            }
        );
    }

    private void AddHelpContent(Model_HelpContent content)
    {
        if (!string.IsNullOrEmpty(content.Key))
        {
            _helpContentCache[content.Key] = content;
        }
    }

    #endregion
}
