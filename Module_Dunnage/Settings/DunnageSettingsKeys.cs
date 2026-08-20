namespace MTM_Receiving_Application.Module_Dunnage.Settings;

public static class DunnageSettingsKeys
{
    public static class Labels
    {
        public const string DunnageLabelPath = "Dunnage.Labels.DunnageLabelPath";
    }

    public static class UserLabels
    {
        public const string Category = "Dunnage.UserLabels";
        public const string DunnageLabelPath = "DunnageLabelPath";
    }

    public static class Application
    {
        public const string DefaultImageLocation = "Dunnage.Application.DefaultImageLocation";
    }

    public static class UserPreferences
    {
        public const string DefaultLocation = "Dunnage.UserPreferences.DefaultLocation";
        public const string PreferredThumbnailSize =
            "Dunnage.UserPreferences.PreferredThumbnailSize";
        public const string PreferPartImages = "Dunnage.UserPreferences.PreferPartImages";
        public const string TypeSelectionSort = "Dunnage.UserPreferences.TypeSelectionSort";
        public const string ShowPartsWithoutImages =
            "Dunnage.UserPreferences.ShowPartsWithoutImages";
    }

    public static class UiUx
    {
        public const string EnableTypeImages = "Dunnage.UiUx.EnableTypeImages";
        public const string EnablePartImages = "Dunnage.UiUx.EnablePartImages";
        public const string DefaultThumbnailSize = "Dunnage.UiUx.DefaultThumbnailSize";
        public const string MaximumImageFileSizeKb = "Dunnage.UiUx.MaximumImageFileSizeKb";
        public const string DefaultVisualSource = "Dunnage.UiUx.DefaultVisualSource";
    }

    public static class Workflow
    {
        public const string ShowTypeImagesOnTypeSelection =
            "Dunnage.Workflow.ShowTypeImagesOnTypeSelection";
        public const string ShowPartImagesOnPartSelection =
            "Dunnage.Workflow.ShowPartImagesOnPartSelection";
        public const string ShowImagesOnReview = "Dunnage.Workflow.ShowImagesOnReview";
        public const string FallbackToTypeImageWhenPartMissing =
            "Dunnage.Workflow.FallbackToTypeImageWhenPartMissing";
    }

    public static class Shortcuts
    {
        public const string ModeSelection = "Dunnage.Shortcuts.ModeSelection";
        public const string ClearLabelData = "Dunnage.Shortcuts.ClearLabelData";
        public const string NextStep = "Dunnage.Shortcuts.NextStep";
        public const string BackStep = "Dunnage.Shortcuts.BackStep";
        public const string Help = "Dunnage.Shortcuts.Help";
        public const string IsToggleSimpleNavigationEnabled =
            "Dunnage.Shortcuts.IsToggleSimpleNavigationEnabled";
    }
}
