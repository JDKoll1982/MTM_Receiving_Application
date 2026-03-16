using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for ClearLabelDataCommand — moves all completed Volvo shipments from the
/// active queue tables (volvo_label_data, volvo_line_data) into the history archive
/// tables (volvo_label_history, volvo_line_history) via Dao_VolvoLabelHistory.
/// Returns the total number of records archived (headers + lines).
/// </summary>
public class ClearLabelDataCommandHandler : IRequestHandler<ClearLabelDataCommand, Model_Dao_Result<int>>
{
    private readonly IDao_VolvoLabelHistory _historyDao;
    private readonly IService_VolvoAuthorization _authService;

    public ClearLabelDataCommandHandler(
        IDao_VolvoLabelHistory historyDao,
        IService_VolvoAuthorization authService)
    {
        _historyDao = historyDao ?? throw new ArgumentNullException(nameof(historyDao));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task<Model_Dao_Result<int>> Handle(ClearLabelDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var authResult = await _authService.CanCompleteShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>("You are not authorized to clear label data");
            }

            var archivedBy = string.IsNullOrWhiteSpace(request.ArchivedBy) ? "SYSTEM" : request.ArchivedBy;
            var result = await _historyDao.ClearToHistoryAsync(archivedBy);

            if (!result.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage ?? "Clear label data failed");
            }

            var (headersMoved, linesMoved) = result.Data;
            return Model_Dao_Result_Factory.Success<int>(headersMoved + linesMoved);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Unexpected error during clear label data: {ex.Message}", ex);
        }
    }
}
