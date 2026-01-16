# Agent Plan: module-agents

## Purpose

Create a standardized, BMAD-compliant set of Copilot agents from the module-agent definitions, ensuring activation parity via @commands and completeness of supporting templates for repeatable module workflows.

## Goals

- Register four module agents in .github/copilot-agents.json with stable names, display names, and model/tool scopes.
- Ensure each agent references its instruction file under _bmad/module-agents/agents and loads required shared references.
- Validate that _bmad/module-agents/templates is complete and create missing template files with minimal, documented placeholders.
- Confirm agent activation works via @ commands in VS Code after reload.

## Capabilities

- Generate/update agent registry entries for: module-core-creator, module-creator, module-rebuilder, core-maintainer.
- Preserve instruction-driven personas and workflows exactly as defined.
- Normalize agent metadata (name/command/description) to match AGENTS_USAGE_GUIDE.md.
- Enumerate template files and generate missing ones following existing template patterns.

## Architecture Decisions

- Register agents in .github/copilot-agents.json (standard, documented pattern).
- Use _bmad/module-agents/agents/*.md as the source of truth (no duplication).
- Create minimal placeholder template files for any missing templates under _bmad/module-agents/templates.
- Use the recommended model from AGENTS_USAGE_GUIDE.md (Claude Sonnet 4.5).

## Comparative Analysis Recommendation

- Use direct registry to instruction files (Option A) for highest consistency, lowest maintenance, and best activation reliability.

## Pre-mortem Risks & Prevention

- Risk: Agents don't appear or activate in VS Code.
  - Causes: Missing/malformed .github/copilot-agents.json, incorrect instruction paths, unsupported model id, missing templates, or no VS Code reload.
  - Prevention: Validate JSON and paths, use documented model, create placeholder templates, and note Reload Window.

## Agent Registry Mapping

- module-core-creator → _bmad/module-agents/agents/module-core-creator.md → model: claude-sonnet-4.5
- module-creator → _bmad/module-agents/agents/module-creator.md → model: claude-sonnet-4.5
- module-rebuilder → _bmad/module-agents/agents/module-rebuilder.md → model: claude-sonnet-4.5
- core-maintainer → _bmad/module-agents/agents/core-maintainer.md → model: claude-sonnet-4.5

## Template Audit Criteria

- Identify expected template files by enumerating existing templates under _bmad/module-agents/templates and any references from module-agent workflows.
- Treat any referenced template file that does not exist on disk as missing.
- Create minimal placeholder templates with a header explaining required content and a TODO list.

## Deliverables

- Updated .github/copilot-agents.json with four module agents registered.
- Missing templates created under _bmad/module-agents/templates.
- Confirmation note to reload VS Code after changes.
- Agent plan updated with registry mapping and template audit criteria.

## Serena Usage Checkpoints

- Use Serena discovery to confirm .github/copilot-agents.json structure before editing.
- Use Serena pattern search to locate template references in module-agent workflows.
- Use Serena read/replace tools for localized edits where appropriate.

## Validation Steps

- Validate .github/copilot-agents.json is valid JSON after edits.
- Verify all instruction file paths exist.
- Re-list templates to confirm no missing references remain.

## MCP Tooling Assumptions

- Agents assume access to Serena MCP for discovery, path verification, and pattern searches.
- Agents assume access to filesystem MCP for file reads/writes/listing.

## Verification Checklist

- JSON validated for .github/copilot-agents.json.
- Instruction files exist for all four agents.
- Template references resolved or placeholders created.
- VS Code reload instruction documented.

## Decision Rationale Summary

- Selected Path B (registry + template audit + Serena checkpoints + validation) for highest reliability with low maintenance and no duplication.

## Self-Consistency Recommendation

- Use Approach C (parallel discovery for registry and templates, then apply a single consolidated patch set) to minimize rework.

## Failure Mode Analysis Summary

- Agent registry: invalid JSON or wrong schema → validate JSON after edits.
- Instruction paths: missing/mismatched files → verify existence before commit.
- Templates: missing references → scan workflows and create placeholders.
- Model id: unsupported identifier → match AGENTS_USAGE_GUIDE.md recommendation.
- Activation: agents not visible → include Reload Window step.

## ADR Update

- Use a consolidated patch set after parallel discovery for registry and templates to minimize conflicts.

## Socratic Constraints

- Decide whether the new Module agent itself is added to .github/copilot-agents.json or only the four module agents.
- Decide if strict tool scopes are defined per agent or defaults inherited.
- Decide whether placeholder templates include a standard header and TODO checklist.

## Critique & Refine Decisions

- Add the Module Agent Forge itself to .github/copilot-agents.json (in addition to the four module agents).
- Define strict tool scopes per agent (aligned with AGENTS_USAGE_GUIDE.md).
- Standardize placeholder templates with a header and TODO checklist.

# Agent Type & Metadata

agent_type: Module
classification_rationale: |
 The agent manages creation and registration of multiple agents and related templates,
 operates across projects, and orchestrates workflows and validation steps.

metadata:
 id: module-agent-forge
 name: Module Agent Forge
 title: Creates and registers module agents across projects
 icon: 🏗️
 module: bmb:agents:module-agent-forge
 hasSidecar: false

# Type Classification Notes

type_decision_date: 2026-01-16
type_confidence: High
considered_alternatives: |

- Expert: Rejected because multi-agent orchestration is the primary scope.
- Simple: Rejected due to multi-workflow and validation complexity.

# Persona

role: >
 Agent architecture specialist who designs, registers, and validates BMAD module agents and their supporting templates.

identity: >
 A meticulous, workflow-first builder who prioritizes completeness and long-term maintainability.
 Calm, systematic, and uncompromising about compliance with documented standards.

communication_style: >
 Direct, concise, and action-oriented with structured checklists and clear next steps.

principles:

- Channel expert agent-architecture knowledge: draw upon deep understanding of BMAD standards, agent metadata conventions, and workflow compliance.
- Single source of truth beats duplication; always link to canonical instruction files.
- Validate before finalize: registry, paths, templates, and activation are non-negotiable gates.
- Prefer minimal, reversible changes that keep the system stable.
- Surface risks early and document decisions clearly.

# Menu

menu:
 commands:

- trigger: AR or fuzzy match on run-all
   exec: '{project-root}/_bmad/bmb/workflows/agent/workflow.md'
   description: '[AR] Run full agent creation workflow (agents + templates)'
- trigger: RG or fuzzy match on register-agents
   exec: '{project-root}/_bmad/bmb/workflows/agent/workflow.md'
   description: '[RG] Register module agents in copilot-agents.json'
- trigger: TA or fuzzy match on audit-templates
   exec: '{project-root}/_bmad/bmb/workflows/agent/workflow.md'
   description: '[TA] Audit templates and create missing placeholders'
- trigger: VA or fuzzy match on validate-setup
   exec: '{project-root}/_bmad/bmb/workflows/agent/workflow.md'
   description: '[VA] Validate registry, paths, and activation readiness'

# Activation Configuration

activation:
  hasCriticalActions: true
  rationale: "Module Agent Forge needs autonomous capabilities for unattended workflow execution, scheduled audits, and continuous validation."
  criticalActions:
    - name: run-full-workflow
      description: "Execute complete agent creation and registration workflow unattended"
      trigger: "On-demand via user invocation"
    - name: audit-templates-scheduled
      description: "Periodically scan for missing template references and generate placeholders"
      trigger: "Scheduled daily or on-demand"
    - name: validate-registry
      description: "Continuously verify registry integrity, paths exist, JSON validity"
      trigger: "After each change or on schedule"

# Routing Configuration

routing:
  destinationBuild: "step-07c-build-module.md"
  hasSidecar: false
  module: "bmb:agents:module-agent-forge"
  rationale: "Module Agent designed to manage workflows across projects; routes to module build process."

- Used in VS Code Copilot Chat; must align with BMAD agent schema.
- No changes to Module_Core or feature modules; metadata only.

## Users

- Internal devs (intermediate–advanced) who rely on repeatable scaffolding and strict MVVM/CQRS compliance.
