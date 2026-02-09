---
description: 'Continue implementation work on the next incomplete task or a specific task ID'
name: 'implement-task'
agent: 'agent'
tools: ['read_file', 'replace_string_in_file', 'create_file', 'code_search', 'file_search', 'get_file', 'run_command_in_terminal']
argument-hint: 'Optional: Task ID (e.g., "TASK001"). Leave empty to continue next incomplete task.'
---

# Implement Task

## Mission

Automatically continue implementation work on the next incomplete task, or work on a specific task if provided. This command enables seamless continuation of work across sessions by automatically identifying what needs to be done next and executing it.

## Scope & Preconditions

**When to Use:**
- User requests "implement task [ID]" for specific task
- User requests "implement task" with no ID to auto-continue
- User requests "continue work" or "keep going"
- Starting a new work session and want to resume where left off

**Preconditions:**
- Task files exist at `memory-bank/tasks/`
- Task index exists at `memory-bank/tasks/_index.md`
- Tasks have clear implementation plans

**If No Tasks Available:**
- Check for "In Progress" tasks first
- Then check "Pending" tasks
- If none found, report completion or suggest creating new task

## Inputs

**Optional:**
- `${input:taskId[:auto]}` - Task ID to work on (e.g., "TASK001", "TASK003")
  - If empty or "auto": Find next incomplete task automatically
  - If provided: Work on specified task

**Auto-Detection Logic:**
- Priority 1: Tasks with status "In Progress"
- Priority 2: Tasks with status "Pending"
- Within each priority: Oldest first (by "Added" date)

## Workflow

### Step 1: Determine Task to Work On

**If Task ID Provided:**
1. Read `memory-bank/tasks/_index.md`
2. Verify task exists
3. Load task file `memory-bank/tasks/TASKID-*.md`
4. Proceed to Step 2

**If No Task ID (Auto Mode):**
1. Read `memory-bank/tasks/_index.md`
2. Scan "In Progress" section first
   - Select oldest task (first in list)
3. If no "In Progress" tasks, scan "Pending" section
   - Select oldest pending task
4. If no tasks found, report: "No incomplete tasks found. All work complete!"
5. Load selected task file
6. Announce: "Auto-continuing work on [TASKID]: [Task Name]"

**Action:** Use `read_file` to load task index and task file.

### Step 2: Analyze Task Context

Read and parse the task file:

**Extract Key Information:**
- **Overall Status:** Current completion percentage
- **Implementation Plan:** Steps to execute
- **Subtasks:** Which are complete, in progress, or pending
- **Progress Log:** Recent work and decisions
- **Thought Process:** Original approach and reasoning

**Identify Next Work Item:**
- Find first subtask with status "Not Started" or "In Progress"
- If all subtasks complete but task status ≠ "Completed", verify and mark complete
- If blocked subtask found, check if blocker can be resolved

**Action:** Parse task file structure and identify next actionable item.

### Step 3: Understand Implementation Requirements

For the next subtask to implement:

**Review Context:**
- Read relevant source files mentioned in task
- Search for related code patterns
- Review Memory Bank for architectural patterns
- Check recent progress log for decisions made

**Gather Requirements:**
- What files need to be created/modified?
- What patterns should be followed?
- Are there dependencies that must be completed first?
- What validation/testing is required?

**Action:** Use `code_search`, `file_search`, `get_file` to gather context.

### Step 4: Execute Implementation

Implement the next subtask following established patterns:

**For Test Generation Tasks:**
1. Identify source file to test
2. Review existing test patterns (from AuditBehaviorTests or similar)
3. Generate comprehensive test file
4. Use established naming conventions
5. Include all test scenarios (happy path, edge cases, error handling)

**For Feature Implementation Tasks:**
1. Create/modify required files
2. Follow MVVM/CQRS patterns
3. Use dependency injection
4. Add proper error handling
5. Include XML documentation

**For Refactoring Tasks:**
1. Identify code to refactor
2. Plan backward-compatible changes
3. Execute refactoring in small steps
4. Validate after each step

**Action:** Use `create_file`, `replace_string_in_file` to implement changes.

### Step 5: Validate Implementation

**Run Tests:**
```bash
dotnet test [ProjectPath] --filter "Category=Unit" --logger "console;verbosity=minimal"
```

**Check Build:**
```bash
dotnet build [ProjectPath]
```

**Verify Results:**
- All tests pass
- No compilation errors
- Code follows established patterns
- Documentation is complete

**If Validation Fails:**
1. Review error messages
2. Fix issues
3. Re-run validation
4. Repeat until all checks pass

**Action:** Use `run_command_in_terminal` to validate.

### Step 6: Update Task Progress

After successful implementation:

**Update Subtask Status:**
- Mark completed subtask as "Complete"
- Update "Updated" date
- Add notes about implementation details

**Add Progress Log Entry:**
```markdown
### YYYY-MM-DD
- Completed subtask X.Y: [Description]
- Generated [N] tests, all passing
- Followed [Pattern] pattern from [Reference]
- Fixed [Issue] by [Solution]
- Next: Subtask X.Y+1 - [Description]
```

**Recalculate Completion:**
- Count completed subtasks
- Update overall completion percentage
- Update status if needed (Pending → In Progress, In Progress → Completed)

**Action:** Use `replace_string_in_file` to update task file.

### Step 7: Update Memory Bank

If significant progress or patterns discovered:

**Update systemPatterns.md:**
- Document new patterns discovered
- Add code examples
- Note common pitfalls avoided

**Update activeContext.md:**
- Update "Current Work Focus"
- Move completed work to "Recent Changes"
- Set "Next Steps"

**Update progress.md:**
- Add accomplishment to "Recent Accomplishments"
- Update milestone status
- Note any blockers encountered

**Action:** Use `replace_string_in_file` to update Memory Bank files.

### Step 8: Determine Next Action

**If More Work in Current Task:**
- Identify next incomplete subtask
- Announce: "Completed subtask X.Y. Continuing with subtask X.Y+1..."
- Return to Step 3 to implement next subtask

**If Current Task Complete:**
- Mark task as "Completed" with date
- Move to "Completed" section in _index.md
- Update activeContext.md with accomplishment
- Announce: "Task [ID] complete! Moving to next task..."
- Return to Step 1 to find next task (auto mode)

**If All Tasks Complete:**
- Celebrate! 🎉
- Generate comprehensive summary
- Suggest next areas for improvement

**Action:** Continue loop until stopping condition or user interruption.

### Step 9: Present Progress Summary

After each subtask completion, show:

```markdown
## Progress Update: [TASKID]

**Subtask Completed:** X.Y - [Description]

### What Was Done:
- [Implementation detail 1]
- [Implementation detail 2]
- [Implementation detail 3]

### Results:
- ✅ Tests: [N] passing, [M] failing
- ✅ Build: Successful
- ✅ Patterns: [Pattern names followed]

### Task Status:
- Overall: [X]% complete ([N]/[M] subtasks)
- Next: Subtask [X.Y+1] - [Description]

### Continuing automatically...
```

**Action:** Present clear progress to user.

## Output Expectations

**Format:**
- Updated task file with completed subtasks
- Updated Memory Bank files (if significant progress)
- New/modified source files as needed
- Test results showing validation

**Location:**
- Task file: `memory-bank/tasks/TASKID-taskname.md`
- Source files: As specified in task implementation plan
- Test files: `MTM_Receiving_Application.Tests/` hierarchy
- Memory Bank: `memory-bank/` core files

**Success Criteria:**
- Subtask marked complete in task file
- Implementation validates successfully (tests pass, builds succeed)
- Progress log updated with completion details
- Next subtask identified or task marked complete
- Memory Bank updated with new patterns (if applicable)

**Continuation Behavior:**
- **Auto-continue:** Automatically move to next subtask/task
- **Stop conditions:** All tasks complete, blocker encountered, validation fails
- **User interruption:** Gracefully stop and save progress

## Quality Assurance

### Validation Steps

**Per Subtask:**
1. Implementation follows established patterns
2. Tests pass (if test generation task)
3. Build succeeds
4. Code meets quality standards
5. Documentation is complete

**Per Task:**
1. All subtasks marked complete
2. Overall status matches completion
3. Progress log documents entire journey
4. Memory Bank updated with lessons learned

### Common Issues

**Issue:** Can't find next task
**Solution:** Check _index.md for "In Progress" or "Pending" tasks, create new task if needed

**Issue:** Task blocked on dependency
**Solution:** Document blocker, mark task as "Blocked", move to next available task

**Issue:** Validation fails
**Solution:** Review errors, fix issues, re-validate before marking complete

**Issue:** Unclear implementation requirements
**Solution:** Review Thought Process and Progress Log in task file for context

## Special Modes

### Batch Mode (Multiple Subtasks)

Continue implementing until stopping condition:
- All subtasks in current task complete
- Validation failure
- Blocker encountered
- User interrupts

**Announce progress after each subtask:**
```
✅ Completed subtask 2.3 (5/15 subtasks complete)
▶️ Starting subtask 2.4: [Description]
```

### Single Subtask Mode

Implement one subtask then stop:
- Complete current subtask
- Validate
- Update task file
- Present summary
- Stop (don't auto-continue)

**User can specify:** "implement task TASK004 - next subtask only"

## Integration with Other Prompts

**Works With:**
- `update-task`: Update task after manual work
- `update-memory-bank`: Refresh Memory Bank after major progress
- `create-task`: Create new task when all complete

**Workflow Integration:**
```
User: "implement task"
  → Agent finds TASK004 (In Progress, 47%)
  → Agent completes subtasks 2.5, 2.6, 2.7
  → Task now 60% complete
  → Agent updates progress
  → Agent continues to subtask 2.8
  → [Work continues until completion or stop]
```

## Reference Documentation

- Primary: `.github/instructions/memory-bank.instructions.md`
- Related: `.github/prompts/memory-bank-update-task.prompt.md`
- Related: `memory-bank/systemPatterns.md`
- Related: `memory-bank/tasks/_index.md`

## Example Usage

**User Command (Auto Mode):**
```
implement task
```

**Agent Response:**
1. "Scanning for incomplete tasks..."
2. "Found TASK004 (In Progress - 47%): Generate All Unit Tests for Module_Core"
3. "Next subtask: 1.8 - Converter_IconCodeToGlyph"
4. [Reads source file, generates tests]
5. [Validates tests pass]
6. "✅ Completed subtask 1.8: Generated 8 tests, all passing"
7. "Continuing to subtask 1.15: Converter_PartIDToQualityHoldBrush"
8. [Continues automatically...]

**User Command (Specific Task):**
```
implement task TASK005
```

**Agent Response:**
1. "Loading TASK005: Implement LoggingBehaviorTests"
2. "Status: Pending (0% complete)"
3. "Starting subtask 1.1: Analyze LoggingBehavior implementation"
4. [Executes work...]
5. [Updates progress...]

**User Command (Single Subtask):**
```
implement task TASK004 - next subtask only
```

**Agent Response:**
1. "Loading TASK004..."
2. "Implementing subtask 1.8: Converter_IconCodeToGlyph"
3. [Executes work...]
4. "✅ Subtask complete. Stopping as requested."
5. [Does not auto-continue]

## Advanced Features

### Smart Dependency Resolution

If subtask depends on another incomplete subtask:
1. Detect dependency (mentioned in notes or implementation plan)
2. Mark current subtask as "Blocked" with reason
3. Move to dependency subtask
4. Complete dependency
5. Return to original subtask
6. Mark as "In Progress" and complete

### Pattern Learning

As tasks are completed:
1. Identify recurring patterns
2. Suggest adding to systemPatterns.md
3. Auto-update if pattern is clear and consistent
4. Document in progress log for future reference

### Progress Persistence

After each subtask:
1. Save task file immediately
2. Update _index.md if status changed
3. Ensure all progress is persisted
4. Enable graceful interruption and resumption

## Stopping Gracefully

**User can interrupt at any time:**
- Agent saves current progress
- Marks current subtask as "In Progress" (not complete)
- Updates progress log with partial work
- Presents status: "Work paused at subtask X.Y (N% complete)"
- Next "implement task" will resume exactly where left off

## Success Metrics

**Per Session:**
- Subtasks completed: [N]
- Tests generated: [M] ([P]% passing)
- Files created/modified: [K]
- Validation success rate: [X]%

**Overall Task Progress:**
- Completion: [X]% → [Y]% (+[Z] percentage points)
- Time to completion estimate: [N] more subtasks

---

**This prompt enables autonomous, continuous implementation with automatic progress tracking and intelligent task selection.**
