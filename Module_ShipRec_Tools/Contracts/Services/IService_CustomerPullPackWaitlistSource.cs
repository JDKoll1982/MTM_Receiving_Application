using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;

public interface IService_CustomerPullPackWaitlistSource
{
    Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> UpsertAsync(
        Model_CustomerPullPack_WaitlistEntry entry
    );

    Task<Model_Dao_Result<List<Model_CustomerPullPack_WaitlistEntry>>> GetQueueAsync(
        string? waitlistId = null,
        string? customerId = null,
        string? requesterUserId = null,
        string? currentOwnerUserId = null,
        string? locationId = null,
        IReadOnlyCollection<Enum_CustomerPullPackWaitlistStatus>? statusSet = null,
        bool useDefaultOpenWork = true,
        int maxResults = 250
    );

    Task<Model_Dao_Result<Model_CustomerPullPack_WaitlistEntry>> GetByIdAsync(string waitlistId);
}
