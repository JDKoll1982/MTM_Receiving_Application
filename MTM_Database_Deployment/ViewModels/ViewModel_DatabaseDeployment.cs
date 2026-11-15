using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;

namespace MTM_Database_Deployment.ViewModels;

/// <summary>
/// Main window view model: connection settings, What-If preview, and both deployment modes.
/// </summary>
public partial class ViewModel_DatabaseDeployment : ObservableObject
{
    private readonly Service_DeploymentSettings _settingsService;
    private readonly Service_ObjectCatalog _catalog;
    private readonly Model_ConnectionSettings _settings;
    private CancellationTokenSource? _cancellation;

    [ObservableProperty]
    private string preferredServer = string.Empty;

    [ObservableProperty]
    private string fallbackServer = string.Empty;

    [ObservableProperty]
    private double port = 3306;

    [ObservableProperty]
    private string database = string.Empty;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Ready.";

    [ObservableProperty]
    private string progressMessage = string.Empty;

    [ObservableProperty]
    private string planSummary = "No preview yet.";

    [ObservableProperty]
    private string logText = string.Empty;

    [ObservableProperty]
    private string activeServerText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool removeDatabaseOnlyObjects;

    [ObservableProperty]
    private bool confirmRebuild;

    [ObservableProperty]
    private int modeIndex;

    public bool IsRebuildMode => ModeIndex == 1;

    public ObservableCollection<Model_SchemaChange> PlanChanges { get; } = new();

    public string SqlRoot => _catalog.SqlRoot;

    public ViewModel_DatabaseDeployment()
    {
        _settingsService = new Service_DeploymentSettings();
        _catalog = new Service_ObjectCatalog(_settingsService.SqlRoot);
        _settings = _settingsService.Load();

        PreferredServer = _settings.PreferredServer;
        FallbackServer = _settings.FallbackServer;
        Port = _settings.Port;
        Database = _settings.Database;
        UserName = _settings.User;
        Password = _settings.Password;
        ActiveServerText = $"Preferred: {PreferredServer}:{Port}";
        StatusMessage = $"SQL objects: {_catalog.LoadObjects().Count} script(s) found.";
    }

    private Model_ConnectionSettings BuildSettings()
    {
        var settings = _settings.Clone();
        settings.PreferredServer = PreferredServer?.Trim() ?? string.Empty;
        settings.FallbackServer = FallbackServer?.Trim() ?? string.Empty;
        settings.Port = (int)Math.Round(Port);
        settings.Database = Database?.Trim() ?? string.Empty;
        settings.User = UserName?.Trim() ?? string.Empty;
        settings.Password = Password ?? string.Empty;
        return settings;
    }

    private Service_MySqlGateway CreateGateway() => new(BuildSettings());

    private void AppendLog(string message)
    {
        LogText = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}{LogText}";
    }

    private void SaveSettings() => _settingsService.Save(BuildSettings());

    private async Task<Model_DeploymentPlan> BuildPlanAsync(Service_MySqlGateway gateway, string databaseName)
    {
        var comparer = new Service_SchemaComparer(gateway, _catalog);
        var progress = new Progress<string>(message =>
        {
            ProgressMessage = message;
            AppendLog(message);
        });

        var plan = await comparer.BuildPlanAsync(databaseName, progress, _cancellation!.Token).ConfigureAwait(true);

        PlanChanges.Clear();
        foreach (var change in plan.Changes.OrderBy(item => item.Kind).ThenBy(item => item.Target, StringComparer.OrdinalIgnoreCase))
        {
            PlanChanges.Add(change);
        }

        PlanSummary = plan.Summary;
        ActiveServerText = $"Active: {gateway.ActiveServer}:{gateway.Port}";
        return plan;
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        _cancellation = new CancellationTokenSource();

        try
        {
            ProgressMessage = "Checking server availability...";
            var gateway = CreateGateway();
            var message = await gateway.ResolveServerAsync(_cancellation.Token).ConfigureAwait(true);
            ActiveServerText = $"Active: {gateway.ActiveServer}:{gateway.Port}";
            StatusMessage = message;
            AppendLog(message);
            SaveSettings();
        }
        catch (Exception exception)
        {
            StatusMessage = exception.Message;
            AppendLog("Connection check failed: " + exception.Message);
        }
        finally
        {
            IsBusy = false;
            ProgressMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task PreviewAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        _cancellation = new CancellationTokenSource();

        try
        {
            SaveSettings();
            var gateway = CreateGateway();
            var plan = await BuildPlanAsync(gateway, Database).ConfigureAwait(true);
            StatusMessage = $"Preview complete. {plan.Summary}";
            AppendLog(StatusMessage);
        }
        catch (Exception exception)
        {
            StatusMessage = "Preview failed: " + exception.Message;
            AppendLog(StatusMessage);
        }
        finally
        {
            IsBusy = false;
            ProgressMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeployAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (IsRebuildMode && !ConfirmRebuild)
        {
            StatusMessage = "Fresh rebuild needs the confirmation check box before it can run.";
            return;
        }

        IsBusy = true;
        _cancellation = new CancellationTokenSource();

        try
        {
            SaveSettings();
            var gateway = CreateGateway();
            var engine = new Service_DeploymentEngine(gateway, _catalog);
            var progress = new Progress<string>(message =>
            {
                ProgressMessage = message;
                AppendLog(message);
            });

            Model_DeploymentResult result;

            if (IsRebuildMode)
            {
                AppendLog($"Fresh rebuild of {Database} started.");
                result = await engine.RebuildAsync(Database, progress, _cancellation.Token).ConfigureAwait(true);
            }
            else
            {
                var plan = await BuildPlanAsync(gateway, Database).ConfigureAwait(true);

                result = await engine.ApplyAsync(plan, RemoveDatabaseOnlyObjects, progress, _cancellation.Token).ConfigureAwait(true);
            }

            var reportPath = SaveReport(result, gateway);
            StatusMessage = result.IsSuccess
                ? $"Deployment finished. {result.AppliedCount} applied, {result.BenignSkipCount} already applied. Report: {reportPath}"
                : $"Deployment finished with {result.FailedCount} failure(s). Report: {reportPath}";

            AppendLog(StatusMessage);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Deployment cancelled.";
            AppendLog(StatusMessage);
        }
        catch (Exception exception)
        {
            StatusMessage = "Deployment failed: " + exception.Message;
            AppendLog(StatusMessage);
        }
        finally
        {
            IsBusy = false;
            ProgressMessage = string.Empty;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _cancellation?.Cancel();
    }

    [RelayCommand]
    private void ExportPlan()
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MTM Database Deployment",
                "reports");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"deployment-plan-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Target: {ActiveServerText} | database {Database}");
            builder.AppendLine(PlanSummary);
            builder.AppendLine(new string('-', 100));
            foreach (var change in PlanChanges)
            {
                builder.AppendLine(change.DisplayText);
            }

            File.WriteAllText(path, builder.ToString());
            StatusMessage = "Preview exported: " + path;
            AppendLog(StatusMessage);
        }
        catch (Exception exception)
        {
            StatusMessage = "Export failed: " + exception.Message;
        }
    }

    private string SaveReport(Model_DeploymentResult result, Service_MySqlGateway gateway)
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MTM Database Deployment",
                "reports");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"deployment-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            var builder = new System.Text.StringBuilder();
            builder.AppendLine($"Database: {Database}");
            builder.AppendLine($"Server: {gateway.ActiveServer}:{gateway.Port}");
            builder.AppendLine($"Mode: {(IsRebuildMode ? "Fresh rebuild" : "Update in place")}");
            builder.AppendLine($"Completed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine(result.ToReportText());
            File.WriteAllText(path, builder.ToString());
            return path;
        }
        catch (Exception exception)
        {
            return "(report not saved: " + exception.Message + ")";
        }
    }
}
