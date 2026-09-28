"""Connection and secrets handling for the MTM reference-data sync.

Safety rules implemented here (see .github/instructions/database/
db-script-execution-standards.instructions.md):

* Secrets are never hardcoded or committed.
* The password is always entered via a hidden prompt (getpass) unless it is
  already present in the environment / provided on the command line for CI.
* In non-interactive runs every value must come from the environment or CLI
  arguments; the tool fails fast instead of guessing or using empty secrets.
* Passwords are only ever passed to subprocesses through the MYSQL_PWD
  environment variable, never as command-line arguments.
"""

from __future__ import annotations

import dataclasses
import datetime
import decimal
import getpass
import os
import sys
from typing import Any, Dict, List, Optional, Sequence

_ENV_HOST = ("MTM_SYNC_HOST",)
_ENV_PORT = ("MTM_SYNC_PORT",)
_ENV_USER = ("MTM_SYNC_USER",)
_ENV_PASSWORD = ("MTM_SYNC_PASSWORD", "MYSQL_PWD")
_ENV_NON_INTERACTIVE = "MTM_SYNC_NON_INTERACTIVE"


def _env_first(names: Sequence[str]) -> Optional[str]:
    for name in names:
        value = os.environ.get(name)
        if value:
            return value
    return None


@dataclasses.dataclass
class Login:
    """MySQL login credentials plus the host/port they belong to."""

    host: str
    port: int
    user: str
    password: str

    def cli_environment(self) -> Dict[str, str]:
        """Environment for mysql/mysqldump subprocesses (no argv secret)."""
        env = dict(os.environ)
        env["MYSQL_PWD"] = self.password
        return env

    def redacted(self) -> str:
        return f"{self.user}@{self.host}:{self.port}"


def _is_non_interactive(flag: Optional[bool]) -> bool:
    if flag is not None:
        return flag
    if _env_first((_ENV_NON_INTERACTIVE,)) in ("1", "true", "yes"):
        return True
    try:
        return not sys.stdin.isatty()
    except Exception:
        return True


def resolve_login(
    host_arg: Optional[str] = None,
    port_arg: Optional[int] = None,
    user_arg: Optional[str] = None,
    password_arg: Optional[str] = None,
    non_interactive: Optional[bool] = None,
) -> Login:
    """Resolve connection credentials from CLI args, env, then interactive prompts.

    Raises RuntimeError with a clear message when a required value is missing in
    non-interactive mode so CI fails fast instead of guessing.
    """
    non_interactive = _is_non_interactive(non_interactive)

    host = host_arg or _env_first(_ENV_HOST)
    port_value = port_arg or _env_first(_ENV_PORT)
    port = int(port_value) if port_value else 3306
    user = user_arg or _env_first(_ENV_USER)
    password = password_arg or _env_first(_ENV_PASSWORD)

    if non_interactive:
        missing = []
        if not host:
            missing.append("host (--host or MTM_SYNC_HOST)")
        if not user:
            missing.append("user (--user or MTM_SYNC_USER)")
        if not password:
            missing.append("password (MTM_SYNC_PASSWORD or MYSQL_PWD)")
        if missing:
            raise RuntimeError(
                "Non-interactive run missing required connection values: "
                + ", ".join(missing)
                + ". Provide them via environment variables or CLI args; never guess credentials."
            )
        return Login(host=host, port=port, user=user or "", password=password or "")

    # Interactive: prompt for whatever is not already supplied.
    if not host:
        host = input("MySQL host [localhost]: ").strip() or "localhost"
    if not user:
        user = input("MySQL user: ").strip()
    if not password:
        password = getpass.getpass(f"MySQL password for {user}@{host}: ")
    return Login(host=host, port=port, user=user, password=password)


def connect(login: Login, database: Optional[str] = None):
    """Open a pymysql connection (lazy import). database may be None (server-level)."""
    try:
        import pymysql
        import pymysql.cursors
    except ImportError as exc:  # pragma: no cover - exercised only without deps
        raise RuntimeError(
            "pymysql is required. Install dependencies with: "
            "pip install -r Database/SyncTool/requirements.txt"
        ) from exc
    return pymysql.connect(
        host=login.host,
        port=login.port,
        user=login.user,
        password=login.password,
        database=database,
        charset="utf8mb4",
        autocommit=False,
        cursorclass=pymysql.cursors.DictCursor,
    )


def run_query(conn, sql: str, params: Optional[Sequence[Any]] = None) -> List[Dict[str, Any]]:
    """Execute a read-only SELECT and return all rows as dicts."""
    with conn.cursor() as cursor:
        cursor.execute(sql, params)
        return list(cursor.fetchall())


def run_dml(conn, sql: str, params: Optional[Sequence[Any]] = None) -> int:
    """Execute a DML statement and return affected row count."""
    with conn.cursor() as cursor:
        cursor.execute(sql, params)
        return cursor.rowcount


def quote_ident(name: str) -> str:
    """Quote a MySQL identifier with backticks."""
    return "`" + str(name).replace("`", "``") + "`"


def _escape_string(value: str) -> str:
    """Escape a string for inclusion inside a single-quoted MySQL literal."""
    out: List[str] = []
    for ch in value:
        if ch == "\\":
            out.append("\\\\")
        elif ch == "'":
            out.append("\\'")
        elif ch == '"':
            out.append('\\"')
        elif ch == "\x00":
            out.append("\\0")
        elif ch == "\n":
            out.append("\\n")
        elif ch == "\r":
            out.append("\\r")
        elif ch == "\x1a":
            out.append("\\Z")
        else:
            out.append(ch)
    return "".join(out)


def format_literal(value: Any) -> str:
    """Render a Python value as a safe MySQL literal (for --generate-sql)."""
    if value is None:
        return "NULL"
    if isinstance(value, bool):
        return "1" if value else "0"
    if isinstance(value, (int, decimal.Decimal)):
        return str(value)
    if isinstance(value, float):
        return repr(value)
    if isinstance(value, datetime.datetime):
        return "'" + value.strftime("%Y-%m-%d %H:%M:%S") + "'"
    if isinstance(value, datetime.date):
        return "'" + value.isoformat() + "'"
    if isinstance(value, datetime.time):
        return "'" + value.isoformat() + "'"
    if isinstance(value, bytes):
        return "'" + _escape_string(value.decode("latin-1", "replace")) + "'"
    return "'" + _escape_string(str(value)) + "'"
