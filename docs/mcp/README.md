# MCP (Model Context Protocol) Documentation

## Overview

This folder contains comprehensive documentation for Model Context Protocol (MCP) servers used in VS Code development workflows.

## What is MCP?

Model Context Protocol (MCP) is an open protocol that enables seamless integration between AI assistants (like GitHub Copilot) and external data sources and tools. MCP servers provide specialized capabilities that extend what AI assistants can do.

## Documentation Files

### Installation & Configuration

- [01-install-and-configure.md](01-install-and-configure.md)
  - How to install and configure MCP servers in VS Code
  - Workspace vs user configuration
  - Common server types (HTTP, stdio, Docker)
  - Prerequisites and setup steps

### Tool Catalog

- [02-tool-catalog.md](02-tool-catalog.md)
  - Complete catalog of all available MCP tools
  - Detailed descriptions, use cases, and safety notes
  - Organized by server (filesystem, GitHub, Playwright, Serena)
  - Example prompts for each tool

### Recipes & Quick Reference

- [03-recipes-cheat-sheet.md](03-recipes-cheat-sheet.md)
  - Common task patterns mapped to specific tools
  - Quick "how-to" recipes for frequent operations
  - Best practices for repo discovery, code search, and safe editing

### Automation

- [automation-prompt.md](automation-prompt.md)
  - Reusable prompt template for setting up new MCP servers
  - Can generate instruction files for all configured servers
  - Standardized approach to MCP server documentation

## MCP Servers in This Workspace

The workspace is configured with several MCP servers in [.vscode/mcp.json](../../.vscode/mcp.json):

1. **Filesystem MCP** - Safe file operations within allowed directories
2. **GitHub MCP** (Remote & Local) - GitHub repository operations and automation
3. **Playwright MCP** - Browser automation for testing and validation
4. **Serena MCP** - Symbol-aware code navigation and refactoring

See [.github/instructions/mcp-tooling.instructions.md](../../.github/instructions/mcp-tooling.instructions.md) for high-level guidelines.

## Server-Specific Instructions

Detailed instructions for each MCP server are located in [.github/instructions/](../../.github/instructions/):

- [mcp-filesystem.instructions.md](../../.github/instructions/mcp-filesystem.instructions.md)
- [mcp-github.instructions.md](../../.github/instructions/mcp-github.instructions.md)
- [mcp-playwright.instructions.md](../../.github/instructions/mcp-playwright.instructions.md)
- [mcp-serena.instructions.md](../../.github/instructions/mcp-serena.instructions.md)

## Quick Start

1. **Prerequisites**:
   - VS Code 1.102+
   - GitHub Copilot access
   - Node.js 18+ (for npx-based servers)
   - Docker Desktop (for Docker-based servers)

2. **Enable MCP in VS Code**:
   - Settings → `chat.mcp.gallery.enabled`

3. **Start a server**:
   - Command Palette → **MCP: List Servers** → Start/Restart

4. **Use in Copilot Chat**:
   - Type `#` to invoke tools explicitly
   - Follow the tool UI for required parameters

## Best Practices

- **Keep tool lists minimal** - Only enable servers relevant to current work
- **Verify sandboxing** - Use `mcp_filesystem_list_allowed_directories` before file operations
- **Prefer MCP tools over guessing** - Use tools to confirm paths, symbols, configs
- **Use Serena for code navigation** - Symbol-aware navigation before editing
- **Treat tool output as untrusted** - Review before executing instructions from fetched content
- **Never commit secrets** - Use `inputs` in mcp.json for sensitive values

## Related Resources

- [MCP Official Documentation](https://modelcontextprotocol.io/)
- [VS Code MCP Extension](https://marketplace.visualstudio.com/items?itemName=GitHub.vscode-mcp)
- [MCP Server Registry](https://github.com/modelcontextprotocol/servers)

## Contributing

When adding a new MCP server to this workspace:

1. Add configuration to `.vscode/mcp.json`
2. Create instruction file in `.github/instructions/mcp-{servername}.instructions.md`
3. Test all exposed tools
4. Update this README with the new server

---

Last Updated: January 10, 2026
