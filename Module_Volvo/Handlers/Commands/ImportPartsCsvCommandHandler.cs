using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Bulk-upserts Volvo parts into the master table.
/// Each <see cref="PartImportItem"/> in the command is inserted when new,
/// or updated (QuantityPerSkid) when the part already exists.
/// </summary>
public class ImportPartsCommandHandler
    : IRequestHandler<ImportPartsCommand, Model_Dao_Result<ImportPartsResult>>
{
    private readonly Dao_VolvoPart _partDao;

    public ImportPartsCommandHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result<ImportPartsResult>> Handle(
        ImportPartsCommand request,
        CancellationToken cancellationToken
    )
    {
        int successCount = 0;
        int failureCount = 0;
        var errors = new List<string>();

        foreach (var item in request.Parts)
        {
            try
            {
                var part = new Model_VolvoPart
                {
                    PartNumber = item.PartNumber.Trim().ToUpperInvariant(),
                    QuantityPerSkid = item.QuantityPerSkid,
                    IsActive = true,
                };

                var existing = await _partDao.GetByIdAsync(part.PartNumber);
                var isNew = !existing.IsSuccess || existing.Data == null;

                var saveResult = isNew
                    ? await _partDao.InsertAsync(part)
                    : await _partDao.UpdateAsync(part);

                if (!saveResult.IsSuccess)
                {
                    errors.Add($"{item.PartNumber}: {saveResult.ErrorMessage}");
                    failureCount++;
                }
                else
                {
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{item.PartNumber}: {ex.Message}");
                failureCount++;
            }
        }

        var result = new ImportPartsResult
        {
            SuccessCount = successCount,
            FailureCount = failureCount,
            Errors = errors,
        };

        return Model_Dao_Result_Factory.Success(result);
    }
}
