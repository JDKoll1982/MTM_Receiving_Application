using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Returns all active generated Volvo label rows currently queued for review and printing.
/// </summary>
public record GetVolvoGeneratedLabelDataQuery
    : IRequest<Model_Dao_Result<List<Model_VolvoGeneratedLabelData>>>;
