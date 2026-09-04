"""Backup, restore, and disposable-copy helpers using the mysql client tools.

Passwords are passed to the CLI tools exclusively through the MYSQL_PWD
environment variable (never as a command-line argument) so they are not exposed
in process listings or logs.

The disposable copy (`mtm_receiving_application_backup_test`) is the only schema
this module may drop or create. A guard prevents ever dropping core or test.
"""

from __future__ import annotations

import datetime
import shutil
import subprocess
from pathlib import Path
from typing import Dict, List, Optional

from .connection import Login, run_query


def _require_cli(name: str) -> str:
    path = shutil.which(name)
    if not path:
        raise RuntimeError(
            f"The mysql client tool '{name}' was not found on PATH. It is required for "
            f"backup/restore/copy operations. Install MySQL client tools and retry."
        )
    return path


def _run_cli(command: List[str], login: Login) -> str:
    """Run a mysql/mysqldump command, returning stdout. Raises on failure."""
    proc = subprocess.run(
        command,
        capture_output=True,
        text=True,
        env=login.cli_environment(),
        check=False,
    )
    if proc.returncode != 0:
        stderr_tail = (proc.stderr or "").strip().splitlines()[-5:]
        raise RuntimeError(
            f"Command failed ({proc.returncode}): {' '.join(command[:4])}...\n"
            + "\n".join(stderr_tail)
        )
    return proc.stdout or ""


def _common_args(login: Login) -> List[str]:
    # Note: the password is deliberately NOT passed here; the CLI tools read it
    # from the MYSQL_PWD environment variable set in login.cli_environment().
    return [
        f"--host={login.host}",
        f"--port={login.port}",
        f"--user={login.user}",
    ]


def backup_filename(db_name: str, when: Optional[datetime.datetime] = None) -> str:
    when = when or datetime.datetime.now()
    return f"{db_name}_backup_{when.strftime('%Y%m%d_%H%M%S')}.sql"


def dump_database(login: Login, db_name: str, out_path: str) -> str:
    """mysqldump a database (data + routines + triggers + events) to a file."""
    out_file = Path(out_path)
    out_file.parent.mkdir(parents=True, exist_ok=True)
    mysqldump = _require_cli("mysqldump")
    command = (
        [mysqldump]
        + _common_args(login)
        + [
            "--single-transaction",
            "--routines",
            "--triggers",
            "--events",
            "--skip-add-locks",
            "--skip-lock-tables",
            "--default-character-set=utf8mb4",
        ]
        + [db_name]
    )
    with out_file.open("w", encoding="utf-8", newline="") as handle:
        proc = subprocess.run(
            command,
            stdout=handle,
            stderr=subprocess.PIPE,
            text=True,
            env=login.cli_environment(),
            check=False,
        )
    if proc.returncode != 0:
        stderr_tail = (proc.stderr or "").strip().splitlines()[-5:]
        raise RuntimeError(
            f"mysqldump failed for {db_name} ({proc.returncode}).\n"
            + "\n".join(stderr_tail)
        )
    return str(out_file)


def restore_database(login: Login, db_name: str, dump_path: str) -> None:
    """Restore a mysqldump file into an existing schema via the mysql client."""
    mysql = _require_cli("mysql")
    command = [mysql] + _common_args(login) + [db_name]
    with open(dump_path, "r", encoding="utf-8") as handle:
        proc = subprocess.run(
            command,
            stdin=handle,
            capture_output=True,
            text=True,
            env=login.cli_environment(),
            check=False,
        )
    if proc.returncode != 0:
        stderr_tail = (proc.stderr or "").strip().splitlines()[-8:]
        raise RuntimeError(
            f"Restore into {db_name} failed ({proc.returncode}).\n"
            + "\n".join(stderr_tail)
        )


def _assert_disposable_schema(name: str, protected: List[str], allowed: str) -> None:
    """Never allow drop/create on core, test, or any unconfigured schema."""
    if name != allowed or name in protected:
        raise RuntimeError(
            f"Refusing to (re)create or drop schema '{name}': only the configured "
            f"disposable copy '{allowed}' may be dropped/recreated by this tool."
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


def connect_no_db(login: Login):
    from .connection import connect

    return connect(login, database=None)


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
