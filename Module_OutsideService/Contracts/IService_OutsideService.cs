using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_OutsideService.Models;

namespace MTM_Receiving_Application.Module_OutsideService.Contracts;

/// <summary>
/// Business service contract for the Outside Service module.
/// </summary>
public interface IService_OutsideService
{
    /// <summary>
    /// Creates a new Outside Service request with one or more lines and package rows.
    /// </summary>
    Task<Model_Dao_Result<Model_OutsideServiceRequest>> CreateRequestAsync(
        Model_OutsideServiceRequest request
    );

    /// <summary>
    /// Returns all active waitlist lines.
    /// </summary>
    Task<Model_Dao_Result<List<Model_OutsideServiceRequestLine>>> GetOpenLinesAsync();

    /// <summary>
    /// Returns all completed waitlist lines.
    /// </summary>
    Task<Model_Dao_Result<List<Model_OutsideServiceRequestLine>>> GetCompletedLinesAsync();

    /// <summary>
    /// Validates an exact part number against Infor Visual.
    /// </summary>
    Task<Model_Dao_Result<bool>> ValidatePartAsync(string partId);

    /// <summary>
    /// Returns user-friendly part suggestions for the Part Match Helper.
    /// </summary>
    Task<Model_Dao_Result<List<Model_OutsideServicePartMatchSuggestion>>> GetPartSuggestionsAsync(
        string searchTerm
    );

    /// <summary>
    /// Returns most-recent-first vendor suggestions for a part.
    /// </summary>
    Task<Model_Dao_Result<List<Model_OutsideServiceVendorSuggestion>>> GetVendorSuggestionsAsync(
        string partId
    );

    /// <summary>
    /// Saves Setup-phase data for a line.
    /// </summary>
    Task<Model_Dao_Result> SaveSetupAsync(Model_OutsideServiceRequestLine line);

    /// <summary>
    /// Marks a line complete.
    /// </summary>
    Task<Model_Dao_Result> MarkCompleteAsync(int lineId, string? completionNotes);
}
