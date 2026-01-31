using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_Workflow : Page
    {
        public ViewModel_Receiving_Workflow ViewModel { get; }
        private readonly IService_ReceivingWorkflow _workflowService;
        private readonly IService_Help _helpService;

        public View_Receiving_Workflow(
            ViewModel_Receiving_Workflow viewModel,
            IService_ReceivingWorkflow workflowService,
            IService_Help helpService,
            View_Receiving_ModeSelection modeSelectionView,
            View_Receiving_ManualEntry manualEntryView,
            View_Receiving_EditMode editModeView,
            View_Receiving_POEntry poEntryView,
            View_Receiving_LoadEntry loadEntryView,
            View_Receiving_WeightQuantity weightQuantityView,
            View_Receiving_HeatLot heatLotView,
            View_Receiving_PackageType packageTypeView,
            View_Receiving_Review reviewView)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(workflowService);
            ArgumentNullException.ThrowIfNull(helpService);
            ArgumentNullException.ThrowIfNull(modeSelectionView);
            ArgumentNullException.ThrowIfNull(manualEntryView);
            ArgumentNullException.ThrowIfNull(editModeView);
            ArgumentNullException.ThrowIfNull(poEntryView);
            ArgumentNullException.ThrowIfNull(loadEntryView);
            ArgumentNullException.ThrowIfNull(weightQuantityView);
            ArgumentNullException.ThrowIfNull(heatLotView);
            ArgumentNullException.ThrowIfNull(packageTypeView);
            ArgumentNullException.ThrowIfNull(reviewView);

            ViewModel = viewModel;
            _workflowService = workflowService;
            _helpService = helpService;
            this.InitializeComponent();

            ModeSelectionHost.Content = modeSelectionView;
            ManualEntryHost.Content = manualEntryView;
            EditModeHost.Content = editModeView;
            POEntryHost.Content = poEntryView;
            LoadEntryHost.Content = loadEntryView;
            WeightQuantityHost.Content = weightQuantityView;
            HeatLotHost.Content = heatLotView;
            PackageTypeHost.Content = packageTypeView;
            ReviewHost.Content = reviewView;
        }

        private async void HelpButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_helpService == null || _workflowService == null)
            {
                return;
            }

            await _helpService.ShowContextualHelpAsync(_workflowService.CurrentStep);
        }


    }
}

