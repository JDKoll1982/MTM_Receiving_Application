# Reporting Refactor Checklist

Last Updated: 2026-03-17

## Goal

Execute the reporting module refactor in controlled phases so the module becomes:

- data-contract correct
- Outlook clipboard friendly
- free of stale spreadsheet/export assumptions
- easier to maintain across Receiving, Dunnage, and Volvo workflows

## Phase 1: Cleanup And Stabilization

- [x] Remove stale spreadsheet/export wording from `Module_Reporting/README.md`
- [x] Remove stale Routing references from `Module_Reporting/README.md`
- [x] Remove stale Routing references from `Module_Reporting/SETTABLE_OBJECTS_REPORT.md`
- [x] Rename reporting UI text away from generic `Email Format` wording where appropriate
- [ ] Remove obsolete comments/notes from reporting module files that only restate obvious behavior
- [x] Confirm there is no active CSV/XLS/XLSX code path left in `Module_Reporting`

## Phase 2: Clipboard And Outlook Formatting

- [x] Extract clipboard packaging out of `ViewModel_Reporting_Main`
- [x] Add a dedicated reporting clipboard formatter/helper/service
- [x] Replace raw `SetHtmlFormat(...)` usage with `HtmlFormatHelper.CreateHtmlFormat(...)`
- [x] Replace raw HTML `SetText(...)` fallback with a true plain-text fallback
- [x] Build spreadsheet-like HTML tables with inline styles suitable for Outlook paste
- [ ] Test clipboard output in Outlook desktop
- [ ] Test clipboard output in New Outlook
- [ ] Test clipboard output in Word and plain-text targets

## Phase 3: Availability And UI Behavior

- [x] Fix Volvo availability counting in `Dao_Reporting.CheckAvailabilityAsync()`
- [ ] Ensure module enablement logic distinguishes `0 rows` from query failure
- [x] Refresh UI wording/status text to match the real reporting workflow
- [ ] Improve report presentation beyond the current minimal three-field list

## Phase 4: Volvo Reporting Path

- [x] Add Volvo-specific formatting in `Service_Reporting.FormatForEmailAsync()`
- [ ] Confirm whether Volvo reporting should remain shipment-summary only
- [ ] If required, define a line-detail reporting path for Volvo

## Phase 5: Receiving Data-Contract Repair

- [x] Align `Dao_Reporting.GetReceivingHistoryAsync()` with the live Receiving view contract
- [ ] Align `Database/Schemas/34_View_receiving_history.sql` with reporting needs
- [ ] Decide whether Receiving reporting should include weight
- [ ] Decide the correct date semantics for Receiving reporting
- [ ] Decide the correct `source module` semantics for Receiving reporting

## Phase 6: Dunnage Data Semantics

- [ ] Decide whether Dunnage should surface username, employee number, or both
- [ ] Rename or normalize the projected Dunnage employee field accordingly
- [ ] Validate spec concatenation output against current Dunnage expectations

## Phase 7: Shared Model And Architecture

- [ ] Decide whether `Model_ReportRow` remains viable as the shared report model
- [ ] If not, split reporting into per-module DTOs plus a presentation adapter
- [ ] Revisit `IService_Reporting` once the data contracts are stable
- [ ] Keep one reporting shell page while splitting module-specific pipelines internally

## Phase 8: Validation

- [ ] Build the solution after each phase
- [ ] Validate Receiving report generation against deployed views
- [ ] Validate Dunnage report generation against live data
- [ ] Validate Volvo report generation against live data
- [ ] Validate clipboard output in real user paste targets
- [ ] Update documentation after implementation stabilizes

## Initial Execution Order

1. Cleanup And Stabilization
2. Clipboard And Outlook Formatting
3. Availability And UI Behavior
4. Volvo Reporting Path
5. Receiving Data-Contract Repair
6. Dunnage Data Semantics
7. Shared Model And Architecture
8. Validation
