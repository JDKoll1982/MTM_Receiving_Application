using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI
{
    public static class Helper_WorkflowHelpContentGenerator
    {
        public static UIElement GenerateHelpContent(Enum_ReceivingWorkflowStep step)
        {
            return step switch
            {
                Enum_ReceivingWorkflowStep.ModeSelection => CreateModeSelectionHelp(),
                Enum_ReceivingWorkflowStep.ManualEntry => CreateManualEntryHelp(),
                Enum_ReceivingWorkflowStep.POEntry => CreatePOEntryHelp(),
                Enum_ReceivingWorkflowStep.LoadEntry => CreateLoadEntryHelp(),
                Enum_ReceivingWorkflowStep.WeightQuantityEntry => CreateWeightQuantityHelp(),
                Enum_ReceivingWorkflowStep.HeatLotEntry => CreateHeatLotHelp(),
                Enum_ReceivingWorkflowStep.PackageTypeEntry => CreatePackageTypeHelp(),
                Enum_ReceivingWorkflowStep.Review => CreateReviewHelp(),
                _ => CreateDefaultHelp()
            };
        }

        private static UIElement CreateModeSelectionHelp()
        {
            return CreateHelpPanel(
                "Entry Mode Selection",
                "Choose how you want to enter receiving data.",
                new (string header, string content)[]
                {
                    ("Guided Wizard", "Step-by-step process for standard receiving. Ideal for one PO or part at a time. Validates data at each step. Best for beginners or infrequent users."),
                    ("Manual Entry", "Grid-based bulk data entry. Enter multiple loads at once. Best for experienced users. Faster for large shipments.")
                });
        }

        private static UIElement CreateManualEntryHelp()
        {
            return CreateHelpPanel(
                "Manual Entry Mode",
                "Bulk data entry grid for experienced users. Enter multiple loads quickly.",
                new (string header, string content)[]
                {
                    ("Grid Columns", "PO Number (6 digits) â€¢ Part ID (material part number) â€¢ Weight/Qty (pounds or piece count) â€¢ Heat/Lot (traceability) â€¢ Package Type (Coils, Sheets, Skids)"),
                    ("Quick Actions", "Add Row (insert new blank row) â€¢ Remove Row (delete selected row) â€¢ Click cells to edit directly"),
                    ("Important", "Data is validated when you click Save. Errors will be highlighted. Best for users familiar with receiving workflow.")
                });
        }

        private static UIElement CreatePOEntryHelp()
        {
            return CreateHelpPanel(
                "Purchase Order Entry",
                "Enter the PO number from your vendor shipment.",
                new (string header, string content)[]
                {
                    ("Accepted Formats", "6 digits (e.g., 66868) or with prefix (e.g., PO-066868). Auto-formats on tab/enter."),
                    ("What Happens", "System validates PO exists in Infor Visual, shows all parts on the PO, displays remaining quantities. Select a part to receive."),
                    ("Non-PO Entry", "Use for items without a PO. Enter part number manually (e.g., MMC0000123 or MMF0000456).")
                });
        }

        private static UIElement CreateLoadEntryHelp()
        {
            return CreateHelpPanel(
                "Number of Loads",
                "How many skids/loads are you receiving for this part?",
                new (string header, string content)[]
                {
                    ("Examples", "â€¢ Coils: Each coil = 1 load\nâ€¢ Sheets: Each skid of sheets = 1 load\nâ€¢ Bars: Each bundle = 1 load"),
                    ("Valid Range", "Enter between 1 and 99 loads."),
                    ("Note", "Each load will be labeled separately for tracking.")
                });
        }

        private static UIElement CreateWeightQuantityHelp()
        {
            return CreateHelpPanel(
                "Weight/Quantity Entry",
                "Enter the weight or quantity for each load.",
                new (string header, string content)[]
                {
                    ("For Coils/Sheets (by weight)", "Enter weight in pounds (lbs). Example: 2500 for 2,500 lbs. Whole numbers only - decimals NOT allowed."),
                    ("For Bars/Pieces (by count)", "Enter piece count. Example: 48 for 48 pieces. Whole numbers only."),
                    ("Warning", "Total entered should not exceed PO quantity. System will alert if over.")
                });
        }

        private static UIElement CreateHeatLotHelp()
        {
            return CreateHelpPanel(
                "Heat/Lot Numbers",
                "Enter heat or lot numbers for traceability.",
                new (string header, string content)[]
                {
                    ("Heat Number (for coils/sheets)", "Mill-assigned identifier. Usually found on coil tag. Example: H123456789"),
                    ("Lot Number (for bars/parts)", "Batch identifier from vendor. Found on packing slip or bundle tag. Example: LOT-2024-1234"),
                    ("Optional Field", "If blank, will be recorded as 'Not Entered'.")
                });
        }

        private static UIElement CreatePackageTypeHelp()
        {
            return CreateHelpPanel(
                "Package Details",
                "Define how material is packaged for accurate tracking and inventory.",
                new (string header, string content)[]
                {
                    ("Package Types", "â€¢ Coils: Single coils on eye-to-sky skids\nâ€¢ Sheets: Stacked flat sheets on skids\nâ€¢ Skids: Mixed or bundled material\nâ€¢ Custom: Enter your own description"),
                    ("Packages per Load", "How many packages on each load? Examples: 1 coil per skid = 1, or 5 bundles per skid = 5"),
                    ("Weight per Package", "Automatically calculated by dividing total load weight by package count.")
                });
        }

        private static UIElement CreateReviewHelp()
        {
            return CreateHelpPanel(
                "Review & Save",
                "Review all entries before saving to the system.",
                new (string header, string content)[]
                {
                    ("Single Entry View", "Review one load at a time. Use Previous/Next to navigate. All fields displayed clearly."),
                    ("Table View", "See all loads at once. Quick overview of all data. Switch views with Table View button."),
                    ("What Gets Saved", "â€¢ Database record (permanent)\nâ€¢ Local CSV backup\nâ€¢ Network CSV (if accessible)\nâ€¢ Printable receiving labels")
                });
        }

        private static UIElement CreateDefaultHelp()
        {
            var stack = new StackPanel { Spacing = 12, Padding = new Thickness(4), MaxWidth = 450 };

            var title = new TextBlock
            {
                Text = "Receiving Workflow Help",
                Style = Application.Current.Resources["SubtitleTextBlockStyle"] as Style
            };

            var description = new TextBlock
            {
                Text = "Navigate through the workflow using the Next and Back buttons. Help is available at each step.",
                TextWrapping = TextWrapping.Wrap
            };

            stack.Children.Add(title);
            stack.Children.Add(description);

            return stack;
        }

        private static UIElement CreateHelpPanel(string title, string description, (string header, string content)[] sections)
        {
            var mainStack = new StackPanel { Spacing = 20, Padding = new Thickness(4), MaxWidth = 450 };

            // Title Section
            var titleStack = new StackPanel { Spacing = 8 };
            var titleText = new TextBlock
            {
                Text = title,
                Style = Application.Current.Resources["SubtitleTextBlockStyle"] as Style,
                Foreground = Application.Current.Resources["AccentTextFillColorPrimaryBrush"] as SolidColorBrush
            };

            var underline = new Border
            {
                Height = 2,
                Background = Application.Current.Resources["AccentFillColorDefaultBrush"] as SolidColorBrush,
                CornerRadius = new CornerRadius(1),
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 60
            };

            var descText = new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as SolidColorBrush
            };

            titleStack.Children.Add(titleText);
            titleStack.Children.Add(underline);
            titleStack.Children.Add(descText);
            mainStack.Children.Add(titleStack);

            // Content Sections
            foreach (var section in sections)
            {
                var border = new Border
                {
                    Background = Application.Current.Resources["LayerFillColorDefaultBrush"] as SolidColorBrush,
                    Padding = new Thickness(16),
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as SolidColorBrush
                };

                var sectionStack = new StackPanel { Spacing = 8 };

                var headerText = new TextBlock
                {
                    Text = section.header,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    FontSize = 14
                };

                var contentText = new TextBlock
                {
                    Text = section.content,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as SolidColorBrush
                };

                sectionStack.Children.Add(headerText);
                sectionStack.Children.Add(contentText);
                border.Child = sectionStack;
                mainStack.Children.Add(border);
            }

            return mainStack;
        }
    }
}

