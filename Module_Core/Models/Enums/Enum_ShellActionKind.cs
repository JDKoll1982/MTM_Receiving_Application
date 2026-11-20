namespace MTM_Receiving_Application.Module_Core.Models.Enums;

/// <summary>
/// What a shell action position is for, so the bar can colour it.
/// </summary>
/// <remarks>
/// The position alone does not say enough: the Back slot carries a page's Start New Entry on the
/// Result page, and the Next slot carries Save verbs, so the bar styles by intent rather than by
/// slot.
/// </remarks>
public enum Enum_ShellActionKind
{
    /// <summary>The position carries a neutral action, such as Next or Start New Entry.</summary>
    Plain,

    /// <summary>The position goes back a step.</summary>
    Back,

    /// <summary>The position saves, so it reads as the primary action.</summary>
    Save,
}
