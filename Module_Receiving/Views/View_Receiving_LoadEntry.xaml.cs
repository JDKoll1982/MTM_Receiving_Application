using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_LoadEntry : UserControl
    {
        public ViewModel_Receiving_LoadEntry ViewModel { get; }
        private readonly IService_Focus _focusService;

        public View_Receiving_LoadEntry(
            ViewModel_Receiving_LoadEntry viewModel,
            IService_Focus focusService)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);

            ViewModel = viewModel;
            _focusService = focusService;
            DataContext = ViewModel;
            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
        }
    }
}
