# Database Deployment Module Specification

Portable specification for the idempotent MySQL object deployment tool.
Copy this document into another repository to recreate the module with the same behavior, layout, and guarantees.

## Purpose And Scope

The deployment tool ships every database object as a plain `.sql` file and converges a live database to that file tree.

- No migration scripts. A migration layer exists only to move an already-deployed database forward; the tool removes that need.
- No data loss on a normal run. Objects are created when missing, altered when their definition differs, and replaced when they are code objects.
- Two modes: **update in place** (default, additive) and **fresh rebuild** (destructive, confirmed twice).
- Preview first. Every change is computed and shown before a single statement runs.

Out of scope: SQL Server / ERP databases. Those stay read-only and are never touched by this tool.

## Design Rules

These rules are the contract. Changing one changes the tool's safety model.

- Table scripts are `CREATE TABLE IF NOT EXISTS` only. A table script never contains `DROP TABLE` or destructive `ALTER`.
- Every code object is replaceable: views use `CREATE OR REPLACE VIEW`, procedures and triggers use `DROP ... IF EXISTS` followed by `CREATE`.
- Seed scripts are re-runnable. They resolve ids from natural keys and guard inserts with `NOT EXISTS`, so a second run changes nothing.
- SQL scripts never contain `USE <database>;`. The target database is always supplied by the tool, so the same tree deploys to dev, test, or production.
- Expected schema is derived by executing the table scripts, not by parsing DDL text.
- Destructive work runs last, and only when the operator explicitly confirmed it.

## Repository Layout

```text
<repo>/
  Database/
    Database_Deployment/
      Sql_Files/
        Schemas/            table definitions, one file per table
        Views/              CREATE OR REPLACE VIEW
        StoredProcedures/   DROP + CREATE PROCEDURE, grouped in subfolders
        Triggers/           DROP + CREATE TRIGGER
        SeedData/           idempotent reference data
        _Archive/           retired scripts; never deployed
  <DeploymentProject>/
    app.manifest
    App.xaml
    App.xaml.cs
    MainWindow.xaml
    MainWindow.xaml.cs
    deployment.settings.json
    Models/
    Services/
    ViewModels/
```

## Solution Wiring

The deployment tool is a standalone desktop executable that can be built, copied, and run on any machine with MySQL access.

- Add the project to the solution file, and exclude its folder from any other project's default globs.
- Example exclusion for a sibling WinUI project:

```xml
<ItemGroup>
  <Compile Remove="MTM_Database_Deployment\**" />
  <Page Remove="MTM_Database_Deployment\**" />
  <PRIResource Remove="MTM_Database_Deployment\**" />
  <None Remove="MTM_Database_Deployment\**" />
</ItemGroup>
```

## Project Scaffolding

Minimum project file for a WinUI 3, unpackaged, self-contained desktop exe.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.22621.0</TargetFramework>
    <TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
    <UseWinUI>true</UseWinUI>
    <WindowsPackageType>None</WindowsPackageType>
    <WindowsAppSDKSelfContained>true</WindowsAppSDKSelfContained>
    <Platforms>x64</Platforms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <!-- Ship the SQL tree next to the executable so the tool runs without the repo. -->
  <ItemGroup>
    <Content Include="..\Database\Database_Deployment\Sql_Files\**\*.sql">
      <Link>Sql_Files\%(RecursiveDir)%(Filename)%(Extension)</Link>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <None Include="deployment.settings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.2" />
    <PackageReference Include="DevWinUI" Version="10.9.1" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="2.4.0" />
    <PackageReference Include="MySql.Data" Version="9.7.0" />
  </ItemGroup>
</Project>
```

`app.manifest` declares the executable as Windows 10 compatible and DPI aware; `App.xaml.cs` creates and activates the window.
The view model must be constructed and assigned before `InitializeComponent()` so `x:Bind` resolves.
`App.xaml` must merge `ms-appx:///DevWinUI/Themes/Generic.xaml` or the DevWinUI controls render without templates.

## Settings File Contract

`deployment.settings.json` sits next to the executable and is intentionally hand-editable so a site can retarget the tool without a rebuild.

```json
{
  "PreferredServer": "172.16.1.104",
  "FallbackServer": "127.0.0.1",
  "Port": 3306,
  "Database": "mtm_receiving_application",
  "User": "root",
  "Password": "",
  "ConnectTimeoutSeconds": 5
}
```

- Never commit a real password. The file is a template; the operator fills it on the machine that deploys.
- The UI reads these values into editable fields and persists changes back to the same file.

## Settings And Path Resolution

- Settings path: `AppContext.BaseDirectory/deployment.settings.json`.
- SQL root: `AppContext.BaseDirectory/Sql_Files` when present, otherwise walk parent directories looking for `Database/Database_Deployment/Sql_Files`.
- The fallback walk keeps the tool usable when it is run straight from `bin` during development.

## Server Resolution

Every deployment and preview resolves the server first, so a missing preferred host fails fast instead of hanging.

1. TCP-probe `PreferredServer` on `Port` with a 2 second budget.
2. If that fails, TCP-probe `FallbackServer`.
3. If both fail, abort with a message naming both endpoints.
4. Use a short connection timeout (`ConnectTimeoutSeconds`, floor of 3 seconds) for the real connection.

The active server and whether the fallback was used are surfaced in the UI and in the deployment report.

## SQL Object Tree Contract

| Folder | Kind | Recursed | Deployed as |
| --- | --- | --- | --- |
| `Schemas` | Table | No | `CREATE TABLE IF NOT EXISTS`, foreign key checks disabled |
| `Views` | View | No | `CREATE OR REPLACE VIEW` |
| `StoredProcedures` | Procedure | Yes | `DROP PROCEDURE IF EXISTS` + `CREATE PROCEDURE` |
| `Triggers` | Trigger | Yes | `DROP TRIGGER IF EXISTS` + `CREATE TRIGGER` |
| `SeedData` | Seed | No | Idempotent `INSERT` / upsert, continue on error |

- Only `*.sql` files are discovered.
- Any path segment named `_Archive` is skipped, so retired scripts stay in the repo without being deployed.
- Files are ordered by relative path, so numeric filename prefixes control execution order.
- A new object kind means a new folder, a new enum value, and one more branch in the catalog.

## Object Authoring Rules

### Table Scripts

- One table per file, `CREATE TABLE IF NOT EXISTS`, quoted identifiers for reserved words such as `` `MaxValue` ``.
- Declare columns, indexes, and foreign keys on the table itself; the comparer reads them from live metadata.
- Never add `DROP TABLE`, `TRUNCATE`, or a data-changing `ALTER` to a table script.
- Never add `USE <database>;`.

### View Scripts

- `CREATE OR REPLACE VIEW` only, so re-running is a no-op replacement.
- A view must not reference a column that only exists in a later-deployed object. If a view needs a column added by a table script, the view must live in `Views`, not in `Schemas`.

### Procedure And Trigger Scripts

- `DROP ... IF EXISTS` then `CREATE`, using the `DELIMITER $$` wrapper the parser understands.
- Procedures encapsulate writes; application code never inlines SQL.
- Parameters and result shape are part of the app contract; the deployment tool only replaces the body.

### Seed Scripts

- Re-runnable by construction. Second run must insert zero rows.
- Resolve parent ids from natural keys immediately before use, for example:

```sql
SET @type_id = (SELECT id FROM dunnage_types WHERE type_name = 'Bags' ORDER BY id LIMIT 1);
```

- Do not rely on an insert procedure's `OUT` id when the procedure may skip the insert; a skipped insert leaves the variable at its previous value and silently attaches child rows to the wrong parent.
- Guard set-based seeds with `WHERE NOT EXISTS` rather than one `CALL` per row when the write procedure is not idempotent.
- Duplicate-key errors are tolerated by the engine, but a seed should not depend on that tolerance.

## Deployment Engine Behavior

### Preview (What-If)

1. Resolve the server.
2. Create a scratch schema named `mtm_expected_<8 hex chars>`.
3. Execute every table script into the scratch schema with foreign key checks disabled.
4. Snapshot table metadata for both the scratch schema and the target database.
5. Diff, then drop the scratch schema in a `finally` block.
6. Add one planned change per view, procedure, trigger, and seed.

A preview never writes to the target database.

### Apply (Update In Place)

Runs in ordered phases:

1. **Ensure tables.** Execute every table script against the target. `IF NOT EXISTS` makes this a no-op for tables that already exist.
2. **Reconcile columns, indexes, and foreign keys.** Apply only the `AddColumn`, `AlterColumn`, `AddIndex`, and `AddForeignKey` changes the comparer produced. A change whose statement failed is reported as a failure, never as applied.
3. **Remove blocking extras.** When the operator confirmed removals, drop the database-only columns that are `NOT NULL` with no default. They have to go before anything writes, because every insert that omits them fails - including the seeds in phase 5.
4. **Stop if the schema did not converge.** If any change in phase 2 failed, report and return. A missing column invalidates the views, procedures, and seeds that need it, and continuing buries the real error under a cascade of downstream failures.
5. **Replace code objects.** Views, then procedures, then triggers.
6. **Run seeds.** Continue-on-error so one stale seed cannot abort a deployment.
7. **Remove the remaining extras.** Only when the operator ticked the removal confirmation, and only after everything else succeeded.

### Rebuild (Destructive)

1. `DROP DATABASE IF EXISTS` then `CREATE DATABASE` with `utf8mb4` / `utf8mb4_unicode_ci`.
2. Deploy the full tree in order: tables, views, procedures, triggers, seeds.

The UI requires a separate acknowledgement checkbox before this mode can run.

## Schema Comparison Algorithm

The comparer compares live metadata against materialized expected metadata, never against parsed text.

- **CreateTable**: expected table absent from the target.
- **AddColumn**: expected column absent from the live table; SQL is `ALTER TABLE ... ADD COLUMN`.
- **AlterColumn**: column signature differs; SQL is `ALTER TABLE ... MODIFY COLUMN` using the expected definition.
- **AddIndex**: index missing, or present with a different signature (drop then re-create).
- **AddForeignKey**: constraint missing, or present with a different signature (drop then re-create).
- **ExtraColumn / ExtraTable**: present in the target, absent from the file tree. Reported always, removed only on confirmation.
- **Blocking extra column**: an `ExtraColumn` that is `NOT NULL` with no default. It is flagged so the preview and the summary say so, and it is the one extra that must be removed before the seeds run.

Signatures are compared as ordinal strings generated from normalized metadata, so formatting differences in the source DDL never cause false positives.

### Generating A Column Definition

`information_schema` reports a string default as the bare literal, so a naive `DEFAULT {value}` emits `DEFAULT Quantity` and MySQL rejects the statement. The generator must therefore:

- quote the default for character types, doubling any embedded quote: `DEFAULT 'Quantity'`,
- leave numeric defaults unquoted: `DEFAULT 0`,
- leave expressions unquoted: `CURRENT_TIMESTAMP`, anything containing `(`, `b'0'`, `0x..`, `_binary..`,
- treat `EXTRA` containing `DEFAULT_GENERATED` as already an expression.

## Data Safety Guarantees

- A normal apply never drops a table or a column.
- A normal apply never truncates or rewrites existing rows.
- Seeds insert only what is missing.
- Extra objects are reported in the preview before they can be removed.
- The scratch schema used for preview is always dropped, even when the preview throws.
- Destructive modes require an explicit confirmation.
- Removals are split by risk: blocking columns go before anything writes, everything else runs last.

## Result Reporting

The in-memory result tracks four counters: applied, skipped, benign skips, and failures.

- A statement whose error matches a benign pattern is counted as a benign skip, not a failure. Benign patterns include `already exists`, `duplicate entry`, `duplicate column name`, `duplicate key name`, `duplicate foreign key constraint name`, `multiple primary key defined`, `check that column/key exists`, and `can't drop`.
- Benign skips are aggregated per object and printed once with an occurrence count, so a re-run of a large seed does not flood the report.
- Failures are aggregated the same way, keyed by message and printed in first-seen order with an occurrence count. A seed that fails the same way 90 times appears as one line ending in `(x90)`.
- A text report is written to `%LOCALAPPDATA%\<Tool Name>\reports\deployment-yyyyMMdd-HHmmss.txt` and includes the database, resolved server, mode, timestamp, counters, per-object messages, and failures.


## UI Surface

A single window, written for an operator with no database background. Three steps, one primary button each, and every label in plain language.

### Wizard shape

1. **Connect.** One button. The tool probes the preferred server, then the spare, then signs in for real (`SELECT 1` against the target database) before it claims success.
2. **Check.** Builds the plan and renders it as plain-language groups, not SQL kinds.
3. **Save.** Recaps the groups, offers the two modes, and runs the deployment.

A `StepBar` trail shows progress and `AutoSelectOnItemClick` stays off so the trail cannot skip a step. `Back` re-shows the previous step without discarding the plan.

### Plain-language mapping

The view model turns `Model_SchemaChange` kinds into groups an operator understands, in this order:

| Group | Change kinds | Wording intent |
| --- | --- | --- |
| Must be removed first | blocking `ExtraColumn` | "the standard lists cannot be saved until it is gone" |
| New tables | `CreateTable` | brand-new tables |
| New detail fields | `AddColumn` | "existing rows simply have it blank" |
| Roomier fields | `AlterColumn` | "your values are kept" |
| Speed-up rules | `AddIndex`, `AddForeignKey` | housekeeping |
| Saved queries and rules | `UpdateView`, `UpdateProcedure`, `UpdateTrigger` | stored instructions refreshed |
| Standard reference lists | `RunSeed` | "anything you added yourself stays" |
| Leftovers found in the database | non-blocking removals | left alone unless the box is ticked |

Identifiers become sentences: `dunnage_parts.quantity_type` renders as "Dunnage parts - quantity type", and `StoredProcedures/Dunnage/sp_Dunnage_Types_Insert.sql` as "Dunnage types insert".

### Safety and disclosure

- The safety line is always visible and changes with the mode: "Your records are kept. Nothing is deleted." versus "This erases every record."
- Rebuild requires its own checkbox and keeps the save button disabled until it is ticked.
- Leftover removal is opt-in and off by default.
- SQL detail is folded behind a "Technical details - for IT" `Expander`, and the same text can be saved to a file for IT.

### Controls

DevWinUI supplies `StepBar`, `InfoCard`, `SelectorBarSegmented`, and `ProgressButton`; WinUI supplies `Expander`, `ScrollViewer`, `ProgressRing`, and the theme-resource card surfaces. `ms-appx:///DevWinUI/Themes/Generic.xaml` must be merged in `App.xaml`, because `StepBar` reads its own templates from `Application.Current.Resources` in its constructor.

`ProgressButton` is a `ToggleButton`: bind `IsChecked` to the busy flag, `CheckedContent` to the busy label, and `IsEnabled` to a `CanSave` flag, because the control does not refuse a second click.

### Binding rules

- All bindings use `x:Bind` with an explicit `Mode`. No runtime `{Binding}`.
- Visibility comes from `x:Bind` function calls (`ui:Show.If(flag)`), not a converter: this window's binding root is a `Window`, which is not a framework element, so the generated code cannot build a converter lookup root.
- Controls that cannot be bound (`SelectorBarSegmented.SelectedItem`, `PasswordBox.Password`, window size) are wired in code behind; no business logic lives there.

### Copy rules

- Never show a raw change kind, table name, or SQL error as the headline.
- State what happens to the operator's data before what will change.
- A failure says the records are untouched and points at IT, then offers the details file.


## Porting Checklist

1. Create the project, add it to the solution, and exclude its folder from every other project's globs.
2. Copy `Models`, `Services`, and `ViewModels` unchanged; they contain no MTM-specific names.
3. Point `<Content Include>` at the target repo's SQL tree, or copy `Sql_Files` next to the executable by hand.
4. Rewrite `deployment.settings.json` defaults for the new site.
5. Normalize the existing SQL tree: strip `USE`, replace `CREATE TABLE` with `CREATE TABLE IF NOT EXISTS`, remove `DROP TABLE`, convert views to `CREATE OR REPLACE`, archive the migration folder.
6. Fold every migration's column, index, and constraint additions into the owning table script before archiving the migration.
7. Make each seed re-runnable.
8. Run preview against an empty database, then apply, then apply again and confirm the second run reports no table, column, index, or foreign key work.

## Validation Checklist

- [ ] Preview against an empty database plans one `CreateTable` per table script.
- [ ] Connect reports success only after a real sign-in, not just a TCP probe.
- [ ] A wrong password shows the sign-in message, and a missing database name is called out separately.
- [ ] Step 1, 2, and 3 each render in light and dark themes with readable card edges.
- [ ] Choosing "Start over" swaps the checkbox and keeps the save button disabled until it is ticked.
- [ ] Apply against an empty database finishes with zero failures.
- [ ] Object counts in the target match the file tree counts.
- [ ] A second preview reports zero table, column, index, and foreign key changes.
- [ ] A second apply finishes with zero failures and zero new rows.
- [ ] Sentinel rows inserted between runs survive the redeploy unchanged.
- [ ] Dropping a column from the target is detected and restored by the next apply.
- [ ] A column that exists only in the target appears as an extra and is removed only when confirmed.
- [ ] Fresh rebuild recreates every table and leaves them empty.
- [ ] No script in the tree contains `USE `, `DROP TABLE`, or a raw `CREATE VIEW` without `OR REPLACE`.
- [ ] A column whose file default is a string converges with `DEFAULT '<value>'`, not `DEFAULT <value>`.
- [ ] A database-only `NOT NULL` column with no default is named as blocking in the preview summary.
- [ ] A schema change that fails stops the run before views, procedures, and seeds are touched.

## Automated Tests

The following tests ship with the module and fail if its guarantees regress.

- `Service_SqlScriptParserTests` — statement splitting: `DELIMITER` bodies, semicolons inside literals,
  escapes, line/`#`/block comments, missing final delimiter, empty script.
- `Service_ObjectCatalogTests` — folder classification, procedure recursion, deployment order, and the
  rule that anything under `_Archive` is never discovered.
- `Model_SchemaChangeTests` — `IsRemoval` / `RequiresConfirmation`, which gate the destructive branch.
- `Model_DeploymentResultTests` — benign-skip aggregation and the report's failure section.
- `DunnageSeedIdempotencyIntegrationTests` — deploys the real schema and dunnage procedures into a
  scratch schema, replays the dunnage seed twice, and asserts the row counts do not move and no custom
  field references a missing type.

The integration test needs `MTM_TEST_MYSQL_CONNECTION_STRING` (or `MTM_MYSQL_CONNECTION_STRING`) and
creates then drops its own `mtm_seedtest_<hex>` schema; with no connection string it passes without
connecting.

```shell
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj \
  --filter "FullyQualifiedName~MTM_Database_Deployment|FullyQualifiedName~DunnageSeedIdempotencyIntegrationTests"
```


## Known Constraints

- MySQL only. The metadata queries and DDL syntax are MySQL-specific.
- Column renames are seen as one drop plus one add; the tool cannot infer intent, so treat a rename as a deliberate, confirmed change.
- A table whose primary key is added later cannot be altered in place by MySQL; use rebuild mode for that case.
- The comparer ignores table-level options such as engine, charset, and comments; only columns, indexes, and foreign keys are compared.
- Write-audit columns are the responsibility of the procedure or seed, not the deployment tool.

## See Also

- [Database object catalog](./database-object-catalog.md)
- [Deployment SQL tree](../Database/Database_Deployment/Sql_Files)
- [Test project](../MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj)
