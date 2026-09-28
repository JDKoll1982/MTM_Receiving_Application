"""Pure-Python logical backup / restore for MySQL (no external tools).

Backup/copy/restore operations previously shelled out to the mysql/mysqldump
CLI tools. This module replaces them with pymysql-only logic so the bundled
SyncTool executable needs nothing installed on the machine.

Scope covered:
  * base tables   - schema (SHOW CREATE TABLE) + row data (INSERTs)
  * views
  * stored routines (procedures and functions)
  * triggers
  * events

The generated file is standard, reviewable SQL that uses DELIMITER blocks for
routines/triggers/events, so it can also be inspected or piped through a mysql
client if one happens to be present.
"""

from __future__ import annotations

import datetime
import re
from pathlib import Path
from typing import Any, Dict, List, Optional, TextIO

from pymysql.converters import escape_item

from .connection import connect, run_query

#: Rows per INSERT statement (keeps files reasonable and under packet limits).
_BATCH_SIZE = 100


# --------------------------------------------------------------------------- #
# Small SQL helpers
# --------------------------------------------------------------------------- #
def _q(name: str) -> str:
    return "`" + str(name).replace("`", "``") + "`"


def _time_literal(td: datetime.timedelta) -> str:
    total = int(td.total_seconds())
    sign = "-" if total < 0 else ""
    total = abs(total)
    secs = total % 60
    mins = (total // 60) % 60
    hours = total // 3600
    return f"'{sign}{hours:02d}:{mins:02d}:{secs:02d}'"


def _literal(conn: Any, value: Any) -> str:
    """Render one value as a safe SQL literal."""
    if value is None:
        return "NULL"
    if isinstance(value, bytes):
        return "X'" + value.hex().upper() + "'"
    if isinstance(value, datetime.datetime):
        text = value.strftime("%Y-%m-%d %H:%M:%S")
        if value.microsecond:
            text += f".{value.microsecond:06d}"
        return f"'{text}'"
    if isinstance(value, datetime.date):
        return "'" + value.isoformat() + "'"
    if isinstance(value, datetime.time):
        text = value.isoformat()
        return "'" + text + "'"
    if isinstance(value, datetime.timedelta):
        return _time_literal(value)

    escaped = escape_item(value, conn.encoding)
    if isinstance(escaped, bytes):
        escaped = escaped.decode("utf-8")
    return str(escaped)


# --------------------------------------------------------------------------- #
# Object discovery
# --------------------------------------------------------------------------- #
def _list(conn: Any, sql: str, params: tuple) -> List[str]:
    return [r["n"] for r in run_query(conn, sql, params)]


def list_base_tables(conn: Any, db: str) -> List[str]:
    return _list(
        conn,
        "SELECT TABLE_NAME AS n FROM information_schema.TABLES "
        "WHERE TABLE_SCHEMA=%s AND TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME",
        (db,),
    )


def list_views(conn: Any, db: str) -> List[str]:
    return _list(
        conn,
        "SELECT TABLE_NAME AS n FROM information_schema.VIEWS "
        "WHERE TABLE_SCHEMA=%s ORDER BY TABLE_NAME",
        (db,),
    )


def list_routines(conn: Any, db: str) -> List[str]:
    """Return routine names in dependency-safe-ish order (functions first)."""
    rows = run_query(
        conn,
        "SELECT ROUTINE_NAME AS n, ROUTINE_TYPE AS t FROM information_schema.ROUTINES "
        "WHERE ROUTINE_SCHEMA=%s ORDER BY FIELD(t, 'FUNCTION', 'PROCEDURE'), ROUTINE_NAME",
        (db,),
    )
    return [f"{r['n']}|{r['t']}" for r in rows]


def list_events(conn: Any, db: str) -> List[str]:
    return _list(
        conn,
        "SELECT EVENT_NAME AS n FROM information_schema.EVENTS "
        "WHERE EVENT_SCHEMA=%s ORDER BY EVENT_NAME",
        (db,),
    )


def list_triggers(conn: Any, db: str) -> List[dict]:
    rows = run_query(
        conn,
        "SELECT TRIGGER_NAME, EVENT_MANIPULATION, EVENT_OBJECT_TABLE, "
        "       ACTION_TIMING, ACTION_ORIENTATION, ACTION_STATEMENT, DEFINER "
        "FROM information_schema.TRIGGERS "
        "WHERE TRIGGER_SCHEMA=%s ORDER BY TRIGGER_NAME",
        (db,),
    )
    return rows


def _order_tables(conn: Any, db: str, table_names: List[str]) -> List[str]:
    """Order tables so parents are created before their children."""
    rows = run_query(
        conn,
        "SELECT TABLE_NAME AS child, REFERENCED_TABLE_NAME AS parent "
        "FROM information_schema.KEY_COLUMN_USAGE "
        "WHERE TABLE_SCHEMA=%s AND REFERENCED_TABLE_NAME IS NOT NULL",
        (db,),
    )
    parents: Dict[str, set] = {}
    for row in rows:
        parents.setdefault(row["child"], set()).add(row["parent"])

    remaining = set(table_names)
    ordered: List[str] = []
    while remaining:
        progressed = False
        for name in sorted(remaining):
            deps = {
                parent
                for parent in parents.get(name, ())
                if parent in remaining and parent != name
            }
            if not deps:
                ordered.append(name)
                remaining.discard(name)
                progressed = True
        if not progressed:
            # Cyclic / self-referential leftovers - emit them as-is.
            ordered.extend(sorted(remaining))
            break
    return ordered


# --------------------------------------------------------------------------- #
# SHOW CREATE helpers
# --------------------------------------------------------------------------- #
def _show_create(conn: Any, statement: str, key: str) -> str:
    rows = run_query(conn, statement)
    if not rows:
        raise RuntimeError(f"Expected a result from: {statement}")
    definition = rows[0].get(key)
    if not definition:
        raise RuntimeError(
            f"SHOW CREATE returned no '{key}' (insufficient privileges or object missing)."
        )
    return str(definition).rstrip()


def _definer(userhost: Any) -> str:
    text = str(userhost or "")
    if "@" in text:
        user, _, host = text.rpartition("@")
    else:
        user, host = text, "%"
    return f"{_q(user)}@{_q(host)}"


# --------------------------------------------------------------------------- #
# Dump writers
# --------------------------------------------------------------------------- #
def _drop_table(handle: TextIO, table: str) -> None:
    handle.write(f"DROP TABLE IF EXISTS {_q(table)};\n")


def _generated_columns(conn: Any, db: str, table: str):
    """Names of generated (virtual/stored) columns; they cannot receive values."""
    rows = run_query(
        conn,
        "SELECT COLUMN_NAME AS n FROM information_schema.COLUMNS "
        "WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s "
        "AND EXTRA REGEXP 'VIRTUAL GENERATED|STORED GENERATED'",
        (db, table),
    )
    return {row["n"] for row in rows}


def _write_table(conn: Any, db: str, table: str, handle: TextIO) -> None:
    handle.write(f"\n--\n-- Table `{table}`\n--\n")
    handle.write(_show_create(conn, f"SHOW CREATE TABLE {_q(db)}.{_q(table)}", "Create Table"))
    handle.write(";\n")

    generated = _generated_columns(conn, db, table)
    handle.write(f"SET @saved_sql_mode = @@SQL_MODE;\n")
    columns_sql: Optional[str] = None
    batch: List[str] = []
    with conn.cursor() as cursor:
        cursor.execute(f"SELECT * FROM {_q(db)}.{_q(table)}")
        columns = [
            column[0] for column in cursor.description if column[0] not in generated
        ]
        if columns:
            columns_sql = ", ".join(_q(column) for column in columns)
            for row in cursor:
                values = ", ".join(_literal(conn, row[column]) for column in columns)
                batch.append(f"({values})")
                if len(batch) >= _BATCH_SIZE:
                    _flush_inserts(handle, table, columns_sql, batch)
    _flush_inserts(handle, table, columns_sql, batch)
    handle.write(f"SET SQL_MODE = @saved_sql_mode;\n")


def _flush_inserts(
    handle: TextIO, table: str, columns_sql: Optional[str], batch: List[str]
) -> None:
    if not batch or not columns_sql:
        batch.clear()
        return
    handle.write(
        f"INSERT INTO {_q(table)} ({columns_sql}) VALUES\n"
        + ",\n".join(batch)
        + ";\n"
    )
    batch.clear()


def _write_view(conn: Any, db: str, view: str, handle: TextIO) -> None:
    handle.write(f"\n--\n-- View `{view}`\n--\n")
    handle.write(f"DROP VIEW IF EXISTS {_q(view)};\n")
    definition = _show_create(conn, f"SHOW CREATE VIEW {_q(db)}.{_q(view)}", "Create View")
    handle.write(definition)
    if not definition.rstrip().endswith(";"):
        handle.write(";")
    handle.write("\n")


def _write_routine(conn: Any, db: str, entry: str, handle: TextIO) -> None:
    name, rtype = entry.split("|", 1)
    keyword = "PROCEDURE" if rtype == "PROCEDURE" else "FUNCTION"
    key = "Create Procedure" if rtype == "PROCEDURE" else "Create Function"
    definition = _show_create(
        conn, f"SHOW CREATE {keyword} {_q(db)}.{_q(name)}", key
    )
    handle.write(f"\n--\n-- {keyword} `{name}`\n--\n")
    handle.write("DELIMITER ;;\n")
    handle.write(f"DROP {keyword} IF EXISTS {_q(name)};;\n")
    handle.write(definition)
    handle.write(";;\nDELIMITER ;\n")


def _write_trigger(conn: Any, db: str, trigger: dict, handle: TextIO) -> None:
    name = trigger["TRIGGER_NAME"]
    handle.write(f"\n--\n-- Trigger `{name}`\n--\n")
    handle.write("DELIMITER ;;\n")
    handle.write(f"DROP TRIGGER IF EXISTS {_q(name)};;\n")
    handle.write(
        f"CREATE DEFINER={_definer(trigger.get('DEFINER'))} "
        f"TRIGGER {_q(name)} {trigger['ACTION_TIMING']} {trigger['EVENT_MANIPULATION']} "
        f"ON {_q(trigger['EVENT_OBJECT_TABLE'])} FOR EACH ROW "
        f"{trigger['ACTION_STATEMENT']};;\n"
    )
    handle.write("DELIMITER ;\n")


def _write_event(conn: Any, db: str, event: str, handle: TextIO) -> None:
    handle.write(f"\n--\n-- Event `{event}`\n--\n")
    handle.write("DELIMITER ;;\n")
    handle.write(f"DROP EVENT IF EXISTS {_q(event)};;\n")
    definition = _show_create(conn, f"SHOW CREATE EVENT {_q(db)}.{_q(event)}", "Create Event")
    handle.write(definition)
    if not definition.rstrip().endswith(";;"):
        if definition.rstrip().endswith(";"):
            handle.write(";")
        else:
            handle.write(";;")
    handle.write("\nDELIMITER ;\n")


# --------------------------------------------------------------------------- #
# Top-level dump / restore
# --------------------------------------------------------------------------- #
def write_dump(conn: Any, db: str, handle: TextIO) -> None:
    """Write a full logical dump of `db` to the open text handle."""
    handle.write(
        f"-- MTM pure-Python logical dump of `{db}`\n"
        f"-- Generated without the mysql/mysqldump client tools.\n"
        "SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;\n"
        "SET @OLD_SQL_MODE=@@SQL_MODE;\n"
        "SET NAMES utf8mb4;\n"
    )

    base_tables = _order_tables(conn, db, list_base_tables(conn, db))

    # Drop tables children-first so restoring into an existing schema is safe.
    for table in reversed(base_tables):
        _drop_table(handle, table)

    for table in base_tables:
        _write_table(conn, db, table, handle)
    for view in list_views(conn, db):
        _write_view(conn, db, view, handle)
    for entry in list_routines(conn, db):
        _write_routine(conn, db, entry, handle)
    for trigger in list_triggers(conn, db):
        _write_trigger(conn, db, trigger, handle)
    for event in list_events(conn, db):
        _write_event(conn, db, event, handle)

    handle.write(
        "SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;\n"
        "SET SQL_MODE=@OLD_SQL_MODE;\n"
    )


# --------------------------------------------------------------------------- #
# Restore (statement loader)
# --------------------------------------------------------------------------- #
def _split_sql_statements(sql_text: str) -> List[str]:
    """Split SQL text into statements honouring DELIMITER and quoting/comments."""
    statements: List[str] = []
    delimiter = ";"
    buffer: List[str] = []
    n = len(sql_text)
    i = 0
    statement_start = True

    while i < n:
        # DELIMITER commands only appear at a statement boundary.
        if statement_start:
            rest = sql_text[i:]
            match = re.match(r"(?is)^(\s*)DELIMITER\s+(\S+)", rest)
            if match:
                stripped = len(match.group(1))
                i += stripped + len(match.group(0)) - stripped
                delimiter = match.group(2)
                statement_start = True
                continue

        char = sql_text[i]

        # Comments
        if char == "-" and i + 1 < n and sql_text[i + 1] == "-":
            end = sql_text.find("\n", i + 2)
            i = n if end == -1 else end + 1
            continue
        if char == "#":
            end = sql_text.find("\n", i + 1)
            i = n if end == -1 else end + 1
            continue
        if char == "/" and i + 1 < n and sql_text[i + 1] == "*":
            end = sql_text.find("*/", i + 2)
            i = n if end == -1 else end + 2
            continue

        # Quoted strings / identifiers
        if char in ("'", '"', "`"):
            quote = char
            buffer.append(char)
            i += 1
            while i < n:
                current = sql_text[i]
                buffer.append(current)
                if current == "\\" and i + 1 < n:
                    buffer.append(sql_text[i + 1])
                    i += 2
                    continue
                if current == quote:
                    i += 1
                    break
                i += 1
            continue

        # Statement terminator
        if delimiter and sql_text.startswith(delimiter, i):
            statement = "".join(buffer).strip()
            if statement:
                statements.append(statement)
            buffer = []
            i += len(delimiter)
            statement_start = True
            continue

        if not char.isspace():
            statement_start = False

        buffer.append(char)
        i += 1

    tail = "".join(buffer).strip()
    if tail:
        statements.append(tail)
    return statements


def load_dump(conn: Any, dump_path: str) -> None:
    """Restore a logical dump (written by write_dump) into the connected schema."""
    text = Path(dump_path).read_text(encoding="utf-8")
    statements = _split_sql_statements(text)

    if not statements:
        raise RuntimeError(f"Dump file contained no statements: {dump_path}")

    executed = 0
    for statement in statements:
        try:
            with conn.cursor() as cursor:
                cursor.execute(statement)
            executed += 1
        except Exception as exc:  # surface the offending statement for diagnosis
            snippet = " ".join(statement.split())[:200]
            raise RuntimeError(
                f"Restore failed near: {snippet}\nError: {exc}"
            ) from exc
        conn.commit()

    return None
