---
applyTo: "**/*.{json,yml,yaml,csproj,props,slnx,manifest,xml}"
description: "Registry and maintenance rules for non-Markdown config/settings files that cannot hold the DOC-META header."
---

<!-- 
[DOC-META-START]
- File Name: non-markdown-config-files.instructions.md
- Description: Registry and maintenance rules for non-Markdown config/settings files that cannot hold the DOC-META header.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 20-21: # Non-Markdown Config & Settings Files
  - Line 22-25: ## Purpose
  - Line 26-36: ## Registry
  - Line 37-41: ## Maintenance Rules
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
- `.vscode/launch.json`, `mcp.json`, `settings.json`, `tasks.json` — editor/MCP/task config
- `.serena/project.yml` — Serena project configuration
- `.github/copilot-agents.json` — Copilot agent registration metadata

## Maintenance Rules

- Never embed a DOC-META header in these files.
- When one of these files changes meaning, update this registry and the referencing instruction file.
- Treat `appsettings.Development.json` as gitignored and secret-bearing.
