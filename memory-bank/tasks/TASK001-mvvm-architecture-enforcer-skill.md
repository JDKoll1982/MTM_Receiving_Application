# TASK001 - Create mvvm-architecture-enforcer skill and skill-authoring instructions

**Status:** In Progress  
**Added:** 2026-01-24  
**Updated:** 2026-01-24

## Original Request
- Generate a plan to create the skill referenced by the HTML documentation.
- Generate a new custom instruction file for creating skills.
- User created `memory-bank/` and requested initialization.

## Thought Process
This repo uses strict MVVM rules and has an Agent Skills documentation site. The getting started tutorial demonstrates a "mvvm-architecture-enforcer" skill with scripts, references, and templates. To make the docs truthful and actionable, the skill should exist in `.github/skills/` with the referenced files. Additionally, the team wants standardized guidance for authoring future skills, which belongs in a new `.github/instructions/*.instructions.md`.

## Implementation Plan
- Initialize `memory-bank/` with required core files.
- Add a skill-authoring custom instruction file.
- Create `.github/skills/mvvm-architecture-enforcer/` and populate SKILL.md, scripts, references, templates.
- Update `docs/agent-skills-guide/` HTML pages to reference the real skill files.
- Validate navigation/anchor links.

## Progress Tracking

**Overall Status:** In Progress - 90%

### Subtasks
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | Initialize memory-bank core files | Complete | 2026-01-24 | Created core Memory Bank docs + task tracking |
| 1.2 | Add skill-authoring instruction file | Complete | 2026-01-24 | Added `.github/instructions/skill-authoring.instructions.md` |
| 1.3 | Create mvvm-architecture-enforcer skill folder | Complete | 2026-01-24 | Created `.github/skills/mvvm-architecture-enforcer/` with SKILL.md + resources |
| 1.4 | Update HTML docs to reference real skill files | Complete | 2026-01-24 | Updated `getting-started.html` to link to real repo files |
| 1.5 | Validate docs (links/anchors) | Complete | 2026-01-24 | Verified key href targets and anchors via repository search |
| 1.6 | Optional: Add LICENSE.txt | Not Started | 2026-01-24 | Only if explicit licensing in the skill folder is desired |

## Progress Log
### 2026-01-24
- Started initializing `memory-bank/` directory with required core files and first task entry.

### 2026-01-24
- Completed Memory Bank initialization (core files + tasks tracking).
- Added `.github/instructions/skill-authoring.instructions.md` to standardize creating skills.
- Implemented `.github/skills/mvvm-architecture-enforcer/` with scripts/references/templates.
- Updated `docs/agent-skills-guide/getting-started.html` to link to real skill files.
- Validated navigation links and key anchors without using terminal commands (terminal scripts can lock up).
