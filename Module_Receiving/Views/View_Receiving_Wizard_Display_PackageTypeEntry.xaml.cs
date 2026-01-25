using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_PackageType : UserControl
    {
        public ViewModel_Receiving_Wizard_Display_PackageTypeEntry ViewModel
        {
            get => (ViewModel_Receiving_Wizard_Display_PackageTypeEntry)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModel_Receiving_Wizard_Display_PackageTypeEntry),
                typeof(View_Receiving_PackageType),
                new PropertyMetadata(null));
        
        private readonly IService_Focus _focusService;

    public View_Receiving_PackageType(
        ViewModel_Receiving_Wizard_Display_PackageTypeEntry viewModel,
        IService_Focus focusService)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);

        ViewModel = viewModel;
        _focusService = focusService;
        
        this.InitializeComponent();

        this.Loaded += PackageTypeView_Loaded;
        _focusService.AttachFocusOnVisibility(this, PackageTypeComboBox);
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Receiving_PackageType()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_PackageTypeEntry>();
        _focusService = App.GetService<IService_Focus>();
        this.InitializeComponent();
        this.Loaded += PackageTypeView_Loaded;
        _focusService.AttachFocusOnVisibility(this, PackageTypeComboBox);
    }

        private async void PackageTypeView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }
}
