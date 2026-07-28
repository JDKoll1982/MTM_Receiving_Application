using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_Review : UserControl
    {
        public ViewModel_Receiving_Review ViewModel { get; }
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public View_Receiving_Review(
            ViewModel_Receiving_Review viewModel,
            IService_AdaptiveLayout adaptiveLayout
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(adaptiveLayout);

            ViewModel = viewModel;
            _adaptiveLayout = adaptiveLayout;
            DataContext = ViewModel;
            this.InitializeComponent();
            Loaded += View_Receiving_Review_Loaded;
            SizeChanged += View_Receiving_Review_SizeChanged;
        }

        private void View_Receiving_Review_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void View_Receiving_Review_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void ApplyAdaptiveLayout()
        {
            var state = _adaptiveLayout.ResolveReceivingLayoutState(ActualWidth);
            _ = VisualStateManager.GoToState(this, state, false);
        }
    }
}
