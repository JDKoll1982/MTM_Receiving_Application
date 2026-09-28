using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    public interface IService_Focus
    {
        /// <summary>
        /// Sets focus to the specified control.
        /// </summary>
        /// <param name="control">The control to focus.</param>
        public void SetFocus(Control control);

        /// <summary>
        /// Finds the first focusable input control (TextBox, NumberBox, ComboBox, Button, etc.)
        /// within the container and sets focus to it.
        /// </summary>
        /// <param name="container">The container to search.</param>
        public void SetFocusFirstInput(DependencyObject container);

        /// <summary>
        /// Attaches a listener to the element's Visibility property.
        /// When the element becomes Visible, it sets focus to the target control
        /// (or the first input if target is null).
        /// </summary>
        /// <param name="view">The view/element to monitor for visibility changes.</param>
        /// <param name="targetControl">Optional specific control to focus. If null, searches for first input.</param>
        public void AttachFocusOnVisibility(FrameworkElement view, Control? targetControl = null);

        /// <summary>
        /// Focuses the control synchronously and reports whether focus was applied.
        /// Returns <see langword="false"/> when the control is not yet loaded, visible, or enabled,
        /// which lets callers retry after the next layout pass.
        /// </summary>
        /// <param name="control">The control to focus.</param>
        public bool TrySetFocus(Control control);

        /// <summary>
        /// Focuses the text box synchronously, selects its full contents, and reports whether focus was applied.
        /// </summary>
        /// <param name="textBox">The text box to focus and select.</param>
        public bool TrySetFocusAndSelectAll(TextBox textBox);
    }
}
