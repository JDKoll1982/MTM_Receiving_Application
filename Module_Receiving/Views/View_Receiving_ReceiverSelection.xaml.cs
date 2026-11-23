using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    /// <summary>
    /// The By Receiver list. Every value on screen comes from the view model through x:Bind; the
    /// only code here is the click-to-command bridge a DataTemplate cannot express.
    /// </summary>
    public sealed partial class View_Receiving_ReceiverSelection
        : UserControl,
            IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_ReceiverSelection ViewModel { get; }

        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public View_Receiving_ReceiverSelection(
            ViewModel_Receiving_ReceiverSelection viewModel,
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

            Loaded += View_Receiving_ReceiverSelection_Loaded;
            SizeChanged += View_Receiving_ReceiverSelection_SizeChanged;
        }

        /// <summary>
        /// Puts focus on the search box whenever the list becomes the active screen.
        /// </summary>
        public bool FocusForAccess()
        {
            return _focusService.TrySetFocusAndSelectAll(ReceiverSearchTextBox);
        }

        private void View_Receiving_ReceiverSelection_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void View_Receiving_ReceiverSelection_SizeChanged(
            object sender,
            SizeChangedEventArgs e
        )
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

        private void ReceiverList_ItemClick(object sender, ItemClickEventArgs e)
        {
            _ = sender;

            if (e.ClickedItem is Model_InforVisualReceiverRow row)
            {
                ViewModel.SelectRowCommand.Execute(row);
            }
        }
    }
}
