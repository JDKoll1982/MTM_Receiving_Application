# -*- mode: python ; coding: utf-8 -*-

# Builds Database/SyncTool/sync_reference_data.exe (one-file) for the MTM app.
#
# Usage (from the repo venv):
#     .venv\Scripts\python.exe Database\SyncTool\build_exe.py
#
# The MTM .csproj invokes this before every app build when the repo venv exists,
# so the standalone executable is produced automatically and bundled with the app.
# When PyInstaller is not installed the script skips gracefully and the app keeps
# using the Python-script fallback.

from __future__ import annotations

import pathlib
import subprocess
import sys

TOOL_DIR = pathlib.Path(__file__).resolve().parent  # Database/SyncTool
SPEC = TOOL_DIR / "sync_reference_data.spec"
ENTRY = TOOL_DIR / "sync_reference_data.py"
EXE = TOOL_DIR / "dist" / "sync_reference_data.exe"


def _sources() -> list:
    """Source files that, when changed, require a rebuild."""
    files = [ENTRY]
    if SPEC.exists():
        files.append(SPEC)
    package = TOOL_DIR / "mtm_sync"
    if package.is_dir():
        files.extend(sorted(package.glob("*.py")))
    return files


def _is_outdated() -> bool:
    """True when any source file is newer than the existing exe."""
    if not EXE.exists():
        return True
    try:
        exe_mtime = EXE.stat().st_mtime
    except OSError:
        return True
    for source in _sources():
        try:
            if source.stat().st_mtime > exe_mtime:
                return True
        except OSError:
            continue
    return False


def main() -> int:
    if not _is_outdated():
        print(f"sync_reference_data.exe is up to date: {EXE}")
        return 0

    try:
        import PyInstaller  # noqa: F401
    except ImportError:
        print("PyInstaller is not installed - skipping SyncTool exe build (Python-script fallback remains).")
        print("Install it with: .venv\\Scripts\\python.exe -m pip install pyinstaller")
        return 0

    if not SPEC.exists() and not ENTRY.exists():
        print(f"Neither the spec nor the entry script exist under {TOOL_DIR} - cannot build.", file=sys.stderr)
        return 1

    target = str(SPEC) if SPEC.exists() else str(ENTRY)
    print("Building sync_reference_data.exe with PyInstaller ...")
    result = subprocess.run(
        [sys.executable, "-m", "PyInstaller", "--noconfirm", "--clean", target],
        cwd=str(TOOL_DIR),
    )
    if result.returncode != 0:
        print("PyInstaller failed; see errors above.", file=sys.stderr)
        return result.returncode

    if EXE.exists():
        print(f"Built {EXE} ({EXE.stat().st_size / 1_048_576:.1f} MB).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
