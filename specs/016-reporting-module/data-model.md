# Data Model: Reporting Module

**Feature**: Reporting Module  
**Database**: `mtm_receiving_application` (MySQL 8.x)  
**Compatibility**: MySQL 5.7.24+ (no JSON functions, CTEs, window functions, CHECK constraints)

## Overview

The Reporting module does not create its own tables. Instead, it uses views that aggregate data from Receiving, Dunnage, Routing, and Volvo modules. These views provide a unified interface for generating end-of-day reports.

## Views

### vw_receiving_history

**Purpose**: Flattened view of receiving loads for reporting

```sql
CREATE OR REPLACE VIEW vw_receiving_history AS
SELECT 
  load_id as id,
  po_number,
  part_id as part_number,
  part_description,
  quantity_received as quantity,
  weight as weight_lbs,
  CONCAT_WS('/', heat_number, lot_number) as heat_lot_number,
  DATE(created_at) as created_date,
  'Receiving' as source_module
FROM receiving_loads
ORDER BY created_at DESC;
```

### vw_dunnage_history

**Purpose**: Flattened view of dunnage loads for reporting

```sql
CREATE OR REPLACE VIEW vw_dunnage_history AS
SELECT 
  dl.load_id as id,
  dt.type_name as dunnage_type,
  dp.part_number as part_number,
  GROUP_CONCAT(CONCAT(ds.spec_key, ':', ds.spec_value) ORDER BY ds.display_order SEPARATOR ', ') as specs_combined,
  dl.quantity,
  DATE(dl.created_at) as created_date,
  'Dunnage' as source_module
FROM dunnage_loads dl
INNER JOIN dunnage_types dt ON dl.type_id = dt.type_id
INNER JOIN dunnage_parts dp ON dl.part_id = dp.part_id
LEFT JOIN dunnage_specs ds ON dp.part_id = ds.part_id
GROUP BY dl.load_id
ORDER BY dl.created_at DESC;
```

### vw_routing_history

**Purpose**: Flattened view of routing labels for reporting

```sql
CREATE OR REPLACE VIEW vw_routing_history AS
SELECT 
  id,
  deliver_to,
  department,
  package_description,
  po_number,
  work_order as work_order_number,
  employee_number,
  created_date,
  'Routing' as source_module
FROM routing_labels
WHERE is_archived = 1
ORDER BY created_date DESC, label_number ASC;
```

### vw_volvo_history

**Purpose**: Flattened view of Volvo shipments for reporting

```sql
CREATE OR REPLACE VIEW vw_volvo_history AS
SELECT 
  vs.id,
  vs.shipment_number,
  vs.po_number,
  vs.receiver_number,
  vs.status,
  vs.shipment_date as created_date,
  COUNT(vsl.id) as part_count,
  vs.employee_number,
  'Volvo' as source_module
FROM volvo_shipments vs
LEFT JOIN volvo_shipment_lines vsl ON vs.id = vsl.shipment_id
GROUP BY vs.id
ORDER BY vs.shipment_date DESC;
```

## Data Models (C#)

### Model_ReportRow

**Purpose**: Unified data structure for report rows across all modules

```csharp
public class Model_ReportRow
{
    public int Id { get; set; }
    public string? PONumber { get; set; }
    public string? PartNumber { get; set; }
    public string? PartDescription { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? WeightLbs { get; set; }
    public string? HeatLotNumber { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SourceModule { get; set; } = string.Empty;
    
    // Routing-specific fields
    public string? DeliverTo { get; set; }
    public string? Department { get; set; }
    public string? PackageDescription { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? EmployeeNumber { get; set; }
    
    // Dunnage-specific fields
    public string? DunnageType { get; set; }
    public string? SpecsCombined { get; set; }
    
    // Volvo-specific fields
    public int? ShipmentNumber { get; set; }
    public string? ReceiverNumber { get; set; }
    public string? Status { get; set; }
    public int? PartCount { get; set; }
}
```

## PO Number Normalization Algorithm

**Purpose**: Normalize PO numbers to standard format (matches EndOfDayEmail.js logic)

```csharp
public string NormalizePONumber(string? poNumber)
{
    if (string.IsNullOrWhiteSpace(poNumber))
        return "No PO";
    
    poNumber = poNumber.Trim();
    
    // Pass through non-numeric values (e.g., "Customer Supplied")
    if (!poNumber.All(char.IsDigit))
        return poNumber;
    
    // Validate length (must be 5-6 digits)
    if (poNumber.Length < 5)
        return "Validate PO";
    
    // Pad to 6 digits if needed
    if (poNumber.Length == 5)
        poNumber = "0" + poNumber;
    
    // Format as PO-XXXXXX
    return "PO-" + poNumber;
}
```

## CSV Export Format

**Purpose**: CSV structure matching MiniUPSLabel.csv

```csv
PO Number,Part,Description,Qty,Weight,Heat/Lot,Date
PO-063150,12345,Steel Plate,100.00,2500.00,HT123/LT456,2026-01-03
PO-063151,67890,Aluminum Bar,50.00,1250.00,HT789/LT012,2026-01-03
```

## Email Format

**Purpose**: HTML table with alternating row colors grouped by date

```html
<table>
  <tr style="background-color: #f0f0f0;">
    <th>PO Number</th>
    <th>Part</th>
    <th>Description</th>
    <th>Qty</th>
    <th>Weight</th>
    <th>Heat/Lot</th>
    <th>Date</th>
  </tr>
  <tr style="background-color: #ffffff;">
    <td>PO-063150</td>
    <td>12345</td>
    <td>Steel Plate</td>
    <td>100.00</td>
    <td>2500.00</td>
    <td>HT123/LT456</td>
    <td>2026-01-03</td>
  </tr>
  <tr style="background-color: #f9f9f9;">
    <td>PO-063151</td>
    <td>67890</td>
    <td>Aluminum Bar</td>
    <td>50.00</td>
    <td>1250.00</td>
    <td>HT789/LT012</td>
    <td>2026-01-03</td>
  </tr>
</table>
```

## Migration Strategy

1. **Create views** in this order (depends on other module tables):
   - `vw_receiving_history` (depends on receiving_loads)
   - `vw_dunnage_history` (depends on dunnage_loads, dunnage_types, dunnage_parts, dunnage_specs)
   - `vw_routing_history` (depends on routing_labels)
   - `vw_volvo_history` (depends on volvo_shipments, volvo_shipment_lines)

2. **Test queries**: Verify views return correct data for each module

3. **Validate PO normalization**: Test with various PO number formats

---

**Reference**: See [../011-module-reimplementation/data-model.md](../011-module-reimplementation/data-model.md) for complete data model context

