using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Keeps the two Label Overlay outputs' printer setups and list filters per user, so the overlay and the
/// cut-out cards each reopen the way they were left and neither overwrites the other.
/// </summary>
/// <remarks>
/// These live in the user settings table rather than a file on the PC, because they describe how a person
/// works rather than what hardware is plugged in — unlike, say, the list of installed printers.
/// </remarks>
public interface IService_ReceivingLabelOverlaySettings
{
    /// <summary>The printer setup for one output, or that output's defaults when nothing is saved.</summary>
    Task<Model_Dao_Result<Model_LabelOverlayPrintSettings>> GetPrintProfileAsync(
        Enum_LabelOverlayPrintFormat format,
        int? userId = null
    );

    /// <summary>Keeps the printer setup for one output.</summary>
    Task<Model_Dao_Result> SavePrintProfileAsync(
        Enum_LabelOverlayPrintFormat format,
        Model_LabelOverlayPrintSettings settings,
        int? userId = null
    );

    /// <summary>The list filter for one output, or that output's defaults when nothing is saved.</summary>
    Task<Model_Dao_Result<Model_LabelOverlayListFilter>> GetListFilterAsync(
        Enum_LabelOverlayPrintFormat format,
        int? userId = null
    );

    /// <summary>Keeps the list filter for one output.</summary>
    Task<Model_Dao_Result> SaveListFilterAsync(
        Enum_LabelOverlayPrintFormat format,
        Model_LabelOverlayListFilter filter,
        int? userId = null
    );

    /// <summary>The part the cut-out tab was last working on, or empty.</summary>
    Task<string> GetCardSheetPartIdAsync(int? userId = null);

    /// <summary>Keeps the part the cut-out tab is working on.</summary>
    Task<Model_Dao_Result> SaveCardSheetPartIdAsync(string partId, int? userId = null);
}
