using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;

/// <summary>
/// Persists and retrieves saved Customer Pull n' Pack user defaults.
/// </summary>
public class Dao_CustomerPullPackUserDefaults
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
    };

    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dao_CustomerPullPackUserDefaults"/> class.
    /// </summary>
    /// <param name="connectionString">MySQL application connection string.</param>
    public Dao_CustomerPullPackUserDefaults(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Saves or replaces one user's Customer Pull n' Pack defaults row.
    /// </summary>
    /// <param name="defaults"></param>
    /// <param name="updatedByUserId"></param>
    public async Task<Model_Dao_Result<Model_CustomerPullPack_UserDefaults>> UpsertAsync(
        Model_CustomerPullPack_UserDefaults defaults,
        string updatedByUserId
    )
    {
        ArgumentNullException.ThrowIfNull(defaults);

        var statusParameter = new MySqlParameter("@p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorMessageParameter = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            new("@p_user_id", defaults.UserId ?? string.Empty),
            new("@p_default_customer_id", defaults.DefaultCustomerId ?? string.Empty),
            new(
                "@p_favorite_customer_ids_json",
                JsonSerializer.Serialize(
                    defaults
                        .FavoriteCustomerIds.Where(static customerId =>
                            string.IsNullOrWhiteSpace(customerId) is false
                        )
                        .Select(static customerId => customerId.Trim().ToUpperInvariant())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    SerializerOptions
                )
            ),
            new("@p_last_good_date_range_type", defaults.LastGoodDateRangeType ?? string.Empty),
            new("@p_last_good_date_from", (object?)defaults.LastGoodDateFrom ?? DBNull.Value),
            new("@p_last_good_date_to", (object?)defaults.LastGoodDateTo ?? DBNull.Value),
            new("@p_default_sort_mode", defaults.DefaultSortMode.ToString()),
            new("@p_default_shortages_only", defaults.DefaultShortagesOnly),
            new("@p_default_unpulled_only", defaults.DefaultUnpulledOnly),
            new("@p_default_late_orders_only", defaults.DefaultLateOrdersOnly),
            new(
                "@p_default_waitlist_status_set_json",
                JsonSerializer.Serialize(
                    defaults
                        .DefaultWaitlistStatusSet.Select(static status => status.ToString())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList(),
                    SerializerOptions
                )
            ),
            new("@p_default_print_preset", defaults.DefaultPrintPreset.ToString()),
            new("@p_last_updated_by_user_id", updatedByUserId ?? string.Empty),
            statusParameter,
            errorMessageParameter,
        };

        var executionResult = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_CustomerPullPack_UserDefaults_Upsert",
            parameters,
            _connectionString
        );

        if (!executionResult.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_UserDefaults>(
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

        if (procedureStatus != 0)
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_UserDefaults>(
                string.IsNullOrWhiteSpace(procedureErrorMessage)
                    ? "Customer Pull n' Pack user-defaults save failed."
                    : procedureErrorMessage
            );
        }

        return Model_Dao_Result_Factory.Success(defaults, executionResult.AffectedRows);
    }

    /// <summary>
    /// Loads one user's saved defaults or returns a feature-default row when none has been saved yet.
    /// </summary>
    /// <param name="userId"></param>
    public async Task<Model_Dao_Result<Model_CustomerPullPack_UserDefaults>> GetByUserIdAsync(
        string userId
    )
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_UserDefaults>(
                "User ID is required."
            );
        }

        var parameters = new Dictionary<string, object> { { "p_user_id", userId.Trim() } };
        var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_CustomerPullPack_UserDefaults_GetByUser",
            MapUserDefaults,
            parameters
        );

        if (result.IsSuccess && result.Data is not null)
        {
            return result;
        }

        if (
            string.Equals(
                result.ErrorMessage,
                "No record found",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return Model_Dao_Result_Factory.Success(CreateDefaultDefaults(userId));
        }

        return Model_Dao_Result_Factory.Failure<Model_CustomerPullPack_UserDefaults>(
            result.ErrorMessage,
            result.Exception
        );
    }

    private static Model_CustomerPullPack_UserDefaults MapUserDefaults(IDataReader reader)
    {
        return new Model_CustomerPullPack_UserDefaults
        {
            UserId = ReadString(reader, "UserId"),
            DefaultCustomerId = ReadString(reader, "DefaultCustomerId"),
            FavoriteCustomerIds = DeserializeStringList(
                ReadString(reader, "FavoriteCustomerIdsJson")
            ),
            LastGoodDateRangeType = ReadString(reader, "LastGoodDateRangeType"),
            LastGoodDateFrom = ReadNullableDateTime(reader, "LastGoodDateFrom"),
            LastGoodDateTo = ReadNullableDateTime(reader, "LastGoodDateTo"),
            DefaultSortMode = ParseSortMode(ReadString(reader, "DefaultSortMode")),
            DefaultShortagesOnly = ReadBoolean(reader, "DefaultShortagesOnly"),
            DefaultUnpulledOnly = ReadBoolean(reader, "DefaultUnpulledOnly"),
            DefaultLateOrdersOnly = ReadBoolean(reader, "DefaultLateOrdersOnly"),
            DefaultWaitlistStatusSet = DeserializeStatusList(
                ReadString(reader, "DefaultWaitlistStatusSetJson")
            ),
            DefaultPrintPreset = ParsePrintMode(ReadString(reader, "DefaultPrintPreset")),
        };
    }

    private static Model_CustomerPullPack_UserDefaults CreateDefaultDefaults(string userId)
    {
        return new Model_CustomerPullPack_UserDefaults
        {
            UserId = userId,
            DefaultWaitlistStatusSet =
            [
                Enum_CustomerPullPackWaitlistStatus.Requested,
                Enum_CustomerPullPackWaitlistStatus.Accepted,
                Enum_CustomerPullPackWaitlistStatus.Problem,
            ],
        };
    }

    private static List<string> DeserializeStringList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, SerializerOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<Enum_CustomerPullPackWaitlistStatus> DeserializeStatusList(string json)
    {
        return DeserializeStringList(json).Select(ParseStatus).Distinct().ToList();
    }

    private static Enum_CustomerPullPackWaitlistStatus ParseStatus(string value)
    {
        return Enum.TryParse<Enum_CustomerPullPackWaitlistStatus>(value, true, out var parsed)
            ? parsed
            : Enum_CustomerPullPackWaitlistStatus.Requested;
    }

    private static Enum_CustomerPullPackSortMode ParseSortMode(string value)
    {
        return Enum.TryParse<Enum_CustomerPullPackSortMode>(value, true, out var parsed)
            ? parsed
            : Enum_CustomerPullPackSortMode.PullDate;
    }

    private static Enum_CustomerPullPackPrintMode ParsePrintMode(string value)
    {
        return Enum.TryParse<Enum_CustomerPullPackPrintMode>(value, true, out var parsed)
            ? parsed
            : Enum_CustomerPullPackPrintMode.CurrentView;
    }

    private static string ReadString(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
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

    private static DateTime? ReadNullableDateTime(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
