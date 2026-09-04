"""MTM reference-data sync: test -> core.

A safe, repeatable CLI that synchronizes selected reference/seed tables from the
test MySQL database into the core database, and reports schema/routine/trigger
drift. See Database/SyncTool/README.md and repository root Prompt.md.
"""

__version__ = "1.0.0"
