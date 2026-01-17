# Volvo Generate Labels Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start]) --> Click[Click Generate Labels]
    Click --> Pending{Pending shipment exists?}
    Pending -- No --> SavePending[Save or update pending shipment]
    Pending -- Yes --> GetPending[Get pending shipment]
    SavePending --> GetPending
    GetPending --> Found{Pending shipment found?}
    Found -- No --> Error[Show error: no pending shipment]
    Found -- Yes --> Generate[Generate label CSV]
    Generate --> ShowPath[Show file path / success message]
    Error --> End([End])
    ShowPath --> End
```

## User-Friendly Steps

1. Click Generate Labels.
2. If the shipment isn’t saved yet, it’s saved as pending first (or updated if one already exists).
3. The system generates a CSV file for LabelView.
4. A success message shows the file path.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| Generate Labels | Generate Labels button | GenerateLabelCsvQuery | n/a | ViewModel: ViewModel_Volvo_ShipmentEntry.GenerateLabelsAsync | If HasPendingShipment is false, SaveShipmentInternalAsync runs first (updates existing pending if found) |
| Fetch pending | Internal | GetPendingShipmentQuery | n/a | ViewModel: GenerateLabelsAsync | Uses Environment.UserName; requires pending shipment ID |
| Create CSV | Internal | GenerateLabelCsvQuery | n/a | Handler: GenerateLabelCsvQueryHandler | Success message includes CSV file path |
