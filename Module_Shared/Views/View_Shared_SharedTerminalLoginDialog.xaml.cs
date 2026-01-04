using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Shared.Views
{
    /// <summary>
    /// Dialog for PIN-based authentication on shared terminals.
    /// Allows username + 4-digit PIN entry with 3-attempt limit.
    /// </summary>
    public sealed partial class View_Shared_SharedTerminalLoginDialog : ContentDialog
    {
        public ViewModel_Shared_SharedTerminalLogin ViewModel { get; }
        private int _attemptCount = 0;
        private const int MaxAttempts = 3;

        /// <summary>
        /// Constructor with ViewModel injection
        /// </summary>
        /// <param name="viewModel"></param>
        public View_Shared_SharedTerminalLoginDialog(ViewModel_Shared_SharedTerminalLogin viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Wire up event handlers
            PrimaryButtonClick += OnPrimaryButtonClick;
            CloseButtonClick += OnCloseButtonClick;
            Loaded += (s, e) => UsernameTextBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Handles the Login button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            // Clear previous errors
            ErrorInfoBar.IsOpen = false;

            // Validate input
            string username = UsernameTextBox.Text?.Trim() ?? string.Empty;
            string pin = PinPasswordBox.Password?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorInfoBar.Title = "Validation Error";
                ErrorInfoBar.Message = "Username is required.";
                ErrorInfoBar.IsOpen = true;
                UsernameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                ErrorInfoBar.Title = "Validation Error";
                ErrorInfoBar.Message = "PIN is required.";
                ErrorInfoBar.IsOpen = true;
                PinPasswordBox.Focus(FocusState.Programmatic);
                return;
            }

            if (pin.Length != 4 || !int.TryParse(pin, out _))
            {
                ErrorInfoBar.Title = "Validation Error";
                ErrorInfoBar.Message = "PIN must be exactly 4 numeric digits.";
                ErrorInfoBar.IsOpen = true;
                PinPasswordBox.Password = string.Empty;
                PinPasswordBox.Focus(FocusState.Programmatic);
                return;
            }

            // Attempt authentication via ViewModel
            ViewModel.Username = username;
            ViewModel.Pin = pin;

            bool success = await ViewModel.LoginAsync();

            if (success)
            {
                // Authentication successful - close dialog
                Hide();
            }
            else
            {
                // Authentication failed
                _attemptCount++;
                ViewModel.AttemptCount = _attemptCount;

                // Clear PIN field
                PinPasswordBox.Password = string.Empty;
                PinPasswordBox.Focus(FocusState.Programmatic);

                // Show error
                ErrorInfoBar.Title = "Login Failed";
                ErrorInfoBar.Message = ViewModel.ErrorMessage ?? "Invalid username or PIN. Please try again.";
                ErrorInfoBar.IsOpen = true;

                // Update attempt counter display
                if (_attemptCount < MaxAttempts)
                {
                    AttemptCounterTextBlock.Text = $"Attempt {_attemptCount} of {MaxAttempts}";
                    AttemptCounterTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    // Maximum attempts reached - show lockout message
                    AttemptCounterTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                        Microsoft.UI.Colors.Red);
                    AttemptCounterTextBlock.Text = "Maximum login attempts exceeded. Application closing for security.";
                    AttemptCounterTextBlock.Visibility = Visibility.Visible;

                    ErrorInfoBar.Title = "Account Locked";
                    ErrorInfoBar.Message = "Too many failed login attempts. The application will close.";
                    ErrorInfoBar.Severity = InfoBarSeverity.Error;
                    ErrorInfoBar.IsOpen = true;

                    // Disable input controls
                    UsernameTextBox.IsEnabled = false;
                    PinPasswordBox.IsEnabled = false;
                    IsPrimaryButtonEnabled = false;
                    IsSecondaryButtonEnabled = false;

                    // Wait 5 seconds then close dialog with failure result
                    await System.Threading.Tasks.Task.Delay(5000);

                    // Signal to caller that lockout occurred
                    ViewModel.IsLockedOut = true;
                    Hide();
                }
            }
        }

        /// <summary>
        /// Handles the Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // User cancelled login
            ViewModel.IsCancelled = true;
        }
    }
}
