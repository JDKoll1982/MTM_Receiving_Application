using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.Contracts;

/// <summary>
/// Writes one accepted line. There is no bulk path, no end-of-run apply and no
/// second procedure (FR-041).
/// </summary>
public interface IService_ReconciliationWriteService
{
    /// <summary>
    /// Re-runs the quantity proof for the finding's group, then performs exactly
    /// one write and returns the finding in state Written.
    /// </summary>
    /// <param name="finding">The finding being accepted.</param>
    /// <param name="destinationLocation">The destination to write.</param>
    /// <param name="operatorUserId">The authenticated operator recorded on the write.</param>
    Task<Model_Dao_Result<Model_ReconciliationFinding>> AcceptAsync(
        Model_ReconciliationFinding finding,
        string destinationLocation,
        string operatorUserId
    );
}
