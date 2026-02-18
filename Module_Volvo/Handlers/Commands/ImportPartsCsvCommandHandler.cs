using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// [STUB] Handler for import operations - imports parts data.
/// TODO: Implement database import operation.
/// </summary>
public class ImportPartsCommandHandler : IRequestHandler<ImportPartsCommand, Model_Dao_Result<ImportPartsResult>>
{
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;

    public ImportPartsCommandHandler(Dao_VolvoPart partDao, Dao_VolvoPartComponent componentDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
    }

    public async Task<Model_Dao_Result<ImportPartsResult>> Handle(ImportPartsCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Model_Dao_Result_Factory.Failure<ImportPartsResult>("TODO: Implement database import operation.");

        // --- original implementation below (unreachable) ---
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return Model_Dao_Result_Factory.Failure<ImportPartsResult>("File path is required");
            }

            var fileContent = await File.ReadAllTextAsync(request.FilePath, cancellationToken);
            var lines = fileContent
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (lines.Count < 2)
            {
                return Model_Dao_Result_Factory.Failure<ImportPartsResult>(
                    "File must contain header and at least one data row");
            }

            var header = lines[0];
            if (!header.Contains("PartNumber") || !header.Contains("QuantityPerSkid"))
            {
                return Model_Dao_Result_Factory.Failure<ImportPartsResult>(
                    "File must contain columns: PartNumber, QuantityPerSkid, Components");
            }

            int successCount = 0;
            int failureCount = 0;
            var errors = new List<string>();

            for (int i = 1; i < lines.Count; i++)
            {
                try
                {
                    var fields = ParseDataLine(lines[i]);
                    if (fields.Length < 2)
                    {
                        errors.Add($"Line {i + 1}: Invalid format - expected at least 2 fields");
                        failureCount++;
                        continue;
                    }

                    var partNumber = fields[0].Trim();
                    var quantityStr = fields[1].Trim();
                    var componentsStr = fields.Length > 2 ? fields[2].Trim() : string.Empty;

                    if (string.IsNullOrWhiteSpace(partNumber))
                    {
                        errors.Add($"Line {i + 1}: Part number is required");
                        failureCount++;
                        continue;
                    }

                    if (!int.TryParse(quantityStr, out int quantity) || quantity <= 0)
                    {
                        errors.Add($"Line {i + 1}: Invalid quantity '{quantityStr}'");
                        failureCount++;
                        continue;
                    }

                    var part = new Model_VolvoPart
                    {
                        PartNumber = partNumber.Trim().ToUpperInvariant(),
                        QuantityPerSkid = quantity,
                        IsActive = true
                    };

                    var existing = await _partDao.GetByIdAsync(part.PartNumber);
                    var isNew = !existing.IsSuccess || existing.Data == null;

                    Model_Dao_Result saveResult = isNew
                        ? await _partDao.InsertAsync(part)
                        : await _partDao.UpdateAsync(part);

                    if (!saveResult.IsSuccess)
                    {
                        errors.Add($"Line {i + 1}: {saveResult.ErrorMessage}");
                        failureCount++;
                        continue;
                    }

                    var components = ParseComponents(part.PartNumber, componentsStr);

                    if (components.Count > 0)
                    {
                        var deleteResult = await _componentDao.DeleteByParentPartAsync(part.PartNumber);
                        if (!deleteResult.IsSuccess)
                        {
                            errors.Add($"Line {i + 1}: Failed to clear components - {deleteResult.ErrorMessage}");
                            failureCount++;
                            continue;
                        }

                        foreach (var component in components)
                        {
                            var insertResult = await _componentDao.InsertAsync(component);
                            if (!insertResult.IsSuccess)
                            {
                                errors.Add($"Line {i + 1}: Failed to save component {component.ComponentPartNumber}");
                                failureCount++;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        await _componentDao.DeleteByParentPartAsync(part.PartNumber);
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Line {i + 1}: {ex.Message}");
                    failureCount++;
                }
            }

            var result = new ImportPartsResult
            {
                SuccessCount = successCount,
                FailureCount = failureCount,
                Errors = errors
            };

            return Model_Dao_Result_Factory.Success(result);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<ImportPartsResult>(
                $"Unexpected error importing data: {ex.Message}", ex);
        }
    }

    private static string[] ParseDataLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());
        return fields.ToArray();
    }

    private static List<Model_VolvoPartComponent> ParseComponents(string parentPartNumber, string componentsStr)
    {
        var components = new List<Model_VolvoPartComponent>();

        if (string.IsNullOrWhiteSpace(componentsStr))
        {
            return components;
        }

        var componentPairs = componentsStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in componentPairs)
        {
            var parts = pair.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int compQty) && compQty > 0)
            {
                components.Add(new Model_VolvoPartComponent
                {
                    ParentPartNumber = parentPartNumber,
                    ComponentPartNumber = parts[0].Trim(),
                    Quantity = compQty
                });
            }
        }

        return components;
    }
}
