using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_POEntry : UserControl, IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_POEntry ViewModel { get; }
        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public View_Receiving_POEntry(
            ViewModel_Receiving_POEntry viewModel,
            IService_Focus focusService,
            IService_AdaptiveLayout adaptiveLayout
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);
            ArgumentNullException.ThrowIfNull(adaptiveLayout);

            ViewModel = viewModel;
            _focusService = focusService;
            _adaptiveLayout = adaptiveLayout;
            DataContext = ViewModel;
            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
            Loaded += View_Receiving_POEntry_Loaded;
            SizeChanged += View_Receiving_POEntry_SizeChanged;
        }

        private void View_Receiving_POEntry_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void View_Receiving_POEntry_SizeChanged(object sender, SizeChangedEventArgs e)
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

        /// <summary>
        /// Moves focus to the PO entry field whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            _focusService.SetFocus(PoNumberTextBox);
        }

        private void POTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Trigger auto-correction command
            ViewModel.PoTextBoxLostFocusCommand.Execute(null);
        }
    }
}
