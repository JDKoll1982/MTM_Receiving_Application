# Advanced Settings

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Admin Mode](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md), [CSV Export Paths Business Rule](../../Module_Dunnage/01-Business-Rules/csv-export-paths.md)

---

## Purpose

Advanced Settings provides administrators with access to system-level configuration options including CSV export paths, grid performance tuning, and debug/logging settings. These settings affect all users and should be modified with caution.

---

## Access

**Location**: Admin Mode → Advanced Settings  
**Permission**: Administrator only  
**Warning Level**: High (changes affect all users)

---

## Advanced Settings UI

### UI Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Advanced Settings                                   [Back to Dashboard] [✕] │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ ⚠ CAUTION: These settings affect all users and system performance.         │
│   Make changes carefully and test before applying in production.            │
│                                                                              │
│ CSV EXPORT CONFIGURATION                                                    │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Local export path (user's computer)                                         │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ C:\Users\{username}\AppData\Local\MTM\Dunnage\                        │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Use {username} placeholder for user-specific paths.                      │
│ Example: C:\Users\jdoe\AppData\Local\MTM\Dunnage\                          │
│                                                                              │
│ Network export path (shared location)                                       │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ \\SERVER01\Receiving\Dunnage\                                         │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ UNC path to network share. Leave empty to disable network export.        │
│                                                                              │
│ ☑ Create dated subfolders (YYYY-MM-DD format)                              │
│ ☐ Fail export if network path unavailable (otherwise save local only)      │
│                                                                              │
│ [Test Local Path]  [Test Network Path]  [View Export Log]                  │
│                                                                              │
│ GRID PERFORMANCE TUNING                                                     │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Virtualization threshold (rows before enabling virtualization)              │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 100                                                               ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Lower = Better performance for large datasets (50-200 recommended)       │
│ ℹ Higher = Simpler rendering for small datasets (fewer than 100 rows)      │
│                                                                              │
│ Auto-save debounce delay (milliseconds)                                     │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 500                                                               ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Delay after user stops typing before auto-save triggers (300-1000)       │
│                                                                              │
│ ☑ Enable async validation (validate in background, don't block UI)         │
│ ☑ Cache dropdown options (faster rendering, more memory)                   │
│                                                                              │
│ DEBUG & LOGGING                                                             │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Logging level                                                               │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ ● Info (normal operation)                                             │  │
│ │ ○ Debug (detailed diagnostics)                                        │  │
│ │ ○ Trace (verbose, performance impact)                                 │  │
│ │ ○ Error only (minimal logging)                                        │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ ☐ Enable SQL query logging (debug database operations)                     │
│ ☐ Log validation failures (track data quality issues)                      │
│ ☑ Log CSV export operations                                                │
│ ☑ Log workflow mode switches                                               │
│                                                                              │
│ [View Application Log]  [Clear Log]  [Export Log]                          │
│                                                                              │
│ DATABASE MAINTENANCE                                                        │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Transaction history retention (days)                                        │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 365                                                               ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Older transactions archived (not deleted). 365-1095 recommended.         │
│                                                                              │
│ Audit log retention (days)                                                  │
│ ┌───────────────────────────────────────────────────────────────────────┐  │
│ │ 1095 (3 years)                                                    ▲▼  │  │
│ └───────────────────────────────────────────────────────────────────────┘  │
│ ℹ Never deleted (permanent). For reference only.                           │
│                                                                              │
│ [Run Database Optimization]  [View Database Stats]                         │
│                                                                              │
│ SYSTEM INFORMATION                                                          │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Application Version: 1.0.0.245                                              │
│ Database Version: 2.5.1                                                     │
│ Last Database Migration: 2026-01-15 10:30:00                                │
│ Active Users (24h): 12                                                      │
│ Total Dunnage Loads (30d): 1,245                                            │
│                                                                              │
│ [Check for Updates]  [Export System Report]                                │
│                                                                              │
│         [Reset All to Defaults]                          [Cancel]  [Save]   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Setting Categories

### Category 1: CSV Export Configuration

#### Local Export Path

**Purpose**: Define where CSV files are saved on user's local computer.

**Default**: `C:\Users\{username}\AppData\Local\MTM\Dunnage\`

**Placeholders**:
- `{username}`: Replaced with current Windows username
- `{date}`: Replaced with current date (YYYY-MM-DD)
- `{time}`: Replaced with current time (HHmmss)

**Validation**:
```
Rules:
- Valid Windows path format
- Absolute path (not relative)
- Directory must be writable (tested on save)
- Max length: 260 characters (Windows MAX_PATH limit)

Valid Examples:
✅ C:\Users\{username}\AppData\Local\MTM\Dunnage\
✅ C:\Exports\Dunnage\
✅ D:\Company\Receiving\Dunnage\

Invalid Examples:
❌ Exports\Dunnage\ (relative path)
❌ C:\Program Files\MTM\ (requires admin rights)
❌ Invalid:\Path\ (invalid drive letter)
```

**Behavior**:
```
On CSV export:
1. Resolve placeholders:
   C:\Users\{username}\AppData\Local\MTM\Dunnage\
   → C:\Users\jdoe\AppData\Local\MTM\Dunnage\

2. If "Create dated subfolders" checked:
   → C:\Users\jdoe\AppData\Local\MTM\Dunnage\2026-01-25\

3. Create directory if not exists

4. Save file:
   → C:\Users\jdoe\AppData\Local\MTM\Dunnage\2026-01-25\dunnage_export_20260125_143022.csv

5. If error: Show error dialog, don't block user
```

---

#### Network Export Path

**Purpose**: Define shared network location for CSV exports (optional).

**Default**: Empty (disabled)

**Format**: UNC path (`\\SERVER\SHARE\Folder\`)

**Validation**:
```
Rules:
- Valid UNC path format (\\SERVER\SHARE\...)
- Leave empty to disable network export
- Reachability tested on save
- Max length: 260 characters

Valid Examples:
✅ \\SERVER01\Receiving\Dunnage\
✅ \\10.0.1.5\Shares\Dunnage\
✅ (empty - disabled)

Invalid Examples:
❌ C:\Server\Share\ (not UNC)
❌ \\SERVER (incomplete UNC)
❌ \\SERVER\SHARE (missing trailing backslash)
```

**Behavior**:
```
On CSV export:
1. Save to local path (always)

2. If network path configured and reachable:
   → Also save to network path
   → Create dated subfolder if enabled
   → Log success

3. If network path unreachable:
   → If "Fail export if network unavailable" checked:
     ❌ Show error, cancel entire export operation
   → If unchecked:
     ⚠ Show warning, continue with local export only
     → Log warning
```

---

#### Create Dated Subfolders

**Control**: Checkbox

**Default**: Checked

**Behavior**:
```
When checked:
Base path: C:\Exports\Dunnage\
Final path: C:\Exports\Dunnage\2026-01-25\

When unchecked:
Base path: C:\Exports\Dunnage\
Final path: C:\Exports\Dunnage\

Benefit of dated folders:
- Organizes exports by date
- Prevents filename collisions
- Easier to find recent exports
```

---

#### Fail Export if Network Unavailable

**Control**: Checkbox

**Default**: Unchecked (allow local-only export)

**Behavior**:
```
When checked:
Network path unreachable → Cancel entire export, show error

When unchecked:
Network path unreachable → Continue with local export, show warning
```

**Use Cases**:
- **Checked**: When network export is critical (compliance, backup)
- **Unchecked**: When local export is sufficient fallback (convenience)

---

#### Test Path Buttons

**Test Local Path**:
```
Action:
1. Resolve placeholders
2. Attempt to create directory
3. Attempt to create test file
4. Delete test file
5. Show result dialog:

Success:
┌─────────────────────────────────────────────────────┐
│ ✅ Local Path Test Successful                       │
│                                                      │
│ Path: C:\Users\jdoe\AppData\Local\MTM\Dunnage\     │
│ Status: Writable                                    │
│ Free Space: 25.3 GB                                 │
│                                                      │
│                              [OK]                    │
└─────────────────────────────────────────────────────┘

Failure:
┌─────────────────────────────────────────────────────┐
│ ❌ Local Path Test Failed                           │
│                                                      │
│ Path: C:\Users\jdoe\AppData\Local\MTM\Dunnage\     │
│ Error: Access denied                                │
│                                                      │
│ Please check path and permissions.                  │
│                                                      │
│                              [OK]                    │
└─────────────────────────────────────────────────────┘
```

**Test Network Path**:
```
Same as Test Local Path, but also checks network reachability.

Success:
✅ Network Path Test Successful
Path: \\SERVER01\Receiving\Dunnage\
Status: Reachable, Writable
Ping: 12 ms

Failure:
❌ Network Path Test Failed
Path: \\SERVER01\Receiving\Dunnage\
Error: Network path not found
```

---

### Category 2: Grid Performance Tuning

#### Virtualization Threshold

**Purpose**: Define row count at which grid switches to virtualized rendering.

**Control**: Number spinner

**Range**: 50-500 rows

**Default**: 100 rows

**Behavior**:
```
If grid has fewer than threshold rows:
→ Render all rows in DOM (simple, full features)
→ Better for small datasets (no scrolling issues)

If grid has more than threshold rows:
→ Enable virtualization (render only visible rows)
→ Better performance for large datasets
→ Trade-off: Some features limited (e.g., Ctrl+F may not work)

Recommendation:
- 50-100: Most users, typical workflow (<100 loads per session)
- 100-200: Power users with large batches
- 200-500: Exceptional cases (slower but more compatible)
```

---

#### Auto-Save Debounce Delay

**Purpose**: Delay after user stops typing before auto-save triggers.

**Control**: Number spinner (milliseconds)

**Range**: 300-1000 ms

**Default**: 500 ms

**Behavior**:
```
User types in cell:
→ Start debounce timer
→ If user types again: Reset timer
→ If timer expires: Trigger auto-save

Example (500ms delay):
User types "1" at 0ms
User types "0" at 200ms (timer resets)
User types "0" at 350ms (timer resets)
User stops typing
Timer expires at 850ms → Auto-save triggers
```

**Trade-offs**:
- **Lower delay (300ms)**: Faster saves, more database writes
- **Higher delay (1000ms)**: Fewer saves, more keystrokes before save

**Recommendation**: 500ms (balance between responsiveness and database load)

---

#### Enable Async Validation

**Control**: Checkbox

**Default**: Checked

**Behavior**:
```
When checked:
User types → Validation runs in background thread
→ UI remains responsive
→ Validation results appear after completion

When unchecked:
User types → Validation runs on main thread
→ UI may freeze during validation (complex rules)
→ Synchronous, predictable behavior
```

**Use Cases**:
- **Checked**: Recommended for responsive UI
- **Unchecked**: Debug validation issues, deterministic behavior

---

#### Cache Dropdown Options

**Control**: Checkbox

**Default**: Checked

**Behavior**:
```
When checked:
→ Load dropdown options once on app start
→ Cache in memory for session
→ Faster dropdown rendering
→ Higher memory usage

When unchecked:
→ Query dropdown options on each cell edit
→ Slower but always up-to-date
→ Lower memory usage
```

**Recommendation**: Checked (options rarely change during session)

---

### Category 3: Debug & Logging

#### Logging Level

**Control**: Radio buttons

**Options**:
1. **Error only** - Log errors and critical issues only
2. **Info** (Default) - Log normal operations (save, export, etc.)
3. **Debug** - Log detailed diagnostics (SQL queries, validation)
4. **Trace** - Log verbose information (performance impact)

**Log File Location**: `C:\Users\{username}\AppData\Local\MTM\Logs\dunnage.log`

**Behavior**:
```
Error only:
→ Minimal logging (errors, exceptions)
→ Smallest log file size
→ Production default

Info:
→ Normal operation logging
→ User actions, transactions, exports
→ Recommended for production

Debug:
→ Detailed diagnostics
→ SQL queries, validation results
→ Troubleshooting

Trace:
→ Verbose logging (every method call)
→ Performance overhead
→ Short-term debugging only
```

---

#### Log Options

**Enable SQL Query Logging** (Checkbox):
```
When checked:
→ Log all SQL queries with parameters and execution time
→ Useful for debugging database performance issues
→ Large log files
```

**Log Validation Failures** (Checkbox):
```
When checked:
→ Log all validation errors with field and value
→ Track data quality issues
→ Identify common user errors
```

**Log CSV Export Operations** (Checkbox, Default: Checked):
```
When checked:
→ Log CSV export events (success/failure, path, row count)
→ Audit trail for exported data
```

**Log Workflow Mode Switches** (Checkbox, Default: Checked):
```
When checked:
→ Log when user switches between Guided/Manual/Edit modes
→ Track workflow usage patterns
```

---

#### View Application Log

**Action**: Opens log viewer dialog

**UI**:
```
┌─────────────────────────────────────────────────────┐
│ Application Log Viewer                              │
│ ───────────────────────────────────────────────────│
│                                                      │
│ [Refresh]  [Filter: All ▼]  [Search: _________]    │
│                                                      │
│ ┌─────────────────────────────────────────────────┐│
│ │2026-01-25 14:30:22 [INFO] User logged in: jdoe ││
│ │2026-01-25 14:31:05 [INFO] CSV exported: 25 rows││
│ │2026-01-25 14:32:11 [WARN] Network path unavail.││
│ │2026-01-25 14:33:45 [ERROR] Validation failed   ││
│ │...                                              ││
│ └─────────────────────────────────────────────────┘│
│                                                      │
│ [Export Log]  [Clear Log]              [Close]      │
└─────────────────────────────────────────────────────┘
```

---

### Category 4: Database Maintenance

#### Transaction History Retention

**Purpose**: Define how long transaction history is kept in active database.

**Control**: Number spinner (days)

**Range**: 30-1095 days (1 month to 3 years)

**Default**: 365 days (1 year)

**Behavior**:
```
Nightly maintenance job:
→ SELECT transactions older than {retention} days
→ Move to archive table (not deleted)
→ Archived data still accessible via "View Historical Data" (future)

Benefits:
- Keeps active database smaller
- Faster queries on current data
- Historical data preserved for auditing
```

---

#### Audit Log Retention

**Purpose**: Define how long audit logs are retained.

**Control**: Number spinner (days)

**Range**: 365-3650 days (1-10 years)

**Default**: 1095 days (3 years)

**Note**: Audit logs are never deleted, only archived (for compliance).

---

#### Database Optimization

**Run Database Optimization** (Button):
```
Action:
1. Analyze database usage
2. Rebuild indexes
3. Update statistics
4. Vacuum/compact tables
5. Report results

Dialog:
┌─────────────────────────────────────────────────────┐
│ Database Optimization                               │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Running optimization tasks...                       │
│ ✅ Analyzing tables (2/5 complete)                  │
│ ⏳ Rebuilding indexes (in progress)                 │
│ ⏳ Updating statistics (pending)                     │
│ ⏳ Compacting database (pending)                     │
│                                                      │
│ [Cancel]                                            │
└─────────────────────────────────────────────────────┘

Success:
┌─────────────────────────────────────────────────────┐
│ ✅ Database Optimization Complete                   │
│                                                      │
│ Results:                                            │
│ • Tables analyzed: 15                               │
│ • Indexes rebuilt: 32                               │
│ • Database size reduced: 125 MB → 118 MB (-5.6%)   │
│ • Estimated query performance improvement: +12%     │
│                                                      │
│ Recommendation: Run monthly for best performance.   │
│                                                      │
│                              [OK]                    │
└─────────────────────────────────────────────────────┘
```

---

### Category 5: System Information

**Read-Only Fields**:
- **Application Version**: Current app version (from assembly)
- **Database Version**: Schema version (from migrations table)
- **Last Database Migration**: Date of last schema update
- **Active Users (24h)**: Count of users with activity in last 24 hours
- **Total Dunnage Loads (30d)**: Count of loads created in last 30 days

**Check for Updates** (Button):
```
Action:
→ Query update server for new version
→ Show dialog with update info

Available:
┌─────────────────────────────────────────────────────┐
│ Update Available                                    │
│ ───────────────────────────────────────────────────│
│                                                      │
│ A new version is available!                         │
│                                                      │
│ Current Version: 1.0.0.245                          │
│ Latest Version:  1.0.1.300                          │
│                                                      │
│ Release Notes:                                      │
│ • Bug fix: CSV export path validation              │
│ • Performance: Faster grid virtualization           │
│ • New feature: Bulk edit mode                       │
│                                                      │
│ [Download]  [Remind Me Later]  [Skip This Version] │
└─────────────────────────────────────────────────────┘
```

---

## Reset All to Defaults

**Access**: "Reset All to Defaults" button (bottom of page)

**Confirmation**:
```
┌─────────────────────────────────────────────────────┐
│ Confirm Reset to Defaults                           │
│ ───────────────────────────────────────────────────│
│                                                      │
│ Reset ALL advanced settings to factory defaults?    │
│                                                      │
│ This will reset:                                    │
│ • CSV export paths                                  │
│ • Grid performance settings                         │
│ • Logging configuration                             │
│ • Database retention policies                       │
│                                                      │
│ This does NOT affect:                               │
│ • User data (loads, transactions)                   │
│ • Dunnage types, parts, or associations             │
│ • User preferences                                  │
│                                                      │
│ Are you sure?                                       │
│                                                      │
│                      [Cancel]  [Reset]              │
└─────────────────────────────────────────────────────┘
```

---

## Related Documentation

- [Admin Mode Specification](../../Module_Dunnage/02-Workflow-Modes/004-admin-mode-specification.md)
- [CSV Export Paths Business Rule](../../Module_Dunnage/01-Business-Rules/csv-export-paths.md)
- [Settings Architecture](../00-Core/settings-architecture.md)

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete
