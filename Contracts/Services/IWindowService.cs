using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Contracts.Services;

public interface IWindowService
{
    XamlRoot? GetXamlRoot();
}
