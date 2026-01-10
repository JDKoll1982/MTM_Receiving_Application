# Prompt: Setup MCP Server(s) + Generate `{mcpserver}.instructions.md` (Any Project)

Use this prompt in VS Code (Copilot Chat, Agent mode). It is designed to work in **any** repo.

---

## Goal
Either:
1) Install and configure a single MCP server named `{mcpserver}` in VS Code, verify it starts, and generate `{mcpserver}.instructions.md`, **or**
2) Read the repo’s `.vscode/mcp.json` and generate a `{serverName}.instructions.md` file for **every server** already configured there.

## Inputs you must ask me for
1. `{mcpserver}` name (short, camelCase)
2. Install method:
   - Registry
   - Manual `.vscode/mcp.json`
3. Server transport:
   - `http` (hosted)
   - `stdio` (local)
4. Runtime:
   - `npx` (Node)
   - `docker`
   - `python` / `uvx`
5. Allowed paths (if filesystem-like server)
6. Secrets required (PATs, API keys, etc.)

If I choose mode (2) “generate for all servers”, you must ask:
- Where to write the instruction files (e.g., `docs/mcp/` or `.github/instructions/`)
- Whether to include a “safe defaults” section per server

## Required steps (do not skip)
1. **Discover official install snippet**
   - Find the server’s official repo/README and copy the recommended VS Code config block.
2. **Update MCP config**
   - Add the server to `.vscode/mcp.json` (workspace) OR user MCP config.
   - If secrets are needed, add them via `inputs` and reference `${input:...}`.
3. **Install prerequisites**
   - If Node-based: ensure Node 18+ is available; run `npm` only if needed.
   - If Docker-based: confirm Docker Desktop is running.
4. **Start and validate**
   - Restart the server from `MCP: List Servers`.
   - Run at least one safe tool call to verify it works.
5. **Generate instruction file**
   Create `{mcpserver}.instructions.md` containing:
   - What the server is for
   - How to install (config snippet)
   - How to start/restart
   - Tool list (ALL tools exposed by that server)
   - At least 1–3 use cases per tool
   - Safety notes
   - Example prompts

## If generating instructions for ALL servers in `.vscode/mcp.json`
1. Read `.vscode/mcp.json` and list all server keys under `servers`.
2. For each server key:
   - Determine if it’s `http/sse` vs `stdio`.
   - Document prerequisites (Node/Docker/Python).
   - Start/restart the server.
   - Enumerate all tools exposed by that server (via VS Code MCP tool discovery).
   - Generate `{serverKey}.instructions.md`.
3. Validate that every configured server has a corresponding instructions file.

## Output format rules
- The instruction file must be fully self-contained.
- Do not hardcode secrets.
- If the server has many tools, group them by category.

## One-shot template you can reuse
"Install MCP server `{mcpserver}` for this repo. Use the official VS Code config. Put configuration in `.vscode/mcp.json` (workspace), using `inputs` for secrets. Start/restart it and validate it works with a safe call. Then generate `{mcpserver}.instructions.md` that includes: install steps, tool list (no omissions), use cases per tool, safety notes, and example prompts."

"OR: read `.vscode/mcp.json` and generate `{serverKey}.instructions.md` for every configured server under `servers`, ensuring no server is skipped."
