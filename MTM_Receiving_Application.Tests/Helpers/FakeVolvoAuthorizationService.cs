using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Tests.Helpers;

/// <summary>
/// Fake authorization service for Volvo handler tests.
/// </summary>
public class FakeVolvoAuthorizationService : IService_VolvoAuthorization
{
    public Task<Model_Dao_Result> CanManageShipmentsAsync() => Task.FromResult(new Model_Dao_Result { Success = true });

    public Task<Model_Dao_Result> CanManageMasterDataAsync() => Task.FromResult(new Model_Dao_Result { Success = true });

    public Task<Model_Dao_Result> CanCompleteShipmentsAsync() => Task.FromResult(new Model_Dao_Result { Success = true });

    public Task<Model_Dao_Result> CanGenerateLabelsAsync() => Task.FromResult(new Model_Dao_Result { Success = true });
}
