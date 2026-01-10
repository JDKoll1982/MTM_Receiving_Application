using CommunityToolkit.Mvvm.ComponentModel;

namespace Module_UI_Mockup.ViewModels;

/// <summary>
/// Base class for all ViewModels in UI Mockup module.
/// Provides common observable properties for UI state management.
/// This class is standalone with zero external dependencies.
/// </summary>
public partial class ViewModel_Base : ObservableObject
{
    /// <summary>
    /// Indicates whether the ViewModel is currently performing an operation.
    /// Used to show loading indicators in the UI and disable controls during processing.
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Status message to display to the user.
    /// Typically shown in a status bar or notification area to provide feedback.
    /// Initialized to empty string to avoid null reference issues.
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;
}
