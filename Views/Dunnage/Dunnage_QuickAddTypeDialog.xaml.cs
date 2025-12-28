using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_QuickAddTypeDialog : ContentDialog, INotifyPropertyChanged
{
    private bool _isIconSelected;
    private string _selectedIcon = "\uE7B8"; // Default box icon
    private string _selectedIconName = "Box"; // Default icon name

    public string TypeName { get; private set; } = string.Empty;
    public string SelectedIconGlyph { get; private set; } = "\uE7B8";

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
