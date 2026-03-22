# Assumptions For Remaining SQL Validation Fixes

1. Assumption: The remaining `sp_Settings_*` validation errors should be fixed by updating the stored procedures to match the newer `settings_universal`, `settings_personal`, and `settings_activity` schemas rather than by reintroducing old columns such as `setting_id`, `user_setting_id`, and `workstation_name`.
   Why this assumption is needed: The deployed schema files show those columns do not exist, but the affected procedures still reference them.
   Potential impact if wrong: Updating procedures to the new schema could break callers that still expect the old ID-based contract.
   Alternative interpretations considered: Restore legacy columns to the schema; keep both old and new procedure sets side by side; treat the failing procedures as obsolete and exclude them from deployment/validation.

2. Assumption: The old ID-based settings procedures are intended to be semantically remapped onto the category/key-based Settings Core model.
   Why this assumption is needed: The current schema stores settings by `category` and `setting_key`, while several failing procedures still operate on `setting_id` and assume a user-setting row identity that no longer exists in the same form.
   Potential impact if wrong: A naive remap could silently change business behavior, especially for audit history and user override resolution.
   Alternative interpretations considered: Preserve ID-based procedures by joining through a separate lookup table; deprecate these procedures and migrate application code to `sp_SettingsCore_*`; restore an explicit settings definition table with stable IDs.

3. Assumption: Audit logging should now target `settings_activity.workstation` instead of `settings_activity.workstation_name`, and any legacy references to `setting_id` or `user_setting_id` in audit rows should be replaced with the available category/key fields.
   Why this assumption is needed: The current `settings_activity` table includes `scope`, `category`, `setting_key`, `user_id`, `changed_by`, `ip_address`, and `workstation`, but not the legacy columns referenced by the failing procedures.
   Potential impact if wrong: Audit history may lose expected traceability or fail to satisfy downstream readers that still depend on legacy identifiers.
   Alternative interpretations considered: Add compatibility columns back to `settings_activity`; log legacy identifiers in `new_value`/`old_value`; leave audit procedures unchanged and mark them unsupported until callers are updated.

4. Assumption: `Database/StoredProcedures/Settings/sp_SettingsCore.sql` should either be excluded from per-file parameter-token validation or split into one routine per file, because the current validator treats the whole file as a single catalog entry and attributes tokens from later procedures to the first one.
   Why this assumption is needed: The file contains many procedures, but the validator catalog model is currently one entry per file/object-name pair and only captures the first routine signature cleanly.
   Potential impact if wrong: Suppressing those warnings could hide a legitimate parser bug the team wants fixed instead of worked around.
   Alternative interpretations considered: Build a full multi-routine parser for combined SQL files; split `sp_SettingsCore.sql` into separate procedure files; keep the warnings as a known limitation.

5. Assumption: The remaining low-noise parser warnings (`dupes` derived-table alias in migration 39 and `p_index_name` in the migration helper files) are acceptable to defer until after the Settings contract decision.
   Why this assumption is needed: Those warnings are narrow parser limitations, while the remaining 25 errors are dominated by unresolved schema/procedure contract mismatches.
   Potential impact if wrong: The validation report will continue to contain a few misleading warnings, reducing trust in the tool.
   Alternative interpretations considered: Keep refining the parser first; whitelist the known warnings in the report generator; fix both parser and Settings procedures in one pass.

Please confirm, correct, or replace these assumptions before I modify the affected Settings SQL files. If you want, reply with one of these directions:

- `D`: A and verify legacy calls are not being made, if so update the calls
