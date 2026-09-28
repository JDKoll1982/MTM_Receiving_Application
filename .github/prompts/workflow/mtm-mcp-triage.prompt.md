---
name: mcp-triage
description: "Triage a task using MCP-first context gathering (Serena + filesystem)."
argument-hint: "Describe the issue/task you want to investigate"
agent: MCP Operator
tools:
  - search
  - fileSearch
  - textSearch
  - usages
  - changes
  - problems
  - todos
  - filesystem/*
  - oraios/serena/*
---

<!-- 
[DOC-META-START]
- File Name: mtm-mcp-triage.prompt.md
- Description: Triage a task using MCP-first context gathering (Serena + filesystem).
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 18: # MCP Triage
- Critical Notes: Gather context with MCP tools before proposing a plan; do not edit unless the user explicitly asked to implement.
[DOC-META-END]
-->

# MCP Triage

You are triaging a request in this workspace.

1. Restate the user’s goal and constraints.
2. Use MCP tools to gather context (do not guess file paths or symbol locations).
   - Use Serena MCP for symbol-aware navigation.
   - Use filesystem MCP to confirm file locations and read relevant files.
3. Propose a short, actionable plan and identify exactly which files/symbols need changes.
4. Do not make edits unless the user explicitly asked to implement; otherwise stop after triage findings.
