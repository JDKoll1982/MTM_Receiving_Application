namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Contract implemented by settings views or view models to control navigation hub button visibility.
/// </summary>
public interface ISettingsNavigationNavState
{
    bool IsBackVisible { get; }
    bool IsNextVisible { get; }
    bool IsCancelVisible { get; }
    bool IsSaveVisible { get; }
    bool IsResetVisible { get; }
}
