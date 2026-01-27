using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

/// <summary>
/// Query to retrieve reference data (PartTypes, PackageTypes, Locations, etc.)
/// Used for populating dropdowns and comboboxes in UI
/// </summary>
public class QueryRequest_Receiving_Shared_Get_ReferenceData : IRequest<Model_Dao_Result<Model_Receiving_DataTransferObjects_ReferenceData>>
{
    /// <summary>
    /// Which reference datasets to retrieve (empty = all)
    /// </summary>
    public List<string> DatasetNames { get; set; } = new();
}
