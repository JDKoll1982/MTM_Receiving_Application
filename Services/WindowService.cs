using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Services;

public class WindowService : IWindowService
{
    public XamlRoot? GetXamlRoot()
    {
        return App.MainWindow?.Content?.XamlRoot;
    }
}
