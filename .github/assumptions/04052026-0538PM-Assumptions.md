# Receiving Location Reconciliation Assumptions

Last Updated: 2026-04-05

1. Confirmed by user on 2026-04-05: MTM does not use the Infor Visual `TRACE` feature, and heat/lot values are not stored in Infor Visual.
   Why this matters: reconciliation must not depend on `TRACE`, `TRACE_INV_TRANS`, or heat/lot matching.
   Implementation consequence: location reconciliation will use receiving transaction history plus current inventory only.

2. Current location should be inferred from both `INVENTORY_TRANS` history and current on-hand inventory in `PART_LOCATION`.
   Why this assumption is needed: without traceability, we need both the latest movement evidence and the current inventory footprint to make the best available location decision.
   Potential impact if wrong: a single-source approach could over-trust stale transaction history or ignore the current state of inventory.
   Alternative interpretations considered: use only `PART_LOCATION`; use only `INVENTORY_TRANS`.

3. When reconciliation returns multiple distinct current locations for the same receiving row, the app should report the row as ambiguous and skip the update.
   Why this assumption is needed: silently choosing one location would overwrite MySQL history without strong evidence that the selected location is correct.
   Potential impact if wrong: users may need to handle more rows manually, but the app avoids writing an incorrect location automatically.
   Alternative interpretations considered: choose the highest-quantity location; choose the newest transaction-derived location; prompt per-row for manual selection.

4. `Validate all History` widens the MySQL row scope only; the Visual-side match still uses each row's own PO/part/date and optional heat/lot.
   Why this assumption is needed: the toggle description is about whether historical MySQL rows participate in reconciliation, not about broadening the underlying Visual match criteria.
   Potential impact if wrong: reconciliation may be narrower than expected for some edge cases where receipts were posted on a different day than the local save date.
   Alternative interpretations considered: query Visual across a broader received-date window when the toggle is enabled; ignore the saved date completely in all-history mode.

User clarification received on 2026-04-05: do not use trace-based logic. Proceeding with transaction-history plus current-inventory reconciliation.
