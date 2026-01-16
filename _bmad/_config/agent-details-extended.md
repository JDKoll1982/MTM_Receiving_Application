# BMad Agent Reorganization - Extended Details

## Comprehensive Agent Catalog with Use Cases and Custom Prompts

### 1. Setup & Core (1 agent)

**System initialization and BMad framework setup.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `setup-bmad-master.agent.md` | `bmd-custom-core-bmad-master.agent.md` | `_bmad/core/agents/bmad-master.md` | Starting a new project and need to initialize BMad framework | Master orchestrator - lists available tasks/workflows, executes any BMad operation |

**Custom Prompts** (`.github/prompts/setup-prompts.md`):

- Initialize BMad in existing project
- Configure BMad for new greenfield project
- List all available BMad tasks and workflows
- Execute specific BMad workflow by name
- Troubleshoot BMad configuration issues

### 2. Development Agents (4 agents)

**Hands-on code implementation, testing, and documentation.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `dev-developer.agent.md` | `bmd-custom-bmm-dev.agent.md` | `_bmad/bmm/agents/dev.md` | Implementing a user story with strict TDD/red-green-refactor | Execute dev stories, write tests first, implement features following acceptance criteria |
| `dev-quick-flow.agent.md` | `bmd-custom-bmm-quick-flow-solo-dev.agent.md` | `_bmad/bmm/agents/quick-flow-solo-dev.md` | Solo developer rapidly prototyping a feature without full BMM ceremony | Streamlined dev workflow for solo developers, less process overhead |
| `dev-test-engineer.agent.md` | `bmd-custom-bmm-tea.agent.md` | `_bmad/bmm/agents/tea.md` | Setting up automated testing framework or generating comprehensive test suites | Initialize test frameworks, create E2E tests, generate test automation, CI/CD setup |
| `dev-tech-writer.agent.md` | `bmd-custom-bmm-tech-writer.agent.md` | `_bmad/bmm/agents/tech-writer.md` | Documenting existing brownfield project or creating API docs | Generate project documentation, create Mermaid/Excalidraw diagrams, validate docs |

**Custom Prompts** (`.github/prompts/dev-prompts.md`):

- Implement feature with TDD approach
- Debug failing tests and fix implementation
- Refactor code while maintaining test coverage
- Generate unit tests for existing code
- Create integration tests for API endpoints
- Set up E2E test framework (Playwright/Cypress)
- Write comprehensive test scenarios
- Generate test automation suite
- Create API documentation with code examples
- Document existing codebase architecture
- Generate Mermaid diagrams for system design
- Create flowcharts for complex logic
- Validate documentation against standards

### 3. Planning & Architecture (3 agents)

**High-level design, requirements analysis, and project planning.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `plan-architect.agent.md` | `bmd-custom-bmm-architect.agent.md` | `_bmad/bmm/agents/architect.md` | Designing system architecture for a new microservice | Create architecture documents, check implementation readiness |
| `plan-analyst.agent.md` | `bmd-custom-bmm-analyst.agent.md` | `_bmad/bmm/agents/analyst.md` | Researching market competitors and gathering requirements for new product | Brainstorm projects, conduct research, create product briefs, document existing projects |
| `plan-project-manager.agent.md` | `bmd-custom-bmm-pm.agent.md` | `_bmad/bmm/agents/pm.md` | Creating a PRD from stakeholder interviews and user feedback | Create PRDs through user interviews, generate epics/stories, course correction analysis |

**Custom Prompts** (`.github/prompts/plan-prompts.md`):

- Design scalable system architecture
- Create architecture decision records (ADRs)
- Evaluate technology stack options
- Assess implementation readiness
- Conduct market research and competitive analysis
- Gather and analyze user requirements
- Create detailed product brief
- Conduct stakeholder interviews
- Create comprehensive PRD from discovery
- Break PRD into epics and user stories
- Plan sprint goals and milestones
- Analyze and correct off-track implementation

### 4. Agile/Scrum (1 agent)

**Helps organize team tasks and run effective meetings.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `agile-scrum-master.agent.md` | `bmd-custom-bmm-sm.agent.md` | `_bmad/bmm/agents/sm.md` | Planning next sprint and preparing developer-ready stories | Sprint planning, create developer-ready stories, run retrospectives, course correction |

**Custom Prompts** (`.github/prompts/agile-prompts.md`):

- Generate sprint plan from epic backlog
- Create developer-ready user stories
- Prepare story with acceptance criteria
- Facilitate sprint retrospective
- Analyze blockers and suggest solutions
- Update sprint status and progress
- Convert epic to actionable tasks
- Run sprint planning session

### 5. UI/UX Design (1 agent)

**User interface design, user experience optimization, and interaction patterns.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `ui-ux-designer.agent.md` | `bmd-custom-bmm-ux-designer.agent.md` | `_bmad/bmm/agents/ux-designer.md` | Designing user interface and user flows for new mobile app feature | Generate UX design from PRD, create wireframes, design interaction patterns |

**Custom Prompts** (`.github/prompts/ui-prompts.md`):

- Create UX design from product requirements
- Generate wireframes for web/mobile app
- Design user flow diagrams
- Create design system components
- Establish UI accessibility guidelines
- Design responsive layouts
- Create interaction prototypes
- Validate design against UX best practices

### 6. Build/Scaffold Agents (3 agents)

**Automated scaffolding and code generation for new components.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `build-agent-creator.agent.md` | `bmd-custom-bmb-agent-builder.agent.md` | `_bmad/bmb/agents/agent-builder.md` | Creating a custom BMad agent for specialized domain workflow | Create/edit BMad agents with proper persona, menu, compliance validation |
| `build-module-creator.agent.md` | `bmd-custom-bmb-module-builder.agent.md` | `_bmad/bmb/agents/module-builder.md` | Building a new BMad module for payment processing workflows | Brainstorm modules, create product briefs, scaffold complete modules with agents/workflows |
| `build-workflow-creator.agent.md` | `bmd-custom-bmb-workflow-builder.agent.md` | `_bmad/bmb/agents/workflow-builder.md` | Creating a custom workflow for automated deployment pipeline | Create/edit BMad workflows, validate workflow structure |

**Custom Prompts** (`.github/prompts/build-prompts.md`):

- Create new BMad agent with persona
- Edit existing agent while maintaining compliance
- Validate agent against BMad standards
- Brainstorm new BMad module ideas
- Create product brief for module
- Scaffold complete BMad module
- Create custom workflow template
- Edit workflow while maintaining structure
- Generate workflow from requirements

### 7. Creative/Innovation (6 agents)

**Ideation, problem-solving, and strategic innovation.**

| New Name | Old Name | Links To | Real-World Use | In-App Purpose |
|----------|----------|----------|----------------|----------------|
| `creative-brainstorming.agent.md` | `bmd-custom-cis-brainstorming-coach.agent.md` | `_bmad/cis/agents/brainstorming-coach.md` | Running team ideation session for new product features | Facilitate structured brainstorming with creative techniques |
| `creative-problem-solver.agent.md` | `bmd-custom-cis-creative-problem-solver.agent.md` | `_bmad/cis/agents/creative-problem-solver.md` | Finding innovative solution to technical constraint | Apply creative problem-solving frameworks to challenges |
| `creative-design-thinking.agent.md` | `bmd-custom-cis-design-thinking-coach.agent.md` | `_bmad/cis/agents/design-thinking-coach.md` | Running design thinking workshop for user-centered innovation | Guide design thinking process from empathy to prototyping |
| `creative-innovation.agent.md` | `bmd-custom-cis-innovation-strategist.agent.md` | `_bmad/cis/agents/innovation-strategist.md` | Developing innovation strategy for competitive advantage | Strategic innovation planning and opportunity identification |
| `creative-presentation.agent.md` | `bmd-custom-cis-presentation-master.agent.md` | `_bmad/cis/agents/presentation-master.md` | Creating executive presentation for product launch | Design compelling presentations with storytelling |
| `creative-storytelling.agent.md` | `bmd-custom-cis-storyteller.agent.md` | `_bmad/cis/agents/storyteller/` | Crafting product narrative for marketing campaign | Create compelling narratives and stories |

**Custom Prompts** (`.github/prompts/creative-prompts.md`):

- Facilitate brainstorming session
- Generate wild ideas with structured prompts
- Apply SCAMPER technique to innovation
- Use design thinking for problem solving
- Run empathy mapping exercise
- Create user journey maps
- Generate creative solutions to constraints
- Develop innovation strategy
- Identify market opportunities
- Create compelling product presentation
- Design executive pitch deck
- Craft product narrative and messaging
- Create customer success stories

### 8. Specification Agents (8 agents - Keep existing names)

**Feature specification, requirements documentation, and implementation planning.**

| Name | Real-World Use | In-App Purpose |
|------|----------------|----------------|
| `spec-analyze.agent.md` | Analyzing dependencies and requirements before starting implementation | Analyze project requirements, identify dependencies and risks |
| `spec-checklist.agent.md` | Creating implementation checklist for complex feature | Generate comprehensive implementation checklists |
| `spec-clarify.agent.md` | Clarifying ambiguous requirements with stakeholders | Clarify vague requirements through guided questioning |
| `spec-constitution.agent.md` | Defining project principles and non-negotiables | Create project constitution with core principles |
| `spec-implement.agent.md` | Getting step-by-step implementation guidance | Provide implementation guidance and best practices |
| `spec-plan.agent.md` | Creating detailed implementation plan from requirements | Create actionable implementation plans |
| `spec-specify.agent.md` | Writing detailed technical specification for API | Write comprehensive technical specifications |
| `spec-tasks.agent.md` | Breaking down epic into granular development tasks | Break requirements into actionable tasks |
| `spec-tasks-to-issues.agent.md` | Converting task list to GitHub issues for team tracking | Convert tasks to GitHub issues automatically |

**Custom Prompts** (`.github/prompts/spec-prompts.md`):

- Analyze feature requirements
- Identify technical dependencies
- Assess implementation risks
- Create feature implementation checklist
- Generate testing checklist
- Clarify ambiguous requirements
- Create question list for stakeholders
- Define project constitution
- Establish core principles and constraints
- Create step-by-step implementation guide
- Provide technology-specific guidance
- Generate implementation plan
- Create project timeline
- Write API specification
- Document data models
- Create user story specifications
- Break epic into tasks
- Estimate task complexity
- Convert tasks to GitHub issues
- Generate issue templates

---

## Summary: Custom GitHub Prompts

This reorganization creates **8 custom prompt files** in `.github/prompts/`:

1. **setup-prompts.md** - BMad initialization and configuration (5 prompts)
2. **dev-prompts.md** - Development, testing, documentation (13 prompts)
3. **plan-prompts.md** - Architecture, analysis, project management (12 prompts)
4. **agile-prompts.md** - Sprint planning, retrospectives (8 prompts)
5. **ui-prompts.md** - UX design, wireframes, accessibility (8 prompts)
6. **build-prompts.md** - Agent/module/workflow scaffolding (9 prompts)
7. **creative-prompts.md** - Brainstorming, innovation, storytelling (13 prompts)
8. **spec-prompts.md** - Requirements, specifications, planning (20 prompts)

**Total: 88 custom prompts** covering all agent capabilities for slash command access.
