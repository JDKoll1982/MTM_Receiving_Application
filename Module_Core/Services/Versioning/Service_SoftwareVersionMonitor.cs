using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Application;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Services.Versioning;

/// <summary>
/// Monitors the required application version and shuts the app down on mismatch.
/// </summary>
public class Service_SoftwareVersionMonitor : IService_SoftwareVersionMonitor
{
    private const string LogContext = "Core.SoftwareVersion";
    private static readonly TimeSpan MonitoringInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PromptTimeout = TimeSpan.FromMinutes(1);

    private readonly Dao_SoftwareVersion _softwareVersionDao;
    private readonly IService_Window _windowService;
    private readonly IService_Dispatcher _dispatcher;
    private readonly IService_ApplicationShutdown _applicationShutdown;
    private readonly IService_LoggingUtility _logger;
    private readonly string _currentVersion;
    private readonly SemaphoreSlim _checkLock = new(1, 1);

    private IService_DispatcherTimer? _monitoringTimer;
    private bool _mismatchPromptActive;

    public Service_SoftwareVersionMonitor(
        Dao_SoftwareVersion softwareVersionDao,
        IService_Window windowService,
        IService_Dispatcher dispatcher,
        IService_ApplicationShutdown applicationShutdown,
        IService_LoggingUtility logger,
        IOptions<ApplicationSettings> applicationSettings
    )
    {
        _softwareVersionDao =
            softwareVersionDao ?? throw new ArgumentNullException(nameof(softwareVersionDao));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _applicationShutdown =
            applicationShutdown ?? throw new ArgumentNullException(nameof(applicationShutdown));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentVersion = NormalizeVersion(
            applicationSettings?.Value?.Version
                ?? throw new ArgumentNullException(nameof(applicationSettings))
        );
    }

    public Task<Model_Dao_Result> ValidateOnStartupAsync(XamlRoot? dialogXamlRoot = null)
    {
        return CheckVersionAsync(dialogXamlRoot, isStartup: true);
    }

    public void StartMonitoring()
    {
        if (_monitoringTimer != null || _applicationShutdown.IsShutdownRequested)
        {
            return;
        }

        _monitoringTimer = _dispatcher.CreateTimer();
        _monitoringTimer.Interval = MonitoringInterval;
        _monitoringTimer.IsRepeating = true;
        _monitoringTimer.Tick += OnMonitoringTimerTick;
        _monitoringTimer.Start();

        _logger.LogInfo(
            $"Started software version monitoring every {MonitoringInterval.TotalMinutes:0} minutes.",
            LogContext
        );
    }

    public void StopMonitoring()
    {
        if (_monitoringTimer == null)
        {
            return;
        }

        _monitoringTimer.Tick -= OnMonitoringTimerTick;
        _monitoringTimer.Stop();
        _monitoringTimer = null;
    }

    private async void OnMonitoringTimerTick(object? sender, object e)
    {
        await CheckVersionAsync(dialogXamlRoot: null, isStartup: false);
    }

    private async Task<Model_Dao_Result> CheckVersionAsync(XamlRoot? dialogXamlRoot, bool isStartup)
    {
        if (_applicationShutdown.IsShutdownRequested)
        {
            return Model_Dao_Result_Factory.Success();
        }

        if (!await _checkLock.WaitAsync(0))
        {
            return Model_Dao_Result_Factory.Success();
        }

        try
        {
            var currentVersionResult = ValidateCurrentVersion();
            if (!currentVersionResult.Success)
            {
                return currentVersionResult;
            }

            var storedVersionResult = await _softwareVersionDao.GetCurrentAsync();
            if (!storedVersionResult.Success)
            {
                if (
                    string.Equals(
                        storedVersionResult.ErrorMessage,
                        "No record found",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return await BootstrapVersionRecordAsync();
                }

                _logger.LogError(
                    $"Software version check failed while reading MySQL version: {storedVersionResult.ErrorMessage}",
                    storedVersionResult.Exception,
                    LogContext
                );

                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = storedVersionResult.ErrorMessage,
                    Exception = storedVersionResult.Exception,
                    Severity = storedVersionResult.Severity,
                };
            }

            var requiredVersion = NormalizeVersion(
                storedVersionResult.Data?.RequiredVersion ?? string.Empty
            );
            if (!VersionsMatch(_currentVersion, requiredVersion))
            {
                await HandleMismatchAsync(requiredVersion, dialogXamlRoot, isStartup);
                return Model_Dao_Result_Factory.Failure(
                    $"Application version mismatch detected. Running {_currentVersion}, required {requiredVersion}."
                );
            }

            _logger.LogInfo(
                $"Software version check passed. Running {_currentVersion}.",
                LogContext
            );

            return Model_Dao_Result_Factory.Success();
        }
        finally
        {
            _checkLock.Release();
        }
    }

    private Model_Dao_Result ValidateCurrentVersion()
    {
        if (string.IsNullOrWhiteSpace(_currentVersion))
        {
            _logger.LogError("Application.Version is missing or empty.", null, LogContext);
            return Model_Dao_Result_Factory.Failure("Application version is not configured.");
        }

        return Model_Dao_Result_Factory.Success();
    }

    private async Task<Model_Dao_Result> BootstrapVersionRecordAsync()
    {
        var bootstrapResult = await _softwareVersionDao.UpsertAsync(
            _currentVersion,
            Environment.UserName
        );
        if (!bootstrapResult.Success)
        {
            _logger.LogError(
                $"Failed to seed software_version with {_currentVersion}: {bootstrapResult.ErrorMessage}",
                bootstrapResult.Exception,
                LogContext
            );
            return bootstrapResult;
        }

        _logger.LogInfo(
            $"Seeded software_version with current application version {_currentVersion}.",
            LogContext
        );

        return Model_Dao_Result_Factory.Success();
    }

    private async Task HandleMismatchAsync(
        string requiredVersion,
        XamlRoot? dialogXamlRoot,
        bool isStartup
    )
    {
        if (_mismatchPromptActive || _applicationShutdown.IsShutdownRequested)
        {
            return;
        }

        _mismatchPromptActive = true;
        StopMonitoring();

        try
        {
            var xamlRoot = dialogXamlRoot ?? _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                _logger.LogCritical(
                    $"Application version mismatch detected without a XamlRoot. Running {_currentVersion}, required {requiredVersion}.",
                    null,
                    LogContext
                );

                _applicationShutdown.RequestShutdown("software_version_mismatch", 1);
                App.MainWindow?.Close();
                return;
            }

            await ShowMismatchDialogAndShutdownAsync(xamlRoot, requiredVersion, isStartup);
        }
        finally
        {
            _mismatchPromptActive = false;
        }
    }

    private async Task ShowMismatchDialogAndShutdownAsync(
        XamlRoot xamlRoot,
        string requiredVersion,
        bool isStartup
    )
    {
        bool allowClose = false;
        var timingSource = new CancellationTokenSource();

        var message =
            $"This application build does not match the required database version.\n\n"
            + $"Running version: {_currentVersion}\n"
            + $"Required version: {requiredVersion}\n\n"
            + (
                isStartup
                    ? "The application cannot finish starting until the version mismatch is resolved."
                    : "The application can no longer continue because the required version changed while it was running."
            )
            + "\n\nThe application will close after you acknowledge this message or after 1 minute.";

        var dialog = new ContentDialog
        {
            Title = "Application Version Mismatch",
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            xamlRoot
        );

        dialog.PrimaryButtonClick += (_, _) => allowClose = true;
        dialog.Closing += (_, args) =>
        {
            if (!allowClose && args.Result != ContentDialogResult.Primary)
            {
                args.Cancel = true;
            }
        };

        var timeoutTask = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(PromptTimeout, timingSource.Token);
                _dispatcher.TryEnqueue(() =>
                {
                    allowClose = true;
                    dialog.Hide();
                });
            }
            catch (OperationCanceledException) { }
        });

        var result = await dialog.ShowAsync();
        timingSource.Cancel();
        await timeoutTask;
        timingSource.Dispose();

        _logger.LogWarning(
            result == ContentDialogResult.Primary
                ? $"Application version mismatch acknowledged by user. Running {_currentVersion}, required {requiredVersion}."
                : $"Application version mismatch dialog timed out. Running {_currentVersion}, required {requiredVersion}.",
            LogContext
        );

        _applicationShutdown.RequestShutdown("software_version_mismatch", 1);
        App.MainWindow?.Close();
    }

    private static string NormalizeVersion(string version)
    {
        return version.Trim();
    }

    private static bool VersionsMatch(string currentVersion, string requiredVersion)
    {
        if (
            Version.TryParse(currentVersion, out var current)
            && Version.TryParse(requiredVersion, out var required)
        )
        {
            return current == required;
        }

        return string.Equals(currentVersion, requiredVersion, StringComparison.OrdinalIgnoreCase);
    }
}
