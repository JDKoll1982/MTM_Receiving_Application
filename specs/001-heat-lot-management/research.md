# Phase 0 Research: Heat / Lot Management

**Feature**: `specs/001-heat-lot-management/heat-lot-management.spec.md`
**Created**: 2026-10-14

Each entry records a genuine choice the codebase did not already settle, with the decision taken, why,
and what was rejected.

## D1. What identifies a lot

**Decision**: A lot is identified by the pair (normalized heat number, supplier group). The supplier
group is the resolved official supplier id, the normalized supplier name as a fallback, or the literal
`NONPO` when a receipt has no supplier.

**Rationale**: The product owner requires that two suppliers using the same heat number stay visually
distinct, and the receiving tables carry the supplier as a name only. Keying on the pair satisfies
FR-005, and the `NONPO` sentinel keeps the unique key valid because MySQL treats NULLs in a unique key
as distinct, which would silently permit duplicate ledger rows.

**Alternatives considered**: Keying on the heat number alone. Rejected: it merges two suppliers' lots
into one marking and violates FR-005. Keying on (heat, part number). Rejected: the same physical lot
legitimately arrives against several part numbers, which would split one lot into several markings and
defeat the visual purpose.

## D2. Whether the marking is computed live or pinned

**Decision**: The hash is a seed only. The first time a lot is resolved, the marking is written to the
ledger and every later read returns the stored row. A locked row is returned untouched.

**Rationale**: A live recompute means that adding a shape or color later silently repaints every
historical lot, while the physical marking on the floor cannot change. That divergence is worse than
having no marking, and it is exactly what FR-004 and SC-004 forbid.

**Alternatives considered**: Pure recompute with no table. Rejected: it cannot satisfy FR-004, cannot
support an override, and leaves no audit trail. A cache-only ledger rebuilt on demand. Rejected: a
flush would repaint history.

## D3. How collisions are handled

**Decision**: After computing the seed, the resolver probes forward deterministically past any
marking already held by another lot in the same scope, up to a bounded number of attempts, then
accepts the computed value if every attempt is taken. The policy is a setting.

**Rationale**: 10 shapes and 12 colors give 120 combinations. With roughly twenty lots active at once
the chance of at least one shared marking is about 70 percent, which is high enough to matter and low
enough that a human will never catch it. FR-015 requires the later lot to get a different marking, and
the ledger already holds everything needed to probe.

**Alternatives considered**: Leave collisions alone. Rejected: it violates FR-015. Add a third
dimension such as fill pattern now. Rejected as premature: it triples the palette surface before the
need is proven, and the palette model is already shaped to accept it later.

## D4. When the marking is resolved, and whether it is ever recomputed

**Decision**: The marking is resolved once when a receipt line is saved, and its two codes are stored
on the queue row. Promotion copies the stored codes into receiving history, and reprint copies them
back. Nothing re-resolves on promotion or reprint, and a reprint row never resolves at all.

**Rationale**: Promotion and reprint are separate user actions that happen at different times, so a
marking resolved at promotion time could disagree with what the handler saw when the line was
received. Copying the stored value is also the only approach that satisfies FR-016, and it sidesteps
the reprint rows that are deliberately outside the normal flow.

**Alternatives considered**: Resolving on the fly wherever a marking is displayed. Rejected: it
reintroduces live recompute and would show a different marking on an old reprint. Resolving at
promotion. Rejected: the stored value and the displayed value would drift.

## D5. What counts as a blank heat number

**Decision**: A line counts as having no heat number when the value is null, empty, whitespace only,
or the literal sentinel the application already substitutes, `"Nothing Entered"`. Those lines receive
no marking.

**Rationale**: The receiving flow does not store a blank heat value. Three separate call sites write
`"Nothing Entered"` instead. Treating that literal as a real lot number would collapse every
blank-heat line in the application into one shared lot and one shared marking, which is a silent and
severe defect.

**Alternatives considered**: Treating only null and whitespace as blank. Rejected: it produces exactly
that collapse. Removing the sentinel at the source. Rejected: it is out of scope and would change
existing stored data and the reprint path.

## D6. How heat values are normalized for matching

**Decision**: A new normalizer trims, collapses internal whitespace, uppercases, and strips known
vendor separators to produce the matching key. The stored `heat` value is not changed.

**Rationale**: FR-003 and the spec's edge cases require `ABC-123`, `abc123` and `ABC 123` to resolve to
one lot. The existing normalization is only trim plus uppercase, which does not collapse separators.
Leaving the stored value alone is required because existing rows and the reprint procedure copy `heat`
verbatim.

**Alternatives considered**: Reusing the model's existing heat normalization. Rejected: too weak, and
it would have to change stored values to work consistently.

## D7. How a supplier is resolved to a stable identity

**Decision**: Resolve the supplier name on the receipt to the official supplier id using the existing
Infor Visual vendor fuzzy search, taking the id when exactly one match is returned. On zero or
multiple matches, fall back to the normalized supplier name. Store both the resolved id and the name
seen on the receipt on the ledger row.

**Rationale**: FR-006 requires matching the official record rather than comparing typed text, and
comparing raw names would let `ABC Corp` and `ABC Corp.` become two suppliers and two markings. The
codebase has no exact supplier-id lookup, only a fuzzy search, so a fallback is required rather than
optional.

**Alternatives considered**: Comparing normalized names only. Rejected: it violates FR-006. Failing the
receipt when the supplier cannot be resolved. Rejected: it violates FR-008's principle that the
feature never blocks receiving.

## D8. How the enabled part list is stored

**Decision**: A dedicated table with a unique key on the part number and an active flag, plus a
stored-procedure family in the same shape as the existing welded-coils settings table. Removing a part
deactivates it rather than deleting the row.

**Rationale**: This is data, not a preference, so it does not belong in the settings manifest. The
welded-coils table is the established precedent for a Ship/Rec Tools owned list, and its active-flag
pattern maps exactly onto FR-012, which requires removal to leave recorded markings untouched.

**Alternatives considered**: Storing the list as a JSON settings value. Rejected: it cannot carry a
unique key, cannot be searched, and would need hand-rolled parsing. Hard-deleting on removal.
Rejected: it loses the audit trail of what was once enabled.

## D9. Where the new settings live and who may change them

**Decision**: All new keys go in the existing `ShipRecTools` manifest category. The system-wide switch
is system scoped at `Admin` permission; the remaining keys are user scoped at `User` permission.

**Rationale**: Reusing the category needs no enum or parser change. The switch is the only setting that
changes behaviour for everyone, so it is the only one gated above a normal user, and a missing manifest
entry fails closed with no visible error, so under-permissioning is the safe direction.

**Alternatives considered**: Making every key `User` level. Rejected: any user could disable the
feature for the whole site. A new settings category. Rejected: it fragments the module's settings
surface for no gain.

## D10. Where the marking is shown

**Decision**: The marking appears on each receipt line and on the per-step receipt header card. The
reprint grid also shows it, which requires a new field on the reprint row model and its mapper, because
that mapper currently discards the heat value the procedure already returns.

**Rationale**: FR-014 names the line and the receipt; FR-016 requires the marking to be visible on a
reprinted receipt. The reprint grid is the only place a reprinted receipt is reviewed before it is
re-queued.

**Alternatives considered**: Lines only. Rejected: it fails the receipt-level requirement in FR-014.
Leaving the reprint grid alone. Rejected: the requirement is explicit, and the data is already
returned by the procedure.

## D11. How the schema change reaches installed databases

**Decision**: Ship both a numbered schema file for fresh deployments and a numbered migration for
installed databases, and have the migration re-create the receiving procedures it invalidates as part
of the same file.

**Rationale**: The schema files use `CREATE TABLE IF NOT EXISTS`, so they cannot alter an existing
table; the migration is the only path that fixes an installed database. Existing migrations that add
columns to these very tables already re-deploy the procedures alongside the change, because those
procedures enumerate their columns explicitly and would otherwise fail.

**Alternatives considered**: Migration only. Rejected: a fresh deployment would miss the tables. Schema
files only. Rejected: an installed database would never receive the columns. Relying on the deployment
tool to re-deploy procedures separately. Rejected: it leaves a window where the procedures and the
tables disagree.

## D12. The column-enumeration blast radius

**Decision**: Treat the column addition as a change to fourteen database objects, including the
history view, and update the schema files to match the migration.

**Rationale**: Both receipt tables are read with explicit column lists and no `SELECT *`. The history
view unions one select from each table into a fixed column set, so two new columns mean four new view
columns or the view definition diverges from the tables.

**Alternatives considered**: Adding the columns to the procedures that obviously display a marking
only. Rejected: promotion and reprint silently blank the marking if their column lists are not updated,
which is the failure the feature exists to prevent.

## D13. How a manual override records its reason

**Decision**: The ledger carries a reason field alongside the existing notes field, and an override
requires it. The change is attributed to the acting user with a timestamp.

**Rationale**: FR-018 requires a recorded reason and FR-020 requires attribution. The proposed ledger
shape had a notes column only, which the spec does not name as the reason.

**Alternatives considered**: Reusing the notes column. Rejected: notes is free text for operators, and
overloading it makes the reason unqueryable. Adding a separate audit table. Rejected as more surface
than the requirement needs.

## D14. Whether to add the marking to the Infor Visual supplier lookup interface

**Decision**: Do not extend `IService_InforVisual`. Call the existing vendor fuzzy search as-is and
apply the fallback described in D7.

**Rationale**: The existing method already returns the supplier id in its result key, so a new interface
member would add surface without adding capability, and the fallback has to exist regardless.

**Alternatives considered**: Adding an exact supplier-id lookup. Rejected as unnecessary surface for a
fallback that is required either way.
