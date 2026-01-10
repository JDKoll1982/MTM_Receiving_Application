using CommunityToolkit.Mvvm.ComponentModel;

namespace Module_UI_Mockup.ViewModels;

/// <summary>
/// ViewModel for the main UI Mockup window.
/// Manages tab navigation state and current tab information.
/// </summary>
public partial class ViewModel_UI_Mockup : ViewModel_Base
{
    #region Observable Properties
    /// <summary>
    /// Gets or sets the current tab name displayed in the status bar.
    /// Updates when user navigates between tabs.
    /// </summary>
    [ObservableProperty]
    private string _currentTabName = "Welcome";
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the ViewModel_UI_Mockup class.
    /// </summary>
    public ViewModel_UI_Mockup()
    {
        StatusMessage = "Ready";
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the current tab name when tab selection changes.
    /// </summary>
    /// <param name="tabName">The name of the selected tab.</param>
    public void UpdateCurrentTab(string tabName)
    {
        CurrentTabName = tabName;
        StatusMessage = $"Viewing: {tabName}";
    }
    #endregion
}
