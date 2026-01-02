using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Shared;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public Shared_MainWindowViewModel ViewModel { get; }
        private readonly IService_UserSessionManager _sessionManager;
        private bool _hasNavigatedOnStartup = false;

        public MainWindow(Shared_MainWindowViewModel viewModel, IService_UserSessionManager sessionManager)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _sessionManager = sessionManager;

            // Set initial window size (1450x900 to accommodate wide data grids and toolbars)
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1450, 900));

            // Center window on screen
            CenterWindow();

            // Configure title bar to blend with UI
            ConfigureTitleBar();

            // Set user display from current session
            if (_sessionManager.CurrentSession != null)
            {
                var user = _sessionManager.CurrentSession.User;
                UserDisplayTextBlock.Text = user.DisplayName;
                UserPicture.DisplayName = user.DisplayName;
            }

            // Wire up activity tracking
            if (Content is UIElement rootElement)
            {
                rootElement.PointerMoved += (s, e) => _sessionManager.UpdateLastActivity();
                rootElement.KeyDown += (s, e) => _sessionManager.UpdateLastActivity();
            }

            this.Activated += MainWindow_Activated;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // Ensure the application process terminates when the main window is closed
            Application.Current.Exit();
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                _sessionManager.UpdateLastActivity();

                // Navigate to Receiving workflow on first activation
                if (!_hasNavigatedOnStartup)
                {
                    _hasNavigatedOnStartup = true;
                    PageTitleTextBlock.Text = "📥 Receiving - Mode Selection";
                    ContentFrame.Navigate(typeof(Views.Receiving.Receiving_WorkflowView));
                    ContentFrame.Navigated += ContentFrame_Navigated;
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                PageTitleTextBlock.Text = "Settings";
                ContentFrame.Navigate(typeof(Views.Settings.Settings_WorkflowView));
            }
            else if (args.SelectedItem is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "ReceivingWorkflowView":
                        PageTitleTextBlock.Text = "Receiving Workflow";
                        ContentFrame.Navigate(typeof(Views.Receiving.Receiving_WorkflowView));
                        ContentFrame.Navigated += ContentFrame_Navigated;
                        break;
                    case "DunnageLabelPage":
                        PageTitleTextBlock.Text = "Dunnage Labels";
                        ContentFrame.Navigate(typeof(Views.Dunnage.Dunnage_WorkflowView));
                        ContentFrame.Navigated += ContentFrame_Navigated;
                        break;
                    case "CarrierDeliveryLabelPage":
                        PageTitleTextBlock.Text = "Carrier Delivery";
                        ContentFrame.Navigate(typeof(Views.Main.Main_CarrierDeliveryLabelPage));
                        break;
                }
            }
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // If navigated to ReceivingWorkflowView, subscribe to ViewModel changes to update header
            if (ContentFrame.Content is Views.Receiving.Receiving_WorkflowView receivingView)
            {
                var viewModel = receivingView.ViewModel;
                if (viewModel != null)
                {
                    // Update header with current step title
                    PageTitleTextBlock.Text = viewModel.CurrentStepTitle;

                    // Subscribe to property changes to keep header updated
                    viewModel.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.CurrentStepTitle))
                        {
                            PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                        }
                    };
                }
            }
            // If navigated to DunnageWorkflowView, subscribe to ViewModel changes to update header
            else if (ContentFrame.Content is Views.Dunnage.Dunnage_WorkflowView dunnageView)
            {
                var viewModel = dunnageView.ViewModel;
                if (viewModel != null)
                {
                    // Update header with current step title
                    PageTitleTextBlock.Text = viewModel.CurrentStepTitle;

                    // Subscribe to property changes to keep header updated
                    viewModel.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.CurrentStepTitle))
                        {
                            PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                        }
                    };
                }
            }
            // If navigated to SettingsWorkflowView, subscribe to ViewModel changes to update header
            else if (ContentFrame.Content is Views.Settings.Settings_WorkflowView settingsView)
            {
                var viewModel = settingsView.ViewModel;
                if (viewModel != null)
                {
                    // Update header with current step title
                    PageTitleTextBlock.Text = viewModel.CurrentStepTitle;

                    // Subscribe to property changes to keep header updated
                    viewModel.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.CurrentStepTitle))
                        {
                            PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Update user display from current session
        /// Call this after session is created during startup
        /// </summary>
        public void UpdateUserDisplay()
        {
            if (_sessionManager.CurrentSession != null)
            {
                var user = _sessionManager.CurrentSession.User;
                UserDisplayTextBlock.Text = user.DisplayName;
                UserPicture.DisplayName = user.DisplayName;
            }
        }

        /// <summary>
        /// Center the window on the primary display
        /// </summary>
        private void CenterWindow()
        {
            var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            var centerX = (workArea.Width - 1450) / 2;
            var centerY = (workArea.Height - 900) / 2;

            AppWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
        }

        /// <summary>
        /// Configure title bar to blend with the application UI
        /// </summary>
        private void ConfigureTitleBar()
        {
            try
            {
                if (AppWindow.TitleBar != null)
                {
                    // Make title bar transparent to blend with Mica backdrop
                    AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                    // Set the title bar drag region to the AppTitleBar element
                    AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

                    // Set button colors to match theme
                    var transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

                    // Button colors (will use theme colors)
                    AppWindow.TitleBar.ButtonBackgroundColor = transparentColor;
                    AppWindow.TitleBar.ButtonInactiveBackgroundColor = transparentColor;

                    // Foreground colors for buttons
                    var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
                    AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
                    AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
                    AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;

                    // Set custom drag region for the title bar
                    SetTitleBarDragRegion();
                }
            }
            catch
            {
                // Ignore title bar customization errors
            }
        }

        /// <summary>
        /// Set the drag region for the custom title bar
        /// </summary>
        private void SetTitleBarDragRegion()
        {
            if (AppWindow.TitleBar != null && AppTitleBar != null)
            {
                // Get the title bar height
                var scale = AppTitleBar.XamlRoot.RasterizationScale;
                var titleBarHeight = (int)(AppTitleBar.ActualHeight * scale);

                // The entire AppTitleBar is draggable
                var dragRect = new Windows.Graphics.RectInt32
                {
                    X = 0,
                    Y = 0,
                    Width = (int)(AppTitleBar.ActualWidth * scale),
                    Height = titleBarHeight
                };

                AppWindow.TitleBar.SetDragRectangles(new[] { dragRect });
            }
        }
    }
}
