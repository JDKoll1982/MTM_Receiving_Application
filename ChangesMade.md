Update the Material Availability Board in `Module_ShipRec_Tools`.

Current state and important context:

- The board already stretches the outer card correctly and already has a working `Print` button that generates reporting-style HTML.
- Work orders are already formatted in Infor Visual entry style like `WO-070016`.
- The current header layout still looks visually cramped and inconsistent, as shown in the attached screenshots.
- The current search flow shows the same success/status message twice: once in the app header area and once inside the page `InfoBar`. This duplicate notification should be removed so the user only sees the page-level status message.
- The current associated-part data does not yet expose enough detail for the desired “Required Parts” and “Estimated Coil Use” summary. If needed, extend the SQL / DAO / model pipeline for `20_GetMaterialAvailabilityAssociatedPartRuns.sql` and the related C# mapping.
- From the Infor Visual schema research, `REQUIREMENT` has relevant quantity-related fields including `QTY_PER`, `FIXED_QTY`, `CALC_QTY`, `ISSUED_QTY`, `ALLOCATED_QTY`, `FULFILLED_QTY`, and `USAGE_UM`. Use the best available field(s) for a defensible summary instead of inventing a fake value.
- Additional schema review across `docs/InforVisual/DatabaseCSVFiles` indicates the work-order detail modal can be grounded in real data from `WORK_ORDER`, `REQUIREMENT`, `OPERATION`, `PART`, `VENDOR`, and related joins. `REQUIREMENT` is keyed by the work-order identity plus `OPERATION_SEQ_NO` and `PIECE_NO`, and `OPERATION` is keyed by the work-order identity plus `SEQUENCE_NO`.
- The Infor Visual guide reinforces that requirement quantities and operation progress are meaningful shop-floor concepts for material issue and backflush behavior, so requirement quantity fields are valid inputs for the receiving/inventory summary.

Use the attached screenshots as guidance for the current problems:

- The main summary/header area feels too flat and uneven.
- The quantity block on the right is visually separated but the rest of the content is not grouped as clearly.
- The “Incoming Material” section needs a richer details experience instead of only showing the rollup.
- The duplicate notification is visible in the screenshot where the result count appears both at the top of the app and again inside the page.

Implement the following UI/behavior changes:

1. Improve the top card summary layout so it feels balanced and visually grouped.
   - Increase padding and spacing so the content looks aligned and intentional.
   - Put each summary area into its own small card-like surface.
   - Do this without noticeably increasing the height of the main collapsed card.
   - Keep the collapsed card compact.

2. Update the description presentation.
   - Change it from plain text to something like: `Description: {Description}`.
   - Make the description text slightly smaller than it is now.
   - Keep it readable in both dark and light themes.

3. Improve the location summary text.
   - Replace `4 location(s) with qty > 0` with smarter grammar.
   - Use wording along the lines of:
     - `1 location found - Click to view it`
     - `4 locations found - Click to view them`
   - The exact wording does not have to match this exactly, but it should be natural English.

4. Replace the current next-run summary with a richer single summary card.
   - Keep this in one small card-like section, not split into multiple mini-cards.
   - Use wording along the lines of:
     - `Next Run: {WorkOrder} / {Part Number} on {Date} | Required Parts: {Required Parts} | Estimated Coil Use: {EstimatedCoilUse}`
   - The exact wording does not need to be identical, but the summary must clearly include:
     - Work order
     - parent/associated part number
     - date
     - required parts quantity
     - estimated coil use
   - If “Estimated Coil Use” cannot be calculated exactly from existing fields, derive the best reasonable value from the requirement data and label it consistently.
   - Do not hardcode a fake number.
   - If the estimated coil use is greater than what is currently on hand, make that mismatch obvious in the UI.
   - The warning should be visible in the summary experience without forcing the user to expand the full card.

5. Give each card-like summary section its own distinct visual color treatment.
   - Do not use hardcoded colors.
   - Use theme-aware brushes / theme resources so the UI looks correct in both dark mode and light mode.
   - The sections should feel distinct from each other while still fitting the app’s existing visual language.

6. Improve the Incoming Material experience.
   - Keep the current rollup summary in the board.
   - Add a new stylized modal dialog/window for viewing incoming-material details.
   - The modal should show the matching incoming material lines with at least:
     - PO number
     - date
     - vendor
     - qty received
     - qty ordered
     - remaining qty
   - This modal should feel visually polished and match the rest of the application.
   - Add a clear way for the user to open this modal from the Incoming Material section.
   - Add a `Print` action to this incoming-material modal using the same general HTML print approach already used earlier for the board, but do not force a datagrid if a card/repeater-based printable layout is a better fit.

7. Add a work-order details modal reachable from the work-order display.
   - Add a `Click here for more info` link or equivalent action on the work-order number shown in the next-run summary area.
   - Clicking it should open a stylized modal with fuller job/work-order detail that is useful from a receiving / inventory perspective.
   - This modal should use categorized card sections rather than feeling like a raw dump of fields.
   - Add a `Print` action to this work-order modal using the same general HTML print approach already used earlier for the board.
   - If a datagrid does not fit the information well, use another layout such as grouped cards, definition rows, or repeaters.

8. Add a settings page in `Module_Core` for field visibility and print control.
   - Add a new settings page and a navigation card in the existing Module Core settings navigation page.
   - Use this page to toggle on / off which of the selected work-order detail fields are shown in the UI.
   - Use the same settings page to control which of those selected fields are included in print output.
   - These settings should apply to both the work-order details modal UI and its printable output.
   - The settings experience should be clear enough that a user can decide what is visible versus what is only used in background logic.

9. Remove the duplicate success notification behavior.
   - When the user searches, do not show the same result count/status in both the header-level notification area and the page `InfoBar`.
   - Keep only the page-level status messaging for this feature unless there is a strong reason otherwise.

Work-order modal field menu to support with real Infor Visual data:

Use this as a selectable scope list for the work-order details modal. Keep the modal focused on receiving / inventory usefulness, not every possible manufacturing field. These options should be presented in categorized card sections in the eventual UI.

| Include | Dont Show, but use in logic | Card Category              | Candidate field / detail                                                                                                              | Likely source table(s)                                          | Why it matters for receiving / inventory                                               |
| ------- | --------------------------- | -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| [x]     | [ ]                         | Job identity               | Work order display ID (`TYPE/BASE/LOT/SPLIT/SUB` formatted for UI)                                                                    | `WORK_ORDER`                                                    | Primary reference the user can match against the shop/job context                      |
| [x]     | [ ]                         | Job identity               | Work order status                                                                                                                     | `WORK_ORDER`                                                    | Helps determine whether the job is released, active, complete, or otherwise actionable |
| [x]     | [ ]                         | Job identity               | Parent / associated part number                                                                                                       | `WORK_ORDER`, `PART`                                            | Identifies the assembly or parent job that is consuming the material                   |
| [x]     | [ ]                         | Job identity               | Parent / associated part description                                                                                                  | `WORK_ORDER`, `PART`                                            | Gives the receiver context without needing to decode the part ID                       |
| [ ]     | [ ]                         | Job identity               | Site ID                                                                                                                               | `WORK_ORDER`, `OPERATION`                                       | Useful when jobs may span or be scheduled by site                                      |
| [x]     | [ ]                         | Scheduling                 | Next due-to-run date                                                                                                                  | existing `WORK_ORDER` / `REQUIREMENT` date logic                | Core date the board is already surfacing                                               |
| [x]     | [ ]                         | Scheduling                 | Next due-date source                                                                                                                  | existing `REQUIREMENT` / `WORK_ORDER` date logic                | Shows whether the date came from requirement demand, due date, or operation schedule   |
| [x]     | [ ]                         | Scheduling                 | Required date                                                                                                                         | `REQUIREMENT.REQUIRED_DATE`                                     | Tells receiving when the component is needed                                           |
| [ ]     | [ ]                         | Scheduling                 | Requirement / material status                                                                                                         | `REQUIREMENT.STATUS`                                            | Helps show whether the component requirement is still open or otherwise constrained    |
| [x]     | [ ]                         | Scheduling                 | Work order status effective date                                                                                                      | `WORK_ORDER` if available in query scope                        | Gives timing context for current job state                                             |
| [x]     | [ ]                         | Requirement consumption    | Component / input part number                                                                                                         | `REQUIREMENT.PART_ID`                                           | Confirms which raw material or coil the job is consuming                               |
| [ ]     | [ ]                         | Requirement consumption    | Component / input part description                                                                                                    | `PART`, existing input-part joins                               | More readable component context                                                        |
| [x]     | [ ]                         | Requirement consumption    | Operation sequence                                                                                                                    | `REQUIREMENT.OPERATION_SEQ_NO`, `OPERATION.SEQUENCE_NO`         | Shows which operation consumes the material                                            |
| [ ]     | [ ]                         | Requirement consumption    | Requirement piece number                                                                                                              | `REQUIREMENT.PIECE_NO`                                          | Useful when multiple requirement lines exist on the same operation                     |
| [x]     | [x]                         | Requirement consumption    | Qty per - use with other below quantity fields to get estimated usage.                                                                | `REQUIREMENT.QTY_PER`                                           | One of the strongest candidates for estimated usage                                    |
| [x]     | [x]                         | Requirement consumption    | Fixed qty                                                                                                                             | `REQUIREMENT.FIXED_QTY`                                         | Needed when usage is not purely variable                                               |
| [x]     | [x]                         | Requirement consumption    | Calculated qty                                                                                                                        | `REQUIREMENT.CALC_QTY`                                          | Best available precomputed quantity if populated                                       |
| [x]     | [x]                         | Requirement consumption    | Issued qty                                                                                                                            | `REQUIREMENT.ISSUED_QTY`                                        | Shows how much has already been consumed/issued                                        |
| [x]     | [x]                         | Requirement consumption    | Allocated qty                                                                                                                         | `REQUIREMENT.ALLOCATED_QTY`                                     | Shows how much inventory is already reserved to the job                                |
| [x]     | [x]                         | Requirement consumption    | Fulfilled qty                                                                                                                         | `REQUIREMENT.FULFILLED_QTY`                                     | Helps compare requested vs satisfied demand                                            |
| [x]     | [x]                         | Requirement consumption    | Usage unit of measure - Normalize this (EA = Each, LBS = Pounds, excedra... use with the estimate (ex: 25 {NormalizedValue} per Part) | `REQUIREMENT.USAGE_UM`                                          | Required so the quantity is understandable to receivers                                |
| [x]     | [x]                         | Requirement consumption    | Scrap percent                                                                                                                         | `REQUIREMENT.SCRAP_PERCENT`                                     | Important if estimated coil use should include scrap burden                            |
| [ ]     | [ ]                         | Requirement consumption    | Required-for-setup flag                                                                                                               | `REQUIREMENT.REQUIRED_FOR_SETUP`                                | Useful signal that the material is needed before run completion begins                 |
| [x]     | [ ]                         | Operation context          | Operation type                                                                                                                        | `OPERATION.OPERATION_TYPE`                                      | Helps users understand what kind of job step this is                                   |
| [x]     | [ ]                         | Operation context          | Resource / work center                                                                                                                | `OPERATION.RESOURCE_ID`                                         | Useful when coordinating with production or locating the job step                      |
| [x]     | [ ]                         | Operation context          | Service ID / outside process                                                                                                          | `OPERATION.SERVICE_ID`                                          | Helps identify outsourced or service-based steps                                       |
| [x]     | [x]                         | Operation context          | Operation warehouse                                                                                                                   | `OPERATION.WAREHOUSE_ID`                                        | Useful if material movement or staging is tied to a warehouse                          |
| [ ]     | [ ]                         | Operation context          | Load size qty                                                                                                                         | `OPERATION.LOAD_SIZE_QTY`                                       | Potentially useful when interpreting batch-oriented usage                              |
| [x]     | [x]                         | Operation context          | Run qty per cycle                                                                                                                     | `OPERATION.RUN_QTY_PER_CYCLE`                                   | Useful if estimated usage should be framed per cycle or batch                          |
| [x]     | [ ]                         | Operation context          | Setup hours / run hours                                                                                                               | `OPERATION.SETUP_HRS`, `OPERATION.RUN_HRS`                      | Secondary context for job timing and urgency                                           |
| [ ]     | [ ]                         | Vendor / sourcing          | Requirement vendor ID                                                                                                                 | `REQUIREMENT.VENDOR_ID`, `VENDOR`                               | Useful when the material or process is vendor-linked                                   |
| [ ]     | [ ]                         | Vendor / sourcing          | Requirement vendor part ID                                                                                                            | `REQUIREMENT.VENDOR_PART_ID`                                    | Helps cross-reference supplier paperwork                                               |
| [ ]     | [ ]                         | Vendor / sourcing          | Manufacturer name / part ID                                                                                                           | `REQUIREMENT.MFG_NAME`, `REQUIREMENT.MFG_PART_ID`               | Useful for traceability and receiving verification                                     |
| [x]     | [ ]                         | Dimensional / traceability | Dimensions text / expression                                                                                                          | `REQUIREMENT.DIMENSIONS`, `REQUIREMENT.DIM_EXPRESSION`          | Potentially useful for coil or cut-size interpretation                                 |
| [x]     | [ ]                         | Dimensional / traceability | Length / width / height                                                                                                               | `REQUIREMENT.LENGTH`, `REQUIREMENT.WIDTH`, `REQUIREMENT.HEIGHT` | Useful if dimensional context matters to receiving checks                              |
| [x]     | [ ]                         | Dimensional / traceability | Drawing ID / revision                                                                                                                 | `REQUIREMENT.DRAWING_ID`, `REQUIREMENT.DRAWING_REV_NO`          | Helpful when receiving against engineering-controlled material                         |

Expected selection behavior for the modal scope table:

- Treat the table above as a planning checklist for which work-order details should be included in the modal.
- Only surface fields that can be tied cleanly to the selected work order and requirement context.
- Prefer fields that are immediately helpful to receivers, material handlers, or inventory staff over general ERP metadata.
- The eventual modal UI should group selected fields into card sections such as `Job Summary`, `Requirement / Coil Use`, `Operation Context`, `Vendor / Sourcing`, and `Traceability / Dimensions`.
- Fields marked `Include` should be treated as the initial default UI / print scope.
- Fields marked `Dont Show, but use in logic` should be available to support calculations, warnings, normalization, and summary generation even when they are not displayed directly.
- Fields marked `Dont Show, but use in logic` should remain available for calculations even when hidden by default, and should be revealable through a small `Show all` chip in the modal that toggles those fields on / off in the UI unless that behavior is disabled in settings.
- The new Module Core settings page should allow these defaults to be overridden without code changes.

Implementation guidance:

- Follow the project’s MVVM architecture strictly.
- Keep business logic out of code-behind.
- Prefer minimal, surgical changes to the existing Material Availability Board flow.
- If needed, extend the associated-part SQL query and its C# model/DAO mapping to support required quantity and estimated coil usage.
- If needed, extend the incoming-material detail models so the modal can show vendor and PO line detail cleanly.
- If needed, add a second query / modal data pipeline for richer work-order details, but keep it read-only and scoped to the selected job.
- Persist the new field-visibility and print-selection settings in the existing application settings patterns rather than inventing a one-off storage mechanism.
- Keep Infor Visual access read-only.

Files likely involved:

- `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml`
- `Module_ShipRec_Tools/Views/View_Tool_MaterialAvailabilityBoard.xaml.cs`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/Services/Service_Tool_MaterialAvailabilityBoard.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityCard.cs`
- `Module_ShipRec_Tools/Models/Model_Tool_MaterialAvailabilityAssociatedPartRun.cs`
- `Module_Core/Models/InforVisual/Model_InforVisualAssociatedPartRunRow.cs`
- `Module_Core/Data/InforVisual/Dao_InforVisualConnection.cs`
- `Database/InforVisualScripts/Queries/20_GetMaterialAvailabilityAssociatedPartRuns.sql`
- `Module_Core` settings page, viewmodel, model, and related settings navigation page/card wiring
- any new query, DAO model, or modal-specific model needed for work-order detail
- any new dialog/modal viewmodel/view files needed for incoming material detail

Acceptance criteria:

- The collapsed main card looks visually balanced and grouped.
- The summary areas look like distinct small cards/sections.
- The collapsed card is not noticeably taller than before.
- The description is labeled and slightly smaller.
- The location summary uses correct singular/plural grammar and invites the user to expand.
- The next-run summary is richer and includes work order, part number, date, required parts, and estimated coil use.
- If estimated coil use exceeds on-hand inventory, the UI makes that risk obvious.
- The summary surfaces use theme-aware colors that look correct in both dark and light mode.
- The Incoming Material section can open a polished modal with PO/date/vendor/received/ordered/remaining detail.
- The Incoming Material modal includes a `Print` action.
- The work-order link opens a polished details modal with categorized card sections and a `Print` action.
- The work-order details modal includes a small `Show all` chip that can reveal or hide logic-only fields unless that behavior has been disabled in settings.
- `ChangesMade.md` includes a checkbox table so the final work-order modal field set can be selected intentionally.
- A new Module Core settings page exists to control which selected work-order fields appear in the UI and in print output.
- The Module Core settings navigation includes a card / entry point for that new settings page.
- Searching no longer creates duplicate notifications.
- Any new data used for required-parts / coil-use is sourced from real Infor Visual fields and not guessed.

## Work-Order Modal Field Selection Checklist

Use this table as the implementation review checklist for the default Material Availability work-order modal and print scope.

| Default UI | Default Print | Logic-Only Reveal | Card Category             | Field                                |
| ---------- | ------------- | ----------------- | ------------------------- | ------------------------------------ |
| [x]        | [x]           | [ ]               | Job Summary               | Work order display                   |
| [x]        | [x]           | [ ]               | Job Summary               | Work order status                    |
| [x]        | [x]           | [ ]               | Job Summary               | Parent / associated part number      |
| [x]        | [x]           | [ ]               | Job Summary               | Parent / associated part description |
| [x]        | [x]           | [ ]               | Scheduling                | Next due-to-run date                 |
| [x]        | [x]           | [ ]               | Scheduling                | Next due-date source                 |
| [x]        | [x]           | [ ]               | Scheduling                | Required date                        |
| [x]        | [x]           | [ ]               | Scheduling                | Work order status effective date     |
| [x]        | [x]           | [ ]               | Requirement / Coil Use    | Component / input part number        |
| [x]        | [x]           | [ ]               | Requirement / Coil Use    | Operation sequence                   |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Qty per                              |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Fixed qty                            |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Calculated qty                       |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Issued qty                           |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Allocated qty                        |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Fulfilled qty                        |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Usage unit of measure                |
| [ ]        | [ ]           | [x]               | Requirement / Coil Use    | Scrap percent                        |
| [x]        | [x]           | [ ]               | Operation Context         | Operation type                       |
| [x]        | [x]           | [ ]               | Operation Context         | Resource / work center               |
| [x]        | [x]           | [ ]               | Operation Context         | Service ID / outside process         |
| [ ]        | [ ]           | [x]               | Operation Context         | Operation warehouse                  |
| [ ]        | [ ]           | [x]               | Operation Context         | Run qty per cycle                    |
| [x]        | [x]           | [ ]               | Operation Context         | Setup hours / run hours              |
| [x]        | [x]           | [ ]               | Traceability / Dimensions | Dimensions text / expression         |
| [x]        | [x]           | [ ]               | Traceability / Dimensions | Length / width / height              |
| [x]        | [x]           | [ ]               | Traceability / Dimensions | Drawing ID / revision                |
