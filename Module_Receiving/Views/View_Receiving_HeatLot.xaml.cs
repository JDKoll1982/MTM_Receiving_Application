using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_HeatLot : UserControl, IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_HeatLot ViewModel
        {
            get => (ViewModel_Receiving_HeatLot)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ViewModel_Receiving_HeatLot),
            typeof(View_Receiving_HeatLot),
            new PropertyMetadata(null)
        );
        private readonly IService_Focus _focusService;

        public View_Receiving_HeatLot(
            ViewModel_Receiving_HeatLot viewModel,
            IService_Focus focusService
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);

            ViewModel = viewModel;
            _focusService = focusService;
            DataContext = ViewModel;
            this.InitializeComponent();
            AttachLoadFocus();
        }

        /// <summary>
        /// Moves focus to the first heat or lot input whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            FocusFirstLoadHeatLotInput();
        }

        private void AttachLoadFocus()
        {
            this.Loaded += (_, _) =>
            {
                if (this.Visibility == Visibility.Visible)
                {
                    FocusFirstLoadHeatLotInput();
                }
            };
            this.RegisterPropertyChangedCallback(
                UIElement.VisibilityProperty,
                (_, _) =>
                {
                    if (this.Visibility == Visibility.Visible)
                    {
                        FocusFirstLoadHeatLotInput();
                    }
                }
            );
        }

        private void FocusFirstLoadHeatLotInput()
        {
            if (this.DispatcherQueue == null || this.Visibility != Visibility.Visible)
            {
                return;
            }

            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    var target = FindDescendant<TextBox>(LoadsItemsControl);
                    if (target != null)
                    {
                        _focusService.SetFocus(target);
                        return;
                    }

                    _focusService.SetFocusFirstInput(this);
                });
            });
        }

        private static T? FindDescendant<T>(DependencyObject parent)
            where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var index = 0; index < childCount; index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                if (child is T match)
                {
                    return match;
                }

                var nestedMatch = FindDescendant<T>(child);
                if (nestedMatch != null)
                {
                    return nestedMatch;
                }
            }

            return null;
        }
    }
}
