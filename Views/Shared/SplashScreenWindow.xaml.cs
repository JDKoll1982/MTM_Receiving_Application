using Microsoft.UI.Xaml;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Views.Shared
{
    /// <summary>
    /// Splash screen window displayed during application startup
    /// Shows branding, progress, and status messages during initialization
    /// </summary>
    public sealed partial class SplashScreenWindow : Window
    {
        public SplashScreenViewModel ViewModel { get; }

        public SplashScreenWindow(SplashScreenViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            
            // Subscribe to ViewModel property changes
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Extend content into title bar area BEFORE centering/other operations
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);
            
            // Configure title bar appearance
            var titleBar = AppWindow.TitleBar;
            titleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
            titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonForegroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonPressedBackgroundColor = Microsoft.UI.Colors.Transparent;
            titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.Transparent;
            
            // Center window on screen after title bar configuration
            CenterWindow();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                StatusMessageTextBlock.Text = ViewModel.StatusMessage;
                MainProgressBar.Value = ViewModel.ProgressPercentage;
                MainProgressBar.IsIndeterminate = ViewModel.IsIndeterminate;
                ProgressPercentageTextBlock.Text = ViewModel.IsIndeterminate ? "" : $"{ViewModel.ProgressPercentage}%";
            });
        }

        /// <summary>
        /// Center the splash screen window on the display
        /// </summary>
        private void CenterWindow()
        {
            // Get the display area
            var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            // Calculate center position
            var centerX = (workArea.Width - 600) / 2;
            var centerY = (workArea.Height - 400) / 2;

            // Move window to center
            AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(centerX, centerY, 600, 400));
        }
    }
}
