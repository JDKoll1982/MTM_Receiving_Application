using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Queries
{
    /// <summary>
    /// Search for parts by PO number or part number.
    /// Data source: Infor Visual SQL Server (READ ONLY).
    /// </summary>
    /// <param name="SearchTerm">The search term</param>
    /// <param name="SearchType">The search type: "PO" or "PartNumber"</param>
    public record GetPartLookupQuery(
        string SearchTerm,
        string SearchType
    ) : IRequest<Result<List<PartInfo>>>;

    /// <summary>
    /// Part information from Infor Visual.
    /// </summary>
    /// <param name="PartId">The part identifier</param>
    /// <param name="PartNumber">The part number</param>
    /// <param name="Description">Part description</param>
    /// <param name="PONumber">Purchase order number</param>
    /// <param name="VendorName">Vendor name</param>
    public record PartInfo(
        int PartId,
        string PartNumber,
        string Description,
        string PONumber,
        string VendorName
    );
}
