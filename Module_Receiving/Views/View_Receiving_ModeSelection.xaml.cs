using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_ModeSelection : UserControl
    {
        public ViewModel_Receiving_ModeSelection ViewModel { get; }

        public View_Receiving_ModeSelection(ViewModel_Receiving_ModeSelection viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            ViewModel = viewModel;
            DataContext = ViewModel;
            this.InitializeComponent();
        }

        private async void OnGuidedDefaultChecked(object sender, RoutedEventArgs e)
        {
            await ViewModel.HandleGuidedDefaultChangedAsync(true);
        }

        private async void OnGuidedDefaultUnchecked(object sender, RoutedEventArgs e)
        {
            await ViewModel.HandleGuidedDefaultChangedAsync(false);
        }

        private async void OnManualDefaultChecked(object sender, RoutedEventArgs e)
        {
            await ViewModel.HandleManualDefaultChangedAsync(true);
        }

        private async void OnManualDefaultUnchecked(object sender, RoutedEventArgs e)
        {
            await ViewModel.HandleManualDefaultChangedAsync(false);
        }

        private async void OnEditDefaultChecked(object sender, RoutedEventArgs e)
        {
            await ViewModel.HandleEditDefaultChangedAsync(true);
        }

        private async void OnEditDefaultUnchecked(object sender, RoutedEventArgs e)
        {
            await ViewModel.HandleEditDefaultChangedAsync(false);
        }
    }
}
