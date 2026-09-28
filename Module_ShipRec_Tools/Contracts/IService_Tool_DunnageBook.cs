using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Contract for the dunnage book generator tool.
/// </summary>
public interface IService_Tool_DunnageBook
{
    /// <summary>
    /// Loads all dunnage parts grouped by type, resolving each type's custom UDC field
    /// definitions so cards can render labeled values.
    /// </summary>
    Task<Model_Dao_Result<List<Model_Tool_DunnageBook_TypeGroup>>> LoadDunnageGroupsAsync();

    /// <summary>
    /// Builds an 8.5x11 portrait paged book document from the selected entries in
    /// <paramref name="typeGroups"/>, honoring the cover page and table of contents options.
    /// </summary>
    Task<Model_Dao_Result<Model_FormattedReportDocument>> BuildBookAsync(
        IReadOnlyList<Model_Tool_DunnageBook_TypeGroup> typeGroups,
        Model_Tool_DunnageBook_Config config
    );
}
