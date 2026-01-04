using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_PackageType : UserControl
    {
        public ViewModel_Receiving_PackageType ViewModel { get; }
        private readonly IService_Focus _focusService;

        public View_Receiving_PackageType()
        {
            ViewModel = App.GetService<ViewModel_Receiving_PackageType>();
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
