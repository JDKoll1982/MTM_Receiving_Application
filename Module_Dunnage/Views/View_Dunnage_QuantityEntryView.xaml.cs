using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuantityEntryView : UserControl
{
    public ViewModel_Dunnage_QuantityEntry ViewModel { get; }
    private readonly IService_Focus _focusService;
    private readonly Dictionary<TextBox, NumberBox> _trackedInputBoxes = new();
    private bool _isUpdatingImmediateInput;

    public View_Dunnage_QuantityEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_QuantityEntry>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadContextData();
    }

    private void NumberOfLoadsNumberBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is NumberBox numberBox)
        {
            AttachImmediateIntegerHandler(numberBox);
        }
    }

    private void LoadQuantityNumberBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is NumberBox numberBox)
        {
            AttachImmediateIntegerHandler(numberBox);
        }
    }

    private void IntegerNumberBox_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not NumberBox numberBox)
        {
            return;
        }

        var inputBox = FindDescendant<TextBox>(numberBox);
        if (inputBox is null)
        {
            return;
        }

        inputBox.TextChanging -= IntegerInputBox_TextChanging;
        _trackedInputBoxes.Remove(inputBox);
    }

    private void AttachImmediateIntegerHandler(NumberBox numberBox)
    {
        var inputBox = FindDescendant<TextBox>(numberBox);
        if (inputBox is null)
        {
            return;
        }

        _trackedInputBoxes[inputBox] = numberBox;
        inputBox.TextChanging -= IntegerInputBox_TextChanging;
        inputBox.TextChanging += IntegerInputBox_TextChanging;
    }

    private void IntegerInputBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        if (_isUpdatingImmediateInput || !_trackedInputBoxes.TryGetValue(sender, out var numberBox))
        {
            return;
        }

        var sanitizedText = SanitizeIntegerText(sender.Text);
        if (!string.Equals(sender.Text, sanitizedText, System.StringComparison.Ordinal))
        {
            _isUpdatingImmediateInput = true;
            sender.Text = sanitizedText;
            sender.SelectionStart = sender.Text.Length;
            _isUpdatingImmediateInput = false;
        }

        if (ReferenceEquals(numberBox, NumberOfLoadsNumberBox))
        {
            if (
                int.TryParse(
                    sanitizedText,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var numberOfLoads
                )
                && numberOfLoads > 0
            )
            {
                ViewModel.NumberOfLoads = numberOfLoads;
            }

            return;
        }

        if (numberBox.DataContext is not Model_DunnageLoad load)
        {
            return;
        }

        if (
            int.TryParse(
                sanitizedText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var quantity
            )
        )
        {
            load.Quantity = quantity;
        }
        else if (string.IsNullOrWhiteSpace(sanitizedText))
        {
            load.Quantity = 0;
        }
    }

    private static string SanitizeIntegerText(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        var childCount = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < childCount; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }
}
