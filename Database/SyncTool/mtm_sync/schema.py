"""Schema / object drift comparison between two MySQL databases.

All reads use information_schema so the comparison is deterministic and does not
rely on an external schema dump. The drift report is read-only and is used to
tell the operator what DDL still needs to be reconciled through the committed
schema pipeline (Database/Database_Deployment/Sql_Files) - the sync tool never
generates or applies DDL by diffing two live databases.

Compared layers:
  * table structure: columns, indexes, foreign keys
  * views
  * stored routines (procedures and functions)
  * triggers
  * events
"""

from __future__ import annotations

import dataclasses
import re
from typing import Any, Callable, Dict, List, Set, Tuple

from .connection import run_query

# Status values used across the report.
SYNC = "in-sync"
MISSING = "missing"  # present in test, absent from core
EXTRA = "extra"  # present in core, absent from test
DIFFERENT = "different"

#: Statuses that indicate the operator must reconcile something.
DIRTY = (MISSING, EXTRA, DIFFERENT)

_WS = re.compile(r"\s+")


def _normalize_definition(text: Any, db_name: str) -> str:
    """Collapse whitespace and hide the database name inside definitions."""
    if text is None:
        return ""
    cleaned = _WS.sub(" ", str(text)).strip()
    return cleaned.replace(db_name, "<DB>")


@dataclasses.dataclass
class TableDef:
    name: str
    columns: Set[str]
    indexes: Set[str]
    fks: Set[str]


@dataclasses.dataclass
class SchemaSnapshot:
    """A point-in-time snapshot of one database's schema-relevant objects."""

    db_name: str
    tables: Dict[str, TableDef]  # real tables only (not views)
    views: Dict[str, str]  # view name -> normalized definition
    routines: Dict[Tuple[str, str], str]  # (name, type) -> normalized definition
    triggers: Dict[str, str]  # trigger name -> normalized signature
    events: Dict[str, str]  # event name -> normalized signature


def _fetch_table_names(conn, db_name: str) -> List[str]:
    rows = run_query(conn, """
        SELECT TABLE_NAME AS name
        FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = %s AND TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_NAME
        """,
        (db_name,),
    )
    return [r["name"] for r in rows]


def _fetch_columns(conn, db_name: str, table: str) -> Set[str]:
    rows = run_query(conn, """
        SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT, EXTRA,
               COLLATION_NAME
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
        ORDER BY ORDINAL_POSITION
        """,
        (db_name, table),
    )
    signatures: Set[str] = set()
    for r in rows:
        default = r.get("COLUMN_DEFAULT")
        signatures.add(
            "|".join(
                [
                    str(r["COLUMN_NAME"]),
                    str(r["COLUMN_TYPE"]),
                    str(r["IS_NULLABLE"]),
                    "NULL" if default is None else str(default),
                    str(r.get("EXTRA") or ""),
                    str(r.get("COLLATION_NAME") or ""),
                ]
            )
        )
    return signatures


def _fetch_indexes(conn, db_name: str, table: str) -> Set[str]:
    rows = run_query(conn, """
        SELECT NON_UNIQUE, INDEX_NAME, SEQ_IN_INDEX, COLUMN_NAME, SUB_PART,
               NULLABLE, INDEX_TYPE
        FROM information_schema.STATISTICS
        WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
        ORDER BY INDEX_NAME, SEQ_IN_INDEX
        """,
        (db_name, table),
    )
    signatures: Set[str] = set()
    for r in rows:
        signatures.add(
            "|".join(
                [
                    str(r["NON_UNIQUE"]),
                    str(r["INDEX_NAME"]),
                    str(r["SEQ_IN_INDEX"]),
                    str(r["COLUMN_NAME"]),
                    "NULL" if r.get("SUB_PART") is None else str(r["SUB_PART"]),
                    str(r["NULLABLE"]),
                    str(r["INDEX_TYPE"]),
                ]
            )
        )
    return signatures


def _fetch_fks(conn, db_name: str) -> Dict[str, Set[str]]:
    constraints = run_query(conn, """
        SELECT CONSTRAINT_NAME, TABLE_NAME, REFERENCED_TABLE_NAME, UPDATE_RULE, DELETE_RULE
        FROM information_schema.REFERENTIAL_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = %s
        ORDER BY CONSTRAINT_NAME
        """,
        (db_name,),
    )
    columns = run_query(conn, """
        SELECT CONSTRAINT_NAME, COLUMN_NAME, REFERENCED_COLUMN_NAME
        FROM information_schema.KEY_COLUMN_USAGE
        WHERE CONSTRAINT_SCHEMA = %s AND REFERENCED_TABLE_NAME IS NOT NULL
        ORDER BY CONSTRAINT_NAME, ORDINAL_POSITION
        """,
        (db_name,),
    )
    by_constraint: Dict[str, List[Tuple[str, str]]] = {}
    for c in columns:
        by_constraint.setdefault(c["CONSTRAINT_NAME"], []).append(
            (c["COLUMN_NAME"], c["REFERENCED_COLUMN_NAME"])
        )
    result: Dict[str, Set[str]] = {}
    for c in constraints:
        mapping = by_constraint.get(c["CONSTRAINT_NAME"], [])
        mapping_str = ",".join(f"{col}->{ref}" for col, ref in mapping)
        signature = "|".join(
            [
                str(c["CONSTRAINT_NAME"]),
                str(c["TABLE_NAME"]),
                str(c["REFERENCED_TABLE_NAME"]),
                mapping_str,
                str(c["UPDATE_RULE"]),
                str(c["DELETE_RULE"]),
            ]
        )
        result.setdefault(str(c["TABLE_NAME"]), set()).add(signature)
    return result


def fetch_schema_snapshot(conn, db_name: str) -> SchemaSnapshot:
    """Read a full schema snapshot for one database."""
    tables: Dict[str, TableDef] = {}
    fks = _fetch_fks(conn, db_name)
    for table in _fetch_table_names(conn, db_name):
        tables[table] = TableDef(
            name=table,
            columns=_fetch_columns(conn, db_name, table),
            indexes=_fetch_indexes(conn, db_name, table),
            fks=fks.get(table, set()),
        )

    views_rows = run_query(conn, """
        SELECT TABLE_NAME AS name, VIEW_DEFINITION AS definition
        FROM information_schema.VIEWS
        WHERE TABLE_SCHEMA = %s
        ORDER BY TABLE_NAME
        """,
        (db_name,),
    )
    views = {
        str(r["name"]): _normalize_definition(r.get("definition"), db_name)
        for r in views_rows
    }

    routine_rows = run_query(conn, """
        SELECT ROUTINE_NAME AS name, ROUTINE_TYPE AS type, ROUTINE_DEFINITION AS definition
        FROM information_schema.ROUTINES
        WHERE ROUTINE_SCHEMA = %s
        ORDER BY ROUTINE_TYPE, ROUTINE_NAME
        """,
        (db_name,),
    )
    routines: Dict[Tuple[str, str], str] = {}
    for r in routine_rows:
        routines[(str(r["name"]), str(r["type"]))] = _normalize_definition(
            r.get("definition"), db_name
        )

    trigger_rows = run_query(conn, """
        SELECT TRIGGER_NAME AS name, EVENT_MANIPULATION, EVENT_OBJECT_TABLE, ACTION_TIMING,
               ACTION_STATEMENT AS definition
        FROM information_schema.TRIGGERS
        WHERE TRIGGER_SCHEMA = %s
        ORDER BY TRIGGER_NAME
        """,
        (db_name,),
    )
    triggers: Dict[str, str] = {}
    for r in trigger_rows:
        signature = "|".join(
            [
                str(r["EVENT_MANIPULATION"]),
                str(r["EVENT_OBJECT_TABLE"]),
                str(r["ACTION_TIMING"]),
                _normalize_definition(r.get("definition"), db_name),
            ]
        )
        triggers[str(r["name"])] = signature

    event_rows = run_query(conn, """
        SELECT EVENT_NAME AS name, EVENT_DEFINITION AS definition, EVENT_TYPE, STATUS,
               ON_COMPLETION, INTERVAL_VALUE, INTERVAL_FIELD
        FROM information_schema.EVENTS
        WHERE EVENT_SCHEMA = %s
        ORDER BY EVENT_NAME
        """,
        (db_name,),
    )
    events: Dict[str, str] = {}
    for r in event_rows:
        signature = "|".join(
            [
                _normalize_definition(r.get("definition"), db_name),
                str(r.get("EVENT_TYPE") or ""),
                str(r.get("STATUS") or ""),
                str(r.get("ON_COMPLETION") or ""),
                str(r.get("INTERVAL_VALUE") or ""),
                str(r.get("INTERVAL_FIELD") or ""),
            ]
        )
        events[str(r["name"])] = signature

    return SchemaSnapshot(
        db_name=db_name,
        tables=tables,
        views=views,
        routines=routines,
        triggers=triggers,
        events=events,
    )


@dataclasses.dataclass
class DriftEntry:
    layer: str
    object: str
    status: str
    detail: str = ""


def _compare_set(
    entries: List[DriftEntry],
    layer: str,
    object_name: str,
    test_values: Set[str],
    core_values: Set[str],
) -> None:
    only_test = test_values - core_values
    only_core = core_values - test_values
    if only_test or only_core:
        detail = []
        if only_test:
            detail.append("in test only: " + ", ".join(sorted(only_test))[:600])
        if only_core:
            detail.append("in core only: " + ", ".join(sorted(only_core))[:600])
        entries.append(
            DriftEntry(
                layer=layer,
                object=object_name,
                status=DIFFERENT,
                detail="; ".join(detail),
            )
        )


def compare_schemas(test_snap: SchemaSnapshot, core_snap: SchemaSnapshot) -> List[DriftEntry]:
    """Compare test -> core and produce a list of drift entries (test as source).

    Status semantics relative to core:
      missing = object exists in test but not in core (needs to be added to core),
      extra   = object exists in core but not in test,
      different = exists in both but definitions/columns differ.
    """
    entries: List[DriftEntry] = []

    test_table_names = set(test_snap.tables)
    core_table_names = set(core_snap.tables)

    # Tables that exist in one database but not the other.
    for name in sorted(test_table_names - core_table_names):
        entries.append(DriftEntry("table", name, MISSING))
    for name in sorted(core_table_names - test_table_names):
        entries.append(DriftEntry("table", name, EXTRA))

    # Tables present in both: compare columns, indexes, FKs.
    for name in sorted(test_table_names & core_table_names):
        t = test_snap.tables[name]
        c = core_snap.tables[name]
        _compare_set(entries, "table:columns", name, t.columns, c.columns)
        _compare_set(entries, "table:indexes", name, t.indexes, c.indexes)
        _compare_set(entries, "table:fks", name, t.fks, c.fks)

    # Views.
    test_views = set(test_snap.views)
    core_views = set(core_snap.views)
    for name in sorted(test_views - core_views):
        entries.append(DriftEntry("view", name, MISSING))
    for name in sorted(core_views - test_views):
        entries.append(DriftEntry("view", name, EXTRA))
    for name in sorted(test_views & core_views):
        if test_snap.views[name] != core_snap.views[name]:
            entries.append(DriftEntry("view", name, DIFFERENT, "view definition differs"))

    # Routines (procedures + functions).
    test_routines = set(test_snap.routines)
    core_routines = set(core_snap.routines)
    for key in sorted(test_routines - core_routines):
        entries.append(DriftEntry(key[1].lower() + "s", key[0], MISSING))
    for key in sorted(core_routines - test_routines):
        entries.append(DriftEntry(key[1].lower() + "s", key[0], EXTRA))
    for key in sorted(test_routines & core_routines):
        if test_snap.routines[key] != core_snap.routines[key]:
            entries.append(
                DriftEntry(key[1].lower() + "s", key[0], DIFFERENT, "routine body differs")
            )

    # Triggers.
    test_triggers = set(test_snap.triggers)
    core_triggers = set(core_snap.triggers)
    for name in sorted(test_triggers - core_triggers):
        entries.append(DriftEntry("trigger", name, MISSING))
    for name in sorted(core_triggers - test_triggers):
        entries.append(DriftEntry("trigger", name, EXTRA))
    for name in sorted(test_triggers & core_triggers):
        if test_snap.triggers[name] != core_snap.triggers[name]:
            entries.append(DriftEntry("trigger", name, DIFFERENT, "trigger body differs"))

    # Events.
    test_events = set(test_snap.events)
    core_events = set(core_snap.events)
    for name in sorted(test_events - core_events):
        entries.append(DriftEntry("event", name, MISSING))
    for name in sorted(core_events - test_events):
        entries.append(DriftEntry("event", name, EXTRA))
    for name in sorted(test_events & core_events):
        if test_snap.events[name] != core_snap.events[name]:
            entries.append(DriftEntry("event", name, DIFFERENT, "event body differs"))

    return entries


def summarize(entries: List[DriftEntry]) -> Dict[str, Dict[str, int]]:
    """Summarize drift counts per layer -> status."""
    summary: Dict[str, Dict[str, int]] = {}
    for e in entries:
        layer = summary.setdefault(e.layer, {})
        layer[e.status] = layer.get(e.status, 0) + 1
    return summary


def print_report(entries: List[DriftEntry], source_db: str, target_db: str) -> None:
    """Print a concise human-readable drift report."""
    print(f"\nDrift report: {target_db} vs {source_db} (source=test).")
    print(f"{'Layer':<18}{'Object':<38}{'Status':<12}Detail")
    print("-" * 120)
    if not entries:
        print("No drift found.")
        return
    for e in entries:
        object_name = e.object[:37]
        print(f"{e.layer:<18}{object_name:<38}{e.status:<12}{e.detail[:70]}")
    print("-" * 120)
    summary = summarize(entries)
    for layer in sorted(summary):
        parts = ", ".join(f"{k}={v}" for k, v in sorted(summary[layer].items()))
        print(f"{layer}: {parts}")
    dirty = [e for e in entries if e.status in DIRTY]
    print(f"\n{len(dirty)} drift item(s) to reconcile via committed SQL files under "
          f"Database/Database_Deployment/Sql_Files.")
