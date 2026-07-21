using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Placeholder scanner validation service scaffold.
/// Feature behavior will be added in a later implementation pass.
/// </summary>
public sealed class Service_ScannerValidation : IService_ScannerValidation
{
	public Task<Model_Dao_Result<Model_ScannerItemValidationResult>> ValidateNewItemAsync(
		Model_ScannerItemValidationRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (string.IsNullOrWhiteSpace(request.PartId))
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Success(
					new Model_ScannerItemValidationResult
					{
						SessionId = request.SessionId,
						ItemId = request.ItemId,
						State = Enum_ScannerValidationState.Invalid,
						Message = "Part ID is required.",
						Notes = "Add a part before saving this row.",
					}
				)
			);
		}

		if (string.IsNullOrWhiteSpace(request.FromLocation))
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Success(
					new Model_ScannerItemValidationResult
					{
						SessionId = request.SessionId,
						ItemId = request.ItemId,
						State = Enum_ScannerValidationState.Invalid,
						Message = "From location is required.",
						Notes = "Select a source location before sending.",
					}
				)
			);
		}

		if (string.IsNullOrWhiteSpace(request.ToLocation))
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Success(
					new Model_ScannerItemValidationResult
					{
						SessionId = request.SessionId,
						ItemId = request.ItemId,
						State = Enum_ScannerValidationState.Invalid,
						Message = "To location is required.",
						Notes = "Select a destination location before sending.",
					}
				)
			);
		}

		if (!decimal.TryParse(request.Quantity, out var quantity) || quantity <= 0)
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Success(
					new Model_ScannerItemValidationResult
					{
						SessionId = request.SessionId,
						ItemId = request.ItemId,
						State = Enum_ScannerValidationState.Invalid,
						Message = "Quantity must be a positive number.",
						Notes = "Enter a quantity greater than zero.",
					}
				)
			);
		}

		return Task.FromResult(
			Model_Dao_Result_Factory.Success(
				new Model_ScannerItemValidationResult
				{
					SessionId = request.SessionId,
					ItemId = request.ItemId,
					State = Enum_ScannerValidationState.Valid,
					Message = string.Empty,
					Notes = "Validation passed.",
					CanonicalPartId = request.PartId.Trim().ToUpperInvariant(),
					CanonicalFromLocation = request.FromLocation.Trim().ToUpperInvariant(),
					CanonicalToLocation = request.ToLocation.Trim().ToUpperInvariant(),
				}
			)
		);
	}

	public async Task<Model_Dao_Result<IReadOnlyList<Model_ScannerItemValidationResult>>> ValidateSessionItemsAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	)
	{
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
}