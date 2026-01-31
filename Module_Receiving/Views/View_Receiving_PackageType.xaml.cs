using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_PackageType : UserControl
    {
        public ViewModel_Receiving_PackageType ViewModel
        {
            get => (ViewModel_Receiving_PackageType)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModel_Receiving_PackageType),
                typeof(View_Receiving_PackageType),
                new PropertyMetadata(null));

        private readonly IService_Focus _focusService;

        public View_Receiving_PackageType(
            ViewModel_Receiving_PackageType viewModel,
            IService_Focus focusService)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);

            ViewModel = viewModel;
            _focusService = focusService;
            DataContext = ViewModel;

            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, PackageTypeComboBox);
        }
    }
}
