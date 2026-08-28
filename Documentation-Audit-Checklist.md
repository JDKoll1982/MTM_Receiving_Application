<!-- 
[DOC-META-START]
- File Name: Documentation-Audit-Checklist.md
- Description: Progress tracker for the 2026-08-28 documentation audit (headers, content refresh, consolidation).
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 14: Build Health Gate
  - Line 22: Scope
  - Line 28: Progress Legend
  - Line 34: Core Root Files
  - Line 43: Instructions (.github/instructions)
  - Line 126: Agents (.github/agents)
  - Line 145: Prompts (.github/prompts)
  - Line 200: Serena Memories (.serena/memories)
  - Line 232: Repo Memories (/memories/repo)
  - Line 248: Config Index + Archive Moves
- Critical Notes: Do not audit .github/archive (historical). Mark [x] only after a file's header is inserted and content verified.
[DOC-META-END]
-->

# Documentation Audit Checklist

## Build Health Gate

- [x] `dotnet build MTM_Receiving_Application.slnx` — 0 errors, 0 warnings
- [x] `dotnet test MTM_Receiving_Application.Tests/...Tests.csproj` — 553/553 passing
- [x] Removed stale `mtm_verify/verify.csproj` reference from `.slnx`

## Scope

Core agent surface only: `.github/agents`, `.github/instructions`, `.github/prompts`,
`.github/copilot-instructions.md`, `AGENTS.md`, `.github/README.md`, `.serena/memories`,
`/memories/repo`. `.github/archive/` excluded.

## Progress Legend

`[ ]` pending · `[x]` header inserted + content verified · `~` consolidated/removed

## Core Root Files

- [ ] `.github/copilot-instructions.md`
- [ ] `AGENTS.md`
- [ ] `.github/README.md`

## Instructions (.github/instructions)

### architecture/

- [ ] architecture/cqrs-behaviors-pipeline.instructions.md
- [ ] architecture/cqrs.instructions.md
- [ ] architecture/dao-pattern.instructions.md
- [ ] architecture/dotnet-architecture-good-practices.instructions.md
- [ ] architecture/error-handling.instructions.md
- [ ] architecture/labelview-lbl-parsing.instructions.md
- [ ] architecture/labelview-mainwindow-user-label-buttons.instructions.md
- [ ] architecture/module-coupling-constraints.instructions.md
- [ ] architecture/mvvm-pattern.instructions.md
- [ ] architecture/settings-management-system.instructions.md
- [ ] architecture/winui3-dialog-window-patterns.instructions.md
- [ ] architecture/winui3-header-workflow.instructions.md
- [ ] architecture/winui3-xaml-converters.instructions.md

### copilotforms/

- [ ] copilotforms/copilotforms-code-review.instructions.md
- [ ] copilotforms/copilotforms-configuration-environment-issue.instructions.md
- [ ] copilotforms/copilotforms-database-issue.instructions.md
- [ ] copilotforms/copilotforms-debugging-logging.instructions.md
- [ ] copilotforms/copilotforms-debugging.instructions.md
- [ ] copilotforms/copilotforms-documentation-change.instructions.md
- [ ] copilotforms/copilotforms-feature-removal-request.instructions.md
- [ ] copilotforms/copilotforms-improvement-refactor.instructions.md
- [ ] copilotforms/copilotforms-logic-change-test-generation.instructions.md
- [ ] copilotforms/copilotforms-logic-correction.instructions.md
- [ ] copilotforms/copilotforms-logging-refactor.instructions.md
- [ ] copilotforms/copilotforms-naming-consistency-cleanup.instructions.md
- [ ] copilotforms/copilotforms-new-feature-request.instructions.md
- [ ] copilotforms/copilotforms-performance-issue-optimization.instructions.md
- [ ] copilotforms/copilotforms-test-generation.instructions.md
- [ ] copilotforms/copilotforms-ui-change-logic-change.instructions.md
- [ ] copilotforms/copilotforms-ui-change.instructions.md
- [ ] copilotforms/copilotforms-ui-mockup.instructions.md

### database/

- [ ] database/infor-visual-database-reference.instructions.md
- [ ] database/infor-visual-query-authoring.instructions.md
- [ ] database/sql-schema-generation.instructions.md
- [ ] database/sql-sp-generation.instructions.md

### documentation/

- [ ] documentation/markdown-style.instructions.md
- [ ] documentation/mermaid-diagrams.instructions.md
- [ ] documentation/module-doc-maintenance.instructions.md
- [ ] documentation/spec-slice-format.instructions.md
- [ ] documentation/update-docs-on-code-change.instructions.md

### languages/

- [ ] languages/csharp.instructions.md
- [ ] languages/powershell-pester-5.instructions.md
- [ ] languages/powershell-scripting-ai.instructions.md
- [ ] languages/powershell.instructions.md
- [ ] languages/python.instructions.md
- [ ] languages/shell.instructions.md

### quality/

- [ ] quality/code-review-generic.instructions.md
- [ ] quality/object-calisthenics.instructions.md
- [ ] quality/performance-optimization.instructions.md
- [ ] quality/security-and-owask.instructions.md
- [ ] quality/self-explanatory-code-commenting.instructions.md

### testing/

- [ ] testing/testing-strategy.instructions.md

### tooling/

- [ ] tooling/context7-mcp-token-friendly.instructions.md
- [ ] tooling/mcp-tooling.instructions.md
- [ ] tooling/microsoft-learn-mcp-token-friendly.instructions.md
- [ ] tooling/skill-authoring.instructions.md
- [ ] tooling/winapp-mcp.instructions.md

### tooling/serena/

- [ ] tooling/serena/serena-01-overview.instructions.md
- [ ] tooling/serena/serena-02-tools-reference.instructions.md
- [ ] tooling/serena/serena-03-language-support.instructions.md
- [ ] tooling/serena/serena-04-running.instructions.md
- [ ] tooling/serena/serena-05-clients.instructions.md
- [ ] tooling/serena/serena-06-workflow.instructions.md
- [ ] tooling/serena/serena-07-memories.instructions.md
- [ ] tooling/serena/serena-08-configuration.instructions.md
- [ ] tooling/serena/serena-09-dashboard-logs-security.instructions.md
- [ ] tooling/serena/serena-10-advanced-usage.instructions.md
- [ ] tooling/serena/serena-token-friendly.instructions.md
- [ ] tooling/serena/serena-tools.instructions.md

### workflow/

- [ ] workflow/agents.instructions.md
- [ ] workflow/comprehensive-research.instructions.md
- [ ] workflow/dotnet-upgrade.instructions.md
- [ ] workflow/instructions.instructions.md
- [ ] workflow/prompt-engineer-every-message.instructions.md
- [ ] workflow/prompt.instructions.md
- [ ] workflow/taming-copilot.instructions.md

- [ ] instructions/README.md

## Agents (.github/agents)

- [ ] agents/mtm-module-improvement-auditor.agent.md
- [ ] agents/prompt-builder.agent.md
- [ ] agents/speckit.analyze.agent.md
- [ ] agents/speckit.checklist.agent.md
- [ ] agents/speckit.clarify.agent.md
- [ ] agents/speckit.constitution.agent.md
- [ ] agents/speckit.implement.agent.md
- [ ] agents/speckit.module-specify.agent.md
- [ ] agents/speckit.plan.agent.md
- [ ] agents/speckit.specify.agent.md
- [ ] agents/speckit.tasks.agent.md
- [ ] agents/winui3-expert.agent.md
- [ ] agents/README.md

## Prompts (.github/prompts)

### copilotforms/

- [ ] prompts/copilotforms/copilotforms-code-review.prompt.md
- [ ] prompts/copilotforms/copilotforms-configuration-environment-issue.prompt.md
- [ ] prompts/copilotforms/copilotforms-database-issue.prompt.md
- [ ] prompts/copilotforms/copilotforms-debugging-logging.prompt.md
- [ ] prompts/copilotforms/copilotforms-debugging.prompt.md
- [ ] prompts/copilotforms/copilotforms-documentation-change.prompt.md
- [ ] prompts/copilotforms/copilotforms-feature-removal-request.prompt.md
- [ ] prompts/copilotforms/copilotforms-improvement-refactor.prompt.md
- [ ] prompts/copilotforms/copilotforms-logic-change-test-generation.prompt.md
- [ ] prompts/copilotforms/copilotforms-logic-correction.prompt.md
- [ ] prompts/copilotforms/copilotforms-logging-refactor.prompt.md
- [ ] prompts/copilotforms/copilotforms-naming-consistency-cleanup.prompt.md
- [ ] prompts/copilotforms/copilotforms-new-feature-request.prompt.md
- [ ] prompts/copilotforms/copilotforms-performance-issue-optimization.prompt.md
- [ ] prompts/copilotforms/copilotforms-test-generation.prompt.md
- [ ] prompts/copilotforms/copilotforms-ui-change-logic-change.prompt.md
- [ ] prompts/copilotforms/copilotforms-ui-change.prompt.md
- [ ] prompts/copilotforms/copilotforms-ui-mockup.prompt.md

### design/

- [ ] prompts/design/mtm-mermaid-architecture-diagram-continue.prompt.md
- [ ] prompts/design/mtm-mermaid-architecture-diagram.prompt.md
- [ ] prompts/design/mtm-ui-mockups-comfyui.prompt.md
- [ ] prompts/design/mtm-ui-mockups.prompt.md

### maintenance/

- [ ] prompts/maintenance/commit-message.prompt.md
- [ ] prompts/maintenance/mtm-documentation.prepare.prompt.md
- [ ] prompts/maintenance/mtm-github-reset-documents.prompt.md
- [ ] prompts/maintenance/mtm-update-application-version.prompt.md

### quality/

- [ ] prompts/quality/mtm-module-code-review.prompt.md

### speckit/

- [ ] prompts/speckit/speckit.analyze.prompt.md
- [ ] prompts/speckit/speckit.checklist.prompt.md
- [ ] prompts/speckit/speckit.clarify.prompt.md
- [ ] prompts/speckit/speckit.constitution.prompt.md
- [ ] prompts/speckit/speckit.implement.prompt.md
- [ ] prompts/speckit/speckit.module-specify.prompt.md
- [ ] prompts/speckit/speckit.plan.prompt.md
- [ ] prompts/speckit/speckit.specify.prompt.md
- [ ] prompts/speckit/speckit.tasks.prompt.md

### testing/

- [ ] prompts/testing/mtm-converter-unit-tests.prompt.md
- [ ] prompts/testing/mtm-dao-unit-tests.prompt.md
- [ ] prompts/testing/mtm-generate-all-tests.prompt.md
- [ ] prompts/testing/mtm-helper-unit-tests.prompt.md
- [ ] prompts/testing/mtm-model-unit-tests.prompt.md
- [ ] prompts/testing/mtm-module-models-optimize-and-test.prompt.md
- [ ] prompts/testing/mtm-viewmodel-unit-tests.prompt.md
- [ ] prompts/testing/outside-service-setup-vm-tests.prompt.md

### workflow/

- [ ] prompts/workflow/mtm-mcp-implement.prompt.md
- [ ] prompts/workflow/mtm-mcp-triage.prompt.md
- [ ] prompts/workflow/noob-mode.prompt.md

### root/

- [ ] prompts/mtm-update-application-version.prompt.md
- [ ] prompts/plan-mainWindowButtonRevamp.prompt.md
- [ ] prompts/README.md

## Serena Memories (.serena/memories)

- [ ] agent_memory_convention.md
- [ ] architectural_patterns.md
- [ ] coding_standards.md
- [ ] constitution_summary.md
- [ ] copilotforms-index-notes.md
- [ ] customer-pull-pack-crystal-bindings.md
- [ ] dao_best_practices.md
- [ ] database-deployment-migration-mode.md
- [ ] default-terminal-shell.md
- [ ] dialog_patterns.md
- [ ] dunnage-seed-data-signatures.md
- [ ] error_handling_guide.md
- [ ] forbidden_practices.md
- [ ] help_system_architecture.md
- [ ] infor-visual-csv-source-of-truth.md
- [ ] infor_visual_constraints.md
- [ ] memory_maintenance.md
- [ ] mvvm_guide.md
- [ ] project_overview.md
- [ ] serena-tool-compatibility.md
- [ ] stored-procedure-test-build-workaround.md
- [ ] suggested_commands.md
- [ ] task_completion_workflow.md
- [ ] tech_stack.md
- [ ] test-coverage-workflow.md
- [ ] winui-input-json-build-lock.md
- [ ] xaml_binding_patterns.md

## Repo Memories (/memories/repo)

- [ ] copilotforms-index-notes.md
- [ ] customer-pull-pack-crystal-bindings.md
- [ ] database-deployment-migration-mode.md
- [ ] default-terminal-shell.md
- [ ] dunnage-seed-data-signatures.md
- [ ] infor-visual-csv-source-of-truth.md
- [ ] serena-tool-compatibility.md
- [ ] stored-procedure-test-build-workaround.md
- [ ] test-coverage-workflow.md
- [ ] winui-input-json-build-lock.md

## Config Index + Archive Moves

- [ ] Create `instructions/tooling/non-markdown-config-files.instructions.md` (JSON/XAML/YAML can't hold headers)
- [ ] Reference the new config file from `tooling/mcp-tooling.instructions.md` and `tooling/serena/serena-08-configuration.instructions.md`
- [ ] Move obsolete root reports to `.github/archive/` (ScannerUpdate.md, SCANNER_HANDOFF.md, Part-Padding-Feature-Report.md, ReprintLabelsPlan.md, NextVersionPatchNotes.md, ItemsToSendUpdate.md, DunnageLabelData_LabelView_Queries.md, DunnageUDC_LabelView_Queries.md, ScannerUpdate.html)

## Completion Log (2026-08-28)

Done:
- Build gate: 0 errors, 0 warnings, 553/553 tests green; stale `.slnx` temp reference removed.
- Headers added: core root files (copilot-instructions, AGENTS, .github/README), 3 READMEs, all 27 Serena memories, 5 tooling instruction files, this checklist.
- Content fixes: build/test commands corrected to `.slnx` + test csproj in copilot-instructions, AGENTS, suggested_commands, task_completion_workflow.
- Created: `tooling/non-markdown-config-files.instructions.md` (+ references in mcp-tooling and serena-08); repo memory `winrt-static-init-unit-tests.md`.
- Moved 9 obsolete root reports to `.github/archive/obsolete-root-reports/`.

Remaining backlog (headers only; content is current):
- ~71 `.github/instructions/**` files (architecture, copilotforms, database, documentation, languages, quality, testing, workflow, remaining serena-0x, skill-authoring, winapp-mcp).
- 12 `.github/agents/*.agent.md` files.
- 49 `.github/prompts/**/*.prompt.md` files.

