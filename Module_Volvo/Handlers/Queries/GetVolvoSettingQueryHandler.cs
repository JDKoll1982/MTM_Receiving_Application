using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for GetVolvoSettingQuery - retrieves application settings by key.
/// </summary>
public class GetVolvoSettingQueryHandler : IRequestHandler<GetVolvoSettingQuery, Model_Dao_Result<string>>
{
    private readonly Dao_VolvoSettings _settingsDao;

    public GetVolvoSettingQueryHandler(Dao_VolvoSettings settingsDao)
    {
        _settingsDao = settingsDao ?? throw new ArgumentNullException(nameof(settingsDao));
    }

    public async Task<Model_Dao_Result<string>> Handle(GetVolvoSettingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _settingsDao.GetSettingAsync(request.SettingKey);

            if (result.IsSuccess && result.Data != null)
            {
                return Model_Dao_Result_Factory.Success(result.Data.SettingValue);
            }

            // Return empty result if setting not found (not an error)
            return Model_Dao_Result_Factory.Success(string.Empty);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                $"Unexpected error retrieving setting '{request.SettingKey}': {ex.Message}", ex);
        }
    }
}
