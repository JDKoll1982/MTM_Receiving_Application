# AGENT-PLUGIN-RECOMMENDATIONS

Plugin recommendations for the MTM Receiving Application based on an inventory of this repo's
stack (WinUI 3 / Windows App SDK, .NET 10, C# 13, MySQL 5.7, read-only Infor Visual SQL Server,
xUnit + FluentAssertions, Spec Kit, Serilog, OpenTelemetry) and the currently enabled tooling
(Serena MCP, Playwright MCP, Microsoft Learn MCP — no CLI plugins installed).

All plugins listed below come from the free, open-source `github/awesome-copilot` marketplace, so
there is no license cost. They run through the `copilot` CLI, which is already installed on this
machine.

- Marketplace: `github/awesome-copilot` (registered by default)
- All 10 Tier 1 plugins are now installed (verified 2026-11-11 via `copilot plugin list`).

## Installation Status

Verified with `copilot plugin list` on 2026-11-11.

| Plugin | Version | Status |
| --- | --- | --- |
| `dotnet` | 0.2.5 | Installed — registers a C# Roslyn LSP via `dnx` |
| `winui` | 0.8.0 | Installed — 7 skills, requires WinApp CLI |
| `winappcli` | 0.8.0 | Installed — 14 skills, requires WinApp CLI |
| `csharp-dotnet-development` | 1.2.0 | Installed |
| `dotnet-test` | 1.0.0 | Installed — 10 agents, 0 hooks, 0 MCP servers |
| `testing-automation` | 1.0.0 | Installed |
| `database-data-management` | 1.0.0 | Installed |
| `speckit-companion` | 0.2.1 | Installed — registers the `speckit-spec-board` canvas |
| `dependabot-triage` | 1.0.0 | Installed — registers the `dependabot-triage` canvas |
| `github-security` | 0.2.2 | Installed — was missing from the first batch, added after verification |

Tier 2 plugins installed alongside Tier 1 (detected 2026-11-11):

| Plugin | Version | Status |
| --- | --- | --- |
| `project-documenter` | 1.0.0 | Installed |
| `context-engineering` | 1.0.0 | Installed |
| `diagram-viewer` | 1.0.2 | Installed |

Post-install checks performed:

- 71 plugin skills loaded across all installed plugins; **no** name collisions with the 37 repo
  skills in [.github/Skills](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/.github/Skills>).
- No plugin contributes hooks or MCP servers, so [.github/hooks/speckit.json](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/.github/hooks/speckit.json>),
  `.vscode/mcp.json`, and `~/.copilot/mcp-config.json` are unchanged.
- Installs are user-scoped under `~/.copilot/installed-plugins/`, so no plugin files entered the repo.
- `dotnet --version` returns 11.0.100, with 10.0.303 and 10.0.404 also present; the plugin's bundled
  `global.json` pins 10.0.100 with `rollForward: latestMajor`, which resolves cleanly. `dnx` exists at
  `C:\Program Files\dotnet\dnx.cmd`.
- `winapp --help` reports Windows App Development CLI 0.8.0, resolved at
  `C:\Users\jkoll\AppData\Local\Microsoft\WindowsApps\winapp.exe`.

### Outstanding Follow-Ups

| Item | Detail | Action |
| --- | --- | --- |
| WinApp CLI missing | Resolved 2026-11-11 | `Microsoft.WinAppCli` 0.8.0 installed via winget, per-user, on PATH. `winapp --version` prints nothing — use `winapp --help` to confirm. The six dependent skills now run natively |
| Two C# language servers | Decided 2026-11-11: keep both, split by job | Serena owns symbol work (`find_symbol`, `get_symbols_overview`, `find_referencing_symbols`, `replace_symbol_body`, `rename_symbol`); the `dotnet` plugin LSP owns diagnostics, hover types, and go-to-definition. If memory pressure appears, drop the `dotnet` LSP first and keep Serena |
| Overlapping Windows skills | `winui` and `winappcli` both ship from `microsoft/WinAppCli`. `winui-packaging` and `winapp-package`, `winui-setup` and `winapp-setup`, `winui-ui-testing` and `winapp-ui-automation` cover near-identical ground | Harmless duplication, but name the specific skill when authoring prompts so routing is deterministic |

The routing decision is recorded durably in
[.serena/memories/serena-tool-compatibility.md](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/.serena/memories/serena-tool-compatibility.md>)
so future sessions do not re-litigate it.

## Tier 1 — Install First

Highest fit for this repo. Each one covers work the repo already does every day. All 10 are now
installed — see [Installation Status](#installation-status).

| Plugin | What It Adds | Why It Fits This Repo | Install |
| --- | --- | --- | --- |
| `dotnet` | C# language server (LSP) integration plus high-level .NET development skills | Repo has no C# LSP wired anywhere in `.vscode/mcp.json` or `~/.copilot/mcp-config.json`; Serena covers symbol edits but not full LSP diagnostics/navigation for a 300+ file solution | `copilot plugin install dotnet@awesome-copilot` |
| `winui` | WinUI 3 agents and skills: new app creation, framework conversion, feature addition, MSIX packaging, code signing, UI automation testing, Windows App SDK guidance | This is the primary stack (`UseWinUI=true`, `net10.0-windows10.0.22621.0`, Windows App SDK 2.4.0) and the repo has strict XAML/`x:Bind` rules that benefit from a WinUI-specialist skill set | `copilot plugin install winui@awesome-copilot` |
| `csharp-dotnet-development` | Prompts, instructions, and chat modes for C#/.NET testing, documentation, and best practices | Complements the existing Roslynator analyzers and CSharpier formatting with agent-facing conventions for the same rules | `copilot plugin install csharp-dotnet-development@awesome-copilot` |
| `dotnet-test` | Running, generating, analyzing, and improving .NET tests: execution, filtering, platform detection, coverage, testability | Repo has [MTM_Receiving_Application.Tests](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/MTM_Receiving_Application.Tests>) with a unit-vs-integration split and `dotnet test` as the documented path; this automates filter selection and coverage review | `copilot plugin install dotnet-test@awesome-copilot` |
| `testing-automation` | Unit, integration, and E2E testing strategy plus TDD workflows | Pairs with the repo's testing-strategy rules (FluentAssertions, `IAsyncLifetime`, `TEST-` prefixed data, `MethodName_ShouldResult_WhenCondition`) | `copilot plugin install testing-automation@awesome-copilot` |
| `database-data-management` | DBA tooling, SQL optimization, and stored-procedure development for SQL Server and general databases | Directly relevant to the dual-database design: stored-procedure-only MySQL writes plus read-only Infor Visual queries, and to the CSV schema reference workflow under `docs/InforVisual/DatabaseCSVFiles` | `copilot plugin install database-data-management@awesome-copilot` |
| `speckit-companion` | Live board of every spec with its specify → plan → tasks → implement pipeline, document links, task progress, and a next-step button | The repo is already Spec Kit native (`.specify/` with extensions, memory, templates, workflows, plus 50+ `speckit.*` agents and prompts in `.github/`); this gives the pipeline a UI instead of hand-driving agents | `copilot plugin install speckit-companion@awesome-copilot` |
| `github-security` | Reviews session changes for vulnerabilities and fixes code scanning alerts | The app handles MySQL credentials, connection strings, and ERP data; a change-scoped security pass per session is cheap insurance | `copilot plugin install github-security@awesome-copilot` |
| `dependabot-triage` | Triages Dependabot alerts and recommends safe, evidence-based dependency updates | The project pins ~30 NuGet packages including previews (`Microsoft.Windows.SDK.NET` 10.0.18362.6-preview, `Microsoft.WindowsAppSDK` 2.4.0) that need careful bumping | `copilot plugin install dependabot-triage@awesome-copilot` |
| `winappcli` | Windows packaging and distribution: MSIX installers, code signing, certificates, Windows SDK/App SDK setup, package identity, `appxmanifest` authoring, Store distribution | The repo carries both [Package.appxmanifest](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/Package.appxmanifest>) and [appxmanifest.xml](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/appxmanifest.xml>) with `WindowsPackageType=None`, so packaging and signing decisions come up repeatedly | `copilot plugin install winappcli@awesome-copilot` |

## Tier 2 — High Value, Install as Needed

Useful for specific work streams rather than every session.

| Plugin | What It Adds | Why It Fits This Repo | Install |
| --- | --- | --- | --- |
| `dotnet-diag` | .NET performance investigation, debugging, and incident analysis skills | The app ships Serilog + OpenTelemetry exporters and LiveCharts dashboards, and has a module performance audit workflow (`agent-performance-audit` skill exists already) | `copilot plugin install dotnet-diag@awesome-copilot` |
| `pr-review` | Read-only PR review canvas with AI-grouped changes, expandable diffs, and per-hunk questions | Repo is on GitHub (`JDKoll1982/MTM_Receiving_Application`) with large multi-module diffs that are hard to review in a flat diff | `copilot plugin install pr-review@awesome-copilot` |
| `arch` | Cited architecture document generation for a local repo plus a phased modernization plan | 14 `Module_*` projects with strict layering rules make architecture documentation and drift checks worth automating | `copilot plugin install arch@awesome-copilot` |
| `project-documenter` | Markdown docs, draw.io diagrams, PNG exports, and a formatted `.docx` discovered from the actual tech stack | Complements the required `Documentation/` folder structure per module with generated diagrams and shareable deliverables | `copilot plugin install project-documenter@awesome-copilot` |
| `accessibility-agents` | WCAG 2.2 AA enforcement via router skills, review hooks, and scanner MCP server | A WinUI 3 desktop app with heavy `x:Bind` views and in-app help; accessibility review currently has no automated gate | `copilot plugin install accessibility-agents@awesome-copilot` |
| `sonarqube` | 7,500+ issue types, secrets scanning, agentic analysis, and quality gates across 40+ languages | Catches SQL-in-strings, hardcoded secrets, and complexity issues that Roslynator does not. Confirm licensing for a private repo before adopting | `copilot plugin install sonarqube@awesome-copilot` |
| `megalinter` | 100+ linters for 69+ languages, run from CI or locally | The repo has C#, SQL, PowerShell, Python, Bash, and Markdown under lint pressure but **no** `.github/workflows` yet; MegaLinter gives a single entry point for all of them | `copilot plugin install megalinter@awesome-copilot` |
| `context-engineering` | Context management guidance, a multi-file change planning agent, and context-aware prompts | Directly addresses the pain of 300+ files and deep `Module_*` nesting that motivated the Serena setup | `copilot plugin install context-engineering@awesome-copilot` |
| `keep-the-why` | Preserves and recovers architectural decisions, rejected alternatives, workarounds, and incident learnings | The repo already keeps `Changes-and-Decisions` docs and Serena memories; this keeps that rationale recoverable from code | `copilot plugin install keep-the-why@awesome-copilot` |
| `technical-spike` | Spike creation, management, and research to retire unknowns before spec and implementation | Fits the existing `speckit.assess.*` front end when a feature has unresolved database or integration unknowns | `copilot plugin install technical-spike@awesome-copilot` |
| `usage-insights` | Copilot token and AI-credit usage with per-agent rollups and session drill-down | Six subagents are configured with per-agent models; this makes the cost of that split visible | `copilot plugin install usage-insights@awesome-copilot` |
| `diagram-viewer` | Renders diagrams with clickable drill-down and agent explanations inside a canvas | Useful for the MVVM layer diagrams and module dependency maps the docs call for | `copilot plugin install diagram-viewer@awesome-copilot` |
| `markdstage` | Create, edit, present, and export Markdown slides in a canvas | Turns the existing `specs/` and module docs into reviewable decks without a separate tool | `copilot plugin install markdstage@awesome-copilot` |

## Tier 3 — Situational or Low Priority

| Plugin | Consider When | Note |
| --- | --- | --- |
| `modernize-dotnet` | A future framework jump is planned | Already on .NET 10, so little immediate value |
| `upgrade-agent` | Migrating a legacy companion app | Same overlap as `modernize-dotnet` |
| `repo-actions-hub` | GitHub Actions workflows get added | No `.github/workflows` exists today |
| `project-planning` | Formal epic/feature breakdown is needed | Partially covered by the `speckit.assess.*` agents already in [.github/agents](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/.github/agents>) |
| `backlog-swipe-triage` | Issue backlog grows | Repo has no active issue triage flow visible |
| `windows-app-storage-inspector-cleanup` | Diagnosing local disk usage from label/XLSX artifacts | Narrow, occasional need |
| `agent-radar` | Auditing what agents and MCP servers touch | Useful once several plugins are installed |
| `token-pacman` | Fun visualization of credit burn | Entertainment, not engineering value |

## Skip These

| Plugin Family | Reason to Skip |
| --- | --- |
| `microsoft-docs` | Redundant — the `microsoftdocs/mcp` server is already in `~/.copilot/mcp-config.json` |
| `chrome-devtools-plugin` | Web/Chrome-only; this is a WinUI 3 desktop app and Playwright MCP is already configured |
| Built-in `browser-use` | Currently disabled and not needed for a desktop client |
| `azure-*`, `aws-*`, `foundry-*`, `dataverse*`, `power-platform-*`, `salesforce-development`, `stripe`, `datadog`, `sentry-triage`, `incident-io` | No matching services in this codebase |
| `java-*`, `python-mcp-development`, `go-mcp-development`, `rust-mcp-development`, `ruby-mcp-development`, `php-mcp-development`, `kotlin-mcp-development`, `typescript-mcp-development` | Wrong stack |
| `react18-upgrade`, `react19-upgrade`, `ember`, `php-*`, `ui5*`, `flutter`-adjacent plugins | Wrong stack |
| `postgres-skills`, `cockroachdb`, `oracle-to-postgres-migration-expert`, `elasticsearch`, `sumo-logic` | Repo uses MySQL 5.7 and SQL Server, not PostgreSQL/Cockroach/Oracle |
| `acreadiness-cockpit`, `cms-development`, `mobile-canvas`, `figma`, `adobe-illustrator` | Out of scope for an internal receiving app |
| `noob-mode`, `demotivational-calendar`, `color-orb`, `backrooms-canvas`, `wiqd` | Novelty plugins with no engineering value here |

## Installation Notes

- Install per user, not per repo, so the plugins also apply to sibling MTM repos:
  `copilot plugin install <name>@awesome-copilot`
- Verify afterwards with `copilot plugin list`.
- `copilot plugin install` supports `owner/repo:path` and direct git URLs, so any plugin not in
  the marketplace can be pulled from its own repository.
- Several plugins (notably `accessibility-agents`, `project-documenter`, and `dotnet`) add MCP
  servers or hooks. Review those additions the same way this repo's
  [.github/hooks/speckit.json](</C:/Users/jkoll/source/repos/MTM_Receiving_Application/.github/hooks/speckit.json>)
  is reviewed, and keep MySQL/SQL Server credentials out of any plugin config.

## Recommended Minimum Set

If installing only five: `dotnet`, `winui`, `dotnet-test`, `database-data-management`,
`speckit-companion`.

## See Also

- <https://github.com/github/awesome-copilot>
- <https://docs.github.com/copilot/concepts/agents/copilot-cli/about-cli-plugins>
