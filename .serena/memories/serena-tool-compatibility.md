<!-- 
[DOC-META-START]
- File Name: serena-tool-compatibility.md
- Description: Serena tool compatibility notes for this environment.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 12-16: # Serena Tool Compatibility
- Critical Notes: None
[DOC-META-END]
-->

# Serena Tool Compatibility

- Upgraded Serena no longer accepts legacy optional tool names like `think_about_collected_information`, `think_about_task_adherence`, `think_about_whether_you_are_done`, `summarize_changes`, or `prepare_for_new_conversation` in `.serena/project.yml`.
- Keep `included_optional_tools` limited to tool names returned by the current `serena tools list` CLI.
- Verified compatible project setting on 2026-04-11: `included_optional_tools: [execute_shell_command]`.
