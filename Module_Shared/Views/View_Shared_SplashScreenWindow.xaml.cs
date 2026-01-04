using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Shared.Views
{
    /// <summary>
    /// Splash screen window displayed during application startup
    /// Shows branding, progress, and status messages during initialization
    /// Window size: 500x450 pixels (optimized for dialog display)
    /// </summary>
    public sealed partial class View_Shared_SplashScreenWindow : Window
    {
        private const int WindowWidth = 850;
        private const int WindowHeight = 700;

        public ViewModel_Shared_SplashScreen ViewModel { get; }

        /// <summary>
        /// Flag to indicate if the window is being closed programmatically by the application logic.
        /// If false (default), it means the user closed the window manually, which should exit the application.
        /// </summary>
        public bool IsProgrammaticClose { get; set; } = false;

        public View_Shared_SplashScreenWindow(ViewModel_Shared_SplashScreen viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            // Subscribe to ViewModel property changes
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Configure window appearance and size
            ConfigureWindow();

            // Handle manual closure
            this.Closed += SplashScreenWindow_Closed;
        }

        private void SplashScreenWindow_Closed(object sender, WindowEventArgs args)
        {
            // If the user manually closes the splash screen (e.g. via Alt+F4 or taskbar),
            // we must ensure the application process terminates.
            if (!IsProgrammaticClose)
            {
                Application.Current.Exit();
            }
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

