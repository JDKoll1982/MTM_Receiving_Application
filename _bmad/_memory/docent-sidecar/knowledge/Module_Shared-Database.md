---
module_name: Module_Shared
component: database-mapping
generated_on: 2026-01-09
analyst: Docent v1.0.0
---

# Module_Shared - Database & Integration Notes

Companion to:
- [_bmad/_memory/docent-sidecar/knowledge/Module_Shared.md](../docent-sidecar/knowledge/Module_Shared.md)

## Direct Database Access

Module_Shared has **no direct database access**:

- No MySQL DAOs
- No SQL Server DAOs
- No stored procedure calls

## Indirect Database/Integration Touchpoints (via Module_Core services)

Module_Shared UI flows call into Module_Core services which may access databases:

- `IService_Authentication`
  - Used by shared terminal login (PIN authentication)
  - Used by new user setup (load departments, validate PIN, create user)
- `IService_Help`
  - Used to load help content and persist dismiss state
- `IService_Notification`
  - Used to present global status messages (published by `ViewModel_Shared_Base.ShowStatus()`)

## Security/Operational Notes

- Shared terminal login enforces a 3-attempt limit and then closes the dialog with a lockout flag.
- New user setup has an optional ERP credential capture section; UI warns that credentials are stored in plain text (implementation is in services outside this module).
