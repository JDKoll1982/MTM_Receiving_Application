# 01-FeatureChange-CustomerPullPack-MockDataAssetsAndParity

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this feature slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Customer Pull n' Pack mock-data asset split and parity hardening

## Implementation Order

01

## Why This Runs First

The current Customer Pull n' Pack mock rows live inside the shared Infor Visual mock catalog.
Creating dedicated module-owned mock assets first reduces later conflicts when the service split
and screen wiring stop depending on the shared catalog files.

## Confirmed Decisions

- Customer Pull n' Pack mock data must move into a module-owned folder under `Module_ShipRec_Tools`.
- The split must use dedicated static and dedicated runtime files.
- Mock parity must match both the UI-facing feature contract and the raw field shape expected from the live path.
- Do not leave Customer Pull n' Pack mock data as an ad hoc subset inside the shared Infor Visual catalog.

## Current Repo State

- Shared mock catalog service: `Module_Core/Services/Database/Service_InforVisualMockDataCatalog.cs`
- Shared catalog model: `Module_Core/Models/InforVisual/Model_InforVisualMockDataCatalog.cs`
- Shared JSON files:
  - `Module_Settings.Core/Defaults/inforvisual-mock-data.json`
  - `Module_Settings.Core/Defaults/inforvisual.mock-runtime.json`
- Customer Pull n' Pack mock rows currently live in the shared catalog as:
  - `CustomerPullPackDemandRows`
  - `CustomerPullPackLocationRows`
- Existing row models already match the feature well enough to reuse:
  - `Module_Core/Models/InforVisual/Model_InforVisualCustomerPullPackDemandRow.cs`
  - `Module_Core/Models/InforVisual/Model_InforVisualCustomerPullPackLocationRow.cs`

## Required Outcome

Create a dedicated, module-owned Customer Pull n' Pack mock-data catalog and JSON asset pair so
later feature work can stop reading Customer Pull n' Pack rows from the shared Infor Visual mock
catalog files.

## Primary Change Areas

- Add a module-owned mock-data folder under `Module_ShipRec_Tools`.
- Add dedicated static and runtime JSON files for Customer Pull n' Pack mock data.
- Add a module-scoped mock-data catalog model and loader service for Customer Pull n' Pack.
- Preserve raw field parity with the current live and mock projection path.
- Do not rewire the full feature to the new service yet unless a small compatibility step is required.

## Files That Must Change

- `Module_Core/Models/InforVisual/Model_InforVisualMockDataCatalog.cs`
- `Module_Core/Services/Database/Service_InforVisualMockDataCatalog.cs`
- `Module_Settings.Core/Defaults/inforvisual-mock-data.json`
- `Module_Settings.Core/Defaults/inforvisual.mock-runtime.json`

## Files to Add

- Add a module-owned folder under `Module_ShipRec_Tools` for Customer Pull n' Pack mock assets.
- Add a dedicated static mock-data JSON file in that folder.
- Add a dedicated runtime mock-data JSON file in that folder.
- Add a module-scoped mock-data catalog model for Customer Pull n' Pack.
- Add a module-scoped mock-data catalog service contract and implementation.
- Add targeted unit tests for the new module-scoped mock-data catalog loader.

## Recommended Target Shape

- Reuse the existing demand-row and location-row model types unless a real parity gap forces a new row type.
- The new module-scoped catalog should contain only the Customer Pull n' Pack mock datasets needed by this workflow.
- The static JSON file should hold the default fixture set.
- The runtime JSON file should hold mutable or developer-edited overrides.
- The loader should merge static plus runtime content in the same spirit as the shared mock catalog service.

## Implementation Steps

1. Create a module-owned Customer Pull n' Pack mock-data folder under `Module_ShipRec_Tools`.
2. Create a dedicated static JSON file for Customer Pull n' Pack demand rows and location rows.
3. Create a dedicated runtime JSON file for Customer Pull n' Pack demand rows and location rows.
4. Add a module-scoped mock-data catalog model that expresses only the Customer Pull n' Pack datasets.
5. Add a module-scoped mock-data catalog service that:
   - loads the static file
   - loads the runtime file
   - normalizes rows the same way the shared mock catalog currently does
   - merges runtime rows over static rows using stable keys
6. Copy or move the existing Customer Pull n' Pack mock data out of the shared Infor Visual JSON files into the new module-owned files.
7. Remove the Customer Pull n' Pack mock-data sections from the shared JSON files once the module-owned files contain the migrated data.
8. Keep the shared mock catalog service backward compatible for unrelated workflows.
9. Do not complete the full feature-service rewiring in this slice. That happens in `02-FeatureChange-CustomerPullPack-DataSourceResolverAndServiceSplit.md`.

## Task Checklist

- [x] Create the module-owned Customer Pull n' Pack mock-data folder.
- [x] Add dedicated static and runtime JSON files for Customer Pull n' Pack.
- [x] Add a module-scoped mock-data catalog model.
- [x] Add a module-scoped mock-data catalog interface.
- [x] Add a module-scoped mock-data catalog service implementation.
- [x] Preserve demand-row and location-row raw-field parity with the current live/mock projection contract.
- [x] Migrate the existing Customer Pull n' Pack JSON data out of the shared Infor Visual mock JSON files.
- [x] Remove the migrated Customer Pull n' Pack sections from the shared JSON files.
- [x] Add unit tests covering static plus runtime merge behavior and row normalization.

## Validation

- Add a focused test file for the new module-scoped mock-data loader service.
- Verify the loader returns the same demand and location rows that the current shared catalog would return for Customer Pull n' Pack.
- Verify runtime rows override static rows by stable key.
- Verify the shared JSON files no longer carry Customer Pull n' Pack mock datasets.
- Run the narrow Customer Pull n' Pack unit tests if they still compile against the transitional compatibility state.

## Guardrails

- Do not change live Infor Visual SQL query behavior in this slice.
- Do not introduce the mock/live resolver in this slice.
- Do not modify unrelated shared Infor Visual mock datasets.
- Keep the new files ASCII and keep comments minimal.

## Completion Criteria

- Customer Pull n' Pack mock data is no longer stored inside the shared Infor Visual mock JSON files.
- The module owns its own static and runtime Customer Pull n' Pack mock assets.
- A dedicated module-scoped loader can return normalized Customer Pull n' Pack demand and location rows.
- Tests prove the new loader preserves parity with the prior shared-data behavior.
