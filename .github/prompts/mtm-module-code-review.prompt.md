# Module Code Review - Automated Analysis & Fix Application

**Version:** 1.0  
**Created:** January 5, 2026  
**Agent:** Code Review Sentinel

---

## What This Does

Comprehensive code review and automated fix application for WinUI 3 modules in the MTM Receiving Application. The agent:

1. **Analyzes** entire module for critical vulnerabilities, architectural violations, and improvements
2. **Generates** CODE_REVIEW.md with categorized issues and checkboxes
3. **Discovers** hardcoded values for settings documentation
4. **Applies** fixes automatically after you amend the review
5. **Validates** build after EVERY fix
6. **Tracks** progress across sessions with persistent memory
7. **Versions** review documents when complete

---

## Quick Start

### Option 1: Copilot Slash Command (Recommended)

```
/review-module
```

The agent will ask which module to analyze or auto-detect from your current file.

### Option 2: Direct Invocation

```
@code-reviewer analyze Module_Volvo
```

or

```
@code-reviewer I module-code-review
```

---

## Workflow

### Step 1: Analysis

**Command:** `[I]init` or `[A]analyze`

**What happens:**

- Agent scans all Services, DAOs, ViewModels, Views, Models in target module
- Applies 10+ severity categories (🔴 CRITICAL → 🟤 LOGGING/DOCS)
- Detects hardcoded values for settings
- Checks architectural violations against constitution
- Generates `CODE_REVIEW.md` in module root
- Creates `{Module}Settings.md` in Documentation/FutureEnhancements/Module_Settings/

**Output:**

```
✅ Analysis Complete for Module_Volvo

Generated:
- CODE_REVIEW.md (26 issues found)
- VolvoSettings.md (15 settings discovered)

Severity Breakdown:
🔴 CRITICAL: 3
🟡 SECURITY: 2
... (all severities)

Next: Review CODE_REVIEW.md and amend checkboxes
```

### Step 2: Review & Amend

**Open:** `{Module}/CODE_REVIEW.md`

**Amend checkboxes:**

- `✅` = Apply this fix automatically
- `⬜` = Skip this fix (I'll handle manually or ignore)

**Save the file** with your amendments

### Step 3: Apply Fixes

**Command:** `[F]fix`

**What happens:**

- Agent reads your amended CODE_REVIEW.md
- Groups fixes by dependencies (smart ordering)
- Applies fixes one by one
- **Builds after EACH fix** - stops on errors
- Fixes build errors automatically when possible
- Updates checkboxes as fixes complete
- Tracks progress in memories

**Flags:**

- `--skip-maintain` - Skip 🔧 MAINTAIN issues
- `--skip-logging` - Skip 🟤 LOGGING/DOCS issues
- `--only-critical` - Apply only 🔴 CRITICAL issues
- `--only-security` - Apply only 🟡 SECURITY issues

**Example:**

```
@code-reviewer fix --only-critical
```

**Output:**

```
[1/26] Applying fix #2: Create stored procedure sp_volvo_shipment_update
✅ Build succeeded (3.2s)

[2/26] Applying fix #1: Update Dao to use stored proc
✅ Build succeeded (3.4s)

... (continues for all checked fixes)

🎉 Fix Application Complete!
Applied: 18/26 fixes
Build: ✅ Passing
Time: 54 minutes
```

### Step 4: Generate Documentation

**Command:** `[D]docs`

**What happens:**

- Analyzes all Services in module
- Checks for existing `.github/instructions/service-{name}.instructions.md`
- Compares service methods with documentation
- **Versions** documentation if service changed (v1 → v2)
- Generates new service instruction files
- Updates {Module}Settings.md with hardcoded values

**Output:**

```
✅ Documentation Generation Complete

Service Documentation:
- Created: service-volvo.instructions.md
- Updated: service-volvo-masterdata.instructions.md (v1 → v2)

Settings Documentation:
- VolvoSettings.md updated (18 settings documented)
```

### Step 5: Archive & Repeat

**Command:** `[V]archive`

**When:** All checkboxes are ✅ (review complete)

**What happens:**

- Verifies all issues fixed
- Creates `Archived_Code_Reviews/` folder in module
- Moves `CODE_REVIEW.md` → `Archived_Code_Reviews/CODE_REVIEW_V1_20260105_143000.md`
- Updates memories with archived version
- Asks: "Start new CODE_REVIEW_V2.md?"

---

## Menu Commands

| Command | Description |
|---------|-------------|
| `[I]init` | Initialize or resume module review |
| `[A]analyze` | Analyze module and generate CODE_REVIEW.md |
| `[F]fix` | Apply fixes from amended CODE_REVIEW.md |
| `[D]docs` | Generate/update service documentation |
| `[S]status` | Show current review status and progress |
| `[V]archive` | Archive completed review and start new version |
| `[MH]menu/help` | Show this menu |
| `[CH]chat` | Chat with agent about review |

---

## Severity Categories

| Icon | Name | Examples |
|------|------|----------|
| 🔴 | CRITICAL | SQL injection, path traversal, data loss, no transactions |
| 🟡 | SECURITY | Missing authorization, input validation, hardcoded secrets |
| 🟠 | DATA | Race conditions, integrity violations, cascade delete issues |
| 🔵 | QUALITY | Magic strings, dead code, duplication, missing docs |
| 🟣 | PERFORMANCE | N+1 queries, inefficient collections, no caching |
| 🔧 | MAINTAIN | Complex validation, inconsistent naming, tech debt |
| 🟢 | EDGE CASE | Zero/negative values, null handling, boundary conditions |
| 🎨 | UI DESIGN | Workflow improvements, form usability, shortcuts |
| 👤 | UX | User feedback, error messages, progress indicators |
| 🟤 | LOGGING/DOCS | Missing logs, exception details, XML docs |

---

## What Gets Created

### In Module Folder

```
Module_{Name}/
├── CODE_REVIEW.md                    # ← Main review document
├── CODE_REVIEW_V1_{timestamp}.md     # (moved to Archived_Code_Reviews/ when complete)
└── Archived_Code_Reviews/
    └── CODE_REVIEW_V1_20260105.md    # ← Archived versions
```

### In Documentation Folder

```
Documentation/FutureEnhancements/Module_Settings/
└── {Module}Settings.md               # ← Configurable settings guide
```

### In .github Folder

```
.github/instructions/
├── service-volvo.instructions.md          # ← Service guides
├── service-volvo-masterdata.instructions.md
└── service-routing_v2.instructions.md     # ← Versioned if changed
```

### In Database Folder

```
Database/
├── StoredProcedures/{Module}/
│   └── sp_{operation}.sql            # ← New stored procedures
└── Migrations/
    └── {###}_{migration_name}.sql    # ← Schema migrations
```

---

## Examples

### Example 1: First-Time Module Analysis

```
User: @code-reviewer analyze Module_Volvo

Agent:
Found in Module_Volvo:
- 2 Services
- 4 DAOs
- 3 ViewModels
- 3 Views
- 4 Models

Analyzing...
✅ Analysis Complete
CODE_REVIEW.md generated with 26 issues

Next: Review and amend checkboxes
```

### Example 2: Continuing Previous Review

```
User: @code-reviewer I

Agent:
Found existing CODE_REVIEW.md in Module_Volvo
Version: V1
Status: 8/26 fixes remaining

Last fix: Issue #17 - Large file check
Build: ✅ Passing

Ready to continue? [y/n]
```

### Example 3: Applying Specific Fixes

```
User: @code-reviewer fix --only-critical

Agent:
Loading CODE_REVIEW.md...
Found 3 CRITICAL issues marked ✅

Execution Order:
Phase 1: Database
  - #2: Create stored procedure
  - #1: Update DAO to use stored proc
  
Phase 2: Validation
  - #3: Add path validation

Proceed? [y/n]
```

---

## Configuration

### File Access Boundaries

Agent can **READ/WRITE**:

- Target `Module_*/` directory
- `Database/StoredProcedures/{Module}/`
- `Database/Migrations/`
- `.github/instructions/service-*.instructions.md`
- `.github/templates/code-review/`
- `Documentation/FutureEnhancements/Module_Settings/`

Agent **CANNOT** access:

- Other modules (unless explicitly analyzing)
- User data or external files
- System files

### Build & Test Settings

- **Build after each fix:** ✅ Yes (mandatory)
- **Fix build errors:** ✅ Automatic retry (up to 3 attempts)
- **Run tests:** ❌ No (too time-consuming)
- **On build failure:** Stop and repair before continuing

### Memory Persistence

Agent remembers across sessions:

- Current module and version
- Fix progress (completed/remaining)
- Module-specific patterns
- Hardcoded values discovered
- Last action taken

Located: `_bmad/_memory/code-reviewer-sidecar/memories.md`

---

## Troubleshooting

### "Build failed after fix"

**Agent will:**

1. Analyze error message
2. Attempt automatic fix (missing using, syntax, etc.)
3. Rebuild to confirm
4. Continue if fixed, or ask for help after 3 attempts

### "Fix marked ✅ but not applied"

**Possible causes:**

- Fix has dependencies (must fix #2 before #1)
- File changed since analysis (re-analyze)
- Build error prevented continuation

**Solution:**
Run `[S]status` to see what's blocking

### "Agent created wrong fix"

**Steps:**

1. Review git diff
2. Revert the specific file: `git checkout -- {file}`
3. Mark issue as ⬜ in CODE_REVIEW.md
4. Fix manually or ask agent to try different approach

### "Module not found"

**Solution:**
Ensure module folder exists and follows naming: `Module_{Name}`

List available modules:

```powershell
Get-ChildItem -Directory -Filter "Module_*"
```

---

## Advanced Usage

### Custom Severity Analysis

Analyze only specific severity:

```
@code-reviewer analyze Module_Routing --only-critical
```

### Resume After Interruption

```
@code-reviewer status
@code-reviewer fix
```

Agent remembers where it left off

### Re-analyze Completed Module

```
@code-reviewer I
> Archive CODE_REVIEW_V1 and start new review? [y/n]
```

Useful after major refactoring

### Batch Documentation Generation

```
@code-reviewer docs
```

Generates instruction files for ALL services in module

---

## Best Practices

1. **Review before fixing** - Always amend CODE_REVIEW.md first
2. **Start with CRITICAL** - Use `--only-critical` for first pass
3. **Test incrementally** - Review changes every 5-10 fixes
4. **Archive when done** - Don't leave completed reviews unarchived
5. **Keep settings docs** - Implement settings page using generated docs
6. **Version service docs** - Agent auto-versions when service changes

---

## Tips & Tricks

- **Quick status check:** Type `[S]status` anytime to see progress
- **Chat mode:** Use `[CH]chat` to ask questions about findings
- **Skip low-priority:** Use `--skip-maintain --skip-logging` for fast pass
- **Fix order matters:** Agent auto-detects dependencies
- **Build errors are OK:** Agent will fix common build errors automatically
- **Memory persists:** You can leave and come back - agent remembers

---

## Related Documentation

- [Code Reviewer Agent](_bmad/agents/code-reviewer/code-reviewer.agent.yaml)
- [MVVM Pattern Instructions](.github/instructions/mvvm-pattern.instructions.md)
- [DAO Pattern Instructions](.github/instructions/dao-pattern.instructions.md)
- [Constitution](.specify/memory/constitution.md)

---

**Need Help?**

```
@code-reviewer [MH]menu
@code-reviewer [CH]chat "How do I handle database-dependent fixes?"
```

---

**Version:** 1.0  
**Last Updated:** January 5, 2026  
**Maintained By:** Development Team
