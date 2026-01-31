using System;
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
    }
}
