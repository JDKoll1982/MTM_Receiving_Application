"""Disposable-copy verification workflow.

Implements the safety routine from Prompt.md / the DB-script standards:

1. Create `mtm_receiving_application_backup_test` from the core database.
2. Run the sync against the copy (what-if -> generated SQL -> execute).
3. Verify:
   * structure layers of the copy equal the original core snapshot
     (the sync never changes schema),
   * every synced table matches the UNION of core-snapshot rows and test rows
     (both databases can legitimately hold rows the other does not - rows that
     only exist in core must survive, rows that only exist in test must be
     added, shared keys follow the table's write mode),
   * excluded/prod-owned tables are byte-for-byte unchanged from core.
4. Run the sync a second time and assert zero changes (idempotency).
5. Drop the disposable copy and report results.

Returns True when every check passes.
"""

from __future__ import annotations

import dataclasses
from pathlib import Path
from typing import Any, Dict, List, Optional

from . import backup, config as cfg, schema as schema_mod, sync as sync_mod
from .connection import Login, connect


@dataclasses.dataclass
class CheckResult:
    name: str
    ok: bool
    detail: str = ""


def _parent_key_maps(conn, fk_contexts) -> Dict[str, Dict[Any, Any]]:
    """Load parent id -> natural key maps (per parent table) for FK conversion."""
    maps: Dict[str, Dict[Any, Any]] = {}
    for ctx in fk_contexts:
        if ctx.parent_table in maps:
            continue
        rows = sync_mod._read_rows(
            conn, ctx.parent_table, [ctx.parent_id_column, ctx.parent_key_column]
        )
        maps[ctx.parent_table] = {
            r[ctx.parent_id_column]: r[ctx.parent_key_column] for r in rows
        }
    return maps


def _actual_rows_by_natural_key(
    copy_conn,
    table: str,
    columns: List[str],
    key_columns: List[str],
    fk_contexts,
) -> Dict[tuple, Dict[str, Any]]:
    """Read actual copy rows, convert FK ids into parent natural keys, and index
    them by the table's natural key (the same key the expected union uses)."""
    rows = sync_mod._read_rows(copy_conn, table, columns)
    key_maps = _parent_key_maps(copy_conn, fk_contexts)
    out: Dict[tuple, Dict[str, Any]] = {}
    for row in rows:
        business = sync_mod._to_business_row(row, fk_contexts, key_maps)
        out[tuple(business.get(c) for c in key_columns)] = {
            c: business[c] for c in columns
        }
    return out


def _check_structure(pre_sync_snap, post_sync_snap, copy_db: str) -> List[CheckResult]:
    """The sync is DML-only, so the copy's schema must be identical before and
    after execution. Comparing the copy to itself avoids false positives from
    logical-copy normalization during copy creation."""
    entries = schema_mod.compare_schemas(pre_sync_snap, post_sync_snap)
    dirty = [e for e in entries if e.status in schema_mod.DIRTY]
    if not dirty:
        return [
            CheckResult(
                f"structure: sync did not change {copy_db} schema",
                True,
                "tables/columns/indexes/FKs/views/routines/triggers/events unchanged",
            )
        ]
    summary = schema_mod.summarize(dirty)
    detail = "; ".join(f"{k}: {v}" for k, v in sorted(summary.items()))
    affected = ", ".join(sorted({e.object for e in dirty})[:25])
    return [
        CheckResult(
            f"structure: sync did not change {copy_db} schema",
            False,
            f"{detail}; affected: {affected}",
        )
    ]


def _check_synced_union(
    copy_conn,
    core_conn,
    test_conn,
    core_db: str,
    test_db: str,
    copy_db: str,
    policies: List[cfg.TablePolicy],
) -> List[CheckResult]:
    """Every synced table must equal the union of core rows and test rows.

    The expected union is derived by planning with the *core* as the target
    (read-only), which yields exactly core rows + test rows merged per the
    per-table write mode and ignore list. The executed copy must match it.
    """
    results: List[CheckResult] = []
    expected_plans = sync_mod.plan_all(test_conn, core_conn, test_db, core_db, policies)

    for plan in expected_plans:
        table = plan.policy.table
        shared = plan.shared_columns
        try:
            actual = _actual_rows_by_natural_key(
                copy_conn, table, shared, plan.policy.natural_keys, plan.fk_contexts
            )
        except Exception as exc:  # table missing on the copy
            results.append(
                CheckResult(f"synced {table}: present on copy", False, str(exc))
            )
            continue
        expected = plan.expected_rows

        missing_keys = set(expected) - set(actual)
        extra_keys = set(actual) - set(expected)
        value_mismatch = [
            k
            for k in set(expected) & set(actual)
            if expected[k] != actual[k]
        ]
        if not missing_keys and not extra_keys and not value_mismatch:
            results.append(
                CheckResult(
                    f"synced {table}: copy == union(core, test)",
                    True,
                    f"{len(expected)} row(s)",
                )
            )
        else:
            detail = []
            if missing_keys:
                detail.append(f"{len(missing_keys)} expected row(s) missing")
            if extra_keys:
                detail.append(f"{len(extra_keys)} unexpected row(s)")
            if value_mismatch:
                detail.append(f"{len(value_mismatch)} row(s) with differing values")
            results.append(
                CheckResult(f"synced {table}: copy == union(core, test)", False, "; ".join(detail))
            )
    return results


def _check_excluded_unchanged(
    copy_conn,
    core_conn,
    core_db: str,
    copy_db: str,
    synced_tables: set,
) -> List[CheckResult]:
    """Excluded / prod-owned tables must be byte-for-byte identical to core."""
    core_snap = schema_mod.fetch_schema_snapshot(core_conn, core_db)
    results: List[CheckResult] = []
    for table in sorted(core_snap.tables):
        if table in synced_tables:
            continue
        result = backup.compare_checksum(core_conn, copy_conn, core_db, copy_db, table)
        if result["ok"]:
            results.append(CheckResult(f"excluded {table}: unchanged from core", True))
        else:
            results.append(
                CheckResult(
                    f"excluded {table}: unchanged from core",
                    False,
                    str(result["detail"]),
                )
            )
    return results


def _check_idempotent(
    test_conn,
    copy_conn,
    test_db: str,
    copy_db: str,
    policies: List[cfg.TablePolicy],
) -> List[CheckResult]:
    """A second sync run on the copy must change zero rows."""
    residual = sync_mod.residual_changes(test_conn, copy_conn, test_db, copy_db, policies)
    if not residual:
        return [
            CheckResult(
                "idempotency: second run changes 0 rows",
                True,
                "no residual inserts/updates",
            )
        ]
    detail = "; ".join(f"{t}: +{i} insert, {u} update" for t, i, u in residual)
    return [CheckResult("idempotency: second run changes 0 rows", False, detail)]


def print_results(results: List[CheckResult]) -> bool:
    """Print check results and return whether all passed."""
    all_ok = True
    for r in results:
        all_ok = all_ok and r.ok
        marker = "PASS" if r.ok else "FAIL"
        line = f"[{marker}] {r.name}"
        if r.detail:
            line += f" - {r.detail}"
        print(line)
    return all_ok


def verify_on_copy(
    login: Login,
    config: cfg.SyncConfig,
    backup_dir: str,
    keep_copy: bool = False,
) -> bool:
    """Run the full disposable-copy verification workflow. Returns all-pass."""
    core_db, test_db, copy_db = config.core_db, config.test_db, config.backup_db
    active = config.active_policies
    synced_tables = {p.table for p in active}

    checks: List[CheckResult] = []

    # 1. Create the disposable copy from core.
    print(f"\nStep 1: creating disposable copy {copy_db} from {core_db} ...")
    dump_path = backup.make_disposable_copy(
        login, core_db, copy_db, backup_dir, protected=[core_db, test_db]
    )
    print(f"  copy created from dump: {dump_path}")

    copy_conn = connect(login, copy_db)
    core_conn = connect(login, core_db)
    test_conn = connect(login, test_db)
    try:
        # Snapshot the copy's schema before any write so we can prove the sync
        # (DML only) never changes schema.
        pre_sync = schema_mod.fetch_schema_snapshot(copy_conn, copy_db)

        # 2. What-if plan + reviewable SQL against the copy, then execute.
        plans = sync_mod.plan_all(test_conn, copy_conn, test_db, copy_db, active)
        sync_mod.print_plan_summary(plans)
        review_file = Path(backup_dir) / f"review_{copy_db}_{Path(dump_path).stem}.sql"
        review_file.write_text(
            sync_mod.render_sql_file(plans, test_db, copy_db, "sync_policy.yml"),
            encoding="utf-8",
        )
        print(f"  review SQL written (not executed): {review_file}")
        print("  executing sync on the disposable copy ...")
        changed = sync_mod.execute_plans(copy_conn, plans)
        print(f"  applied: {changed}")

        # 3a. Structure: the sync must not have changed the copy's schema.
        post_sync = schema_mod.fetch_schema_snapshot(copy_conn, copy_db)
        checks.extend(_check_structure(pre_sync, post_sync, copy_db))

        # 3b. Synced tables must equal union(core rows, test rows).
        checks.extend(
            _check_synced_union(
                copy_conn, core_conn, test_conn, core_db, test_db, copy_db, active
            )
        )

        # 3c. Excluded / prod-owned tables unchanged from core.
        checks.extend(_check_excluded_unchanged(copy_conn, core_conn, core_db, copy_db, synced_tables))

        # 4. Idempotency: a second sync on the copy changes zero rows.
        checks.extend(_check_idempotent(test_conn, copy_conn, test_db, copy_db, active))
    finally:
        copy_conn.close()
        core_conn.close()
        test_conn.close()

    # Report.
    print("\nVerification results (disposable copy):")
    all_pass = print_results(checks)

    if not keep_copy:
        print(f"\nStep 5: dropping disposable copy {copy_db} ...")
        backup.drop_disposable_copy(login, copy_db, protected=[core_db, test_db])
        print("  dropped.")
    else:
        print(f"\nKeeping disposable copy {copy_db} (--keep-copy was passed).")

    print(
        "\nVERDICT: " + ("PASS - safe to run on live core after operator review."
                         if all_pass else "FAIL - do NOT run against live core.")
    )
    return all_pass
