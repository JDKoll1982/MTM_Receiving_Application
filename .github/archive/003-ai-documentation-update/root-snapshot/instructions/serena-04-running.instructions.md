---
applyTo: "**"
description: >
  How to run Serena — uvx (recommended), local install, Docker, Nix;
  MCP server modes (stdio, HTTP); and all command-line arguments.
---

# Running Serena

Official docs: <https://oraios.github.io/serena/02-usage/020_running.html>

Serena is a command-line tool. This page covers the main ways to run it, how to start
the MCP server, and available command-line options.

---

## Ways of Running Serena

### Option 1: uvx (Recommended — No Installation Required)

`uvx` is part of `uv` (<https://github.com/astral-sh/uv>). It runs the latest version of
Serena directly from GitHub **without installing it locally**.

```bash
# Install uv (once)
winget install astral-sh.uv

# Run any Serena command
uvx --from git+https://github.com/oraios/serena serena <command>

# Examples
uvx --from git+https://github.com/oraios/serena serena --help
uvx --from git+https://github.com/oraios/serena serena start-mcp-server
uvx --from git+https://github.com/oraios/serena serena project create --language csharp
uvx --from git+https://github.com/oraios/serena serena project index
```

The `<serena>` placeholder in Serena's documentation refers to this full `uvx` command.

### Option 2: Local Installation

```bash
# Install globally with uv
uv tool install git+https://github.com/oraios/serena

# Then run directly
serena start-mcp-server
serena project create --language csharp
```

### Option 3: Nix

```bash
nix run github:oraios/serena -- <command> [options]
```

### Option 4: Docker

```bash
# Pull and run Serena in a container (sandboxed execution)
docker run -it --rm \
  -v /path/to/project:/workspace \
  ghcr.io/oraios/serena:latest \
  start-mcp-server --project /workspace
```

Docker is recommended for security-sensitive environments where you want to isolate
Serena's shell execution capability.

---

## Starting the MCP Server

The MCP server is the main thing you run when using Serena with an AI agent.

```bash
serena start-mcp-server [options]
```

By default, Serena:

- Uses **stdio** transport (client starts the server as a subprocess)
- Opens the **web dashboard** at `http://localhost:24282/dashboard/index.html`
- Uses the **desktop-app** context (full toolset)

### Transport Modes

#### Standard I/O (Default)

The client (VSCode, Claude Desktop, etc.) starts Serena as a subprocess. Communication
happens via stdin/stdout. **This is the default for all client configurations.**

No extra setup needed — just provide the launch command to the MCP client.

#### Streamable HTTP

For scenarios where multiple agents share one Serena instance:

```bash
serena start-mcp-server --transport streamable-http --port 8765
# Access at http://localhost:8765
```

**⚠️ Caution:** Only one project can be active at a time in a single Serena instance.
If multiple agents need different projects, run separate instances with stdio.

---

## Key Command-Line Arguments

Add `--help` to any command for a full list of options.

```bash
serena start-mcp-server --help
```

| Argument                               | Description                                           | MTM Example                                    |
| -------------------------------------- | ----------------------------------------------------- | ---------------------------------------------- |
| `--project <path\|name>`               | Project to activate at startup                        | `--project "C:\...\MTM_Receiving_Application"` |
| `--project-from-cwd`                   | Auto-detect project from current dir (for CLI agents) | Used in Claude Code                            |
| `--context <context>`                  | Operating context (see configuration)                 | `--context ide` (VSCode)                       |
| `--mode <mode>`                        | Activate a mode (repeatable)                          | `--mode editing --mode interactive`            |
| `--language-backend JetBrains`         | Use JetBrains plugin instead of LSP                   | When using Rider                               |
| `--open-web-dashboard <true\|false>`   | Control auto-open of dashboard                        | `--open-web-dashboard false`                   |
| `--transport <stdio\|streamable-http>` | MCP transport protocol                                | `--transport streamable-http`                  |

---

## Other Useful Commands

```bash
# Get help on all commands
serena --help

# Project management
serena project create --language csharp --name "MyProject"
serena project index
serena project list

# Configuration management
serena config edit            # Open global config in editor

# Context and mode management
serena context list
serena context create <name>
serena mode list
serena mode create <name>

# Start project-level server (for multi-project queries)
serena start-project-server
```

---

## MTM-Recommended Launch Configuration

For the MTM project in VSCode (`ide` context):

```bash
uvx \
  --from git+https://github.com/oraios/serena \
  serena start-mcp-server \
  --context ide \
  --project "C:\Users\johnk\source\repos\MTM_Receiving_Application" \
  --open-web-dashboard false
```

Or as a VSCode `mcp.json` entry (auto-started by VSCode):

```json
{
  "servers": {
    "serena": {
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
  }
}
```

---

## Performance: With vs Without Indexing

| Metric                | Without Index | With Index  |
| --------------------- | ------------- | ----------- |
| First tool call       | 5-10 seconds  | 1-2 seconds |
| Subsequent calls      | 2-5 seconds   | <1 second   |
| 10-tool session total | ~50 seconds   | ~15 seconds |

**Run index once after project creation:**

```bash
cd C:\Users\johnk\source\repos\MTM_Receiving_Application
uvx --from git+https://github.com/oraios/serena serena project index
```

Re-index only after major refactoring (50+ files changed) or if tools slow down.
