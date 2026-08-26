using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Shared.Services.Lookup;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Scanner item and location validation service with Infor Visual-backed checks.
/// </summary>
public sealed class Service_ScannerValidation : IService_ScannerValidation
{
	private readonly IService_InforVisual _inforVisualService;
	private readonly Strategy_SharedLocationLookup _locationStrategy;

	public Service_ScannerValidation(IService_InforVisual inforVisualService)
	{
		_inforVisualService =
			inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
		_locationStrategy = new Strategy_SharedLocationLookup(_inforVisualService);
	}

	public string FormatLocation(string location)
	{
		if (string.IsNullOrWhiteSpace(location))
		{
			return location ?? string.Empty;
		}

		var result = _locationStrategy.ApplyFormatting(
			new Model_SharedLookupRequest
			{
				LookupType = Enum_SharedLookupType.Location,
				RawInput = location.Trim(),
			}
		);

		return result.FormattedValue;
	}

	public Task<Model_Dao_Result<bool>> TransferSavedSinceAsync(
		string partId,
		string fromWarehouse,
		string fromLocation,
		string toWarehouse,
		string toLocation,
		decimal quantity,
		DateTime afterUtc,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return _inforVisualService.ScannerTransferExistsAsync(
			partId,
			fromWarehouse,
			fromLocation,
			toWarehouse,
			toLocation,
			quantity,
			afterUtc
		);
	}

	public Task<Model_Dao_Result<bool>> PartExistsAsync(
		string partId,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var canonicalPartId = partId?.Trim().ToUpperInvariant() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(canonicalPartId))
		{
			return Task.FromResult(Model_Dao_Result_Factory.Success(false));
		}

		return _inforVisualService.PartExistsAsync(canonicalPartId);
	}

	public Task<Model_Dao_Result<Model_ScannerItemValidationResult>> ValidateNewItemAsync(
		Model_ScannerItemValidationRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (request is null)
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Failure<Model_ScannerItemValidationResult>(
					"Validation request is required."
				)
			);
		}

		var canonicalPartId = request.PartId?.Trim().ToUpperInvariant() ?? string.Empty;
		var canonicalFromWarehouse = string.IsNullOrWhiteSpace(request.FromWarehouse)
			? "002"
			: request.FromWarehouse.Trim().ToUpperInvariant();
		var canonicalFromLocation = request.FromLocation?.Trim().ToUpperInvariant() ?? string.Empty;
		var canonicalToWarehouse = string.IsNullOrWhiteSpace(request.ToWarehouse)
			? "002"
			: request.ToWarehouse.Trim().ToUpperInvariant();
		var canonicalToLocation = request.ToLocation?.Trim().ToUpperInvariant() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(request.PartId))
		{
			return Task.FromResult(Model_Dao_Result_Factory.Success(InvalidResult(
				request,
				"Part ID is required.",
				"Add a part before saving this row.",
				canonicalPartId,
				canonicalFromLocation,
				canonicalToLocation
			)));
		}

		if (string.IsNullOrWhiteSpace(request.FromLocation))
		{
			return Task.FromResult(Model_Dao_Result_Factory.Success(InvalidResult(
				request,
				"From location is required.",
				"Select a source location before sending.",
				canonicalPartId,
				canonicalFromLocation,
				canonicalToLocation
			)));
		}

		if (string.IsNullOrWhiteSpace(request.ToLocation))
		{
			return Task.FromResult(Model_Dao_Result_Factory.Success(InvalidResult(
				request,
				"To location is required.",
				"Select a destination location before sending.",
				canonicalPartId,
				canonicalFromLocation,
				canonicalToLocation
			)));
		}

		if (!decimal.TryParse(request.Quantity, out var quantity) || quantity <= 0)
		{
			return Task.FromResult(Model_Dao_Result_Factory.Success(InvalidResult(
				request,
				"Quantity must be a positive number.",
				"Enter a quantity greater than zero.",
				canonicalPartId,
				canonicalFromLocation,
				canonicalToLocation
			)));
		}

		return ValidateAgainstInforVisualAsync(
			request,
			quantity,
			canonicalPartId,
			canonicalFromWarehouse,
			canonicalFromLocation,
			canonicalToWarehouse,
			canonicalToLocation,
			cancellationToken
		);
	}

	public async Task<Model_Dao_Result<IReadOnlyList<Model_ScannerItemValidationResult>>> ValidateSessionItemsAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_ScannerItemValidationResult>>(
				"Session is required."
			);
		}

		var results = new List<Model_ScannerItemValidationResult>(session.Items.Count);

		foreach (var item in session.Items)
		{
			var request = new Model_ScannerItemValidationRequest
			{
				SessionId = session.SessionId,
				ItemId = item.ItemId,
				PartId = item.PayloadPartId,
				FromWarehouse = item.PayloadFromWarehouse,
				FromLocation = item.PayloadFromLocation,
				ToWarehouse = item.PayloadToWarehouse,
				ToLocation = item.PayloadToLocation,
				Quantity = item.PayloadQuantity,
			};

			var itemResult = await ValidateNewItemAsync(request, cancellationToken);
			if (!itemResult.Success || itemResult.Data is null)
			{
				return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_ScannerItemValidationResult>>(
					itemResult.ErrorMessage
				);
			}

			item.ApplyValidationResult(itemResult.Data);
			results.Add(itemResult.Data);
		}

		return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_ScannerItemValidationResult>>(results);
	}

	public async Task<Model_Dao_Result<Model_ScannerLocationValidationResult>> ValidateLocationAsync(
		string location,
		string warehouseCode,
		CancellationToken cancellationToken = default
	)
	{
		var canonicalLocation = location?.Trim().ToUpperInvariant() ?? string.Empty;
		var canonicalWarehouse = string.IsNullOrWhiteSpace(warehouseCode)
			? "002"
			: warehouseCode.Trim().ToUpperInvariant();

		if (string.IsNullOrWhiteSpace(canonicalLocation))
		{
			return Model_Dao_Result_Factory.Success(
				new Model_ScannerLocationValidationResult
				{
					IsValid = false,
					Message = "Location is required.",
				}
			);
		}

		var resolution = await ResolveLocationAsync(
			canonicalLocation,
			canonicalWarehouse,
			cancellationToken
		);
		if (!resolution.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerLocationValidationResult>(
				resolution.ErrorMessage,
				resolution.Exception
			);
		}

		if (string.IsNullOrWhiteSpace(resolution.Data))
		{
			return Model_Dao_Result_Factory.Success(
				new Model_ScannerLocationValidationResult
				{
					IsValid = false,
					Message = $"Location {canonicalLocation} does not exist in warehouse {canonicalWarehouse}.",
				}
			);
		}

		return Model_Dao_Result_Factory.Success(
			new Model_ScannerLocationValidationResult
			{
				IsValid = true,
				CanonicalLocation = resolution.Data,
			}
		);
	}

	public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetLocationSuggestionsAsync(
		string location,
		string warehouseCode,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (string.IsNullOrWhiteSpace(location))
		{
			return Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>());
		}

		var canonicalWarehouse = string.IsNullOrWhiteSpace(warehouseCode)
			? "002"
			: warehouseCode.Trim().ToUpperInvariant();

		var fuzzyResult = await _inforVisualService.FuzzySearchLocationsAsync(
			location.Trim(),
			canonicalWarehouse
		);
		if (!fuzzyResult.Success || fuzzyResult.Data is null)
		{
			return fuzzyResult;
		}

		var suggestions = fuzzyResult
			.Data.Where(static result => string.IsNullOrWhiteSpace(result.Label) is false)
			.GroupBy(static result => result.Label.Trim(), StringComparer.OrdinalIgnoreCase)
			.Select(static group => group.First())
			.ToList();

		return Model_Dao_Result_Factory.Success(suggestions);
	}

	public async Task<Model_Dao_Result<IReadOnlyList<Model_InforVisualMaterialLocationRow>>> GetLocationsWithStockAsync(
		string partId,
		string warehouseCode,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var canonicalPartId = partId?.Trim().ToUpperInvariant() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(canonicalPartId))
		{
			return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
				"Part ID is required."
			);
		}

		var canonicalWarehouse = string.IsNullOrWhiteSpace(warehouseCode)
			? "002"
			: warehouseCode.Trim().ToUpperInvariant();

		var stockResult = await _inforVisualService.GetMaterialAvailabilityCurrentStockAsync(
			null,
			canonicalPartId,
			canonicalWarehouse
		);
		if (!stockResult.Success || stockResult.Data is null)
		{
			return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
				stockResult.ErrorMessage,
				stockResult.Exception
			);
		}

		// Aggregate per-location stock and keep only locations with positive on-hand quantity.
		var inStockLocations = stockResult.Data
			.Where(row =>
				row.Quantity > 0
				&& string.IsNullOrWhiteSpace(row.LocationId) is false
			)
			.GroupBy(row => row.LocationId.Trim(), StringComparer.OrdinalIgnoreCase)
			.Select(group => new Model_InforVisualMaterialLocationRow
			{
				PartId = canonicalPartId,
				PartDescription = group.First().PartDescription,
				WarehouseCode = canonicalWarehouse,
				LocationId = group.First().LocationId.Trim(),
				Quantity = group.Sum(row => row.Quantity),
				CommittedQuantity = group.Sum(row => row.CommittedQuantity),
			})
			.OrderBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
			.ToList();

		return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
			inStockLocations
		);
	}

	public async Task<Model_Dao_Result<IReadOnlyList<Model_InforVisualMaterialLocationRow>>> GetPartsAtLocationAsync(
		string locationId,
		string warehouseCode,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var canonicalLocation = locationId?.Trim().ToUpperInvariant() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(canonicalLocation))
		{
			return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
				"Location is required."
			);
		}

		var canonicalWarehouse = string.IsNullOrWhiteSpace(warehouseCode)
			? "002"
			: warehouseCode.Trim().ToUpperInvariant();

		var stockResult = await _inforVisualService.GetMaterialAvailabilityCurrentStockAsync(
			canonicalLocation,
			null,
			canonicalWarehouse
		);
		if (!stockResult.Success || stockResult.Data is null)
		{
			return Model_Dao_Result_Factory.Failure<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
				stockResult.ErrorMessage,
				stockResult.Exception
			);
		}

		// Aggregate per-part stock at this location and keep only parts with positive on-hand.
		var parts = stockResult.Data
			.Where(row =>
				row.Quantity > 0
				&& string.IsNullOrWhiteSpace(row.PartId) is false
			)
			.GroupBy(row => row.PartId.Trim(), StringComparer.OrdinalIgnoreCase)
			.Select(group => new Model_InforVisualMaterialLocationRow
			{
				PartId = group.First().PartId.Trim(),
				PartDescription = group.First().PartDescription,
				WarehouseCode = canonicalWarehouse,
				LocationId = canonicalLocation,
				Quantity = group.Sum(row => row.Quantity),
				CommittedQuantity = group.Sum(row => row.CommittedQuantity),
			})
			.OrderBy(row => row.PartId, StringComparer.OrdinalIgnoreCase)
			.ToList();

		return Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
			parts
		);
	}

	private async Task<Model_Dao_Result<Model_ScannerItemValidationResult>> ValidateAgainstInforVisualAsync(
		Model_ScannerItemValidationRequest request,
		decimal quantity,
		string canonicalPartId,
		string canonicalFromWarehouse,
		string canonicalFromLocation,
		string canonicalToWarehouse,
		string canonicalToLocation,
		CancellationToken cancellationToken
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var partExists = await _inforVisualService.PartExistsAsync(canonicalPartId);
		if (!partExists.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerItemValidationResult>(
				partExists.ErrorMessage,
				partExists.Exception
			);
		}

		if (!partExists.Data)
		{
			return Model_Dao_Result_Factory.Success(
				InvalidResult(
					request,
					"Part ID was not found.",
					"Part must exist in Infor Visual before sending.",
					canonicalPartId,
					canonicalFromLocation,
					canonicalToLocation
				)
			);
		}

		var fromLocationResolution = await ResolveLocationAsync(
			canonicalFromLocation,
			canonicalFromWarehouse,
			cancellationToken
		);
		if (!fromLocationResolution.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerItemValidationResult>(
				fromLocationResolution.ErrorMessage,
				fromLocationResolution.Exception
			);
		}

		if (string.IsNullOrWhiteSpace(fromLocationResolution.Data))
		{
			return Model_Dao_Result_Factory.Success(
				InvalidResult(
					request,
					"From location was not found.",
					$"Location {canonicalFromLocation} does not exist in warehouse {canonicalFromWarehouse}.",
					canonicalPartId,
					canonicalFromLocation,
					canonicalToLocation
				)
			);
		}

		canonicalFromLocation = fromLocationResolution.Data;

		var toLocationResolution = await ResolveLocationAsync(
			canonicalToLocation,
			canonicalToWarehouse,
			cancellationToken
		);
		if (!toLocationResolution.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerItemValidationResult>(
				toLocationResolution.ErrorMessage,
				toLocationResolution.Exception
			);
		}

		if (string.IsNullOrWhiteSpace(toLocationResolution.Data))
		{
			return Model_Dao_Result_Factory.Success(
				InvalidResult(
					request,
					"To location was not found.",
					$"Location {canonicalToLocation} does not exist in warehouse {canonicalToWarehouse}.",
					canonicalPartId,
					canonicalFromLocation,
					canonicalToLocation
				)
			);
		}

		canonicalToLocation = toLocationResolution.Data;

		var stockResult = await _inforVisualService.GetMaterialAvailabilityCurrentStockAsync(
			canonicalFromLocation,
			canonicalPartId,
			canonicalFromWarehouse
		);
		if (!stockResult.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerItemValidationResult>(
				stockResult.ErrorMessage,
				stockResult.Exception
			);
		}

		var availableQuantity = stockResult.Data?
			.Where(row =>
				string.Equals(row.PartId, canonicalPartId, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(row.LocationId, canonicalFromLocation, StringComparison.OrdinalIgnoreCase)
			)
			.Sum(row => row.Quantity) ?? 0m;

		if (availableQuantity < quantity)
		{
			return Model_Dao_Result_Factory.Success(
				InvalidResult(
					request,
					"Source quantity is insufficient.",
					$"Requested {quantity.ToString("0.####", CultureInfo.InvariantCulture)} exceeds available {availableQuantity.ToString("0.####", CultureInfo.InvariantCulture)} at {canonicalFromLocation}.",
					canonicalPartId,
					canonicalFromLocation,
					canonicalToLocation
				)
			);
		}

		return Model_Dao_Result_Factory.Success(
			new Model_ScannerItemValidationResult
			{
				SessionId = request.SessionId,
				ItemId = request.ItemId,
				State = Enum_ScannerValidationState.Valid,
				Message = string.Empty,
				Notes = "Validation passed.",
				CanonicalPartId = canonicalPartId,
				CanonicalFromLocation = canonicalFromLocation,
				CanonicalToLocation = canonicalToLocation,
			}
		);
	}

	private async Task<Model_Dao_Result<string?>> ResolveLocationAsync(
		string location,
		string warehouseCode,
		CancellationToken cancellationToken
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existsResult = await _inforVisualService.LocationExistsAsync(location, warehouseCode);
		if (!existsResult.Success)
		{
			return Model_Dao_Result_Factory.Failure<string?>(
				existsResult.ErrorMessage,
				existsResult.Exception
			);
		}

		if (existsResult.Data)
		{
			return Model_Dao_Result_Factory.Success<string?>(location);
		}

		var normalizedLocation = NormalizeLocationForMatch(location);
		if (string.IsNullOrWhiteSpace(normalizedLocation) || string.Equals(normalizedLocation, location, StringComparison.OrdinalIgnoreCase))
		{
			return Model_Dao_Result_Factory.Success<string?>(null);
		}

		existsResult = await _inforVisualService.LocationExistsAsync(normalizedLocation, warehouseCode);
		if (!existsResult.Success)
		{
			return Model_Dao_Result_Factory.Failure<string?>(
				existsResult.ErrorMessage,
				existsResult.Exception
			);
		}

		return Model_Dao_Result_Factory.Success<string?>(existsResult.Data ? normalizedLocation : null);
	}

	private static string NormalizeLocationForMatch(string location)
	{
		return new string(location.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
	}

	private static Model_ScannerItemValidationResult InvalidResult(
		Model_ScannerItemValidationRequest request,
		string message,
		string notes,
		string canonicalPartId,
		string canonicalFromLocation,
		string canonicalToLocation
	)
	{
		return new Model_ScannerItemValidationResult
		{
			SessionId = request.SessionId,
			ItemId = request.ItemId,
			State = Enum_ScannerValidationState.Invalid,
			Message = message,
			Notes = notes,
			CanonicalPartId = canonicalPartId,
			CanonicalFromLocation = canonicalFromLocation,
			CanonicalToLocation = canonicalToLocation,
		};
	}
}
