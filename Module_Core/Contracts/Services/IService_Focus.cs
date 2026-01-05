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
    }
}
