/// <summary>
/// View_Receiving_HeatLot.xaml.cs
/// Last Updated: 6/29/2026 7:47:10 PM
/// By: JOHNSPC\johnk
/// </summary>

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    /// <summary>
    /// Defines the <see cref="View_Receiving_HeatLot" />.
    /// </summary>
    public sealed partial class View_Receiving_HeatLot : UserControl, IReceivingWorkflowFocusable
    {
        /// <summary>
        /// Gets the ViewModel.
        /// </summary>
        public ViewModel_Receiving_HeatLot ViewModel { get => (ViewModel_Receiving_HeatLot)GetValue(ViewModelProperty); private set => SetValue(ViewModelProperty, value); }

        /// <summary>
        /// Defines the ViewModelProperty.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ViewModel_Receiving_HeatLot),
            typeof(View_Receiving_HeatLot),
            new PropertyMetadata(null)
        );

        /// <summary>
        /// Defines the _focusService.
        /// </summary>
        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        /// <summary>
        /// Initializes a new instance of the <see cref="View_Receiving_HeatLot"/> class.
        /// </summary>
        /// <param name="viewModel">The viewModel<see cref="ViewModel_Receiving_HeatLot"/>.</param>
        /// <param name="focusService">The focusService<see cref="IService_Focus"/>.</param>
        public View_Receiving_HeatLot(
            ViewModel_Receiving_HeatLot viewModel,
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
            Loaded += View_Receiving_HeatLot_Loaded;
            SizeChanged += View_Receiving_HeatLot_SizeChanged;
        }

        private void View_Receiving_HeatLot_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void View_Receiving_HeatLot_SizeChanged(object sender, SizeChangedEventArgs e)
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
        /// Moves focus to the first heat or lot input whenever guided mode re-enters this step.
        /// </summary>
        public bool FocusForAccess()
        {
            return FocusFirstLoadHeatLotInput();
        }

        /// <summary>
        /// Moves focus to the first heat or lot input.
        /// </summary>
        private bool FocusFirstLoadHeatLotInput()
        {
            if (this.Visibility != Visibility.Visible)
            {
                return false;
            }

            var target = FindDescendant<TextBox>(LoadsItemsControl);
            return target is not null && _focusService.TrySetFocus(target);
        }

        /// <summary>
        /// Finds the first descendant of the specified type within the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of descendant to find.</typeparam>
        /// <param name="parent">The parent <see cref="DependencyObject"/>.</param>
        /// <returns>The first descendant of type <see cref="T"/> if found; otherwise, <c>null</c>.</returns>
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
