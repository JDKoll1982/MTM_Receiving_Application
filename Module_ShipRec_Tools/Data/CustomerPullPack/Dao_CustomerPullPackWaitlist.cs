using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;

/// <summary>
/// Persists and retrieves Customer Pull n' Pack waitlist work through MTM-managed stored procedures.
/// </summary>
public class Dao_CustomerPullPackWaitlist
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
    };

    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dao_CustomerPullPackWaitlist"/> class.
    /// </summary>
    /// <param name="connectionString">MySQL application connection string.</param>
    public Dao_CustomerPullPackWaitlist(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Implements Workflow 2.1, 2.2, and 2.3 by saving one waitlist entry and surfacing duplicate-open results.
    /// </summary>
    /// <param name="entry">Waitlist entry payload to create or update.</param>
    public virtual async Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> UpsertAsync(
        Model_CustomerPullPack_WaitlistEntry entry
    )
    {
        ArgumentNullException.ThrowIfNull(entry);

        var waitlistIdParameter = new MySqlParameter("@p_waitlist_id", MySqlDbType.VarChar, 36)
        {
            Direction = ParameterDirection.InputOutput,
            Value = string.IsNullOrWhiteSpace(entry.WaitlistId)
                ? DBNull.Value
                : entry.WaitlistId.Trim(),
        };
        var duplicateWaitlistIdParameter = new MySqlParameter(
            "@p_duplicate_existing_waitlist_id",
            MySqlDbType.VarChar,
            36
        )
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("@p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorMessageParameter = new MySqlParameter(
            "@p_error_message",
            MySqlDbType.VarChar,
            1000
        )
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            waitlistIdParameter,
            new("@p_source_line_key", entry.SourceLineKey ?? string.Empty),
            new("@p_customer_id", entry.CustomerId ?? string.Empty),
            new("@p_customer_name", entry.CustomerName ?? string.Empty),
            new("@p_customer_order_id", entry.CustomerOrderId ?? string.Empty),
            new("@p_parent_part_id", entry.ParentPartId ?? string.Empty),
            new("@p_requested_quantity", entry.RequestedQuantity),
            new("@p_requested_by_user_id", entry.RequestedByUserId ?? string.Empty),
            new("@p_requested_by_display_name", entry.RequestedByDisplayName ?? string.Empty),
            new("@p_requester_context_note", entry.RequesterContextNote ?? string.Empty),
            new("@p_current_status", entry.CurrentStatus.ToString()),
            new("@p_current_owner_user_id", entry.CurrentOwnerUserId ?? string.Empty),
            new("@p_current_owner_display_name", entry.CurrentOwnerDisplayName ?? string.Empty),
            new("@p_location_review_flag", entry.LocationReviewFlag),
            new("@p_problem_reason", entry.ProblemReason.ToString()),
            new("@p_handler_note", entry.HandlerNote ?? string.Empty),
            new("@p_completion_user_id", entry.CompletionUserId ?? string.Empty),
            new("@p_completion_timestamp", (object?)entry.CompletionTimestamp ?? DBNull.Value),
            new("@p_last_updated_by_user_id", entry.LastUpdatedByUserId ?? string.Empty),
            new("@p_recheck_indicator", entry.RecheckIndicator),
            new(
                "@p_selected_locations_json",
                JsonSerializer.Serialize(
                    entry
                        .SelectedLocations.Where(static location =>
                            string.IsNullOrWhiteSpace(location) is false
                        )
                        .Select(static location => location.Trim().ToUpperInvariant())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    SerializerOptions
                )
            ),
            duplicateWaitlistIdParameter,
            statusParameter,
            errorMessageParameter,
        };

        var executionResult = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_CustomerPullPack_Waitlist_Upsert",
            parameters,
            _connectionString
        );

        if (!executionResult.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                executionResult.ErrorMessage,
                executionResult.Exception
            );
        }

        var procedureStatus =
            statusParameter.Value is DBNull ? 0 : Convert.ToInt32(statusParameter.Value);
        var procedureErrorMessage =
            errorMessageParameter.Value is DBNull
                ? string.Empty
                : errorMessageParameter.Value?.ToString() ?? string.Empty;
        var duplicateWaitlistId =
            duplicateWaitlistIdParameter.Value is DBNull
                ? string.Empty
                : duplicateWaitlistIdParameter.Value?.ToString() ?? string.Empty;
        var resolvedWaitlistId =
            waitlistIdParameter.Value is DBNull
                ? string.Empty
                : waitlistIdParameter.Value?.ToString() ?? string.Empty;

        if (procedureStatus == 2)
        {
            var duplicateEntry = CloneEntry(entry);
            duplicateEntry.WaitlistId = duplicateWaitlistId;

            return new Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
            {
                Success = false,
                Data = duplicateEntry,
                ErrorMessage = string.IsNullOrWhiteSpace(procedureErrorMessage)
                    ? "An open waitlist item already exists for this report line."
                    : procedureErrorMessage,
                Severity = Enum_ErrorSeverity.Warning,
                ReturnValue = duplicateWaitlistId,
            };
        }

        if (procedureStatus != 0)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                string.IsNullOrWhiteSpace(procedureErrorMessage)
                    ? "Customer Pull n' Pack waitlist save failed."
                    : procedureErrorMessage
            );
        }

        var savedEntry = CloneEntry(entry);
        savedEntry.WaitlistId = resolvedWaitlistId;

        return new Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>
        {
            Success = true,
            Data = savedEntry,
            AffectedRows = executionResult.AffectedRows,
            ReturnValue = resolvedWaitlistId,
        };
    }

    /// <summary>
    /// Implements Workflow 3.1 by loading queue items with optional status, location, customer, requester, or owner filters.
    /// </summary>
    /// <param name="waitlistId"></param>
    /// <param name="customerId"></param>
    /// <param name="requesterUserId"></param>
    /// <param name="currentOwnerUserId"></param>
    /// <param name="locationId"></param>
    /// <param name="statusSet"></param>
    /// <param name="useDefaultOpenWork"></param>
    /// <param name="maxResults"></param>
    public virtual Task<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>> GetQueueAsync(
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
        var parameters = new Dictionary<string, object>
        {
            {
                "p_waitlist_id",
                string.IsNullOrWhiteSpace(waitlistId) ? DBNull.Value : waitlistId.Trim()
            },
            {
                "p_customer_id",
                string.IsNullOrWhiteSpace(customerId) ? DBNull.Value : customerId.Trim()
            },
            {
                "p_requester_user_id",
                string.IsNullOrWhiteSpace(requesterUserId) ? DBNull.Value : requesterUserId.Trim()
            },
            {
                "p_current_owner_user_id",
                string.IsNullOrWhiteSpace(currentOwnerUserId)
                    ? DBNull.Value
                    : currentOwnerUserId.Trim()
            },
            {
                "p_location_id",
                string.IsNullOrWhiteSpace(locationId) ? DBNull.Value : locationId.Trim()
            },
            { "p_status_set_json", SerializeStatusSet(statusSet) ?? (object)DBNull.Value },
            { "p_use_default_open_work", useDefaultOpenWork },
            { "p_max_results", maxResults },
        };

        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_CustomerPullPack_Waitlist_GetQueue",
            MapWaitlistEntry,
            parameters
        );
    }

    /// <summary>
    /// Retrieves one waitlist item by its stable waitlist ID.
    /// </summary>
    /// <param name="waitlistId"></param>
    public virtual async Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> GetByIdAsync(
        string waitlistId
    )
    {
        if (string.IsNullOrWhiteSpace(waitlistId))
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                "Waitlist ID is required."
            );
        }

        var queueResult = await GetQueueAsync(
            waitlistId: waitlistId,
            useDefaultOpenWork: false,
            maxResults: 1
        );
        if (!queueResult.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                queueResult.ErrorMessage,
                queueResult.Exception
            );
        }

        var waitlistEntry = queueResult.Data?.FirstOrDefault();
        if (waitlistEntry is null)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_WaitlistEntry>(
                "Waitlist item not found."
            );
        }

        return Model_Dao_Result_Factory.Success(waitlistEntry, 1);
    }

    private static Model_CustomerPullPack_WaitlistEntry MapWaitlistEntry(IDataReader reader)
    {
        return new Model_CustomerPullPack_WaitlistEntry
        {
            WaitlistId = ReadString(reader, "WaitlistId"),
            SourceLineKey = ReadString(reader, "SourceLineKey"),
            CustomerId = ReadString(reader, "CustomerId"),
            CustomerName = ReadString(reader, "CustomerName"),
            CustomerOrderId = ReadString(reader, "CustomerOrderId"),
            ParentPartId = ReadString(reader, "ParentPartId"),
            RequestedQuantity = ReadDecimal(reader, "RequestedQuantity"),
            SelectedLocations = SplitLocations(ReadString(reader, "SelectedLocations")),
            RequestedByUserId = ReadString(reader, "RequestedByUserId"),
            RequestedByDisplayName = ReadString(reader, "RequestedByDisplayName"),
            RequesterContextNote = ReadString(reader, "RequesterContextNote"),
            CurrentStatus = ParseStatus(ReadString(reader, "CurrentStatus")),
            CurrentOwnerUserId = ReadString(reader, "CurrentOwnerUserId"),
            CurrentOwnerDisplayName = ReadString(reader, "CurrentOwnerDisplayName"),
            LocationReviewFlag = ReadBoolean(reader, "LocationReviewFlag"),
            ProblemReason = ParseProblemReason(ReadString(reader, "ProblemReason")),
            HandlerNote = ReadString(reader, "HandlerNote"),
            CompletionUserId = ReadString(reader, "CompletionUserId"),
            CompletionTimestamp = ReadNullableDateTime(reader, "CompletionTimestamp"),
            LastUpdatedByUserId = ReadString(reader, "LastUpdatedByUserId"),
            LastUpdatedTimestamp = ReadDateTime(reader, "LastUpdatedTimestamp"),
            RequestTimestamp = ReadDateTime(reader, "RequestTimestamp"),
            RecheckIndicator = ReadBoolean(reader, "RecheckIndicator"),
        };
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

    private static string? SerializeStatusSet(
        IReadOnlyCollection<Enum_CustomerPullPackWaitlistStatus>? statusSet
    )
    {
        if (statusSet is null || statusSet.Count == 0)
        {
            return null;
        }

        var names = statusSet.Select(static status => status.ToString()).Distinct().ToList();
        return JsonSerializer.Serialize(names, SerializerOptions);
    }

    private static List<string> SplitLocations(string joinedLocations)
    {
        if (string.IsNullOrWhiteSpace(joinedLocations))
        {
            return [];
        }

        return joinedLocations
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Enum_CustomerPullPackWaitlistStatus ParseStatus(string value)
    {
        return Enum.TryParse<Enum_CustomerPullPackWaitlistStatus>(value, true, out var parsed)
            ? parsed
            : Enum_CustomerPullPackWaitlistStatus.Requested;
    }

    private static Enum_CustomerPullPackProblemReason ParseProblemReason(string value)
    {
        return Enum.TryParse<Enum_CustomerPullPackProblemReason>(value, true, out var parsed)
            ? parsed
            : Enum_CustomerPullPackProblemReason.None;
    }

    private static string ReadString(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static decimal ReadDecimal(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static bool ReadBoolean(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
        {
            return false;
        }

        var value = reader.GetValue(ordinal);
        return value switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            _ => Convert.ToBoolean(value),
        };
    }

    private static DateTime ReadDateTime(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? DateTime.UtcNow : reader.GetDateTime(ordinal);
    }

    private static DateTime? ReadNullableDateTime(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
