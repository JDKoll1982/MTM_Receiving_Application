using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_OutsideService.Data;

/// <summary>
/// Data access for the Outside Service module.
/// </summary>
public class Dao_OutsideServiceRequest
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new DAO instance.
    /// </summary>
    /// <param name="connectionString"></param>
    public Dao_OutsideServiceRequest(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a request header, its lines, and package rows within one transaction.
    /// </summary>
    /// <param name="request"></param>
    public async Task<Model_Dao_Result<Model_OutsideServiceRequest>> CreateRequestAsync(
        Model_OutsideServiceRequest request
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var header = await InsertHeaderAsync(
                connection,
                (MySqlTransaction)transaction,
                request
            );
            if (!header.IsSuccess || header.Data is null)
            {
                await transaction.RollbackAsync();
                return header;
            }

            request.OutsideServiceRequestId = header.Data.OutsideServiceRequestId;
            request.RequestNumber = header.Data.RequestNumber;
            request.CreatedUtc = header.Data.CreatedUtc;

            foreach (var line in request.Lines.OrderBy(line => line.LineNumber))
            {
                line.OutsideServiceRequestId = request.OutsideServiceRequestId;
                line.RequestNumber = request.RequestNumber;
                line.CreatedByUser = request.CreatedByUser;
                line.CreatedByDisplay = request.CreatedByDisplay;
                line.CreatedUtc = request.CreatedUtc;
                line.RequestNotes = request.RequestNotes;

                var lineInsert = await InsertLineAsync(
                    connection,
                    (MySqlTransaction)transaction,
                    line
                );
                if (!lineInsert.IsSuccess || lineInsert.Data <= 0)
                {
                    await transaction.RollbackAsync();
                    return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                        lineInsert.ErrorMessage,
                        lineInsert.Exception
                    );
                }

                line.OutsideServiceRequestLineId = lineInsert.Data;

                foreach (var package in line.Packages.OrderBy(package => package.PackageSequence))
                {
                    package.OutsideServiceRequestLineId = line.OutsideServiceRequestLineId;
                    var packageResult =
                        await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                            connection,
                            (MySqlTransaction)transaction,
                            "sp_OutsideService_RequestPackage_Insert",
                            new Dictionary<string, object>
                            {
                                {
                                    "outside_service_request_line_id",
                                    package.OutsideServiceRequestLineId
                                },
                                { "package_sequence", package.PackageSequence },
                                { "package_quantity", package.PackageQuantity },
                            }
                        );

                    if (!packageResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                            packageResult.ErrorMessage,
                            packageResult.Exception
                        );
                    }
                }
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success(request);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                $"Failed to create Outside Service request: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns all active lines.
    /// </summary>
    public Task<Model_Dao_Result<List<Model_OutsideServiceRequestLine>>> GetOpenLinesAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_OutsideService_Request_GetOpen",
            MapLineFromReader
        );
    }

    /// <summary>
    /// Returns all completed lines.
    /// </summary>
    public Task<Model_Dao_Result<List<Model_OutsideServiceRequestLine>>> GetCompletedLinesAsync()
    {
        return Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_OutsideService_Request_GetCompleted",
            MapLineFromReader
        );
    }

    /// <summary>
    /// Saves Setup-phase data.
    /// </summary>
    /// <param name="line"></param>
    public Task<Model_Dao_Result> SaveSetupAsync(Model_OutsideServiceRequestLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_OutsideService_RequestLine_UpdateSetup",
            new Dictionary<string, object>
            {
                { "outside_service_request_line_id", line.OutsideServiceRequestLineId },
                { "package_count", line.PackageCount },
                { "package_summary", line.PackageSummary },
                {
                    "setup_vendor_id",
                    string.IsNullOrWhiteSpace(line.SetupVendorId)
                        ? DBNull.Value
                        : line.SetupVendorId
                },
                {
                    "setup_vendor_name",
                    string.IsNullOrWhiteSpace(line.SetupVendorName)
                        ? DBNull.Value
                        : line.SetupVendorName
                },
                {
                    "setup_vendor_source",
                    string.IsNullOrWhiteSpace(line.SetupVendorSource)
                        ? DBNull.Value
                        : line.SetupVendorSource
                },
                {
                    "bol_number",
                    string.IsNullOrWhiteSpace(line.BOLNumber) ? DBNull.Value : line.BOLNumber
                },
                {
                    "scheduled_ship_utc",
                    line.ScheduledShipUtc.HasValue ? line.ScheduledShipUtc.Value : DBNull.Value
                },
                {
                    "shipping_contact",
                    string.IsNullOrWhiteSpace(line.ShippingContact)
                        ? DBNull.Value
                        : line.ShippingContact
                },
                {
                    "setup_notes",
                    string.IsNullOrWhiteSpace(line.SetupNotes) ? DBNull.Value : line.SetupNotes
                },
            }
        );
    }

    /// <summary>
    /// Marks a line complete.
    /// </summary>
    /// <param name="lineId"></param>
    /// <param name="completionNotes"></param>
    public Task<Model_Dao_Result> MarkCompleteAsync(int lineId, string? completionNotes)
    {
        return Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_OutsideService_RequestLine_MarkComplete",
            new Dictionary<string, object>
            {
                { "outside_service_request_line_id", lineId },
                {
                    "completion_notes",
                    string.IsNullOrWhiteSpace(completionNotes) ? DBNull.Value : completionNotes
                },
            }
        );
    }

    private static async Task<Model_Dao_Result<Model_OutsideServiceRequest>> InsertHeaderAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        Model_OutsideServiceRequest request
    )
    {
        await using var command = new MySqlCommand(
            "sp_OutsideService_Request_Insert",
            connection,
            transaction
        )
        {
            CommandType = CommandType.StoredProcedure,
        };

        command.Parameters.AddWithValue("@p_created_by_user", request.CreatedByUser);
        command.Parameters.AddWithValue("@p_created_by_display", request.CreatedByDisplay);
        command.Parameters.AddWithValue(
            "@p_request_notes",
            string.IsNullOrWhiteSpace(request.RequestNotes) ? DBNull.Value : request.RequestNotes
        );

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return Model_Dao_Result_Factory.Failure<Model_OutsideServiceRequest>(
                "Header insert did not return a request identifier."
            );
        }

        var saved = new Model_OutsideServiceRequest
        {
            OutsideServiceRequestId = Convert.ToInt32(
                reader["outside_service_request_id"],
                CultureInfo.InvariantCulture
            ),
            RequestNumber = reader["request_number"]?.ToString() ?? string.Empty,
            CreatedUtc = reader["created_utc"] is DateTime createdUtc
                ? DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc)
                : DateTime.UtcNow,
        };

        return Model_Dao_Result_Factory.Success(saved);
    }

    private static async Task<Model_Dao_Result<int>> InsertLineAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        Model_OutsideServiceRequestLine line
    )
    {
        await using var command = new MySqlCommand(
            "sp_OutsideService_RequestLine_Insert",
            connection,
            transaction
        )
        {
            CommandType = CommandType.StoredProcedure,
        };

        command.Parameters.AddWithValue(
            "@p_outside_service_request_id",
            line.OutsideServiceRequestId
        );
        command.Parameters.AddWithValue("@p_line_number", line.LineNumber);
        command.Parameters.AddWithValue("@p_part_id", line.PartId);
        command.Parameters.AddWithValue("@p_package_count", line.PackageCount);
        command.Parameters.AddWithValue("@p_package_summary", line.PackageSummary);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return Model_Dao_Result_Factory.Failure<int>(
                "Line insert did not return a line identifier."
            );
        }

        return Model_Dao_Result_Factory.Success<int>(
            Convert.ToInt32(reader["outside_service_request_line_id"], CultureInfo.InvariantCulture)
        );
    }

    private static Model_OutsideServiceRequestLine MapLineFromReader(IDataReader reader)
    {
        var line = new Model_OutsideServiceRequestLine
        {
            OutsideServiceRequestId = Convert.ToInt32(
                reader["outside_service_request_id"],
                CultureInfo.InvariantCulture
            ),
            RequestNumber = reader["request_number"]?.ToString() ?? string.Empty,
            CreatedByUser = reader["created_by_user"]?.ToString() ?? string.Empty,
            CreatedByDisplay = reader["created_by_display"]?.ToString() ?? string.Empty,
            CreatedUtc = reader["created_utc"] is DateTime createdUtc
                ? DateTime.SpecifyKind(createdUtc, DateTimeKind.Utc)
                : DateTime.UtcNow,
            RequestNotes = reader["request_notes"]?.ToString(),
            OutsideServiceRequestLineId = Convert.ToInt32(
                reader["outside_service_request_line_id"],
                CultureInfo.InvariantCulture
            ),
            LineNumber = Convert.ToInt32(reader["line_number"], CultureInfo.InvariantCulture),
            PartId = reader["part_id"]?.ToString() ?? string.Empty,
            PackageCount = Convert.ToInt32(reader["package_count"], CultureInfo.InvariantCulture),
            LinePhase = ParsePhase(reader["line_phase"]?.ToString()),
            SetupVendorId = reader["setup_vendor_id"]?.ToString(),
            SetupVendorName = reader["setup_vendor_name"]?.ToString(),
            SetupVendorSource = reader["setup_vendor_source"]?.ToString(),
            BOLNumber = reader["bol_number"]?.ToString(),
            ScheduledShipUtc = reader["scheduled_ship_utc"] is DateTime scheduled
                ? DateTime.SpecifyKind(scheduled, DateTimeKind.Utc)
                : null,
            ShippingContact = reader["shipping_contact"]?.ToString(),
            SetupNotes = reader["setup_notes"]?.ToString(),
            CompletedUtc = reader["completed_utc"] is DateTime completed
                ? DateTime.SpecifyKind(completed, DateTimeKind.Utc)
                : null,
            CompletionNotes = reader["completion_notes"]?.ToString(),
        };

        var summary = reader["package_summary"]?.ToString();
        if (!string.IsNullOrWhiteSpace(summary))
        {
            var parts = summary.Split(" / ", StringSplitOptions.RemoveEmptyEntries);
            line.Packages = parts
                .Select(
                    (value, index) =>
                        new Model_OutsideServiceRequestPackage
                        {
                            PackageSequence = index + 1,
                            PackageQuantity = decimal.TryParse(
                                value,
                                NumberStyles.Number,
                                CultureInfo.InvariantCulture,
                                out var parsedQuantity
                            )
                                ? parsedQuantity
                                : 0m,
                        }
                )
                .ToList();
        }

        return line;
    }

    private static Enum_OutsideServiceLinePhase ParsePhase(string? value)
    {
        return Enum.TryParse<Enum_OutsideServiceLinePhase>(value, true, out var phase)
            ? phase
            : Enum_OutsideServiceLinePhase.Initialize;
    }
}
