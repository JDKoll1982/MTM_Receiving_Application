using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Abstraction over the Volvo label-history archive DAO.
/// Enables unit testing of <c>ClearLabelDataCommandHandler</c> without a database connection.
/// </summary>
public interface IDao_VolvoLabelHistory
{
    /// <summary>
    /// Atomically moves all active Volvo shipments and their lines from the active tables
    /// to the history archive tables.
    /// Returns a tuple of <c>(HeadersMoved, LinesMoved)</c> on success.
    /// </summary>
    /// <param name="archivedBy">User or system identifier recorded in the archive record.</param>
    Task<Model_Dao_Result<(int HeadersMoved, int LinesMoved)>> ClearToHistoryAsync(
        string archivedBy
    );

    /// <summary>
    /// Gets an archived shipment header by history-table ID.
    /// </summary>
    /// <param name="shipmentHistoryId"></param>
    Task<Model_Dao_Result<Model_VolvoShipment?>> GetArchivedShipmentByIdAsync(
        int shipmentHistoryId
    );

    /// <summary>
    /// Gets archived shipment lines by archived shipment header ID.
    /// </summary>
    /// <param name="shipmentHistoryId"></param>
    Task<
        Model_Dao_Result<System.Collections.Generic.List<Model_VolvoShipmentLine>>
    > GetArchivedLinesByShipmentHistoryIdAsync(int shipmentHistoryId);

    /// <summary>
    /// Deletes an archived shipment header and its archived lines.
    /// </summary>
    /// <param name="shipmentHistoryId"></param>
    Task<Model_Dao_Result> DeleteArchivedShipmentAsync(int shipmentHistoryId);
}
