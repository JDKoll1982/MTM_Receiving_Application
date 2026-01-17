# Volvo Shipment History Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start]) --> Open[Open Shipment History page]
    Open --> LoadRecent["Load recent shipments (last 30 days)"]
    LoadRecent --> SelectFilters[Select start date, end date, status]
    SelectFilters --> Filter[Click Filter]

    Filter --> Selected{Select a shipment?}
    Selected -- Yes --> ViewDetails[Click View Details]
    ViewDetails --> ShowDialog[Show details dialog]
    Selected -- No --> EditSelected{Edit selected shipment?}
    ShowDialog --> EditSelected

    EditSelected -- Yes --> OpenEdit[Open Edit dialog]
    OpenEdit --> UpdateData[Update header/lines]
    UpdateData --> SaveChanges[Save changes]
    SaveChanges --> Refresh[Refresh history]
    EditSelected -- No --> ExportCsv{Export CSV?}
    Refresh --> ExportCsv

    ExportCsv -- Yes --> Export[Export shipment history to CSV]
    ExportCsv -- No --> Back{Back to shipment entry?}
    Export --> Back

    Back -- Yes --> GoBack[Back to shipment entry]
    Back -- No --> End([End])
    GoBack --> End
```

## User-Friendly Steps

1. Open Volvo Shipment History to see recent shipments.
2. Set your date range and status filter, then click Filter.
3. Select a shipment to view details.
4. Click View Details to see header and line items.
5. Click Edit to change shipment data, then save.
6. Click Export CSV to download the filtered list.
7. Use Back to return to the entry screen.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| Load recent | OnPageLoaded → LoadRecentShipmentsAsync | GetRecentShipmentsQuery | n/a | ViewModel: ViewModel_Volvo_History.LoadRecentShipmentsAsync | Default start date = now - 30 days |
| Filter | Filter button | GetShipmentHistoryQuery | n/a | ViewModel: FilterAsync | Status options: All, Pending PO, Completed |
| View details | View Details button | GetShipmentDetailQuery | n/a | ViewModel: ViewDetailAsync | Shows PO, Receiver, Status, Notes, and line counts |
| Edit shipment | Edit button | UpdateShipmentCommand | ShipmentId > 0; ShipmentDate ≤ now; Parts not empty; Notes ≤ 1000; PONumber ≤ 50; ReceiverNumber ≤ 50; each part: PartNumber required, ReceivedSkidCount > 0 | Validator: UpdateShipmentCommandValidator; ViewModel: EditAsync | Uses VolvoShipmentEditDialog; updates lines via UpdateShipmentCommand |
| Export | Export CSV button | ExportShipmentsQuery | n/a | ViewModel: ExportAsync | Uses StartDate, EndDate, StatusFilter |
