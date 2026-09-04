"""Configuration loading for the MTM reference-data sync.

Reads the checked-in YAML policy (sync_policy.yml) that defines the database
names and the exact per-table sync policy (allowlist, natural keys, ignore
lists, write modes, and FK remaps). The policy file is the single source of
truth for what this tool may ever touch.
"""

from __future__ import annotations

import dataclasses
import os
from pathlib import Path
from typing import Any, Dict, List

import yaml

#: Canonical direction value for tables the tool may write.
DIR_TEST_TO_CORE = "test->core"
#: Canonical direction value for tables the tool refuses to write.
DIR_EXCLUDE = "exclude"

#: Accepted write modes.
MODE_INSERT_IF_MISSING = "insert-if-missing"
MODE_UPSERT = "upsert"
VALID_MODES = (MODE_INSERT_IF_MISSING, MODE_UPSERT)


def _normalize_direction(value: str) -> str:
    """Normalize policy direction strings ('test -> core' vs 'test->core')."""
    cleaned = (value or "").strip().lower().replace(" ", "")
    if cleaned in ("test->core", "testtocore", "test_to_core"):
        return DIR_TEST_TO_CORE
    if cleaned in ("exclude", "excluded", "none", "never"):
        return DIR_EXCLUDE
    raise ValueError(f"Unknown policy direction: {value!r}")


@dataclasses.dataclass
class FkMap:
    """Remap a child FK column to a parent table via the parent's natural key.

    Example: dunnage_parts.type_id -> dunnage_types (parent_key type_name).
    The child's value is the parent's id in the *source* database; it is
    translated to the parent's id in the *target* database so FK integrity is
    preserved even when identities differ between databases.
    """

    column: str
    references: str
    parent_key: List[str]

    @classmethod
    def from_mapping(cls, raw: Dict[str, Any]) -> "FkMap":
        parent_key = raw.get("parent_key")
        if isinstance(parent_key, str):
            parent_key = [parent_key]
        return cls(
            column=str(raw["column"]),
            references=str(raw["references"]),
            parent_key=[str(k) for k in (parent_key or [])],
        )


@dataclasses.dataclass
class TablePolicy:
    """Per-table sync policy."""

    table: str
    direction: str
    mode: str
    natural_keys: List[str]
    ignore_columns: List[str]
    fk_map: List[FkMap]
    comment: str = ""

    @property
    def active(self) -> bool:
        return self.direction == DIR_TEST_TO_CORE

    @classmethod
    def from_mapping(cls, raw: Dict[str, Any]) -> "TablePolicy":
        direction = _normalize_direction(str(raw.get("direction", DIR_EXCLUDE)))
        mode = str(raw.get("mode", MODE_INSERT_IF_MISSING)).strip().lower()
        if mode not in VALID_MODES:
            raise ValueError(f"Table {raw.get('table')!r} has invalid mode {mode!r}")
        natural_keys = raw.get("natural_keys") or []
        if isinstance(natural_keys, str):
            natural_keys = [natural_keys]
        ignore_columns = raw.get("ignore_columns") or []
        if isinstance(ignore_columns, str):
            ignore_columns = [ignore_columns]
        fk_map = [FkMap.from_mapping(m) for m in (raw.get("fk_map") or [])]
        if not natural_keys:
            raise ValueError(f"Table {raw.get('table')!r} has no natural_keys")
        return cls(
            table=str(raw["table"]),
            direction=direction,
            mode=mode,
            natural_keys=[str(k) for k in natural_keys],
            ignore_columns=[str(c) for c in ignore_columns],
            fk_map=fk_map,
            comment=str(raw.get("comment", "") or ""),
        )


@dataclasses.dataclass
class ExcludedTable:
    """Documented reason a table is outside the sync allowlist."""

    table: str
    reason: str = ""


@dataclasses.dataclass
class SyncConfig:
    """Full sync configuration loaded from the YAML policy file."""

    core_db: str
    test_db: str
    backup_db: str
    policies: List[TablePolicy]
    excluded: List[ExcludedTable]

    @property
    def active_policies(self) -> List[TablePolicy]:
        return [p for p in self.policies if p.active]

    @classmethod
    def from_mapping(cls, raw: Dict[str, Any]) -> "SyncConfig":
        defaults = raw.get("database_defaults") or {}
        tables = [TablePolicy.from_mapping(t) for t in (raw.get("tables") or [])]
        excluded = [
            ExcludedTable(table=str(e.get("table", "")), reason=str(e.get("reason", "") or ""))
            for e in (raw.get("excluded") or [])
        ]
        return cls(
            core_db=str(defaults.get("core_db", "mtm_receiving_application")),
            test_db=str(defaults.get("test_db", "mtm_receiving_application_test")),
            backup_db=str(defaults.get("backup_db", "mtm_receiving_application_backup_test")),
            policies=tables,
            excluded=excluded,
        )


def default_config_path() -> Path:
    """Resolve the policy file next to this package (Database/SyncTool)."""
    return Path(__file__).resolve().parent.parent / "sync_policy.yml"


def load_config(path: str | os.PathLike | None = None) -> SyncConfig:
    """Load and validate the YAML policy configuration."""
    cfg_path = Path(path) if path else default_config_path()
    if not cfg_path.is_file():
        raise FileNotFoundError(f"Sync policy file not found: {cfg_path}")
    with cfg_path.open("r", encoding="utf-8") as handle:
        raw = yaml.safe_load(handle) or {}
    config = SyncConfig.from_mapping(raw)
    if not config.active_policies:
        raise ValueError(f"No tables with direction 'test -> core' in {cfg_path}")
    return config
