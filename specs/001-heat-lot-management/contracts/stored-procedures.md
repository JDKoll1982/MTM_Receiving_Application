# Stored Procedure Contract: Heat / Lot Management

The MySQL contract for the Heat / Lot Management feature. One procedure per file. New procedures land
under `Database/Database_Deployment/Sql_Files/StoredProcedures/Settings/`. Changed procedures keep the
file they already occupy under `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/`.

## Conventions

- Every procedure that reports an outcome ends with `OUT p_status INT` and
  `OUT p_error_message VARCHAR(1000)`. `p_status` is `0` on success and `1` on failure;
  `p_error_message` is `NULL` on success.
- An expected business failure — a duplicate lot, a missing row, an unpermitted state — raises
  `SIGNAL SQLSTATE '45000'` with a readable `MESSAGE_TEXT`. The handler copies that message into
  `p_error_message` and sets `p_status = 1`. An unexpected failure is never swallowed.
- A read-only procedure returns a result set and declares no OUT parameters.
- A procedure that modifies data declares `SET NOCOUNT ON` and wraps its work in an explicit
  `START TRANSACTION` / `COMMIT`, with an exit handler that rolls back.
- No C# file issues raw SQL. Every call reaches these procedures through `Dao_Tool_LotMarking` or an
  existing receiving DAO.
- Infor Visual (SQL Server) stays read-only and is not touched by this feature.
- `p_heat_key` carries the normalized heat number produced by `Helper_LotNumberNormalizer.Normalize`.
  The value held in `heat` stays exactly as the operator typed it.
- `p_supplier_group` is never NULL. It carries the resolved official supplier id, the normalized
  supplier name as a fallback, or the literal `NONPO` when the receipt has no supplier. Keeping it
  non-NULL is what makes the unique key work, because MySQL treats NULLs in a unique key as distinct.

## Ledger Tables

- `lot_markings` — new, `Database/Database_Deployment/Sql_Files/Schemas/50_Table_lot_markings.sql`.
  One row per lot: `id`, `heat_key`, `heat`, `supplier_group`, `supplier_id`, `supplier_name`,
  `heat_shape_code`, `heat_color_code`, `source`, `is_locked`, `override_reason`, `notes`,
  `modified_by`, `created_at`, `updated_at`. A unique key on (`heat_key`, `supplier_group`) enforces
  one marking per lot (FR-001, FR-003, FR-005).
- **As built (2026-10-14).** The shipped table names these columns `heat_number`,
  `heat_number_normalized`, `supplier_id`, `supplier_name`, `shape_code`, `color_code`,
  `marking_source`, `resolved_by` and `resolved_at`, and it holds the supplier **group** in
  `supplier_id` rather than in a separate `supplier_group` column. What matters here is the width:
  the group is `VARCHAR(255)`, sized for the normalized vendor name it falls back to, and not the
  `VARCHAR(15)` of an id. A 15 character column and matching procedure parameters rejected real
  vendor names with error 1406 and stopped markings being recorded, which is why
  `17_Migration_lot_markings_supplier_group_width.sql` exists.
- `lot_marking_part_scope` — new,
  `Database/Database_Deployment/Sql_Files/Schemas/51_Table_lot_marking_part_scope.sql`. One row per
  part number: `id`, `part_id`, `is_enabled`, `added_by`, `created_at`, `updated_at`. A unique key on
  `part_id` lets a removal deactivate instead of delete (FR-009, FR-012).
- `receiving_history` and `receiving_label_data` — changed, each gaining `heat_shape_code` and
  `heat_color_code`. Both are written once onto the receiving queue row and copied verbatim from
  there, so a marking is never recomputed (FR-004, FR-016).

## Lot Marking Procedures

### `sp_LotMarkings_Resolve`

- **Inputs**: `p_heat_key VARCHAR(100)`, `p_supplier_group VARCHAR(255)`.
- **Returns**: at most one row — `id`, `heat_shape_code`, `heat_color_code`, `source`, `is_locked`.
  An empty result set means this lot has never been seen.
- **Failure modes**: none. A new lot is a normal answer, not an error. The caller inserts and retries.
- **Probe set**: the caller hands the unfiltered ledger projection from `sp_LotMarkings_GetAll` to
  `Helper_LotMarkingResolver.Resolve`, so a combination already held by any lot is skipped. Probing
  across every lot rather than only the current supplier is what makes two suppliers sharing one heat
  number distinguishable (SC-005).
- **Satisfies**: FR-001, FR-003, FR-004, FR-005, FR-007, FR-015.

### `sp_LotMarkings_GetAll`

- **Inputs**: `p_supplier_group VARCHAR(255)` optional — `NULL` returns every lot. `p_limit INT`
  optional — `NULL` returns every matching row.
- **Returns**: one row per lot — `id`, `heat_key`, `heat`, `supplier_group`, `supplier_name`,
  `heat_shape_code`, `heat_color_code`, `source`, `is_locked`, `override_reason`, `modified_by`,
  `updated_at` — ordered by `updated_at DESC`.
- **Failure modes**: none. An empty result set means no lot has been resolved yet.
- **Satisfies**: FR-018, FR-019, and the edge case requiring a shared marking to be visible in the
  tool rather than hidden. The unfiltered call also supplies the probe set described above.

### `sp_LotMarkings_GetByHeat`

- **Inputs**: `p_heat_search VARCHAR(100)`, `p_limit INT`.
- **Returns**: the `sp_LotMarkings_GetAll` projection restricted to rows whose `heat` or `heat_key`
  contains `p_heat_search`, ordered by `heat` then `supplier_group`.
- **Failure modes**: none. No match returns an empty result set and the tool shows its empty state.
- **Satisfies**: FR-017. This is a partial-match search for a person; `sp_LotMarkings_Resolve` is the
  exact-key lookup used by the write path.

### `sp_LotMarkings_Insert`

- **Inputs**: `p_heat_key VARCHAR(100)`, `p_heat VARCHAR(100)`,
  `p_supplier_group VARCHAR(255)`, `p_supplier_id VARCHAR(50)`, `p_supplier_name VARCHAR(255)`,
  `p_shape_code VARCHAR(20)`, `p_color_code VARCHAR(20)`, `p_source VARCHAR(20)`,
  `p_modified_by VARCHAR(100)`.
- **Outputs**: `p_new_id INT` — the new row id, `0` on failure. `p_status INT`,
  `p_error_message VARCHAR(1000)`.
- **Failure modes**: `SIGNAL SQLSTATE '45000'` `'Lot marking already exists for this heat and
  supplier'` when (`heat_key`, `supplier_group`) is already taken. That signal is the race guard
  behind FR-004: two concurrent receipts for one new lot must not both create a marking. The caller
  re-reads through `sp_LotMarkings_Resolve` and uses the row that won.
- **Satisfies**: FR-001, FR-002, FR-004. `p_modified_by`, `created_at`, and `updated_at` are recorded
  on the row for traceability.

### `sp_LotMarkings_UpdateMarking`

- **Inputs**: `p_id INT`, `p_shape_code VARCHAR(20)`, `p_color_code VARCHAR(20)`,
  `p_reason VARCHAR(500)`, `p_modified_by VARCHAR(100)`.
- **Outputs**: `p_status INT`, `p_error_message VARCHAR(1000)`. The procedure also returns the stored
  row so the caller can confirm `is_locked` is unchanged.
- **Behaviour**: sets both codes, sets `source` to the manual value, stores `p_reason` in
  `override_reason`, and stamps `modified_by` and `updated_at`. It never clears `is_locked`.
- **Failure modes**: `SIGNAL SQLSTATE '45000'` `'A reason is required to change a lot marking'` when
  `p_reason` is null, empty, or whitespace only. `SIGNAL SQLSTATE '45000'` `'Lot marking not found'`
  when no row matches `p_id`.
- **Lock note**: FR-019 forbids **the system** from changing a locked marking, while FR-018 requires
  the operator to be able to replace one by hand. A lock therefore blocks the resolver, not the
  operator, so this procedure updates a locked row and leaves the flag set.
- **Satisfies**: FR-018, FR-020.

### `sp_LotMarkings_SetLocked`

- **Inputs**: `p_id INT`, `p_is_locked TINYINT(1)`, `p_modified_by VARCHAR(100)`.
- **Outputs**: `p_status INT`, `p_error_message VARCHAR(1000)`.
- **Behaviour**: flips `is_locked`, stamps `modified_by` and `updated_at`, and changes no code.
- **Failure modes**: `SIGNAL SQLSTATE '45000'` `'Lot marking not found'` when no row matches `p_id`.
- **Satisfies**: FR-019.

## Part Scope Procedures

### `sp_LotMarkingPartScope_GetAll`

- **Inputs**: none.
- **Returns**: `id`, `part_id`, `is_enabled`, `added_by`, `created_at`, `updated_at`, ordered with
  enabled rows first, then by `part_id`.
- **Failure modes**: none. An empty result set is the specified starting state, in which the feature
  is invisible (FR-009, FR-010).
- **Satisfies**: FR-009, FR-012.

### `sp_LotMarkingPartScope_Search`

- **Inputs**: `p_search_text VARCHAR(50)`, `p_limit INT`.
- **Returns**: the `sp_LotMarkingPartScope_GetAll` projection restricted to rows whose `part_id`
  contains `p_search_text`, enabled rows first.
- **Failure modes**: none. An empty result set means the part is not on the list yet, which is a
  normal answer.
- **Note**: the candidate list a user chooses a new part from comes from the existing part fuzzy
  search, not from this procedure. This one answers "is this part already on the list, and is it on
  or off", which is what makes a removed part re-enableable.
- **Satisfies**: FR-011, and the re-enable path required by FR-012.

### `sp_LotMarkingPartScope_Upsert`

- **Inputs**: `p_part_id VARCHAR(50)`, `p_added_by VARCHAR(100)`.
- **Outputs**: `p_scope_id INT`, `p_status INT`, `p_error_message VARCHAR(1000)`.
- **Behaviour**: inserts the row when the part is absent; when the part is already present it sets
  `is_enabled` to 1, leaves `added_by` at the original value, and stamps `updated_at`. It never
  touches `lot_markings`.
- **Failure modes**: `SIGNAL SQLSTATE '45000'` `'Part number is required'` when `p_part_id` is null,
  empty, or whitespace only. An already-present part is **not** a failure: the upsert is idempotent
  so a double click cannot fail the tool.
- **Satisfies**: FR-009, FR-011, FR-012.

### `sp_LotMarkingPartScope_SetEnabled`

- **Inputs**: `p_part_id VARCHAR(50)`, `p_is_enabled TINYINT(1)`, `p_modified_by VARCHAR(100)`.
- **Outputs**: `p_status INT`, `p_error_message VARCHAR(1000)`.
- **Behaviour**: flips `is_enabled` and stamps `updated_at`. It deletes nothing and never touches
  `lot_markings`, which is how FR-012 is honoured: removing a part stops new assignments and leaves
  every recorded marking alone.
- **Failure modes**: `SIGNAL SQLSTATE '45000'` `'Part number is not on the lot marking list'` when no
  row matches `p_part_id`.
- **Satisfies**: FR-009, FR-010, FR-012.

### `sp_LotMarkingPartScope_Delete`

- **Inputs**: `p_part_id VARCHAR(50)`.
- **Outputs**: `p_status INT`, `p_error_message VARCHAR(1000)`.
- **Behaviour**: permanently removes one scope row. No `lot_markings` row is read or written.
- **Failure modes**: `SIGNAL SQLSTATE '45000'` `'Part number is not on the lot marking list'` when no
  row matches `p_part_id`.
- **Note**: the tool's Remove action calls `sp_LotMarkingPartScope_SetEnabled` with
  `p_is_enabled = 0`, so the audit trail of what was once enabled survives. This procedure exists for
  clearing a row that was added by mistake.
- **Satisfies**: FR-012.

## Changed Receiving Procedures

Each change below is confined to the column lists and parameter lists named; no other behaviour moves.

### `sp_Receiving_LabelData_Insert`

- **Change**: `p_heat_shape_code VARCHAR(20)` and `p_heat_color_code VARCHAR(20)` appended as the last
  two IN parameters, with `heat_shape_code` and `heat_color_code` added to the explicit column list
  and to the matching `VALUES` list. The procedure declares no OUT parameters and keeps its
  `INSERT IGNORE` behaviour.
- **Satisfies**: FR-001, FR-008, FR-009, FR-010, FR-014.

### `sp_Receiving_LabelData_Update`

- **Change**: the same two IN parameters appended, and `heat_shape_code` and `heat_color_code` added to
  the `SET` list. The parameter list grows from 36 to 38 IN parameters; the procedure declares no OUT
  parameters.
- **Satisfies**: FR-014.

### `sp_Receiving_LabelData_GetAll`

- **Change**: `heat_shape_code` and `heat_color_code` added to the explicit SELECT list. The procedure
  takes no parameters and declares no OUT parameters.
- **Satisfies**: FR-014.

### `sp_Receiving_LabelData_ClearToHistory`

- **Change**: `heat_shape_code` and `heat_color_code` added to the `receiving_history` INSERT column
  list and to the `SELECT` reading `receiving_label_data`. This is a straight copy — the promotion
  path never resolves a marking.
- **Failure modes**: unchanged. A rollback sets `OUT p_status` to 1 with `p_error_message` populated
  and `p_rows_moved` to 0. `OUT p_rows_moved` and `OUT p_archive_batch_id` keep their present
  meaning, and the copy does not disturb them.
- **Satisfies**: FR-016.

### `sp_Receiving_LabelData_InsertFromHistory`

- **Change**: `heat_shape_code` and `heat_color_code` added to the `receiving_label_data` INSERT
  column list and selected from `receiving_history`, so a queued reprint carries the original marking
  exactly.
- **Failure modes**: unchanged. `SIGNAL SQLSTATE '45000'` `'This history record is already queued for
  reprint.'` when an `is_reprint = 1` row already exists for the same history GUID.
- **Satisfies**: FR-016.

### `sp_Receiving_Load_GetAll`

- **Change**: `heat_shape_code` and `heat_color_code` added to the explicit SELECT list reading
  `receiving_history`. Parameters and OUT parameters are unchanged — the procedure has neither.
- **Satisfies**: FR-014, FR-016.

### `sp_Receiving_Load_Insert`

- **Change**: `p_HeatShapeCode VARCHAR(20)` and `p_HeatColorCode VARCHAR(20)` added as the last two IN
  parameters, with the two columns added to the `receiving_history` INSERT. No OUT parameters.
- **Satisfies**: FR-016.

### `sp_Receiving_Load_Update`

- **Change**: `p_HeatShapeCode` and `p_HeatColorCode` added as the last two IN parameters, and the two
  columns added to both statements the procedure already runs — the `receiving_history` UPDATE and the
  `receiving_label_data` UPDATE — so an edited load cannot leave the two tables disagreeing.
- **Satisfies**: FR-016.

### `sp_Receiving_History_Get`

- **Change**: `heat_shape_code` and `heat_color_code` added to the SELECT reading `receiving_history`,
  beside the `heat` it already returns. Parameters are unchanged.
- **Satisfies**: FR-016.

### `sp_Receiving_History_GetForReprint`

- **Change**: `rh.heat_shape_code` and `rh.heat_color_code` added to the SELECT, beside the `rh.heat`
  the procedure already returns. Its four optional parameters and its `already_queued` flag are
  unchanged.
- **Note**: this procedure already returns `heat`; it is the reprint row mapper that currently
  discards it. The two new columns have to reach the reprint row model for FR-016 to hold on screen.
- **Satisfies**: FR-016.

### `sp_Receiving_History_Import`

- **Change**: `p_HeatShapeCode VARCHAR(20)` and `p_HeatColorCode VARCHAR(20)` added, with the two
  columns added to the `receiving_history` INSERT, so an imported history row can carry a marking
  rather than arriving permanently blank.
- **Satisfies**: FR-016.

## Changed View

### `Database/Database_Deployment/Sql_Files/Views/02_View_receiving_history.sql`

- **Change**: `heat_shape_code` and `heat_color_code` added to **both** UNION branches — the one over
  `receiving_history` and the one over `receiving_label_data`. Both branches must list them, in the
  same relative position, or the union stops being positionally aligned. No existing view column
  moves, so current consumers are unaffected.
- **Failure modes**: none. The file is a `CREATE OR REPLACE VIEW` and is idempotent.
- **Satisfies**: FR-014, FR-016.

## Deployment Order

1. Fresh database: create the tables first — `Schemas/10_Table_receiving_history.sql`,
   `Schemas/11_Table_receiving_label_data.sql`, `Schemas/50_Table_lot_markings.sql`,
   `Schemas/51_Table_lot_marking_part_scope.sql`.
2. Then deploy the new `Settings/` procedure files and the changed `Receiving/` procedure files.
3. Then run `Views/02_View_receiving_history.sql`, last, because both receipt tables have to carry the
   columns before the view can select them.
4. Installed database: `Migrations/16_Migration_lot_markings.sql` creates the two tables, adds the two
   columns to both receipt tables, and re-creates every receiving procedure and the view it
   invalidates **inside the same file**. Shipping the re-created procedures with the columns keeps an
   installed database from ever sitting in a state where its procedures and its tables disagree.
5. `Database/SyncTool/sync_policy.yml` allow-lists the two new tables so a data sync carries them.
