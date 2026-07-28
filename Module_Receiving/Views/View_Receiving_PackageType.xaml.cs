using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_PackageType : UserControl, IReceivingWorkflowFocusable
    {
        public ViewModel_Receiving_PackageType ViewModel
        {
            get => (ViewModel_Receiving_PackageType)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ViewModel_Receiving_PackageType),
            typeof(View_Receiving_PackageType),
            new PropertyMetadata(null)
        );

        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public View_Receiving_PackageType(
            ViewModel_Receiving_PackageType viewModel,
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
            AttachPackagePerLoadFocus();
            Loaded += View_Receiving_PackageType_Loaded;
            SizeChanged += View_Receiving_PackageType_SizeChanged;
        }

        private void View_Receiving_PackageType_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void View_Receiving_PackageType_SizeChanged(object sender, SizeChangedEventArgs e)
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
        /// Moves focus to the package input whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            FocusFirstPackagesPerLoadInput();
        }

        private void AttachPackagePerLoadFocus()
        {
            this.Loaded += (_, _) =>
            {
                if (this.Visibility == Visibility.Visible)
                {
                    FocusFirstPackagesPerLoadInput();
                }
            };
            this.RegisterPropertyChangedCallback(
                UIElement.VisibilityProperty,
                (_, _) =>
                {
                    if (this.Visibility == Visibility.Visible)
                    {
                        FocusFirstPackagesPerLoadInput();
                    }
                }
            );
        }

        private void FocusFirstPackagesPerLoadInput()
        {
            if (this.DispatcherQueue == null || this.Visibility != Visibility.Visible)
            {
                return;
            }

            this.DispatcherQueue.TryEnqueue(() =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    var target = FindDescendant<NumberBox>(LoadsItemsControl);
                    if (target != null)
                    {
                        _focusService.SetFocus(target);
                        return;
                    }

                    _focusService.SetFocus(PackageTypeComboBox);
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
