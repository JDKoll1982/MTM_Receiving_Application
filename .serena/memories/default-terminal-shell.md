<!-- 
[DOC-META-START]
- File Name: default-terminal-shell.md
- Description: Default integrated terminal shell and shell-compatibility guidance.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-16: # Default Terminal Shell
- Critical Notes: Use sh-compatible commands; invoke pwsh explicitly for PowerShell syntax.
[DOC-META-END]
-->

# Default Terminal Shell

- Workspace default Windows integrated terminal is Git Bash (sh).
- Use shell-compatible commands by default in instructions and agent guidance.
- Invoke PowerShell explicitly with pwsh -Command or pwsh -File when a task needs PowerShell syntax or .ps1 execution.
