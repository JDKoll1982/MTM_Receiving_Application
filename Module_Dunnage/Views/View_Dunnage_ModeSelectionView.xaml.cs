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

    private async void OpenImageSearch_Click(object sender, RoutedEventArgs e)
    {
        var dialog = App.GetService<View_Dunnage_Dialog_ImagePartSearch>();
        if (dialog is null)
        {
            return;
        }

        dialog.XamlRoot = XamlRoot;
        await dialog.ShowAsync();
    }
}
