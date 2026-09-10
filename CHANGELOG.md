# CHANGELOG

All notable changes to the MTM Receiving Application are documented in this file.
Categories follow Keep a Changelog conventions: Added, Changed, Fixed, Removed, Security.

## Unreleased

### Fixed

#### Receiving history delete no longer raises a false missing-row error (2026-09-10)

- **Symptom:** Removing loads from Receiving Edit Mode failed with
  `InvalidOperationException: No receiving history row matched the delete request for load '1'`.
  The failure surfaced to the user as `Failed to save receiving data`.
- **Impact:** The selected history rows were not deleted and the transaction rolled back, so no
  data was lost. The real cost was that receiving history rows could not be removed at all.
- **Cause:** `sp_Receiving_Load_Delete` ends with `SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;`.
  MySQL reports the row count of a stored procedure's **final** statement when the procedure is
  invoked, and `SET` always reports `0`. `Dao_ReceivingLoad.DeleteLoadsAsync` read
  `AffectedRows <= 0` as "no matching row" and threw, even though the preceding `DELETE` had
  succeeded.
- **Fix:** When `AffectedRows <= 0`, `DeleteLoadsAsync` now re-checks the row with
  `ReceivingHistoryRowExistsAsync` and throws only when the row is genuinely still present. This
  matches the guard already used by `UpdateLoadsAsync` in the same DAO and by
  `Dao_ReceivingLabelData`.
- **Files changed:** `Module_Receiving/Data/Dao_ReceivingLoad.cs`

## Verification Evidence

### Reported affected rows versus actual deletion

A throwaway probe ran against the test schema (`mtm_receiving_application_test`) to isolate the
affected-rows behavior of a `CALL`. Both variants deleted their target row.

| Call body                                                       | Rows reported by client | Rows actually remaining |
| --------------------------------------------------------------- | ----------------------- | ----------------------- |
| `SET FOREIGN_KEY_CHECKS=0; DELETE ...; SET FOREIGN_KEY_CHECKS=back;` | 0                       | 0                       |
| `DELETE ...;`                                                   | 1                       | 0                       |

### Deployed procedure matches the repository

The live `sp_Receiving_Load_Delete` definition was read from the database and compared against
`Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql`.
The trailing `SET FOREIGN_KEY_CHECKS = v_old_foreign_key_checks;` is present in both, so the bug
is in the stored procedure's shape, not in a deployment drift.

### Introducing change

The trailing `SET` was appended by commit `1e4119e1` on 2026-07-20, which added foreign-key-check
handling across the Receiving and Volvo procedures. Any caller that infers row existence from the
affected-rows count of those procedures inherited the same defect.

## Related Risk

61 stored procedures under
`Database/Database_Deployment/Sql_Files/StoredProcedures/` end with a
`SET FOREIGN_KEY_CHECKS` restore rather than a data-modifying statement. Every DAO that calls one
of these procedures and branches on `AffectedRows` is exposed to the same false-negative result.

Call sites already guarded against this and known to be safe:

- `Dao_ReceivingLoad.UpdateLoadsAsync` — re-checks row existence when `AffectedRows <= 0`.
- `Dao_ReceivingLabelData` — re-checks row existence and falls back to an idempotent path.

`Dao_ReceivingLoad.DeleteLoadsAsync` was the only receiving delete path missing the guard and is
resolved by this change.

## Follow-Up

- [ ] Audit remaining DAO call sites that branch on `AffectedRows` for the 61 procedures with a
      trailing `SET FOREIGN_KEY_CHECKS` restore, prioritizing delete and update paths.
- [ ] Decide whether to fix the shape of the stored procedures themselves. Moving the
      `FOREIGN_KEY_CHECKS` restore into the `EXIT HANDLER` and terminating the body with the
      data-modifying statement would restore meaningful affected-rows reporting for every caller.
      This is a database-schema change and requires explicit approval before implementation.

## See Also

- `Module_Receiving/Data/Dao_ReceivingLoad.cs`
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Receiving/sp_Receiving_Load_Delete.sql`
- <https://keepachangelog.com/en/1.1.0/>
