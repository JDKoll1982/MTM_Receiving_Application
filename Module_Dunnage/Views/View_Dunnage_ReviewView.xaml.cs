using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_ReviewView : UserControl
{
    public ViewModel_Dunnage_Review ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_ReviewView(
        ViewModel_Dunnage_Review viewModel,
        IService_Focus focusService)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);

        ViewModel = viewModel;
        _focusService = focusService;

        InitializeComponent();
        DataContext = ViewModel;

        _focusService.AttachFocusOnVisibility(this);
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Dunnage_ReviewView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_Review>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();
        DataContext = ViewModel;
        _focusService.AttachFocusOnVisibility(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadSessionLoads();
    }
}
