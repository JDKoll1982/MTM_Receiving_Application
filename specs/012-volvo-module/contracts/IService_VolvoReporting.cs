using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Volvo;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service interface for Volvo module reporting integration
/// Provides data for End-of-Day reports alongside Receiving, Dunnage, and Routing modules
/// </summary>
public interface IService_VolvoReporting
{
    /// <summary>
    /// Gets count of Volvo shipment records for a date range
    /// Used by shared Reporting module to enable/disable Volvo checkbox
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Record count</returns>
    Task<Model_Dao_Result<int>> GetRecordCountAsync(
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets Volvo shipment data for End-of-Day report
    /// Returns shipments grouped by status (Pending PO / Completed) with part counts
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Report data with Pending PO and Completed sections</returns>
    Task<Model_Dao_Result<Model_VolvoReportData>> GetReportDataAsync(
        DateTime startDate,
        DateTime endDate);
}

/// <summary>
/// Volvo report data structure for End-of-Day reports
/// </summary>
public class Model_VolvoReportData
{
    /// <summary>
    /// Pending PO shipments (awaiting PO from purchasing)
    /// </summary>
    public List<Model_VolvoReportShipment> PendingPO { get; set; } = new();

    /// <summary>
    /// Completed shipments (with PO and Receiver numbers)
    /// </summary>
    public List<Model_VolvoReportShipment> Completed { get; set; } = new();
}

/// <summary>
/// Shipment summary for reporting
/// </summary>
public class Model_VolvoReportShipment
{
    public int Id { get; set; }
    public DateTime ShipmentDate { get; set; }
    public int ShipmentNumber { get; set; }
    public string? PONumber { get; set; }
    public string? ReceiverNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PartCount { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
}

