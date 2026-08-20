# Customer Pull n' Pack Crystal Bindings

- Customer Pull n' Pack crystal report control is fed by `ViewModel_Tool_CustomerPullPackReport.CrystalReportGroups`, not placeholder groups.
- `ApplyCrystalRequestLineSelection` updates `DemandLines[].IsSelected`, refreshes snapshot state, and drives waitlist creation selection.
- `ApplyCrystalLocationSelection` applies selection by `ParentPartId` + `LocationId` across matching `DemandLines[].LocationOptions`.
- `View_CustomerPullPack_CrystalReportLines` raises UI-only selection events back to the report page; the view model owns actual selection state.
- `QuantitySelected` in crystal groups is derived from selected lines' `QuantityToPack` total within each parent-part group.
