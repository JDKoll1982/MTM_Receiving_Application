using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminTypesView : Page
{
    public ViewModel_Dunnage_AdminTypes ViewModel { get; }
    private readonly IService_Focus _focusService;
    private readonly IService_LoggingUtility _loggingUtility;

    public View_Dunnage_AdminTypesView(
        ViewModel_Dunnage_AdminTypes viewModel,
        IService_Focus focusService,
        IService_LoggingUtility loggingUtility)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);
        ArgumentNullException.ThrowIfNull(loggingUtility);

        ViewModel = viewModel;
        _focusService = focusService;
        _loggingUtility = loggingUtility;

        InitializeComponent();
        DataContext = ViewModel;

        _focusService.AttachFocusOnVisibility(this);
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Dunnage_AdminTypesView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminTypes>();
        _focusService = App.GetService<IService_Focus>();
        _loggingUtility = App.GetService<IService_LoggingUtility>();
        InitializeComponent();
        DataContext = ViewModel;
        _focusService.AttachFocusOnVisibility(this);
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        _loggingUtility.LogInfo("Admin Types View loaded", "AdminTypesView");
        await ViewModel.LoadTypesCommand.ExecuteAsync(null);
    }
}

