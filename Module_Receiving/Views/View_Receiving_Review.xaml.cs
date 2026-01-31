using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_Review : UserControl
    {
        public ViewModel_Receiving_Review ViewModel { get; }

        public View_Receiving_Review(ViewModel_Receiving_Review viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            ViewModel = viewModel;
            DataContext = ViewModel;
            this.InitializeComponent();
        }
    }
}
