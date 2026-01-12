using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Tests;

public partial class TestApp : Application
{
    public TestApp()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // No window needed for unit tests
    }
}
