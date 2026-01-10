# MCP Guide 01 — Install & Configure MCP Servers in VS Code (Any Project)

This guide is **project-agnostic**. It shows how to install/configure MCP servers in VS Code and how to validate they are working.

## What you need
- VS Code **1.102+** (newer is better)
- GitHub Copilot access (for Copilot Chat + Agent mode)
- For local servers:
  - Node.js **18+** (for `npx`/Node-based MCP servers)
  - Docker Desktop (for containerized MCP servers)
  - Python/uv/uvx (only if you use Python-based servers like Serena)

## Where MCP configuration lives
VS Code supports MCP servers at:
- **Workspace** config: `.vscode/mcp.json` (checked into your repo)
- **User** config: Command Palette → **MCP: Open User Configuration**

### Basic format
```jsonc
{
  "servers": {
    "serverName": {
      "type": "stdio" | "http" | "sse",
      "command": "...",
      "args": ["..."],
      "env": { "KEY": "VALUE" },
      "envFile": "${workspaceFolder}/.env"
    }
  },
  "inputs": [
    {
      "type": "promptString",
      "id": "someSecret",
      "description": "API Key",
      "password": true
    }
  ]
}
```

## Installing MCP servers (recommended order)

### Option A (Recommended): Install from the MCP registry in VS Code
1. Settings: enable `chat.mcp.gallery.enabled`
2. Extensions view → search `@mcp`
3. Install in workspace or user profile

### Option B: Manual config (works for any MCP server)
1. Create/edit `.vscode/mcp.json`
2. Add the server config under `servers`
3. Restart it: Command Palette → **MCP: List Servers** → Start/Restart

## Common MCP server types

### 1) Remote (HTTP) servers
Use when the server is hosted (no local install). Example: GitHub hosted MCP.

```jsonc
{
  "servers": {
    "githubRemote": {
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/"
    }
  }
}
```

### 2) Local stdio servers
These run on your machine and talk via stdin/stdout.

#### Node-based (NPX)
```jsonc
{
  "servers": {
    "playwright": {
      "type": "stdio",
      "command": "npx",
      "args": ["@playwright/mcp@latest"]
    }
  }
}
```

#### Docker-based
```jsonc
{
  "servers": {
    "githubLocal": {
      "type": "stdio",
      "command": "docker",
      "args": [
        "run", "-i", "--rm",
        "-e", "GITHUB_PERSONAL_ACCESS_TOKEN",
        "ghcr.io/github/github-mcp-server"
      ],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "${input:github_token}"
      }
    }
  },
  "inputs": [
    {
      "type": "promptString",
      "id": "github_token",
      "description": "GitHub Personal Access Token",
      "password": true
    }
  ]
}
```

## Server-specific install guides (copy/paste)

### GitHub MCP — hosted (no PAT; simplest)
Use when your VS Code + Copilot supports remote MCP.
```jsonc
{
  "servers": {
    "githubRemote": {
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/"
    }
  }
}
```

### GitHub MCP — local via Docker (PAT required)
Use when you need local control or your environment blocks remote MCP.
```jsonc
{
  "inputs": [
    {
      "type": "promptString",
      "id": "github_token",
      "description": "GitHub Personal Access Token",
      "password": true
    }
  ],
  "servers": {
    "githubLocal": {
      "type": "stdio",
      "command": "docker",
      "args": [
        "run", "-i", "--rm",
        "-e", "GITHUB_PERSONAL_ACCESS_TOKEN",
        "ghcr.io/github/github-mcp-server"
      ],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "${input:github_token}"
      }
    }
  }
}
```

### Playwright MCP (browser automation)
Prereq: Node.js 18+
```jsonc
{
  "servers": {
    "playwright": {
      "type": "stdio",
      "command": "npx",
      "args": ["@playwright/mcp@latest"]
    }
  }
}
```

### Filesystem MCP (safe, scoped access)
Prereq: Node.js 18+
```jsonc
{
  "servers": {
    "filesystem": {
      "type": "stdio",
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-filesystem",
        "${workspaceFolder}"
      ]
    }
  }
}
```

### Serena (codebase analysis/navigation)
Prereq: `uvx` available
```jsonc
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
  }
}
```

## How to start/restart and validate servers
1. Command Palette → **MCP: List Servers**
2. Pick a server → **Start**
3. Open Chat → Tools picker → ensure the server’s tools are enabled
4. Run a safe validation prompt, e.g.:
   - filesystem: “List allowed directories”
   - github: “List my repos”
   - playwright: “Open example.com and extract heading text”

## Security checklist (do this in every project)
- Prefer **scoped filesystem roots** (only workspace folders)
- Use `inputs` for secrets; never hardcode tokens
- Prefer short-lived tokens (30–90 days)
- Use read-only modes when possible
- Treat local MCP servers as code execution (only run trusted servers)
