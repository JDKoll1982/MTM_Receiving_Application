"""Command-line entry point for the MTM reference-data sync.

Modes (read-only by default):
  inspect       drift report + data-sync what-if summary (read-only)
  whatif        data-sync plan summary only (read-only)
  generate-sql  write the ordered, reviewable SQL file (no writes)
  execute       apply the sync to a target (core requires backup + confirm)
  make-copy     create the disposable verification copy from core
  verify-copy   run the full disposable-copy verification workflow
  drop-copy     drop the disposable verification copy
  restore       restore a previously taken backup (destructive; --confirm)
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import List, Optional

from . import __version__
from . import backup, schema as schema_mod, sync as sync_mod, verify as verify_mod
from .config import SyncConfig, default_config_path, load_config
from .connection import Login, connect, resolve_login

TOOL_DIR = Path(__file__).resolve().parent.parent
DEFAULT_BACKUP_DIR = TOOL_DIR / "backups"

TARGET_CHOICES = ["core", "test", "backup_test"]


def _target_db(target: str, config: SyncConfig) -> str:
    if target == "core":
        return config.core_db
    if target == "test":
        return config.test_db
    if target == "backup_test":
        return config.backup_db
    raise ValueError(f"Unknown target: {target}")


def _add_common_args(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--config", default=str(default_config_path()),
                        help="Path to the YAML policy file (default: sync_policy.yml next to tool).")
    parser.add_argument("--host", help="MySQL host (or env MTM_SYNC_HOST).")
    parser.add_argument("--port", type=int, help="MySQL port (or env MTM_SYNC_PORT).")
    parser.add_argument("--user", help="MySQL user (or env MTM_SYNC_USER).")
    parser.add_argument("--non-interactive", action="store_true",
                        help="Never prompt; require env/args. Password must be in MTM_SYNC_PASSWORD "
                             "or MYSQL_PWD.")
    parser.add_argument("--core-db", help="Override core database name.")
    parser.add_argument("--test-db", help="Override test database name.")
    parser.add_argument("--backup-db", help="Override disposable copy database name.")
    parser.add_argument("--backup-dir", default=str(DEFAULT_BACKUP_DIR),
                        help=f"Directory for backups (default: {DEFAULT_BACKUP_DIR}).")


def _make_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="mtm-sync",
        description="Synchronize selected reference/seed data from the test MySQL database to "
                    "core, and report schema/routine/trigger drift. Read-only unless an explicit "
                    "execution mode is requested.",
        epilog="Secrets are never read from the command line. Provide MTM_SYNC_PASSWORD (or "
               "MYSQL_PWD) for non-interactive runs; interactive runs prompt with a hidden input.",
    )
    parser.add_argument("--version", action="version", version=f"mtm-sync {__version__}")
    sub = parser.add_subparsers(dest="command", required=True)

    p_inspect = sub.add_parser("inspect", help="Drift report + sync what-if (read-only).")
    _add_common_args(p_inspect)
    p_inspect.add_argument("--target", choices=TARGET_CHOICES, default="core",
                           help="Database to compare/sync into (default: core).")

    p_whatif = sub.add_parser("whatif", help="Data-sync plan summary (read-only).")
    _add_common_args(p_whatif)
    p_whatif.add_argument("--target", choices=TARGET_CHOICES, default="core",
                          help="Database to sync into (default: core).")

    p_gen = sub.add_parser("generate-sql",
                           help="Write the ordered, reviewable SQL file (no writes).")
    _add_common_args(p_gen)
    p_gen.add_argument("--target", choices=TARGET_CHOICES, default="core",
                       help="Database the SQL would sync into (default: core).")
    p_gen.add_argument("output", help="Output .sql file path.")

    p_exec = sub.add_parser("execute", help="Apply the sync to a target database.")
    _add_common_args(p_exec)
    p_exec.add_argument("--target", choices=TARGET_CHOICES, default="core",
                        help="Database to sync into (default: core).")
    p_exec.add_argument("--confirm", action="store_true",
                        help="Acknowledge the destructive action (required for live core).")
    p_exec.add_argument("--skip-validation", action="store_true",
                        help="Do not re-plan/validate after executing.")

    p_make = sub.add_parser("make-copy", help="Create the disposable verification copy from core.")
    _add_common_args(p_make)

    p_drop = sub.add_parser("drop-copy", help="Drop the disposable verification copy.")
    _add_common_args(p_drop)

    p_verify = sub.add_parser("verify-copy",
                              help="Run the full disposable-copy verification workflow.")
    _add_common_args(p_verify)
    p_verify.add_argument("--keep-copy", action="store_true",
                          help="Do not drop the disposable copy at the end.")

    p_restore = sub.add_parser("restore", help="Restore a backup file into a target schema.")
    _add_common_args(p_restore)
    p_restore.add_argument("--target", choices=TARGET_CHOICES, default="core",
                           help="Database to restore into (default: core).")
    p_restore.add_argument("--confirm", action="store_true",
                           help="Acknowledge the destructive restore.")
    p_restore.add_argument("dump_file", help="Path to the .sql backup file to restore.")

    return parser


def _config_from_args(args) -> SyncConfig:
    config = load_config(args.config)
    if getattr(args, "core_db", None):
        config.core_db = args.core_db
    if getattr(args, "test_db", None):
        config.test_db = args.test_db
    if getattr(args, "backup_db", None):
        config.backup_db = args.backup_db
    return config


def _login_from_args(args) -> Login:
    return resolve_login(
        host_arg=args.host,
        port_arg=args.port,
        user_arg=args.user,
        password_arg=None,  # secrets never accepted on the command line
        non_interactive=args.non_interactive or None,
    )


def _print_drift_and_plan(test_conn, core_conn, config: SyncConfig) -> List[sync_mod.TablePlan]:
    test_snap = schema_mod.fetch_schema_snapshot(test_conn, config.test_db)
    core_snap = schema_mod.fetch_schema_snapshot(core_conn, config.core_db)
    entries = schema_mod.compare_schemas(test_snap, core_snap)
    schema_mod.print_report(entries, config.test_db, config.core_db)

    print("\nData sync (test -> core) what-if:")
    plans = sync_mod.plan_all(test_conn, core_conn, config.test_db, config.core_db,
                              config.active_policies)
    sync_mod.print_plan_summary(plans)
    return plans


def cmd_inspect(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    test_conn = connect(login, config.test_db)
    core_conn = connect(login, config.core_db)
    try:
        _print_drift_and_plan(test_conn, core_conn, config)
    finally:
        test_conn.close()
        core_conn.close()
    return 0


def cmd_whatif(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    target_db = _target_db(args.target, config)
    if target_db == config.test_db:
        print("Target equals the source (test). Nothing to sync.")
        return 0
    source_db = config.test_db
    source_conn = connect(login, source_db)
    target_conn = connect(login, target_db)
    try:
        plans = sync_mod.plan_all(source_conn, target_conn, source_db, target_db,
                                  config.active_policies)
        sync_mod.print_plan_summary(plans)
    finally:
        source_conn.close()
        target_conn.close()
    return 0


def cmd_generate_sql(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    target_db = _target_db(args.target, config)
    if target_db == config.test_db:
        print("Target equals the source (test). Nothing to sync.")
        return 0
    source_db = config.test_db
    source_conn = connect(login, source_db)
    target_conn = connect(login, target_db)
    try:
        plans = sync_mod.plan_all(source_conn, target_conn, source_db, target_db,
                                  config.active_policies)
        sync_mod.print_plan_summary(plans)
        sql = sync_mod.render_sql_file(plans, source_db, target_db, args.config)
        out = Path(args.output)
        out.parent.mkdir(parents=True, exist_ok=True)
        out.write_text(sql, encoding="utf-8")
        print(f"\nGenerated review SQL written to: {out}")
        print("No database writes were performed.")
    finally:
        source_conn.close()
        target_conn.close()
    return 0


def _require_confirm(args, message: str) -> bool:
    if args.confirm:
        return True
    if args.non_interactive:
        raise RuntimeError(f"--confirm is required for this action in non-interactive mode: "
                           f"{message}")
    try:
        answer = input(f"{message} (type 'yes' to continue): ").strip().lower()
    except EOFError:
        return False
    return answer == "yes"


def cmd_execute(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    target_db = _target_db(args.target, config)
    source_db = config.test_db

    if target_db == source_db:
        print("Target equals the source (test). Nothing to sync.")
        return 0

    is_core = target_db == config.core_db

    # Core requires a fresh full snapshot into the live-backup schema first.
    if is_core:
        protected = [config.core_db, config.test_db, config.backup_db]
        backup.make_live_backup(
            login,
            config.core_db,
            config.live_backup_db,
            dump_dir=args.backup_dir,
            protected=protected,
        )
        print(f"Live core database backed up to {config.live_backup_db}.")
        if not _require_confirm(
            args,
            f"This will write reference data into the LIVE core database {target_db}.",
        ):
            print(
                "Aborted by operator. No writes performed. "
                f"The backup copy {config.live_backup_db} is kept for reference."
            )
            return 2

    # The disposable copy must exist as the sync target. Bootstrap it fresh from
    # core when it is missing (e.g., after drop-copy or a fresh machine) so the
    # run never dies with "Unknown database '<copy>'" (error 1049).
    if target_db == config.backup_db and not backup.database_exists(login, target_db):
        print(
            f"Disposable copy {target_db} does not exist; creating it from "
            f"{config.core_db} first ..."
        )
        backup.make_disposable_copy(
            login,
            config.core_db,
            config.backup_db,
            args.backup_dir,
            protected=[config.core_db, config.test_db],
        )
        print(f"Disposable copy {target_db} ready.")

    source_conn = connect(login, source_db)
    target_conn = connect(login, target_db)
    try:
        print(f"Planning sync {source_db} -> {target_db} ...")
        plans = sync_mod.plan_all(source_conn, target_conn, source_db, target_db,
                                  config.active_policies)
        sync_mod.print_plan_summary(plans)

        print("\nExecuting ...")
        changed = sync_mod.execute_plans(target_conn, plans)
        for table, count in changed.items():
            print(f"  {table}: {count} row(s) changed")
        print("Committed.")

        if not args.skip_validation:
            print("\nValidating after execution (idempotency) ...")
            residual = sync_mod.residual_changes(source_conn, target_conn, source_db,
                                                 target_db, config.active_policies)
            if residual:
                detail = "; ".join(f"{t}: +{i} insert, {u} update" for t, i, u in residual)
                print(f"!!! VALIDATION MISMATCH - residual changes remain: {detail}")
                return 1
            print("Validation OK: a second run would change 0 rows.")
        return 0
    except Exception as exc:
        # Transaction rolled back inside execute_plans. On core, roll back from the
        # live-backup schema so the live database is restored to its pre-update state.
        if is_core:
            print(f"\nERROR: {exc}")
            print(
                f"Aborting and restoring core from live backup "
                f"{config.live_backup_db} ..."
            )
            try:
                backup.restore_core_from_live_backup(
                    login,
                    config.core_db,
                    config.live_backup_db,
                    dump_dir=args.backup_dir,
                    protected=[config.core_db, config.test_db, config.backup_db],
                )
                print("Restore complete.")
            except Exception as restore_exc:
                print(f"RESTORE FAILED - manual intervention required: {restore_exc}")
                return 3
        else:
            print(f"\nERROR: {exc}")
            print("No writes committed (rolled back).")
        return 1
    finally:
        source_conn.close()
        target_conn.close()


def cmd_make_copy(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    dump_path = backup.make_disposable_copy(
        login, config.core_db, config.backup_db, args.backup_dir,
        protected=[config.core_db, config.test_db],
    )
    print(f"Disposable copy {config.backup_db} created from {config.core_db}.")
    print(f"Dump used: {dump_path}")
    return 0


def cmd_drop_copy(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    backup.drop_disposable_copy(login, config.backup_db,
                                protected=[config.core_db, config.test_db])
    print(f"Dropped disposable copy {config.backup_db}.")
    return 0


def cmd_verify_copy(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    ok = verify_mod.verify_on_copy(
        login, config, args.backup_dir, keep_copy=args.keep_copy
    )
    return 0 if ok else 1


def cmd_restore(args) -> int:
    config = _config_from_args(args)
    login = _login_from_args(args)
    target_db = _target_db(args.target, config)
    if not _require_confirm(
        args,
        f"This RESTORES {Path(args.dump_file).name} into {target_db}, replacing its tables.",
    ):
        print("Aborted by operator. No restore performed.")
        return 2
    backup.restore_database(login, target_db, args.dump_file)
    print(f"Restored {args.dump_file} into {target_db}.")
    return 0


def main(argv: Optional[List[str]] = None) -> int:
    parser = _make_parser()
    args = parser.parse_args(argv)
    handlers = {
        "inspect": cmd_inspect,
        "whatif": cmd_whatif,
        "generate-sql": cmd_generate_sql,
        "execute": cmd_execute,
        "make-copy": cmd_make_copy,
        "drop-copy": cmd_drop_copy,
        "verify-copy": cmd_verify_copy,
        "restore": cmd_restore,
    }
    try:
        return handlers[args.command](args)
    except (RuntimeError, ValueError, FileNotFoundError) as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
