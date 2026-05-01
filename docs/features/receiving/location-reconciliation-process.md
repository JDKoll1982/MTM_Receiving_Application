# Receiving Location Reconciliation Process

Last Updated: 2026-05-01

This diagram shows how the current Receiving reconciliation feature decides whether a saved row needs review.

## What The Feature Checks

- It always checks `Current Labels`.
- It checks `History` rows from today by default.
- If the `Validate All History During Reconciliation` setting is on, it checks all visible history instead.
- It only sends rows to the review screen when the system finds a confident new location that is different from the saved one.

## Mermaid Diagram

```mermaid
flowchart TD
    A[User starts Reconcile Saved Locations] --> B[Load saved MTM rows from Current Labels and Receiving History]
    B --> C{Validate All History setting on?}
    C -- No --> D[Use Current Labels plus today's history rows]
    C -- Yes --> E[Use Current Labels plus all visible history rows]
    D --> F[Check each saved row]
    E --> F

    F --> G{Is this a PO row with both Part ID and PO Number?}
    G -- No --> H[Skip row<br/>No reconciliation review]
    G -- Yes --> I[Normalize PO number and group rows by:<br/>PO + Part + PO line + receipt date]

    I --> J[Ask InforVisual for:<br/>1. Current location evidence<br/>2. Transaction history]
    J --> K[Remove any destination locations the user chose to ignore]
    K --> L{Any usable candidate locations left?}

    L -- No --> M{Does history prove:<br/>moved into WC and later fully removed?}
    M -- Yes --> N[Propose WC as the validated location]
    M -- No --> O[Mark row as not found or pending<br/>No review row created]

    L -- Yes --> P{Does one candidate prove an exact match?}
    P -- Yes --> Q[Use the exact match]
    P -- No --> R{Does one location hold the full matched same-day transfer total for the whole receipt group?}
    R -- Yes --> S[Allocate that destination across the grouped rows]
    R -- No --> T[Mark row ambiguous or not found<br/>No review row created]

    Q --> U{Is the proposed location the same as the saved location?}
    S --> U
    N --> U

    U -- Yes --> V[Mark unchanged<br/>Keep it out of review]
    U -- No --> W[Mark updated<br/>Add row to reconciliation review queue]

    W --> X[Review screen shows:<br/>old location, new location, part, PO, quantity, user, date, time, reason]
    X --> Y{User choice}
    Y -- Save --> Z[MTM row is updated with the proposed location]
    Y -- Ignore --> AA[MTM row stays exactly as saved]

    V --> AB[Summary only]
    H --> AB
    O --> AB
    T --> AB
    Z --> AB
    AA --> AB
    AB[Finish with summary:<br/>saved, ignored, unchanged, and needs-attention counts]
```

## Plain-English Outcome Rules

- `Needs review`: only rows marked `Updated` reach the review screen.
- `No review needed`: rows already matching the saved location are marked `Unchanged`.
- `Skipped automatically`: non-PO rows, rows missing PO or part data, and rows whose only candidates are ignored do not enter review.
- `Needs manual follow-up`: ambiguous rows and pending Visual receipt situations are not auto-saved.
- `Hidden from the final needs-attention list`: rows marked `NotFound` are not shown in the final review ViewModel's visible unresolved list.