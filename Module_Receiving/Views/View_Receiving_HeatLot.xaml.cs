using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_HeatLot : UserControl
    {
        public ViewModel_Receiving_HeatLot ViewModel
        {
            get => (ViewModel_Receiving_HeatLot)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModel_Receiving_HeatLot),
                typeof(View_Receiving_HeatLot),
                new PropertyMetadata(null));
        private readonly IService_Focus _focusService;

        public View_Receiving_HeatLot(
            ViewModel_Receiving_HeatLot viewModel,
            IService_Focus focusService)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);

            ViewModel = viewModel;
            _focusService = focusService;
            DataContext = ViewModel;
            this.InitializeComponent();
            _focusService.AttachFocusOnVisibility(this);
        }
    }
}
