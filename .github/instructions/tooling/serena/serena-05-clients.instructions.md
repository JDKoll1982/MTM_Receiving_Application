---
applyTo: "**"
description: >
  How to connect Serena MCP server to MCP clients — VSCode (primary for MTM),
  Claude Code, Claude Desktop, Codex, JetBrains, and other clients.
---

<!-- 
[DOC-META-START]
- File Name: serena-05-clients.instructions.md
- Description: How to connect Serena MCP server to MCP clients — VSCode (primary for MTM), Claude Code, Claude Desktop, Codex, JetBrains, and other clients.
- Last Updated: 2026-11-12
- Quick TOC:
  - Line 27-36: # Connecting Serena to MCP Clients
  - Line 37-91: ## VSCode (Primary for MTM Project)
  - Line 92-129: ## Claude Code
  - Line 130-156: ## Claude Desktop
  - Line 157-179: ## Codex (OpenAI)
  - Line 180-193: ## JetBrains Junie / AI Assistant
  - Line 194-219: ## Antigravity and Other Clients
  - Line 220-232: ## Context Selection Guide
  - Line 233-240: ## MTM VSCode Setup Checklist
- Critical Notes: VSCode shares one long-lived Serena HTTP server on port 24300 (.serena/start-serena-http.ps1); other clients still use uvx stdio.
[DOC-META-END]
-->

# Connecting Serena to MCP Clients

Official docs: <https://oraios.github.io/serena/02-usage/030_clients.html>

All configurations launch Serena as a subprocess using `uvx` in **stdio** mode, except the
MTM workspace itself, which shares a single long-lived HTTP server across chat sessions.
Adapt the command if you prefer a local installation or Docker.

---

## VSCode (Primary for MTM Project)

VSCode GitHub Copilot supports MCP tools natively. Configure Serena via `.vscode/mcp.json`
in the project root.

> **Recommended context for VSCode:** `ide`
> The `ide` context disables tools that duplicate VS Code's built-in file/shell capabilities,
> reducing noise and saving tokens.

**`.vscode/mcp.json`:**

```json
{
  "servers": {
    "oraios/serena": {
      "type": "http",
      "url": "http://127.0.0.1:24300/mcp"
    }
  }
}
```

**Shared-server model (required for MTM):**

A `stdio` configuration spawns a brand-new Serena process for every chat session, leaving
orphaned processes and dashboard ports behind. MTM instead runs one long-lived Serena HTTP
server and points every chat at it:

| File                             | Purpose                                                              |
| -------------------------------- | -------------------------------------------------------------------- |
| `.vscode/mcp.json`               | Connects chats to `http://127.0.0.1:24300/mcp` (no process spawn)    |
| `.vscode/tasks.json`             | `runOn: folderOpen` task that ensures the server is up               |
| `.serena/start-serena-http.ps1`  | Idempotent launcher — reuses a running server, starts only if down   |
| `.serena/stop-serena-http.ps1`   | Stops the shared server                                              |

**What this does:**

- Starts Serena at most once, not once per chat session
- Activates the MTM project immediately (via `--project <workspace>`)
- Uses `ide` context which skips file/shell tools that VS Code handles natively
- Passes `--open-web-dashboard false` so no browser tab opens on each launch

Notes:

- VS Code asks once whether to allow automatic tasks. Accept it, or run
  **Tasks: Run Task → Serena: ensure MCP server is running** manually after opening the workspace.
- Launcher output goes to `.serena/logs/mcp-server.log` and `.serena/logs/mcp-server.err.log`
  (both gitignored).
- Use the **Serena: restart MCP server** and **Serena: stop MCP server** tasks when the server
  needs recycling.
- Only one project can be active per Serena instance, so a shared server is correct only because
  this workspace is the only project connected to it.

---

## Claude Code

Claude Code supports Serena with per-project or global configuration.

### Per-Project Configuration

```bash
# Run this in the project directory
claude mcp add serena -- uvx \
  --from git+https://github.com/oraios/serena \
  serena start-mcp-server \
  --context claude-code \
  --project "$(pwd)"
```

### Global Configuration (Works in Any Project)

```bash
claude mcp add --scope user serena -- uvx \
  --from git+https://github.com/oraios/serena \
  serena start-mcp-server \
  --context claude-code \
  --project-from-cwd
```

`--project-from-cwd` searches upward from Claude Code's working directory for `.serena/project.yml`
or `.git`, activating the containing directory as the project root.

**`claude-code` context:** Disables tools that duplicate Claude Code's built-in capabilities
(file reading, shell execution), so only Serena's unique symbol-level tools are active.

### Token Efficiency (Claude Code v2.0.74+)

Set `"type": "deferred"` in Claude Code's MCP config to enable on-demand tool loading.
This avoids sending all tool descriptions upfront, saving context tokens.

---

## Claude Desktop

Edit `claude_desktop_config.json` (at `%APPDATA%\Claude\claude_desktop_config.json` on Windows):

```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena",
        "start-mcp-server",
        "--project",
        "C:\\Users\\johnk\\source\\repos\\MTM_Receiving_Application"
      ]
    }
  }
}
```

**Note:** Claude Desktop uses the `desktop-app` context (default), which includes the full
toolset including file operations. Use explicit `--context ide` when VSCode is also open.

---

## Codex (OpenAI)

```json
{
  "mcpServers": {
    "serena": {
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena",
        "start-mcp-server",
        "--context",
        "codex",
        "--project-from-cwd"
      ]
    }
  }
}
```

---

## JetBrains Junie / AI Assistant

When using JetBrains Junie (Rider, IntelliJ, etc.) with the Serena JetBrains Plugin:

1. Install the **Serena plugin** from the JetBrains Marketplace
2. Open the MTM project in Rider
3. The plugin handles language analysis; set `language_backend: jetbrains` in `.serena/project.yml`
4. Configure Junie's MCP connection to point to Serena

See the [JetBrains plugin docs](https://oraios.github.io/serena/02-usage/025_jetbrains_plugin.html)
for detailed setup.

---

## Antigravity and Other Clients

For any MCP-compatible client, use the general pattern:

```json
{
  "command": "uvx",
  "args": [
    "--from",
    "git+https://github.com/oraios/serena",
    "serena",
    "start-mcp-server",
    "--project",
    "/path/to/project"
  ]
}
```

For clients using OpenAI-compatible tool descriptions, specify the `oaicompat-agent` context:

```bash
serena start-mcp-server --context oaicompat-agent
```

---

## Context Selection Guide

| Client               | Recommended Context     | Reason                                           |
| -------------------- | ----------------------- | ------------------------------------------------ |
| **VSCode Copilot**   | `ide`                   | VS Code handles files/shell; Serena adds symbols |
| **Claude Code**      | `claude-code`           | Claude Code has built-in file/shell tools        |
| **Claude Desktop**   | `desktop-app` (default) | Full Serena toolset needed                       |
| **Codex**            | `codex`                 | OpenAI Codex optimizations                       |
| **JetBrains Junie**  | `ide`                   | IDE handles most file operations                 |
| **Standalone agent** | `agent`                 | Full toolset for autonomous operation            |

---

## MTM VSCode Setup Checklist

- [ ] `uv` is installed (`winget install astral-sh.uv`)
- [ ] `.vscode/mcp.json` exists with the HTTP configuration above
- [ ] `.vscode/tasks.json` contains the `Serena: ensure MCP server is running` folder-open task
- [ ] Automatic tasks are allowed (or the ensure task was run manually once)
- [ ] `.serena/start-serena-http.ps1` reports `reusing it` on a second run
- [ ] `.serena/project.yml` exists (run `serena project create` if not)
- [ ] Project is indexed (`serena project index` — one-time setup)
- [ ] Memories exist in `.serena/memories/` (populated by onboarding)
- [ ] VSCode's "Copilot Chat" sidebar shows Serena tools in the agent mode
