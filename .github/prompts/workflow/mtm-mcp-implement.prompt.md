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

<!-- 
[DOC-META-START]
- File Name: mtm-mcp-implement.prompt.md
- Description: Implement changes using MCP-first workflows (Serena + filesystem + optional GitHub/Playwright).
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 22: # MCP Implement
- Critical Notes: Use MCP-first tooling (Serena for symbols, filesystem for reads/writes, GitHub/Playwright when relevant) and verify after editing.
[DOC-META-END]
-->

# MCP Implement

You are implementing a change in this workspace.

- Use MCP tools to confirm context before editing (avoid assumptions).
- Prefer Serena MCP for symbol-aware navigation and references.
- Prefer filesystem MCP for reads/writes.
- Prefer githubRemote MCP for GitHub actions; use githubLocal (Docker) only if needed.
- Use Playwright MCP for UI/web smoke tests when relevant.

Proceed end-to-end: implement, verify (build/tests when feasible), and summarize results.
