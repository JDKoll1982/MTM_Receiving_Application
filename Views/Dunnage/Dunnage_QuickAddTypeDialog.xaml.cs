using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MTM_Receiving_Application.Models.Dunnage;
using System.Linq;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_QuickAddTypeDialog : ContentDialog, INotifyPropertyChanged
{
    private bool _isIconSelected;
    private string _selectedIcon = "\uDB81\uDF20"; // Default box icon
    private string _selectedIconName = "Box"; // Default icon name

    public string TypeName { get; private set; } = string.Empty;
    public string SelectedIconGlyph { get; private set; } = "\uDB81\uDF20";
    public ObservableCollection<Model_SpecItem> Specs { get; } = new();

    public bool IsIconSelected
    {
        get => _isIconSelected;
        set
        {
            _isIconSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Dunnage_QuickAddTypeDialog()
    {
        InitializeComponent();
        IsIconSelected = true;
        SpecsListView.ItemsSource = Specs;
        NewSpecTypeCombo.SelectionChanged += OnSpecTypeChanged;
    }

    private void OnSpecTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (NewSpecTypeCombo.SelectedItem is ComboBoxItem item)
        {
            var type = item.Content.ToString();
            NumberOptionsPanel.Visibility = type == "Number" ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    // Overload removed - use InitializeForEdit(string, string, Dictionary<string, SpecDefinition>) instead

    public void InitializeForEdit(string typeName, string iconGlyph, Dictionary<string, SpecDefinition> specs)
    {
        Title = "Edit Dunnage Type";
        PrimaryButtonText = "Save Changes";

        TypeNameTextBox.Text = typeName;
        _selectedIcon = iconGlyph;
        SelectedIconDisplay.Glyph = iconGlyph;
        SelectedIconNameText.Text = "Current Icon";

        TypeName = typeName;
        SelectedIconGlyph = iconGlyph;

        Specs.Clear();
        foreach (var kvp in specs)
        {
            Specs.Add(new Model_SpecItem
            {
                Name = kvp.Key,
                DataType = kvp.Value.DataType,
                IsRequired = kvp.Value.Required,
                Unit = kvp.Value.Unit,
                MinValue = kvp.Value.MinValue,
                MaxValue = kvp.Value.MaxValue
            });
        }
    }

    private void OnAddSpecClick(object sender, RoutedEventArgs e)
    {
        AddSpec();
    }

    private void AddSpec()
    {
        var name = NewSpecNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (Specs.Any(s => s.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var typeItem = NewSpecTypeCombo.SelectedItem as ComboBoxItem;
        var type = typeItem?.Content.ToString() ?? "Text";
        var required = NewSpecRequiredCheck.IsChecked ?? false;
        var unit = string.Empty;
        double? minValue = null;
        double? maxValue = null;

        if (type == "Number")
        {
            unit = NewSpecUnitBox.Text.Trim();
            if (!double.IsNaN(NewSpecMinValueBox.Value))
            {
                minValue = NewSpecMinValueBox.Value;
            }

            if (!double.IsNaN(NewSpecMaxValueBox.Value))
            {
                maxValue = NewSpecMaxValueBox.Value;
            }
        }

        Specs.Add(new Model_SpecItem
        {
            Name = name,
            DataType = type,
            IsRequired = required,
            Unit = unit,
            MinValue = minValue,
            MaxValue = maxValue
        });

        // Clear form
        NewSpecNameBox.Text = string.Empty;
        NewSpecUnitBox.Text = string.Empty;
        NewSpecMinValueBox.Value = double.NaN;
        NewSpecMaxValueBox.Value = double.NaN;
        NewSpecRequiredCheck.IsChecked = false;
        NewSpecTypeCombo.SelectedIndex = 0;
    }

    private void OnRemoveSpecClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Model_SpecItem spec)
        {
            Specs.Remove(spec);
        }
    }

    private async void OnSelectIconClick(object sender, RoutedEventArgs e)
    {
        var iconWindow = new Shared.Shared_IconSelectorWindow();

        // Show window and wait for it to close
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(iconWindow);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

        iconWindow.Activate();

        // Wait for window to close (polling approach)
        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        iconWindow.Closed += (s, args) => tcs.SetResult(true);
        await tcs.Task;

        // Check if icon was selected
        if (iconWindow.IconWasSelected && iconWindow.SelectedIconGlyph != null)
        {
            _selectedIcon = iconWindow.SelectedIconGlyph;
            _selectedIconName = iconWindow.SelectedIconName ?? "Icon";

            SelectedIconDisplay.Glyph = _selectedIcon;
            SelectedIconNameText.Text = _selectedIconName;
            IsIconSelected = true;
        }
    }

    private void OnTypeNameChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = (TextBox)sender;
        ValidationTextBlock.Visibility = Visibility.Collapsed; // Hide error while typing
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var input = TypeNameTextBox.Text.Trim();

        // 1. Required check
        if (string.IsNullOrWhiteSpace(input))
        {
            ShowError("Type name is required");
            args.Cancel = true;
            return;
        }

        // 2. Start with Capital Letter check
        if (!char.IsUpper(input[0]))
        {
            ShowError("Name must start with a capital letter");
            args.Cancel = true;
            return;
        }

        // 3. No spaces check
        if (input.Contains(" "))
        {
            ShowError("Name cannot contain spaces");
            args.Cancel = true;
            return;
        }

        // 4. No special characters check (Alphanumeric only)
        if (!AlphanumericRegex().IsMatch(input))
        {
            ShowError("Name cannot contain special characters");
            args.Cancel = true;
            return;
        }

        // Set properties for caller to retrieve
        TypeName = input;
        SelectedIconGlyph = _selectedIcon;
    }

    [System.Text.RegularExpressions.GeneratedRegex("^[a-zA-Z0-9]*$")]
    private static partial System.Text.RegularExpressions.Regex AlphanumericRegex();

    private void ShowError(string message)
    {
        ValidationTextBlock.Text = message;
        ValidationTextBlock.Visibility = Visibility.Visible;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
