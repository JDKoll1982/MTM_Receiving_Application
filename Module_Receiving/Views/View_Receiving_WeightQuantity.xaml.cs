using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_WeightQuantity : UserControl, IReceivingWorkflowFocusable
    {
        private readonly Storyboard _currentTotalReminderStoryboard = new();
        private readonly SolidColorBrush _currentTotalReminderBrush = new(
            Windows.UI.Color.FromArgb(255, 153, 0, 0));
        private bool _isViewLoaded;
        private bool _pendingCurrentTotalReminderAnimation;

        public ViewModel_Receiving_WeightQuantity ViewModel
        {
            get => (ViewModel_Receiving_WeightQuantity)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(ViewModel_Receiving_WeightQuantity),
            typeof(View_Receiving_WeightQuantity),
            new PropertyMetadata(null)
        );
        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;

        public View_Receiving_WeightQuantity(
            ViewModel_Receiving_WeightQuantity viewModel,
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
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Loaded += View_Receiving_WeightQuantity_Loaded;
            SizeChanged += View_Receiving_WeightQuantity_SizeChanged;
            this.Unloaded += View_Receiving_WeightQuantity_Unloaded;
            AttachLoadFocus();
            InitializeCurrentTotalReminderAnimation();

            CurrentTotalReminderTextBlock.Foreground = _currentTotalReminderBrush;
        }

        /// <summary>
        /// Moves focus to the first weight or quantity input whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            FocusFirstLoadQuantityInput();
        }

        private void AttachLoadFocus()
        {
            this.Loaded += (_, _) =>
            {
                if (this.Visibility == Visibility.Visible)
                {
                    FocusFirstLoadQuantityInput();
                }
            };
            this.RegisterPropertyChangedCallback(
                UIElement.VisibilityProperty,
                (_, _) =>
                {
                    if (this.Visibility == Visibility.Visible)
                    {
                        FocusFirstLoadQuantityInput();
                    }
                }
            );
        }

        private void FocusFirstLoadQuantityInput()
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

                    _focusService.SetFocusFirstInput(this);
                });
            });
        }

        private void View_Receiving_WeightQuantity_Unloaded(object sender, RoutedEventArgs e)
        {
            _isViewLoaded = false;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            this.Unloaded -= View_Receiving_WeightQuantity_Unloaded;
            Loaded -= View_Receiving_WeightQuantity_Loaded;
            SizeChanged -= View_Receiving_WeightQuantity_SizeChanged;
        }

        private void View_Receiving_WeightQuantity_Loaded(object sender, RoutedEventArgs e)
        {
            _isViewLoaded = true;
            ApplyAdaptiveLayout();

            if (_pendingCurrentTotalReminderAnimation)
            {
                _pendingCurrentTotalReminderAnimation = false;
                StartCurrentTotalReminderAnimation();
            }
        }

        private void View_Receiving_WeightQuantity_SizeChanged(object sender, SizeChangedEventArgs e)
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

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel_Receiving_WeightQuantity.CurrentTotal))
            {
                TriggerCurrentTotalReminderAnimation();
            }
        }

        private void TriggerCurrentTotalReminderAnimation()
        {
            if (_isViewLoaded is false)
            {
                _pendingCurrentTotalReminderAnimation = true;
                return;
            }

            StartCurrentTotalReminderAnimation();
        }

        private void InitializeCurrentTotalReminderAnimation()
        {
            var first = new ColorAnimationUsingKeyFrames();
            Storyboard.SetTarget(first, _currentTotalReminderBrush);
            Storyboard.SetTargetProperty(first, "Color");

            first.AutoReverse = true;

            // 0ms: Start at your base Dark Red
            first.KeyFrames.Add(
                new LinearColorKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                    Value = Windows.UI.Color.FromArgb(255, 153, 0, 0),
                }
            );
            // 500ms: Dim down to Maroon (40% red intensity)
            first.KeyFrames.Add(
                new LinearColorKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(500)),
                    Value = Windows.UI.Color.FromArgb(255, 102, 0, 0),
                }
            );
            // 1000ms: Dim down to Very Dark Maroon (20% red intensity)
            first.KeyFrames.Add(
                new LinearColorKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1000)),
                    Value = Windows.UI.Color.FromArgb(255, 51, 0, 0),
                }
            );
            // 1500ms: Turn completely Black, then immediately reverse back up
            first.KeyFrames.Add(
                new DiscreteColorKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1500)),
                    Value = Windows.UI.Color.FromArgb(255, 0, 0, 0),
                }
            );

            _currentTotalReminderStoryboard.Children.Add(first);
        }


        private void StartCurrentTotalReminderAnimation()
        {
            _currentTotalReminderStoryboard.Stop();
            _currentTotalReminderStoryboard.Begin();
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
