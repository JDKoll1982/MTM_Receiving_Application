using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Material.Icons;
using MTM_Receiving_Application.Helpers.UI;
using MTM_Receiving_Application.Contracts.Services;
using System.Diagnostics;

namespace MTM_Receiving_Application.Views.Shared;

public sealed partial class Shared_IconSelectorWindow : Window
{
    private readonly IService_LoggingUtility? _loggingService;
    private List<IconInfo> _allIcons = new();
    private List<IconInfo> _commonIcons = new();
    private List<IconInfo> _filteredIcons = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int ICONS_PER_PAGE = 16;

    public MaterialIconKind? SelectedIconKind { get; private set; }
    public string? SelectedIconName { get; private set; }
    public bool IconWasSelected { get; private set; }

    private TaskCompletionSource<MaterialIconKind?>? _selectionTaskCompletionSource;

    public Shared_IconSelectorWindow()
    {
        InitializeComponent();

        try
        {
            _loggingService = App.GetService<IService_LoggingUtility>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to get logging service: {ex.Message}");
        }

        // Custom Title Bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        LoadIcons();

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

        // Handle window closing to complete the task
        this.Closed += (s, e) =>
        {
            _selectionTaskCompletionSource?.TrySetResult(IconWasSelected ? SelectedIconKind : null);
        };
    }

    /// <summary>
    /// Sets the initial icon selection in the grid
    /// </summary>
    public void SetInitialSelection(MaterialIconKind iconKind)
    {
        SelectedIconKind = iconKind;

        // Find and select the icon in the grid after it loads
        DispatcherQueue.TryEnqueue(() =>
        {
            var iconInfo = _allIcons.FirstOrDefault(i => i.Kind == iconKind);
            if (iconInfo != null)
            {
                IconGridView.SelectedItem = iconInfo;
                IconGridView.ScrollIntoView(iconInfo);
            }
        });
    }

    /// <summary>
    /// Waits for the user to select an icon or close the window
    /// </summary>
    public Task<MaterialIconKind?> WaitForSelectionAsync()
    {
        _selectionTaskCompletionSource = new TaskCompletionSource<MaterialIconKind?>();
        return _selectionTaskCompletionSource.Task;
    }

    private void LoadIcons()
    {
        try
        {
            var icons = Helper_MaterialIcons.GetAllIcons();
            _allIcons = icons.ConvertAll(k => new IconInfo(k.ToString(), k));

            // Define common/curated icons for dunnage and packaging
            var commonIconNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PackageVariantClosed", "PackageVariant", "Package", "CubeOutline", "Cube",
                "Archive", "ArchiveOutline", "Box", "BoxShadow",
                "Pallet", "ForkliftBox",
                "TruckDelivery", "TruckCargo", "Truck",
                "ScaleBalance", "Weight", "WeightKilogram", "WeightPound",
                "RulerSquare", "Ruler", "Tape",
                "Label", "LabelOutline", "Barcode", "QrcodeEdit", "QrcodeScan",
                "BubbleSheet", "Wrap",
                "Layers", "LayersTriple", "StackExchange", "StackOverflow",
                "FormatWrap", "SafetyGoggles", "HardHat",
                "Tag", "TagMultiple", "TagOutline",
                "WoodBoard", "Cardboard", "CardboardBox",
                "ShieldCheck", "Shield", "Security",
                "CalendarCheck", "Calendar", "ClockOutline",
                "AlertCircle", "Alert", "Information", "CheckCircle"
            };

            _commonIcons = _allIcons
                .Where(i => commonIconNames.Contains(i.Name))
                .ToList();

            // Start with common icons (unless "Show all" is checked)
            _filteredIcons = ShowAllIconsCheckBox?.IsChecked == true
                ? new List<IconInfo>(_allIcons)
                : new List<IconInfo>(_commonIcons);

            UpdateGridView();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("Error loading icons", ex);
            Debug.WriteLine($"Error loading icons: {ex}");
        }
    }

    private void UpdateGridView()
    {
        _totalPages = (int)Math.Ceiling((double)_filteredIcons.Count / ICONS_PER_PAGE);
        if (_totalPages == 0)
        {
            _totalPages = 1;
        }

        if (_currentPage > _totalPages)
        {
            _currentPage = _totalPages;
        }

        if (_currentPage < 1)
        {
            _currentPage = 1;
        }

        var pagedIcons = _filteredIcons
            .Skip((_currentPage - 1) * ICONS_PER_PAGE)
            .Take(ICONS_PER_PAGE)
            .ToList();

        IconGridView.ItemsSource = pagedIcons;

        // Update pagination controls
        if (PageInfoTextBlock != null)
        {
            PageInfoTextBlock.Text = $"Page {_currentPage} of {_totalPages}";
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
            FirstPageButton.IsEnabled = _currentPage > 1;
            LastPageButton.IsEnabled = _currentPage < _totalPages;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower().Trim();
        var sourceList = ShowAllIconsCheckBox?.IsChecked == true ? _allIcons : _commonIcons;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            _filteredIcons = new List<IconInfo>(sourceList);
        }
        else
        {
            _filteredIcons = sourceList
                .Where(i => i.Name.ToLower().Contains(searchText))
                .ToList();
        }

        _currentPage = 1;
        UpdateGridView();
    }

    private void OnShowAllIconsChanged(object sender, RoutedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower().Trim();
        var sourceList = ShowAllIconsCheckBox?.IsChecked == true ? _allIcons : _commonIcons;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            _filteredIcons = new List<IconInfo>(sourceList);
        }
        else
        {
            _filteredIcons = sourceList
                .Where(i => i.Name.ToLower().Contains(searchText))
                .ToList();
        }

        _currentPage = 1;
        UpdateGridView();
    }

    private void OnIconItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is IconInfo icon)
        {
            SelectedIconKind = icon.Kind;
            SelectedIconName = icon.Name;
            IconWasSelected = true;
            Close();
        }
    }

    private void OnPreviousPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            UpdateGridView();
        }
    }

    private void OnNextPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            UpdateGridView();
        }
    }

    private void OnFirstPageClick(object sender, RoutedEventArgs e)
    {
        _currentPage = 1;
        UpdateGridView();
    }

    private void OnLastPageClick(object sender, RoutedEventArgs e)
    {
        _currentPage = _totalPages;
        UpdateGridView();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        IconWasSelected = false;
        Close();
    }

    public class IconInfo
    {
        public string Name { get; set; }
        public MaterialIconKind Kind { get; set; }

        public IconInfo(string name, MaterialIconKind kind)
        {
            Name = name;
            Kind = kind;
        }
    }
}
