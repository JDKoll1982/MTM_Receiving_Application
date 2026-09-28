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
            [DunnageSettingsKeys.Labels.DunnageLabelPath] = string.Empty,
            [DunnageSettingsKeys.Application.DefaultImageLocation] = string.Empty,
            [DunnageSettingsKeys.UserPreferences.DefaultLocation] = "RECV",
            [DunnageSettingsKeys.UserPreferences.PreferredThumbnailSize] = "96",
            [DunnageSettingsKeys.UserPreferences.PreferPartImages] = "true",
            [DunnageSettingsKeys.UserPreferences.TypeSelectionSort] = "Name (A-Z)",
            [DunnageSettingsKeys.UserPreferences.ShowPartsWithoutImages] = "false",
            [DunnageSettingsKeys.UiUx.EnableTypeImages] = "true",
            [DunnageSettingsKeys.UiUx.EnablePartImages] = "true",
            [DunnageSettingsKeys.UiUx.DefaultThumbnailSize] = "96",
            [DunnageSettingsKeys.UiUx.MaximumImageFileSizeKb] = "512",
            [DunnageSettingsKeys.UiUx.DefaultVisualSource] = "Part , Type then Icon",
            [DunnageSettingsKeys.Workflow.ShowTypeImagesOnTypeSelection] = "true",
            [DunnageSettingsKeys.Workflow.ShowPartImagesOnPartSelection] = "true",
            [DunnageSettingsKeys.Workflow.ShowImagesOnReview] = "true",
            [DunnageSettingsKeys.Workflow.FallbackToTypeImageWhenPartMissing] = "true",
        };

    public static IReadOnlyDictionary<string, bool> BoolDefaults { get; } =
        new Dictionary<string, bool>
        {
            [DunnageSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled] = true,
        };
}
