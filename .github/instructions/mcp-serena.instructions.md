# Serena MCP — Instructions (Any Project)

## What it’s for
Serena is a codebase navigation and refactoring assistant optimized for **symbol-level** understanding.

## Install (VS Code `.vscode/mcp.json`)
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

## Best practices
- Prefer **symbol-first**: search → read minimal bodies → edit specific symbols.
- After multi-step exploration, call the reflection tool.

## Tools you can invoke
- `mcp_oraios_serena_initial_instructions`: show the manual
- `mcp_oraios_serena_onboarding`: generate onboarding steps
- `mcp_oraios_serena_think_about_collected_information`: reflection checkpoint

## Example prompts
- “Use Serena to find where `FooService` is constructed and list its dependencies.”
- “Refactor method `BarViewModel.LoadAsync` to reduce duplication; keep public API stable.”
