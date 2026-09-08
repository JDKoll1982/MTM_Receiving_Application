using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Scanner Settings view code-behind (view only; logic lives in the ViewModel).
/// </summary>
public sealed partial class View_Scanner_Settings : Page
{
    public ViewModel_Scanner_Settings ViewModel { get; }

    public View_Scanner_Settings(ViewModel_Scanner_Settings viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += OnLoaded;
    }

    private async void OnHelpClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var helpService = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Help>();
        await helpService.ShowHelpAsync("Scanner.Main");
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.LoadProfilesAsync();
    }
}
