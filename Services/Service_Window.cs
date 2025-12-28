using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Services;

public class Service_Window : IService_Window
{
    public XamlRoot? GetXamlRoot()
    {
        return App.MainWindow?.Content?.XamlRoot;
    }
}
