using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Helpers.UI;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Views.Shared
{
    /// <summary>
    /// Splash screen window displayed during application startup
    /// Shows branding, progress, and status messages during initialization
    /// Window size: 500x450 pixels (optimized for dialog display)
    /// </summary>
    public sealed partial class SplashScreenWindow : Window
    {
        private const int WindowWidth = 850;
        private const int WindowHeight = 700;

        public SplashScreenViewModel ViewModel { get; }

        public SplashScreenWindow(SplashScreenViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            
            // Subscribe to ViewModel property changes
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            // Configure window appearance and size
            ConfigureWindow();
        }

        /// <summary>
        /// Configure window appearance, size, and position
        /// </summary>
        private void ConfigureWindow()
        {
            // Use custom title bar with transparent styling
            this.UseCustomTitleBar();
            this.HideTitleBarIcon();
            this.MakeTitleBarTransparent();
            
            // Set fixed window size
            this.SetWindowSize(WindowWidth, WindowHeight);
            this.SetFixedSize(disableMaximize: true, disableMinimize: true);
            
            // Center on screen
            this.CenterOnScreen();
        }

        /// <summary>
        /// Handle ViewModel property changes and update UI
        /// </summary>
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
    }
}
