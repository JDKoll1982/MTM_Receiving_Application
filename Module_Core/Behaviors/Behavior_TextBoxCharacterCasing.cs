using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Core.Behaviors
{
    /// <summary>
    /// Forces textbox input to uppercase while preserving the caret position.
    /// </summary>
    public static class Behavior_TextBoxCharacterCasing
    {
        public static readonly DependencyProperty ForceUppercaseProperty =
            DependencyProperty.RegisterAttached(
                "ForceUppercase",
                typeof(bool),
                typeof(Behavior_TextBoxCharacterCasing),
                new PropertyMetadata(false, OnForceUppercaseChanged)
            );

        private static readonly DependencyProperty IsUpdatingTextProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdatingText",
                typeof(bool),
                typeof(Behavior_TextBoxCharacterCasing),
                new PropertyMetadata(false)
            );

        public static bool GetForceUppercase(DependencyObject obj)
        {
            return (bool)obj.GetValue(ForceUppercaseProperty);
        }

        public static void SetForceUppercase(DependencyObject obj, bool value)
        {
            obj.SetValue(ForceUppercaseProperty, value);
        }

        private static bool GetIsUpdatingText(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsUpdatingTextProperty);
        }

        private static void SetIsUpdatingText(DependencyObject obj, bool value)
        {
            obj.SetValue(IsUpdatingTextProperty, value);
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
            var upperText = currentText.ToUpperInvariant();
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
    }
}