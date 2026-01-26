using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents a receiving transaction (master record)
/// Maps to: receiving_transactions table in SQL Server
/// </summary>
public class Model_Receiving_TableEntitys_ReceivingTransaction
{
    /// <summary>
    /// Unique transaction identifier (auto-generated)
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Purchase Order number (NULL for non-PO receiving)
    /// Format: PO-XXXXXX (6 digits)
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// User ID who created the transaction
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User name (for display and audit purposes)
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Workflow mode used to create this transaction
    /// Values: 'Wizard', 'Manual', 'Edit'
    /// </summary>
    public Enum_Receiving_Mode_WorkflowMode WorkflowMode { get; set; }

    /// <summary>
    /// Date/time when transaction was created
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Whether transaction has been exported to CSV
    /// </summary>
    public bool ExporteDataTransferObjectsCSV { get; set; }

    /// <summary>
    /// Local CSV export file path
    /// </summary>
    public string? CSVExportPathLocal { get; set; }

    /// <summary>
    /// Network CSV export file path
    /// </summary>
    public string? CSVExportPathNetwork { get; set; }

    /// <summary>
    /// Date/time when CSV was exported
    /// </summary>
    public DateTime? CSVExportedAt { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Record last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated the record
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Collection of loads associated with this transaction
    /// </summary>
    public List<Model_Receiving_TableEntitys_ReceivingLoad> Loads { get; set; } = new();
}
