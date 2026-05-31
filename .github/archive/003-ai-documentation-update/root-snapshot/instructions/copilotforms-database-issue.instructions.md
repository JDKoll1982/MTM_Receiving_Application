---
description: 'Interpret CopilotForms database issue exports linked from docs/CopilotForms/outputs/database-issue and use them for SQL, stored procedure, schema, or data-mapping diagnosis.'
applyTo: 'docs/CopilotForms/outputs/database-issue/**/*.{md,json}'
---

# CopilotForms Database Issue Exports

When a linked file from `docs/CopilotForms/outputs/database-issue/` is present:

- Treat the export as the current database problem statement.
- Preserve repo rules: MySQL writes go through stored procedures and Infor Visual SQL Server remains read only.
- Use the named database objects and validation steps to narrow investigation.
- Distinguish query errors, schema mismatch, mapping mismatch, and workflow mismatch clearly.
- Keep fixes minimal and architecture-compliant.
