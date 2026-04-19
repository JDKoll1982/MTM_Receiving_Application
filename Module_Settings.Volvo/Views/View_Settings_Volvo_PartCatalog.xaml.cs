using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_PartCatalog : Page
{
    private bool _hasLoaded;

    public ViewModel_Settings_Volvo_PartCatalog ViewModel { get; }

    public View_Settings_Volvo_PartCatalog(ViewModel_Settings_Volvo_PartCatalog viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }
}
