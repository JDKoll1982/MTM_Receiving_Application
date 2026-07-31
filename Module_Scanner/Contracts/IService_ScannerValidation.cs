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
}