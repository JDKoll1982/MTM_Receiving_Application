# BMAD Remediation Guide

Use this guide to drive BMAD-led fixes for architectural violations. The focus is on small, taskable files and repeatable workflows that leverage Docent/Serena analysis plus the repomix snapshot.

## How to Work

- Run a quick scan (Docent `QA`) to confirm component counts, then a full scan (Docent `AM`) when you begin work on a module.
- Cross-check findings with repomix for fast code references: [repomix-output-code-only.xml](repomix-output-code-only.xml).
- Use the playbooks in this folder to apply BMAD agents (Docent, dev-developer, dev-quick-flow, plan-architect) to each violation type.
- Prefer many small fixes merged frequently; keep Module_Core free of module-specific logic.

## Priority Violations to Tackle First

1. Module-specific services parked in Module_Core (Dunnage, Receiving, Volvo) instead of in their own modules.
2. ViewModels wired to multi-method services instead of CQRS/MediatR handlers.
3. DAOs/DB calls lacking explicit stored-procedure usage or Model_Dao_Result returns.
4. Any binding or code-behind logic that violates MVVM/x:Bind purity.

## File Map

- `Violations/violation-catalog.md` — current violation checklist and where to look.
- `Playbooks/cqrs-migration.md` — migrate service-heavy flows to MediatR handlers.
- `Playbooks/module-core-decoupling.md` — move module logic out of Module_Core.
- `Playbooks/dao-and-db-checks.md` — enforce stored-procedure and read-only rules.
- `Playbooks/viewmodel-and-xaml-audit.md` — enforce MVVM and x:Bind rules.
