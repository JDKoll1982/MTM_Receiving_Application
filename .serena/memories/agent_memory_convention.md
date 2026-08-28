<!-- 
[DOC-META-START]
- File Name: agent_memory_convention.md
- Description: Convention for what belongs in agent memory files vs instruction files.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-17: # Agent Memory Convention
- Critical Notes: Keep memories short; put durable rules in instruction files.
[DOC-META-END]
-->

# Agent Memory Convention

- Durable, repo-scoped project knowledge lives in this folder (`.serena/memories/`) so it is **git-committed and repo-bound** — it travels with the repo to any machine/clone and is shareable with the team.
- The Copilot `memory` tool stores `/memories/` (user, session, repo) under VS Code's machine-local `workspaceStorage` (`%APPDATA%\Code\User\workspaceStorage\<workspace-hash>\GitHub.copilot-chat\memory-tool\`) — that data is **NOT in the repo** and does **NOT** travel to other computers.
- Rule: write durable project knowledge to a committed file in `.serena/memories/` (or another tracked location) instead of relying only on the local memory tool. Keep notes short, topical, and dated.
- Files in this folder named like the memory-tool notes (e.g. `dunnage-seed-data-signatures.md`) are mirrors of `/memories/repo/` content, persisted here so they are shared.
