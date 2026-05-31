---
applyTo: "**"
description: >
  How to connect Serena MCP server to MCP clients — VSCode (primary for MTM),
  Claude Code, Claude Desktop, Codex, JetBrains, and other clients.
---

# Connecting Serena to MCP Clients

Official docs: <https://oraios.github.io/serena/02-usage/030_clients.html>

All configurations launch Serena as a subprocess using `uvx` in **stdio** mode.
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
      "type": "stdio",
      "command": "uvx",
      "args": [
        "--from",
        "git+https://github.com/oraios/serena",
        "serena",
        "start-mcp-server",
        "--context",
        "ide",
        "--project",
        "${workspaceFolder}"
      ]
    }
  },
  "inputs": []
}
```

**What this does:**

- Starts Serena automatically when VS Code opens this workspace
- Activates the MTM project immediately (via `--project ${workspaceFolder}`)
- Uses `ide` context which skips file/shell tools that VS Code handles natively

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
- [ ] `.vscode/mcp.json` exists with the configuration above
- [ ] `.serena/project.yml` exists (run `serena project create` if not)
- [ ] Project is indexed (`serena project index` — one-time setup)
- [ ] Memories exist in `.serena/memories/` (populated by onboarding)
- [ ] VSCode's "Copilot Chat" sidebar shows Serena tools in the agent mode
