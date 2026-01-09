# Docent Sidecar - Persistent Memory

This folder stores persistent memory for the **Docent** Expert agent - Module Documentation Specialist.

## Purpose

Docent maintains living documentation for WinUI 3 modules, tracking analyzed modules, discovered patterns, and validation history. The sidecar provides persistent storage for:

- **Module analysis history** - Which modules have been documented, when, and their current state
- **Pattern library** - Common architectural patterns discovered across modules
- **MTM conventions** - Project-specific MVVM patterns, naming conventions, and best practices
- **Validation tracking** - Last validation dates, detected drift, update history

## Files

### Core Memory Files

- **memories.md** - Module tracking registry, validation history, discovered patterns
- **instructions.md** - MTM architectural conventions, MVVM patterns, analysis protocols

### Workflows (Complex Multi-Step Processes)

- **workflows/full-module-analysis.md** - Complete 7-section documentation generation
- **workflows/quick-analysis.md** - Fast module overview workflow
- **workflows/update-view-documentation.md** - Incremental View/ViewModel refresh
- **workflows/database-schema-analysis.md** - Database layer deep-dive
- **workflows/validate-documentation.md** - Documentation drift detection and correction
- **workflows/generate-diagram.md** - Mermaid diagram generation

### Knowledge Base

- **knowledge/mvvm-patterns.md** - MVVM architecture patterns reference
- **knowledge/mermaid-diagram-templates.md** - Diagram generation templates
- **knowledge/mtm-architecture-reference.md** - MTM Receiving Application conventions

## Access Pattern

Docent accesses these files via critical_actions on activation:

```yaml
- Load COMPLETE file {project-root}/_bmad/_memory/docent-sidecar/memories.md
- Load COMPLETE file {project-root}/_bmad/_memory/docent-sidecar/instructions.md
```

Commands reference workflow files:

```yaml
exec: '{project-root}/_bmad/_memory/docent-sidecar/workflows/full-module-analysis.md'
```

## File Access Restrictions

Per critical_actions, Docent:

- ✅ **CAN** read/write files in `_bmad/_memory/docent-sidecar/`
- ❌ **CANNOT** access files outside this directory (safety constraint)

## Memory Structure Philosophy

- **memories.md** = Dynamic tracking data (changes frequently)
- **instructions.md** = Stable reference data (changes rarely)
- **workflows/** = Executable processes (complex multi-step operations)
- **knowledge/** = Static reference material (architectural patterns)

---

**Created:** 2026-01-08  
**Agent:** Docent (Expert Agent)  
**Version:** 1.0.0
