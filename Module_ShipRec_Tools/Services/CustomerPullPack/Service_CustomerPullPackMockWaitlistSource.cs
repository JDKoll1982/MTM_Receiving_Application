using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Service_CustomerPullPackMockWaitlistSource
    : IService_CustomerPullPackWaitlistSource
{
    private readonly IService_CustomerPullPackMockDataCatalog _mockDataCatalog;
    private readonly IService_LoggingUtility? _logger;
    private readonly object _syncLock = new();
    private readonly Dictionary<string, Model_CustomerPullPack_WaitlistEntry> _entries = new(
        StringComparer.OrdinalIgnoreCase
    );

    private bool _isInitialized;
    private int _nextSequence = 1000;

    public Service_CustomerPullPackMockWaitlistSource(
        IService_CustomerPullPackMockDataCatalog mockDataCatalog,
        IService_LoggingUtility? logger = null
    )
    {
        _mockDataCatalog = mockDataCatalog;
        _logger = logger;
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> UpsertAsync(
        Model_CustomerPullPack_WaitlistEntry entry
    )
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_syncLock)
        {
            EnsureInitialized();

            var normalizedEntry = CloneEntry(entry);
            normalizedEntry.WaitlistId = normalizedEntry.WaitlistId.Trim();

            if (string.IsNullOrWhiteSpace(normalizedEntry.WaitlistId))
            {
                var duplicateEntry = _entries.Values.FirstOrDefault(existing =>
                    string.Equals(
                        existing.SourceLineKey,
                        normalizedEntry.SourceLineKey,
                        StringComparison.OrdinalIgnoreCase
                    ) && IsOpenStatus(existing.CurrentStatus)
                );

                if (duplicateEntry is not null)
                {
                    return Task.FromResult(
                        new Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
                        {
                            Success = false,
                            Data = CloneEntry(duplicateEntry),
                            ErrorMessage =
                                "An open waitlist item already exists for this report line.",
                            Severity = Enum_ErrorSeverity.Warning,
                            ReturnValue = duplicateEntry.WaitlistId,
                        }
                    );
                }

                normalizedEntry.WaitlistId = GenerateWaitlistId();
                if (normalizedEntry.RequestTimestamp == default)
                {
                    normalizedEntry.RequestTimestamp = DateTime.UtcNow;
                }
            }

            if (normalizedEntry.LastUpdatedTimestamp == default)
            {
                normalizedEntry.LastUpdatedTimestamp = DateTime.UtcNow;
            }

            _entries[normalizedEntry.WaitlistId] = CloneEntry(normalizedEntry);
            return Task.FromResult(
                new Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
                {
                    Success = true,
                    Data = CloneEntry(normalizedEntry),
                    ReturnValue = normalizedEntry.WaitlistId,
                    AffectedRows = 1,
                }
            );
        }
    }

    public Task<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>> GetQueueAsync(
        string? waitlistId = null,
        string? customerId = null,
        string? requesterUserId = null,
        string? currentOwnerUserId = null,
        string? locationId = null,
        IReadOnlyCollection<Enum_CustomerPullPackWaitlistStatus>? statusSet = null,
        bool useDefaultOpenWork = true,
        int maxResults = 250
    )
    {
        lock (_syncLock)
        {
            EnsureInitialized();

            IEnumerable<Model_CustomerPullPack_WaitlistEntry> entries = _entries.Values.Select(
                CloneEntry
            );

            if (string.IsNullOrWhiteSpace(waitlistId) is false)
            {
                entries = entries.Where(entry =>
                    string.Equals(
                        entry.WaitlistId,
                        waitlistId.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(customerId) is false)
            {
                entries = entries.Where(entry =>
                    string.Equals(
                        entry.CustomerId,
                        customerId.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(requesterUserId) is false)
            {
                entries = entries.Where(entry =>
                    string.Equals(
                        entry.RequestedByUserId,
                        requesterUserId.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(currentOwnerUserId) is false)
            {
                entries = entries.Where(entry =>
                    string.Equals(
                        entry.CurrentOwnerUserId,
                        currentOwnerUserId.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(locationId) is false)
            {
                entries = entries.Where(entry =>
                    entry.SelectedLocations.Contains(
                        locationId.Trim(),
                        StringComparer.OrdinalIgnoreCase
                    )
                );
            }

            if (useDefaultOpenWork)
            {
                entries = entries.Where(entry => IsOpenStatus(entry.CurrentStatus));
            }
            else if (statusSet is not null && statusSet.Count > 0)
            {
                var normalizedStatuses = new HashSet<Enum_CustomerPullPackWaitlistStatus>(
                    statusSet
                );
                entries = entries.Where(entry => normalizedStatuses.Contains(entry.CurrentStatus));
            }

            var results = entries
                .OrderBy(entry => entry.CustomerId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.CustomerOrderId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.ParentPartId, StringComparer.OrdinalIgnoreCase)
                .Take(maxResults)
                .ToList();

            return Task.FromResult(Model_Dao_Result_Factory.Success(results, results.Count));
        }
    }

    public Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> GetByIdAsync(
        string waitlistId
    )
    {
        if (string.IsNullOrWhiteSpace(waitlistId))
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                    "Waitlist ID is required."
                )
            );
        }

        lock (_syncLock)
        {
            EnsureInitialized();

            if (_entries.TryGetValue(waitlistId.Trim(), out var entry))
            {
                return Task.FromResult(Model_Dao_Result_Factory.Success(CloneEntry(entry), 1));
            }

            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                    "Waitlist item not found."
                )
            );
        }
    }

    internal Model_CustomerPullPack_WaitlistEntry? GetBySourceLineKey(string sourceLineKey)
    {
        if (string.IsNullOrWhiteSpace(sourceLineKey))
        {
            return null;
        }

        lock (_syncLock)
        {
            EnsureInitialized();

            return _entries
                .Values.Where(entry =>
                    string.Equals(
                        entry.SourceLineKey,
                        sourceLineKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .OrderByDescending(entry => entry.LastUpdatedTimestamp)
                .Select(CloneEntry)
                .FirstOrDefault();
        }
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
        {
            return;
        }

        var demandRows = _mockDataCatalog.GetDemandRows();
        var locationRows = _mockDataCatalog.GetLocationRows();

        foreach (var row in demandRows.Where(static item => item.HasLinkedWaitlist))
        {
            if (string.IsNullOrWhiteSpace(row.LinkedWaitlistId))
            {
                continue;
            }

            var entry = CreateSeedEntry(row, locationRows);
            _entries[entry.WaitlistId] = entry;
            _nextSequence = Math.Max(_nextSequence, ExtractNumericSuffix(entry.WaitlistId) + 1);
        }

        _isInitialized = true;
        _logger?.LogInfo("Initialized Customer Pull n' Pack mock waitlist source.");
    }

    private static Model_CustomerPullPack_WaitlistEntry CreateSeedEntry(
        Model_InforVisualCustomerPullPackDemandRow row,
        IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> locationRows
    )
    {
        var selectedLocations = locationRows
            .Where(location =>
                string.Equals(
                    location.SourceLineKey,
                    row.SourceLineKey,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Where(static location => location.InitiallySelected)
            .Select(static location => location.LocationId)
            .Where(static locationId => string.IsNullOrWhiteSpace(locationId) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var hasAnyLocationRows = locationRows.Any(location =>
            string.Equals(
                location.SourceLineKey,
                row.SourceLineKey,
                StringComparison.OrdinalIgnoreCase
            )
        );
        var status = ParseStatus(row.LinkedWaitlistStatus);
        DateTime? completionTimestamp =
            status == Enum_CustomerPullPackWaitlistStatus.Completed ? row.PullDate : null;

        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = row.LinkedWaitlistId,
            SourceLineKey = row.SourceLineKey,
            CustomerId = row.CustomerId,
            CustomerName = row.CustomerName,
            CustomerOrderId = row.CustomerOrderId,
            ParentPartId = row.ParentPartId,
            RequestedQuantity = row.QuantityToPack,
            SelectedLocations = selectedLocations,
            RequestedByUserId = "mock.requester",
            RequestedByDisplayName = "Mock Requester",
            RequesterContextNote = row.RequesterNote,
            CurrentStatus = status,
            CurrentOwnerUserId = string.Empty,
            CurrentOwnerDisplayName = string.Empty,
            LocationReviewFlag = hasAnyLocationRows is false,
            ProblemReason = Enum_CustomerPullPackProblemReason.None,
            HandlerNote = string.Empty,
            CompletionUserId = completionTimestamp.HasValue ? "mock.handler" : string.Empty,
            CompletionTimestamp = completionTimestamp,
            LastUpdatedByUserId = "mock.catalog",
            LastUpdatedTimestamp = row.PullDate,
            RequestTimestamp = row.PullDate,
            RecheckIndicator = row.RecheckIndicator,
        };
    }

    private string GenerateWaitlistId()
    {
        var waitlistId = $"CPP-WL-MOCK-{_nextSequence:0000}";
        _nextSequence++;
        return waitlistId;
    }

    private static int ExtractNumericSuffix(string waitlistId)
    {
        var digits = new string(waitlistId.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var parsed) ? parsed : 0;
    }

    private static bool IsOpenStatus(Enum_CustomerPullPackWaitlistStatus status)
    {
        return status
            is Enum_CustomerPullPackWaitlistStatus.Requested
                or Enum_CustomerPullPackWaitlistStatus.Accepted
                or Enum_CustomerPullPackWaitlistStatus.Problem;
    }

    private static Enum_CustomerPullPackWaitlistStatus ParseStatus(string value)
    {
        return Enum.TryParse<Enum_CustomerPullPackWaitlistStatus>(value, true, out var parsed)
            ? parsed
            : Enum_CustomerPullPackWaitlistStatus.Requested;
    }

    private static Model_CustomerPullPack_WaitlistEntry CloneEntry(
        Model_CustomerPullPack_WaitlistEntry entry
    )
    {
        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = entry.WaitlistId,
            SourceLineKey = entry.SourceLineKey,
            CustomerId = entry.CustomerId,
            CustomerName = entry.CustomerName,
            CustomerOrderId = entry.CustomerOrderId,
            ParentPartId = entry.ParentPartId,
            RequestedQuantity = entry.RequestedQuantity,
            SelectedLocations = entry.SelectedLocations.ToList(),
            RequestedByUserId = entry.RequestedByUserId,
            RequestedByDisplayName = entry.RequestedByDisplayName,
            RequesterContextNote = entry.RequesterContextNote,
            CurrentStatus = entry.CurrentStatus,
            CurrentOwnerUserId = entry.CurrentOwnerUserId,
            CurrentOwnerDisplayName = entry.CurrentOwnerDisplayName,
            LocationReviewFlag = entry.LocationReviewFlag,
            ProblemReason = entry.ProblemReason,
            HandlerNote = entry.HandlerNote,
            CompletionUserId = entry.CompletionUserId,
            CompletionTimestamp = entry.CompletionTimestamp,
            LastUpdatedByUserId = entry.LastUpdatedByUserId,
            LastUpdatedTimestamp = entry.LastUpdatedTimestamp,
            RequestTimestamp = entry.RequestTimestamp,
            RecheckIndicator = entry.RecheckIndicator,
        };
    }
}
