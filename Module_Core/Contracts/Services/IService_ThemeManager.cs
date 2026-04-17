using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Manages persisted user theme preferences and applies the selected theme to the running app.
/// </summary>
public interface IService_ThemeManager
{
    Task<ElementTheme> GetSavedThemeAsync();

    Task ApplySavedThemeAsync();

    Task ApplyThemeAsync(ElementTheme theme);
}
