---
applyTo: "**/*.{json,yml,yaml,csproj,props,slnx,manifest,xml}"
description: "Registry and maintenance rules for non-Markdown config/settings files that cannot hold the DOC-META header."
---

<!-- 
[DOC-META-START]
- File Name: non-markdown-config-files.instructions.md
- Description: Registry and maintenance rules for non-Markdown config/settings files that cannot hold the DOC-META header.
- Last Updated: 2026-11-12
- Quick TOC:
  - Line 20-21: # Non-Markdown Config & Settings Files
  - Line 22-25: ## Purpose
  - Line 26-38: ## Registry
  - Line 39-43: ## Maintenance Rules
- Critical Notes: JSON/XAML/YAML/XML/MSBuild cannot hold HTML comments; this file is the authoritative index.
[DOC-META-END]
-->

# Non-Markdown Config & Settings Files

## Purpose

JSON, XAML, YAML, XML, and MSBuild files cannot contain the HTML comment header used for Markdown docs. Keep them documented here instead of trying to embed headers.

## Registry

- `MTM_Receiving_Application.slnx` — solution; contains the app project only (tests are not in the solution)
- `MTM_Receiving_Application.csproj`, `Directory.Build.props` — build configuration
- `appsettings.json`, `appsettings.Development.json` — app config and connection strings (Development is gitignored)
- `Settings.XamlStyler.json` — XAML Styler formatting rules
- `app.manifest`, `appxmanifest.xml`, `Package.appxmanifest` — app identity/capabilities
- `.vscode/launch.json`, `mcp.json`, `settings.json`, `tasks.json` — editor/MCP/task config;
  `mcp.json` points at the shared Serena HTTP server (`http://127.0.0.1:24300/mcp`) and
  `tasks.json` runs the folder-open task that starts it only when it is not already running
- `.serena/project.yml` — Serena project configuration
- `.serena/start-serena-http.ps1`, `.serena/stop-serena-http.ps1` — idempotent launcher/stopper
  for that shared server (port must stay in sync with `.vscode/mcp.json`; logs in `.serena/logs/`)
- `.github/copilot-agents.json` — Copilot agent registration metadata (legacy BMAD agents only; the `speckit.companion.*` agents are discovered from `.github/agents/`)
- `.specify/extensions/companion/extension.yml` — Companion extension manifest; registers the `speckit.companion.*` commands and the spec-kit lifecycle hooks
- `.specify/extensions/companion/commands/.manifest.json` — Companion command-to-artifact map, read by the doctor's "closed without the file it promised" check

## Maintenance Rules

- Never embed a DOC-META header in these files.
- When one of these files changes meaning, update this registry and the referencing instruction file.
- Treat `appsettings.Development.json` as gitignored and secret-bearing.
