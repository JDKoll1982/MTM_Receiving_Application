# Implementation Plan: Customer Pull n' Pack Tool

**Branch**: `[001-customer-pull-pack]` | **Date**: 2026-05-25 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-customer-pull-pack/spec.md`

## Summary

Implement Customer Pull n' Pack as a Module_ShipRec_Tools vertical slice that combines a read-only customer demand report, report-side location selection from SUB PARTS ON HAND rows, MTM-managed waitlist creation/update with duplicate prevention, a dedicated queue with explicit ownership rules, requester-facing recheck indicators for completed work that needs review, and print/default restoration flows. The implementation will follow the existing WinUI 3 + MediatR + stored-procedure architecture, keeping source demand read-only and persisting operational waitlist state in MTM-managed MySQL records while preserving post-acceptance requester note editing boundaries and the full print/defaults matrix defined in the specification.

## Current UI Alignment Snapshot

- The report page now uses a collapsed-by-default filter expander with the primary report actions visible in the expander header.
- The report body embeds a dedicated crystal-style user control to align the in-app surface to the legacy grouped report layout.
- The Ship/Rec host widens the main window while Customer Pull n' Pack is active and restores the standard shell size when the user leaves the tool.
- The embedded crystal-style user control currently contains temporary sample report groups for visual alignment only; completion still requires replacing those placeholders with live grouped report projections and live selection bindings.
- The reviewed selection UX is row-highlight driven for both request lines and `SUB PARTS ON HAND` rows; the remaining implementation work is to bind that visual selection state to the actual report-side waitlist selection context.

## Technical Context

**Language/Version**: C# 13 on .NET 10  
**Primary Dependencies**: WinUI 3, CommunityToolkit.Mvvm, MediatR, FluentValidation, Serilog, FluentAssertions, Bogus  
**Storage**: MySQL 5.7 for MTM-managed waitlist/default records; SQL Server/In﻿for Visual read-only demand and inventory context  
**Testing**: xUnit + FluentAssertions; handler/validator unit tests and DAO/integration coverage where persistence is involved  
**Target Platform**: Windows desktop (WinUI 3, Windows App SDK)  
**Project Type**: Modular desktop application within an existing WinUI monolith  
**Performance Goals**: Reach actionable demand or clear no-demand state in under 30 seconds; generate print-ready output within 60 seconds; avoid report-to-queue roundtrips for 95% of daily updates; capture those timings in the feature quickstart validation flow  
**Constraints**: x:Bind only; no business logic in code-behind; MediatR/CQRS-first; MySQL stored procedures only; Infor Visual read-only; one open waitlist item per source line; report-side location selection stays unselected for new requests until explicit click  
**Scale/Scope**: One Ship/Rec Tools feature spanning report, waitlist queue, requester recheck signaling, scoped requester-after-acceptance edits, print/default flows, 7 workflow blocks, and a new MTM-managed waitlist persistence model

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

Verify alignment with the constitution:

- MVVM purity (partial ViewModels, x:Bind only, no code-behind business logic).
- Data access integrity (MySQL stored procedures only; Infor Visual read-only; instance DAOs returning Model_Dao_Result; no static DAOs).
- CQRS/MediatR usage with pipeline behaviors (validation/logging/audit) and mediator-first ViewModels.
- DI registration in `Infrastructure/DependencyInjection/` extension methods, wired from `App.xaml.cs`, with module boundaries (Module_Core infra only; module-specific logic stays local).
- Validation/logging/error handling (FluentValidation + IService_ErrorHandler + Serilog structured logs; no exception leakage to UI).
- Security/session discipline (auth tiers, timeouts, auditability; no secrets in code).
- Library-first approach (use approved libraries before writing custom services: MediatR, FluentValidation, Serilog, Mapster, Ardalis.GuardClauses, FluentAssertions/Bogus).

Pre-Phase 0 status: PASS

- Report, queue, and print behaviors can be implemented with mediator requests/commands and x:Bind-driven ViewModels.
- No constitution waiver is needed for duplicate prevention, owner retention on Problem, or report-side location selection.
- Data boundaries remain compliant: Visual read-only, MTM waitlist state persisted separately.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Module_ShipRec_Tools/
├── Contracts/
├── Models/
├── Services/
├── ViewModels/
├── Views/
│   └── Controls/
├── Dialogs/
└── docs/

Infrastructure/
└── DependencyInjection/

Module_Core/
├── Helpers/
├── Services/
└── Models/

Database/
├── Scripts/
└── StoredProcedures/   # Feature persistence additions if this repo uses explicit SP deployment files

MTM_Receiving_Application.Tests/
├── Module_ShipRec_Tools/
├── Integration/
└── TestUtilities/
```

**Structure Decision**: Extend the existing Module_ShipRec_Tools module rather than introducing a new module. Keep feature-specific models, queries/commands, ViewModels, and Views local to Module_ShipRec_Tools; keep generic helpers, logging, and error handling in Module_Core/Infrastructure. Add tests under MTM_Receiving_Application.Tests aligned to handler, validation, and persistence seams.

## Remaining Binding Gap

- `View_CustomerPullPack_CrystalReportLines` must be fed from a live grouped projection derived from `DemandLine`, `LocationOption`, and linked waitlist state instead of the current temporary sample rows.
- The embedded crystal-style surface must drive real report-line and location selection state used by waitlist creation, duplicate detection, and selected-quantity summaries.
- The current UI alignment work is intentionally incremental: visual parity and host behavior are in place, but sign-off requires live bindings and production data replacing all preview-only placeholder values.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation                  | Why Needed         | Simpler Alternative Rejected Because |
| -------------------------- | ------------------ | ------------------------------------ |
| None | N/A | N/A |

## Phase 0 Output

- [research.md](./research.md)

## Phase 1 Output

- [data-model.md](./data-model.md)
- [quickstart.md](./quickstart.md)
- [contracts/openapi.yaml](./contracts/openapi.yaml)

## Post-Design Constitution Check

Post-Phase 1 status: PASS

- MVVM purity remains intact: report state, queue state, and print state are planned as ViewModel-bound state with no code-behind business logic.
- Data access boundaries remain compliant: Visual demand/inventory queries are read-only, MTM waitlist state is separate, and MySQL persistence is planned through stored procedures.
- CQRS is reinforced by the contract set: report query, linked waitlist query, batch upsert command, queue query, status update command, unassign command, and print read-model queries.
- Ownership, requester-after-acceptance edit boundaries, recheck-indicator behavior, and duplicate-prevention rules are handled as explicit command-side or query-side validation rather than ad hoc UI-only logic.
- Print and defaults design covers current view, floor copy, shortage-only view, waitlist-only view, pull list, selected part/customer-order contexts, and saved favorite-customer/filter presets.
- No constitutional violations or waivers are required for the current design.
