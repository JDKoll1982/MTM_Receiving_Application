using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Placeholder validation contract for the scanner module scaffold.
/// Feature behavior will be added in a later implementation pass.
/// </summary>
public interface IService_ScannerValidation
{
	Task<Model_Dao_Result<Model_ScannerItemValidationResult>> ValidateNewItemAsync(
		Model_ScannerItemValidationRequest request,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<IReadOnlyList<Model_ScannerItemValidationResult>>> ValidateSessionItemsAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<Model_ScannerLocationValidationResult>> ValidateLocationAsync(
		string location,
		string warehouseCode,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetLocationSuggestionsAsync(
		string location,
		string warehouseCode,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Returns every warehouse location that currently holds stock for
	/// <paramref name="partId"/> (quantity &gt; 0), scoped to the given warehouse.
	/// Used by the workbench From-location picker when the entered source location
	/// does not resolve, so the operator can choose the real source location.
	/// </summary>
	Task<Model_Dao_Result<IReadOnlyList<Model_InforVisualMaterialLocationRow>>> GetLocationsWithStockAsync(
		string partId,
		string warehouseCode,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Applies the shared warehouse-location autocomplete formatting (dash rule) to
	/// <paramref name="location"/> — for example "VA101" becomes "V-A1-01" and "R5"
	/// becomes "R-05". Returns the uppercased input when no rule applies.
	/// </summary>
	string FormatLocation(string location);
}