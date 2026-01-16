# Module Agent Forge

Purpose: Creates and registers module agents across projects, audits templates, and validates the Copilot agents registry.

Capabilities:
- Run full agent creation workflow (agents + templates)
- Register module agents in `.github/copilot-agents.json`
- Audit `_bmad/module-agents/templates` and create placeholders
- Validate JSON and instruction paths; prompt to reload VS Code

Usage Hints:
- Use commands: AR (run-all), RG (register-agents), TA (audit-templates), VA (validate-setup)
- Assumes Serena MCP and Filesystem MCP are available for discovery and edits
