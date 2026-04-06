namespace MTM_Receiving_Application.Module_Dunnage.Settings;

public static class DunnageSettingsKeys
{
    public static class UserPreferences
    {
        public const string DefaultLocation = "Dunnage.UserPreferences.DefaultLocation";
        public const string PreferredThumbnailSize =
            "Dunnage.UserPreferences.PreferredThumbnailSize";
        public const string PreferPartImages = "Dunnage.UserPreferences.PreferPartImages";
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
}
