using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Contract implemented by settings pages to handle navigation hub actions.
/// </summary>
public interface ISettingsNavigationActions
{
    Task SaveAsync();
    Task ResetAsync();
    Task CancelAsync();
    Task BackAsync();
    Task NextAsync();
}
