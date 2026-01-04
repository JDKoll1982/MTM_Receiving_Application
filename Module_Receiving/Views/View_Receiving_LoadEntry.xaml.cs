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

        public View_Receiving_LoadEntry()
        {
            ViewModel = App.GetService<ViewModel_Receiving_LoadEntry>();
            _focusService = App.GetService<IService_Focus>();
            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, LoadCountBox);
        }
    }
}
