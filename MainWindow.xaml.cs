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
        public MainWindowViewModel ViewModel { get; }
        private readonly IService_UserSessionManager _sessionManager;

        public MainWindow(MainWindowViewModel viewModel, IService_UserSessionManager sessionManager)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _sessionManager = sessionManager;

            // Set initial window size (1200x800 recommended for main application window)
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
            
            // Center window on screen
            CenterWindow();

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
                // ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (args.SelectedItem is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "ReceivingWorkflowView":
                        PageTitleTextBlock.Text = "Receiving Workflow";
                        ContentFrame.Navigate(typeof(Views.Receiving.ReceivingWorkflowView));
                        ContentFrame.Navigated += ContentFrame_Navigated;
                        break;
                    case "DunnageLabelPage":
                        PageTitleTextBlock.Text = "Dunnage Labels";
                        ContentFrame.Navigate(typeof(Views.Receiving.DunnageLabelPage));
                        break;
                    case "CarrierDeliveryLabelPage":
                        PageTitleTextBlock.Text = "Carrier Delivery";
                        ContentFrame.Navigate(typeof(Views.Receiving.CarrierDeliveryLabelPage));
                        break;
                }
            }
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // If navigated to ReceivingWorkflowView, subscribe to ViewModel changes to update header
            if (ContentFrame.Content is Views.Receiving.ReceivingWorkflowView receivingView)
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
            
            var centerX = (workArea.Width - 1200) / 2;
            var centerY = (workArea.Height - 800) / 2;
            
            AppWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
        }
    }
}
