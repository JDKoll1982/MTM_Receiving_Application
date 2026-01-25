using System;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Result of a workflow save operation including CSV export and database persistence.
/// </summary>
public record Model_Receiving_Result_Save(
    /// <summary>
    /// File path to the exported CSV file.
    /// </summary>
    string CsvPath,
    
    /// <summary>
    /// Database record ID from the completed_transactions table (if saved to database).
    /// </summary>
    int DatabaseRecordId,
    
    /// <summary>
    /// Timestamp when the workflow was saved.
    /// </summary>
    DateTime SavedAt
);
