# Questions for Module Code Review Automation

**Date:** January 5, 2026  
**Context:** Creating `module-code-review.prompt.md` for automated code reviews  
**Reference:** Module_Volvo code review workflow

---

## 1. Prompt File Location

Where should `module-code-review.prompt.md` be stored?

- [ ] A. Root of repository (`/module-code-review.prompt.md`)
- [x] B. `.github/prompts/module-code-review.prompt.md`
- [ ] C. `_bmad/prompts/module-code-review.prompt.md`
- [ ] D. Other: _______________

**Recommended:** Option B (keeps prompts organized in `.github/`)

---

## 2. Template File Strategy

Should templates be embedded or separate files?

- [ ] A. Embedded in the prompt file as markdown code blocks
- [x] B. Separate template files in `.github/templates/` directory
- [ ] C. Both (embedded with extraction capability)

**Recommended:** Option C (allows reuse and version control)

---

## 3. Version Numbering & Archival

For `CODE_REVIEW_V{#}.md` versioning:

### 3a. Starting Version
- [x] A. Current `CODE_REVIEW.md` = V1 (next would be V2, rename `CODE_REVIEW.md`)
- [ ] B. Start fresh at V1 (archive current as V0)
- [ ] C. Other: _______________

### 3b. "All Fixes Complete" Definition
What triggers creating a new version?

- [x] A. All checkboxes marked ✅ (no ⬜ remaining)
- [ ] B. All 🔴 CRITICAL and 🟡 SECURITY issues fixed
- [ ] C. User explicitly runs "new review" command
- [ ] D. Combination: _______________

### 3c. Archive Strategy
- [x] A. Move old versions to `Archived_Code_Reviews/` subfolder in module
- [x] B. Append timestamp to filename (`CODE_REVIEW_20260105.md`)
- [ ] C. Keep all versions in module root
- [ ] D. Other: _______________

**Recommended:** Option A (Archive folder keeps root clean)

---

## 4. Module Detection

How should the prompt identify which module to analyze?

- [x] A. User specifies module name as parameter (e.g., "Module_Volvo")
- [ ] B. Auto-detect from current file/folder in VS Code
- [ ] C. Analyze all `Module_*` folders in workspace
- [x] D. Interactive: Prompt asks user to select from available modules
- [ ] E. Combination: _______________

**Recommended:** Option E (B for single-file context, D for workspace context)

---

## 5. Code Review Scope

Which file types should be analyzed? (Check all that apply)

- [x] Services (`Services/*.cs`)
- [x] DAOs (`Data/*.cs`)
- [x] ViewModels (`ViewModels/*.cs`)
- [x] Views (`Views/*.xaml` and `.xaml.cs`)
- [x] Models (`Models/*.cs`)
- [x] Interfaces (`Interfaces/*.cs`)
- [x] Enums (`Enums/*.cs`)
- [x] Dialogs/Windows (`*.xaml`, `*.xaml.cs`)
- [x] Other: `\Documentation\Module_Settings`, `.github\instructions`

**Recommended:** All of the above (comprehensive analysis)

---

## 6. Analysis Depth

What severity levels should be included? (Check all that apply)

- [x] 🔴 CRITICAL (SQL injection, path traversal, data loss)
- [x] 🟡 SECURITY (authorization, validation, hardcoded values)
- [x] 🟠 DATA (race conditions, integrity, cascade deletes)
- [x] 🔵 QUALITY (magic strings, dead code, duplication)
- [x] 🟣 PERFORMANCE (N+1 queries, inefficient collections)
- [x] 🔧 MAINTAIN (complex logic, missing docs, naming)
- [x] 🟢 EDGE CASE (boundary conditions, null handling)
- [x] {Needs Icon} UI Design (search for ways to improve workflow)
- [x] {Needs Icon} User Experience (serach for ways to improve user experience, goes with UI Design)
- [x] 🟤 LOGGING/DOCUMENTATION

**Recommended:** All categories for complete review

---

## 7. Fix Execution Mode

When applying fixes automatically:

- [ ] A. Interactive: Confirm before applying each fix
- [ ] B. Batch: Apply all fixes in one run
- [ ] C. Dry-run: Show what would be fixed without changing files
- [ ] D. Semi-interactive: Group fixes by severity, confirm per group
- [ ] E. Manual: Only generate report, user applies fixes manually
- [x] F. Generate Report, on next run of the prompt you'll see that the report exists, the user is responisble for amending it before the next prompt run.

**Recommended:** Option D (balances automation with control)

---

## 8. Build & Test Integration

When running automated fixes:

### 8a. Build Timing
- [x] A. Build after each fix (slow but catches errors early)
- [ ] B. Build only at end of all fixes
- [ ] C. Build after each severity group
- [ ] D. No automatic builds (user runs manually)

### 8b. On Build Failure
- [ ] A. Stop immediately and report error
- [ ] B. Revert last fix and continue
- [ ] C. Continue with remaining fixes
- [ ] D. User decides per failure
- [x] E. Fix Build Failure, then continue with next fix.

### 8c. Test Execution
- [ ] A. Run `dotnet test` if tests exist
- [x] B. Skip tests (too time-consuming)
- [ ] C. Only run affected tests
- [ ] D. User controls via flag

**Recommended:** 8a=C, 8b=A, 8c=D (safe defaults with user control)

---

## 9. Settings Document Generation

For the `VolvoSettings.md`-style template:

### 9a. Value Extraction
- [x] A. Include actual values scraped from code (e.g., `10000` for MaxCsvLines)
- [x] B. Provide structure with placeholders only
- [x] C. Both: Actual values where found, placeholders otherwise

### 9b. Hardcoded Value Detection
- [x] A. Automatically analyze code to find all hardcoded values
- [ ] B. Use predefined patterns (magic numbers > 10, strings in conditions)
- [ ] C. Manual: User specifies what to look for
- [ ] D. Combination

### 9c. Settings Categories
Use same categories as VolvoSettings.md?
- File System Paths
- CSV Generation Limits
- Validation Rules
- Email Formatting
- AutoSuggest Behavior
- Data Retention
- Workflow Behavior
- Logging & Diagnostics
- UI Text (Labels, Default Values, Header Texts, Icons (Use `Module_Shared\Views\View_Shared_IconSelectorWindow.xaml` in settings page to change these))

- [ ] Yes, use these categories
- [x] No, customize per module: All plus the 1 I added (UI Text)

**Recommended:** 9a=C, 9b=D (auto-detect with manual override), 9c=Yes with customization

---

## 10. Service Documentation

For service instruction files (like `service-volvo.v{#}.instructions.md`):

### 10a. Generation Scope
- [x] A. Generate for ALL services in module
- [x] B. Only services without existing `.instructions.md` files
- [x] C. Update/append to existing documentation
- [ ] D. User selects which services to document

**NOTE:** Also Validate existing services for changes, if found delete old file and recreate with version tag `service_volvo_v1.instructions.md` to `service_volvo_v2.instructions.md`

### 10b. Trivial Service Threshold
Skip documentation for services with:
- [ ] A. < 3 methods
- [ ] B. < 5 methods
- [ ] C. Only simple CRUD operations
- [x] D. Never skip (document everything)

### 10c. Documentation Detail Level
- [ ] A. Full (like current service-volvo.instructions.md)
- [ ] B. Medium (core methods + patterns only)
- [ ] C. Light (method signatures + one-line descriptions)
- [ ] D. Adaptive: Detail based on service complexity
- [x] E. Refer to `https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions` (Recreate current `service_volvo.instructions.md` if needed)

**Recommended:** 10a=B, 10b=D, 10c=D (comprehensive but efficient)

---

## 11. Issue Severity & Prioritization

### 11a. Fix Order
Should fixes be applied in severity order?

- [ ] A. Yes: 🔴 → 🟡 → 🟠 → 🔵 → 🟣 → 🔧 → 🟢 → 🟤
- [ ] B. No: Fix in file/logical grouping order
- [ ] C. User chooses order interactively
- [x] D. Smart: Group related fixes regardless of severity

### 11b. Low-Priority Handling
Allow skipping low-priority issues?

- [ ] A. Yes, user can exclude 🔧 MAINTAIN and 🟤 LOGGING
- [ ] B. No, fix everything
- [x] C. Flag-based: `--skip-{type} --only-{type}`

### 11c. Dependency Handling
Some fixes depend on others (e.g., transaction fix needs stored proc):

- [x] A. Auto-detect dependencies and order fixes
- [ ] B. Warn user and let them handle ordering
- [ ] C. Create fix "phases" for dependent groups

**Recommended:** 11a=D, 11b=C, 11c=A (intelligent automation)

---

## 12. Edge Case: Empty Module

What if module has no Services, DAOs, or ViewModels?

- [ ] A. Skip with message: "No analyzable code found"
- [x] B. Analyze whatever files exist (Views, Models only)
- [ ] C. Generate warning report
- [ ] D. Ask user if they want to analyze anyway

**Recommended:** Option D (user decides)

---

## 13. Edge Case: Existing CODE_REVIEW.md

When prompt finds existing `CODE_REVIEW_V{#}.md`:

### 13a. If Incomplete (⬜ items remain)
- [x] A. Continue with unfixed items automatically
- [ ] B. Ask user: "Continue fixes or start new review?"
- [ ] C. Show progress summary, then ask

### 13b. If Complete (all ✅)
- [ ] A. Archive current, create V2 automatically
- [x] B. Ask user: "Archive {FileName} and Start new review or re-analyze {FileName} with {Module} for task completeness?"
- [ ] C. Show summary, offer options

### 13c. If Malformed (missing checkboxes, wrong format)
- [ ] A. Regenerate from scratch
- [x] B. Attempt repair
- [ ] C. Ask user preference

**Recommended:** 13a=C, 13b=B, 13c=B (informative and safe)

---

## 14. Database-Dependent Fixes

Some fixes require stored procedures or schema changes:

### 14a. Stored Procedure Generation
- [x] A. Auto-generate / Update / Remove SQL files in `Database/StoredProcedures/{Module}/`, VALIDATE the SQL file's contents is not used by other methods before modification, deletion.
- [ ] B. Only generate if template/pattern exists
- [ ] C. Skip database fixes, document only
- [ ] D. User confirms before generating SQL

### 14b. Schema Migration
For fixes requiring DB constraints or indexes:
- [x] A. Generate migration SQL file in `Database\Migrations` called `{#(3 didget)}_{migration_name_fix}.sql`
- [ ] B. Add to documentation only
- [ ] C. Skip these fixes
- [ ] D. User decides per fix

**Recommended:** 14a=D, 14b=A (safe with user control)

---

## 15. Progress Tracking

How should the prompt track progress?

- [ ] A. Update checkboxes in `CODE_REVIEW_V{#}.md` after each fix
- [x] B. Update `CODE_REVIEW_V{#}.md` only at end of session
- [ ] C. Separate PROGRESS.md file
- [ ] D. Both CODE_REVIEW.md and console output

**Recommended:** Option D (persistent + visible)

---

## 16. Output & Reporting

What should be generated?

- [x] CODE_REVIEW.md (issue list with checkboxes)
- [ ] IMPLEMENTATION_SUMMARY.md (changes made)
- [x] {Module}Settings.md (configurable values)
- [x] service-{name}.instructions.md (per service)
- [ ] VALIDATION_REPORT.md (build/test results)
- [ ] Other: _______________

**Current Volvo Module Has:** All of the above ✅

---

## 17. Prompt Invocation

How should the prompt be triggered?

- [ ] A. VS Code command palette: "Run Module Code Review"
- [x] B. Copilot slash command: `/review-module`
- [x] C. Workspace file with instructions for Copilot
- [ ] D. Manual: User pastes prompt into chat
- [ ] E. Combination: _______________

**Recommended:** Option E (C for reference, D for execution)

---

## 18. Multi-Module Support

Should the prompt support batch analysis?

- [ ] A. Yes, analyze all modules in one run
- [x] B. No, one module at a time
- [ ] C. User selects multiple modules
- [ ] D. Parallel analysis if possible

**Recommended:** Option C (controlled batch processing)

---

## Additional Questions / Notes

Carefully read this entire file as I added , changed removed things!!!

---

**Status:** ⬜ Awaiting Answers  
**Next Step:** Create `module-code-review.prompt.md` based on responses
