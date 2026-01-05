# Code Review Sentinel - GitHub Copilot Agent

**Agent Type:** Expert Agent with Persistent Memory  
**BMAD Agent:** `_bmad/agents/code-reviewer/code-reviewer.agent.yaml`  
**Slash Command:** `/review-module`  
**Aliases:** `@code-reviewer`, `@review`, `@sentinel`

---

## Description

Automated WinUI 3 module code review specialist for the MTM Receiving Application. Analyzes Services, DAOs, ViewModels, Views, and Models for critical vulnerabilities, architectural violations, performance issues, and improvement opportunities. Generates comprehensive CODE_REVIEW.md with categorized issues, applies fixes automatically with build validation, and maintains persistent memory across sessions.

---

## Capabilities

- **Comprehensive Analysis**: Scans modules for 10+ severity categories (CRITICAL → LOGGING/DOCS)
- **Smart Fix Application**: Auto-orders fixes by dependencies, validates build after each change
- **Settings Discovery**: Extracts hardcoded values for centralized configuration
- **Service Documentation**: Generates GitHub Copilot instruction files for services
- **Database Operations**: Creates stored procedures and migration scripts
- **Progress Persistence**: Remembers state across sessions via sidecar memory
- **Version Control**: Archives completed reviews with timestamps

---

## Usage

### Quick Commands

```
/review-module                          # Start interactive review
@code-reviewer analyze Module_Volvo     # Analyze specific module
@code-reviewer fix                      # Apply fixes from amended review
@code-reviewer docs                     # Generate service documentation
@code-reviewer status                   # Show current progress
@code-reviewer archive                  # Archive completed review
```

### Menu Commands

| Command | Function |
|---------|----------|
| `[I]init` | Initialize or resume module review |
| `[A]analyze` | Analyze module and generate CODE_REVIEW.md |
| `[F]fix` | Apply checked fixes with build validation |
| `[D]docs` | Generate/update service documentation |
| `[S]status` | Show review progress and state |
| `[V]archive` | Archive review and increment version |
| `[MH]menu` | Display menu options |
| `[CH]chat` | Chat about review findings |

### Flags

- `--only-critical` - Apply only 🔴 CRITICAL fixes
- `--only-security` - Apply only 🟡 SECURITY fixes
- `--skip-maintain` - Skip 🔧 MAINTAIN issues
- `--skip-logging` - Skip 🟤 LOGGING/DOCS issues

---

## Workflow

1. **Analyze**: Scan module → Generate CODE_REVIEW.md with all issues marked ⬜ (not done)
2. **Review**: User reviews and marks priorities
3. **Fix**: Agent applies fixes for ⬜ items, marks ✅ when complete
4. **Document**: Generate service instruction files
5. **Archive**: When complete, archive review and start new version

**Checkbox Semantics:**
- ✅ = COMPLETED (fix has been applied successfully)
- ⬜ = NOT DONE (pending fix or awaiting user decision)
- ❌ or ➖ = SKIP (user explicitly chose not to fix)

---

## Severity Categories

| Icon | Category | Description |
|------|----------|-------------|
| 🔴 | CRITICAL | SQL injection, path traversal, data loss, transaction failures |
| 🟡 | SECURITY | Authorization, input validation, hardcoded secrets |
| 🟠 | DATA | Race conditions, integrity violations, cascade deletes |
| 🔵 | QUALITY | Magic strings, dead code, duplication, missing documentation |
| 🟣 | PERFORMANCE | N+1 queries, inefficient collections, missing caching |
| 🔧 | MAINTAIN | Complex validation, inconsistent naming, technical debt |
| 🟢 | EDGE CASE | Boundary conditions, null handling, zero values |
| 🎨 | UI DESIGN | Workflow improvements, form usability |
| 👤 | UX | User feedback, error messages, progress indicators |
| 🟤 | LOGGING/DOCS | Missing logs, exception details, XML documentation |

---

## Generated Files

### Module Output
- `{Module}/CODE_REVIEW.md` - Main review document with checkboxes
- `{Module}/Archived_Code_Reviews/CODE_REVIEW_V{#}_{timestamp}.md` - Archived versions

### Documentation
- `Documentation/FutureEnhancements/Module_Settings/{Module}Settings.md` - Settings guide

### Service Instructions
- `.github/instructions/service-{name}.instructions.md` - GitHub Copilot format
- `.github/instructions/service-{name}_v{#}.instructions.md` - Versioned when changed

### Database
- `Database/StoredProcedures/{Module}/sp_{operation}.sql` - New stored procedures
- `Database/Migrations/{###}_{migration_name}.sql` - Schema migrations

---

## Architecture

### BMAD Integration

**Agent File:** `_bmad/agents/code-reviewer/code-reviewer.agent.yaml`  
**Agent Type:** Expert (with sidecar for persistent memory)

**Sidecar Structure:**
```
_bmad/_memory/code-reviewer-sidecar/
├── instructions.md          # Startup protocols and behaviors
├── memories.md              # Session state and module history
└── workflows/
    ├── analyze-module.md    # Analysis workflow
    ├── apply-fixes.md       # Fix application with build validation
    └── generate-docs.md     # Documentation generation
```

### Templates

**Location:** `.github/templates/code-review/`

- `CODE_REVIEW.template.md` - Main review document structure
- `ModuleSettings.template.md` - Settings documentation format
- `service-instructions.template.md` - Service guide format
- `MIGRATION.template.sql` - Database migration format

---

## Configuration

### File Access Scope

**Allowed:**
- Target `Module_{Name}/` (full access)
- `Database/StoredProcedures/{Module}/` (create/modify/delete)
- `Database/Migrations/` (create)
- `.github/instructions/service-*.instructions.md` (create/update)
- `.github/templates/code-review/` (read)
- `Documentation/FutureEnhancements/Module_Settings/` (create/update)
- `_bmad/_memory/code-reviewer-sidecar/` (full access)

**Restricted:**
- Other modules (unless analyzing)
- System files
- User data

### Build Validation

- **Timing:** After EVERY fix
- **On Error:** Auto-retry up to 3 times with different approaches
- **Recovery:** Automatic for common errors (missing usings, syntax)
- **Failure:** Stops and requests user intervention

### Memory Persistence

**Location:** `_bmad/_memory/code-reviewer-sidecar/memories.md`

**Tracks:**
- Current module and CODE_REVIEW version
- Fix progress (completed/remaining)
- Module-specific patterns discovered
- Hardcoded values found
- Last action and timestamp

---

## Integration with GitHub Copilot

### Custom Instructions Location

This agent follows GitHub Copilot custom instruction format:
https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions

### Service Documentation Format

Generated service instruction files are GitHub Copilot compatible:
- `.github/instructions/service-{name}.instructions.md`
- Includes method templates, common patterns, validation rules
- Auto-versioned when service changes

### Activation

**In GitHub Copilot Chat:**
```
/review-module
@code-reviewer [I]init
@code-reviewer analyze Module_Volvo
```

**Via Command Palette:**
- Not applicable (agent runs in Copilot chat)

---

## Examples

### Example 1: First Review

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

✅ Analysis Complete for Module_Volvo
CODE_REVIEW.md generated with 26 issues

Severity Breakdown:
🔴 CRITICAL: 3
🟡 SECURITY: 2
🟠 DATA: 2
🔵 QUALITY: 7
🟣 PERFORMANCE: 2
🔧 MAINTAIN: 5
🟢 EDGE CASE: 3
🟤 LOGGING/DOCS: 2

Next: Review CODE_REVIEW.md and amend checkboxes (✅ = fix, ⬜ = skip)
```

### Example 2: Applying Critical Fixes

```
User: @code-reviewer fix --only-critical

Agent:
Loading CODE_REVIEW.md...
Found 3 CRITICAL issues marked ✅

Execution Order (dependency-aware):
Phase 1: Database Foundation
  - #2: Create sp_volvo_shipment_update.sql
  - #1: Update Dao_VolvoShipment to use stored proc

Phase 2: Validation  
  - #3: Add path validation

Proceed? [y/n]

User: y

Agent:
[1/3] Creating stored procedure sp_volvo_shipment_update.sql
✅ Build succeeded (3.2s)

[2/3] Updating Dao_VolvoShipment to use stored procedure
✅ Build succeeded (3.4s)

[3/3] Adding path validation in GenerateLabelCsvAsync
✅ Build succeeded (3.1s)

🎉 All CRITICAL fixes applied!
Build: ✅ Passing
Time: 9.7s
```

### Example 3: Resuming After Interruption

```
User: @code-reviewer status

Agent:
Module: Module_Volvo
CODE_REVIEW: V1
Progress: 18/26 fixes applied (69%)

Last fix: Issue #17 - Large file check
Timestamp: 2026-01-05 14:30:22
Build: ✅ Passing

Remaining: 8 issues (all ⬜ - marked skip)
Ready to archive? [V]archive
```

---

## Troubleshooting

### Build Fails After Fix

**Agent auto-recovers** by:
1. Analyzing error message
2. Attempting common fixes (usings, syntax, orphaned code)
3. Rebuilding to verify
4. Repeating up to 3 times

**If all attempts fail:**
- Agent reports error details
- Marks fix as failed in memories
- Continues with remaining fixes
- Includes failed fix in summary

### Fix Order Issues

**Agent detects dependencies:**
- Stored procedure creation → DAO usage
- Constants class → Service reference
- Service method → ViewModel call

**Smart grouping ensures:**
- Dependencies fixed before dependents
- Related fixes grouped by file
- Critical severity prioritized

### Memory Issues

**Reset memories if needed:**
1. Edit `_bmad/_memory/code-reviewer-sidecar/memories.md`
2. Clear module state section
3. Restart agent with `[I]init`

---

## Performance

**Typical Analysis Time:**
- Small module (< 10 files): ~30 seconds
- Medium module (10-30 files): ~2 minutes  
- Large module (30+ files): ~5 minutes

**Fix Application:**
- ~3 minutes per fix (including build)
- Smart grouping reduces overall time
- Build validation adds safety overhead

**Memory Footprint:**
- Sidecar files: ~35 KB total
- Templates: ~15 KB
- Scales with module size

---

## Best Practices

1. **Review before fixing** - Always amend CODE_REVIEW.md checkboxes first
2. **Start with critical** - Use `--only-critical` for first pass
3. **Test incrementally** - Review changes every 5-10 fixes via git diff
4. **Archive when done** - Don't leave completed reviews unarchived
5. **Monitor build** - Agent stops on errors, but verify final build
6. **Use documentation** - Implement settings from generated {Module}Settings.md
7. **Version services** - Agent auto-versions when service changes detected

---

## Related Files

- **User Guide:** `.github/prompts/module-code-review.prompt.md`
- **Agent Definition:** `_bmad/agents/code-reviewer/code-reviewer.agent.yaml`
- **Workflows:** `_bmad/_memory/code-reviewer-sidecar/workflows/`
- **Templates:** `.github/templates/code-review/`
- **MVVM Guide:** `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Guide:** `.github/instructions/dao-pattern.instructions.md`

---

## Version History

**1.0** (2026-01-05)
- Initial release
- Expert agent with persistent memory
- 10 severity categories
- Smart dependency detection
- Build validation after each fix
- Service documentation generation
- Settings discovery
- Version control for reviews

---

**Maintained By:** Development Team  
**License:** Internal Use Only  
**Support:** Use `@code-reviewer [CH]chat` for questions
