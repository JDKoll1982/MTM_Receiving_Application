# BMad Agent Reorganization Plan

## Current State Analysis

### Existing Agents (28 total)

#### BMad Core (1 agent)
- `bmd-custom-core-bmad-master.agent.md` → Links to `_bmad/core/agents/bmad-master.md`

#### BMM - Module (9 agents)
- `bmd-custom-bmm-analyst.agent.md` → Links to `_bmad/bmm/agents/analyst.md`
- `bmd-custom-bmm-architect.agent.md` → Links to `_bmad/bmm/agents/architect.md`
- `bmd-custom-bmm-dev.agent.md` → Links to `_bmad/bmm/agents/dev.md`
- `bmd-custom-bmm-pm.agent.md` → Links to `_bmad/bmm/agents/pm.md`
- `bmd-custom-bmm-quick-flow-solo-dev.agent.md` → Links to `_bmad/bmm/agents/quick-flow-solo-dev.md`
- `bmd-custom-bmm-sm.agent.md` → Links to `_bmad/bmm/agents/sm.md`
- `bmd-custom-bmm-tea.agent.md` → Links to `_bmad/bmm/agents/tea.md`
- `bmd-custom-bmm-tech-writer.agent.md` → Links to `_bmad/bmm/agents/tech-writer.md`
- `bmd-custom-bmm-ux-designer.agent.md` → Links to `_bmad/bmm/agents/ux-designer.md`

#### BMB - Builder (3 agents)
- `bmd-custom-bmb-agent-builder.agent.md` → Links to `_bmad/bmb/agents/agent-builder.md`
- `bmd-custom-bmb-module-builder.agent.md` → Links to `_bmad/bmb/agents/module-builder.md`
- `bmd-custom-bmb-workflow-builder.agent.md` → Links to `_bmad/bmb/agents/workflow-builder.md`

#### CIS - Creative/Innovation (6 agents)
- `bmd-custom-cis-brainstorming-coach.agent.md` → Links to `_bmad/cis/agents/brainstorming-coach.md`
- `bmd-custom-cis-creative-problem-solver.agent.md` → Links to `_bmad/cis/agents/creative-problem-solver.md`
- `bmd-custom-cis-design-thinking-coach.agent.md` → Links to `_bmad/cis/agents/design-thinking-coach.md`
- `bmd-custom-cis-innovation-strategist.agent.md` → Links to `_bmad/cis/agents/innovation-strategist.md`
- `bmd-custom-cis-presentation-master.agent.md` → Links to `_bmad/cis/agents/presentation-master.md`
- `bmd-custom-cis-storyteller.agent.md` → Links to `_bmad/cis/agents/storyteller/` (directory?)

#### Speckit (8 agents)
- `speckit.analyze.agent.md`
- `speckit.checklist.agent.md`
- `speckit.clarify.agent.md`
- `speckit.constitution.agent.md`
- `speckit.implement.agent.md`
- `speckit.plan.agent.md`
- `speckit.specify.agent.md`
- `speckit.tasks.agent.md`
- `speckit.taskstoissues.agent.md`

---

## Proposed Reorganization

### Naming Convention
Use functional prefixes to categorize agents by purpose:
- **setup-*** - BMad system setup and initialization
- **dev-*** - Development workflow agents
- **ui-*** - User interface and design agents
- **plan-*** - Planning and architecture agents
- **build-*** - Builder/scaffolding agents
- **creative-*** - Innovation and brainstorming agents
- **spec-*** - Specification and documentation agents

### Reorganized Agent List

#### 1. Setup & Core (1 agent)
**System initialization and BMad framework setup.** These agents handle the foundational setup of the BMad methodology, configure the development environment, and establish core workflows. Use when starting a new project or initializing BMad in an existing repository.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `setup-bmad-master.agent.md` | `bmd-custom-core-bmad-master.agent.md` | `_bmad/core/agents/bmad-master.md` | Master setup and BMad initialization |

#### 2. Development Agents (4 agents)
**Hands-on code implementation, testing, and documentation.** These agents assist with writing code, debugging, refactoring, testing, and creating technical documentation. Use when actively developing features, fixing bugs, or documenting code.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `dev-developer.agent.md` | `bmd-custom-bmm-dev.agent.md` | `_bmad/bmm/agents/dev.md` | Code implementation and development |
| `dev-quick-flow.agent.md` | `bmd-custom-bmm-quick-flow-solo-dev.agent.md` | `_bmad/bmm/agents/quick-flow-solo-dev.md` | Rapid solo development workflow |
| `dev-test-engineer.agent.md` | `bmd-custom-bmm-tea.agent.md` | `_bmad/bmm/agents/tea.md` | Testing and QA |
| `dev-tech-writer.agent.md` | `bmd-custom-bmm-tech-writer.agent.md` | `_bmad/bmm/agents/tech-writer.md` | Technical documentation |

#### 3. Planning & Architecture (3 agents)
**High-level design, requirements analysis, and project planning.** These agents help design system architecture, analyze requirements, plan sprints, and manage project timelines. Use during the planning phase or when making architectural decisions.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `plan-architect.agent.md` | `bmd-custom-bmm-architect.agent.md` | `_bmad/bmm/agents/architect.md` | System architecture and design |
| `plan-analyst.agent.md` | `bmd-custom-bmm-analyst.agent.md` | `_bmad/bmm/agents/analyst.md` | Requirements analysis |
| `plan-project-manager.agent.md` | `bmd-custom-bmm-pm.agent.md` | `_bmad/bmm/agents/pm.md` | Project planning and management |

#### 4. Agile/Scrum (1 agent)
**Helps organize team tasks and run effective meetings.** This agent assists with planning work sprints, running daily check-ins, organizing tasks, and facilitating team improvement discussions. Use when you need help managing tasks or organizing team workflows.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `agile-scrum-master.agent.md` | `bmd-custom-bmm-sm.agent.md` | `_bmad/bmm/agents/sm.md` | Scrum ceremonies and agile processes |

#### 5. UI/UX Design (1 agent)
**User interface design, user experience optimization, and interaction patterns.** This agent helps design intuitive UIs, create wireframes, establish design systems, and ensure accessibility. Use when designing new features or improving user experience.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `ui-ux-designer.agent.md` | `bmd-custom-bmm-ux-designer.agent.md` | `_bmad/bmm/agents/ux-designer.md` | User experience and interface design |

#### 6. Build/Scaffold Agents (3 agents)
**Automated scaffolding and code generation for new components.** These agents generate boilerplate code, create new modules following established patterns, and build workflow templates. Use when creating new features, agents, or workflows to maintain consistency.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `build-agent-creator.agent.md` | `bmd-custom-bmb-agent-builder.agent.md` | `_bmad/bmb/agents/agent-builder.md` | Create new BMad agents |
| `build-module-creator.agent.md` | `bmd-custom-bmb-module-builder.agent.md` | `_bmad/bmb/agents/module-builder.md` | Create new feature modules |
| `build-workflow-creator.agent.md` | `bmd-custom-bmb-workflow-builder.agent.md` | `_bmad/bmb/agents/workflow-builder.md` | Create new workflows |

#### 7. Creative/Innovation (6 agents)
**Ideation, problem-solving, and strategic innovation.** These agents facilitate brainstorming sessions, guide design thinking exercises, help solve complex problems creatively, and craft compelling presentations. Use for innovation workshops, strategic planning, or creative problem-solving.

| New Name | Old Name | Links To | Purpose |
|----------|----------|----------|---------|
| `creative-brainstorming.agent.md` | `bmd-custom-cis-brainstorming-coach.agent.md` | `_bmad/cis/agents/brainstorming-coach.md` | Facilitated brainstorming sessions |
| `creative-problem-solver.agent.md` | `bmd-custom-cis-creative-problem-solver.agent.md` | `_bmad/cis/agents/creative-problem-solver.md` | Creative problem-solving techniques |
| `creative-design-thinking.agent.md` | `bmd-custom-cis-design-thinking-coach.agent.md` | `_bmad/cis/agents/design-thinking-coach.md` | Design thinking workshops |
| `creative-innovation.agent.md` | `bmd-custom-cis-innovation-strategist.agent.md` | `_bmad/cis/agents/innovation-strategist.md` | Innovation strategy and ideation |
| `creative-presentation.agent.md` | `bmd-custom-cis-presentation-master.agent.md` | `_bmad/cis/agents/presentation-master.md` | Presentation design and delivery |
| `creative-storytelling.agent.md` | `bmd-custom-cis-storyteller.agent.md` | `_bmad/cis/agents/storyteller/` | Narrative and storytelling |

#### 8. Specification Agents (8 agents - Keep existing names)
**Feature specification, requirements documentation, and implementation planning.** These agents help write detailed specs, break down features into tasks, create checklists, clarify requirements, and convert tasks to GitHub issues. Use throughout the development lifecycle for documentation and planning.

| Name | Purpose |
|------|---------|
| `spec-analyze.agent.md` | Analyze requirements and dependencies |
| `spec-checklist.agent.md` | Generate implementation checklists |
| `spec-clarify.agent.md` | Clarify ambiguous requirements |
| `spec-constitution.agent.md` | Define project constitution/principles |
| `spec-implement.agent.md` | Implementation guidance |
| `spec-plan.agent.md` | Create implementation plans |
| `spec-specify.agent.md` | Write detailed specifications |
| `spec-tasks.agent.md` | Break down into tasks |

---

## Custom Prompts Strategy

### Standard Agent Template (Simplified)
Remove `<custom>` tags since no customizations exist. Use this template:

```chatagent
---
description: "[Agent Purpose]"
tools: ["changes","edit","fetch","githubRepo","problems","runCommands","runTasks","runTests","search","runSubagent","testFailure","todos","usages"]
---

# [Agent Name]

You must fully embody this agent's persona and follow all activation instructions exactly as specified.

<agent-activation CRITICAL="TRUE">
1. LOAD the FULL agent file from @[path-to-agent]
2. READ its entire contents - this contains the complete agent persona, menu, and instructions
3. Execute ALL activation steps exactly as written in the agent file
4. Follow the agent's persona and menu system precisely
5. Stay in character throughout the session
</agent-activation>
```

### Custom Prompts File Structure
Create / Validate existance of `.github/prompts/` directory with custom prompt files for each agent category (for slash command support):

#### prompts/dev-agents.md
Multi-capability prompts for development agents:
- Code implementation
- Debugging and troubleshooting
- Refactoring and optimization
- Code review
- Performance analysis

#### prompts/plan-agents.md
Multi-capability prompts for planning agents:
- Architecture design
- Requirements analysis
- Technical debt assessment
- Migration planning
- Technology evaluation

#### prompts/build-agents.md
Multi-capability prompts for builder agents:
- Scaffold new modules
- Create agent definitions
- Generate workflow templates
- Setup project structure

#### prompts/creative-agents.md
Multi-capability prompts for creative agents:
- Brainstorming sessions
- Design thinking exercises
- Problem reframing
- Innovation workshops
- Presentation design

#### prompts/spec-agents.md
Multi-capability prompts for specification agents:
- Feature specification
- API documentation
- User story creation
- Acceptance criteria
- Implementation planning

---

## Implementation Plan

### Phase 1: Validation
1. ✅ **Verify all agent links** - Ensure each agent file references an actual file in `_bmad/`
2. ✅ **Check for orphaned agents** - Identify any agents without corresponding `_bmad/` files
3. ✅ **Document missing agents** - Note if any `_bmad/` agents lack `.github/agents/` wrappers

### Phase 2: Rename & Simplify
1. **Rename all agents** following the new naming convention
2. **Remove `<custom>` tags** from all agent files (no customizations exist)
3. **Update descriptions** to be more descriptive and action-oriented

### Phase 3: Custom Prompts
1. **Create `.github/prompts/` directory** (for slash command support)
2. **Generate multi-capability prompt files** for each agent category
3. **Link prompts in agent files** (optional reference in description)

### Phase 4: Documentation
1. **Update agent index** in `_bmad/README.md` (if exists)
2. **Create agent quick reference** guide
3. **Document naming conventions** for future agent creation

---

## Validation Checklist

### Agent File Links
- [ ] All BMM agents link to `_bmad/bmm/agents/*.md`
- [ ] All BMB agents link to `_bmad/bmb/agents/*.md`
- [ ] All CIS agents link to `_bmad/cis/agents/*.md`
- [ ] Core agent links to `_bmad/core/agents/bmad-master.md`
- [ ] Speckit agents link correctly (if applicable)

### File Verification Needed
- [ ] Check if `_bmad/cis/agents/storyteller/` is a directory or should be `storyteller.md`
- [ ] Verify all agent files in `_bmad/` directories are accounted for
- [ ] Confirm speckit agents have corresponding source files

---

## Benefits of Reorganization

### 1. **Improved Discoverability**
- Functional prefixes make it clear what each agent does
- Easier to find the right agent for the task
- Better autocomplete in IDE

### 2. **Reduced Clutter**
- Remove unnecessary `<custom>` tags
- Cleaner, more readable agent files
- Standardized template

### 3. **Enhanced Capabilities**
- Multi-capability custom prompts
- Each agent can handle various tasks within its domain
- Better prompt reusability

### 4. **Future-Proofing**
- Clear naming convention for new agents
- Scalable organization structure
- Easy to add new categories

---

## Next Steps

**Option 1: Auto-Execute**
- Proceed with full reorganization automatically
- Rename all 28 agents
- Create custom prompt files
- Generate validation report

**Option 2: Incremental Approach**
- Start with one category (e.g., dev agents)
- Validate approach
- Proceed with remaining categories

**Option 3: Manual Review**
- Review this plan
- Approve specific changes
- Execute in phases with confirmation

**Recommendation**: Option 1 (Auto-Execute) - The plan is comprehensive and low-risk since we're primarily renaming files and removing empty tags.
