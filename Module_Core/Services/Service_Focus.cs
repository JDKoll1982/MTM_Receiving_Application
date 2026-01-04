using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using System;

namespace MTM_Receiving_Application.Module_Core.Services
{
    public class Service_Focus : IService_Focus
    {
        public void SetFocus(Control control)
        {
            if (control == null)
                return;

            // Ensure we are on the UI thread or use DispatcherQueue
            if (control.DispatcherQueue != null)
            {
                control.DispatcherQueue.TryEnqueue(() =>
                {
                    control.Focus(FocusState.Programmatic);
                });
            }
            else
            {
                control.Focus(FocusState.Programmatic);
            }
        }

        public void SetFocusFirstInput(DependencyObject container)
        {
            if (container == null)
                return;

            if (container is FrameworkElement fe && fe.DispatcherQueue != null)
            {
                fe.DispatcherQueue.TryEnqueue(() =>
                {
                    var control = FindFirstFocusableChild(container);
                    if (control != null)
                    {
                        control.Focus(FocusState.Programmatic);
                    }
                });
            }
        }

        public void AttachFocusOnVisibility(FrameworkElement view, Control? targetControl = null)
        {
            if (view == null)
                return;

            // Initial check if already visible and loaded
            if (view.IsLoaded && view.Visibility == Visibility.Visible)
            {
                if (targetControl != null)
                {
                    SetFocus(targetControl);
                }
                else
                {
                    SetFocusFirstInput(view);
                }
            }

            // Hook into Loaded to handle initial focus
            view.Loaded += (s, e) =>
            {
                if (view.Visibility == Visibility.Visible)
                {
                    if (targetControl != null)
                    {
                        SetFocus(targetControl);
                    }
                    else
                    {
                        SetFocusFirstInput(view);
                    }
                }
            };

            // Hook into Visibility changes
            view.RegisterPropertyChangedCallback(UIElement.VisibilityProperty, (s, dp) =>
            {
                if (view.Visibility == Visibility.Visible)
                {
                    if (targetControl != null)
                    {
                        SetFocus(targetControl);
                    }
                    else
                    {
                        SetFocusFirstInput(view);
                    }
                }
            });
        }

        private Control? FindFirstFocusableChild(DependencyObject parent)
        {
            if (parent == null)
                return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Control control && IsFocusable(control))
                {
                    return control;
                }

                var result = FindFirstFocusableChild(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private bool IsFocusable(Control control)
        {
            if (!control.IsEnabled || control.Visibility != Visibility.Visible)
            {
                return false;
            }

            // Check for specific input types we want to prioritize
            if (control is TextBox ||
                control is NumberBox ||
                control is ComboBox ||
                control is PasswordBox ||
                control is AutoSuggestBox)
            {
                return true;
            }

            // Buttons and other controls are also focusable, but maybe we prefer inputs?
            // For now, let's include them but maybe inputs should be prioritized if we were doing a deeper search.
            // But since we are doing a depth-first search (or breadth-first?), the order matters.
            // The current implementation is depth-first.

            if (control is Button ||
                control is CheckBox ||
                control is RadioButton ||
                control is ToggleSwitch ||
                control is ListView ||
                control is GridView)
            {
                return true;
            }

            return false;
        }
    }
}
