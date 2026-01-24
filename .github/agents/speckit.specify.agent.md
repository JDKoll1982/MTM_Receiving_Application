---
description: Create or update the feature specification from a natural language feature description.
handoffs: 
  - label: Build Technical Plan
    agent: speckit.plan
    prompt: Create a plan for the spec. I am building with...
  - label: Clarify Spec Requirements
    agent: speckit.clarify
    prompt: Clarify specification requirements
    send: true
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

The text the user typed after `/speckit.specify` in the triggering message **is** the feature description. Assume you always have it available in this conversation even if `$ARGUMENTS` appears literally below. Do not ask the user to repeat it unless they provided an empty command.

Given that feature description, do this:

1. **Generate a concise short name** (2-4 words) for the branch:
   - Analyze the feature description and extract the most meaningful keywords
   - Create a 2-4 word short name that captures the essence of the feature
   - Use action-noun format when possible (e.g., "add-user-auth", "fix-payment-bug")
   - Preserve technical terms and acronyms (OAuth2, API, JWT, etc.)
   - Keep it concise but descriptive enough to understand the feature at a glance
   - Examples:
     - "I want to add user authentication" → "user-auth"
     - "Implement OAuth2 integration for the API" → "oauth2-api-integration"
     - "Create a dashboard for analytics" → "analytics-dashboard"
     - "Fix payment processing timeout bug" → "fix-payment-timeout"

2. **Check for existing branches before creating new one**:

   a. First, fetch all remote branches to ensure we have the latest information:

      ```bash
      git fetch --all --prune
      ```

   b. Find the highest feature number across all sources for the short-name:
      - Remote branches: `git ls-remote --heads origin | grep -E 'refs/heads/[0-9]+-<short-name>$'`
      - Local branches: `git branch | grep -E '^[* ]*[0-9]+-<short-name>$'`
      - Specs directories: Check for directories matching `specs/[0-9]+-<short-name>`

   c. Determine the next available number:
      - Extract all numbers from all three sources
      - Find the highest number N
      - Use N+1 for the new branch number

   d. Run the script `.specify/scripts/powershell/create-new-feature.ps1 -Json "$ARGUMENTS"` with the calculated number and short-name:
      - Pass `--number N+1` and `--short-name "your-short-name"` along with the feature description
      - Bash example: `.specify/scripts/powershell/create-new-feature.ps1 -Json "$ARGUMENTS" --json --number 5 --short-name "user-auth" "Add user authentication"`
      - PowerShell example: `.specify/scripts/powershell/create-new-feature.ps1 -Json "$ARGUMENTS" -Json -Number 5 -ShortName "user-auth" "Add user authentication"`

   **IMPORTANT**:
   - Check all three sources (remote branches, local branches, specs directories) to find the highest number
   - Only match branches/directories with the exact short-name pattern
   - If no existing branches/directories found with this short-name, start with number 1
   - You must only ever run this script once per feature
   - The JSON is provided in the terminal as output - always refer to it to get the actual content you're looking for
   - The JSON output will contain BRANCH_NAME and SPEC_FILE paths
   - For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot")

3. Load `.specify/templates/spec-template.md` to understand required sections.

4. Follow this execution flow:

    1. Parse user description from Input
       If empty: ERROR "No feature description provided"
    2. Extract key concepts from description
       Identify: actors, actions, data, constraints
    3. For unclear aspects:
       - Make informed guesses based on context and industry standards
       - Only mark with [NEEDS CLARIFICATION: specific question] if:
         - The choice significantly impacts feature scope or user experience
         - Multiple reasonable interpretations exist with different implications
         - No reasonable default exists
       - **LIMIT: Maximum 3 [NEEDS CLARIFICATION] markers total**
       - Prioritize clarifications by impact: scope > security/privacy > user experience > technical details
    4. Fill User Scenarios & Testing section
       If no clear user flow: ERROR "Cannot determine user scenarios"
    5. Generate Functional Requirements
       Each requirement must be testable
       Use reasonable defaults for unspecified details (document assumptions in Assumptions section)
    6. Define Success Criteria
       Create measurable, technology-agnostic outcomes
       Include both quantitative metrics (time, performance, volume) and qualitative measures (user satisfaction, task completion)
       Each criterion must be verifiable without implementation details
    7. Identify Key Entities (if data involved)
    8. Return: SUCCESS (spec ready for planning)

5. Write the specification to SPEC_FILE using the template structure, replacing placeholders with concrete details derived from the feature description (arguments) while preserving section order and headings.

5a. **Generate Workflow Data Blocks**: For EACH user story that involves user workflows or interactions:
   
   **CRITICAL**: Do NOT generate raw Mermaid diagrams. Instead, generate structured WORKFLOW_DATA blocks that will be parsed by MermaidGenerator.ps1.
   
   **Format**: Immediately after the user story's acceptance scenarios, add:
   
   ```markdown
   ## User Story [N] Workflow Data
   
   <!-- WORKFLOW_START: {UserStory}.{Workflow} --><!--
   WORKFLOW: {UserStory}.{Workflow}
   TITLE: {Clear workflow description}
   DIRECTION: {TD|LR|RL|BT}
   DEPENDS_ON: {Comma-separated workflow IDs or NONE}
   CONFLICTS_WITH: {Comma-separated workflow IDs or NONE}
   INTERACTION: {How this relates to other workflows}
   
   NODE: {NodeName}
   TYPE: {start|process|decision|end}
   SHAPE: {stadium|rect|diamond|circle}
   LABEL: {Display text - use <br/> for line breaks}
   
   NODE: {NodeName}
   TYPE: {type}
   SHAPE: {shape}
   LABEL: {label}
   
   CONNECTION: {FromNode} -> {ToNode}
   CONNECTION: {FromNode} -> {ToNode} [{Label}]
   --><!-- WORKFLOW_END: {UserStory}.{Workflow} -->
   ```
   
   **IMPORTANT**: The workflow data is wrapped in `<!--` and `-->` to hide it from markdown preview while keeping it parseable by MermaidGenerator.ps1.
   ```
   
   **Field Requirements:**
   - **WORKFLOW**: Format as `{UserStoryNumber}.{WorkflowNumber}` (e.g., 1.1, 1.2, 2.1)
   - **TITLE**: Descriptive name for the workflow
   - **DIRECTION**: 
     - `TD` (top-down) - Default for most workflows
     - `LR` (left-right) - For horizontal flows
     - `RL` (right-left) - Rarely used
     - `BT` (bottom-top) - Rarely used
   - **DEPENDS_ON**: List workflow IDs that must complete first, or `NONE` if independent
     - Examples: `NONE`, `1.1`, `1.1, 1.2, 2.1`
   - **CONFLICTS_WITH**: List workflow IDs that cannot coexist with this one, or `NONE`
     - Examples: `NONE`, `1.3`, `2.1, 2.2`
   - **INTERACTION**: Free-text description of how this workflow relates to others
     - Examples:
       - `Primary happy path - foundation for all features`
       - `Alternative to 1.1 - expert users skip wizard`
       - `Extends 1.1 by adding bulk operations`
       - `Conflicts with 3.6 because both modify same data simultaneously`
   
   **Node Requirements:**
   - **NODE**: Simple identifier without spaces (e.g., `Start`, `ValidateStep1`, `ShowError`)
   - **TYPE**: Semantic type for validation
     - `start` - Workflow entry point (use stadium shape)
     - `process` - Action or state (use rect shape)
     - `decision` - Branching logic (use diamond shape)
     - `end` - Workflow completion (use stadium shape)
   - **SHAPE**: Mermaid shape name
     - `stadium` - Rounded pill: `([text])`
     - `rect` - Rectangle: `[text]`
     - `diamond` - Diamond: `{text}`
     - `circle` - Circle: `((text))`
     - `hexagon` - Hexagon: `{{text}}`
   - **LABEL**: Display text (use `<br/>` for multi-line labels)
   
   **Connection Requirements:**
   - Format: `CONNECTION: FromNode -> ToNode` or `CONNECTION: FromNode -> ToNode [Label]`
   - FromNode and ToNode must match NODE names exactly
   - Optional label for decision branches (e.g., `[Yes]`, `[No]`, `[Success]`, `[Error]`)
   
   **Workflow Coverage:**
   - Create separate workflows for:
     - Primary happy path
     - Error handling and edge cases
     - Alternative paths (different modes, bulk operations)
     - Complex multi-step interactions
   - Number workflows sequentially: 1.1, 1.2, 1.3, etc.
   
   **After WORKFLOW_DATA blocks, add placeholder for diagrams:**
   
   ```markdown
   ## User Story [N] Workflow Diagrams
   
   ### Workflow {N}.{X}: {Title}
   
   > **Note**: Run `MermaidProcessor.ps1 -Action Generate` to generate diagrams from WORKFLOW_DATA blocks.
   
   <!-- Mermaid diagram will be inserted here by MermaidGenerator.ps1 -->
   ```
   
   **Example Complete WORKFLOW_DATA Block:**
   
   ```markdown
   ## User Story 1 Workflow Data
   
   <!-- WORKFLOW_START: 1.1 -->
   WORKFLOW: 1.1
   TITLE: Complete 3-Step Guided Workflow
   DIRECTION: TD
   DEPENDS_ON: NONE
   CONFLICTS_WITH: 1.3
   INTERACTION: Primary happy path - conflicts with Expert Mode (1.3) as user can only choose one
   
   NODE: Start
   TYPE: start
   SHAPE: stadium
   LABEL: User selects<br/>Guided Mode
   
   NODE: Step1
   TYPE: process
   SHAPE: rect
   LABEL: Step 1: Order & Part Selection
   
   NODE: ValidateStep1
   TYPE: decision
   SHAPE: diamond
   LABEL: Step 1<br/>valid?
   
   NODE: ShowErrors
   TYPE: process
   SHAPE: rect
   LABEL: Show validation errors
   
   NODE: Step2
   TYPE: process
   SHAPE: rect
   LABEL: Step 2: Load Details
   
   NODE: End
   TYPE: end
   SHAPE: stadium
   LABEL: Workflow complete
   
   CONNECTION: Start -> Step1
   CONNECTION: Step1 -> ValidateStep1
   CONNECTION: ValidateStep1 -> ShowErrors [No]
   CONNECTION: ValidateStep1 -> Step2 [Yes]
   CONNECTION: ShowErrors -> Step1
   CONNECTION: Step2 -> End
   <!-- WORKFLOW_END: 1.1 -->
   
   ## User Story 1 Workflow Diagrams
   
   ### Workflow 1.1: Complete 3-Step Guided Workflow
   
   > **Note**: Run `MermaidProcessor.ps1 -Action Generate` to generate diagrams from WORKFLOW_DATA blocks.
   
   <!-- Mermaid diagram will be inserted here by MermaidGenerator.ps1 -->
   ```

6. **Specification Quality Validation**: After writing the initial spec, validate it against quality criteria:

   a. **Create Spec Quality Checklist**: Generate a checklist file at `FEATURE_DIR/checklists/requirements.md` using the checklist template structure with these validation items:

      ```markdown
      # Specification Quality Checklist: [FEATURE NAME]
      
      **Purpose**: Validate specification completeness and quality before proceeding to planning
      **Created**: [DATE]
      **Feature**: [Link to spec.md]
      
      ## Content Quality
      
      - [ ] No implementation details (languages, frameworks, APIs)
      - [ ] Focused on user value and business needs
      - [ ] Written for non-technical stakeholders
      - [ ] All mandatory sections completed
      
      ## Requirement Completeness
      
      - [ ] No [NEEDS CLARIFICATION] markers remain
      - [ ] Requirements are testable and unambiguous
      - [ ] Success criteria are measurable
      - [ ] Success criteria are technology-agnostic (no implementation details)
      - [ ] All acceptance scenarios are defined
      - [ ] Edge cases are identified
      - [ ] Scope is clearly bounded
      - [ ] Dependencies and assumptions identified
      
      ## Feature Readiness
      
      - [ ] All functional requirements have clear acceptance criteria
      - [ ] User scenarios cover primary flows
      - [ ] Feature meets measurable outcomes defined in Success Criteria
      - [ ] No implementation details leak into specification
      
      ## Notes
      
      - Items marked incomplete require spec updates before `/speckit.clarify` or `/speckit.plan`
      ```

   b. **Run Validation Check**: Review the spec against each checklist item:
      - For each item, determine if it passes or fails
      - Document specific issues found (quote relevant spec sections)

   c. **Handle Validation Results**:

      - **If all items pass**: Mark checklist complete and proceed to step 6

      - **If items fail (excluding [NEEDS CLARIFICATION])**:
        1. List the failing items and specific issues
        2. Update the spec to address each issue
        3. Re-run validation until all items pass (max 3 iterations)
        4. If still failing after 3 iterations, document remaining issues in checklist notes and warn user

      - **If [NEEDS CLARIFICATION] markers remain**:
        1. Extract all [NEEDS CLARIFICATION: ...] markers from the spec
        2. **LIMIT CHECK**: If more than 3 markers exist, keep only the 3 most critical (by scope/security/UX impact) and make informed guesses for the rest
        3. For each clarification needed (max 3), present options to user in this format:

           ```markdown
           ## Question [N]: [Topic]
           
           **Context**: [Quote relevant spec section]
           
           **What we need to know**: [Specific question from NEEDS CLARIFICATION marker]
           
           **Suggested Answers**:
           
           | Option | Answer | Implications |
           |--------|--------|--------------|
           | A      | [First suggested answer] | [What this means for the feature] |
           | B      | [Second suggested answer] | [What this means for the feature] |
           | C      | [Third suggested answer] | [What this means for the feature] |
           | Custom | Provide your own answer | [Explain how to provide custom input] |
           
           **Your choice**: _[Wait for user response]_
           ```

        4. **CRITICAL - Table Formatting**: Ensure markdown tables are properly formatted:
           - Use consistent spacing with pipes aligned
           - Each cell should have spaces around content: `| Content |` not `|Content|`
           - Header separator must have at least 3 dashes: `|--------|`
           - Test that the table renders correctly in markdown preview
        5. Number questions sequentially (Q1, Q2, Q3 - max 3 total)
        6. Present all questions together before waiting for responses
        7. Wait for user to respond with their choices for all questions (e.g., "Q1: A, Q2: Custom - [details], Q3: B")
        8. Update the spec by replacing each [NEEDS CLARIFICATION] marker with the user's selected or provided answer
        9. Re-run validation after all clarifications are resolved

   d. **Update Checklist**: After each validation iteration, update the checklist file with current pass/fail status

7. Report completion with branch name, spec file path, checklist results, and readiness for the next phase (`/speckit.clarify` or `/speckit.plan`).

**NOTE:** The script creates and checks out the new branch and initializes the spec file before writing.

## General Guidelines

## Quick Guidelines

- Focus on **WHAT** users need and **WHY**.
- Avoid HOW to implement (no tech stack, APIs, code structure).
- Written for business stakeholders, not developers.
- DO NOT create any checklists that are embedded in the spec. That will be a separate command.

### Section Requirements

- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation

When creating this spec from a user prompt:

1. **Make informed guesses**: Use context, industry standards, and common patterns to fill gaps
2. **Document assumptions**: Record reasonable defaults in the Assumptions section
3. **Limit clarifications**: Maximum 3 [NEEDS CLARIFICATION] markers - use only for critical decisions that:
   - Significantly impact feature scope or user experience
   - Have multiple reasonable interpretations with different implications
   - Lack any reasonable default
4. **Prioritize clarifications**: scope > security/privacy > user experience > technical details
5. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
6. **Common areas needing clarification** (only if no reasonable default exists):
   - Feature scope and boundaries (include/exclude specific use cases)
   - User types and permissions (if multiple conflicting interpretations possible)
   - Security/compliance requirements (when legally/financially significant)

**Examples of reasonable defaults** (don't ask about these):

- Data retention: Industry-standard practices for the domain
- Performance targets: Standard web/mobile app expectations unless specified
- Error handling: User-friendly messages with appropriate fallbacks
- Authentication method: Standard session-based or OAuth2 for web apps
- Integration patterns: RESTful APIs unless specified otherwise

### Success Criteria Guidelines

Success criteria must be:

1. **Measurable**: Include specific metrics (time, percentage, count, rate)
2. **Technology-agnostic**: No mention of frameworks, languages, databases, or tools
3. **User-focused**: Describe outcomes from user/business perspective, not system internals
4. **Verifiable**: Can be tested/validated without knowing implementation details

**Good examples**:

- "Users can complete checkout in under 3 minutes"
- "System supports 10,000 concurrent users"
- "95% of searches return results in under 1 second"
- "Task completion rate improves by 40%"

**Bad examples** (implementation-focused):

- "API response time is under 200ms" (too technical, use "Users see results instantly")
- "Database can handle 1000 TPS" (implementation detail, use user-facing metric)
- "React components render efficiently" (framework-specific)
- "Redis cache hit rate above 80%" (technology-specific)
