# Receiving Location Evidence Query Workflow

Last Updated: 2026-04-14

This diagram explains how the location-evidence lookup works in simple terms.
It is based on the flow used by `16_GetReceivingLocationEvidence.sql`.

```mermaid
flowchart TD
  Start([Start with one saved receiving row]) --> UseInputs[Use the order number, material, optional line number, and saved date]

  UseInputs --> FindReceiptRows[Find receipt rows that match the same order and material]
  FindReceiptRows --> NarrowByDate[Keep only rows close to the saved date when a date is available]
  NarrowByDate --> PickLatestReceipt[Work out the latest matching receipt location]

  UseInputs --> FindMoveRows[Find movement history for the same order and material]
  FindMoveRows --> NarrowMoves[Keep only movement rows with a real location and a nearby date when available]
  NarrowMoves --> PickLatestMove[Pick the most recent matching movement location]

  UseInputs --> FindCurrentStock[Find where the material exists right now in current inventory]
  FindCurrentStock --> KeepPositiveStock[Keep only current locations that still have stock]

  PickLatestReceipt --> CombineEvidence[Combine receipt evidence, movement evidence, and current inventory]
  PickLatestMove --> CombineEvidence
  KeepPositiveStock --> CombineEvidence

  CombineEvidence --> ReturnRows[Return the current location choices and the latest supporting details]
  ReturnRows --> End([Location evidence is ready for the app to review])
```

## Plain-English Summary

- The lookup starts from one saved receiving row.
- It checks three kinds of evidence:
- Past receipt rows for the same order and material.
- Past movement rows for the same order and material.
- Current inventory locations where that material is still on hand.
- It narrows the results with the saved date when a date is available.
- The app then uses that evidence to decide whether there is one clear current location, no clear answer, or too many possible answers.
- When one destination location holds the full matched PO transfer quantity for the same receipt-day group, the reconciliation flow can allocate that aggregate movement across all affected saved MTM rows.
