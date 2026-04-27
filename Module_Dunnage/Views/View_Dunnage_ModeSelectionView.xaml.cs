using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// UserControl for Dunnage Mode Selection view
/// </summary>
public sealed partial class View_Dunnage_ModeSelectionView : UserControl
{
    public ViewModel_Dunnage_ModeSelection ViewModel { get; }

    public View_Dunnage_ModeSelectionView(ViewModel_Dunnage_ModeSelection viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void OnGuidedDefaultChecked(object sender, RoutedEventArgs e)
    {
        await ViewModel.HandleGuidedDefaultChangedAsync(true);
    }

    private async void OnGuidedDefaultUnchecked(object sender, RoutedEventArgs e)
    {
        await ViewModel.HandleGuidedDefaultChangedAsync(false);
    }

    private async void OnEditDefaultChecked(object sender, RoutedEventArgs e)
    {
        await ViewModel.HandleEditDefaultChangedAsync(true);
    }

    private async void OnEditDefaultUnchecked(object sender, RoutedEventArgs e)
    {
        await ViewModel.HandleEditDefaultChangedAsync(false);
    }

    private async void OnImageSearchDefaultChecked(object sender, RoutedEventArgs e)
    {
        await ViewModel.HandleImageSearchDefaultChangedAsync(true);
    }

    private async void OnImageSearchDefaultUnchecked(object sender, RoutedEventArgs e)
    {
        await ViewModel.HandleImageSearchDefaultChangedAsync(false);
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Dunnage_ModeSelectionView()
    {
        var viewModel = App.GetService<ViewModel_Dunnage_ModeSelection>();
        if (viewModel == null)
        {
            throw new InvalidOperationException(
                "ViewModel_Dunnage_ModeSelection could not be resolved from DI container. "
                    + "Ensure the ViewModel and all its dependencies are registered in ModuleServicesExtensions.cs"
            );
        }
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
