using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_LoadEntry : UserControl
    {
        public ViewModel_Receiving_LoadEntry ViewModel { get; }

        private readonly IService_Focus _focusService;

        public View_Receiving_LoadEntry(
            ViewModel_Receiving_LoadEntry viewModel,
            IService_Focus focusService
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);

            ViewModel = viewModel;
            _focusService = focusService;
            DataContext = ViewModel;
            InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, NumberOfLoadsNumberBox);
        }

        private async void LocationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox || ViewModel.IsMockLocationMode)
            {
                return;
            }

            var validation = await ViewModel.ValidateLocationAsync();
            if (validation.IsValid)
            {
                return;
            }

            ViewModel.ShowStatus(
                validation.Message,
                MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            textBox.Focus(FocusState.Programmatic);
            textBox.SelectAll();
        }
    }
}
