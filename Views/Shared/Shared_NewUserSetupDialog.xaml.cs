using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Views.Shared
{
    /// <summary>
    /// Dialog for creating new user accounts when Windows username is not found in database.
    /// Allows supervisor to create account with full name, department, shift, and PIN.
    /// </summary>
    public sealed partial class Shared_NewUserSetupDialog : ContentDialog
    {
        public Shared_NewUserSetupViewModel ViewModel { get; }

        /// <summary>
        /// Constructor with ViewModel injection
        /// </summary>
        /// <param name="viewModel"></param>
        public Shared_NewUserSetupDialog(Shared_NewUserSetupViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Wire up event handlers
            PrimaryButtonClick += OnCreateAccountButtonClick;
            CloseButtonClick += OnCancelButtonClick;
            Closing += OnDialogClosing;
            Loaded += OnDialogLoaded;
        }

        /// <summary>
        /// Handle dialog closing event (including X button)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // If user closes with X or Cancel button, mark as cancelled
            if (args.Result != ContentDialogResult.Primary)
            {
                ViewModel.IsCancelled = true;
            }
        }

        /// <summary>
        /// Initialize dialog when loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            // Set Windows username (read-only)
            WindowsUsernameTextBox.Text = ViewModel.WindowsUsername;

            // Load departments from database
            await LoadDepartmentsAsync();

            // Set focus to full name field
            FullNameTextBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Load departments from database and populate ComboBox
        /// </summary>
        private async System.Threading.Tasks.Task LoadDepartmentsAsync()
        {
            try
            {
                await ViewModel.LoadDepartmentsAsync();

                // Clear and populate ComboBox
                DepartmentComboBox.Items.Clear();

                foreach (var dept in ViewModel.Departments)
                {
                    DepartmentComboBox.Items.Add(dept);
                }

                // Add "Other" option
                DepartmentComboBox.Items.Add("Other");
            }
            catch (Exception ex)
            {
                StatusInfoBar.Title = "Error Loading Departments";
                StatusInfoBar.Message = $"Failed to load departments: {ex.Message}";
                StatusInfoBar.Severity = InfoBarSeverity.Warning;
                StatusInfoBar.IsOpen = true;
            }
        }

        /// <summary>
        /// Handle Department ComboBox selection change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartmentComboBox.SelectedItem != null)
            {
                string selectedDept = DepartmentComboBox.SelectedItem.ToString() ?? string.Empty;

                // Show/hide custom department field based on selection
                if (selectedDept == "Other")
                {
                    CustomDepartmentPanel.Visibility = Visibility.Visible;
                    ViewModel.ShowCustomDepartment = true;
                    CustomDepartmentTextBox.Focus(FocusState.Programmatic);
                }
                else
                {
                    CustomDepartmentPanel.Visibility = Visibility.Collapsed;
                    ViewModel.ShowCustomDepartment = false;
                    ViewModel.Department = selectedDept;
                }
            }
        }

        /// <summary>
        /// Handle ERP configuration checkbox checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigureErpCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ErpCredentialsPanel.Visibility = Visibility.Visible;
            ViewModel.ConfigureErpAccess = true;

            // Ensure expander is expanded to show the content
            if (!ErpExpander.IsExpanded)
            {
                ErpExpander.IsExpanded = true;
            }

            // Set focus to first ERP field after a brief delay to allow UI to render
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                VisualUsernameTextBox.Focus(FocusState.Programmatic);
            });
        }

        /// <summary>
        /// Handle ERP configuration checkbox unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfigureErpCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ErpCredentialsPanel.Visibility = Visibility.Collapsed;
            ViewModel.ConfigureErpAccess = false;

            // Clear ERP fields when unchecked
            VisualUsernameTextBox.Text = string.Empty;
            VisualPasswordBox.Password = string.Empty;
        }

        /// <summary>
        /// Handle Create Account button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnCreateAccountButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            // Clear previous errors
            StatusInfoBar.IsOpen = false;

            // Collect form data
            string employeeNumber = EmployeeNumberTextBox.Text?.Trim() ?? string.Empty;
            string fullName = FullNameTextBox.Text?.Trim() ?? string.Empty;
            string department = DepartmentComboBox.SelectedItem?.ToString() ?? string.Empty;
            string customDepartment = CustomDepartmentTextBox.Text?.Trim() ?? string.Empty;
            string shift = (ShiftComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string pin = PinPasswordBox.Password?.Trim() ?? string.Empty;
            string confirmPin = ConfirmPinPasswordBox.Password?.Trim() ?? string.Empty;

            // Use custom department if "Other" selected
            if (department == "Other")
            {
                department = customDepartment;
            }

            // Validate all fields
            if (string.IsNullOrWhiteSpace(employeeNumber))
            {
                ShowValidationError("Employee Number is required.");
                EmployeeNumberTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (!int.TryParse(employeeNumber, out int empNum) || empNum <= 0)
            {
                ShowValidationError("Employee Number must be a positive number.");
                EmployeeNumberTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowValidationError("Full Name is required.");
                FullNameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (fullName.Length < 2)
            {
                ShowValidationError("Full Name must be at least 2 characters.");
                FullNameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(department))
            {
                ShowValidationError("Department is required.");
                DepartmentComboBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(shift))
            {
                ShowValidationError("Shift is required.");
                ShiftComboBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(pin))
            {
                ShowValidationError("PIN is required.");
                PinPasswordBox.Focus(FocusState.Programmatic);
                return;
            }

            if (pin.Length != 4 || !pin.All(char.IsDigit))
            {
                ShowValidationError("PIN must be exactly 4 numeric digits.");
                PinPasswordBox.Password = string.Empty;
                ConfirmPinPasswordBox.Password = string.Empty;
                PinPasswordBox.Focus(FocusState.Programmatic);
                return;
            }

            if (pin != confirmPin)
            {
                ShowValidationError("PINs do not match. Please try again.");
                ConfirmPinPasswordBox.Password = string.Empty;
                ConfirmPinPasswordBox.Focus(FocusState.Programmatic);
                return;
            }

            // Set loading state
            SetLoadingState(true);

            // Update ViewModel properties
            ViewModel.EmployeeNumber = employeeNumber;
            ViewModel.FullName = fullName;
            ViewModel.Department = department;
            ViewModel.Shift = shift;
            ViewModel.Pin = pin;

            // Update ERP credentials if configured
            if (ViewModel.ConfigureErpAccess)
            {
                ViewModel.VisualUsername = VisualUsernameTextBox.Text?.Trim();
                ViewModel.VisualPassword = VisualPasswordBox.Password?.Trim();
            }

            // Attempt to create account
            bool success = await ViewModel.CreateAccountAsync();

            // Restore normal state
            SetLoadingState(false);

            if (success)
            {
                // Show success message with employee number
                StatusInfoBar.Title = "Account Created Successfully!";
                StatusInfoBar.Message = $"Your employee number is: {ViewModel.NewEmployeeNumber}. Welcome to the team!";
                StatusInfoBar.Severity = InfoBarSeverity.Success;
                StatusInfoBar.IsOpen = true;

                // Wait a moment for user to see success message
                await System.Threading.Tasks.Task.Delay(2000);

                // Close dialog
                Hide();
            }
            else
            {
                // Show error message
                ShowValidationError(ViewModel.ErrorMessage ?? "Failed to create account. Please try again.");
            }
        }

        /// <summary>
        /// Handle Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnCancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // User cancelled account creation
            ViewModel.IsCancelled = true;
        }

        /// <summary>
        /// Show validation error in InfoBar
        /// </summary>
        /// <param name="message"></param>
        private void ShowValidationError(string message)
        {
            StatusInfoBar.Title = "Validation Error";
            StatusInfoBar.Message = message;
            StatusInfoBar.Severity = InfoBarSeverity.Error;
            StatusInfoBar.IsOpen = true;
        }

        /// <summary>
        /// Set loading state (disable controls, show progress bar)
        /// </summary>
        /// <param name="isLoading"></param>
        private void SetLoadingState(bool isLoading)
        {
            // Disable/enable input controls
            FullNameTextBox.IsEnabled = !isLoading;
            DepartmentComboBox.IsEnabled = !isLoading;
            CustomDepartmentTextBox.IsEnabled = !isLoading;
            ShiftComboBox.IsEnabled = !isLoading;
            PinPasswordBox.IsEnabled = !isLoading;
            ConfirmPinPasswordBox.IsEnabled = !isLoading;

            // Disable/enable buttons
            IsPrimaryButtonEnabled = !isLoading;
            IsSecondaryButtonEnabled = !isLoading;

            // Show/hide progress bar
            LoadingProgressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            if (isLoading)
            {
                StatusInfoBar.Title = "Creating Account";
                StatusInfoBar.Message = "Please wait while we create your account...";
                StatusInfoBar.Severity = InfoBarSeverity.Informational;
                StatusInfoBar.IsOpen = true;
            }
        }
    }
}
