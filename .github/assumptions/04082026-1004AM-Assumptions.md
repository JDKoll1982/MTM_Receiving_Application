Last Updated: 2026-04-08

# Assumptions For Location And Part-Based Material Availability Board

1. Assumption: The feature should be implemented as a new read-only tool in `Module_ShipRec_Tools` and hosted through the existing Ship/Rec tool registry, main container, and tool-selection surface.
   Why this assumption is needed: The prompt strongly recommends `Module_ShipRec_Tools`, and the existing navigation and DI patterns there are the closest structural match.
   Potential impact if wrong: Building this in the wrong module would create avoidable UI duplication, different navigation behavior, and a likely mismatch with the intended user entry point.
   Alternative interpretations considered: Implement the board inside `Module_Receiving`; create a standalone top-level page outside the Ship/Rec tool host.

2. Assumption: Location searches should default to warehouse/site `002` with no warehouse selector in the first pass.
   Why this assumption is needed: Existing Infor Visual location validation and fuzzy-location search APIs already require a warehouse code, but the prompt does not explicitly define whether the board is single-warehouse or user-selectable.
   Potential impact if wrong: The query may omit valid stock from other warehouse scopes or display misleading results if users expect cross-warehouse coverage.
   Alternative interpretations considered: Add a warehouse selector now; search every warehouse/site by default; infer warehouse from the entered location only.

3. Assumption: "All locations with quantity greater than zero" means all current positive-quantity locations for the part within the same warehouse/site scope used by the search, not across all sites in Infor Visual.
   Why this assumption is needed: The prompt requires location rollup visibility, but it does not explicitly state whether that rollup crosses site boundaries.
   Potential impact if wrong: The board could under-report inventory if cross-site visibility is required, or over-report inventory if users only want same-warehouse operational locations.
   Alternative interpretations considered: Same warehouse/site only; all sites; same site but multiple warehouses where site and warehouse differ in MTM data.

4. Assumption: Active incoming-PO status mapping still needs business confirmation before implementation filters the 30-day inbound window.
   Why this assumption is needed: The prompt explicitly asks for confirmation whether active statuses are only `F` and `R`, or whether `O` and `P` should also be included.
   Potential impact if wrong: The inbound-material section could omit legitimate supply or include noise from statuses the business does not consider actionable.
   Alternative interpretations considered: Include only `F` and `R`; include `F`, `R`, `O`, and `P`; postpone strict filtering until status semantics are confirmed.

5. Assumption: PO rollup grain should combine all qualifying PO lines for the same part across all qualifying POs in the 30-day window into one part-level summary per card.
   Why this assumption is needed: The prompt asks for rolled-up PO progress but does not ask for a separate progress block per PO.
   Potential impact if wrong: A part-level rollup could hide per-PO detail the business expects, while per-PO summaries could make the cards much noisier than intended.
   Alternative interpretations considered: Single part-level rollup; one rollup per PO; one rollup per vendor plus part.

6. Assumption: Each card should show a short list of upcoming distinct shipment dates rather than a single best date only.
   Why this assumption is needed: The prompt asks for "upcoming shipment dates" in plural, but the UI shape and maximum detail level are not yet fixed.
   Potential impact if wrong: Showing only one date may hide important near-term arrivals, while showing too many dates may make the card unreadable.
   Alternative interpretations considered: One best date only; a distinct-date list; one row per inbound line inside each card.

7. Assumption: Blanket orders should remain visible on the card, but their date display should use the existing business rule of showing `Last received on` for the relevant line instead of presenting a normal inbound shipment date.
   Why this assumption is needed: The prompt marks this rule as confirmed business guidance, but implementation still needs explicit acknowledgement because it changes how cards sort and display date information.
   Potential impact if wrong: Blanket orders could be sorted or displayed as normal inbound supply, which would mislead planners about expected future receipts.
   Alternative interpretations considered: Hide blanket orders entirely; treat blanket orders like standard inbound dates; display blanket orders in a separate section.

8. Assumption: The tool should be added to the Ship/Rec tool-selection screen as a new analysis-oriented entry, but the exact category label and registry key still need confirmation.
   Why this assumption is needed: The current registry has only one lookup tool, and the prompt suggests an analysis-style placement without defining the exact category shape to preserve.
   Potential impact if wrong: The tool could land in a confusing category or require a later navigation refactor.
   Alternative interpretations considered: Add it under the existing lookup category; add a new analysis category; bypass the tool-selection surface and open it from another workflow.

9. Assumption: Export is out of scope for the first implementation pass.
   Why this assumption is needed: The prompt explicitly frames export as optional and not required unless requested.
   Potential impact if wrong: Omitting export could leave out a user-facing requirement, while adding it now would expand both the UI and validation surface significantly.
   Alternative interpretations considered: No export in v1; CSV only; CSV and XLSX from day one.

10. Assumption: No strict performance SLA is currently defined, so the first implementation should optimize query shape and filtering but not introduce caching or background prefetch behavior unless requested.
    Why this assumption is needed: The prompt asks for a performance target only if one exists, and none is encoded in the repo or the request.
    Potential impact if wrong: The first implementation may feel slower than expected under production data volume, or it may accumulate unnecessary complexity if optimization is overbuilt too early.
    Alternative interpretations considered: Query-only optimization first; add caching immediately; define a hard response-time budget before any implementation.

Please confirm, correct, or clarify these assumptions before implementation continues on the material availability board. The repo instructions require these major assumptions to be explicit before code changes proceed.
