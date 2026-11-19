using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

/// <summary>
/// Turns a flag into a <see cref="Visibility"/> for compiled bindings.
/// </summary>
/// <remarks>
/// <c>x:Bind</c> function bindings can call a static method from a window's XAML even though the
/// window root cannot host a converter resource.
/// </remarks>
public static class Helper_Visibility
{
    /// <summary>
    /// Converts a flag into a visibility.
    /// </summary>
    /// <param name="isVisible">True to show the element.</param>
    /// <returns>Visible when the flag is set, otherwise collapsed.</returns>
    public static Visibility FromBool(bool isVisible) =>
        isVisible ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Converts a nullable flag into a visibility.
    /// </summary>
    /// <param name="isVisible">True to show the element.</param>
    /// <returns>Visible when the flag is true, otherwise collapsed.</returns>
    public static Visibility FromBool(bool? isVisible) =>
        isVisible == true ? Visibility.Visible : Visibility.Collapsed;
}
