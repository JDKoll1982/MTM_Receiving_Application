# Commit Message Generation Prompt

## Task

Perform a thorough examination of all current **uncommitted changes** in this workspace (staged and unstaged) and generate a proper, well-structured Git commit message.

---

## Step 1 — Inventory All Changes

Run the following commands to get a complete picture of the working tree:

```bash
git status
git diff
git diff --staged
```

For each changed file, note:

- **File path** and which module/layer it belongs to (e.g., `Module_Dunnage`, `Module_Receiving`, `Module_OutsideService`, `Database`, `Infrastructure`, `docs`, etc.)
- **Type of change**: new file, modified, deleted, renamed
- **What actually changed**: read the diff carefully — don't just note the file name

---

## Step 2 — Analyze the Changes

Group and categorize the changes by **intent**. For each logical group ask:

1. **What was changed?** (the _what_)
2. **Why was it changed?** (the _why_ — infer from context: bug fix, new feature, refactor, test, docs, config, etc.)
3. **What is the impact?** (does it affect behavior, UI, DB schema, performance, or is it purely cosmetic/structural?)

Use this project's known module structure as context:

- `Module_Dunnage` — Dunnage tracking workflow (labels, loads, types, parts, custom fields)
- `Module_Receiving` — Receiving workflow
- `Module_OutsideService` — Outside service request/waitlist
- `Module_Volvo` — Volvo-specific shipment logic
- `Module_Shared` — Shared models, utilities
- `Module_Core` — Core application services
- `Module_Settings.*` — Per-module settings
- `Database/` — SQL stored procedures, migrations, scripts
- `Infrastructure/` — CQRS, MediatR, FluentValidation, Serilog setup
- `MTM_Receiving_Application.Tests/` — Unit tests
- `docs/`, `specs/` — Documentation and specifications
- `Scripts/` — PowerShell deployment scripts

---

## Step 3 — Determine the Commit Type

Use **Conventional Commits** format. Choose the primary type based on your analysis:

| Type       | When to use                                                     |
| ---------- | --------------------------------------------------------------- |
| `feat`     | New feature or capability added                                 |
| `fix`      | Bug fix                                                         |
| `refactor` | Code restructured without behavior change                       |
| `style`    | Formatting, whitespace, no logic change                         |
| `test`     | Adding or updating unit/integration tests                       |
| `docs`     | Documentation-only changes                                      |
| `chore`    | Build, config, tooling, dependency updates                      |
| `perf`     | Performance improvement                                         |
| `ci`       | CI/CD pipeline changes                                          |
| `db`       | Database schema or stored procedure changes (used in this repo) |

If changes span multiple types, use the **dominant** type for the subject line and cover the others in the body.

---

## Step 4 — Compose the Commit Message

Follow this exact format used in this repository:

```
<type>(<scope>): <short imperative summary under 72 characters>

- <bullet describing first logical change group>
- <bullet describing second logical change group>
- <continue for each meaningful change group>
- <note any breaking changes or important side effects>
```

### Rules:

- **Subject line**: imperative mood ("add", "fix", "update" — NOT "added", "fixes", "updating"), no period at end, max 72 chars
- **Scope**
