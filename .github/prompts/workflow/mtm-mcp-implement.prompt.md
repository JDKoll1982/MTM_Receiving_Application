---
name: mcp-implement
description: "Implement changes using MCP-first workflows (Serena + filesystem + optional GitHub/Playwright)."
argument-hint: "Describe what you want implemented"
agent: MCP Operator
tools:
  - edit
  - search
  - fileSearch
  - textSearch
  - usages
  - changes
  - problems
  - todos
  - filesystem/*
  - oraios/serena/*
  - githubRemote/*
  - githubLocal/*
  - playwright/*
---

# MCP Implement

You are implementing a change in this workspace.

- Use MCP tools to confirm context before editing (avoid assumptions).
- Prefer Serena MCP for symbol-aware navigation and references.
- Prefer filesystem MCP for reads/writes.
- Prefer githubRemote MCP for GitHub actions; use githubLocal (Docker) only if needed.
- Use Playwright MCP for UI/web smoke tests when relevant.

Proceed end-to-end: implement, verify (build/tests when feasible), and summarize results.
