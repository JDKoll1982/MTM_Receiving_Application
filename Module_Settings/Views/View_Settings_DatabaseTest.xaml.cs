using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.ViewModels;
using System;

namespace MTM_Receiving_Application.Module_Settings.Views;

/// <summary>
/// Settings Database Test page - Development tool for validating database schema, stored procedures, and DAOs
/// </summary>
public sealed partial class View_Settings_DatabaseTest : Page
{
    public ViewModel_Settings_DatabaseTest? ViewModel { get; private set; }

    public View_Settings_DatabaseTest()
    {
        System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Constructor started");

        // CRITICAL: Set ViewModel BEFORE InitializeComponent() for x:Bind to work
        try
        {
            System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attempting to get ViewModel from DI");
            ViewModel = App.GetService<ViewModel_Settings_DatabaseTest>();
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] ViewModel obtained: {ViewModel != null}");

            if (ViewModel == null)
            {
                throw new InvalidOperationException("Failed to obtain ViewModel from DI - returned null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Failed to create ViewModel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Exception type: {ex.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Stack trace: {ex.StackTrace}");

            // Cannot continue without ViewModel for x:Bind
            throw;
        }

        try
        {
            // Initialize component AFTER ViewModel is set (required for x:Bind)
            System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Calling InitializeComponent()");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[DatabaseTestView] InitializeComponent() completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] InitializeComponent() FAILED: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Exception type: {ex.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Stack trace: {ex.StackTrace}");
            throw; // Re-throw to show in navigation error dialog
        }

        System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Setting DataContext");
        DataContext = ViewModel;

        // Attach focus service if available
        try
        {
            System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attempting to get Focus service");
            var focusService = App.GetService<IService_Focus>();
            if (focusService != null)
            {
                System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attaching focus service");
                focusService.AttachFocusOnVisibility(this);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Focus service is null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Focus service failed (non-critical): {ex.Message}");
        }

        // Run tests after view is loaded
        System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Attaching Loaded event handler");
        this.Loaded += OnLoaded;

        System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Constructor completed");
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[DatabaseTestView] OnLoaded event fired");
        try
        {
            // Run tests after UI is fully initialized
            if (ViewModel?.RunAllTestsCommand != null)
            {
                System.Diagnostics.Debug.WriteLine("[DatabaseTestView] Executing RunAllTestsCommand");
                await ViewModel.RunAllTestsCommand.ExecuteAsync(null);
                System.Diagnostics.Debug.WriteLine("[DatabaseTestView] RunAllTestsCommand completed");
                UpdateTabVisibility();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Cannot run tests - ViewModel: {ViewModel != null}, Command: {ViewModel?.RunAllTestsCommand != null}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Error running database tests: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Exception type: {ex.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Stack trace: {ex.StackTrace}");
        }
    }

    // Tab click handlers
    private void OnSchemaTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedTab = "Schema";
            UpdateTabVisibility();
        }
    }

    private void OnStoredProceduresTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedTab = "StoredProcedures";
            UpdateTabVisibility();
        }
    }

    private void OnDaosTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedTab = "DAOs";
            UpdateTabVisibility();
        }
    }

    private void OnLogsTabClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.SelectedTab = "Logs";
            UpdateTabVisibility();
        }
    }

    private void UpdateTabVisibility()
    {
        if (ViewModel == null)
            return;

        System.Diagnostics.Debug.WriteLine($"[DatabaseTestView] Selected tab: {ViewModel.SelectedTab}");

        SchemaTabContent.Visibility = ViewModel.SelectedTab == "Schema"
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        StoredProceduresTabContent.Visibility = ViewModel.SelectedTab == "StoredProcedures"
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        DaosTabContent.Visibility = ViewModel.SelectedTab == "DAOs"
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        LogsTabContent.Visibility = ViewModel.SelectedTab == "Logs"
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;
    }
}