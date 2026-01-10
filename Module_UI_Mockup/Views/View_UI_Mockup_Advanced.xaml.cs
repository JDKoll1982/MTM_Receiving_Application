using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup.Views;

public sealed partial class View_UI_Mockup_Advanced : Page
{
    public ViewModel_UI_Mockup_Advanced ViewModel { get; }

    public View_UI_Mockup_Advanced()
    {
        ViewModel = new ViewModel_UI_Mockup_Advanced();
        InitializeComponent();
    }

    private async void RefreshControl_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        // Get deferral to perform async work
        using var deferral = args.GetDeferral();

        // Simulate refresh operation
        await Task.Delay(2000);

        // Deferral completes automatically when disposed
    }
}