using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using Windows.UI;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

/// <summary>
/// Picks the shell action bar's accent brush for a position.
/// </summary>
/// <remarks>
/// The bar colours a position by what it does rather than where it sits, so the choice is made
/// here and the views keep only the state. These are static functions because a window's XAML
/// cannot host a converter resource for <c>x:Bind</c>, and they run on the UI thread, which is
/// where a brush has to be made.
/// </remarks>
public static class Helper_ShellBrush
{
    /// <summary>
    /// Gets the brush for an action position: green for a save, red for a way out, and the
    /// ordinary text colour otherwise.
    /// </summary>
    /// <param name="kind">What the position is for.</param>
    /// <returns>The brush to draw that position with.</returns>
    public static Brush ForActionKind(Enum_ShellActionKind kind) =>
        kind switch
        {
            Enum_ShellActionKind.Save => Success(),
            Enum_ShellActionKind.Back => Danger(),
            _ => PrimaryText(),
        };

    /// <summary>
    /// Gets the text brush for an entry-mode control, green while that mode is the active one.
    /// </summary>
    /// <param name="isActive">True when the operator is working in this mode.</param>
    /// <returns>The brush to draw the mode's label with.</returns>
    public static Brush ModeText(bool isActive) => isActive ? Success() : PrimaryText();

    /// <summary>
    /// Gets the border brush for an entry-mode control, drawn only while that mode is active.
    /// </summary>
    /// <param name="isActive">True when the operator is working in this mode.</param>
    /// <returns>The border brush, transparent when the mode is not active.</returns>
    public static Brush ModeBorder(bool isActive) =>
        isActive ? Success() : new SolidColorBrush(Colors.Transparent);

    private static Brush Success() =>
        Resolve("ShellSuccessBrush", Color.FromArgb(255, 0x0F, 0x7B, 0x0F));

    private static Brush Danger() =>
        Resolve("ShellDangerBrush", Color.FromArgb(255, 0xC4, 0x2B, 0x1C));

    private static Brush PrimaryText() =>
        Resolve("TextFillColorPrimaryBrush", Color.FromArgb(255, 0xFF, 0xFF, 0xFF));

    private static Brush Resolve(string key, Color fallback)
    {
        if (
            Application.Current?.Resources is { } resources
            && resources.TryGetValue(key, out var resolved)
            && resolved is Brush brush
        )
        {
            return brush;
        }

        return new SolidColorBrush(fallback);
    }
}
