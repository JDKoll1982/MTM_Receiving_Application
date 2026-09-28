"""Offline self-test for the sync engine (no database required).

Fakes the database read layer (information_schema + table row reads) and
validates the core planning semantics:

  * parents are planned/inserted before children,
  * child FK columns are carried as parent natural keys (never ids),
  * upsert vs insert-if-missing behave correctly,
  * the plan is a union: core-only rows are preserved, test-only rows added,
  * applying the plan to a copy and re-planning yields zero changes (idempotent).

Run:  python -m mtm_sync.selftest   (from Database/SyncTool)
"""

from __future__ import annotations

import re
from typing import Any, Dict, List

from . import config as cfg
from . import sync


# --------------------------------------------------------------------------- #
# Fake database layer
# --------------------------------------------------------------------------- #
META: Dict[str, List[Dict[str, str]]] = {
    "dunnage_types": [
        {"name": "id", "extra": "auto_increment"},
        {"name": "type_name", "extra": ""},
        {"name": "icon", "extra": ""},
        {"name": "created_date", "extra": ""},
    ],
    "dunnage_parts": [
        {"name": "id", "extra": "auto_increment"},
        {"name": "part_id", "extra": ""},
        {"name": "type_id", "extra": ""},
        {"name": "quantity_type", "extra": ""},
    ],
    "dunnage_non_po_entries": [
        {"name": "id", "extra": "auto_increment"},
        {"name": "value", "extra": ""},
        {"name": "use_count", "extra": ""},
        {"name": "created_at", "extra": ""},
    ],
}

FK_EDGES = [{"child": "dunnage_parts", "parent": "dunnage_types"}]

# id, type_name, icon, created_date
TEST_TYPES = [
    {"id": 1, "type_name": "Pallet", "icon": "P", "created_date": None},
    {"id": 2, "type_name": "Box", "icon": "B", "created_date": None},
]
CORE_TYPES = [{"id": 100, "type_name": "Pallet", "icon": "P", "created_date": None}]
# id, part_id, type_id, quantity_type
TEST_PARTS = [
    {"id": 1, "part_id": "20x20", "type_id": 1, "quantity_type": "Quantity"},
    {"id": 2, "part_id": "32x30", "type_id": 2, "quantity_type": "Quantity"},
]
CORE_PARTS = [
    {"id": 100, "part_id": "20x20", "type_id": 100, "quantity_type": "Quantity"},
    # core-only row: must survive a sync (union semantics)
    {"id": 101, "part_id": "COREONLY", "type_id": 100, "quantity_type": "Quantity"},
]
TEST_NON_PO = [
    {"id": 1, "value": "Rework", "use_count": 7, "created_at": None},
    {"id": 2, "value": "NewReason", "use_count": 1, "created_at": None},
]
CORE_NON_PO = [{"id": 50, "value": "Rework", "use_count": 3, "created_at": None}]


class FakeConn:
    def __init__(self, db: str):
        self.db = db


def _rows_for(db: str, table: str) -> List[Dict[str, Any]]:
    if table == "dunnage_types":
        return list(TEST_TYPES if db == "test" else CORE_TYPES)
    if table == "dunnage_parts":
        return list(TEST_PARTS if db == "test" else CORE_PARTS)
    if table == "dunnage_non_po_entries":
        return list(TEST_NON_PO if db == "test" else CORE_NON_PO)
    return []


def fake_run_query(conn, sql: str, params=None) -> List[Dict[str, Any]]:
    db = conn.db
    if "information_schema.COLUMNS" in sql:
        table = params[1] if params else None
        meta = [dict(r) for r in META.get(table or "", [])]
        if "COLUMN_DEFAULT IS NULL" in sql:
            # Required-insert query: fake metadata has no nullability/defaults,
            # so approximate by excluding identity/generated columns only.
            meta = [
                r for r in meta
                if "auto_increment" not in r["extra"] and "GENERATED" not in r["extra"].upper()
            ]
        return meta
    if "information_schema.TABLES" in sql:
        table = params[1] if params else None
        return [{"c": 1 if table in META else 0}]
    if "information_schema.REFERENTIAL_CONSTRAINTS" in sql:
        return [dict(r) for r in FK_EDGES]
    if "SELECT REFERENCED_COLUMN_NAME" in sql:
        return [{"ref": "id"}]
    # Plain table SELECT: SELECT `a`,`b` FROM `table`
    match = re.search(r"FROM\s+`([^`]+)`", sql)
    table = match.group(1) if match else ""
    columns = re.findall(r"`([^`]+)`", sql.split("FROM")[0])
    rows = _rows_for(db, table)
    return [{c: row.get(c) for c in columns} for row in rows]


sync.run_query = fake_run_query  # type: ignore[assignment]


def _make_policy(table, mode, keys, ignore, fk_map=None):
    return cfg.TablePolicy(
        table=table,
        direction=cfg.DIR_TEST_TO_CORE,
        mode=mode,
        natural_keys=keys,
        ignore_columns=ignore,
        fk_map=fk_map or [],
    )


# --------------------------------------------------------------------------- #
# Checks
# --------------------------------------------------------------------------- #
def _plan_all(db_suffix: str):
    test_conn = FakeConn("test")
    core_conn = FakeConn(db_suffix)
    policies = [
        _make_policy("dunnage_types", cfg.MODE_UPSERT, ["type_name"], ["created_date"]),
        _make_policy(
            "dunnage_parts",
            cfg.MODE_UPSERT,
            ["part_id"],
            [],
            fk_map=[
                cfg.FkMap(column="type_id", references="dunnage_types", parent_key=["type_name"])
            ],
        ),
        _make_policy("dunnage_non_po_entries", cfg.MODE_INSERT_IF_MISSING, ["value"], ["use_count", "created_at"]),
    ]
    return sync.plan_all(test_conn, core_conn, "test", db_suffix, policies)


def _apply_to_core(plans):
    """Simulate executing the plan against the core (mutates CORE_* stores)."""
    for plan in plans:
        table = plan.policy.table
        store = CORE_TYPES if table == "dunnage_types" else (
            CORE_PARTS if table == "dunnage_parts" else CORE_NON_PO
        )
        for row in plan.inserts:
            new_id = (max(r["id"] for r in store) + 1) if store else 1
            full = dict(row)
            if "type_id" in full and full["type_id"] not in ("", None):
                # business space -> target id via dunnage_types.type_name
                tgt = next(
                    t for t in CORE_TYPES if t["type_name"] == full["type_id"]
                )
                full["type_id"] = tgt["id"]
            if "id" not in full:
                full["id"] = new_id
            store.append(full)
        for key_values, changes in plan.updates:
            # find row by natural key
            key_col = list(key_values)[0]
            for row in store:
                if row.get(key_col) == key_values[key_col]:
                    row.update(changes)
                    break


def run_self_tests() -> List[Dict[str, Any]]:
    results: List[Dict[str, Any]] = []
    plans = _plan_all("core")
    by_table = {p.policy.table: p for p in plans}

    def check(name: str, ok: bool, detail: str = ""):
        results.append({"name": name, "ok": bool(ok), "detail": detail})

    types = by_table["dunnage_types"]
    parts = by_table["dunnage_parts"]
    nonpo = by_table["dunnage_non_po_entries"]

    check(
        "parents ordered before children",
        list(by_table).index("dunnage_types") < list(by_table).index("dunnage_parts"),
    )
    check(
        "dunnage_types: +1 insert (Box added to core)",
        len(types.inserts) == 1 and types.inserts[0]["type_name"] == "Box",
        f"inserts={len(types.inserts)}",
    )
    check(
        "dunnage_types: Pallet unchanged",
        types.unchanged == 1,
        f"unchanged={types.unchanged}",
    )
    check(
        "dunnage_parts: +1 insert for 32x30",
        len(parts.inserts) == 1 and parts.inserts[0]["part_id"] == "32x30",
        f"inserts={len(parts.inserts)}",
    )
    check(
        "dunnage_parts: child FK carried as parent NATURAL KEY (Box), never an id",
        parts.inserts[0]["type_id"] == "Box",
        f"type_id={parts.inserts[0].get('type_id')!r}",
    )
    check(
        "dunnage_parts: core-only row COREONLY is preserved (never deleted or updated)",
        parts.unchanged == 1  # only 20x20 exists in both and is identical
        and all(r["part_id"] != "COREONLY" for r in parts.inserts)
        and all(kv["part_id"] != "COREONLY" for kv, _ in parts.updates),
        f"unchanged={parts.unchanged}",
    )
    check(
        "dunnage_non_po_entries: insert-if-missing only adds NewReason (Rework untouched)",
        len(nonpo.inserts) == 1 and nonpo.inserts[0]["value"] == "NewReason"
        and len(nonpo.updates) == 0 and nonpo.unchanged == 1,
        f"inserts={len(nonpo.inserts)} updates={len(nonpo.updates)} unchanged={nonpo.unchanged}",
    )

    # Union expectation: every natural key from core or test is in expected_rows.
    expected_parts_keys = {k[0] for k in parts.expected_rows}
    check(
        "dunnage_parts expected union contains core-only + test-only keys",
        {"COREONLY", "32x30", "20x20"} <= expected_parts_keys,
        f"keys={sorted(expected_parts_keys)}",
    )

    # Idempotency: simulate the execute on core, then re-plan -> zero changes.
    _apply_to_core(plans)
    replans = _plan_all("core")
    residual = [
        (p.policy.table, len(p.inserts), len(p.updates))
        for p in replans
        if p.inserts or p.updates
    ]
    check(
        "idempotent: second run changes 0 rows",
        not residual,
        str(residual),
    )
    return results


def main() -> int:
    print("MTM sync engine offline self-test\n")
    results = run_self_tests()
    all_ok = True
    for r in results:
        all_ok = all_ok and r["ok"]
        marker = "PASS" if r["ok"] else "FAIL"
        print(f"[{marker}] {r['name']}" + (f" - {r['detail']}" if r["detail"] else ""))
    print(f"\n{sum(1 for r in results if r['ok'])}/{len(results)} checks passed.")
    return 0 if all_ok else 1


if __name__ == "__main__":
    import sys

    sys.exit(main())
