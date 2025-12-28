using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Views.Shared;

public sealed partial class Shared_IconSelectorWindow : Window
{
    private List<IconInfo> _allIconsFromFile = new();
    private List<IconInfo> _allIcons = new();
    private List<IconInfo> _filteredIcons = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private static readonly System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("(\\B[A-Z])");
    private static readonly System.Text.RegularExpressions.Regex _regex2 = new System.Text.RegularExpressions.Regex("([a-zA-Z])([0-9])");
    private static readonly System.Text.RegularExpressions.Regex _regex3 = new System.Text.RegularExpressions.Regex("([0-9])([a-zA-Z])");
    private const int ICONS_PER_PAGE = 16;

    public string? SelectedIconGlyph { get; private set; }
    public string? SelectedIconName { get; private set; }
    public bool IconWasSelected { get; private set; }

    public Shared_IconSelectorWindow()
    {
        InitializeComponent();

        // Custom Title Bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        _ = LoadAllIconsAsync();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

        var windowSize = new Windows.Graphics.SizeInt32 { Width = 1000, Height = 850 };
        appWindow.Resize(windowSize);

        var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
        if (displayArea != null)
        {
            var centerX = (displayArea.WorkArea.Width - windowSize.Width) / 2;
            var centerY = (displayArea.WorkArea.Height - windowSize.Height) / 2;
            appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });
        }
    }

    private async Task LoadAllIconsAsync()
    {
        if (_allIconsFromFile.Count > 0)
        {
            return;
        }

        try
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Assets", "FluentIcons.json");

            if (File.Exists(jsonPath))
            {
                var jsonText = await File.ReadAllTextAsync(jsonPath);
                var iconData = JsonSerializer.Deserialize<List<JsonIconData>>(jsonText);

                if (iconData?.Count > 0)
                {
                    _allIconsFromFile = iconData
                        .Select(icon => new IconInfo(
                            CleanIconName(icon.name),
                            icon.codepoint > 0 ? char.ConvertFromUtf32(icon.codepoint) : icon.glyph))
                        .OrderBy(icon => icon.Name)
                        .ToList();

                    ApplyIconFilter();
                    _filteredIcons = [.. _allIcons];
                    UpdateDisplay();
                    return;
                }
            }

            LoadFallbackIcons();
            _allIcons = [.. _allIconsFromFile];
            _filteredIcons = [.. _allIcons];
            UpdateDisplay();
        }
        catch (Exception)
        {
            LoadFallbackIcons();
            _allIcons = [.. _allIconsFromFile];
            _filteredIcons = [.. _allIcons];
            UpdateDisplay();
        }
    }

    private void ApplyIconFilter()
    {
        if (ShowAllIconsCheckBox?.IsChecked == true)
        {
            _allIcons = [.. _allIconsFromFile];
        }
        else
        {
            var relevantKeywords = new[]
            {
                "box", "package", "archive", "cube", "stack", "folder", "container",
                "build", "construction", "manufacturing", "tool", "repair", "hammer",
                "warehouse", "storage", "library", "shelf", "pallet",
                "delivery", "truck", "transport", "vehicle", "shipping",
                "document", "clipboard", "list", "form", "note",
                "grid", "table", "layout", "organize", "arrange",
                "tag", "label", "barcode", "qr", "scan",
                "wood", "metal", "plastic", "material", "crate",
                "shield", "protect", "secure", "lock"
            };

            _allIcons = _allIconsFromFile
                .Where(icon => relevantKeywords.Any(keyword =>
                    icon.Name.ToLower().Contains(keyword)))
                .ToList();
        }
    }

    private string CleanIconName(string rawName)
    {
        // Add space before capital letters (except start)
        var withSpaces = _regex.Replace(rawName, " $1");
        // Add space between letter and number
        withSpaces = _regex2.Replace(withSpaces, "$1 $2");
        // Add space between number and letter
        withSpaces = _regex3.Replace(withSpaces, "$1 $2");
        return withSpaces;
    }

    private void LoadFallbackIcons()
    {
        _allIconsFromFile = new List<IconInfo>
        {
            new IconInfo("Box", "\uE8B8"),
            new IconInfo("Package", "\uE7B8"),
            new IconInfo("Archive", "\uE7B8"), // Fallback
            new IconInfo("Truck", "\uE806")
        };
    }

    private void OnShowAllIconsChanged(object sender, RoutedEventArgs e)
    {
        ApplyIconFilter();
        _filteredIcons = [.. _allIcons];
        _currentPage = 1;
        SearchBox.Text = string.Empty;
        UpdateDisplay();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(searchText))
        {
            _filteredIcons = [.. _allIcons];
        }
        else
        {
            _filteredIcons = _allIcons
                .Where(icon => icon.Name.ToLower().Contains(searchText))
                .ToList();
        }

        _currentPage = 1;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        _totalPages = (int)Math.Ceiling(_filteredIcons.Count / (double)ICONS_PER_PAGE);
        if (_totalPages == 0)
        {
            _totalPages = 1;
        }

        if (_currentPage > _totalPages)
        {
            _currentPage = _totalPages;
        }

        var currentPageIcons = _filteredIcons
            .Skip((_currentPage - 1) * ICONS_PER_PAGE)
            .Take(ICONS_PER_PAGE)
            .ToList();

        IconGridView.ItemsSource = currentPageIcons;

        if (PageInfoTextBlock != null)
        {
            PageInfoTextBlock.Text = $"Page {_currentPage} of {_totalPages}";
            FirstPageButton.IsEnabled = _currentPage > 1;
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
            LastPageButton.IsEnabled = _currentPage < _totalPages;
        }
    }

    private void OnFirstPageClick(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        UpdateDisplay();
    }

    private void OnPreviousPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            UpdateDisplay();
        }
    }

    private void OnNextPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            UpdateDisplay();
        }
    }

    private void OnLastPageClick(object sender, RoutedEventArgs e)
    {
        _currentPage = _totalPages;
        UpdateDisplay();
    }

    private void OnIconItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is IconInfo icon)
        {
            SelectedIconGlyph = icon.Glyph;
            SelectedIconName = icon.Name;
            IconWasSelected = true;
            Close();
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        IconWasSelected = false;
        Close();
    }

    public class IconInfo
    {
        public string Name { get; set; }
        public string Glyph { get; set; }

        public IconInfo(string name, string glyph)
        {
            Name = name;
            Glyph = glyph;
        }
    }

    private class JsonIconData
    {
        public string name { get; set; } = string.Empty;
        public string glyph { get; set; } = string.Empty;
        public int codepoint { get; set; }
    }
}
