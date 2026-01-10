using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Module_UI_Mockup.ViewModels;

/// <summary>
/// ViewModel for the Welcome/Main landing page.
/// Provides quick navigation items for all control categories.
/// </summary>
public partial class ViewModel_UI_Mockup_Main : ViewModel_Base
{
    #region Observable Properties
    /// <summary>
    /// Gets the collection of quick navigation items for control categories.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<QuickNavItem> _quickNavItems;

    /// <summary>
    /// Gets or sets the requested tab index for navigation.
    /// Window will monitor this property and navigate when it changes.
    /// </summary>
    [ObservableProperty]
    private int _requestedTabIndex = -1;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the ViewModel_UI_Mockup_Main class.
    /// </summary>
    public ViewModel_UI_Mockup_Main()
    {
        _quickNavItems = new ObservableCollection<QuickNavItem>
        {
            new QuickNavItem { Title = "Basic Input", Description = "Buttons, toggles, checkboxes, sliders, switches", IconGlyph = "\uE765" },
            new QuickNavItem { Title = "Text Controls", Description = "TextBox, PasswordBox, ComboBox, AutoSuggestBox, NumberBox", IconGlyph = "\uE8D2" },
            new QuickNavItem { Title = "Collections", Description = "ListView, GridView, TreeView, DataGrid, ItemsRepeater", IconGlyph = "\uE8FD" },
            new QuickNavItem { Title = "Navigation", Description = "NavigationView, TabView, BreadcrumbBar, CommandBar", IconGlyph = "\uE700" },
            new QuickNavItem { Title = "Dialogs & Flyouts", Description = "ContentDialog, Flyout, MenuFlyout, TeachingTip", IconGlyph = "\uE8BD" },
            new QuickNavItem { Title = "Date & Time", Description = "CalendarDatePicker, CalendarView, DatePicker, TimePicker", IconGlyph = "\uE787" },
            new QuickNavItem { Title = "Media & Visual", Description = "Image, PersonPicture, WebView2, AnimatedIcon, Shapes", IconGlyph = "\uE8B9" },
            new QuickNavItem { Title = "Layout & Panels", Description = "Grid, StackPanel, ScrollViewer, SplitView, Border", IconGlyph = "\uE80A" },
            new QuickNavItem { Title = "Status & Feedback", Description = "ProgressBar, InfoBar, ToolTip, Badge, Chips", IconGlyph = "\uE7BA" },
            new QuickNavItem { Title = "Advanced", Description = "SwipeControl, InkCanvas, RefreshContainer, Pivot", IconGlyph = "\uE90F" },
            new QuickNavItem { Title = "App Patterns", Description = "Typography, Colors, Spacing, Shadows, Card patterns", IconGlyph = "\uE771" },
            new QuickNavItem { Title = "Custom Controls", Description = "34 manufacturing-specific custom controls", IconGlyph = "\uE946" }
        };

        StatusMessage = "Welcome to WinUI 3 Control Gallery";
    }
    #endregion
}

/// <summary>
/// Represents a quick navigation item for a control category.
/// </summary>
public class QuickNavItem
{
    /// <summary>
    /// Gets or sets the title of the category.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the category.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon glyph for the category.
    /// </summary>
    public string IconGlyph { get; set; } = "\uE8EE";
}
