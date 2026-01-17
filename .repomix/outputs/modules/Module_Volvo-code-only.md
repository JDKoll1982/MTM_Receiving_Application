# User Provided Header
MTM Receiving Application - Module_Volvo Code-Only Export

# Files

## File: Module_Volvo/Interfaces/IDao_VolvoPart.cs
```csharp
public interface IDao_VolvoPart
⋮----
Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(bool includeInactive = false);
Task<Model_Dao_Result<Model_VolvoPart>> GetByPartNumberAsync(string partNumber);
Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoPart part);
Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part);
Task<Model_Dao_Result> DeactivateAsync(string partNumber);
```

## File: Module_Volvo/Interfaces/IDao_VolvoShipment.cs
```csharp
public interface IDao_VolvoShipment
⋮----
Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetAllAsync(bool includeArchived = false);
Task<Model_Dao_Result<Model_VolvoShipment>> GetByIdAsync(int shipmentId);
Task<Model_Dao_Result<Model_VolvoShipment>> GetPendingAsync();
Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoShipment shipment);
Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipment shipment);
Task<Model_Dao_Result> ArchiveAsync(int shipmentId);
```

## File: Module_Volvo/Interfaces/IDao_VolvoShipmentLine.cs
```csharp
public interface IDao_VolvoShipmentLine
⋮----
Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(int shipmentId);
Task<Model_Dao_Result<int>> InsertAsync(Model_VolvoShipmentLine line);
Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipmentLine line);
Task<Model_Dao_Result> DeleteAsync(int lineId);
```

## File: Module_Volvo/Models/Model_EmailRecipient.cs
```csharp
public class Model_EmailRecipient
⋮----
public string ToOutlookFormat()
```

## File: Module_Volvo/Models/Model_VolvoEmailData.cs
```csharp
public class Model_VolvoEmailData
⋮----
public class DiscrepancyLineItem
```

## File: Module_Volvo/Models/VolvoShipmentStatus.cs
```csharp
public static class VolvoShipmentStatus
```

## File: Module_Volvo/Services/IService_VolvoAuthorization.cs
```csharp
public interface IService_VolvoAuthorization
⋮----
Task<Model_Dao_Result> CanManageShipmentsAsync();
Task<Model_Dao_Result> CanManageMasterDataAsync();
Task<Model_Dao_Result> CanCompleteShipmentsAsync();
Task<Model_Dao_Result> CanGenerateLabelsAsync();
```

## File: Module_Volvo/Services/Service_VolvoAuthorization.cs
```csharp
public class Service_VolvoAuthorization : IService_VolvoAuthorization
⋮----
private readonly IService_LoggingUtility _logger;
⋮----
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result> CanManageShipmentsAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanManageShipments");
return new Model_Dao_Result
⋮----
await _logger.LogErrorAsync($"Error checking shipment management authorization: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CanManageMasterDataAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanManageMasterData");
⋮----
await _logger.LogErrorAsync($"Error checking master data authorization: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CanCompleteShipmentsAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanCompleteShipments");
⋮----
await _logger.LogErrorAsync($"Error checking shipment completion authorization: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CanGenerateLabelsAsync()
⋮----
await _logger.LogInfoAsync("Authorization check: CanGenerateLabels");
⋮----
await _logger.LogErrorAsync($"Error checking label generation authorization: {ex.Message}", ex);
```

## File: Module_Volvo/Views/View_Volvo_Settings.xaml.cs
```csharp
public sealed partial class View_Volvo_Settings : Page
⋮----
private async void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await ViewModel.RefreshCommand.ExecuteAsync(null);
```

## File: Module_Volvo/Views/VolvoPartAddEditDialog.xaml
```
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.VolvoPartAddEditDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d"
    Title="Add/Edit Volvo Part"
    PrimaryButtonText="Save"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="OnSaveClicked"
    Width="500">

    <StackPanel Spacing="16" Padding="8">
        <!-- Part Number -->
        <TextBox x:Name="PartNumberTextBox"
                 Header="Part Number"
                 PlaceholderText="e.g., V-EMB-500"
                 MaxLength="50"/>

        <!-- Quantity Per Skid -->
        <NumberBox x:Name="QuantityPerSkidNumberBox"
                   Header="Quantity Per Skid"
                   PlaceholderText="e.g., 88"
                   Minimum="0"
                   Maximum="10000"
                   SpinButtonPlacementMode="Inline"
                   Value="0"/>

        <!-- Historical Integrity Warning (Edit Mode Only) -->
        <InfoBar x:Name="EditModeWarning"
                 Severity="Warning"
                 IsOpen="False"
                 IsClosable="False"
                 Message="Changes to quantity will NOT affect past shipments. Historical piece counts are preserved."/>

        <!-- Component Section (Simplified - Future Enhancement) -->
        <Expander Header="Components (Advanced)" IsExpanded="False">
            <StackPanel Spacing="8" Padding="8">
                <TextBlock Text="Component management will be available in a future update." 
                          Style="{StaticResource CaptionTextBlockStyle}"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                <TextBlock Text="For now, use CSV import to define components." 
                          Style="{StaticResource CaptionTextBlockStyle}"
                          Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>
        </Expander>
    </StackPanel>
</ContentDialog>
```

## File: Module_Volvo/Views/VolvoPartAddEditDialog.xaml.cs
```csharp
public sealed partial class VolvoPartAddEditDialog : ContentDialog
⋮----
public void InitializeForAdd()
⋮----
public void InitializeForEdit(Model_VolvoPart part)
⋮----
private void OnSaveClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
⋮----
if (string.IsNullOrWhiteSpace(PartNumberTextBox.Text))
⋮----
Part = new Model_VolvoPart
⋮----
PartNumber = PartNumberTextBox.Text.Trim().ToUpperInvariant(),
```

## File: Module_Volvo/Contracts/IService_Volvo.cs
```csharp
public interface IService_Volvo
⋮----
public Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
⋮----
public Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId);
public Task<string> FormatEmailTextAsync(
⋮----
public Task<Model_VolvoEmailData> FormatEmailDataAsync(
⋮----
public string FormatEmailAsHtml(Model_VolvoEmailData emailData);
public Task<Model_Dao_Result> ValidateShipmentAsync(
⋮----
public Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
⋮----
public Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync();
public Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync();
public Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber);
public Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync();
public Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
⋮----
public Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetShipmentLinesAsync(int shipmentId);
public Task<Model_Dao_Result> UpdateShipmentAsync(
⋮----
public Task<Model_Dao_Result<string>> ExportHistoryToCsvAsync(
```

## File: Module_Volvo/Contracts/IService_VolvoMasterData.cs
```csharp
public interface IService_VolvoMasterData
⋮----
public Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive = false);
public Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber);
public Task<Model_Dao_Result> AddPartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null);
public Task<Model_Dao_Result> UpdatePartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null);
public Task<Model_Dao_Result> DeactivatePartAsync(string partNumber);
public Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(string partNumber);
public Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportCsvAsync(string csvFilePath);
public Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false);
```

## File: Module_Volvo/Data/Dao_VolvoPartComponent.cs
```csharp
public class Dao_VolvoPartComponent
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetByParentPartAsync(string parentPartNumber)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPartComponent component)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteByParentPartAsync(string parentPartNumber)
⋮----
public async Task<Model_Dao_Result<Dictionary<string, List<Model_VolvoPartComponent>>>> GetComponentsByParentPartsAsync(
⋮----
private static Model_VolvoPartComponent MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoPartComponent
⋮----
ComponentPartNumber = reader.GetString(reader.GetOrdinal("component_part_number")),
Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
ComponentQuantityPerSkid = reader.GetInt32(reader.GetOrdinal("component_quantity_per_skid"))
```

## File: Module_Volvo/Handlers/Commands/AddPartToShipmentCommandHandler.cs
```csharp
public class AddPartToShipmentCommandHandler : IRequestHandler<AddPartToShipmentCommand, Model_Dao_Result>
⋮----
private readonly Dao_VolvoPart _partDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result> Handle(AddPartToShipmentCommand request, CancellationToken cancellationToken)
⋮----
var partResult = await _partDao.GetByIdAsync(request.PartNumber);
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
return Model_Dao_Result_Factory.Success();
```

## File: Module_Volvo/Handlers/Commands/AddVolvoPartCommandHandler.cs
```csharp
public class AddVolvoPartCommandHandler : IRequestHandler<AddVolvoPartCommand, Model_Dao_Result>
⋮----
private readonly Dao_VolvoPart _partDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result> Handle(AddVolvoPartCommand request, CancellationToken cancellationToken)
⋮----
var part = new Model_VolvoPart
⋮----
PartNumber = request.PartNumber.Trim().ToUpperInvariant(),
⋮----
return await _partDao.InsertAsync(part);
```

## File: Module_Volvo/Handlers/Commands/DeactivateVolvoPartCommandHandler.cs
```csharp
public class DeactivateVolvoPartCommandHandler : IRequestHandler<DeactivateVolvoPartCommand, Model_Dao_Result>
⋮----
private readonly Dao_VolvoPart _partDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result> Handle(DeactivateVolvoPartCommand request, CancellationToken cancellationToken)
⋮----
return await _partDao.DeactivateAsync(request.PartNumber);
```

## File: Module_Volvo/Handlers/Commands/ImportPartsCsvCommandHandler.cs
```csharp
public class ImportPartsCsvCommandHandler : IRequestHandler<ImportPartsCsvCommand, Model_Dao_Result<ImportPartsCsvResult>>
⋮----
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
⋮----
public async Task<Model_Dao_Result<ImportPartsCsvResult>> Handle(ImportPartsCsvCommand request, CancellationToken cancellationToken)
⋮----
if (string.IsNullOrWhiteSpace(request.CsvFilePath))
⋮----
var csvContent = await File.ReadAllTextAsync(request.CsvFilePath, cancellationToken);
⋮----
.Split('\n')
.Select(l => l.Trim())
.Where(l => !string.IsNullOrWhiteSpace(l))
.ToList();
⋮----
if (!header.Contains("PartNumber") || !header.Contains("QuantityPerSkid"))
⋮----
errors.Add($"Line {i + 1}: Invalid format - expected at least 2 fields");
⋮----
var partNumber = fields[0].Trim();
var quantityStr = fields[1].Trim();
var componentsStr = fields.Length > 2 ? fields[2].Trim() : string.Empty;
if (string.IsNullOrWhiteSpace(partNumber))
⋮----
errors.Add($"Line {i + 1}: Part number is required");
⋮----
if (!int.TryParse(quantityStr, out int quantity) || quantity <= 0)
⋮----
errors.Add($"Line {i + 1}: Invalid quantity '{quantityStr}'");
⋮----
var part = new Model_VolvoPart
⋮----
PartNumber = partNumber.Trim().ToUpperInvariant(),
⋮----
var existing = await _partDao.GetByIdAsync(part.PartNumber);
⋮----
Model_Dao_Result saveResult = isNew
? await _partDao.InsertAsync(part)
: await _partDao.UpdateAsync(part);
⋮----
errors.Add($"Line {i + 1}: {saveResult.ErrorMessage}");
⋮----
var deleteResult = await _componentDao.DeleteByParentPartAsync(part.PartNumber);
⋮----
errors.Add($"Line {i + 1}: Failed to clear components - {deleteResult.ErrorMessage}");
⋮----
var insertResult = await _componentDao.InsertAsync(component);
⋮----
errors.Add($"Line {i + 1}: Failed to save component {component.ComponentPartNumber}");
⋮----
await _componentDao.DeleteByParentPartAsync(part.PartNumber);
⋮----
errors.Add($"Line {i + 1}: {ex.Message}");
⋮----
var result = new ImportPartsCsvResult
⋮----
return Model_Dao_Result_Factory.Success(result);
⋮----
private static string[] ParseCsvLine(string line)
⋮----
var currentField = new StringBuilder();
⋮----
fields.Add(currentField.ToString());
currentField.Clear();
⋮----
currentField.Append(c);
⋮----
return fields.ToArray();
⋮----
private static List<Model_VolvoPartComponent> ParseComponents(string parentPartNumber, string componentsStr)
⋮----
if (string.IsNullOrWhiteSpace(componentsStr))
⋮----
var componentPairs = componentsStr.Split(';', StringSplitOptions.RemoveEmptyEntries);
⋮----
var parts = pair.Split(':');
if (parts.Length == 2 && int.TryParse(parts[1], out int compQty) && compQty > 0)
⋮----
components.Add(new Model_VolvoPartComponent
⋮----
ComponentPartNumber = parts[0].Trim(),
```

## File: Module_Volvo/Handlers/Commands/RemovePartFromShipmentCommandHandler.cs
```csharp
public class RemovePartFromShipmentCommandHandler : IRequestHandler<RemovePartFromShipmentCommand, Model_Dao_Result>
⋮----
public async Task<Model_Dao_Result> Handle(RemovePartFromShipmentCommand request, CancellationToken cancellationToken)
⋮----
if (string.IsNullOrWhiteSpace(request.PartNumber))
⋮----
return Model_Dao_Result_Factory.Failure("Part number is required");
⋮----
return await Task.FromResult(Model_Dao_Result_Factory.Success());
```

## File: Module_Volvo/Handlers/Commands/UpdateVolvoPartCommandHandler.cs
```csharp
public class UpdateVolvoPartCommandHandler : IRequestHandler<UpdateVolvoPartCommand, Model_Dao_Result>
⋮----
private readonly Dao_VolvoPart _partDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result> Handle(UpdateVolvoPartCommand request, CancellationToken cancellationToken)
⋮----
var part = new Model_VolvoPart
⋮----
PartNumber = request.PartNumber.Trim().ToUpperInvariant(),
⋮----
return await _partDao.UpdateAsync(part);
```

## File: Module_Volvo/Handlers/Queries/ExportPartsCsvQueryHandler.cs
```csharp
public class ExportPartsCsvQueryHandler : IRequestHandler<ExportPartsCsvQuery, Model_Dao_Result<string>>
⋮----
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
⋮----
public async Task<Model_Dao_Result<string>> Handle(ExportPartsCsvQuery request, CancellationToken cancellationToken)
⋮----
var partsResult = await _partDao.GetAllAsync(request.IncludeInactive);
⋮----
var csv = new StringBuilder();
csv.AppendLine("PartNumber,QuantityPerSkid,Components");
⋮----
var componentsResult = await _componentDao.GetByParentPartAsync(part.PartNumber);
⋮----
componentsStr = string.Join(";", componentsResult.Data.Select(c => $"{c.ComponentPartNumber}:{c.Quantity}"));
⋮----
csv.AppendLine($"{EscapeCsvField(part.PartNumber)},{part.QuantityPerSkid},{EscapeCsvField(componentsStr)}");
⋮----
return Model_Dao_Result_Factory.Success(csv.ToString());
⋮----
private static string EscapeCsvField(string? field)
⋮----
if (string.IsNullOrEmpty(field))
⋮----
if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
⋮----
return "\"" + field.Replace("\"", "\"\"") + "\"";
```

## File: Module_Volvo/Handlers/Queries/ExportShipmentsQueryHandler.cs
```csharp
public class ExportShipmentsQueryHandler : IRequestHandler<ExportShipmentsQuery, Model_Dao_Result<string>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
⋮----
public async Task<Model_Dao_Result<string>> Handle(ExportShipmentsQuery request, CancellationToken cancellationToken)
⋮----
var startDate = (request.StartDate ?? DateTimeOffset.Now.AddDays(-30)).DateTime;
⋮----
var historyResult = await _shipmentDao.GetHistoryAsync(startDate, endDate, statusFilter);
⋮----
var csv = new StringBuilder();
csv.AppendLine("ShipmentNumber,Date,PONumber,ReceiverNumber,Status,EmployeeNumber,Notes");
⋮----
csv.AppendLine($"{shipment.ShipmentNumber}," +
⋮----
return Model_Dao_Result_Factory.Success(csv.ToString());
⋮----
private static string EscapeCsv(string? value)
⋮----
if (string.IsNullOrEmpty(value))
⋮----
if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
⋮----
return "\"" + value.Replace("\"", "\"\"") + "\"";
⋮----
private static string NormalizeStatus(string? status)
⋮----
if (string.IsNullOrWhiteSpace(status))
⋮----
if (status.Equals("All", StringComparison.OrdinalIgnoreCase))
⋮----
if (status.Equals("Pending PO", StringComparison.OrdinalIgnoreCase))
⋮----
if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
```

## File: Module_Volvo/Handlers/Queries/FormatEmailDataQueryHandler.cs
```csharp
public class FormatEmailDataQueryHandler : IRequestHandler<FormatEmailDataQuery, Model_Dao_Result<Model_VolvoEmailData>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_LoggingUtility _logger;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result<Model_VolvoEmailData>> Handle(FormatEmailDataQuery request, CancellationToken cancellationToken)
⋮----
var shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
⋮----
var linesResult = await _lineDao.GetByShipmentIdAsync(request.ShipmentId);
⋮----
var explosionResult = await Helper_VolvoShipmentCalculations.CalculateComponentExplosionAsync(
⋮----
return Model_Dao_Result_Factory.Success(emailData);
⋮----
private static Model_VolvoEmailData BuildEmailData(
⋮----
var emailData = new Model_VolvoEmailData
⋮----
AdditionalNotes = string.IsNullOrWhiteSpace(shipment.Notes) ? null : shipment.Notes,
⋮----
var discrepancies = lines.Where(l => l.HasDiscrepancy).ToList();
⋮----
emailData.Discrepancies.Add(new Model_VolvoEmailData.DiscrepancyLineItem
```

## File: Module_Volvo/Handlers/Queries/GetAllVolvoPartsQueryHandler.cs
```csharp
public class GetAllVolvoPartsQueryHandler : IRequestHandler<GetAllVolvoPartsQuery, Model_Dao_Result<List<Model_VolvoPart>>>
⋮----
private readonly Dao_VolvoPart _partDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> Handle(GetAllVolvoPartsQuery request, CancellationToken cancellationToken)
⋮----
return await _partDao.GetAllAsync(request.IncludeInactive);
```

## File: Module_Volvo/Handlers/Queries/GetInitialShipmentDataQueryHandler.cs
```csharp
public class GetInitialShipmentDataQueryHandler : IRequestHandler<GetInitialShipmentDataQuery, Model_Dao_Result<InitialShipmentData>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
⋮----
public async Task<Model_Dao_Result<InitialShipmentData>> Handle(GetInitialShipmentDataQuery request, CancellationToken cancellationToken)
⋮----
var nextNumberResult = await _shipmentDao.GetNextShipmentNumberAsync();
⋮----
var data = new InitialShipmentData
⋮----
return Model_Dao_Result_Factory.Success(data);
```

## File: Module_Volvo/Handlers/Queries/GetPartComponentsQueryHandler.cs
```csharp
public class GetPartComponentsQueryHandler : IRequestHandler<GetPartComponentsQuery, Model_Dao_Result<List<Model_VolvoPartComponent>>>
⋮----
private readonly Dao_VolvoPartComponent _componentDao;
⋮----
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> Handle(GetPartComponentsQuery request, CancellationToken cancellationToken)
⋮----
if (string.IsNullOrWhiteSpace(request.PartNumber))
⋮----
return await _componentDao.GetByParentPartAsync(request.PartNumber);
```

## File: Module_Volvo/Handlers/Queries/GetPendingShipmentQueryHandler.cs
```csharp
public class GetPendingShipmentQueryHandler : IRequestHandler<GetPendingShipmentQuery, Model_Dao_Result<Model_VolvoShipment>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment>> Handle(GetPendingShipmentQuery request, CancellationToken cancellationToken)
⋮----
return await _shipmentDao.GetPendingAsync();
```

## File: Module_Volvo/Handlers/Queries/GetRecentShipmentsQueryHandler.cs
```csharp
public class GetRecentShipmentsQueryHandler : IRequestHandler<GetRecentShipmentsQuery, Model_Dao_Result<List<Model_VolvoShipment>>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> Handle(GetRecentShipmentsQuery request, CancellationToken cancellationToken)
⋮----
var startDate = DateTime.Now.AddDays(-days).Date;
⋮----
return await _shipmentDao.GetHistoryAsync(startDate, endDate, "all");
```

## File: Module_Volvo/Handlers/Queries/GetShipmentDetailQueryHandler.cs
```csharp
public class GetShipmentDetailQueryHandler : IRequestHandler<GetShipmentDetailQuery, Model_Dao_Result<ShipmentDetail>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
⋮----
public async Task<Model_Dao_Result<ShipmentDetail>> Handle(GetShipmentDetailQuery request, CancellationToken cancellationToken)
⋮----
var shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
⋮----
var linesResult = await _lineDao.GetByShipmentIdAsync(request.ShipmentId);
⋮----
var detail = new ShipmentDetail
⋮----
return Model_Dao_Result_Factory.Success(detail);
```

## File: Module_Volvo/Handlers/Queries/GetShipmentHistoryQueryHandler.cs
```csharp
public class GetShipmentHistoryQueryHandler : IRequestHandler<GetShipmentHistoryQuery, Model_Dao_Result<List<Model_VolvoShipment>>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> Handle(GetShipmentHistoryQuery request, CancellationToken cancellationToken)
⋮----
var startDate = (request.StartDate ?? DateTimeOffset.Now.AddDays(-30)).DateTime.Date;
⋮----
return await _shipmentDao.GetHistoryAsync(startDate, endDate, statusFilter);
⋮----
private static string NormalizeStatus(string? status)
⋮----
if (string.IsNullOrWhiteSpace(status))
⋮----
if (status.Equals("All", StringComparison.OrdinalIgnoreCase))
⋮----
if (status.Equals("Pending PO", StringComparison.OrdinalIgnoreCase))
⋮----
if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
```

## File: Module_Volvo/Models/Model_VolvoPart.cs
```csharp
public class Model_VolvoPart
```

## File: Module_Volvo/Models/Model_VolvoPartComponent.cs
```csharp
public class Model_VolvoPartComponent
```

## File: Module_Volvo/Models/Model_VolvoSetting.cs
```csharp
public partial class Model_VolvoSetting : ObservableObject
⋮----
private DateTime _modifiedDate = DateTime.Now;
```

## File: Module_Volvo/Models/Model_VolvoShipmentLine.cs
```csharp
public partial class Model_VolvoShipmentLine : ObservableObject
⋮----
partial void OnReceivedSkidCountChanged(int value)
⋮----
partial void OnQuantityPerSkidChanged(int value)
⋮----
partial void OnExpectedSkidCountChanged(double? value)
⋮----
partial void OnHasDiscrepancyChanged(bool value)
```

## File: Module_Volvo/Requests/Commands/AddVolvoPartCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/CompleteShipmentCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/DeactivateVolvoPartCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/ImportPartsCsvCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/RemovePartFromShipmentCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/SavePendingShipmentCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/UpdateShipmentCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Commands/UpdateVolvoPartCommand.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/ExportPartsCsvQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/ExportShipmentsQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/FormatEmailDataQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GenerateLabelCsvQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetAllVolvoPartsQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetInitialShipmentDataQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetPartComponentsQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetPendingShipmentQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetRecentShipmentsQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetShipmentDetailQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/GetShipmentHistoryQuery.cs
```csharp

```

## File: Module_Volvo/Requests/Queries/SearchVolvoPartsQuery.cs
```csharp

```

## File: Module_Volvo/Validators/AddPartToShipmentCommandValidator.cs
```csharp
public class AddPartToShipmentCommandValidator : AbstractValidator<AddPartToShipmentCommand>
⋮----
.NotEmpty().WithMessage("Part number is required")
.MaximumLength(50).WithMessage("Part number must not exceed 50 characters");
⋮----
.GreaterThan(0).WithMessage("Received skid count must be greater than 0");
⋮----
.NotNull().WithMessage("Expected skid count is required when there is a discrepancy")
.GreaterThan(0).WithMessage("Expected skid count must be greater than 0");
⋮----
.NotEmpty().WithMessage("Discrepancy note is required when there is a discrepancy")
.MaximumLength(500).WithMessage("Discrepancy note must not exceed 500 characters");
```

## File: Module_Volvo/Validators/AddVolvoPartCommandValidator.cs
```csharp
public class AddVolvoPartCommandValidator : AbstractValidator<AddVolvoPartCommand>
⋮----
.NotEmpty().WithMessage("Part number is required")
.MaximumLength(50).WithMessage("Part number must not exceed 50 characters");
⋮----
.GreaterThan(0).WithMessage("Quantity per skid must be greater than 0");
```

## File: Module_Volvo/Validators/DeactivateVolvoPartCommandValidator.cs
```csharp
public class DeactivateVolvoPartCommandValidator : AbstractValidator<DeactivateVolvoPartCommand>
⋮----
.NotEmpty().WithMessage("Part number is required")
.MaximumLength(50).WithMessage("Part number must not exceed 50 characters");
```

## File: Module_Volvo/Validators/ImportPartsCsvCommandValidator.cs
```csharp
public class ImportPartsCsvCommandValidator : AbstractValidator<ImportPartsCsvCommand>
⋮----
.NotEmpty().WithMessage("CSV file path is required")
.Must(File.Exists).WithMessage("CSV file does not exist");
```

## File: Module_Volvo/Validators/SavePendingShipmentCommandValidator.cs
```csharp
public class SavePendingShipmentCommandValidator : AbstractValidator<SavePendingShipmentCommand>
⋮----
.NotEmpty().WithMessage("Shipment date is required")
.LessThanOrEqualTo(System.DateTimeOffset.Now).WithMessage("Shipment date cannot be in the future");
⋮----
.NotEmpty().WithMessage("At least one part is required");
⋮----
.MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
```

## File: Module_Volvo/Validators/UpdateShipmentCommandValidator.cs
```csharp
public class UpdateShipmentCommandValidator : AbstractValidator<UpdateShipmentCommand>
⋮----
.GreaterThan(0).WithMessage("Shipment ID must be greater than 0");
⋮----
.NotEmpty().WithMessage("Shipment date is required")
.LessThanOrEqualTo(System.DateTimeOffset.Now).WithMessage("Shipment date cannot be in the future");
⋮----
.NotEmpty().WithMessage("At least one part is required");
⋮----
.MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
⋮----
.MaximumLength(50).WithMessage("PO number must not exceed 50 characters");
⋮----
.MaximumLength(50).WithMessage("Receiver number must not exceed 50 characters");
RuleForEach(x => x.Parts).ChildRules(part =>
⋮----
part.RuleFor(p => p.PartNumber)
.NotEmpty().WithMessage("Part number is required");
part.RuleFor(p => p.ReceivedSkidCount)
.GreaterThan(0).WithMessage("Received skid count must be greater than 0");
```

## File: Module_Volvo/Validators/UpdateVolvoPartCommandValidator.cs
```csharp
public class UpdateVolvoPartCommandValidator : AbstractValidator<UpdateVolvoPartCommand>
⋮----
.NotEmpty().WithMessage("Part number is required")
.MaximumLength(50).WithMessage("Part number must not exceed 50 characters");
⋮----
.GreaterThan(0).WithMessage("Quantity per skid must be greater than 0");
```

## File: Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml.cs
```csharp
public sealed partial class View_Volvo_ShipmentEntry : Page
⋮----
private async void OnLoaded(object sender, RoutedEventArgs e)
⋮----
await ViewModel.InitializeAsync();
⋮----
private async void AddPartButton_Click(object sender, RoutedEventArgs e)
⋮----
private async void RemoveSelectedPartButton_Click(object sender, RoutedEventArgs e)
⋮----
var noSelectionDialog = new ContentDialog
⋮----
await noSelectionDialog.ShowAsync();
⋮----
var confirmDialog = new ContentDialog
⋮----
var result = await confirmDialog.ShowAsync();
⋮----
ViewModel.Parts.Remove(ViewModel.SelectedPart);
⋮----
private async void ReportDiscrepancyButton_Click(object sender, RoutedEventArgs e)
⋮----
await ViewModel.ToggleDiscrepancyCommand.ExecuteAsync(line);
⋮----
private async void ViewDiscrepancyButton_Click(object sender, RoutedEventArgs e)
⋮----
private async void RemoveDiscrepancyButton_Click(object sender, RoutedEventArgs e)
⋮----
var confirmResult = await confirmDialog.ShowAsync();
⋮----
private async Task ShowViewDiscrepancyDialogAsync(Model_VolvoShipmentLine line)
⋮----
var content = new StackPanel { Spacing = 12 };
content.Children.Add(new TextBlock
⋮----
var diffText = diff > 0 ? $"+{diff}" : diff.ToString();
⋮----
if (!string.IsNullOrWhiteSpace(line.DiscrepancyNote))
⋮----
Margin = new Thickness(0, 8, 0, 4),
⋮----
content.Children.Add(new TextBox
⋮----
var dialog = new ContentDialog
⋮----
await dialog.ShowAsync();
⋮----
private async Task ShowAddPartDialogAsync()
⋮----
await ViewModel.LoadAllPartsForDialogAsync();
⋮----
var errorMessage = new TextBlock
⋮----
Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
⋮----
Margin = new Thickness(0, 0, 0, 12)
⋮----
var searchBox = new TextBox
⋮----
var partsListView = new ListView
⋮----
Margin = new Thickness(0, 0, 0, 12),
⋮----
var receivedSkidsBox = new TextBox
⋮----
InputScope = new InputScope { Names = { new InputScopeName(InputScopeNameValue.Number) } }
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
var filtered = allParts.Where(part =>
⋮----
var partNumber = part.PartNumber.ToLower();
⋮----
return searchIndex == searchText.Length || partNumber.Contains(searchText);
}).ToList();
⋮----
content.Children.Add(errorMessage);
content.Children.Add(searchBox);
var partsHeader = new TextBlock
⋮----
Margin = new Thickness(0, 0, 0, 4)
⋮----
content.Children.Add(partsHeader);
content.Children.Add(partsListView);
content.Children.Add(receivedSkidsBox);
⋮----
var deferral = args.GetDeferral();
⋮----
if (!int.TryParse(receivedSkidsBox.Text, out int skidCount) || skidCount < 1 || skidCount > 99)
⋮----
if (ViewModel.Parts.Any(p => p.PartNumber.Equals(selectedPart.PartNumber, StringComparison.OrdinalIgnoreCase)))
⋮----
var newLine = new Model_VolvoShipmentLine
⋮----
ViewModel.Parts.Add(newLine);
await ViewModel.Logger.LogInfoAsync($"User added part {selectedPart.PartNumber}, {skidCount} skids ({calculatedPieces} pcs)");
⋮----
deferral.Complete();
```

## File: Module_Volvo/Data/Dao_VolvoPart.cs
```csharp
public class Dao_VolvoPart
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(bool includeInactive = false)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result<Model_VolvoPart>> GetByIdAsync(string partNumber)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPart part)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part)
⋮----
public async Task<Model_Dao_Result> DeactivateAsync(string partNumber)
⋮----
var checkResult = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result<Dictionary<string, Model_VolvoPart>>> GetPartsByNumbersAsync(List<string> partNumbers)
⋮----
private static Model_VolvoPart MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoPart
⋮----
PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date"))
```

## File: Module_Volvo/Data/Dao_VolvoShipmentLine.cs
```csharp
public class Dao_VolvoShipmentLine
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result> InsertAsync(Model_VolvoShipmentLine line)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetByShipmentIdAsync(int shipmentId)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipmentLine line)
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int lineId)
⋮----
private static Model_VolvoShipmentLine MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoShipmentLine
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
ShipmentId = reader.GetInt32(reader.GetOrdinal("shipment_id")),
PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
ReceivedSkidCount = reader.GetInt32(reader.GetOrdinal("received_skid_count")),
CalculatedPieceCount = reader.GetInt32(reader.GetOrdinal("calculated_piece_count")),
HasDiscrepancy = reader.GetBoolean(reader.GetOrdinal("has_discrepancy")),
ExpectedSkidCount = reader.IsDBNull(reader.GetOrdinal("expected_skid_count"))
⋮----
: reader.GetInt32(reader.GetOrdinal("expected_skid_count")),
DiscrepancyNote = reader.IsDBNull(reader.GetOrdinal("discrepancy_note"))
⋮----
: reader.GetString(reader.GetOrdinal("discrepancy_note"))
```

## File: Module_Volvo/Handlers/Commands/CompleteShipmentCommandHandler.cs
```csharp
public class CompleteShipmentCommandHandler : IRequestHandler<CompleteShipmentCommand, Model_Dao_Result<int>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly IService_VolvoAuthorization _authService;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task<Model_Dao_Result<int>> Handle(CompleteShipmentCommand request, CancellationToken cancellationToken)
⋮----
var authResult = await _authService.CanCompleteShipmentsAsync();
⋮----
var pendingResult = await _shipmentDao.GetPendingAsync();
⋮----
if (!string.Equals(pendingShipment.Notes ?? string.Empty, request.Notes ?? string.Empty, StringComparison.Ordinal))
⋮----
var updateResult = await _shipmentDao.UpdateAsync(new Model_VolvoShipment
⋮----
var existingLinesResult = await _lineDao.GetByShipmentIdAsync(pendingShipment.Id);
⋮----
var deleteResult = await _lineDao.DeleteAsync(existingLine.Id);
⋮----
var partResult = await _partDao.GetByIdAsync(partDto.PartNumber);
⋮----
var line = new Model_VolvoShipmentLine
⋮----
var lineResult = await _lineDao.InsertAsync(line);
⋮----
var completeResult = await _shipmentDao.CompleteAsync(
⋮----
var shipment = new Model_VolvoShipment
⋮----
var insertResult = await _shipmentDao.InsertAsync(shipment);
⋮----
var completeInsertResult = await _shipmentDao.CompleteAsync(shipmentId, request.PONumber, request.ReceiverNumber);
```

## File: Module_Volvo/Handlers/Commands/UpdateShipmentCommandHandler.cs
```csharp
public class UpdateShipmentCommandHandler : IRequestHandler<UpdateShipmentCommand, Model_Dao_Result>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_VolvoAuthorization _authService;
private readonly IService_LoggingUtility _logger;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result> Handle(UpdateShipmentCommand request, CancellationToken cancellationToken)
⋮----
var shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
var updateResult = await _shipmentDao.UpdateAsync(shipment);
⋮----
var existingLines = await _lineDao.GetByShipmentIdAsync(shipment.Id);
⋮----
var deleteResult = await _lineDao.DeleteAsync(line.Id);
⋮----
var partResult = await _partDao.GetByIdAsync(part.PartNumber);
⋮----
var line = new Model_VolvoShipmentLine
⋮----
newLines.Add(line);
⋮----
var insertResult = await _lineDao.InsertAsync(line);
⋮----
if (shipment.Status == VolvoShipmentStatus.Completed && !string.IsNullOrWhiteSpace(shipment.PONumber))
⋮----
await Helper_VolvoShipmentCalculations.GenerateLabelCsvAsync(
⋮----
return Model_Dao_Result_Factory.Success("Shipment updated successfully");
```

## File: Module_Volvo/Handlers/Queries/SearchVolvoPartsQueryHandler.cs
```csharp
public class SearchVolvoPartsQueryHandler : IRequestHandler<SearchVolvoPartsQuery, Model_Dao_Result<List<Model_VolvoPart>>>
⋮----
private readonly Dao_VolvoPart _partDao;
⋮----
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> Handle(SearchVolvoPartsQuery request, CancellationToken cancellationToken)
⋮----
var allPartsResult = await _partDao.GetAllAsync(includeInactive: false);
⋮----
.Where(p => string.IsNullOrWhiteSpace(request.SearchText) ||
p.PartNumber.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
.Take(request.MaxResults)
.ToList();
return Model_Dao_Result_Factory.Success(filteredParts);
```

## File: Module_Volvo/Helpers/Helper_VolvoShipmentCalculations.cs
```csharp
public static class Helper_VolvoShipmentCalculations
⋮----
public static async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
⋮----
var partResult = await partDao.GetByIdAsync(line.PartNumber);
⋮----
if (aggregatedPieces.ContainsKey(line.PartNumber))
⋮----
var componentsResult = await componentDao.GetByParentPartAsync(line.PartNumber);
⋮----
await logger.LogWarningAsync(
⋮----
if (aggregatedPieces.ContainsKey(component.ComponentPartNumber))
⋮----
return Model_Dao_Result_Factory.Success(aggregatedPieces);
⋮----
public static async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(
⋮----
var authResult = await authService.CanGenerateLabelsAsync();
⋮----
await logger.LogInfoAsync($"Generating label CSV for shipment {shipmentId}");
var shipmentResult = await shipmentDao.GetByIdAsync(shipmentId);
⋮----
var linesResult = await lineDao.GetByShipmentIdAsync(shipmentId);
⋮----
string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string csvDirectory = Path.Combine(appDataPath, "MTM_Receiving_Application", "Volvo", "Labels");
Directory.CreateDirectory(csvDirectory);
⋮----
string filePath = Path.Combine(csvDirectory, fileName);
var csvContent = new StringBuilder();
csvContent.AppendLine("Material,Quantity,Employee,Date,Time,Receiver,Notes");
string dateFormatted = shipment.ShipmentDate.ToString("MM/dd/yyyy");
string timeFormatted = DateTime.Now.ToString("HH:mm:ss");
foreach (var kvp in aggregatedPieces.OrderBy(x => x.Key))
⋮----
csvContent.AppendLine($"{kvp.Key},{kvp.Value},{shipment.EmployeeNumber},{dateFormatted},{timeFormatted},,");
⋮----
await File.WriteAllTextAsync(filePath, csvContent.ToString());
await logger.LogInfoAsync($"Label CSV generated: {filePath}");
⋮----
await logger.LogErrorAsync($"Error generating label CSV: {ex.Message}", ex);
```

## File: Module_Volvo/Models/Model_VolvoShipment.cs
```csharp
public class Model_VolvoShipment
⋮----
public string ShipmentDateDisplay => ShipmentDate.ToString("MM/dd/yyyy");
```

## File: Module_Volvo/Requests/Commands/AddPartToShipmentCommand.cs
```csharp

```

## File: Module_Volvo/Requests/ShipmentLineDto.cs
```csharp

```

## File: Module_Volvo/Services/Service_VolvoMasterData.cs
```csharp
public class Service_VolvoMasterData : IService_VolvoMasterData
⋮----
private readonly Dao_VolvoPart _daoPart;
private readonly Dao_VolvoPartComponent _daoComponent;
private readonly IService_LoggingUtility _logger;
private readonly IService_ErrorHandler _errorHandler;
⋮----
_daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
_daoComponent = daoComponent ?? throw new ArgumentNullException(nameof(daoComponent));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(bool includeInactive)
⋮----
await _logger.LogInfoAsync($"Getting all Volvo parts (includeInactive={includeInactive})");
var result = await _daoPart.GetAllAsync(includeInactive);
⋮----
await _logger.LogErrorAsync($"Failed to get parts: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error getting all parts: {ex.Message}", ex);
await _errorHandler.HandleErrorAsync(
⋮----
public async Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber)
⋮----
await _logger.LogInfoAsync($"Getting part by number: {partNumber}");
var result = await _daoPart.GetByIdAsync(partNumber);
⋮----
await _logger.LogErrorAsync($"Failed to get part {partNumber}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error getting part {partNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> AddPartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null)
⋮----
await _logger.LogInfoAsync($"Adding new part: {part.PartNumber}");
if (string.IsNullOrWhiteSpace(part.PartNumber))
⋮----
return Model_Dao_Result_Factory.Failure("Part number is required");
⋮----
return Model_Dao_Result_Factory.Failure("Quantity per skid must be non-negative");
⋮----
var insertResult = await _daoPart.InsertAsync(part);
⋮----
await _logger.LogErrorAsync($"Failed to insert part {part.PartNumber}: {insertResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure(insertResult.ErrorMessage);
⋮----
var componentResult = await _daoComponent.InsertAsync(component);
⋮----
await _logger.LogErrorAsync($"Failed to insert component {component.ComponentPartNumber}: {componentResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure($"Failed to save component {component.ComponentPartNumber}");
⋮----
await _logger.LogInfoAsync($"Successfully added part {part.PartNumber} with {components?.Count ?? 0} components");
return Model_Dao_Result_Factory.Success("Part added successfully");
⋮----
await _logger.LogErrorAsync($"Error adding part {part.PartNumber}: {ex.Message}", ex);
⋮----
return Model_Dao_Result_Factory.Failure($"Error adding part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> UpdatePartAsync(Model_VolvoPart part, List<Model_VolvoPartComponent>? components = null)
⋮----
await _logger.LogInfoAsync($"Updating part: {part.PartNumber}");
⋮----
var updateResult = await _daoPart.UpdateAsync(part);
⋮----
await _logger.LogErrorAsync($"Failed to update part {part.PartNumber}: {updateResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure(updateResult.ErrorMessage);
⋮----
var deleteResult = await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
⋮----
await _logger.LogErrorAsync($"Failed to delete components for {part.PartNumber}: {deleteResult.ErrorMessage}");
return Model_Dao_Result_Factory.Failure($"Failed to update components: {deleteResult.ErrorMessage}");
⋮----
var insertResult = await _daoComponent.InsertAsync(component);
⋮----
await _logger.LogErrorAsync($"Failed to insert component {component.ComponentPartNumber}: {insertResult.ErrorMessage}");
⋮----
await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
⋮----
await _logger.LogInfoAsync($"Successfully updated part {part.PartNumber} with {components?.Count ?? 0} components");
return Model_Dao_Result_Factory.Success("Part updated successfully");
⋮----
await _logger.LogErrorAsync($"Error updating part {part.PartNumber}: {ex.Message}", ex);
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result> DeactivatePartAsync(string partNumber)
⋮----
await _logger.LogInfoAsync($"Deactivating part: {partNumber}");
var result = await _daoPart.DeactivateAsync(partNumber);
⋮----
await _logger.LogErrorAsync($"Failed to deactivate part {partNumber}: {result.ErrorMessage}");
⋮----
await _logger.LogInfoAsync($"Successfully deactivated part {partNumber}");
⋮----
await _logger.LogErrorAsync($"Error deactivating part {partNumber}: {ex.Message}", ex);
return Model_Dao_Result_Factory.Failure($"Error deactivating part: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(string partNumber)
⋮----
await _logger.LogInfoAsync($"Getting components for part: {partNumber}");
var result = await _daoComponent.GetByParentPartAsync(partNumber);
⋮----
await _logger.LogErrorAsync($"Failed to get components for {partNumber}: {result.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Error getting components for {partNumber}: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportCsvAsync(string csvFilePath)
⋮----
await _logger.LogInfoAsync("Starting CSV import");
if (string.IsNullOrWhiteSpace(csvFilePath))
⋮----
var lines = csvFilePath.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
⋮----
if (!header.Contains("PartNumber") || !header.Contains("QuantityPerSkid"))
⋮----
errors.Add($"Line {i + 1}: Invalid format - expected at least 3 fields");
⋮----
var partNumber = fields[0].Trim();
var quantityStr = fields[1].Trim();
var componentsStr = fields.Length > 2 ? fields[2].Trim() : "";
if (!int.TryParse(quantityStr, out int quantity))
⋮----
errors.Add($"Line {i + 1}: Invalid quantity '{quantityStr}'");
⋮----
var part = new Model_VolvoPart
⋮----
// Parse components (format: "PART1:QTY1;PART2:QTY2")
⋮----
if (!string.IsNullOrWhiteSpace(componentsStr))
⋮----
var componentPairs = componentsStr.Split(';');
⋮----
var parts = pair.Split(':');
⋮----
if (int.TryParse(parts[1], out int compQty))
⋮----
components.Add(new Model_VolvoPartComponent
⋮----
ComponentPartNumber = parts[0].Trim(),
⋮----
var existing = await _daoPart.GetByIdAsync(partNumber);
⋮----
Model_Dao_Result saveResult;
⋮----
errors.Add($"Line {i + 1}: {saveResult.ErrorMessage}");
⋮----
errors.Add($"Line {i + 1}: {ex.Message}");
⋮----
await _logger.LogInfoAsync(summary);
⋮----
summary += "\nErrors:\n" + string.Join("\n", errors);
⋮----
return Model_Dao_Result_Factory.Success((newCount, updatedCount, errorCount));
⋮----
await _logger.LogErrorAsync($"Error importing CSV: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false)
⋮----
await _logger.LogInfoAsync($"Exporting parts to CSV (includeInactive={includeInactive})");
⋮----
var csv = new StringBuilder();
csv.AppendLine("PartNumber,QuantityPerSkid,Components");
⋮----
componentsStr = string.Join(";", componentsResult.Data.Select(c => $"{c.ComponentPartNumber}:{c.Quantity}"));
⋮----
csv.AppendLine($"{EscapeCsvField(part.PartNumber ?? string.Empty)},{part.QuantityPerSkid},{EscapeCsvField(componentsStr)}");
⋮----
await _logger.LogInfoAsync($"Export complete: {partsResult.Data?.Count ?? 0} parts");
return Model_Dao_Result_Factory.Success(csv.ToString());
⋮----
await _logger.LogErrorAsync($"Error exporting CSV: {ex.Message}", ex);
⋮----
private string[] ParseCsvLine(string line)
⋮----
var currentField = new StringBuilder();
⋮----
fields.Add(currentField.ToString());
currentField.Clear();
⋮----
currentField.Append(c);
⋮----
return fields.ToArray();
⋮----
private string EscapeCsvField(string field)
⋮----
if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
⋮----
return "\"" + field.Replace("\"", "\"\"") + "\"";
```

## File: Module_Volvo/Validators/CompleteShipmentCommandValidator.cs
```csharp
public class CompleteShipmentCommandValidator : AbstractValidator<CompleteShipmentCommand>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
⋮----
.NotEmpty().WithMessage("Shipment date is required")
.LessThanOrEqualTo(System.DateTimeOffset.Now).WithMessage("Shipment date cannot be in the future");
⋮----
.NotEmpty().WithMessage("At least one part is required");
⋮----
.NotEmpty().WithMessage("PO number is required")
.MaximumLength(50).WithMessage("PO number must not exceed 50 characters");
⋮----
.NotEmpty().WithMessage("Receiver number is required")
.MaximumLength(50).WithMessage("Receiver number must not exceed 50 characters");
⋮----
.MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters");
RuleForEach(x => x.Parts).ChildRules(part =>
⋮----
part.RuleFor(p => p.PartNumber)
.NotEmpty().WithMessage("Part number is required");
part.RuleFor(p => p.ReceivedSkidCount)
.GreaterThan(0).WithMessage("Received skid count must be greater than 0");
⋮----
.MustAsync(HasPendingShipmentAsync)
.WithMessage("No pending shipment found to complete.");
⋮----
private async Task<bool> HasPendingShipmentAsync(
⋮----
var result = await _shipmentDao.GetPendingAsync();
```

## File: Module_Volvo/Views/View_Volvo_History.xaml.cs
```csharp
public sealed partial class View_Volvo_History : Page
⋮----
private async void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
⋮----
await ViewModel.LoadRecentShipmentsCommand.ExecuteAsync(null);
```

## File: Module_Volvo/Views/View_Volvo_Settings.xaml
```
<Page
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.View_Volvo_Settings"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    mc:Ignorable="d"
    Loaded="OnPageLoaded">

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock 
            Grid.Row="0"
            Text="Volvo Dunnage Parts Master Data"
            Style="{StaticResource TitleTextBlockStyle}"/>

        <!-- Toolbar -->
        <CommandBar Grid.Row="1" DefaultLabelPosition="Right">
            <AppBarButton 
                Icon="Add" 
                Label="Add Part" 
                Command="{x:Bind ViewModel.AddPartCommand, Mode=OneWay}"/>
            <AppBarButton 
                Icon="Edit" 
                Label="Edit Part" 
                Command="{x:Bind ViewModel.EditPartCommand, Mode=OneWay}"/>
            <AppBarButton 
                Icon="Delete" 
                Label="Deactivate" 
                Command="{x:Bind ViewModel.DeactivatePartCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="PreviewLink" 
                Label="View Components" 
                Command="{x:Bind ViewModel.ViewComponentsCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Import" 
                Label="Import CSV" 
                Command="{x:Bind ViewModel.ImportCsvCommand, Mode=OneWay}"/>
            <AppBarButton 
                Icon="Save" 
                Label="Export CSV" 
                Command="{x:Bind ViewModel.ExportCsvCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarButton 
                Icon="Refresh" 
                Label="Refresh" 
                Command="{x:Bind ViewModel.RefreshCommand, Mode=OneWay}"/>
            <AppBarSeparator/>
            <AppBarToggleButton 
                Icon="View" 
                Label="Show Inactive"
                IsChecked="{x:Bind ViewModel.ShowInactive, Mode=TwoWay}"/>
        </CommandBar>

        <!-- Data Grid -->
        <Grid Grid.Row="2">
            <controls:DataGrid
                ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                AutoGenerateColumns="False"
                IsReadOnly="True"
                GridLinesVisibility="All"
                HeadersVisibility="Column"
                AlternatingRowBackground="{ThemeResource SystemListLowColor}"
                SelectionMode="Single">
                <controls:DataGrid.Columns>
                    <controls:DataGridTemplateColumn Header="Part Number" Width="*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoPart">
                                <TextBlock Text="{x:Bind PartNumber, Mode=OneTime}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    <controls:DataGridTemplateColumn Header="Qty/Skid" Width="100">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoPart">
                                <TextBlock Text="{x:Bind QuantityPerSkid, Mode=OneTime}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    <controls:DataGridTemplateColumn Header="Active" Width="80">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoPart">
                                <TextBlock 
                                    Text="{x:Bind IsActive}" 
                                    Foreground="{x:Bind IsActive, Converter={StaticResource BoolToColorConverter}, ConverterParameter='Green|Gray'}"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                </controls:DataGrid.Columns>
            </controls:DataGrid>
        </Grid>

        <!-- Status Bar -->
        <Grid Grid.Row="3" ColumnSpacing="12">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>

            <ProgressRing 
                Grid.Column="0"
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" 
                Height="20"
                Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay}"/>

            <TextBlock 
                Grid.Column="1"
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                VerticalAlignment="Center"/>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Volvo/Views/VolvoShipmentEditDialog.xaml.cs
```csharp
public sealed partial class VolvoShipmentEditDialog : ContentDialog
⋮----
public void LoadShipment(Model_VolvoShipment shipment, ObservableCollection<Model_VolvoShipmentLine> lines, ObservableCollection<Model_VolvoPart> availableParts)
⋮----
ShipmentNumberBox.Text = shipment.ShipmentNumber.ToString();
⋮----
Lines.Clear();
⋮----
Lines.Add(line);
⋮----
System.Diagnostics.Debug.WriteLine($"[EditDialog] Loaded {_allParts.Count} parts for selection");
⋮----
System.Diagnostics.Debug.WriteLine($"[EditDialog] ListView ItemsSource set with {_allParts.Count} parts");
⋮----
System.Diagnostics.Debug.WriteLine("[EditDialog] WARNING: No parts available to load!");
⋮----
public Model_VolvoShipment GetUpdatedShipment()
⋮----
Shipment.PONumber = string.IsNullOrWhiteSpace(PONumberBox.Text) ? null : PONumberBox.Text;
Shipment.ReceiverNumber = string.IsNullOrWhiteSpace(ReceiverNumberBox.Text) ? null : ReceiverNumberBox.Text;
Shipment.Notes = string.IsNullOrWhiteSpace(NotesBox.Text) ? null : NotesBox.Text;
⋮----
public ObservableCollection<Model_VolvoShipmentLine> GetUpdatedLines()
⋮----
private void ToggleAddPartPanel()
⋮----
private void CloseAddPartPanel()
⋮----
private void OnPartSearchTextChanged(object sender, TextChangedEventArgs e)
⋮----
if (string.IsNullOrWhiteSpace(searchText))
⋮----
var filtered = _allParts.Where(part =>
⋮----
var partNumber = part.PartNumber.ToLower();
⋮----
return searchIndex == searchText.Length || partNumber.Contains(searchText);
}).ToList();
⋮----
private void ConfirmAddPart()
⋮----
if (!int.TryParse(AddPartQuantityBox.Text, out int skidCount) || skidCount < 1 || skidCount > 99)
⋮----
if (Lines.Any(p => p.PartNumber.Equals(selectedPart.PartNumber, StringComparison.OrdinalIgnoreCase)))
⋮----
var newLine = new Model_VolvoShipmentLine
⋮----
Lines.Add(newLine);
⋮----
private void RemoveSelectedLine()
⋮----
Lines.Remove(selectedLine);
⋮----
var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
⋮----
timer.Stop();
⋮----
timer.Start();
⋮----
private async void ReportDiscrepancyButton_Click(object sender, RoutedEventArgs e)
⋮----
var expectedSkidsBox = new NumberBox
⋮----
var noteBox = new TextBox
⋮----
var panel = new StackPanel { Spacing = 12 };
panel.Children.Add(expectedSkidsBox);
panel.Children.Add(noteBox);
var dialog = new ContentDialog
⋮----
var result = await dialog.ShowAsync();
⋮----
if (expectedSkidsBox.Value < 1 || string.IsNullOrWhiteSpace(noteBox.Text))
⋮----
line.DiscrepancyNote = noteBox.Text.Trim();
⋮----
private async void ViewDiscrepancyButton_Click(object sender, RoutedEventArgs e)
⋮----
var content = new StackPanel { Spacing = 12 };
content.Children.Add(new TextBlock
⋮----
var diffText = diff > 0 ? $"+{diff}" : diff.ToString();
⋮----
if (!string.IsNullOrWhiteSpace(line.DiscrepancyNote))
⋮----
Margin = new Thickness(0, 8, 0, 4),
⋮----
content.Children.Add(new TextBox
⋮----
await dialog.ShowAsync();
⋮----
private async void RemoveDiscrepancyButton_Click(object sender, RoutedEventArgs e)
⋮----
var confirmDialog = new ContentDialog
⋮----
var confirmResult = await confirmDialog.ShowAsync();
```

## File: Module_Volvo/Data/Dao_VolvoSettings.cs
```csharp
public class Dao_VolvoSettings
⋮----
public async Task<Model_Dao_Result<Model_VolvoSetting>> GetSettingAsync(string settingKey)
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoSetting>>> GetAllSettingsAsync(string? category = null)
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
public async Task<Model_Dao_Result> UpsertSettingAsync(string settingKey, string settingValue, string modifiedBy)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> ResetSettingAsync(string settingKey, string modifiedBy)
⋮----
private static Model_VolvoSetting MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoSetting
⋮----
SettingKey = reader.GetString(reader.GetOrdinal("setting_key")),
SettingValue = reader.GetString(reader.GetOrdinal("setting_value")),
SettingType = reader.GetString(reader.GetOrdinal("setting_type")),
Category = reader.GetString(reader.GetOrdinal("category")),
Description = reader.IsDBNull(reader.GetOrdinal("description"))
⋮----
: reader.GetString(reader.GetOrdinal("description")),
DefaultValue = reader.GetString(reader.GetOrdinal("default_value")),
MinValue = reader.IsDBNull(reader.GetOrdinal("min_value"))
⋮----
: reader.GetInt32(reader.GetOrdinal("min_value")),
MaxValue = reader.IsDBNull(reader.GetOrdinal("max_value"))
⋮----
: reader.GetInt32(reader.GetOrdinal("max_value")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by"))
⋮----
: reader.GetString(reader.GetOrdinal("modified_by"))
```

## File: Module_Volvo/Data/Dao_VolvoShipment.cs
```csharp
public class Dao_VolvoShipment
⋮----
_connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
⋮----
public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> InsertAsync(Model_VolvoShipment shipment)
⋮----
await using var connection = new MySqlConnection(_connectionString);
await connection.OpenAsync();
await using var command = new MySqlCommand("sp_volvo_shipment_insert", connection)
⋮----
command.Parameters.AddWithValue("@p_shipment_date", shipment.ShipmentDate);
command.Parameters.AddWithValue("@p_employee_number", shipment.EmployeeNumber);
command.Parameters.AddWithValue("@p_notes", shipment.Notes ?? (object)DBNull.Value);
var newIdParam = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
⋮----
var shipmentNumberParam = new MySqlParameter("@p_shipment_number", MySqlDbType.Int32)
⋮----
command.Parameters.Add(newIdParam);
command.Parameters.Add(shipmentNumberParam);
await command.ExecuteNonQueryAsync();
var shipmentId = Convert.ToInt32(newIdParam.Value);
var shipmentNumber = Convert.ToInt32(shipmentNumberParam.Value);
⋮----
public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoShipment shipment)
⋮----
var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
return new Model_Dao_Result
⋮----
public async Task<Model_Dao_Result> CompleteAsync(int shipmentId, string poNumber, string receiverNumber)
⋮----
return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
⋮----
public async Task<Model_Dao_Result> DeleteAsync(int shipmentId)
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment>> GetPendingAsync()
⋮----
return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetByIdAsync(int shipmentId)
⋮----
await using var command = new MySqlCommand
⋮----
command.Parameters.AddWithValue("@p_id", shipmentId);
await using var reader = await command.ExecuteReaderAsync();
if (await reader.ReadAsync())
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
⋮----
return await Helper_Database_StoredProcedure.ExecuteListAsync(
⋮----
private static Model_VolvoShipment MapFromReader(IDataReader reader)
⋮----
return new Model_VolvoShipment
⋮----
Id = reader.GetInt32(reader.GetOrdinal("id")),
ShipmentDate = reader.GetDateTime(reader.GetOrdinal("shipment_date")),
ShipmentNumber = reader.GetInt32(reader.GetOrdinal("shipment_number")),
PONumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
⋮----
: reader.GetString(reader.GetOrdinal("po_number")),
ReceiverNumber = reader.IsDBNull(reader.GetOrdinal("receiver_number"))
⋮----
: reader.GetString(reader.GetOrdinal("receiver_number")),
EmployeeNumber = reader.GetString(reader.GetOrdinal("employee_number")),
Notes = reader.IsDBNull(reader.GetOrdinal("notes"))
⋮----
: reader.GetString(reader.GetOrdinal("notes")),
Status = reader.GetString(reader.GetOrdinal("status")),
CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
IsArchived = reader.GetBoolean(reader.GetOrdinal("is_archived"))
⋮----
public async Task<Model_Dao_Result<int>> GetNextShipmentNumberAsync()
⋮----
await using var command = new MySqlCommand(query, connection);
var result = await command.ExecuteScalarAsync();
var nextNumber = Convert.ToInt32(result);
```

## File: Module_Volvo/Handlers/Commands/SavePendingShipmentCommandHandler.cs
```csharp
public class SavePendingShipmentCommandHandler : IRequestHandler<SavePendingShipmentCommand, Model_Dao_Result<int>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
⋮----
public async Task<Model_Dao_Result<int>> Handle(SavePendingShipmentCommand request, CancellationToken cancellationToken)
⋮----
var shipment = new Model_VolvoShipment
⋮----
var updateResult = await _shipmentDao.UpdateAsync(shipment);
⋮----
saveResult = new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
⋮----
var pendingResult = await _shipmentDao.GetPendingAsync();
⋮----
saveResult = await _shipmentDao.InsertAsync(shipment);
⋮----
var existingLinesResult = await _lineDao.GetByShipmentIdAsync(shipmentId);
⋮----
var deleteResult = await _lineDao.DeleteAsync(existingLine.Id);
⋮----
var partResult = await _partDao.GetByIdAsync(partDto.PartNumber);
⋮----
var line = new Model_VolvoShipmentLine
⋮----
var lineResult = await _lineDao.InsertAsync(line);
```

## File: Module_Volvo/Handlers/Queries/GenerateLabelCsvQueryHandler.cs
```csharp
public class GenerateLabelCsvQueryHandler : IRequestHandler<GenerateLabelCsvQuery, Model_Dao_Result<string>>
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_VolvoAuthorization _authService;
private readonly IService_LoggingUtility _logger;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
⋮----
public async Task<Model_Dao_Result<string>> Handle(GenerateLabelCsvQuery request, CancellationToken cancellationToken)
⋮----
return await Helper_VolvoShipmentCalculations.GenerateLabelCsvAsync(
```

## File: Module_Volvo/ViewModels/ViewModel_Volvo_History.cs
```csharp
public partial class ViewModel_Volvo_History : ViewModel_Shared_Base
⋮----
private readonly IMediator _mediator;
⋮----
private DateTimeOffset? _startDate = DateTimeOffset.Now.AddDays(-30);
⋮----
_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
⋮----
private void GoBack()
⋮----
var contentFrame = mainWindow.GetContentFrame();
⋮----
contentFrame.GoBack();
⋮----
private async Task LoadRecentShipmentsAsync()
⋮----
var result = await _mediator.Send(new GetRecentShipmentsQuery());
⋮----
History.Clear();
⋮----
History.Add(shipment);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task FilterAsync()
⋮----
var result = await _mediator.Send(new GetShipmentHistoryQuery
⋮----
private async Task ViewDetailAsync()
⋮----
await _logger.LogInfoAsync($"Loading details for shipment ID: {SelectedShipment.Id}");
var result = await _mediator.Send(new GetShipmentDetailQuery
⋮----
details.AppendLine($"Shipment #{shipment.ShipmentNumber}");
details.AppendLine($"Date: {shipment.ShipmentDate:d}");
details.AppendLine($"PO Number: {shipment.PONumber ?? "N/A"}");
details.AppendLine($"Receiver: {shipment.ReceiverNumber ?? "N/A"}");
details.AppendLine($"Status: {shipment.Status}");
details.AppendLine();
details.AppendLine($"Parts ({lines.Count}):");
⋮----
details.AppendLine($"  • {line.PartNumber}: {line.ReceivedSkidCount} skids ({line.CalculatedPieceCount} pieces)");
⋮----
details.AppendLine($"    ⚠ Discrepancy: Expected {line.ExpectedSkidCount} skids");
⋮----
if (!string.IsNullOrWhiteSpace(shipment.Notes))
⋮----
details.AppendLine($"Notes: {shipment.Notes}");
⋮----
var dialog = new ContentDialog
⋮----
Content = new ScrollViewer
⋮----
Content = new TextBlock
⋮----
Text = details.ToString(),
⋮----
await dialog.ShowAsync();
⋮----
await _logger.LogInfoAsync($"Successfully loaded {lines.Count} lines");
⋮----
private bool CanViewDetail() => SelectedShipment != null && !IsBusy;
⋮----
private async Task EditAsync()
⋮----
var detailResult = await _mediator.Send(new GetShipmentDetailQuery
⋮----
var partsResult = await _mediator.Send(new GetAllVolvoPartsQuery
⋮----
dialog.LoadShipment(detailResult.Data.Shipment, linesCollection, availableParts);
var result = await dialog.ShowAsync();
⋮----
var updatedShipment = dialog.GetUpdatedShipment();
var updatedLines = dialog.GetUpdatedLines();
var updateCommand = new UpdateShipmentCommand
⋮----
ShipmentDate = new DateTimeOffset(updatedShipment.ShipmentDate),
⋮----
Parts = updatedLines.Select(line => new ShipmentLineDto
⋮----
? Convert.ToInt32(line.ExpectedSkidCount.Value)
⋮----
}).ToList()
⋮----
var updateResult = await _mediator.Send(updateCommand);
⋮----
private bool CanEdit() => SelectedShipment != null && !IsBusy;
⋮----
private async Task ExportAsync()
⋮----
var result = await _mediator.Send(new ExportShipmentsQuery
⋮----
if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
⋮----
partial void OnSelectedShipmentChanged(Model_VolvoShipment? value)
⋮----
ViewDetailCommand.NotifyCanExecuteChanged();
EditCommand.NotifyCanExecuteChanged();
```

## File: Module_Volvo/ViewModels/ViewModel_Volvo_Settings.cs
```csharp
public partial class ViewModel_Volvo_Settings : ViewModel_Shared_Base
⋮----
private readonly IMediator _mediator;
⋮----
_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
⋮----
private async Task RefreshAsync()
⋮----
var result = await _mediator.Send(new GetAllVolvoPartsQuery
⋮----
Parts.Clear();
foreach (var part in result.Data.OrderBy(p => p.PartNumber))
⋮----
Parts.Add(part);
⋮----
ActivePartsCount = Parts.Count(p => p.IsActive);
⋮----
await _errorHandler.ShowUserErrorAsync(
⋮----
_errorHandler.HandleException(
⋮----
private async Task AddPartAsync()
⋮----
dialog.InitializeForAdd();
⋮----
dialog.XamlRoot = windowService.GetXamlRoot();
⋮----
var result = await dialog.ShowAsync();
⋮----
var saveResult = await _mediator.Send(new AddVolvoPartCommand
⋮----
private async Task EditPartAsync()
⋮----
dialog.InitializeForEdit(SelectedPart);
⋮----
var saveResult = await _mediator.Send(new UpdateVolvoPartCommand
⋮----
private bool CanEditPart() => SelectedPart != null && !IsBusy;
⋮----
private async Task DeactivatePartAsync()
⋮----
var dialog = new ContentDialog
⋮----
var deactivateResult = await _mediator.Send(new DeactivateVolvoPartCommand
⋮----
private bool CanDeactivatePart() => SelectedPart?.IsActive == true && !IsBusy;
⋮----
private async Task ViewComponentsAsync()
⋮----
var result = await _mediator.Send(new GetPartComponentsQuery
⋮----
? string.Join("\n", result.Data.Select(c => $"• {c.ComponentPartNumber} (Qty: {c.Quantity})"))
⋮----
await dialog.ShowAsync();
⋮----
private bool CanViewComponents() => SelectedPart != null && !IsBusy;
⋮----
private async Task ImportCsvAsync()
⋮----
var picker = new FileOpenPicker();
picker.FileTypeFilter.Add(".csv");
⋮----
var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
var file = await picker.PickSingleFileAsync();
⋮----
var result = await _mediator.Send(new ImportPartsCsvCommand
⋮----
private async Task ExportCsvAsync()
⋮----
var picker = new FileSavePicker();
picker.FileTypeChoices.Add("CSV File", new[] { ".csv" });
⋮----
var file = await picker.PickSaveFileAsync();
⋮----
var result = await _mediator.Send(new ExportPartsCsvQuery
⋮----
await FileIO.WriteTextAsync(file, result.Data);
⋮----
partial void OnShowInactiveChanged(bool value)
⋮----
partial void OnSelectedPartChanged(Model_VolvoPart? value)
⋮----
EditPartCommand.NotifyCanExecuteChanged();
DeactivatePartCommand.NotifyCanExecuteChanged();
ViewComponentsCommand.NotifyCanExecuteChanged();
```

## File: Module_Volvo/Views/View_Volvo_History.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.View_Volvo_History"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    mc:Ignorable="d"
    Loaded="OnPageLoaded">

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="12">
            <Button 
                Command="{x:Bind ViewModel.GoBackCommand}"
                VerticalAlignment="Center"
                ToolTipService.ToolTip="Back to Shipment Entry">
                <SymbolIcon Symbol="Back"/>
            </Button>
            <TextBlock 
                Text="Volvo Shipment History"
                Style="{StaticResource TitleTextBlockStyle}"
                VerticalAlignment="Center"/>
        </StackPanel>

        <!-- Filters -->
        <StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="16">
            <CalendarDatePicker
                Header="Start Date"
                Date="{x:Bind ViewModel.StartDate, Mode=TwoWay}"
                Width="200"/>
            
            <CalendarDatePicker
                Header="End Date"
                Date="{x:Bind ViewModel.EndDate, Mode=TwoWay}"
                Width="200"/>
            
            <ComboBox
                Header="Status"
                ItemsSource="{x:Bind ViewModel.StatusOptions}"
                SelectedItem="{x:Bind ViewModel.StatusFilter, Mode=TwoWay}"
                Width="150"/>
            
            <Button
                Content="Filter"
                Command="{x:Bind ViewModel.FilterCommand}"
                Style="{StaticResource AccentButtonStyle}"
                VerticalAlignment="Bottom"/>
            
            <Button
                Content="Export CSV"
                Command="{x:Bind ViewModel.ExportCommand}"
                VerticalAlignment="Bottom"/>
        </StackPanel>

        <!-- DataGrid -->
        <controls:DataGrid
            Grid.Row="2"
            ItemsSource="{x:Bind ViewModel.History, Mode=OneWay}"
            SelectedItem="{x:Bind ViewModel.SelectedShipment, Mode=TwoWay}"
            AutoGenerateColumns="False"
            IsReadOnly="True"
            CanUserReorderColumns="False"
            GridLinesVisibility="Horizontal">
            
            <controls:DataGrid.Columns>
                <controls:DataGridTemplateColumn Header="Shipment #" Width="100">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate x:DataType="models:Model_VolvoShipment">
                            <TextBlock Text="{x:Bind ShipmentNumber, Mode=OneTime}" />
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>

                <controls:DataGridTemplateColumn Header="Date" Width="120">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate x:DataType="models:Model_VolvoShipment">
                            <TextBlock Text="{x:Bind ShipmentDateDisplay, Mode=OneTime}" />
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>

                <controls:DataGridTemplateColumn Header="PO Number" Width="120">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate x:DataType="models:Model_VolvoShipment">
                            <TextBlock Text="{x:Bind PONumber, Mode=OneTime}" />
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>

                <controls:DataGridTemplateColumn Header="Receiver" Width="120">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate x:DataType="models:Model_VolvoShipment">
                            <TextBlock Text="{x:Bind ReceiverNumber, Mode=OneTime}" />
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>

                <controls:DataGridTemplateColumn Header="Status" Width="120">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate x:DataType="models:Model_VolvoShipment">
                            <TextBlock Text="{x:Bind Status, Mode=OneTime}" />
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>

                <controls:DataGridTemplateColumn Header="Notes" Width="*">
                    <controls:DataGridTemplateColumn.CellTemplate>
                        <DataTemplate x:DataType="models:Model_VolvoShipment">
                            <TextBlock Text="{x:Bind Notes, Mode=OneTime}" TextTrimming="CharacterEllipsis" />
                        </DataTemplate>
                    </controls:DataGridTemplateColumn.CellTemplate>
                </controls:DataGridTemplateColumn>
            </controls:DataGrid.Columns>
        </controls:DataGrid>

        <!-- Status Bar -->
        <StackPanel Grid.Row="3" Orientation="Horizontal" Spacing="12">
            <ProgressRing 
                IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}"
                Width="20" Height="20"/>
            <TextBlock 
                Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}"
                VerticalAlignment="Center"/>
            <Button
                Content="View Details"
                Command="{x:Bind ViewModel.ViewDetailCommand}"
                Margin="12,0,0,0"/>
            <Button
                Content="Edit"
                Command="{x:Bind ViewModel.EditCommand}"/>
        </StackPanel>
    </Grid>
</Page>
```

## File: Module_Volvo/Views/View_Volvo_ShipmentEntry.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<Page
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.View_Volvo_ShipmentEntry"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:mi="using:Material.Icons.WinUI3"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:views="using:MTM_Receiving_Application.Module_Volvo.Views"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    mc:Ignorable="d"
    Loaded="OnLoaded">

    <Page.Resources>
        <converters:Converter_BooleanToVisibility x:Key="BooleanToVisibilityConverter"/>
        <converters:NullableDoubleToDoubleConverter x:Key="NullableDoubleToDoubleConverter"/>
        <converters:Converter_BoolToString x:Key="BoolToStringConverter"/>
        <converters:Converter_IntToString x:Key="IntToStringConverter"/>
        <converters:Converter_InverseBool x:Key="InverseBoolConverter"/>
        <converters:Converter_NullableDoubleToString x:Key="NullableDoubleToStringConverter"/>
        <converters:Converter_NullableIntToString x:Key="NullableIntToStringConverter"/>
    </Page.Resources>

    <Grid Padding="24" RowSpacing="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Success Message -->
        <InfoBar Grid.Row="0"
                 IsOpen="{x:Bind ViewModel.IsSuccessMessageVisible, Mode=OneWay}"
                 Severity="Success"
                 Message="{x:Bind ViewModel.SuccessMessage, Mode=OneWay}"
                 IsClosable="True"/>

        <!-- Header Section -->
        <StackPanel Grid.Row="1" Spacing="16">
            <!-- Shipment Info Row -->
            <Grid ColumnSpacing="16">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>

                <CalendarDatePicker Grid.Column="0"
                                    Header="Shipment Date"
                                    Date="{x:Bind ViewModel.ShipmentDate, Mode=TwoWay}"
                                    HorizontalAlignment="Stretch"/>

                <NumberBox Grid.Column="1"
                           Header="Shipment Number"
                           Value="{x:Bind ViewModel.ShipmentNumber, Mode=TwoWay}"
                           Minimum="1"
                           SpinButtonPlacementMode="Hidden"
                           HorizontalAlignment="Stretch"/>

                <Button Grid.Column="2"
                        VerticalAlignment="Bottom"
                        Style="{StaticResource AccentButtonStyle}"
                        Click="AddPartButton_Click"
                        Margin="0,0,0,4"
                        MinWidth="120">
                    <StackPanel Orientation="Horizontal" Spacing="8">
                        <SymbolIcon Symbol="Add"/>
                        <TextBlock Text="Add Part"/>
                    </StackPanel>
                </Button>
            </Grid>
        </StackPanel>

        <!-- Parts Entry DataGrid -->
        <Grid Grid.Row="2">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
                <RowDefinition Height="Auto"/>
            </Grid.RowDefinitions>

            <TextBlock Grid.Row="0" 
                       Text="Parts Received" 
                       Style="{StaticResource SubtitleTextBlockStyle}"
                       Margin="0,0,0,8"/>

            <controls:DataGrid x:Name="ShipmentLinesGrid" Grid.Row="1"
                               ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
                               SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}"
                               AutoGenerateColumns="False"
                               CanUserSortColumns="False"
                               CanUserReorderColumns="False"
                               GridLinesVisibility="All">
                <controls:DataGrid.Columns>
                    <controls:DataGridTemplateColumn Header="Part Number" Width="2*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind PartNumber, Mode=OneWay}" 
                                           VerticalAlignment="Center"
                                           Margin="8,0"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Received Skids"
                                                      Width="*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBox Text="{x:Bind ReceivedSkidCount, Mode=TwoWay, Converter={StaticResource IntToStringConverter}}"
                                         InputScope="Number"
                                         HorizontalAlignment="Stretch"
                                         VerticalAlignment="Stretch"
                                         BorderThickness="0"
                                         Background="Transparent"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Calculated Pieces" Width="*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind CalculatedPieceCount, Mode=OneWay}" 
                                           VerticalAlignment="Center"
                                           Margin="8,0"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Discrepancy" Width="230">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <StackPanel Orientation="Horizontal" Spacing="4">
                                    <Button Content="Report"
                                            Click="ReportDiscrepancyButton_Click"
                                            IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}"
                                            Tag="{x:Bind}"
                                            MinWidth="50"/>
                                    <Button Content="View"
                                            Click="ViewDiscrepancyButton_Click"
                                            IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"
                                            Tag="{x:Bind}"
                                            MinWidth="50"/>
                                    <Button Content="Remove"
                                            Click="RemoveDiscrepancyButton_Click"
                                            IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"
                                            Tag="{x:Bind}"
                                            MinWidth="50"/>
                                </StackPanel>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Expected Skids"
                                                      Width="0"
                                                      Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind ExpectedSkidCount, Mode=OneWay, Converter={StaticResource NullableDoubleToStringConverter}}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Expected Pieces" 
                                                      Width="0"
                                                      Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind ExpectedPieceCount, Mode=OneWay, Converter={StaticResource NullableIntToStringConverter}}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Difference" 
                                                      Width="0"
                                                      Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind PieceDifference, Mode=OneWay, Converter={StaticResource NullableIntToStringConverter}}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>

                    <controls:DataGridTemplateColumn Header="Discrepancy Note"
                                                      Width="0"
                                                      Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind DiscrepancyNote, Mode=OneWay, TargetNullValue=''}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                </controls:DataGrid.Columns>
            </controls:DataGrid>

            <!-- Order Notes Section -->
            <TextBox Grid.Row="2"
                     Header="Order Notes (applies to entire shipment)"
                     Text="{x:Bind ViewModel.Notes, Mode=TwoWay}"
                     PlaceholderText="Enter any additional notes for this shipment..."
                     TextWrapping="Wrap"
                     AcceptsReturn="True"
                     MinHeight="60"
                     MaxHeight="120"
                     Margin="0,12,0,0"/>
        </Grid>

        <!-- Action Buttons -->
        <CommandBar Grid.Row="3" DefaultLabelPosition="Right">
            <AppBarButton Icon="Delete" Label="Remove Selected Part" Click="RemoveSelectedPartButton_Click"/>
            <AppBarSeparator/>
            <AppBarButton Label="Generate Labels" Command="{x:Bind ViewModel.GenerateLabelsCommand}">
                <AppBarButton.Icon>
                    <FontIcon Glyph="&#xE8B7;"/>
                </AppBarButton.Icon>
            </AppBarButton>
            <AppBarButton Icon="Mail" Label="Preview Email" Command="{x:Bind ViewModel.PreviewEmailCommand}"/>
            <AppBarSeparator/>
            <AppBarButton Icon="Save" Label="Save as Pending" Command="{x:Bind ViewModel.SaveAsPendingCommand}"/>
            <AppBarButton Icon="Accept" 
                          Label="Complete Shipment" 
                          Command="{x:Bind ViewModel.CompleteShipmentCommand}"
                          Visibility="{x:Bind ViewModel.HasPendingShipment, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
            <AppBarSeparator/>
            <AppBarButton Icon="Calendar" Label="View History" Command="{x:Bind ViewModel.ViewHistoryCommand}"/>
        </CommandBar>

        <!-- Status Bar -->
        <Grid Grid.Row="3" Margin="0,8,0,0" Visibility="Collapsed">
            <StackPanel Orientation="Horizontal" Spacing="8">
                <ProgressRing IsActive="{x:Bind ViewModel.IsBusy, Mode=OneWay}" 
                              Width="20" 
                              Height="20"
                              Visibility="{x:Bind ViewModel.IsBusy, Mode=OneWay, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                <TextBlock Text="{x:Bind ViewModel.StatusMessage, Mode=OneWay}" 
                           VerticalAlignment="Center"/>
            </StackPanel>
        </Grid>
    </Grid>
</Page>
```

## File: Module_Volvo/Views/VolvoShipmentEditDialog.xaml
```
<?xml version="1.0" encoding="utf-8"?>
<ContentDialog
    x:Class="MTM_Receiving_Application.Module_Volvo.Views.VolvoShipmentEditDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:models="using:MTM_Receiving_Application.Module_Volvo.Models"
    xmlns:controls="using:CommunityToolkit.WinUI.UI.Controls"
    xmlns:converters="using:MTM_Receiving_Application.Module_Core.Converters"
    Title="Edit Shipment"
    PrimaryButtonText="Save Changes"
    CloseButtonText="Cancel"
    DefaultButton="Primary">
    
    <ContentDialog.Resources>
        <x:Double x:Key="ContentDialogMaxWidth">1400</x:Double>
        <x:Double x:Key="ContentDialogMinWidth">1400</x:Double>
        <x:Double x:Key="ContentDialogMaxHeight">800</x:Double>
        <converters:NullableDoubleToDoubleConverter x:Key="NullableDoubleToDoubleConverter"/>
        <converters:Converter_BoolToString x:Key="BoolToStringConverter"/>
        <converters:Converter_IntToString x:Key="IntToStringConverter"/>
        <converters:Converter_InverseBool x:Key="InverseBoolConverter"/>
        <converters:Converter_NullableDoubleToString x:Key="NullableDoubleToStringConverter"/>
        <converters:Converter_NullableIntToString x:Key="NullableIntToStringConverter"/>
    </ContentDialog.Resources>

    <!-- No ScrollViewer - fixed height layout -->
    <Grid Padding="20" RowSpacing="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Row 0: Error/Success Message Bar -->
        <InfoBar x:Name="ValidationErrorBar"
                 Grid.Row="0"
                 Severity="Error"
                 IsOpen="False"
                 IsClosable="True"
                 Margin="0,0,0,8"/>
        
        <!-- Row 1: Shipment Details (Compact 4-column layout) -->
        <Grid Grid.Row="1" ColumnSpacing="12" RowSpacing="8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <CalendarDatePicker Grid.Column="0"
                                Header="Shipment Date"
                                x:Name="ShipmentDatePicker"
                                HorizontalAlignment="Stretch"/>
            
            <TextBox Grid.Column="1"
                     Header="Shipment #"
                     x:Name="ShipmentNumberBox"
                     IsReadOnly="True"
                     HorizontalAlignment="Stretch"/>
            
            <TextBox Grid.Column="2"
                     Header="PO Number"
                     PlaceholderText="Optional"
                     x:Name="PONumberBox"
                     HorizontalAlignment="Stretch"/>
            
            <TextBox Grid.Column="3"
                     Header="Receiver #"
                     PlaceholderText="Optional"
                     x:Name="ReceiverNumberBox"
                     HorizontalAlignment="Stretch"/>
        </Grid>
        
        <!-- Row 1: Notes (Full Width, Compact) -->
        <TextBox Grid.Row="1"
                 Header="Notes (applies to entire shipment)"
                 PlaceholderText="Optional notes..."
                 x:Name="NotesBox"
                 TextWrapping="Wrap"
                 AcceptsReturn="True"
                 Height="60"
                 HorizontalAlignment="Stretch"/>
        
        <!-- Row 2: Parts Section (Main Content) -->
        <Grid Grid.Row="2">
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>
            
            <!-- Parts Header with Actions -->
            <Grid Grid.Row="0" Margin="0,0,0,8">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                
                <TextBlock Grid.Column="0" 
                           Text="Parts Received" 
                           Style="{StaticResource SubtitleTextBlockStyle}"
                           VerticalAlignment="Center"/>
                
                <Button Grid.Column="2"
                        x:Name="ToggleAddPartButton"
                        Style="{StaticResource AccentButtonStyle}"
                        Height="32"
                        MinWidth="100"
                        Margin="0,0,8,0">
                    <StackPanel Orientation="Horizontal" Spacing="6">
                        <SymbolIcon x:Name="AddPartIcon" Symbol="Add"/>
                        <TextBlock x:Name="AddPartButtonText" Text="Add Part"/>
                    </StackPanel>
                </Button>
                
                <Button Grid.Column="3"
                        x:Name="RemovePartButton"
                        Content="Remove Selected"
                        Height="32"
                        MinWidth="120"/>
            </Grid>
            
            <!-- Add Part Panel (Collapsible) -->
            <Border Grid.Row="1"
                    x:Name="AddPartPanel"
                    Visibility="Collapsed"
                    BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                    BorderThickness="1"
                    CornerRadius="4"
                    Padding="12"
                    Margin="0,0,0,8"
                    Background="{ThemeResource LayerFillColorDefaultBrush}">
                <Grid ColumnSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="2*"/>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    
                    <!-- Left: Search & Parts List -->
                    <StackPanel Grid.Column="0" Spacing="8">
                        <TextBlock x:Name="AddPartErrorMessage"
                                   Foreground="Red"
                                   FontWeight="SemiBold"
                                   TextWrapping="Wrap"
                                   Visibility="Collapsed"/>
                        
                        <TextBox x:Name="PartSearchBox"
                                 PlaceholderText="Search part numbers..."
                                 TextChanged="OnPartSearchTextChanged"/>
                        
                        <Border BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                BorderThickness="1"
                                CornerRadius="4"
                                Height="120">
                            <ListView x:Name="AddPartListView"
                                      SelectionMode="Single"
                                      DisplayMemberPath="PartNumber"
                                      Background="{ThemeResource ControlFillColorDefaultBrush}"/>
                        </Border>
                    </StackPanel>
                    
                    <!-- Middle: Quantity Input -->
                    <StackPanel Grid.Column="1" VerticalAlignment="Top" Margin="0,26,0,0">
                        <TextBox x:Name="AddPartQuantityBox"
                                 Header="Received Skids"
                                 PlaceholderText="1-99"
                                 InputScope="Number"
                                 HorizontalAlignment="Stretch"/>
                    </StackPanel>
                    
                    <!-- Right: Action Buttons -->
                    <StackPanel Grid.Column="2" VerticalAlignment="Top" Spacing="8" Margin="0,26,0,0">
                        <Button x:Name="ConfirmAddPartButton"
                                Content="Add to List"
                                Style="{StaticResource AccentButtonStyle}"
                                MinWidth="100"/>
                        <Button x:Name="CancelAddPartButton"
                                Content="Cancel"
                                MinWidth="100"/>
                    </StackPanel>
                </Grid>
            </Border>
            
            <!-- DataGrid (Fixed Height, Scrollable) -->
            <controls:DataGrid Grid.Row="2"
                               x:Name="PartsDataGrid"
                               AutoGenerateColumns="False"
                               CanUserReorderColumns="False"
                               CanUserSortColumns="False"
                               SelectionMode="Single"
                               GridLinesVisibility="All"
                               HorizontalScrollBarVisibility="Auto"
                               VerticalScrollBarVisibility="Auto">
                
                <controls:DataGrid.Columns>
                    <controls:DataGridTemplateColumn Header="Part Number" Width="2*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind PartNumber, Mode=OneWay}" 
                                           VerticalAlignment="Center"
                                           Margin="8,0"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <controls:DataGridTemplateColumn Header="Received Skids" Width="*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBox Text="{x:Bind ReceivedSkidCount, Mode=TwoWay, Converter={StaticResource IntToStringConverter}}"
                                         InputScope="Number"
                                         HorizontalAlignment="Stretch"
                                         VerticalAlignment="Stretch"
                                         BorderThickness="0"
                                         Background="Transparent"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <controls:DataGridTemplateColumn Header="Calculated Pieces" Width="*">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind CalculatedPieceCount, Mode=OneWay}" 
                                           VerticalAlignment="Center"
                                           Margin="8,0"/>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <controls:DataGridTemplateColumn Header="Discrepancy" Width="230">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <StackPanel Orientation="Horizontal" Spacing="4">
                                    <Button Content="Report"
                                            Click="ReportDiscrepancyButton_Click"
                                            IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay, Converter={StaticResource InverseBoolConverter}}"
                                            Tag="{x:Bind}"
                                            MinWidth="50"/>
                                    <Button Content="View"
                                            Click="ViewDiscrepancyButton_Click"
                                            IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"
                                            Tag="{x:Bind}"
                                            MinWidth="50"/>
                                    <Button Content="Remove"
                                            Click="RemoveDiscrepancyButton_Click"
                                            IsEnabled="{x:Bind HasDiscrepancy, Mode=OneWay}"
                                            Tag="{x:Bind}"
                                            MinWidth="50"/>
                                </StackPanel>
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <!-- Hidden columns for data integrity -->
                    <controls:DataGridTemplateColumn Header="Expected Skids" Width="0" Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind ExpectedSkidCount, Mode=OneWay, Converter={StaticResource NullableDoubleToStringConverter}}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <controls:DataGridTemplateColumn Header="Expected Pieces" Width="0" Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind ExpectedPieceCount, Mode=OneWay, Converter={StaticResource NullableIntToStringConverter}}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <controls:DataGridTemplateColumn Header="Difference" Width="0" Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind PieceDifference, Mode=OneWay, Converter={StaticResource NullableIntToStringConverter}}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                    
                    <controls:DataGridTemplateColumn Header="Discrepancy Note" Width="0" Visibility="Collapsed">
                        <controls:DataGridTemplateColumn.CellTemplate>
                            <DataTemplate x:DataType="models:Model_VolvoShipmentLine">
                                <TextBlock Text="{x:Bind DiscrepancyNote, Mode=OneWay}" />
                            </DataTemplate>
                        </controls:DataGridTemplateColumn.CellTemplate>
                    </controls:DataGridTemplateColumn>
                </controls:DataGrid.Columns>
            </controls:DataGrid>
        </Grid>
    </Grid>
</ContentDialog>
```

## File: Module_Volvo/Services/Service_Volvo.cs
```csharp
public class Service_Volvo : IService_Volvo
⋮----
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_LoggingUtility _logger;
private readonly IService_VolvoAuthorization _authService;
⋮----
_shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
_lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
_partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
_componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
_logger = logger ?? throw new ArgumentNullException(nameof(logger));
_authService = authService ?? throw new ArgumentNullException(nameof(authService));
⋮----
public async Task<Model_Dao_Result<Dictionary<string, int>>> CalculateComponentExplosionAsync(
⋮----
await _logger.LogInfoAsync("Calculating component explosion for shipment lines");
⋮----
var partResult = await _partDao.GetByIdAsync(line.PartNumber);
⋮----
if (aggregatedPieces.ContainsKey(line.PartNumber))
⋮----
var componentsResult = await _componentDao.GetByParentPartAsync(line.PartNumber);
⋮----
await _logger.LogWarningAsync(
⋮----
if (aggregatedPieces.ContainsKey(component.ComponentPartNumber))
⋮----
await _logger.LogInfoAsync($"Component explosion complete: {aggregatedPieces.Count} unique parts");
⋮----
await _logger.LogErrorAsync($"Error calculating component explosion: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<string>> GenerateLabelCsvAsync(int shipmentId)
⋮----
var authResult = await _authService.CanGenerateLabelsAsync();
⋮----
await _logger.LogInfoAsync($"Generating label CSV for shipment {shipmentId}");
var shipmentResult = await _shipmentDao.GetByIdAsync(shipmentId);
⋮----
var linesResult = await _lineDao.GetByShipmentIdAsync(shipmentId);
⋮----
string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string csvDirectory = Path.Combine(appDataPath, "MTM_Receiving_Application", "Volvo", "Labels");
Directory.CreateDirectory(csvDirectory);
⋮----
string filePath = Path.Combine(csvDirectory, fileName);
var csvContent = new StringBuilder();
csvContent.AppendLine("Material,Quantity,Employee,Date,Time,Receiver,Notes");
string dateFormatted = shipment.ShipmentDate.ToString("MM/dd/yyyy");
string timeFormatted = DateTime.Now.ToString("HH:mm:ss");
foreach (var kvp in aggregatedPieces.OrderBy(x => x.Key))
⋮----
csvContent.AppendLine($"{kvp.Key},{kvp.Value},{shipment.EmployeeNumber},{dateFormatted},{timeFormatted},,");
⋮----
await File.WriteAllTextAsync(filePath, csvContent.ToString());
await _logger.LogInfoAsync($"Label CSV generated: {filePath}");
⋮----
await _logger.LogErrorAsync($"Error generating label CSV: {ex.Message}", ex);
⋮----
public async Task<string> FormatEmailTextAsync(
⋮----
var emailText = new StringBuilder();
emailText.AppendLine($"Subject: PO Requisition - Volvo Dunnage - {shipment.ShipmentDate:MM/dd/yyyy} Shipment #{shipment.ShipmentNumber}");
emailText.AppendLine();
emailText.AppendLine("Good morning,");
⋮----
emailText.AppendLine($"Please create a PO for the following Volvo dunnage received on {shipment.ShipmentDate:MM/dd/yyyy}:");
⋮----
var discrepancies = lines.Where(l => l.HasDiscrepancy).ToList();
⋮----
emailText.AppendLine("**DISCREPANCIES NOTED**");
⋮----
emailText.AppendLine("Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote");
emailText.AppendLine(new string('-', 80));
⋮----
string diffStr = difference > 0 ? $"+{difference}" : difference.ToString();
emailText.AppendLine($"{line.PartNumber}\t{expectedPieces}\t{receivedPieces}\t{diffStr}\t{line.DiscrepancyNote ?? ""}");
⋮----
emailText.AppendLine("Requested Lines:");
⋮----
emailText.AppendLine("Part Number\tQuantity (pcs)");
emailText.AppendLine(new string('-', 40));
foreach (var kvp in requestedLines.OrderBy(x => x.Key))
⋮----
emailText.AppendLine($"{kvp.Key}\t{kvp.Value}");
⋮----
if (!string.IsNullOrWhiteSpace(shipment.Notes))
⋮----
emailText.AppendLine("Additional Notes:");
emailText.AppendLine(shipment.Notes);
⋮----
await _logger.LogInfoAsync("Email text formatted");
return emailText.ToString();
⋮----
public async Task<Model_VolvoEmailData> FormatEmailDataAsync(
⋮----
var emailData = new Model_VolvoEmailData
⋮----
AdditionalNotes = string.IsNullOrWhiteSpace(shipment.Notes) ? null : shipment.Notes,
⋮----
emailData.Discrepancies.Add(new Model_VolvoEmailData.DiscrepancyLineItem
⋮----
await _logger.LogInfoAsync("Email data formatted");
⋮----
public string FormatEmailAsHtml(Model_VolvoEmailData emailData)
⋮----
var html = new StringBuilder();
html.AppendLine("<html>");
html.AppendLine("<body style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");
html.AppendLine($"<p>{emailData.Greeting}</p>");
html.AppendLine($"<p>{emailData.Message}</p>");
⋮----
html.AppendLine("<p><strong>**DISCREPANCIES NOTED**</strong></p>");
html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
html.AppendLine("<thead>");
html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
html.AppendLine("<th>Part Number</th>");
html.AppendLine("<th>Packlist Qty</th>");
html.AppendLine("<th>Received Qty</th>");
html.AppendLine("<th>Difference</th>");
html.AppendLine("<th>Note</th>");
html.AppendLine("</tr>");
html.AppendLine("</thead>");
html.AppendLine("<tbody>");
⋮----
string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
html.AppendLine("<tr>");
html.AppendLine($"<td>{disc.PartNumber}</td>");
html.AppendLine($"<td>{disc.PacklistQty}</td>");
html.AppendLine($"<td>{disc.ReceivedQty}</td>");
html.AppendLine($"<td>{diffStr}</td>");
html.AppendLine($"<td>{disc.Note}</td>");
⋮----
html.AppendLine("</tbody>");
html.AppendLine("</table>");
html.AppendLine("<br/>");
⋮----
html.AppendLine("<p><strong>Requested Lines:</strong></p>");
⋮----
html.AppendLine("<th>Quantity (pcs)</th>");
⋮----
foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
⋮----
html.AppendLine($"<td>{kvp.Key}</td>");
html.AppendLine($"<td>{kvp.Value}</td>");
⋮----
if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
⋮----
html.AppendLine("<p><strong>Additional Notes:</strong></p>");
html.AppendLine($"<p>{emailData.AdditionalNotes}</p>");
⋮----
html.AppendLine($"<p>{emailData.Signature.Replace("\\n", "<br/>")}</p>");
html.AppendLine("</body>");
html.AppendLine("</html>");
return html.ToString();
⋮----
/// <summary>
/// Validates shipment data before save
/// Centralized validation logic for data integrity
/// </summary>
/// <param name="shipment"></param>
public async Task<Model_Dao_Result> ValidateShipmentAsync(
⋮----
return Model_Dao_Result_Factory.Failure("At least one part line is required");
⋮----
if (string.IsNullOrWhiteSpace(line.PartNumber))
⋮----
return Model_Dao_Result_Factory.Failure("All parts must have a part number");
⋮----
return Model_Dao_Result_Factory.Failure(
⋮----
if (shipment.ShipmentDate > DateTime.Now.AddDays(1))
⋮----
return Model_Dao_Result_Factory.Failure("Shipment date cannot be in the future");
⋮----
await _logger.LogInfoAsync("Shipment validation passed");
return Model_Dao_Result_Factory.Success();
⋮----
public async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentAsync(
⋮----
var authResult = await _authService.CanManageShipmentsAsync();
⋮----
await _logger.LogInfoAsync("Saving Volvo shipment as pending PO");
var existingPendingResult = await _shipmentDao.GetPendingAsync();
⋮----
await _logger.LogInfoAsync($"Deleting existing pending shipment #{existingPendingResult.Data.ShipmentNumber}");
var deleteResult = await _shipmentDao.DeleteAsync(existingPendingResult.Data.Id);
⋮----
var insertResult = await _shipmentDao.InsertAsync(shipment);
⋮----
await _logger.LogInfoAsync($"Starting line insertion transaction for shipment {shipmentId}, {lines.Count} lines to insert");
await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();
⋮----
await _logger.LogInfoAsync($"Inserting line {lineIndex}/{lines.Count}: " +
⋮----
$"Note={(!string.IsNullOrEmpty(line.DiscrepancyNote) ? "PROVIDED" : "NULL")}");
⋮----
await _logger.LogInfoAsync($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}"))}");
var lineResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
⋮----
await transaction.RollbackAsync();
await _logger.LogErrorAsync($"Failed to insert line {lineIndex} for part {line.PartNumber}");
await _logger.LogErrorAsync($"Error: {lineResult.ErrorMessage}");
⋮----
await _logger.LogErrorAsync($"Exception Type: {lineResult.Exception.GetType().Name}");
await _logger.LogErrorAsync($"Exception Message: {lineResult.Exception.Message}");
⋮----
await _logger.LogErrorAsync($"Inner Exception: {lineResult.Exception.InnerException.Message}");
⋮----
await _logger.LogInfoAsync($"Line {lineIndex} inserted successfully");
⋮----
await transaction.CommitAsync();
await _logger.LogInfoAsync($"Transaction committed: Shipment {shipmentId} saved with {lines.Count} lines");
⋮----
await _logger.LogErrorAsync($"Transaction failed, rolled back: {ex.Message}", ex);
⋮----
await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<Model_VolvoShipment?>> GetPendingShipmentAsync()
⋮----
var result = await _shipmentDao.GetPendingAsync();
⋮----
public async Task<Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>> GetPendingShipmentWithLinesAsync()
⋮----
var shipmentResult = await _shipmentDao.GetPendingAsync();
⋮----
var linesResult = await _lineDao.GetByShipmentIdAsync(shipmentResult.Data.Id);
⋮----
await _logger.LogErrorAsync($"Error getting pending shipment with lines: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result> CompleteShipmentAsync(int shipmentId, string poNumber, string receiverNumber)
⋮----
var authResult = await _authService.CanCompleteShipmentsAsync();
⋮----
return new Model_Dao_Result
⋮----
await _logger.LogInfoAsync($"Completing shipment {shipmentId} with PO={poNumber}, Receiver={receiverNumber}");
if (string.IsNullOrWhiteSpace(poNumber))
⋮----
if (string.IsNullOrWhiteSpace(receiverNumber))
⋮----
var result = await _shipmentDao.CompleteAsync(shipmentId, poNumber, receiverNumber);
⋮----
await _logger.LogInfoAsync($"Shipment {shipmentId} completed successfully");
⋮----
await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetActivePartsAsync()
⋮----
return await _partDao.GetAllAsync(includeInactive: false);
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipment>>> GetHistoryAsync(
⋮----
return await _shipmentDao.GetHistoryAsync(startDate, endDate, status);
⋮----
public async Task<Model_Dao_Result<List<Model_VolvoShipmentLine>>> GetShipmentLinesAsync(int shipmentId)
⋮----
return await _lineDao.GetByShipmentIdAsync(shipmentId);
⋮----
public async Task<Model_Dao_Result> UpdateShipmentAsync(
⋮----
var shipmentResult = await _shipmentDao.UpdateAsync(shipment);
⋮----
var existingLines = await _lineDao.GetByShipmentIdAsync(shipment.Id);
⋮----
await _lineDao.DeleteAsync(line.Id);
⋮----
var lineResult = await _lineDao.InsertAsync(line);
⋮----
await _logger.LogErrorAsync(
⋮----
if (shipment.Status == "completed" && !string.IsNullOrEmpty(shipment.PONumber))
⋮----
return Model_Dao_Result_Factory.Success("Shipment updated successfully");
⋮----
return Model_Dao_Result_Factory.Failure($"Error updating shipment: {ex.Message}");
⋮----
public async Task<Model_Dao_Result<string>> ExportHistoryToCsvAsync(
⋮----
var csv = new StringBuilder();
csv.AppendLine("ShipmentNumber,Date,PONumber,ReceiverNumber,Status,EmployeeNumber,Notes");
⋮----
csv.AppendLine($"{shipment.ShipmentNumber}," +
⋮----
return Model_Dao_Result_Factory.Success(csv.ToString());
⋮----
private string EscapeCsv(string? value)
⋮----
if (string.IsNullOrEmpty(value))
⋮----
if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
⋮----
return $"\"{value.Replace("\"", "\"\"")}\"";
```

## File: Module_Volvo/ViewModels/ViewModel_Volvo_ShipmentEntry.cs
```csharp
public partial class ViewModel_Volvo_ShipmentEntry : ViewModel_Shared_Base
⋮----
private readonly IMediator _mediator;
private readonly IService_Window _windowService;
private readonly IService_UserSessionManager _sessionManager;
⋮----
_mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
⋮----
public async Task InitializeAsync()
⋮----
var initialDataQuery = new GetInitialShipmentDataQuery();
var initialDataResult = await _mediator.Send(initialDataQuery);
⋮----
await _logger.LogInfoAsync($"Initialized with shipment number: {ShipmentNumber}");
⋮----
await _errorHandler.HandleErrorAsync(
⋮----
var pendingQuery = new GetPendingShipmentQuery();
var pendingResult = await _mediator.Send(pendingQuery);
⋮----
await _logger.LogErrorAsync($"Error initializing Volvo shipment entry: {ex.Message}", ex);
⋮----
private async Task LoadPendingShipmentAsync(int shipmentId)
⋮----
var detailQuery = new GetShipmentDetailQuery { ShipmentId = shipmentId };
var detailResult = await _mediator.Send(detailQuery);
⋮----
ShipmentDate = new DateTimeOffset(shipment.ShipmentDate);
⋮----
Parts.Clear();
⋮----
Parts.Add(line);
⋮----
await _logger.LogInfoAsync($"Loaded pending shipment #{shipment.ShipmentNumber} with {Parts.Count} parts");
⋮----
await _logger.LogErrorAsync($"Error loading pending shipment: {ex.Message}", ex);
⋮----
private async Task LoadAllPartsAsync()
⋮----
var partsResult = await _mediator.Send(new GetAllVolvoPartsQuery
⋮----
await _logger.LogErrorAsync($"Error loading Volvo parts cache: {ex.Message}", ex);
⋮----
private void ApplyCachedQuantitiesToLines()
⋮----
var partsByNumber = _allParts.ToDictionary(p => p.PartNumber, StringComparer.OrdinalIgnoreCase);
⋮----
!string.IsNullOrWhiteSpace(line.PartNumber) &&
partsByNumber.TryGetValue(line.PartNumber, out var part))
⋮----
public async Task LoadAllPartsForDialogAsync()
⋮----
AvailableParts.Clear();
foreach (var part in partsResult.Data.OrderBy(p => p.PartNumber))
⋮----
AvailableParts.Add(part);
⋮----
await _logger.LogErrorAsync($"Error loading parts for dialog: {ex.Message}", ex);
⋮----
public async void UpdatePartSuggestions(string queryText)
⋮----
if (string.IsNullOrWhiteSpace(queryText))
⋮----
SuggestedParts.Clear();
⋮----
var searchQuery = new SearchVolvoPartsQuery
⋮----
var searchResult = await _mediator.Send(searchQuery);
⋮----
SuggestedParts.Add(part);
⋮----
var exactMatch = searchResult.Data.FirstOrDefault(p =>
p.PartNumber.Equals(queryText, StringComparison.OrdinalIgnoreCase));
⋮----
await _logger.LogErrorAsync($"Error searching parts: {ex.Message}", ex);
⋮----
public void OnPartSuggestionChosen(Model_VolvoPart? chosenPart)
⋮----
partial void OnPartSearchTextChanged(string value)
⋮----
private async void AddPart()
⋮----
if (SelectedPartToAdd == null || string.IsNullOrWhiteSpace(ReceivedSkidsToAdd))
⋮----
if (!int.TryParse(ReceivedSkidsToAdd, out int skidCount) || skidCount < 1 || skidCount > 99)
⋮----
if (Parts.Any(p => p.PartNumber.Equals(SelectedPartToAdd.PartNumber, StringComparison.OrdinalIgnoreCase)))
⋮----
var addCommand = new AddPartToShipmentCommand
⋮----
var validationResult = await _mediator.Send(addCommand);
⋮----
var newLine = new Model_VolvoShipmentLine
⋮----
Parts.Add(newLine);
await _logger.LogInfoAsync($"User added part {SelectedPartToAdd.PartNumber}, {skidCount} skids ({calculatedPieces} pcs)");
⋮----
await _logger.LogErrorAsync($"Error adding part: {ex.Message}", ex);
⋮----
private bool CanAddPart()
⋮----
!string.IsNullOrWhiteSpace(ReceivedSkidsToAdd) &&
int.TryParse(ReceivedSkidsToAdd, out int count) &&
⋮----
private async void RemovePart()
⋮----
var removeCommand = new RemovePartFromShipmentCommand
⋮----
var validationResult = await _mediator.Send(removeCommand);
⋮----
await _logger.LogInfoAsync($"User removed part {SelectedPart.PartNumber} from shipment");
Parts.Remove(SelectedPart);
⋮----
await _logger.LogErrorAsync($"Error removing part: {ex.Message}", ex);
⋮----
private async Task GenerateLabelsAsync()
⋮----
var pendingQuery = new GetPendingShipmentQuery
⋮----
var csvQuery = new GenerateLabelCsvQuery
⋮----
var csvResult = await _mediator.Send(csvQuery);
⋮----
await _logger.LogInfoAsync($"Labels generated for shipment ID: {pendingResult.Data.Id}, File: {csvResult.Data}");
⋮----
await _logger.LogErrorAsync($"Error generating labels: {ex.Message}", ex);
⋮----
private async Task PreviewEmailAsync()
⋮----
var pendingResult = await _mediator.Send(new GetPendingShipmentQuery
⋮----
var emailResult = await _mediator.Send(new FormatEmailDataQuery
⋮----
await _logger.LogErrorAsync($"Error previewing email: {ex.Message}", ex);
⋮----
private async Task ShowEmailPreviewDialogAsync(Model_VolvoEmailData emailData)
⋮----
var xamlRoot = _windowService.GetXamlRoot();
⋮----
var settingsDao = new Data.Dao_VolvoSettings(Module_Core.Helpers.Database.Helper_Database_Variables.GetConnectionString());
var toResult = await settingsDao.GetSettingAsync("email_to_recipients");
var ccResult = await settingsDao.GetSettingAsync("email_cc_recipients");
⋮----
var dialog = new ContentDialog
⋮----
var mainStack = new StackPanel { Spacing = 12, Margin = new Microsoft.UI.Xaml.Thickness(0) };
var toPanel = new Grid { ColumnSpacing = 8 };
toPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
toPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
var toBox = new TextBox
⋮----
toBox.SetValue(Grid.ColumnProperty, 0);
var toCopyButton = new Button
⋮----
toCopyButton.SetValue(Grid.ColumnProperty, 1);
Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(toCopyButton, "Copy To Recipients");
⋮----
var dataPackage = new DataPackage();
dataPackage.SetText(toBox.Text);
Clipboard.SetContent(dataPackage);
⋮----
toPanel.Children.Add(toBox);
toPanel.Children.Add(toCopyButton);
mainStack.Children.Add(toPanel);
var ccPanel = new Grid { ColumnSpacing = 8 };
ccPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
ccPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
var ccBox = new TextBox
⋮----
ccBox.SetValue(Grid.ColumnProperty, 0);
var ccCopyButton = new Button
⋮----
ccCopyButton.SetValue(Grid.ColumnProperty, 1);
Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(ccCopyButton, "Copy CC Recipients");
⋮----
dataPackage.SetText(ccBox.Text);
⋮----
ccPanel.Children.Add(ccBox);
ccPanel.Children.Add(ccCopyButton);
mainStack.Children.Add(ccPanel);
var subjectPanel = new Grid { ColumnSpacing = 8 };
subjectPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = new Microsoft.UI.Xaml.GridLength(1, Microsoft.UI.Xaml.GridUnitType.Star) });
subjectPanel.ColumnDefinitions.Add(new Microsoft.UI.Xaml.Controls.ColumnDefinition { Width = Microsoft.UI.Xaml.GridLength.Auto });
var subjectBox = new TextBox
⋮----
subjectBox.SetValue(Grid.ColumnProperty, 0);
var subjectCopyButton = new Button
⋮----
subjectCopyButton.SetValue(Grid.ColumnProperty, 1);
Microsoft.UI.Xaml.Controls.ToolTipService.SetToolTip(subjectCopyButton, "Copy Subject");
⋮----
dataPackage.SetText(subjectBox.Text);
⋮----
subjectPanel.Children.Add(subjectBox);
subjectPanel.Children.Add(subjectCopyButton);
mainStack.Children.Add(subjectPanel);
⋮----
var discHeader = new TextBlock
⋮----
mainStack.Children.Add(discHeader);
var discBox = new TextBox
⋮----
var discText = new StringBuilder();
discText.AppendLine("Part Number\tPacklist Qty (pcs)\tReceived Qty (pcs)\tDifference (pcs)\tNote");
discText.AppendLine(new string('-', 80));
⋮----
string diffStr = disc.Difference > 0 ? $"+{disc.Difference}" : disc.Difference.ToString();
discText.AppendLine($"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}");
⋮----
discBox.Text = discText.ToString();
mainStack.Children.Add(discBox);
⋮----
var reqHeader = new TextBlock
⋮----
mainStack.Children.Add(reqHeader);
var reqBox = new TextBox
⋮----
var reqText = new StringBuilder();
reqText.AppendLine("Part Number\tQuantity (pcs)");
reqText.AppendLine(new string('-', 40));
foreach (var kvp in emailData.RequestedLines.OrderBy(x => x.Key))
⋮----
reqText.AppendLine($"{kvp.Key}\t{kvp.Value}");
⋮----
reqBox.Text = reqText.ToString();
mainStack.Children.Add(reqBox);
if (!string.IsNullOrWhiteSpace(emailData.AdditionalNotes))
⋮----
var notesBox = new TextBox
⋮----
mainStack.Children.Add(notesBox);
⋮----
var scrollViewer = new ScrollViewer
⋮----
var result = await dialog.ShowAsync();
⋮----
dataPackage.SetHtmlFormat(htmlContent);
⋮----
dataPackage.SetText(plainText);
⋮----
private string BuildPlainTextEmail(Model_VolvoEmailData emailData)
⋮----
var text = new StringBuilder();
text.AppendLine($"Subject: {emailData.Subject}");
text.AppendLine();
text.AppendLine(emailData.Greeting);
⋮----
text.AppendLine(emailData.Message);
⋮----
text.AppendLine("**DISCREPANCIES NOTED**");
⋮----
text.AppendLine("Part Number\tPacklist Qty\tReceived Qty\tDifference\tNote");
text.AppendLine(new string('-', 80));
⋮----
text.AppendLine($"{disc.PartNumber}\t{disc.PacklistQty}\t{disc.ReceivedQty}\t{diffStr}\t{disc.Note}");
⋮----
text.AppendLine("Requested Lines:");
⋮----
text.AppendLine("Part Number\tQuantity (pcs)");
text.AppendLine(new string('-', 40));
⋮----
text.AppendLine($"{kvp.Key}\t{kvp.Value}");
⋮----
text.AppendLine("Additional Notes:");
text.AppendLine(emailData.AdditionalNotes);
⋮----
text.AppendLine(emailData.Signature);
return text.ToString();
⋮----
private static string FormatEmailAsHtml(Model_VolvoEmailData emailData)
⋮----
var html = new StringBuilder();
html.AppendLine("<html>");
html.AppendLine("<body style='font-family: Calibri, Arial, sans-serif; font-size: 11pt;'>");
html.AppendLine($"<p>{emailData.Greeting}</p>");
html.AppendLine($"<p>{emailData.Message}</p>");
⋮----
html.AppendLine("<p><strong>**DISCREPANCIES NOTED**</strong></p>");
html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; font-size: 10pt;'>");
html.AppendLine("<thead>");
html.AppendLine("<tr style='background-color: #D9D9D9; font-weight: bold;'>");
html.AppendLine("<th>Part Number</th>");
html.AppendLine("<th>Packlist Qty</th>");
html.AppendLine("<th>Received Qty</th>");
html.AppendLine("<th>Difference</th>");
html.AppendLine("<th>Note</th>");
html.AppendLine("</tr>");
html.AppendLine("</thead>");
html.AppendLine("<tbody>");
⋮----
html.AppendLine("<tr>");
html.AppendLine($"<td>{disc.PartNumber}</td>");
html.AppendLine($"<td>{disc.PacklistQty}</td>");
html.AppendLine($"<td>{disc.ReceivedQty}</td>");
html.AppendLine($"<td>{diffStr}</td>");
html.AppendLine($"<td>{disc.Note}</td>");
⋮----
html.AppendLine("</tbody>");
html.AppendLine("</table>");
html.AppendLine("<br/>");
⋮----
html.AppendLine("<p><strong>Requested Lines:</strong></p>");
⋮----
html.AppendLine("<th>Quantity (pcs)</th>");
⋮----
html.AppendLine($"<td>{kvp.Key}</td>");
html.AppendLine($"<td>{kvp.Value}</td>");
⋮----
html.AppendLine("<p><strong>Additional Notes:</strong></p>");
html.AppendLine($"<p>{emailData.AdditionalNotes}</p>");
⋮----
html.AppendLine($"<p>{emailData.Signature.Replace("\\n", "<br/>")}</p>");
html.AppendLine("</body>");
html.AppendLine("</html>");
return html.ToString();
⋮----
private void ViewHistory()
⋮----
_logger.LogInfo("Navigating to Volvo Shipment History");
⋮----
var contentFrame = mainWindow.GetContentFrame();
⋮----
private async Task SaveAsPendingAsync()
⋮----
var partsDto = Parts.Select(p => new ShipmentLineDto
⋮----
}).ToList();
var saveCommand = new SavePendingShipmentCommand
⋮----
var result = await _mediator.Send(saveCommand);
⋮----
await _logger.LogInfoAsync($"Shipment #{ShipmentNumber} saved as pending (ID: {result.Data})");
⋮----
await _logger.LogErrorAsync($"Error saving shipment: {ex.Message}", ex);
⋮----
private async Task<Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>> SaveShipmentInternalAsync()
⋮----
return new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
⋮----
private async Task CompleteShipmentAsync()
⋮----
await _logger.LogErrorAsync($"Error completing shipment: {ex.Message}", ex);
⋮----
private async Task ShowCompletionDialogAsync(Model_VolvoShipment shipment)
⋮----
var stackPanel = new StackPanel { Spacing = 12 };
var poTextBox = new TextBox
⋮----
var receiverTextBox = new TextBox
⋮----
stackPanel.Children.Add(poTextBox);
stackPanel.Children.Add(receiverTextBox);
⋮----
if (string.IsNullOrWhiteSpace(poTextBox.Text) || string.IsNullOrWhiteSpace(receiverTextBox.Text))
⋮----
var completeCommand = new CompleteShipmentCommand
⋮----
PONumber = poTextBox.Text.Trim(),
ReceiverNumber = receiverTextBox.Text.Trim(),
⋮----
var completeResult = await _mediator.Send(completeCommand);
⋮----
await _logger.LogInfoAsync($"Shipment #{ShipmentNumber} completed with PO: {poTextBox.Text.Trim()}");
⋮----
private async Task ToggleDiscrepancyAsync(Model_VolvoShipmentLine? line)
⋮----
var confirmDialog = new ContentDialog
⋮----
var confirmResult = await confirmDialog.ShowAsync();
⋮----
var expectedSkidsBox = new NumberBox
⋮----
var noteBox = new TextBox
⋮----
var panel = new StackPanel { Spacing = 12 };
panel.Children.Add(expectedSkidsBox);
panel.Children.Add(noteBox);
⋮----
line.DiscrepancyNote = string.IsNullOrWhiteSpace(noteBox.Text) ? null : noteBox.Text.Trim();
⋮----
private void StartNewEntry()
⋮----
private bool ValidateShipment()
⋮----
_errorHandler.HandleErrorAsync(
⋮----
true).ConfigureAwait(false);
⋮----
if (string.IsNullOrWhiteSpace(part.PartNumber))
⋮----
private string FormatRecipientsFromJson(string? jsonValue, string fallbackValue)
⋮----
if (string.IsNullOrWhiteSpace(jsonValue))
⋮----
return string.Join("; ", recipients.Select(r => r.ToOutlookFormat()));
⋮----
_logger.LogErrorAsync($"Error parsing email recipients JSON: {ex.Message}", ex).ConfigureAwait(false);
⋮----
private void ValidateSaveEligibility()
⋮----
CanSave = Parts.Count > 0 && Parts.All(p => !string.IsNullOrWhiteSpace(p.PartNumber) && p.ReceivedSkidCount > 0);
⋮----
partial void OnPartsChanged(ObservableCollection<Model_VolvoShipmentLine> value)
⋮----
partial void OnSelectedPartToAddChanged(Model_VolvoPart? value)
⋮----
AddPartCommand.NotifyCanExecuteChanged();
⋮----
partial void OnReceivedSkidsToAddChanged(string value)
⋮----
private void ClearShipmentForm()
```
