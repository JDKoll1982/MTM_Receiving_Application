<!-- 
[DOC-META-START]
- File Name: serena-tool-compatibility.md
- Description: Serena tool compatibility notes for this environment, plus which tool owns which job now that Copilot CLI plugins are installed.
- Last Updated: 2026-11-11
- Quick TOC:
  - Line 12-16: # Serena Tool Compatibility
  - Line 18-30: # Copilot CLI Plugin Routing
- Critical Notes: Serena owns symbol editing; the dotnet plugin's Roslyn LSP owns diagnostics.
[DOC-META-END]
-->

# Serena Tool Compatibility

- Upgraded Serena no longer accepts legacy optional tool names like `think_about_collected_information`, `think_about_task_adherence`, `think_about_whether_you_are_done`, `summarize_changes`, or `prepare_for_new_conversation` in `.serena/project.yml`.
- Keep `included_optional_tools` limited to tool names returned by the current `serena tools list` CLI.
- Verified compatible project setting on 2026-04-11: `included_optional_tools: [execute_shell_command]`.
- Current setting verified 2026-11-11: `included_optional_tools: [restart_language_server, open_dashboard]`.

## Copilot CLI Plugin Routing

Ten Tier 1 plugins from `awesome-copilot` are installed user-scoped under
`~/.copilot/installed-plugins/awesome-copilot/` (verified 2026-11-11). They add no hooks and no
MCP servers, so `.vscode/mcp.json`, `.github/hooks/speckit.json`, and `~/.copilot/mcp-config.json`
are unchanged.

- **Two C# language servers run concurrently.** Serena starts its own Roslyn server
  (`language_servers: [csharp]`) and the `dotnet` plugin starts
  `dnx roslyn-language-server --prerelease`. This is intentional.
- **Ownership split:** use Serena for symbol-level work — `find_symbol`,
  `get_symbols_overview`, `find_referencing_symbols`, `replace_symbol_body`, `rename_symbol`. Use
  the `dotnet` plugin LSP for diagnostics, hover types, and go-to-definition. Do not restart
  Serena's language server to fix a diagnostics problem; check the `dotnet` plugin LSP instead.
- If LSP memory becomes a problem, the documented fallback is to narrow one server rather than
  disable both. Serena tooling must keep working, so drop the `dotnet` plugin LSP first.
- `winapp` CLI 0.8.0 (`Microsoft.WinAppCli`, winget, per-user) is installed and on PATH. The
  `winui-*` and `winapp-*` skills depend on it. Note `winapp --version` prints nothing; use
  `winapp --help` to confirm the version.
- `winui` and `winappcli` both ship from `microsoft/WinAppCli` with overlapping skills
  (`winui-packaging` vs `winapp-package`, `winui-setup` vs `winapp-setup`,
  `winui-ui-testing` vs `winapp-ui-automation`). Name the specific skill in prompts so routing is
  deterministic.
