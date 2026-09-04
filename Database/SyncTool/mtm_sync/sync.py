"""Idempotent reference-data sync engine (test -> core).

The engine computes a *plan* (inserts/updates) for each allowlisted table using
the table's natural business key, honors ignore lists, and orders tables
parents-first. Child foreign keys that point at a parent's identity column are
written through a scalar subquery resolved on the parent's natural key, so FK
integrity is preserved even when identities differ between databases and even
when a parent row is brand new on the target (the parent INSERT precedes the
child INSERT in the same transaction).

The plan can be:

  * reported (what-if),
  * rendered as ordered, reviewable SQL (--generate-sql), or
  * executed inside one transaction (--execute).

The engine never deletes rows, never touches tables outside the allowlist, and
re-running it on an already-synced target produces zero changes (idempotent).
"""

from __future__ import annotations

import dataclasses
from typing import Any, Dict, List, Optional, Sequence, Set, Tuple

from . import config as cfg
from .connection import format_literal, quote_ident, run_dml, run_query


# --------------------------------------------------------------------------- #
# Column metadata helpers
# --------------------------------------------------------------------------- #
@dataclasses.dataclass
class ColumnMeta:
    name: str
    extra: str  # raw information_schema EXTRA text

    @property
    def is_identity(self) -> bool:
        return "auto_increment" in (self.extra or "")

    @property
    def is_generated(self) -> bool:
        return "GENERATED" in (self.extra or "").upper()


def _table_column_meta(conn, db_name: str, table: str) -> Dict[str, ColumnMeta]:
    rows = run_query(conn, """
        SELECT COLUMN_NAME AS name, EXTRA AS extra
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
        """,
        (db_name, table),
    )
    return {r["name"]: ColumnMeta(name=r["name"], extra=r.get("extra") or "") for r in rows}


def _table_exists(conn, db_name: str, table: str) -> bool:
    rows = run_query(conn, """
        SELECT COUNT(*) AS c
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s AND TABLE_TYPE = 'BASE TABLE'
        """,
        (db_name, table),
    )
    return bool(rows and rows[0]["c"])


def _required_insert_columns(conn, db_name: str, table: str) -> List[str]:
    """Target columns that are NOT NULL, have no default, and are not auto/generated.

    These must be supplied when inserting a brand-new row (MySQL rejects NULL
    for them), even when the policy lists them as ignore columns (the ignore
    rule applies to matching/updating existing core rows, not to creating them).
    """
    rows = run_query(conn, """
        SELECT COLUMN_NAME AS name
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
          AND IS_NULLABLE = 'NO'
          AND COLUMN_DEFAULT IS NULL
          AND EXTRA NOT LIKE '%%auto_increment%%'
          AND EXTRA NOT LIKE '%%GENERATED%%'
        """,
        (db_name, table),
    )
    return [r["name"] for r in rows]


def _read_rows(conn, table: str, columns: Sequence[str]) -> List[Dict[str, Any]]:
    if not columns:
        return []
    cols_sql = ", ".join(quote_ident(c) for c in columns)
    return run_query(conn, f"SELECT {cols_sql} FROM {quote_ident(table)}")


def _key_tuple(row: Dict[str, Any], key_columns: Sequence[str]) -> Tuple[Any, ...]:
    return tuple(row.get(c) for c in key_columns)


# --------------------------------------------------------------------------- #
# FK identity remap (natural key -> target id at execution time)
# --------------------------------------------------------------------------- #
@dataclasses.dataclass
class FkContext:
    """Lookups to translate a child FK between databases via the parent's natural key."""

    parent_table: str
    #: child column that references the parent
    child_column: str
    #: parent identity column the child points at (e.g. 'id')
    parent_id_column: str
    #: parent natural-key column used for resolution (single column supported)
    parent_key_column: str
    #: source: parent id -> natural key
    source_id_to_key: Dict[Any, Any]
    #: target: parent natural key -> id
    target_key_to_id: Dict[Any, Any]
    #: target: parent id -> natural key (for expected-state conversion)
    target_id_to_key: Dict[Any, Any]


def _load_fk_context(
    source_conn,
    target_conn,
    source_db: str,
    target_db: str,
    fk_map: cfg.FkMap,
    child_table: str,
) -> FkContext:
    """Load the id <-> natural key maps needed to remap one child FK column."""
    if len(fk_map.parent_key) != 1:
        raise ValueError(
            f"Table {child_table!r} fk_map column {fk_map.column!r} references "
            f"{fk_map.references!r} with composite parent key {fk_map.parent_key}; "
            f"only single-column parent keys are supported for FK remap."
        )
    parent = fk_map.references
    parent_key_column = fk_map.parent_key[0]
    parent_id_column = _parent_id_column(source_conn, source_db, fk_map)

    source_rows = _read_rows(
        source_conn, parent, [parent_id_column, parent_key_column]
    )
    target_rows = _read_rows(
        target_conn, parent, [parent_id_column, parent_key_column]
    )

    source_id_to_key: Dict[Any, Any] = {
        r[parent_id_column]: r[parent_key_column] for r in source_rows
    }
    target_key_to_id: Dict[Any, Any] = {
        r[parent_key_column]: r[parent_id_column] for r in target_rows
    }
    target_id_to_key: Dict[Any, Any] = {
        r[parent_id_column]: r[parent_key_column] for r in target_rows
    }
    return FkContext(
        parent_table=parent,
        child_column=fk_map.column,
        parent_id_column=parent_id_column,
        parent_key_column=parent_key_column,
        source_id_to_key=source_id_to_key,
        target_key_to_id=target_key_to_id,
        target_id_to_key=target_id_to_key,
    )


def _parent_id_column(conn, db_name: str, fk_map: cfg.FkMap) -> str:
    """Determine the parent table's id column for an fk_map entry.

    Prefers the column referenced by an actual foreign key pointing at the
    parent, then falls back to the parent's auto-increment column, then the
    first column (the primary key in practice).
    """
    rows = run_query(conn, """
        SELECT REFERENCED_COLUMN_NAME AS ref
        FROM information_schema.KEY_COLUMN_USAGE
        WHERE CONSTRAINT_SCHEMA = %s AND REFERENCED_TABLE_NAME = %s
        LIMIT 1
        """,
        (db_name, fk_map.references),
    )
    if rows and rows[0]["ref"]:
        return rows[0]["ref"]
    meta = _table_column_meta(conn, db_name, fk_map.references)
    for col in meta.values():
        if col.is_identity:
            return col.name
    if meta:
        return sorted(meta.keys())[0]
    raise ValueError(
        f"Could not determine parent id column for {fk_map.references} "
        f"(referenced by child column {fk_map.column})."
    )


# --------------------------------------------------------------------------- #
# Table plans
# --------------------------------------------------------------------------- #
@dataclasses.dataclass
class TablePlan:
    policy: cfg.TablePolicy
    shared_columns: List[str]  # writable columns present in both DBs (minus ignore)
    fk_contexts: List[FkContext]  # remaps for child FK columns
    inserts: List[Dict[str, Any]]  # full rows (business space) to insert
    updates: List[Tuple[Dict[str, Any], Dict[str, Any]]]  # (key_values, new_values)
    unchanged: int
    skipped: int
    #: expected target rows in business space (FK columns as parent natural keys)
    expected_rows: Dict[Tuple[Any, ...], Dict[str, Any]]

    @property
    def fk_columns(self) -> Set[str]:
        return {ctx.child_column for ctx in self.fk_contexts}

    def resolver_by_column(self, column: str) -> Optional[FkContext]:
        for ctx in self.fk_contexts:
            if ctx.child_column == column:
                return ctx
        return None


def _shared_writable_columns(
    source_conn,
    target_conn,
    source_db: str,
    target_db: str,
    policy: cfg.TablePolicy,
) -> List[str]:
    """Compute columns this sync is allowed to write for a table.

    Only columns present in *both* databases, excluding generated/identity
    columns and the per-table ignore list, are ever written.
    """
    source_meta = _table_column_meta(source_conn, source_db, policy.table)
    target_meta = _table_column_meta(target_conn, target_db, policy.table)

    def writable(meta: Dict[str, ColumnMeta]) -> Set[str]:
        return {
            name
            for name, col in meta.items()
            if not col.is_identity and not col.is_generated
        }

    ignored = set(policy.ignore_columns)
    shared = sorted((writable(source_meta) & writable(target_meta)) - ignored)
    missing_keys = [k for k in policy.natural_keys if k not in shared]
    if missing_keys:
        raise ValueError(
            f"Table {policy.table!r}: natural key column(s) {missing_keys} are not "
            f"writeable on both databases (missing, ignored, or structurally different)."
        )
    return shared


def _to_business_row(
    row: Dict[str, Any],
    fk_contexts: List[FkContext],
    key_maps: Dict[str, Dict[Any, Any]],
) -> Dict[str, Any]:
    """Convert a raw (id-space) row into business space for FK columns.

    key_maps maps parent_table -> {parent_id: natural_key}.
    """
    if not fk_contexts:
        return dict(row)
    business = dict(row)
    for ctx in fk_contexts:
        raw = row.get(ctx.child_column)
        if raw is None:
            business[ctx.child_column] = None
            continue
        mapping = key_maps.get(ctx.parent_table, {})
        business[ctx.child_column] = mapping.get(raw, raw)
    return business


def build_table_plan(
    source_conn,
    target_conn,
    source_db: str,
    target_db: str,
    policy: cfg.TablePolicy,
    fk_contexts: List[FkContext],
) -> TablePlan:
    """Build the insert/update plan for one table (read-only; writes nothing)."""
    if not _table_exists(source_conn, source_db, policy.table):
        raise ValueError(f"Source table does not exist: {source_db}.{policy.table}")
    if not _table_exists(target_conn, target_db, policy.table):
        raise ValueError(f"Target table does not exist: {target_db}.{policy.table}")

    shared = _shared_writable_columns(source_conn, target_conn, source_db, target_db, policy)
    key_columns = list(policy.natural_keys)
    fk_cols = {ctx.child_column for ctx in fk_contexts}
    # FK columns are set through a natural-key subquery; they are never diffed
    # or propagated as value updates on existing rows.
    value_columns = [c for c in shared if c not in key_columns and c not in fk_cols]

    # Ignored audit columns that are still required on INSERT (NOT NULL, no
    # default) must be populated from the source row for brand-new rows. They
    # are never compared or updated for rows that already exist.
    source_names = set(_table_column_meta(source_conn, source_db, policy.table))
    required_target = _required_insert_columns(target_conn, target_db, policy.table)
    insert_fill = sorted(
        c for c in required_target
        if c in source_names and c not in shared and c not in fk_cols
    )
    insert_columns = shared + insert_fill

    # Read source rows and translate child FK ids into parent natural keys.
    source_rows = _read_rows(source_conn, policy.table, insert_columns)
    translated_rows: List[Dict[str, Any]] = []
    skipped = 0
    for row in source_rows:
        if any(row.get(k) is None for k in key_columns):
            skipped += 1  # NULL natural key cannot be matched safely
            continue
        translated = dict(row)
        ok = True
        for ctx in fk_contexts:
            raw = row.get(ctx.child_column)
            if raw is None:
                translated[ctx.child_column] = None
                continue
            natural_key = ctx.source_id_to_key.get(raw)
            if natural_key is None:
                ok = False  # dangling reference inside the source database
                break
            translated[ctx.child_column] = natural_key
        if not ok:
            skipped += 1
            continue
        translated_rows.append(translated)

    # Index current target rows by natural key.
    target_rows = _read_rows(target_conn, policy.table, shared)
    target_by_key: Dict[Tuple[Any, ...], Dict[str, Any]] = {}
    for row in target_rows:
        target_by_key[_key_tuple(row, key_columns)] = row

    inserts: List[Dict[str, Any]] = []
    updates: List[Tuple[Dict[str, Any], Dict[str, Any]]] = []
    unchanged = 0

    for row in translated_rows:
        key = _key_tuple(row, key_columns)
        existing = target_by_key.get(key)
        if existing is None:
            inserts.append(row)
            continue
        if policy.mode == cfg.MODE_INSERT_IF_MISSING:
            unchanged += 1
            continue
        # upsert: push non-key, non-FK, non-ignored values from test to target.
        changes = {c: row[c] for c in value_columns if row.get(c) != existing.get(c)}
        if changes:
            updates.append(({c: key[i] for i, c in enumerate(key_columns)}, changes))
        else:
            unchanged += 1

    # Expected post-sync state in business space (FK columns as natural keys).
    target_key_maps = {ctx.parent_table: ctx.target_id_to_key for ctx in fk_contexts}
    expected_rows: Dict[Tuple[Any, ...], Dict[str, Any]] = {}
    for key, row in target_by_key.items():
        expected_rows[key] = _to_business_row(row, fk_contexts, target_key_maps)
    for row in inserts:
        # Expected-state rows are kept over the comparable (shared) columns only;
        # insert-fill audit columns are not part of the union comparison.
        expected_rows[_key_tuple(row, key_columns)] = {c: row[c] for c in shared}
    for key_values, changes in updates:
        expected_rows[_key_tuple(key_values, key_columns)].update(changes)

    return TablePlan(
        policy=policy,
        shared_columns=shared,
        fk_contexts=fk_contexts,
        inserts=inserts,
        updates=updates,
        unchanged=unchanged,
        skipped=skipped,
        expected_rows=expected_rows,
    )


# --------------------------------------------------------------------------- #
# Table ordering (parents before children)
# --------------------------------------------------------------------------- #
def order_policies(
    conn,
    db_name: str,
    policies: Sequence[cfg.TablePolicy],
) -> List[cfg.TablePolicy]:
    """Order active policies parents-first using real FK relationships on the target."""
    by_table = {p.table: p for p in policies}
    active_names = set(by_table)

    rows = run_query(conn, """
        SELECT TABLE_NAME AS child, REFERENCED_TABLE_NAME AS parent
        FROM information_schema.REFERENTIAL_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = %s
        """,
        (db_name,),
    )
    edges: Dict[str, Set[str]] = {t: set() for t in active_names}
    for r in rows:
        child, parent = r.get("child"), r.get("parent")
        if child in active_names and parent in active_names and child != parent:
            edges[child].add(parent)

    ordered: List[str] = []
    visited: Dict[str, int] = {}

    def visit(table: str) -> None:
        state = visited.get(table, 0)
        if state == 2:
            return
        if state == 1:
            raise ValueError(f"FK cycle detected among synced tables involving {table}")
        visited[table] = 1
        for parent in sorted(edges[table]):
            visit(parent)
        visited[table] = 2
        ordered.append(table)

    try:
        for table in sorted(active_names):
            visit(table)
    except ValueError:
        # Cycle: keep the config file order (already parents-first in policy).
        return list(policies)

    return [by_table[t] for t in ordered]


def plan_all(
    source_conn,
    target_conn,
    source_db: str,
    target_db: str,
    policies: Sequence[cfg.TablePolicy],
) -> List[TablePlan]:
    """Plan all tables in FK-safe order."""
    ordered = order_policies(target_conn, target_db, policies)

    # Load FK contexts per (child table, fk_map).
    plans: List[TablePlan] = []
    for policy in ordered:
        fk_contexts: List[FkContext] = []
        for fk_map in policy.fk_map:
            fk_contexts.append(
                _load_fk_context(source_conn, target_conn, source_db, target_db, fk_map, policy.table)
            )
        plans.append(
            build_table_plan(
                source_conn, target_conn, source_db, target_db, policy, fk_contexts
            )
        )
    return plans


# --------------------------------------------------------------------------- #
# Statement rendering / execution
# --------------------------------------------------------------------------- #
def _column_snippet(
    column: str,
    value: Any,
    resolver: Optional[FkContext],
    literal: bool,
) -> Tuple[str, Any]:
    """Return (sql_snippet, parameter) for one column value.

    FK columns resolve the parent's id through a scalar subquery on the parent's
    natural key, so inserts are correct even when the parent row was itself just
    inserted earlier in the same transaction.
    """
    if resolver is None or value is None:
        if literal:
            return format_literal(value), None
        return "%s", value
    sub = (
        f"(SELECT {quote_ident(resolver.parent_id_column)} "
        f"FROM {quote_ident(resolver.parent_table)} "
        f"WHERE {quote_ident(resolver.parent_key_column)} = "
    )
    if literal:
        return sub + f"{format_literal(value)})", None
    return sub + "%s)", value


def _insert_sql(
    table_ref: str,
    row: Dict[str, Any],
    resolver_by_col: Dict[str, FkContext],
    literal: bool,
) -> Tuple[str, Optional[List[Any]]]:
    cols = list(row.keys())
    col_sql = ", ".join(quote_ident(c) for c in cols)
    snippets: List[str] = []
    params: List[Any] = []
    for c in cols:
        snippet, param = _column_snippet(c, row[c], resolver_by_col.get(c), literal)
        snippets.append(snippet)
        if not literal:
            params.append(param)
    sql = f"INSERT INTO {table_ref} ({col_sql}) VALUES ({', '.join(snippets)})"
    if literal:
        return sql + ";", None
    return sql, params


def _update_sql(
    table_ref: str,
    key_values: Dict[str, Any],
    changes: Dict[str, Any],
    resolver_by_col: Dict[str, FkContext],
    literal: bool,
) -> Tuple[str, Optional[List[Any]]]:
    set_snippets: List[str] = []
    params: List[Any] = []
    for column, value in changes.items():
        snippet, param = _column_snippet(column, value, resolver_by_col.get(column), literal)
        set_snippets.append(f"{quote_ident(column)} = {snippet}")
        if not literal:
            params.append(param)
    where = " AND ".join(f"{quote_ident(c)} = %s" for c in key_values)
    if literal:
        where_lit = " AND ".join(
            f"{quote_ident(c)} = {format_literal(v)}" for c, v in key_values.items()
        )
        return f"UPDATE {table_ref} SET {', '.join(set_snippets)} WHERE {where_lit};", None
    params.extend(key_values.values())
    return f"UPDATE {table_ref} SET {', '.join(set_snippets)} WHERE {where}", params


def render_statements(plans: Sequence[TablePlan]) -> List[str]:
    """Render the ordered, human-reviewable SQL for --generate-sql."""
    statements: List[str] = []
    for plan in plans:
        table_ref = quote_ident(plan.policy.table)
        resolver_by_col = {ctx.child_column: ctx for ctx in plan.fk_contexts}
        statements.append(
            f"-- {plan.policy.table}: +{len(plan.inserts)} insert, "
            f"{len(plan.updates)} update, {plan.unchanged} unchanged, {plan.skipped} skipped"
        )
        for row in plan.inserts:
            sql, _ = _insert_sql(table_ref, row, resolver_by_col, literal=True)
            statements.append(sql)
        for key_values, changes in plan.updates:
            sql, _ = _update_sql(table_ref, key_values, changes, resolver_by_col, literal=True)
            statements.append(sql)
    return statements


def render_sql_file(
    plans: Sequence[TablePlan],
    source_db: str,
    target_db: str,
    policy_file: str,
) -> str:
    """Compose the full ordered SQL text for human review."""
    lines = [
        "-- ============================================================",
        "-- Generated by MTM reference-data sync (test -> core)",
        f"-- Source: {source_db}   Target: {target_db}",
        f"-- Policy: {policy_file}",
        "-- Review before executing. The tool itself never auto-applies this file.",
        "-- Statements are ordered parents-first and FK-safe.",
        "-- ============================================================",
        "START TRANSACTION;",
        "",
    ]
    lines.extend(render_statements(plans))
    lines.append("")
    lines.append("COMMIT;")
    return "\n".join(lines)


def print_plan_summary(plans: Sequence[TablePlan]) -> None:
    """Print a what-if summary of what the sync would do (read-only)."""
    print("\nSync plan (read-only; nothing written):")
    print(f"{'Table':<34}{'+insert':<10}{'~update':<10}{'same':<8}{'skip':<6}")
    print("-" * 70)
    total_inserts = total_updates = 0
    for plan in plans:
        print(
            f"{plan.policy.table:<34}{len(plan.inserts):<10}{len(plan.updates):<10}"
            f"{plan.unchanged:<8}{plan.skipped:<6}"
        )
        total_inserts += len(plan.inserts)
        total_updates += len(plan.updates)
    print("-" * 70)
    print(
        f"Totals: {total_inserts} insert(s), {total_updates} update(s). "
        f"Second run will change 0 rows (idempotent)."
    )


def execute_plans(target_conn, plans: Sequence[TablePlan]) -> Dict[str, int]:
    """Execute plans against the target inside one transaction.

    Returns {table: rows_changed}. On any error the transaction is rolled back
    so nothing is ever partially applied.
    """
    changed: Dict[str, int] = {}
    try:
        for plan in plans:
            table_ref = quote_ident(plan.policy.table)
            resolver_by_col = {ctx.child_column: ctx for ctx in plan.fk_contexts}
            count = 0
            for row in plan.inserts:
                sql, params = _insert_sql(table_ref, row, resolver_by_col, literal=False)
                run_dml(target_conn, sql, params)
                count += 1
            for key_values, changes in plan.updates:
                sql, params = _update_sql(table_ref, key_values, changes, resolver_by_col, literal=False)
                run_dml(target_conn, sql, params)
                count += 1
            changed[plan.policy.table] = count
        target_conn.commit()
    except Exception:
        target_conn.rollback()
        raise
    return changed


def residual_changes(
    source_conn,
    target_conn,
    source_db: str,
    target_db: str,
    policies: Sequence[cfg.TablePolicy],
) -> List[Tuple[str, int, int]]:
    """Re-plan against an updated target and report residual changes.

    A correctly synced target must return an empty list (idempotency check).
    Returns (table, insert_count, update_count) for any residual work.
    """
    plans = plan_all(source_conn, target_conn, source_db, target_db, policies)
    return [
        (p.policy.table, len(p.inserts), len(p.updates))
        for p in plans
        if p.inserts or p.updates
    ]
