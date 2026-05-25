# Data Model: Customer Pull n' Pack Tool

Date: 2026-05-25
Branch: 001-customer-pull-pack

## Entities

### CustomerDemandFilter
Purpose: Captures the active report query context for Workflow 1.1 and 1.2.

Fields:
- customerId: string, required
- customerName: string, required
- dateFrom: date, required
- dateTo: date, required
- shortagesOnly: bool, required
- unpulledOnly: bool, required
- sortMode: enum(PullDate, ShortageFirst, Part), required
- searchText: string, optional

Validation:
- customerId must map to a valid customer from the full customer list.
- dateFrom must be less than or equal to dateTo.

### DemandLine
Purpose: Represents one actionable customer-order line shown in the report.

Fields:
- sourceLineKey: string, required, unique within source scope
- customerId: string, required
- customerOrderId: string, required
- parentPartId: string, required
- sourceLocationId: string, optional
- shipQuantity: decimal, required
- pullDate: date, required
- quantityToPack: decimal, required
- fgOnHandQuantity: decimal, required
- fgLocationId: string, optional
- shortageFlag: bool, required
- lateOrderFlag: bool, required
- requesterNote: string, optional
- hasLinkedWaitlist: bool, required

Relationships:
- One DemandLine may have zero or one open WaitlistEntry.
- One DemandLine may expose zero to many LocationOption rows.

### LocationOption
Purpose: Represents one selectable location row on the report, sourced from SUB PARTS ON HAND or a fallback FG location when appropriate.

Fields:
- locationKey: string, required, unique within parent part context
- parentPartId: string, required
- locationId: string, required
- displayLabel: string, required
- onHandQuantity: decimal, required
- sourceType: enum(SubPartOnHand, FinishedGoodsFallback), required
- selected: bool, required

Validation:
- New request flows must initialize all selected flags to false.
- Update flows may initialize selected based on existing WaitlistEntry.selectedLocations.

### ReportSelectionContext
Purpose: Holds report-side state for Workflow 2.1, 2.2, and 2.3.

Fields:
- selectedSourceLineKeys: collection<string>, required
- selectedLocationKeys: collection<string>, required
- lockedCustomerId: string, derived
- lockedParentPartId: string, derived
- duplicateConflictSourceLineKeys: collection<string>, optional

Validation:
- All selected source lines must share the same customerId and parentPartId.
- selectedLocationKeys may be empty only when no selectable locations exist or when the user is still reviewing options before save.
- The report surface must communicate this state with row-level selection highlights on both request lines and `SUB PARTS ON HAND` rows.

### CrystalReportGroupProjection
Purpose: Temporary presentation model for the embedded crystal-style report surface shown on the main report page.

Fields:
- parentPartDisplay: string, derived from one grouped parent part, required
- quantityToPack: decimal, required
- quantitySelected: decimal, required
- fgLocationId: string, optional
- fgOnHandQuantity: decimal, required
- shortageFlag: bool, required
- lateOrderFlag: bool, required
- serviceNote: string, optional
- requestLines: collection<CrystalRequestLineProjection>, required
- subPartLocations: collection<CrystalSubPartLocationProjection>, required

Validation:
- This projection is a presentation layer only; it must be derived from live `DemandLine`, `LocationOption`, and waitlist-link data before the feature is considered complete.
- Hardcoded sample groups are acceptable only for temporary UI review and must not remain in the production-bound implementation.

### CrystalRequestLineProjection
Purpose: One row in the embedded crystal-style request-line grid.

Fields:
- customerOrderId: string, required
- parentPartId: string, required
- locationId: string, optional
- customerLabel: string, required
- shipQuantity: decimal, required
- pullDateDisplay: string, required
- isSelected: bool, required
- statusBadgeText: string, optional

Validation:
- `isSelected` must ultimately reflect `ReportSelectionContext.selectedSourceLineKeys`, not a control-local placeholder flag.

### CrystalSubPartLocationProjection
Purpose: One selectable `SUB PARTS ON HAND` row in the embedded crystal-style report.

Fields:
- partLocationId: string, required
- onHandQuantity: decimal, required
- isSelected: bool, required

Validation:
- `isSelected` must ultimately reflect `ReportSelectionContext.selectedLocationKeys`, not a control-local placeholder flag.

### WaitlistEntry
Purpose: MTM-managed operational record linked to one source demand line.

Fields:
- waitlistId: string, required
- sourceLineKey: string, required, unique for open items
- customerId: string, required
- customerOrderId: string, required
- parentPartId: string, required
- requestedQuantity: decimal, required
- selectedLocations: collection<string>, required but may be empty for exception path
- requestedByUserId: string, required
- requesterContextNote: string, optional
- currentStatus: enum(Requested, Accepted, Completed, Cancelled, Problem), required
- currentOwnerUserId: string, optional
- currentOwnerDisplayName: string, optional
- locationReviewFlag: bool, required
- problemReason: enum(NotInLocation, IncorrectQty, IncorrectPartNumber, Other), optional
- handlerNote: string, optional
- completionUserId: string, optional
- completionTimestamp: datetime, optional
- lastUpdatedByUserId: string, required
- lastUpdatedTimestamp: datetime, required
- requestTimestamp: datetime, required
- recheckIndicator: bool, required

Validation:
- At most one open WaitlistEntry may exist per sourceLineKey.
- locationReviewFlag must be true when selectedLocations is empty because no selectable locations exist.
- problemReason or handlerNote is required when currentStatus = Problem.
- currentOwnerUserId is assigned on Accepted and retained on Problem until explicit unassign or handoff.

State transitions:
- Requested -> Accepted, Cancelled, Problem
- Accepted -> Completed, Cancelled, Problem, Requested only through explicit unassign/handoff policy
- Problem -> Accepted, Cancelled, Completed, remains owned until explicit unassign/handoff
- Completed is terminal for queue work; later source changes set recheckIndicator instead of reopening automatically

### WaitlistQueueFilter
Purpose: Controls Workflow 3.1 queue visibility.

Fields:
- statusSet: collection<WaitlistStatus>, required
- customerId: string, optional
- locationId: string, optional
- requesterSearchText: string, optional
- ownerUserId: string, optional

Validation:
- Default statusSet = Requested + Accepted + Problem.

### ProblemReasonSelection
Purpose: Captures Problem workflow validation state.

Fields:
- selectedPreset: enum(NotInLocation, IncorrectQty, IncorrectPartNumber), optional
- freeformNote: string, optional

Validation:
- selectedPreset or freeformNote must be present before saving Problem.

### UserWorkingDefaults
Purpose: Persists Workflow 4.1 startup state.

Fields:
- userId: string, required
- defaultCustomerId: string, optional
- favoriteCustomerIds: collection<string>, optional
- lastGoodDateRangeType: string, required
- lastGoodDateFrom: date, optional
- lastGoodDateTo: date, optional
- defaultSortMode: string, required
- defaultShortagesOnly: bool, required
- defaultUnpulledOnly: bool, required
- defaultWaitlistStatusSet: collection<WaitlistStatus>, required
- defaultPrintPreset: string, required

### PrintContext
Purpose: Derived read model for floor copy and pull-list output.

Fields:
- printMode: enum(CurrentView, FloorCopy, PullList, ShortageOnly, WaitlistOnly), required
- customerId: string, required
- activeFiltersSummary: string, required
- generatedAt: datetime, required
- demandLines: collection<DemandLine>, optional
- waitlistEntries: collection<WaitlistEntry>, optional
- groupedSubPartTotals: collection<SubPartPrintGroup>, optional

### SubPartPrintGroup
Purpose: Aggregates pull-list output by unique sub-part.

Fields:
- subPartId: string, required
- totalQuantityNeeded: decimal, required
- totalQuantityOnHand: decimal, required
- chosenLocations: collection<string>, required

## Relationships
- CustomerDemandFilter produces a collection of DemandLine records.
- DemandLine exposes a collection of LocationOption records on the report surface.
- ReportSelectionContext references DemandLine.sourceLineKey and LocationOption.locationKey.
- WaitlistEntry links back to exactly one DemandLine.sourceLineKey.
- WaitlistQueueFilter filters WaitlistEntry collections.
- PrintContext is derived from DemandLine and WaitlistEntry collections.
- CrystalReportGroupProjection is a presentation projection over grouped DemandLine and LocationOption data with linked waitlist overlay information.

## Binding Replacement Map

- `CrystalReportGroupProjection.parentPartDisplay` derives from grouped `DemandLine.parentPartId`.
- `CrystalReportGroupProjection.quantityToPack` derives from the grouped demand quantity-to-pack total.
- `CrystalReportGroupProjection.quantitySelected` derives from the quantity represented by the currently selected report lines inside the same grouped parent-part context.
- `CrystalReportGroupProjection.fgLocationId` and `fgOnHandQuantity` derive from the grouped finished-goods context returned with each demand line.
- `CrystalReportGroupProjection.requestLines` is a projection of the grouped visible `DemandLine` rows plus linked waitlist status overlay.
- `CrystalReportGroupProjection.subPartLocations` is a projection of the available `LocationOption` rows for the same parent-part context.
- `CrystalRequestLineProjection.isSelected` and `CrystalSubPartLocationProjection.isSelected` must bind to the same selection state that the waitlist creation flow uses, not to placeholder preview-only flags.

## Uniqueness And Conflict Rules
- Open waitlist uniqueness key: sourceLineKey + open-status membership.
- Duplicate request attempt returns the existing open WaitlistEntry instead of creating a second record.
- Batch save uniqueness is evaluated per selected sourceLineKey, not per batch request.

## Workflow Coverage
- Workflow 1.1/1.2: CustomerDemandFilter, DemandLine
- Workflow 2.1/2.2/2.3: LocationOption, ReportSelectionContext, WaitlistEntry
- Workflow 3.1/3.2: WaitlistQueueFilter, WaitlistEntry, ProblemReasonSelection
- Workflow 4.1: UserWorkingDefaults, PrintContext, SubPartPrintGroup
