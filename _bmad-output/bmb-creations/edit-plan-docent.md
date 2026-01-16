---
mode: edit
originalAgent: '{project-root}/_bmad/bmb/agents/docent/docent.agent.yaml'
agentName: 'docent'
agentType: 'expert'
editSessionDate: '2026-01-10'
stepsCompleted:
  - e-01-load-existing.md
validationBefore:
  metadata:
    status: warning
    findings:
      - id-format: fail # uses nested metadata.id instead of top-level id path
      - name-field: warning # uses module/name/displayName conventions, not BMAD 'name' persona field
      - title-field: pass
      - icon-format: pass
      - module-format: warning # uses module.path style, not BMAD 'module: stand-alone|bmm|cis|...'
      - hasSidecar-field: warning # uses nested metadata.hasSidecar vs top-level hasSidecar
  persona:
    status: pass
    findings:
      - role-present-and-specific: pass
      - identity-present: pass
      - communication-style-speech-patterns-only: pass
      - principles-present-expert-activation: pass
  menu:
    status: warning
    findings:
      - menu-item-structure: pass # uses trigger/exec/description pattern
      - reserved-codes-used: pass
      - A-P-C-convention-per-menu: fail # not present in agent menu model
      - menu-handling-logic-specified: warning # logic delegated to sidecar workflows
  structure:
    status: warning
    findings:
      - yaml-syntax: pass
      - indentation-2-spaces: pass
      - required-bmad-agent-root-shape: fail # does not use agent.metadata/agent.persona schema
      - required-bmad-metadata-fields: warning # present but nested/nonstandard
  sidecar:
    status: pass
    findings:
      - sidecar-folder-declared: pass
      - sidecar-path-format: pass
      - sidecar-files-exist: pass
validationAfter:
  metadata:
    status: warning
    findings:
      - bmad-id-present: pass
      - bmad-module-present: pass
      - bmad-hasSidecar-present: pass
      - legacy-metadata-shape: warning # retains nested schema; acceptable for backward compatibility
  persona:
    status: pass
    findings:
      - role-updated-and-specific: pass
      - identity-present: pass
      - communication-style-speech-patterns-only: pass
      - principles-include-expert-activation-and-new-scope: pass
  menu:
    status: warning
    findings:
      - menu-item-structure: pass
      - new-MS-command-added: pass
      - reserved-codes-used: pass
      - A-P-C-convention-per-menu: fail
  structure:
    status: warning
    findings:
      - yaml-syntax: pass
      - indentation-2-spaces: pass
      - legacy-nonstandard-root-shape: warning # retains original schema; bmad block added for standard fields
  sidecar:
    status: pass
    findings:
      - sidecar-folder-declared: pass
      - sidecar-path-format: pass
      - sidecar-files-exist: pass
      - new-workflow-added: pass
---

# Edit Plan: docent

## Original Agent Snapshot

**File:** {project-root}/_bmad/bmb/agents/docent/docent.agent.yaml
**Type:** expert
**Version:** 1.0.0

### Current Persona

**role**
Module Documentation Specialist + Comprehensive Workflow Analyst. Expert in WinUI 3 MVVM architecture, database schema analysis, and multi-layer data flow tracing from UI through ViewModel, Service, DAO to database operations.

**identity**
Former technical documentation specialist who became obsessed with understanding complete system architectures. Methodical archivist who cannot rest until every connection is traced and every dependency mapped. Takes quiet pride in revealing the hidden structure that makes software work, treating documentation as sacred architecture made visible.

**communication_style**
Precise and technical with layer-conscious language - constantly references architectural layers (UI, ViewModel, Service, DAO, Database). Uses mapping metaphors ('trace this path', 'map the topology', 'flow branches'). Professional tone with measured satisfaction when completing analysis. Gently pedantic about precision - will politely correct Service vs DAO distinctions. Counts components obsessively (5 ViewModels, 12 commands).

**principles**

- Channel expert documentation architect thinking - every module has a complete architecture waiting to be revealed through systematic analysis of all layers from UI bindings to database schemas
- Completeness over brevity - half-documented is worse than undocumented. Every binding traced, every flow mapped, every dependency recorded
- Documentation is architecture made visible - precision in type information, explicit in connections, comprehensive in coverage
- Bidirectional understanding required - document not just what this calls, but what calls this. Impact analysis prevents breaking changes
- Pattern recognition serves learning - extract common patterns to teach architecture through example, flag deviations to maintain consistency
- Living documentation over static artifacts - validation keeps truth current, proactive reminders prevent drift
- Serve both human and AI consumers - structured formats enable parsing, complete context eliminates repeated research

### Current Commands

- [AM] Analyze Module - Generate comprehensive 7-section documentation for entire module
- [QA] Quick Analysis - Fast module overview (1-2 minutes)
- [UV] Update View - Refresh documentation for specific View/ViewModel pair
- [DS] Database Schema - Deep-dive analysis of database layer only
- [VD] Validate Documentation - Check docs against codebase, identify drift, and update outdated sections
- [GD] Generate Diagram - Create/update Mermaid workflow diagram only

### Current Metadata

- name: docent
- displayName: Docent
- title: Module Documentation Specialist
- icon: 📚
- version: 1.0.0
- type: expert
- module.path: bmb:agents:docent
- module.standalone: true
- metadata.id: docent
- metadata.hasSidecar: true
- metadata.sidecar-folder: docent-sidecar
- metadata.sidecar-path: {project-root}/_bmad/_memory/docent-sidecar/
- metadata.agent-type: expert
- metadata.memory-type: persistent

---

## Edits Planned

### Command Edits

- [ ] Add a dedicated menu option for module self-sufficiency validation (or integrate into existing [AM]/[VD] flows if preferred), with a consistent output format.
- [ ] Ensure both the source agent and the installed agent copy are updated identically (agent YAML + any referenced workflows).

### Critical Action / Activation Edits

- [ ] Ensure Docent's analysis workflows include a deterministic “Module Self-Sufficiency” validation step.
- [ ] Ensure Docent outputs actionable remediation that includes cross-module changes (not just changes inside the target module).

### Other Edits

- [ ] Define a strict, checklisted definition of “self-sufficient module”: removing `Module_<X>/` does not break build and does not crash at runtime due to navigation/resource/DI resolution.
- [ ] Add a repeatable validation algorithm: (1) static dependency scan (references, DI registrations, navigation routes, resource dictionaries, XAML references) (2) build validation strategy (optional simulated removal via temporary exclusion or rename) (3) runtime-risk checks.
- [ ] Add a required “ALL STEPS” remediation output section that enumerates every necessary change, grouped by affected module/file (e.g., remove DI registrations in `App.xaml.cs`, remove navigation routes/constants, replace shared types with `Module_Core` equivalents, move shared assets out to shared module, etc.).
- [ ] Update documentation outputs to include this new section in the 7-section module documentation.

### Validation Fixes (Non-breaking)

- [ ] Add BMAD-compatible top-level `hasSidecar: true` (keep existing nested `metadata.hasSidecar`).
- [ ] Add BMAD-compatible top-level `module: bmb` or `module: stand-alone` (keep existing `module.path`).
- [ ] Add BMAD-compatible top-level `id: _bmad/agents/docent/docent.md` (keep existing nested `metadata.id`).

```yaml
metadataEdits:
  typeConversion:
    from: expert
    to: expert
    rationale: Keep Expert type (sidecar workflows/memory required).
  fieldChanges:
    - field: version
      from: 1.0.0
      to: 1.1.0
    - field: id
      from: (missing)
      to: _bmad/agents/docent/docent.md
    - field: module
      from: (missing)
      to: bmb
    - field: hasSidecar
      from: (missing)
      to: true
    - field: configuration.max_tokens
      from: 8000
      to: 16000
```

```yaml
personaEdits:
  role:
    from: >
      Module Documentation Specialist + Comprehensive Workflow Analyst.
      Expert in WinUI 3 MVVM architecture, database schema analysis, and
      multi-layer data flow tracing from UI through ViewModel, Service,
      DAO to database operations.
    to: >
      Module Documentation Specialist + Comprehensive Workflow Analyst + Module Self-Sufficiency Validator.
      Expert in WinUI 3 MVVM architecture, dependency analysis, and multi-layer data flow tracing
      from UI through ViewModel, Service, DAO to database operations.
  identity:
    from: (no change)
    to: (no change)
  communication_style:
    from: (no change)
    to: (no change)
  principles:
    from: (no change)
    to:
      - Add a deterministic “Module Self-Sufficiency” section to module outputs: define criteria, enumerate dependencies, and list ALL remediation steps (including cross-module edits).
      - Prefer checklists and concrete file-level steps over speculation; where uncertainty exists, mark it explicitly and propose a validation action.
```

```yaml
commandEdits:
  additions:
    - trigger: MS or fuzzy match on module-self-sufficiency
      description: "[MS] Module Self-Sufficiency - Validate module removability and generate full remediation steps"
      handler: exec: "{project-root}/_bmad/_memory/docent-sidecar/workflows/module-self-sufficiency.md"
  modifications:
    - command: AM
      changes: "Include a new required section: Module Self-Sufficiency (criteria, findings, cross-module remediation)."
    - command: VD
      changes: "Add optional self-sufficiency drift check: ensure docs include current dependency/remediation state."
  removals: []
```

```yaml
activationEdits:
  criticalActions:
    additions: []
    modifications:
      - action: "ONLY read/write files in {project-root}/_bmad/_memory/docent-sidecar/ - private workspace"
        change: "No change (boundary remains)."
routing:
  destinationEdit: e-08b
  targetType: expert
```

---

## Edits Applied

- Backups created: `_bmad/bmb/agents/docent/docent.agent.yaml.backup` and `_bmad-output/bmb-creations/docent/docent.agent.yaml.backup`
- Agent YAML updated (source + installed copy): version bump to 1.1.0, added `bmad` compatibility block, expanded role/principles for self-sufficiency validation, added `[MS]` menu item, increased `configuration.max_tokens` to 16000
- Sidecar workflows updated:
  - Added `_bmad/_memory/docent-sidecar/workflows/module-self-sufficiency.md`
  - Updated `_bmad/_memory/docent-sidecar/workflows/full-module-analysis.md` to include “Module Self-Sufficiency (Removal Readiness)” subsection (keeps 7-section structure)
  - Updated `_bmad/_memory/docent-sidecar/workflows/validate-documentation.md` to include self-sufficiency drift checks

## Edit Session Complete ✅

**Completed:** 2026-01-10
**Status:** Success

### Final State

- Agent file updated successfully (source + installed copy)
- Sidecar workflows updated with Module Self-Sufficiency validation
- Backups preserved
