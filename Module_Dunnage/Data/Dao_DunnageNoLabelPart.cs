using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

/// <summary>
/// Data access for <c>dunnage_no_label_parts</c>, the part numbers that need no printed label.
/// </summary>
/// <remarks>
/// The flag is held in its own table rather than as a column on <c>dunnage_parts</c> so the part screens can
/// turn it on and off without the part insert and update procedures growing another parameter, and so the
/// part row that every other query reads stays exactly as it was.
/// </remarks>
public class Dao_DunnageNoLabelPart
{
    private readonly string _connectionString;

    public Dao_DunnageNoLabelPart(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>The part numbers that need no printed label.</summary>
    public virtual async Task<Model_Dao_Result<List<string>>> GetPartIdsAsync()
    {
        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<string>(
            _connectionString,
            "sp_Dunnage_NoLabelParts_GetAll",
            reader =>
            {
                var value = reader["part_id"];

                return value is DBNull || value is null ? string.Empty : Convert.ToString(value) ?? string.Empty;
            }
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<List<string>>(
                result.ErrorMessage ?? "The no-label part list could not be read.",
                result.Exception
            );
        }

        var partIds = new List<string>();

        foreach (var partId in result.Data ?? [])
        {
            if (!string.IsNullOrWhiteSpace(partId))
            {
                partIds.Add(partId.Trim());
            }
        }

        return Model_Dao_Result_Factory.Success(partIds);
    }

    /// <summary>
    /// Turns the flag on or off for one part.
    /// </summary>
    /// <param name="partId">The part the flag belongs to.</param>
    /// <param name="needsNoLabel">True to add the part to the list, false to take it off.</param>
    /// <param name="user">The user making the change.</param>
    public virtual async Task<Model_Dao_Result> SetAsync(string partId, bool needsNoLabel, string user)
    {
        if (string.IsNullOrWhiteSpace(partId))
        {
            return Model_Dao_Result_Factory.Failure(
                "A part number is needed before it can be marked as needing no label."
            );
        }

        var parameters = new[]
        {
            new MySqlParameter("p_part_id", MySqlDbType.VarChar, 50) { Value = partId.Trim() },
            new MySqlParameter("p_needs_no_label", MySqlDbType.Byte)
            {
                Value = needsNoLabel ? 1 : 0,
            },
            new MySqlParameter("p_user", MySqlDbType.VarChar, 50)
            {
                Value = user ?? string.Empty,
            },
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_NoLabelParts_Set",
            parameters,
            _connectionString
        );

        return result.IsSuccess
            ? Model_Dao_Result_Factory.Success()
            : Model_Dao_Result_Factory.Failure(
                result.ErrorMessage ?? "The no-label flag could not be saved.",
                result.Exception
            );
    }
}
