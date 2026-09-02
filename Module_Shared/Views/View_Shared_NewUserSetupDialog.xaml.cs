using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Shared.Views
{
    /// <summary>
    /// Dialog for creating new user accounts when Windows username is not found in database.
    /// Allows supervisor to create account with full name, department, shift, and PIN.
    /// </summary>
    public sealed partial class View_Shared_NewUserSetupDialog : ContentDialog
    {
        private readonly Dictionary<Control, Border> _cardBorders = new();
        public ViewModel_Shared_NewUserSetup ViewModel { get; }

        /// <summary>
        /// Constructor with ViewModel injection
        /// </summary>
        /// <param name="viewModel"></param>
        public View_Shared_NewUserSetupDialog(ViewModel_Shared_NewUserSetup viewModel)
        {
            InitializeComponent();
            Helper_UI_ContentDialogTheme.ApplyTheme(this);
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeCardBorders();
            ClearCardHighlights();

            Closing += OnDialogClosing;
            Loaded += OnDialogLoaded;
        }

        
        private void InitializeCardBorders()
        {
            _cardBorders[FirstNameTextBox] = NameCardBorder;
            _cardBorders[LastNameTextBox] = NameCardBorder;
            _cardBorders[EmployeeNumberTextBox] = EmployeeDetailsCardBorder;
            _cardBorders[DepartmentComboBox] = DepartmentAssignmentCardBorder;
            _cardBorders[CustomDepartmentTextBox] = DepartmentAssignmentCardBorder;
            _cardBorders[ShiftComboBox] = WorkScheduleCardBorder;
            _cardBorders[PinPasswordBox] = AccountSecurityCardBorder;
            _cardBorders[ConfirmPinPasswordBox] = AccountSecurityCardBorder;
            _cardBorders[VisualUsernameTextBox] = ErpAccessCardBorder;
            _cardBorders[VisualPasswordBox] = ErpAccessCardBorder;
        }

        /// <summary>
        /// Handle dialog closing event (including X button)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Only mark as cancelled if the account was not already created.
            // When Hide() is called programmatically after success, args.Result is None (not Primary),
            // so without this guard IsCancelled would be incorrectly set to true after a successful creation.
            if (args.Result != ContentDialogResult.Primary && ViewModel.NewEmployeeNumber <= 0)
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

            // Set focus to first name field
            FirstNameTextBox.Focus(FocusState.Programmatic);
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

                if (ViewModel.Departments.Count == 0)
                {
                    StatusInfoBar.Title = "No Departments Available";
                    StatusInfoBar.Message =
                        "No department options were returned. You can still use Other to continue.";
                    StatusInfoBar.Severity = InfoBarSeverity.Warning;
                    StatusInfoBar.IsOpen = true;
                }
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

        private void InputControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control control && _cardBorders.TryGetValue(control, out var activeCard))
            {
                ActivateCard(activeCard);
            }
        }

        private void InputControl_LostFocus(object sender, RoutedEventArgs e)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                if (FocusManager.GetFocusedElement(XamlRoot) is Control control
                    && _cardBorders.TryGetValue(control, out var activeCard))
                {
                    ActivateCard(activeCard);
                    return;
                }

                ClearCardHighlights();
            });
        }

        /// <summary>
        /// Handle Create Account button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateAccountActionButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateAccountAsync();
        }

        private async System.Threading.Tasks.Task CreateAccountAsync()
        {
            StatusInfoBar.IsOpen = false;

            // Collect form data
            string employeeNumber = EmployeeNumberTextBox.Text?.Trim() ?? string.Empty;
            string firstName = FirstNameTextBox.Text?.Trim() ?? string.Empty;
            string lastName = LastNameTextBox.Text?.Trim() ?? string.Empty;
            string department = DepartmentComboBox.SelectedItem?.ToString() ?? string.Empty;
            string customDepartment = CustomDepartmentTextBox.Text?.Trim() ?? string.Empty;
            string shift =
                (ShiftComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
            string pin = PinPasswordBox.Password?.Trim() ?? string.Empty;
            string confirmPin = ConfirmPinPasswordBox.Password?.Trim() ?? string.Empty;
            string visualUsername = VisualUsernameTextBox.Text?.Trim() ?? string.Empty;
            string visualPassword = VisualPasswordBox.Password?.Trim() ?? string.Empty;

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

            if (string.IsNullOrWhiteSpace(firstName))
            {
                ShowValidationError("First Name is required.");
                FirstNameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                ShowValidationError("Last Name is required.");
                LastNameTextBox.Focus(FocusState.Programmatic);
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

            if (string.IsNullOrWhiteSpace(visualUsername))
            {
                ShowValidationError("Visual/Infor Username is required.");
                VisualUsernameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            if (string.IsNullOrWhiteSpace(visualPassword))
            {
                ShowValidationError("Visual/Infor Password is required.");
                VisualPasswordBox.Focus(FocusState.Programmatic);
                return;
            }

            // Set loading state
            SetLoadingState(true);

            // Update ViewModel properties
            ViewModel.EmployeeNumber = employeeNumber;
            ViewModel.FirstName = firstName;
            ViewModel.LastName = lastName;
            ViewModel.Department = department;
            ViewModel.Shift = shift;
            ViewModel.Pin = pin;
            ViewModel.VisualUsername = visualUsername;
            ViewModel.VisualPassword = visualPassword;

            // Attempt to create account
            bool success = await ViewModel.CreateAccountAsync();

            if (success)
            {
                // Keep everything disabled — startup will continue automatically after the dialog closes.
                // Show success message with employee number
                StatusInfoBar.Title = "Account Created Successfully!";
                StatusInfoBar.Message =
                    $"Your employee number is: {ViewModel.NewEmployeeNumber}. Welcome to the team!";
                StatusInfoBar.Severity = InfoBarSeverity.Success;
                StatusInfoBar.IsOpen = true;

                // Wait a moment for user to see success message, then close
                await System.Threading.Tasks.Task.Delay(2000);
                Hide();
            }
            else
            {
                // Restore interactive state so the user can correct and retry
                SetLoadingState(false);
                ShowValidationError(
                    ViewModel.ErrorMessage ?? "Failed to create account. Please try again."
                );
            }
        }

        /// <summary>
        /// Handle Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelActionButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsCancelled = true;
            Hide();
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
            EmployeeNumberTextBox.IsEnabled = !isLoading;
            FirstNameTextBox.IsEnabled = !isLoading;
            LastNameTextBox.IsEnabled = !isLoading;
            DepartmentComboBox.IsEnabled = !isLoading;
            CustomDepartmentTextBox.IsEnabled = !isLoading;
            ShiftComboBox.IsEnabled = !isLoading;
            PinPasswordBox.IsEnabled = !isLoading;
            ConfirmPinPasswordBox.IsEnabled = !isLoading;
            VisualUsernameTextBox.IsEnabled = !isLoading;
            VisualPasswordBox.IsEnabled = !isLoading;

            CreateAccountActionButton.IsEnabled = !isLoading;
            CancelActionButton.IsEnabled = !isLoading;

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

        private void ActivateCard(Border activeCard)
        {
            foreach (var border in _cardBorders.Values.Distinct())
            {
                SetCardState(border, border == activeCard);
            }
        }

        private void ClearCardHighlights()
        {
            foreach (var border in _cardBorders.Values.Distinct())
            {
                SetCardState(border, isActive: false);
            }
        }

        private void SetCardState(Border border, bool isActive)
        {
            border.Background = GetBrush(
                isActive ? "CardBackgroundFillColorSecondaryBrush" : "CardBackgroundFillColorDefaultBrush",
                border.Background);
            border.BorderBrush = GetBrush(
                isActive ? "AccentFillColorDefaultBrush" : "ControlStrongStrokeColorDefaultBrush",
                border.BorderBrush);
        }

        private static Brush GetBrush(string resourceKey, Brush fallback)
        {
            return Application.Current.Resources.TryGetValue(resourceKey, out var resource)
                && resource is Brush brush
                ? brush
                : fallback ?? new SolidColorBrush(Colors.Transparent);
        }
    }
}
