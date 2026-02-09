---
description: 'Review and update all Memory Bank files to capture current project state, recent changes, and next steps'
name: 'update-memory-bank'
agent: 'agent'
tools: ['read_file', 'replace_string_in_file', 'create_file', 'list_dir']
argument-hint: 'Optional: specific area to focus on (e.g., "testing", "architecture", "progress")'
---

# Update Memory Bank

## Mission

Systematically review ALL Memory Bank files and update them to accurately reflect the current project state. This command ensures that after a memory reset, I can quickly understand the project context, recent changes, and next steps without losing continuity.

## Scope & Preconditions

**When to Use:**
- After completing significant work (new features, bug fixes, refactoring)
- When discovering new architectural patterns or decisions
- When explicitly requested by user with "update memory bank"
- When context needs clarification for future sessions

**Preconditions:**
- Memory Bank folder structure exists at `memory-bank/`
- Core files present: `projectbrief.md`, `productContext.md`, `systemPatterns.md`, `techContext.md`, `activeContext.md`, `progress.md`
- Tasks folder exists with `_index.md`

**If Memory Bank Missing:**
1. Create the complete structure following `.github/instructions/memory-bank.instructions.md`
2. Initialize all core files with current project state
3. Proceed with update workflow

## Inputs

**Required:**
- Current project state (via code inspection, recent commits, test results)
- Recent work completed (from conversation history or git log)

**Optional:**
- `${input:focusArea[:all]}` - Specific area to emphasize (e.g., "testing", "architecture", "tasks")
- User-provided context about decisions or patterns

## Workflow

### Step 1: Review Current Memory Bank State

Read ALL core memory bank files in this order:
1. `memory-bank/projectbrief.md` - Verify scope is current
2. `memory-bank/productContext.md` - Verify goals unchanged
3. `memory-bank/systemPatterns.md` - Check for new patterns
4. `memory-bank/techContext.md` - Verify dependencies current
5. `memory-bank/activeContext.md` - Review recent work
6. `memory-bank/progress.md` - Check milestone status
7. `memory-bank/tasks/_index.md` - Review task statuses
8. Individual task files for recently updated tasks

**Action:** Use `read_file` or `list_dir` to scan memory-bank/ structure.

### Step 2: Analyze Current Project State

Determine what changed since last update:
- New code files or major refactoring
- Test implementations or fixes
- Architectural decisions made
- Dependencies added/updated
- Known issues discovered or resolved

**Action:** Use `code_search` or `file_search` to identify recent changes.

### Step 3: Identify Updates Needed

For EACH core file, determine:
- Is information current and accurate?
- Are there new patterns/decisions to document?
- Has progress been made on milestones?
- Are there new technologies or constraints?
- Have active tasks changed status?

**Create update checklist:**
```markdown
- [ ] projectbrief.md - Update if scope changed
- [ ] productContext.md - Update if goals evolved
- [ ] systemPatterns.md - Add new patterns discovered
- [ ] techContext.md - Update dependencies/versions
- [ ] activeContext.md - Document recent work
- [ ] progress.md - Update milestones/accomplishments
- [ ] tasks/_index.md - Sync task statuses
- [ ] Individual task files - Add progress logs
```

### Step 4: Update Core Files

**For systemPatterns.md:**
- Add newly discovered architectural patterns
- Document critical decisions made
- Update testing patterns with new examples
- Add common pitfalls encountered

**For techContext.md:**
- Update package versions if changed
- Document new dependencies added
- Note any technical constraints discovered

**For activeContext.md:**
- Move "Current Work Focus" to "Recent Changes"
- Document new work focus (next task)
- Update "Active Decisions" with choices made
- Set clear "Next Steps"

**For progress.md:**
- Move completed work to "Recent Accomplishments"
- Update "What Works" section
- Update milestone completion status
- Add new known issues if discovered
- Update "Current Status" section

**Action:** Use `replace_string_in_file` to update existing content or `create_file` if sections missing.

### Step 5: Update Task Tracking

**For tasks/_index.md:**
- Move completed tasks to "Completed" section with date
- Update "In Progress" tasks with current status
- Add new pending tasks if identified
- Archive abandoned tasks with reason

**For individual task files:**
- Add progress log entry with today's date
- Update subtask status table
- Update completion percentage
- Document challenges and decisions made

**Action:** Use `replace_string_in_file` to add entries, preserving existing content.

### Step 6: Update Project Intelligence (.github/instructions/)

If new patterns emerged that should guide future work:
- Document in `.github/copilot-instructions.md` if project-specific
- Create new instruction file if topic warrants dedicated guidance
- Update existing instruction files with refined patterns

**Action:** Use `replace_string_in_file` to add patterns, examples, or best practices.

### Step 7: Validate Updates

Ensure updates follow Memory Bank guidelines:
- All files have "Last Updated: YYYY-MM-DD" updated
- Changes documented clearly and concisely
- No contradictions between files
- activeContext.md accurately reflects current state
- progress.md shows clear path forward

### Step 8: Present Summary

Generate concise summary for user:
```markdown
## Memory Bank Updated: YYYY-MM-DD

### Files Updated:
- systemPatterns.md: Added MediatR 14.0 testing pattern
- techContext.md: Updated MediatR version to 14.0.0
- activeContext.md: Documented AuditBehaviorTests fix
- progress.md: Marked AuditBehaviorTests milestone complete
- tasks/TASK001-fix-auditbehaviortests.md: Added completion entry

### Key Changes Documented:
- MediatR 14.0 delegate signature pattern
- Test type visibility requirements for Moq
- Comprehensive behavior testing approach

### Current State:
- All tests passing (5/5 AuditBehaviorTests)
- Build successful
- Ready for next task: LoggingBehaviorTests
```

## Output Expectations

**Format:**
- Markdown files with consistent formatting
- Clear section headers
- Bullet lists for easy scanning
- Code examples where helpful
- Dates in YYYY-MM-DD format

**Location:**
- Updates in `memory-bank/` folder
- Preserve existing file structure
- Maintain backward compatibility

**Success Criteria:**
- All core files reviewed and updated as needed
- "Last Updated" dates current
- activeContext.md reflects true current state
- progress.md shows clear accomplishments
- Task statuses accurate in _index.md

**Failure Triggers:**
- Unable to read existing memory bank files
- Contradictory information between files
- Missing critical context from recent work

## Quality Assurance

### Validation Steps

**Post-Update Checks:**
1. Read activeContext.md - Does it clearly state current focus?
2. Read progress.md - Are recent accomplishments listed?
3. Read tasks/_index.md - Do statuses match reality?
4. Verify no duplicate information across files
5. Confirm all dates use YYYY-MM-DD format

**User Validation:**
- Present summary of changes made
- Ask: "Does this accurately capture the current state?"
- Adjust based on feedback

### Common Issues

**Issue:** Files don't exist yet
**Solution:** Create complete structure following memory-bank.instructions.md

**Issue:** Too much information to update
**Solution:** Focus on activeContext.md, progress.md, and tasks/ first

**Issue:** Unclear what changed
**Solution:** Ask user for clarification on recent work

## Reference Documentation

- Primary: `.github/instructions/memory-bank.instructions.md`
- Related: `.github/copilot-instructions.md` (project patterns)
- Related: `memory-bank/systemPatterns.md` (architecture reference)

## Example Usage

**User Command:**
```
update memory bank
```

**Agent Response:**
1. "I'll review all Memory Bank files and update them with current project state."
2. [Reads all core files]
3. [Identifies recent work from conversation]
4. [Updates systemPatterns.md with new MediatR pattern]
5. [Updates activeContext.md with completed work]
6. [Updates progress.md with accomplishment]
7. [Updates task file with completion]
8. [Presents summary to user]

**User Command with Focus:**
```
update memory bank - focus on testing patterns
```

**Agent Response:**
1. "I'll update the Memory Bank with emphasis on testing patterns."
2. [Prioritizes systemPatterns.md testing section]
3. [Documents test patterns discovered]
4. [Updates progress.md with test coverage]
5. [Presents focused summary]
