# Data Model: Heat / Lot Management

**Feature**: `specs/001-heat-lot-management/heat-lot-management.spec.md`
**Created**: 2026-10-14
**Inputs**: `heat-lot-management.spec.md`, `plan.md`, `research.md`

## Scope

Two tables are new. `lot_markings` holds one permanent marking per lot and is the only place a shape or
color is ever chosen. `lot_marking_part_scope` holds the opt-in part list. Two existing receipt tables
each gain the same two marking columns. Shapes, colors, marking source and collision policy are code
enums, not tables, so only the chosen value is persisted.

Every read and write in this model goes through `Dao_Tool_LotMarking` using stored procedures. No table
here is ever written by raw SQL, and none of them lives in Infor Visual.

| Entity | Change | Storage |
| --- | --- | --- |
| Lot Marking | new | `lot_markings` |
| Enabled Part | new | `lot_marking_part_scope` |
| Receiving Queue Row | reshaped | `receiving_label_data` |
| Receiving History Row | reshaped | `receiving_history` |
| Palette Value | code only | `Model_LotMarkingPalette` (`Model_LotMarkingShape`, `Model_LotMarkingColor`) |

## Lot Marking (`lot_markings`)

One row per lot. A lot is the pair (normalized heat number, supplier group). The shape and color are
written once, when the lot is first seen, and are read back unchanged on every later mention.

### Fields

| Field | Type | Null / Default | Meaning |
| --- | --- | --- | --- |
| `id` | `INT` | `AUTO_INCREMENT`, PK | Surrogate key. |
| `heat_number` | `VARCHAR(100)` | `NOT NULL` | Heat number exactly as seen, never rewritten. |
| `heat_number_normalized` | `VARCHAR(100)` | `NOT NULL` | The matching key produced by `Helper_LotNumberNormalizer`. |
| `supplier_id` | `VARCHAR(255)` | `NOT NULL`, default `'NONPO'` | Supplier group; never null. Sized for the normalized supplier name it falls back to, which is why the unique key needs `ROW_FORMAT DYNAMIC`. |
| `supplier_name` | `VARCHAR(255)` | `NULL` | Supplier name as it appeared on the receipt. |
| `shape_code` | `VARCHAR(32)` | `NOT NULL` | Assigned shape. Holds the emoji itself, which is the persisted value. |
| `color_code` | `VARCHAR(32)` | `NOT NULL` | Assigned color, an `Enum_LotColor` name. |
| `marking_source` | `VARCHAR(16)` | `NOT NULL`, default `'Auto'` | `Auto`, `Manual` or `Import`. |
| `is_locked` | `TINYINT(1)` | `NOT NULL`, default `0` | `1` when a user has protected the marking. |
| `first_seen_part_number` | `VARCHAR(50)` | `NULL` | The part that first created the lot. |
| `change_reason` | `VARCHAR(255)` | `NULL` | Reason recorded with a manual override. |
| `resolved_by` | `VARCHAR(50)` | `NULL` | The user who last changed the marking. |
| `resolved_at` | `DATETIME` | `NOT NULL`, default `CURRENT_TIMESTAMP` | Last resolve or change. |
| `notes` | `VARCHAR(255)` | `NULL` | Free-text operator note. Not the override reason. |
| `created_at` | `TIMESTAMP` | `NOT NULL`, default `CURRENT_TIMESTAMP` | Row creation. |

### Keys

- `PRIMARY KEY` (`id`).
- `UNIQUE KEY` `uq_lot_markings_heat_supplier` (`heat_number_normalized`, `supplier_id`). 1420 bytes
  in utf8mb4, so the table is pinned to `ROW_FORMAT DYNAMIC` and the 3072 byte large-prefix limit.
- `KEY` `ix_lot_markings_marking` (`shape_code`, `color_code`).

### Validation Rules

- **FR-001** — The unique key admits exactly one row per (normalized heat number, supplier group), so a
  lot always carries one shape and one color. A repeat receipt reads that row instead of inserting a
  second.
- **FR-002** — `shape_code` and `color_code` are both `NOT NULL` and are written by
  `Helper_LotMarkingResolver`. No field exists for a user to specify a marking on a routine receipt.
- **FR-003** — Matching happens on `heat_number_normalized`, so `ABC-123`, `abc123` and `ABC 123` reach
  the same row, while `heat_number` keeps the text that was actually entered.
- **FR-003** — `heat_number_normalized` is `VARCHAR(100)`, the same width as `heat` on the receipt
  tables. Normalization trims, collapses internal whitespace, uppercases and strips vendor separators,
  so it can only shorten a value and cannot truncate (research D6).
- **FR-004** — `shape_code` and `color_code` are written once, at insert. No path recomputes them, and
  adding a shape or color later never updates an existing row.
- **FR-005** — `supplier_id` is part of the unique key, so the same heat number from a second supplier
  inserts its own row with its own marking.
- **FR-006** — `supplier_id` holds the official supplier id when the Infor Visual vendor search returns
  exactly one match. `supplier_name` retains the text seen on the receipt for traceability (research D7).
- **FR-007** — `supplier_id` is `NOT NULL` and defaults to the literal `NONPO`, so receipts with no
  supplier share one group. The column is deliberately not nullable: MySQL treats NULLs in a unique key
  as distinct, which would let one supplierless lot hold two rows.
- **FR-008** — No row is written when the heat number is blank. Blank means null, the empty string,
  whitespace only, or the literal `Nothing Entered` (research D5).
- **FR-015** — `ix_lot_markings_marking` exists so the resolver can find which `(shape_code, color_code)`
  pairs are already held before it accepts a computed marking. The seed is deterministic, and the probe
  advances from it while a pair is taken (research D3).
- **FR-018** — `change_reason` is required whenever a user sets a marking by hand. `notes` stays free
  text and is not the reason field (research D13).
- **FR-019** — `is_locked = 1` pins the row. The resolver returns it unchanged and probes nothing.
- **FR-020** — A manual change writes `resolved_by` and `resolved_at` with the acting user and the time
  of the change.
- **FR-021** — The available palette is enumerated in code, not queried from a table, because the set is
  fixed by the request and the tool lists it directly.

## Enabled Part (`lot_marking_part_scope`)

The opt-in list. A part receives markings only while it has an enabled row here. The table starts empty,
so the feature is invisible until someone adds a part.

### Fields

| Field | Type | Null / Default | Meaning |
| --- | --- | --- | --- |
| `id` | `INT` | `AUTO_INCREMENT`, PK | Surrogate key. |
| `part_id` | `VARCHAR(50)` | `NOT NULL` | Part identifier from Infor Visual. |
| `is_enabled` | `TINYINT(1)` | `NOT NULL`, default `1` | `1` while the part receives markings. |
| `added_by` | `VARCHAR(50)` | `NULL` | The user who enabled the part. |
| `added_at` | `DATETIME` | `NOT NULL`, default `CURRENT_TIMESTAMP` | When the part was enabled. |

### Keys

- `PRIMARY KEY` (`id`).
- `UNIQUE KEY` `uq_lot_marking_part_scope_part` (`part_id`).

### Validation Rules

- **FR-009** — A part receives a marking only when a row exists for it with `is_enabled = 1`.
- **FR-010** — No row is the normal state. The absence of a row never blocks or alters a receipt; the
  part is received exactly as it is today with no marking.
- **FR-011** — Enabling a part writes `added_by` and `added_at`. The part is chosen from the fuzzy-search
  picker, so the table stores the confirmed identifier and never a typed-from-memory value.
- **FR-012** — Removal sets `is_enabled = 0` rather than deleting the row. Nothing in `lot_markings` is
  read, updated or deleted by that action, so markings already recorded for the part survive.
- The unique key holds one row per `part_id`, so a part that is removed and added again reactivates the
  existing row instead of creating a second one.

## Marking Columns on the Receiving Tables

Both receipt tables gain the same two nullable columns. The existing lot column on each is
`heat VARCHAR(100)`, and the new columns are prefixed to match it rather than introducing a second lot
column.

### Fields

| Table | New Field | Type | Null | Meaning |
| --- | --- | --- | --- | --- |
| `receiving_label_data` | `heat_shape_code` | `VARCHAR(32)` | `NULL` | Shape stored on the queue row. |
| `receiving_label_data` | `heat_color_code` | `VARCHAR(32)` | `NULL` | Color stored on the queue row. |
| `receiving_history` | `heat_shape_code` | `VARCHAR(32)` | `NULL` | Shape after promotion. |
| `receiving_history` | `heat_color_code` | `VARCHAR(32)` | `NULL` | Color after promotion. |

On the queue table the supplier is already carried as a name by `po_vendor` and `vendor_name`; on the
history table it is `vendor_name`. Those columns are unchanged, and the marking columns are added
alongside them.

### Validation Rules

- **FR-014** — These columns are the per-line source of the marking shown on a receipt line; the receipt
  header reads its markings from the same values at receipt level.
- **FR-016** — Promotion copies the stored codes from the queue row into the history row. Nothing
  re-resolves at that point, and a reprint copies the codes back to the queue unchanged (research D4).
- **FR-008** — A line with a blank heat number, and a line for a part that is not on the enabled list,
  both leave these columns as NULL and save normally.
- **FR-013** — With the feature switched off, no new line writes a code. The columns stay readable for
  every line that already has one, and nothing is cleared.
- Both columns are nullable because receipts created before this feature carry no marking. Reviewing and
  reprinting those receipts must keep working exactly as it does today.
- The `receiving_history` view (`Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql`)
  unions one select from each table into a fixed column set, so two columns on each table means four new
  view columns. The receiving procedures that enumerate their columns explicitly must add all four, or
  promotion and reprint blank the marking (research D12).

## Enumerations

Each enumeration is a code enum in `Module_ShipRec_Tools/Enums/` and is persisted by name in the column
noted. `shape_code` and `color_code` are `VARCHAR(32)`, wider than the longest name in either list.

### Shape palette (10 values)

```text
⚡, 💗, 🌙, ⭐, 💎, ⬛, 🔺, ⭕, ❌, ➡️
```

### `Enum_LotColor` (12 values)

```text
BLUE, RED, GREEN, WHITE, YELLOW, BLACK, PURPLE, ORANGE, PINK, GRAY, BROWN, TEAL
```

- **FR-001** — 10 shapes and 12 colors give 120 combinations, which is what makes a collision possible
  and therefore what FR-015 has to handle.
- **FR-004** — The two lists can grow later without touching a recorded marking, because the stored value
  is a name and no reader re-derives a marking from the list.
- **FR-021** — The tool lists both enumerations in full so a user can see every available shape and color.

### `Enum_LotMarkingSource` (persisted in `marking_source`)

```text
Auto, Manual, Import
```

- **FR-002** — `Auto` is the column default and the value for every routine receipt.
- **FR-018** — `Manual` is set when a user replaces a marking by hand, and it is the only source that
  requires `change_reason`.
- `Import` is a permitted value of the column, but no requirement in this feature produces it.

### `Enum_LotMarkingCollisionPolicy` (setting `LotMarking.CollisionPolicy`, default `Probe`)

```text
Allow, Probe, Warn
```

- **FR-015** — `Probe` is the default and the behaviour this feature depends on: after computing the
  seed, the resolver deterministically advances to the next free pair, gives up after a bounded number of
  attempts, and then accepts the computed value if every attempt was taken (research D3).
- `Allow` and `Warn` are permitted values of the setting. No requirement in this feature selects them, so
  no behaviour is fixed for them here.

## Relationships

- A receipt line reaches its lot by value, not by foreign key. The line's normalized `heat` and its
  supplier group identify the `lot_markings` row, and the two codes are then copied onto the line so the
  display never has to resolve anything (research D4).
- `receiving_label_data` to `receiving_history` is a one-way copy of `heat_shape_code` and
  `heat_color_code`. The marking is never recalculated in either direction.
- `lot_marking_part_scope.part_id` matches `receiving_label_data.part_id` by value. No foreign key is
  declared, because the list is opt-in configuration and the part identifier on a receipt comes from
  Infor Visual as text.
- Neither new table is joined to Infor Visual. The only contact with Infor Visual is the read-only vendor
  search used to resolve `supplier_id` (research D7, D14).
- Both new tables are owned by `Module_ShipRec_Tools` and are allow-listed in
  `Database/SyncTool/sync_policy.yml` so the sync tool carries them between environments.

## State Transitions

States are properties of the data, not of a row status column. A lot is `Unmarked` while no queue row
carries codes for it, and `Resolved` once a `lot_markings` row exists and its codes are stored on the
queue row.

- `Unmarked` to `Resolved` — a line for an enabled part with a non-blank heat number is saved. The
  ledger row is inserted when the lot has none, and both codes are copied onto the queue row.
- `Resolved` to `Resolved` — the same lot is received again. Nothing is written; the stored row is read
  and its codes are copied.
- `Resolved` to `Resolved` — another supplier sends the same heat number. A separate ledger row is
  inserted, with its own shape and color.
- `Resolved` to `Locked` — a user replaces the marking and locks it. `shape_code`, `color_code`,
  `marking_source = Manual`, `change_reason`, `resolved_by`, `resolved_at` and `is_locked = 1` are all
  written.
- `Resolved` to `Promoted` — the receipt moves into receiving history. The two codes are copied to the
  history row and the ledger row is untouched.
- `Promoted` to `Reprinted` — the receipt is reprinted and re-queued. The stored codes return unchanged
  and no resolve step runs.
- `Unmarked` stays `Unmarked` — a blank heat number, or a part that is not on the enabled list. Nothing
  is written to either new table and the receipt saves normally.

- **FR-019** — `Locked` never returns to `Resolved` automatically. The resolver returns a locked row
  exactly as it stands, whatever the seed would have produced.
- **FR-004** — `Resolved` never returns to `Unmarked`, and a resolved shape or color never changes.
  Growing the palette is the only event that could tempt a change, and it is required to leave recorded
  rows alone.
- **FR-012** — Setting `is_enabled = 0` on a part changes nothing about a lot that is already `Resolved`.
  It only stops new lots for that part from resolving.
- **FR-013** — Switching the feature off stops new transitions into `Resolved`. Rows that are already
  `Resolved`, `Promoted` or `Reprinted` are unaffected.
