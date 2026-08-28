<!-- 
[DOC-META-START]
- File Name: customer-pull-pack-crystal-bindings.md
- Description: Crystal report binding notes for the Customer Pull n' Pack report.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-18: # Customer Pull n' Pack Crystal Bindings
- Critical Notes: None
[DOC-META-END]
-->

# Customer Pull n' Pack Crystal Bindings

- Customer Pull n' Pack crystal report control is fed by `ViewModel_Tool_CustomerPullPackReport.CrystalReportGroups`, not placeholder groups.
- `ApplyCrystalRequestLineSelection` updates `DemandLines[].IsSelected`, refreshes snapshot state, and drives waitlist creation selection.
- `ApplyCrystalLocationSelection` applies selection by `ParentPartId` + `LocationId` across matching `DemandLines[].LocationOptions`.
- `View_CustomerPullPack_CrystalReportLines` raises UI-only selection events back to the report page; the view model owns actual selection state.
- `QuantitySelected` in crystal groups is derived from selected lines' `QuantityToPack` total within each parent-part group.
