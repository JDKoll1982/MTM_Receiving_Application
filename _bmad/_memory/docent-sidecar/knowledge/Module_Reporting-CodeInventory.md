---
module_name: Module_Reporting
component: code-inventory
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Reporting - Code Inventory

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Reporting.md](../docent-sidecar/knowledge/Module_Reporting.md)

## Views

| View | DataContext | Notes |
|------|------------|-------|
| View_Reporting_Main | ViewModel_Reporting_Main | Single UI surface for date range, module selection, report list, export/copy commands. |

## ViewModels

| ViewModel | Responsibilities |
|----------|------------------|
| ViewModel_Reporting_Main | Date range selection; module availability and enablement; per-module report generation; export and email copy commands; holds `ReportData`. |

## Services

| Service | Responsibilities |
|--------|------------------|
| Service_Reporting | Fetch per-module history; normalize PO numbers (Receiving/Routing); CSV export; HTML email formatting. |

## DAOs

| DAO | Responsibilities |
|-----|------------------|
| Dao_Reporting | Read-only SELECT queries against MySQL reporting views; counts for availability. |

## Shared/Core Types Used

- `Model_ReportRow` (Module_Core/Models/Reporting)
- `IService_Reporting` (Module_Core/Contracts/Services)
- `Model_Dao_Result` + factory (Module_Core/Models/Core)
