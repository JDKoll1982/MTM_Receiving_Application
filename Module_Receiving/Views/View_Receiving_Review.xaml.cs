using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using WinRT.Interop;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_Review : UserControl, IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_Review ViewModel { get; }
        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public View_Receiving_Review(
            ViewModel_Receiving_Review viewModel,
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
            ViewModel.RequestOpenLabelOverlay = ShowLabelOverlayAsync;
            Loaded += View_Receiving_Review_Loaded;
            SizeChanged += View_Receiving_Review_SizeChanged;
        }

        /// <summary>
        /// Opens the label overlay window over the main window, with the labels from the receipt that was
        /// just saved already ticked. Built here rather than in the ViewModel so the ViewModel stays free of
        /// UI types.
        /// </summary>
        private async Task ShowLabelOverlayAsync(
            IReadOnlyList<(string PartId, string HeatLotNumber)> lots
        )
        {
            if (App.MainWindow is not { } owner)
            {
                return;
            }

            var overlayViewModel = App.GetService<ViewModel_Receiving_LabelOverlay>();
            overlayViewModel.RequestPreselection(lots);

            var window = new View_Receiving_LabelOverlayWindow(
                overlayViewModel,
                WindowNative.GetWindowHandle(owner)
            );

            await window.ShowModalAsync();
        }

        /// <summary>
        /// Moves focus to the Save to Database button whenever guided mode re-enters this step.
        /// </summary>
        public bool FocusForAccess()
        {
            return _focusService.TrySetFocus(SaveToDatabaseButton);
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
