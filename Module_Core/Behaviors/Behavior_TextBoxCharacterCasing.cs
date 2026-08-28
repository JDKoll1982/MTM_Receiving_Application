using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Core.Behaviors
{
    /// <summary>
    /// Forces textbox input to uppercase while preserving the caret position.
    /// </summary>
    public static class Behavior_TextBoxCharacterCasing
    {
        private static readonly StringComparer Comparison = StringComparer.OrdinalIgnoreCase;
        private static readonly HashSet<string> PresetHeatLotExceptions = new(
            StringComparer.OrdinalIgnoreCase)
        {
            "Refer to Vendor Tag",
            "N/A",
            "Old Coil",
            "Old Flatstock",
            "Old Product",
        };

        // Lazy registration avoids WinRT DependencyProperty init in non-XAML hosts (unit tests).
        private static readonly Lazy<DependencyProperty> ForceUppercaseProperty = new(
            () =>
                DependencyProperty.RegisterAttached(
                    "ForceUppercase",
                    typeof(bool),
                    typeof(Behavior_TextBoxCharacterCasing),
                    new PropertyMetadata(false, OnForceUppercaseChanged)
                )
        );

        private static readonly Lazy<DependencyProperty> IsUpdatingTextProperty = new(
            () =>
                DependencyProperty.RegisterAttached(
                    "IsUpdatingText",
                    typeof(bool),
                    typeof(Behavior_TextBoxCharacterCasing),
                    new PropertyMetadata(false)
                )
        );

        public static bool GetForceUppercase(DependencyObject obj)
        {
            return (bool)obj.GetValue(ForceUppercaseProperty.Value);
        }

        public static void SetForceUppercase(DependencyObject obj, bool value)
        {
            obj.SetValue(ForceUppercaseProperty.Value, value);
        }

        private static bool GetIsUpdatingText(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsUpdatingTextProperty.Value);
        }

        private static void SetIsUpdatingText(DependencyObject obj, bool value)
        {
            obj.SetValue(IsUpdatingTextProperty.Value, value);
        }

        private static void OnForceUppercaseChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args
        )
        {
            if (dependencyObject is not TextBox textBox)
            {
                return;
            }

            textBox.TextChanging -= TextBox_TextChanging;

            if (args.NewValue is true)
            {
                textBox.TextChanging += TextBox_TextChanging;
            }
        }

        private static void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (GetIsUpdatingText(sender))
            {
                return;
            }

            var currentText = sender.Text ?? string.Empty;
            var upperText = NormalizeText(currentText);
            if (string.Equals(currentText, upperText, StringComparison.Ordinal))
            {
                return;
            }

            var selectionStart = sender.SelectionStart;
            var selectionLength = sender.SelectionLength;

            SetIsUpdatingText(sender, true);
            sender.Text = upperText;
            sender.SelectionStart = selectionStart;
            sender.SelectionLength = selectionLength;
            SetIsUpdatingText(sender, false);
        }

        /// <summary>
        /// Returns the forced-uppercase text unless the value matches one of the exception values.
        /// </summary>
        /// <param name="text">The current text value.</param>
        /// <param name="uppercaseExceptions">Values that should preserve their original casing.</param>
        /// <returns>The normalized text value.</returns>
        public static string NormalizeText(string? text, IEnumerable<string>? uppercaseExceptions)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var trimmedText = text.Trim();
            var exceptions = uppercaseExceptions ?? PresetHeatLotExceptions;
            if (MatchesException(trimmedText, exceptions))
            {
                return trimmedText;
            }

            return trimmedText.ToUpperInvariant();
        }

        /// <summary>
        /// Returns the default forced-uppercase text behavior with built-in Heat/Lot exceptions.
        /// </summary>
        /// <param name="text">The current text value.</param>
        /// <returns>The normalized text value.</returns>
        public static string NormalizeText(string? text)
        {
            return NormalizeText(text, PresetHeatLotExceptions);
        }

        private static bool MatchesException(string text, IEnumerable<string>? uppercaseExceptions)
        {
            if (uppercaseExceptions is null)
            {
                return false;
            }

            foreach (var uppercaseException in uppercaseExceptions)
            {
                if (!string.IsNullOrWhiteSpace(uppercaseException) && Comparison.Equals(text, uppercaseException.Trim()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
