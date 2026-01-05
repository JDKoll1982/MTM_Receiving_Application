using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Service for managing Volvo parts master data including CRUD operations and CSV import/export
/// </summary>
public class Service_VolvoMasterData : IService_VolvoMasterData
{
    private readonly Dao_VolvoPart _daoPart;
    private readonly Dao_VolvoPartComponent _daoComponent;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_VolvoMasterData(
        Dao_VolvoPart daoPart,
        Dao_VolvoPartComponent daoComponent,
        IService_LoggingUtility logger,
        IService_ErrorHandler errorHandler)
    {
        _daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
        _daoComponent = daoComponent ?? throw new ArgumentNullException(nameof(daoComponent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting all Volvo parts (includeInactive={includeInactive})");
            var result = await _daoPart.GetAllAsync(includeInactive);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to get parts: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting all parts: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to retrieve parts catalog",
                Enum_ErrorSeverity.Medium,
                ex,
                showDialog: false);
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPart>>($"Error retrieving parts: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting part by number: {partNumber}");
            var result = await _daoPart.GetByIdAsync(partNumber);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to get part {partNumber}: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure<Model_VolvoPart?>(result.ErrorMessage);
            }

            return Model_Dao_Result_Factory.Success<Model_VolvoPart?>(result.Data);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_VolvoPart?>($"Error retrieving part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result> AddPartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null)
    {
        try
        {
            await _logger.LogInfoAsync($"Adding new part: {part.PartNumber}");

            // Validate input
            if (string.IsNullOrWhiteSpace(part.PartNumber))
            {
                return Model_Dao_Result_Factory.Failure("Part number is required");
            }

            if (part.QuantityPerSkid < 0)
            {
                return Model_Dao_Result_Factory.Failure("Quantity per skid must be non-negative");
            }

            // Insert new part
            var insertResult = await _daoPart.InsertAsync(part);
            if (!insertResult.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to insert part {part.PartNumber}: {insertResult.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure(insertResult.ErrorMessage);
            }

            // Insert components if provided
            if (components?.Any() == true)
            {
                foreach (var component in components)
                {
                    component.ParentPartNumber = part.PartNumber;
                    var componentResult = await _daoComponent.InsertAsync(component);
                    if (!componentResult.IsSuccess)
                    {
                        await _logger.LogErrorAsync($"Failed to insert component {component.ComponentPartNumber}: {componentResult.ErrorMessage}");
                        return Model_Dao_Result_Factory.Failure($"Failed to save component {component.ComponentPartNumber}");
                    }
                }
            }

            await _logger.LogInfoAsync($"Successfully added part {part.PartNumber} with {components?.Count ?? 0} components");
            return Model_Dao_Result_Factory.Success("Part added successfully");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error adding part {part.PartNumber}: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                $"Failed to add part {part.PartNumber}",
                Enum_ErrorSeverity.Error,
                ex,
                showDialog: false);
            return Model_Dao_Result_Factory.Failure($"Error adding part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result> UpdatePartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null)
    {
        try
        {
            await _logger.LogInfoAsync($"Updating part: {part.PartNumber}");

            // Validate input
            if (string.IsNullOrWhiteSpace(part.PartNumber))
            {
                return Model_Dao_Result_Factory.Failure("Part number is required");
            }

            if (part.QuantityPerSkid < 0)
            {
                return Model_Dao_Result_Factory.Failure("Quantity per skid must be non-negative");
            }

            // Update existing part
            var updateResult = await _daoPart.UpdateAsync(part);
            if (!updateResult.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to update part {part.PartNumber}: {updateResult.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure(updateResult.ErrorMessage);
            }

            // Update components (delete existing and re-insert)
            if (components?.Any() == true)
            {
                // Delete existing components
                var deleteResult = await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
                if (!deleteResult.IsSuccess)
                {
                    await _logger.LogErrorAsync($"Failed to delete components for {part.PartNumber}: {deleteResult.ErrorMessage}");
                    return Model_Dao_Result_Factory.Failure($"Failed to update components: {deleteResult.ErrorMessage}");
                }

                // Insert new components
                foreach (var component in components)
                {
                    component.ParentPartNumber = part.PartNumber;
                    var insertResult = await _daoComponent.InsertAsync(component);
                    if (!insertResult.IsSuccess)
                    {
                        await _logger.LogErrorAsync($"Failed to insert component {component.ComponentPartNumber}: {insertResult.ErrorMessage}");
                        return Model_Dao_Result_Factory.Failure($"Failed to save component {component.ComponentPartNumber}");
                    }
                }
            }
            else
            {
                // No components - delete any existing ones
                await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
            }

            await _logger.LogInfoAsync($"Successfully updated part {part.PartNumber} with {components?.Count ?? 0} components");
            return Model_Dao_Result_Factory.Success("Part updated successfully");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error updating part {part.PartNumber}: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                $"Failed to update part {part.PartNumber}",
                Enum_ErrorSeverity.Error,
                ex,
                showDialog: false);
            return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result> DeactivatePartAsync(string partNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Deactivating part: {partNumber}");

            var result = await _daoPart.DeactivateAsync(partNumber);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to deactivate part {partNumber}: {result.ErrorMessage}");
            }
            else
            {
                await _logger.LogInfoAsync($"Successfully deactivated part {partNumber}");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error deactivating part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deactivating part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(string partNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting components for part: {partNumber}");
            var result = await _daoComponent.GetByParentPartAsync(partNumber);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to get components for {partNumber}: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting components for {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPartComponent>>($"Error retrieving components: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportCsvAsync(string csvFilePath)
    {
        try
        {
            await _logger.LogInfoAsync("Starting CSV import");

            if (string.IsNullOrWhiteSpace(csvFilePath))
            {
                return Model_Dao_Result_Factory.Failure<(int, int, int)>("CSV content is empty");
            }

            var lines = csvFilePath.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();

            if (lines.Count < 2)
            {
                return Model_Dao_Result_Factory.Failure<(int, int, int)>("CSV file must contain header and at least one data row");
            }

            // Validate header
            var header = lines[0];
            if (!header.Contains("PartNumber") || !header.Contains("Description") || !header.Contains("QuantityPerSkid"))
            {
                return Model_Dao_Result_Factory.Failure<(int, int, int)>("CSV must contain columns: PartNumber, Description, QuantityPerSkid, Components");
            }

            int newCount = 0;
            int updatedCount = 0;
            int errorCount = 0;
            var errors = new List<string>();

            for (int i = 1; i < lines.Count; i++)
            {
                try
                {
                    var fields = ParseCsvLine(lines[i]);

                    if (fields.Length < 3)
                    {
                        errors.Add($"Line {i + 1}: Invalid format - expected at least 3 fields");
                        errorCount++;
                        continue;
                    }

                    var partNumber = fields[0].Trim();
                    var description = fields[1].Trim();
                    var quantityStr = fields[2].Trim();
                    var componentsStr = fields.Length > 3 ? fields[3].Trim() : "";

                    if (!int.TryParse(quantityStr, out int quantity))
                    {
                        errors.Add($"Line {i + 1}: Invalid quantity '{quantityStr}'");
                        errorCount++;
                        continue;
                    }

                    var part = new Model_VolvoPart
                    {
                        PartNumber = partNumber,
                        Description = description,
                        QuantityPerSkid = quantity,
                        IsActive = true
                    };

                    // Parse components (format: "PART1:QTY1;PART2:QTY2")
                    var components = new List<Model_VolvoPartComponent>();
                    if (!string.IsNullOrWhiteSpace(componentsStr))
                    {
                        var componentPairs = componentsStr.Split(';');
                        foreach (var pair in componentPairs)
                        {
                            var parts = pair.Split(':');
                            if (parts.Length == 2)
                            {
                                if (int.TryParse(parts[1], out int compQty))
                                {
                                    components.Add(new Model_VolvoPartComponent
                                    {
                                        ComponentPartNumber = parts[0].Trim(),
                                        Quantity = compQty
                                    });
                                }
                            }
                        }
                    }

                    // Check if exists
                    var existing = await _daoPart.GetByIdAsync(partNumber);
                    bool isNew = !existing.IsSuccess || existing.Data == null;

                    Model_Dao_Result saveResult;
                    if (isNew)
                    {
                        saveResult = await AddPartAsync(part, components);
                    }
                    else
                    {
                        saveResult = await UpdatePartAsync(part, components);
                    }

                    if (saveResult.IsSuccess)
                    {
                        if (isNew)
                        {
                            newCount++;
                        }
                        else
                        {
                            updatedCount++;
                        }
                    }
                    else
                    {
                        errors.Add($"Line {i + 1}: {saveResult.ErrorMessage}");
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Line {i + 1}: {ex.Message}");
                    errorCount++;
                }
            }

            var summary = $"Import complete: {newCount} new, {updatedCount} updated, {errorCount} errors";
            await _logger.LogInfoAsync(summary);

            if (errors.Count > 0)
            {
                summary += "\nErrors:\n" + string.Join("\n", errors);
            }

            return Model_Dao_Result_Factory.Success((newCount, updatedCount, errorCount));
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error importing CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<(int, int, int)>($"Import failed: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false)
    {
        try
        {
            await _logger.LogInfoAsync($"Exporting parts to CSV (includeInactive={includeInactive})");

            var partsResult = await GetAllPartsAsync(includeInactive);
            if (!partsResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<string>(partsResult.ErrorMessage);
            }

            var csv = new StringBuilder();
            csv.AppendLine("PartNumber,Description,QuantityPerSkid,Components");

            foreach (var part in partsResult.Data ?? new List<Model_VolvoPart>())
            {
                var componentsResult = await GetComponentsAsync(part.PartNumber ?? string.Empty);
                var componentsStr = "";

                if (componentsResult.IsSuccess && componentsResult.Data?.Any() == true)
                {
                    componentsStr = string.Join(";", componentsResult.Data.Select(c => $"{c.ComponentPartNumber}:{c.Quantity}"));
                }

                csv.AppendLine($"{EscapeCsvField(part.PartNumber ?? string.Empty)},{EscapeCsvField(part.Description ?? string.Empty)},{part.QuantityPerSkid},{EscapeCsvField(componentsStr)}");
            }

            await _logger.LogInfoAsync($"Export complete: {partsResult.Data?.Count ?? 0} parts");
            return Model_Dao_Result_Factory.Success(csv.ToString());
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error exporting CSV: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Export failed: {ex.Message}");
        }
    }

    private string[] ParseCsvLine(string line)
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

    private string EscapeCsvField(string field)
    {
        if (field == null)
        {
            return "";
        }

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}

