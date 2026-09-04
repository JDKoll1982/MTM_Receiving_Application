#!/usr/bin/env python3
"""Entry-point shim for the MTM reference-data sync CLI.

Run from anywhere (the shim adds this folder to sys.path):

    python Database/SyncTool/sync_reference_data.py --version
    python Database/SyncTool/sync_reference_data.py inspect
    python Database/SyncTool/sync_reference_data.py --help
"""

from __future__ import annotations

import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

from mtm_sync.cli import main  # noqa: E402

if __name__ == "__main__":
    sys.exit(main())
