# Decisions

Last Updated: 2026-11-12

## D-01 — The module boundary is a namespace rule, not a project reference

The repository compiles every module into one assembly, so there is no project reference to remove. The boundary is enforced by the module folder and namespace: no `using` of Receiving, no shared model, enum, constant, data access class, procedure name or settings key. It is verified by building with each module's folder excluded.

## D-02 — The residual is compared against an as-of balance

`CR_PART_LOCATION` is a live snapshot. Comparing a historical day's residual against today's on-hand would fail every day of a backlog sweep. The balance is therefore reconstructed as today's on-hand minus the signed net movement at that location after the strict window. For a same-day run the post-window movement is empty and the figure is exactly the live on-hand.

## D-03 — Saved locations are bound before general allocation

A row whose saved location is already a destination bucket with enough quantity left is bound there first and reported unchanged. Without this, today's allocate-then-compare order can move a row off a location the material genuinely reached.

## D-04 — Movement direction comes from `TYPE`

The outbound leg is `TYPE='O'` and the inbound leg is `TYPE='I'`. The lower-id-is-source heuristic inverted source and destination whenever the out leg carried the higher id, which would have certified a wrong residual.

## D-05 — The proof reads the integer quantity

`quantity` is the field the proof compares; `weight_quantity` is carried for display only. The two are projected separately on the module's own row model, which is why switching the field is a behaviour change rather than a rename.

## D-06 — The grouping key carries the purchase order

The old part-and-date key merged two purchase orders of one part on one day, which would corrupt every bucket in the group. The key is purchase order, part and transaction date; receipts without a purchase order use part, date and vendor.

## D-07 — One navigation entry that opens a launcher page

The module appears once, in the Reporting group. That entry opens a launcher page built in the established mode-select style, which opens the working page. The launcher is where future modes appear without restructuring navigation.

## D-08 — The module ships `Documentation/`, not `docs/`

The documentation rule is binding and calls the folder a deliverable. Older modules keep their legacy `docs/` folders; this module starts from the required shape.

## D-09 — Any authenticated user may correct any row

No supervisor gate is applied. The review flow is unaffected either way, and the single write procedure is the one place a later role gate would be enforced, so nothing is designed out. The working default is recorded in the procedure's header.

## D-10 — The module owns its Visual data access

No data access class is shared between modules. The module's Visual DAO takes the connection string by constructor injection, repeats the read-only guard, and loads its own query files through the core query loader, which is a helper rather than a data access class.

## D-11 — One write procedure covers both row sources

Rows come from the label queue and the history archive. One procedure takes the row source, the row id, the destination and the operator, and reports the affected row count. A discriminant column keeps it one procedure rather than two.

## D-12 — The outcome type is `Enum_ReconciliationOutcome`

The spec calls the values outcomes and pins their display names, so the type noun follows the spec and the members follow the pinned names.

## Open measurement — the quantity field

The non-writable rate on one live day has not been measured. If weight-based parts diverge noticeably more than unit-count parts, reopening D-05 with the measured figure is the trigger to consider the decimal weight quantity instead of the integer.
