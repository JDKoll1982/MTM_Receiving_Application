using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Contracts.Lookup;

/// <summary>
/// Shared typed lookup workflow service for Infor Visual validation pipelines.
/// </summary>
public interface IService_SharedLookupWorkflow
{
    Task<Model_SharedLookupValidationResult> ValidateAsync(
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    );
}
