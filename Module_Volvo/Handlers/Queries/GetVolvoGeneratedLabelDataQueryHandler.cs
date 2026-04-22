using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Loads all active Volvo generated label rows for the review dialog.
/// </summary>
public class GetVolvoGeneratedLabelDataQueryHandler
    : IRequestHandler<
        GetVolvoGeneratedLabelDataQuery,
        Model_Dao_Result<List<Model_VolvoGeneratedLabelData>>
    >
{
    private readonly IDao_VolvoGeneratedLabelData _generatedLabelDataDao;

    public GetVolvoGeneratedLabelDataQueryHandler(
        IDao_VolvoGeneratedLabelData generatedLabelDataDao
    )
    {
        _generatedLabelDataDao =
            generatedLabelDataDao ?? throw new ArgumentNullException(nameof(generatedLabelDataDao));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoGeneratedLabelData>>> Handle(
        GetVolvoGeneratedLabelDataQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _generatedLabelDataDao.GetActiveLabelDataAsync();
    }
}
