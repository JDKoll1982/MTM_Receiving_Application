using System;
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
        ViewModel = App.GetService<ViewModel_Dunnage_ModeSelection>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
