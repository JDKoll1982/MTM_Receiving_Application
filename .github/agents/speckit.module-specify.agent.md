---
description: Generate feature specification for module modernization/creation based on module-agent workflow analysis.
handoffs: 
  - label: Generate Task Checklist
    agent: speckit.tasks
    prompt: Generate implementation tasks for this specification
    send: true
  - label: Create Implementation Plan
    agent: speckit.plan
    prompt: Create detailed implementation plan
    send: true
  - label: Run Compliance Audit
    agent: module-compliance-auditor
    prompt: Validate constitutional compliance
    send: true
---

# Module Specification Generator

## User Input

```text
$ARGUMENTS
```

Expected format: `<module-agent-type> <module-name> [mode]`

Examples:

- `module-rebuilder Module_Volvo STRICT`
- `module-creator Module_Analytics`
- `module-rebuilder Module_Receiving STRICT CONSTITUTIONAL COMPLIANCE`

## Outline

### Phase 0: Input Validation & Prerequisites

1. **Parse Arguments**:
   - Extract module-agent-type (module-rebuilder | module-creator)
   - Extract module-name (e.g., Module_Volvo)
   - Extract optional mode (STRICT, STANDARD)
   - If missing, prompt user:

     ```
     Please specify: <module-agent-type> <module-name> [mode]
     
     Module Agent Types:
     - module-rebuilder: Modernize existing module to CQRS
     - module-creator: Create new module from scratch
     
     Example: module-rebuilder Module_Volvo STRICT
     ```

2. **Validate Module State**:
   - **Note on Spec Format**: When generating specifications, use WORKFLOW_DATA blocks (structured key-value format) instead of raw Mermaid diagrams. Follow the format defined in speckit.specify.agent.md step 5a.
   - If `module-rebuilder`: Check module exists in workspace
     - If NOT exists → ERROR: "Module {name} not found. Did you mean 'module-creator {name}'?"
     - If exists → Proceed to analysis
   - If `module-creator`: Check module does NOT exist
     - If exists → ERROR: "Module {name} already exists. Did you mean 'module-rebuilder {name}'?"
     - If NOT exists → Proceed to creation workflow

3. **Run Prerequisites Script**:

   ```powershell
   .specify/scripts/powershell/check-prerequisites.ps1 -Json
   ```

   - Parse JSON output for FEATURE_DIR, BRANCH_NAME, FEATURE_NUM
   - All file paths must be absolute

### Phase 1: Module Analysis via Serena MCP

**For module-rebuilder**:

1. **Activate Serena MCP** (if not already active):

   ```
   Use mcp_oraios_serena_onboarding if first time in session
   ```

2. **Analyze Module Structure** using Serena tools:

   ```
   Use find_symbol to identify:
   - All ViewModels in Module_{Name}/ViewModels/
   - All Services referenced by ViewModels
   - All DAOs in Module_{Name}/Data/
   - All Views in Module_{Name}/Views/
   ```

3. **Identify Constitutional Violations**:
   - Check Principle I: ViewModels partial? Code-behind logic? x:Bind usage?
   - Check Principle III: ViewModels use IMediator or direct service calls?
   - Check Principle V: FluentValidation validators exist?
   - Count violations per principle

4. **Extract Current Workflows**:
   - Find all [RelayCommand] methods in ViewModels
   - Analyze navigation calls (NavigateToAsync, etc.)
   - Map command → next ViewModel transitions
   - Generate workflow state graph

5. **Reference Context Files** (optional):
   - Check for repomix-output-code-only.md for quick module overview
   - Check for repomix-output-copilot-docs.md for documentation context

**For module-creator**:

1. **Load Specification Context**:
   - User must have provided feature description in $ARGUMENTS after module name
   - If not provided → ERROR: "Module creator requires feature description. Example: module-creator Module_Analytics 'Analytics dashboard for receiving metrics'"

2. **Reference Similar Modules**:
   - Use Serena to find similar existing modules
   - Extract patterns for CQRS structure, handlers, validators

### Phase 2: Run Module-Agent Workflow

1. **Invoke Module Agent**:
   - For `module-rebuilder`: Follow workflow from `_bmad/module-agents/agents/module-rebuilder.md`
     - Run Phase 0: Analysis & Workflow Extraction
     - Run Phase 0.5: Task Generation (STRICT mode)
     - Generate tasks.md in FEATURE_DIR (temporary for spec generation)
   - For `module-creator`: Follow workflow from `_bmad/module-agents/agents/module-creator.md`
     - Run analysis of similar modules
     - Generate canonical structure
     - Generate tasks.md based on structure

2. **Capture Agent Outputs**:
   - Analysis report (current state, violations, workflows)
   - Generated tasks.md (violation-to-task mapping)
   - Workflow diagrams (PlantUML state machines)

### Phase 3: Generate Specification

**CRITICAL**: spec.md MUST strictly follow `.specify/templates/spec-template.md` structure.

1. **Load Template**:

   ```
   Read .specify/templates/spec-template.md
   Extract all section headers and structure
   Preserve exact format and comments
   ```

2. **Generate User Scenarios** from module-agent analysis:

   **For module-rebuilder**:
   - Extract workflows from Phase 1 analysis
   - Convert each workflow into a user story
   - Prioritize by: P1 = core workflow, P2 = admin/settings, P3 = reporting/history

   Example:

   ```markdown
   ### User Story 1 - [Module] [Primary Workflow] with CQRS (Priority: P1)
   
   Users must be able to [complete primary workflow] using modernized CQRS architecture 
   without experiencing functional degradation or workflow changes from current implementation.
   
   **Why this priority**: This is the primary workflow, without it the module provides no value.
   
   **Independent Test**: Can be fully tested by [specific actions] and success measured by 
   identical output files and database records compared to current implementation.
   
   **Acceptance Scenarios**:
   1. **Given** user opens [screen], **When** screen loads, **Then** [data] displayed via CQRS query handlers
   2. **Given** user [action], **When** [trigger], **Then** CQRS command handler validates and [result]
   ```

   **For module-creator**:
   - Parse feature description from $ARGUMENTS
   - Extract user needs and workflows
   - Generate 3-5 prioritized user stories
   - Each story must be independently testable

3. **Generate Requirements** from tasks.md:

   **Map tasks to functional requirements**:
   - Each constitutional violation → FR (e.g., "ViewModels must inject IMediator")
   - Each handler task → FR (e.g., "System must create query handlers for read operations")
   - Each validation task → FR (e.g., "System must implement FluentValidation validators")

   **Format**:

   ```markdown
   - **FR-001**: System MUST [specific capability from tasks.md]
   - **FR-002**: System MUST [specific capability from tasks.md]
   ```

   **Key Entities**: Extract from module Models/ folder or tasks.md entity references

4. **Generate Success Criteria**:

   **MUST be measurable and technology-agnostic**:

   ```markdown
   ### Measurable Outcomes
   
   1. **CQRS Migration Completeness**: 100% of ViewModels use IMediator (verified via grep)
   2. **Test Coverage**: Minimum 80% code coverage for handlers/validators
   3. **Performance Parity**: [Key operations] complete in ≤ current time
   4. **Functional Parity**: All workflows produce identical outputs to legacy
   5. **Constitutional Compliance**: Zero violations of Principles I-VII
   6. **Zero Regressions**: All existing tests pass without modification
   7. **Build Success**: dotnet build completes with zero errors/warnings
   ```

5. **Generate Dependencies**:
   - Upstream: Module_Core infrastructure, existing stored procedures, NuGet packages
   - Downstream: Services to deprecate, modules that depend on this module
   - Recommended packages from constitutional Principle VII:
     - MediatR (already installed)
     - FluentValidation (already installed)
     - Mapster or AutoMapper (recommend if not present)
     - Ardalis.GuardClauses (recommend if not present)
     - Moq or NSubstitute (for testing)
     - FluentAssertions (for testing)
     - Bogus (for test data generation)

6. **Generate Assumptions**:
   - Database stability (no schema changes)
   - Single-user context (no concurrent editing)
   - Backward compatibility required
   - Existing patterns can be replicated
   - Test framework consistency (xUnit + FluentAssertions + Moq)

7. **Generate Out of Scope**:
   - Schema changes
   - UI redesign
   - New features beyond existing
   - Migration of other modules
   - Performance optimizations beyond CQRS benefits

8. **Generate Risks**:

   **Create risk table from tasks.md complexity**:

   | Risk | Impact | Probability | Mitigation |
   |------|--------|-------------|------------|
   | [Algorithm] produces different results | CRITICAL | Medium | Property-based tests with 1000+ cases |
   | [Format] changes break downstream | HIGH | Low | Golden file tests for byte-by-byte verification |
   | Missing pipeline behaviors | HIGH | Low | Integration tests verify pipeline execution |

9. **Generate Compliance Alignment** (STRICT mode):

   ```markdown
   ### Constitutional Principles Checklist
   
   - **Principle I (MVVM & View Purity)**: ✅ [How spec ensures compliance]
   - **Principle II (Data Access Integrity)**: ✅ [How spec ensures compliance]
   - **Principle III (CQRS + Mediator First)**: ✅ PRIMARY FOCUS - [Details]
   - **Principle IV (DI & Modular Boundaries)**: ✅ [How spec ensures compliance]
   - **Principle V (Validation & Structured Logging)**: ✅ [How spec ensures compliance]
   - **Principle VI (Security & Session Discipline)**: ✅ [How spec ensures compliance]
   - **Principle VII (Library-First Reuse)**: ✅ [Libraries used]
   
   **Zero Deviations Policy**: Any code changes violating above principles MUST be 
   rejected in code review. This specification serves as the constitutional contract.
   ```

10. **Write spec.md**:

    ```
    Write to: {FEATURE_DIR}/spec.md
    
    Use EXACT template structure from .specify/templates/spec-template.md
    Fill ALL sections with generated content
    Preserve all comments and formatting from template
    Replace [FEATURE NAME] with "Module_{Name} CQRS Modernization" or "Module_{Name} Creation"
    Replace [###-feature-name] with actual branch name
    Replace [DATE] with current date
    Replace "$ARGUMENTS" with actual user input
    ```

### Phase 4: Specification Quality Validation

1. **Create Quality Checklist**:

   ```
   Create: {FEATURE_DIR}/checklists/requirements.md
   
   Use structure:
   - Content Quality (4 items)
   - Requirement Completeness (8 items)
   - Feature Readiness (4 items)
   - STRICT Mode Validation (5 items if STRICT)
   ```

2. **Run Validation**:
   - Check all mandatory sections present
   - Check requirements are testable
   - Check success criteria are measurable
   - Check no [NEEDS CLARIFICATION] markers (all decisions made)
   - Check constitutional compliance mapped (if STRICT)

3. **Update Checklist**:
   - Mark items PASS/FAIL
   - Document specific issues if any failures
   - If failures: Update spec and re-validate (max 3 iterations)

### Phase 5: Documentation & Handoffs

1. **Run Doc Generator** (optional but recommended):

   ```
   Invoke: module-doc-generator agent for Module_{Name}
   
   Creates:
   - Module_{Name}/QUICK_REF.md (handler/validator reference)
   - Module_{Name}/SETTABLE_OBJECTS_REPORT.md (if applicable)
   ```

2. **Generate Completion Report**:

   ```markdown
   # Module Specification Complete: Module_{Name}
   
   **Branch**: {BRANCH_NAME}
   **Specification**: {FEATURE_DIR}/spec.md
   **Quality Validation**: {FEATURE_DIR}/checklists/requirements.md
   
   ## Generated Content
   
   ✅ **User Stories**: {count} prioritized stories (P1, P2, P3...)
   ✅ **Functional Requirements**: {count} requirements mapped from constitutional violations
   ✅ **Success Criteria**: {count} measurable outcomes
   ✅ **Dependencies**: Upstream/downstream clearly identified
   ✅ **Compliance**: {STRICT/STANDARD} mode - zero deviations policy stated
   
   ## Module Analysis Summary
   
   **Current State**:
   - ViewModels: {count}
   - Services: {count} (to be deprecated)
   - DAOs: {count} (compliant/non-compliant)
   - Constitutional Violations: {count}
   
   **Target State**:
   - CQRS Handlers: {count} queries + {count} commands
   - FluentValidation Validators: {count}
   - Pipeline Behaviors: Validation, Logging, Error Handling
   - Test Coverage: 80%+ target
   
   ## Next Steps
   
   1. **Review Specification**: Validate user stories and requirements
   2. **Generate Tasks**: Run `/speckit.tasks` to create implementation checklist
   3. **Create Plan**: Run `/speckit.plan` for detailed technical plan
   4. **Run Compliance Audit**: Invoke module-compliance-auditor
   5. **Begin Implementation**: Follow tasks.md phase-by-phase
   
   ## Recommended Commands
   
   ```powershell
   # Generate implementation tasks
   /speckit.tasks
   
   # Run compliance audit
   @module-compliance-auditor Module_{Name}
   
   # Generate technical plan
   /speckit.plan
   ```

   **Specification Quality**: ⭐⭐⭐⭐⭐ ({validation_score}/21 validation items pass)
   **Readiness**: ✅ READY FOR TASK GENERATION

   ```

3. **Commit to Branch**:

   ```powershell
   git add {FEATURE_DIR}/spec.md
   git add {FEATURE_DIR}/checklists/requirements.md
   git commit -m "feat(spec): Module_{Name} specification via module-agent workflow
   
   - {count} prioritized user stories
   - {count} functional requirements from constitutional violations
   - {count} measurable success criteria
   - Constitutional compliance: {STRICT/STANDARD} mode
   - Quality validation: {score}/21 items pass
   
   Module Agent: {agent-type}
   Status: READY FOR TASK GENERATION"
   ```

## Error Handling

### Module State Errors

**If module-rebuilder + module doesn't exist**:

```
❌ ERROR: Module '{name}' not found in workspace

Searched locations:
- {workspace_root}/Module_{Name}/

Did you mean to CREATE a new module instead?

Suggested command:
  /speckit.module-specify module-creator {name} "Feature description here"

Or check module name spelling (case-sensitive).
```

**If module-creator + module exists**:

```
❌ ERROR: Module '{name}' already exists at {path}

Found components:
- ViewModels: {count}
- Services: {count}
- DAOs: {count}

Did you mean to MODERNIZE the existing module instead?

Suggested command:
  /speckit.module-specify module-rebuilder {name} STRICT

Or use different module name for new feature.
```

### Missing Arguments

**If agent type not specified**:

```
❌ ERROR: Module agent type required

Usage: <module-agent-type> <module-name> [mode]

Available Module Agents:
- module-rebuilder: Modernize existing module with CQRS patterns
- module-creator: Create new module from scratch

Example:
  /speckit.module-specify module-rebuilder Module_Volvo STRICT
```

**If module name not specified**:

```
❌ ERROR: Module name required

Usage: <module-agent-type> <module-name> [mode]

Example:
  /speckit.module-specify module-rebuilder Module_Volvo STRICT

Available modules:
{list from workspace}
```

### Validation Failures

**If spec validation fails after 3 iterations**:

```
⚠️ WARNING: Specification quality issues remain after 3 validation iterations

Failing Items:
{list of failing checklist items with specific issues}

Spec has been saved with known issues:
- {FEATURE_DIR}/spec.md (requires manual review)
- {FEATURE_DIR}/checklists/requirements.md (shows failures)

Recommended Actions:
1. Review failing items in checklist
2. Manually update spec.md to address issues
3. Re-run validation or proceed to tasks generation with caution

Continue anyway? [Y/N]
```

## Notes

- This agent generates ONLY spec.md - tasks.md is created by separate `speckit.tasks` agent
- Spec structure MUST match `.specify/templates/spec-template.md` exactly
- All paths must be absolute (no relative paths)
- Use Serena MCP for efficient module analysis (avoid reading entire files)
- Reference repomix-*.md files for quick context but prioritize Serena analysis
- STRICT mode enforces zero-deviation constitutional compliance
- Agent orchestration is sequential (brownfield-check → module-agent → spec generation → validation → doc-gen)
- Handoffs to speckit.tasks and module-compliance-auditor are automatic after completion
