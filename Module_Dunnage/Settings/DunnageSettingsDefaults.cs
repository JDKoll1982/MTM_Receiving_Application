using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Dunnage.Settings;

/// <summary>
/// Default values for Module_Dunnage settings when no persisted value exists.
/// </summary>
public static class DunnageSettingsDefaults
{
    public static IReadOnlyDictionary<string, string> StringDefaults { get; } =
        new Dictionary<string, string>
        {
            [DunnageSettingsKeys.UserPreferences.DefaultLocation] = "RECV",
            [DunnageSettingsKeys.UserPreferences.DefaultImageLocation] = string.Empty,
            [DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize] = "96",
            [DunnageSettingsKeys.UserPreferences.PreferPartImages] = "true",
            [DunnageSettingsKeys.UiUx.EnableTypeImages] = "true",
            [DunnageSettingsKeys.UiUx.EnablePartImages] = "true",
            [DunnageSettingsKeys.UiUx.DefaultThumbnailSize] = "96",
            [DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb] = "512",
            [DunnageSettingsKeys.UiUx.DefaultVisualSource] = "PartThenTypeThenIcon",
            [DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection] = "true",
            [DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection] = "true",
            [DunnageSettingsKeys.Workflow.ShowImagesOnReview] = "true",
            [DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing] = "true",
        };
}
