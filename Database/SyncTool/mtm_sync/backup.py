"""Backup, restore, and copy helpers.

Backup/copy/restore are implemented in pure Python with pymysql (see
`logical_dump`), so the bundled SyncTool executable does not need the
mysql/mysqldump CLI tools installed on the machine. Passwords are never written
to disk or logs.

Two schemas may be dropped/recreated by this module - never core or test:
  * the disposable verification copy (mtm_receiving_application_backup_test)
  * the live-backup snapshot (mtm_receiving_application_backup)
"""

from __future__ import annotations

import datetime
import os
from pathlib import Path
from typing import Dict, List, Optional

from . import logical_dump
from .connection import Login, connect, run_query


def backup_filename(db_name: str, when: Optional[datetime.datetime] = None) -> str:
    when = when or datetime.datetime.now()
    return f"{db_name}_backup_{when.strftime('%Y%m%d_%H%M%S')}.sql"


def dump_database(login: Login, db_name: str, out_path: str) -> str:
    """Write a pure-Python logical dump (data + routines + triggers + events)."""
    out_file = Path(out_path)
    out_file.parent.mkdir(parents=True, exist_ok=True)
    connection = connect(login, db_name)
    try:
        with out_file.open("w", encoding="utf-8", newline="") as handle:
            logical_dump.write_dump(connection, db_name, handle)
    finally:
        connection.close()
    return str(out_file)


def restore_database(login: Login, db_name: str, dump_path: str) -> None:
    """Restore a logical dump file into the given (existing) schema."""
    connection = connect(login, db_name)
    try:
        logical_dump.load_dump(connection, dump_path)
    finally:
        connection.close()

def _assert_disposable_schema(name: str, protected: List[str], allowed: str) -> None:
    """Never allow drop/create on core, test, or any unconfigured schema."""
    if name != allowed or name in protected:
        raise RuntimeError(
            f"Refusing to (re)create or drop schema '{name}': only the configured "
            f"clone/backup target '{allowed}' may be dropped/recreated by this tool."
        )


def make_disposable_copy(
    login: Login,
    core_db: str,
    backup_db: str,
    dump_dir: str,
    protected: Optional[List[str]] = None,
) -> str:
    """Create the disposable verification copy from the core database.

    Returns the path of the dump used to build it.
    """
    _assert_disposable_schema(backup_db, protected or [], backup_db)
    server_conn = connect_no_db(login)
    try:
        run_ddl(server_conn, f"DROP DATABASE IF EXISTS `{backup_db}`")
        run_ddl(server_conn, f"CREATE DATABASE `{backup_db}` "
                             f"CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci")
    finally:
        server_conn.close()

    dump_path = dump_database(login, core_db, str(Path(dump_dir) / backup_filename(core_db)))
    restore_database(login, backup_db, dump_path)
    return dump_path


def drop_disposable_copy(
    login: Login,
    backup_db: str,
    protected: Optional[List[str]] = None,
) -> None:
    """Drop the disposable verification copy."""
    _assert_disposable_schema(backup_db, protected or [], backup_db)
    server_conn = connect_no_db(login)
    try:
        run_ddl(server_conn, f"DROP DATABASE IF EXISTS `{backup_db}`")
    finally:
        server_conn.close()


def make_live_backup(
    login: Login,
    core_db: str,
    live_backup_db: str,
    dump_dir: str,
    protected: Optional[List[str]] = None,
) -> str:
    """Snapshot the live core database into the live-backup schema.

    Drops/recreates `live_backup_db` (never core/test or the disposable copy) and
    copies the entire core database into it using the pure-Python logical copy.
    Returns the (deleted) temp dump path used to build the copy.
    """
    _assert_disposable_schema(live_backup_db, protected or [], live_backup_db)
    server_conn = connect_no_db(login)
    try:
        run_ddl(server_conn, f"DROP DATABASE IF EXISTS `{live_backup_db}`")
        run_ddl(
            server_conn,
            f"CREATE DATABASE `{live_backup_db}` "
            f"CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci",
        )
    finally:
        server_conn.close()

    dump_path = dump_database(
        login, core_db, str(Path(dump_dir) / backup_filename(core_db))
    )
    try:
        restore_database(login, live_backup_db, dump_path)
    finally:
        _remove_if_present(dump_path)
    return dump_path


def restore_core_from_live_backup(
    login: Login,
    core_db: str,
    live_backup_db: str,
    dump_dir: str,
    protected: Optional[List[str]] = None,
) -> None:
    """Roll the live core database back from the live-backup snapshot.

    Used to restore core after a failed live update. The live-backup schema is
    read (never dropped); its tables replace the core tables in place.
    """
    _assert_disposable_schema(live_backup_db, protected or [], live_backup_db)
    dump_path = dump_database(
        login,
        live_backup_db,
        str(Path(dump_dir) / backup_filename(live_backup_db)),
    )
    try:
        restore_database(login, core_db, dump_path)
    finally:
        _remove_if_present(dump_path)


def _remove_if_present(path: str) -> None:
    try:
        os.remove(path)
    except OSError:
        pass


def connect_no_db(login: Login):
    from .connection import connect

    return connect(login, database=None)


def database_exists(login: Login, db_name: str) -> bool:
    """True when the given schema exists on the server."""
    conn = connect_no_db(login)
    try:
        rows = run_query(
            conn,
            "SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = %s",
            (db_name,),
        )
        return bool(rows)
    finally:
        conn.close()


def run_ddl(conn, sql: str) -> None:
    with conn.cursor() as cursor:
        cursor.execute(sql)
    conn.commit()


def table_rowcount(conn, db_name: str, table: str) -> int:
    rows = run_query(conn, f"SELECT COUNT(*) AS c FROM `{db_name}`.`{table}`")
    return int(rows[0]["c"])


def table_checksum(conn, db_name: str, table: str) -> Optional[int]:
    """CHECKSUM TABLE value (None when the table does not exist)."""
    rows = run_query(conn, f"CHECKSUM TABLE `{db_name}`.`{table}`")
    if not rows:
        return None
    return rows[0].get("Checksum")


def compare_checksum(conn_a, conn_b, db_a: str, db_b: str, table: str) -> Dict[str, object]:
    """Compare one table between two schemas by checksum + row count."""
    try:
        count_a = table_rowcount(conn_a, db_a, table)
    except Exception:
        return {"table": table, "ok": False, "detail": f"missing in {db_a}"}
    try:
        count_b = table_rowcount(conn_b, db_b, table)
    except Exception:
        return {"table": table, "ok": False, "detail": f"missing in {db_b}"}
    checksum_a = table_checksum(conn_a, db_a, table)
    checksum_b = table_checksum(conn_b, db_b, table)
    ok = count_a == count_b and checksum_a == checksum_b
    detail = "" if ok else f"count {count_a} vs {count_b}; checksum {checksum_a} vs {checksum_b}"
    return {"table": table, "ok": ok, "detail": detail}
