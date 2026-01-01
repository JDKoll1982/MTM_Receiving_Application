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

        public MainWindow(Shared_MainWindowViewModel viewModel, IService_UserSessionManager sessionManager)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _sessionManager = sessionManager;

            // Set initial window size (1450x900 to accommodate wide data grids and toolbars)
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1450, 900));

            // Center window on screen
            CenterWindow();

#if DEBUG
            // Visual indicator for Debug mode - InforVisual Disabled
            try
            {
                if (AppWindow.TitleBar != null)
                {
                    var redColor = Windows.UI.Color.FromArgb(255, 220, 53, 69); // Bootstrap Danger Red
                    var whiteColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);

                    AppWindow.TitleBar.ForegroundColor = whiteColor;
                    AppWindow.TitleBar.BackgroundColor = redColor;
                    AppWindow.TitleBar.ButtonForegroundColor = whiteColor;
                    AppWindow.TitleBar.ButtonBackgroundColor = redColor;
                    AppWindow.TitleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 200, 35, 51);
                    AppWindow.TitleBar.ButtonHoverForegroundColor = whiteColor;
                    AppWindow.TitleBar.InactiveForegroundColor = Windows.UI.Color.FromArgb(255, 200, 200, 200);
                    AppWindow.TitleBar.InactiveBackgroundColor = Windows.UI.Color.FromArgb(255, 150, 0, 0);
                }
                this.Title = "MTM Receiving Application (DEBUG MODE - InforVisual Disabled)";
            }
            catch { /* Ignore styling errors in debug mode */ }
#endif

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
    }
}
