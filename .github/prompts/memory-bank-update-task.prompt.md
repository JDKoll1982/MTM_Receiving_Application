---
description: 'Update an existing task with progress, status changes, and decision documentation'
name: 'update-task'
agent: 'agent'
tools: ['read_file', 'replace_string_in_file']
argument-hint: 'Task ID (e.g., "TASK001") and optional progress description'
---

# Update Task

## Mission

Update an existing Memory Bank task file with progress details, subtask status changes, decision documentation, and overall status tracking. Maintain complete audit trail of work progress for continuity across sessions.

## Scope & Preconditions

**When to Use:**
- User requests "update task [ID]"
- Progress made on a task (subtask completed, status changed)
- Decision made affecting task approach
- Issue encountered requiring documentation
- Task blocked or completed

**Preconditions:**
- Task file exists at `memory-bank/tasks/TASKID-taskname.md`
- Task index exists at `memory-bank/tasks/_index.md`
- Task ID is valid (format: TASKXXX)

**If Task Not Found:**
- Verify ID format and existence in _index.md
- Suggest similar task IDs if typo suspected
- Offer to create new task if work is new

## Inputs

**Required:**
- `${input:taskId}` - Task ID to update (e.g., "TASK001", "TASK003")

**Optional:**
- `${input:progressDescription}` - What was accomplished (e.g., "Completed subtask 1.2, fixed compilation errors")
- `${input:newStatus}` - Status change (Pending → In Progress → Completed/Blocked/Abandoned)
- `${input:completionPercentage}` - Overall completion (e.g., "60%")
- Conversation context about decisions made

## Workflow

### Step 1: Locate and Read Task File

**Action:** Use `read_file` to load task content

**Path Pattern:** `memory-bank/tasks/TASKID-*.md`

**Validation:**
- Task file exists
- File follows expected format
- Can parse current status and subtasks

**If Multiple Files Match:**
- Use _index.md to find exact filename
- Warn if filename doesn't match expected pattern

### Step 2: Determine Update Type

Analyze what kind of update is needed:

**Status Update:**
- Moving from Pending → In Progress
- Marking as Completed
- Marking as Blocked (with reason)
- Marking as Abandoned (with reason)

**Progress Update:**
- One or more subtasks completed
- Work underway on specific subtask
- Partial progress on task

**Decision Documentation:**
- Approach changed
- Issue encountered and solved
- Trade-off made
- Technical discovery

**Combination:**
- Status change + progress + decisions

### Step 3: Update Subtask Status Table

Identify which subtasks changed:
- Mark as "Complete" if subtask finished
- Mark as "In Progress" if currently working
- Mark as "Blocked" if cannot proceed
- Update "Updated" column with today's date
- Add notes if relevant context exists

**Before:**
```markdown
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | Analyze implementation | Not Started | 2025-01-15 | |
| 1.2 | Create test types | Not Started | 2025-01-15 | |
```

**After:**
```markdown
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | Analyze implementation | Complete | 2025-01-19 | Pattern identified |
| 1.2 | Create test types | Complete | 2025-01-19 | Made public for Moq |
```

**Action:** Use `replace_string_in_file` to update table rows.

### Step 4: Add Progress Log Entry

Create new dated entry in Progress Log section:

**Format:**
```markdown
### YYYY-MM-DD
- Updated subtask X.Y status to [Status]
- [Action taken or work completed]
- Encountered issue with [problem description]
- Made decision to [approach/solution]
- [Additional context or findings]
```

**Guidelines:**
- One entry per update session
- Bullet list format for easy scanning
- Specific details about what changed
- Document WHY decisions were made
- Link to code files or references if helpful

**Example:**
```markdown
### 2025-01-19
- Updated subtask 1.1 status to Complete
- Updated subtask 1.2 status to Complete
- Started work on subtask 1.3 (implement test for primary functionality)
- Discovered MediatR 14.0 changed RequestHandlerDelegate signature
- Made decision to use discard parameter `(_)` for CancellationToken
- Encountered Moq proxy generation error with private test types
- Fixed by making TestRequest and TestResponse public
- All compilation errors resolved
```

**Action:** Use `replace_string_in_file` to add new section at end of Progress Log.

### Step 5: Update Overall Status and Completion

**Calculate Completion Percentage:**
```
Completion % = (Completed Subtasks / Total Subtasks) × 100
```

**Example:**
- Total subtasks: 10
- Completed: 6
- Completion: 60%

**Update Overall Status:**
- Not Started → In Progress (when first subtask starts)
- In Progress → Blocked (if cannot proceed)
- In Progress → Completed (when all subtasks done)
- Any → Abandoned (if work cancelled)

**Format:**
```markdown
**Overall Status:** In Progress - 60%
```

**Action:** Use `replace_string_in_file` to update status line.

### Step 6: Update "Updated" Date

Change the metadata at top of file:

**Before:**
```markdown
**Updated:** 2025-01-15
```

**After:**
```markdown
**Updated:** 2025-01-19
```

**Action:** Use `replace_string_in_file` to update date.

### Step 7: Update Task Index

If status changed (especially to Completed, Blocked, or Abandoned):

**Move entry to correct section in _index.md:**

**Before (Pending section):**
```markdown
## Pending
- [TASK001] Fix AuditBehaviorTests - Placeholder test with issues
```

**After (Completed section):**
```markdown
## Completed
- [TASK001] Fix AuditBehaviorTests - Completed on 2025-01-19
```

**Guidelines:**
- Update description to reflect completion/blocking
- Add completion date for Completed tasks
- Add blocking reason for Blocked tasks
- Add abandonment reason for Abandoned tasks

**Action:** Use `replace_string_in_file` to move entry between sections.

### Step 8: Update Active Context (If Major Progress)

If this update represents significant progress or completion:

**Update activeContext.md:**
- Move completed work to "Recent Changes"
- Update "Current Work Focus" if switching tasks
- Update "Next Steps" with new immediate actions

**Example:**
```markdown
## Recent Changes

### Completed: AuditBehaviorTests Implementation (TASK001)
Fixed all issues in placeholder test file, implemented 5 comprehensive tests,
documented MediatR 14.0 testing patterns. All tests passing.

## Current Work Focus

### Next: LoggingBehaviorTests Implementation (TASK002)
Apply patterns from TASK001 to implement comprehensive tests for LoggingBehavior.
```

**Action:** Use `replace_string_in_file` to update activeContext.md.

### Step 9: Present Update Summary

Show user what changed:

```markdown
## Task Updated: TASK001

**Status:** Pending → Completed (100%)  
**Updated:** 2025-01-19

### Progress Since Last Update:
- ✅ Subtask 1.8: Fixed delegate signature
- ✅ Subtask 1.9: Fixed Moq proxy generation  
- ✅ Subtask 1.10: Verified all tests passing

### Key Accomplishments:
- All 5 tests passing
- Documented MediatR 14.0 testing patterns
- Established reusable test structure

### Challenges Overcome:
- MediatR delegate signature change
- Moq proxy generation with private types

### Next Actions:
- Apply patterns to TASK002 (LoggingBehaviorTests)
- Update systemPatterns.md with testing patterns
```

## Output Expectations

**Format:**
- Updated markdown task file with new progress log entry
- Updated subtask table with current statuses
- Updated completion percentage
- Updated "Updated" date

**Location:**
- Task file: `memory-bank/tasks/TASKID-taskname.md`
- Index (if status changed): `memory-bank/tasks/_index.md`
- Active context (if significant): `memory-bank/activeContext.md`

**Success Criteria:**
- Progress log has new dated entry
- Subtask statuses reflect reality
- Completion percentage is accurate
- Overall status matches subtask completion
- _index.md reflects correct status

**Failure Triggers:**
- Task ID not found
- Cannot parse existing task file
- Conflicting status information

## Quality Assurance

### Validation Steps

**Pre-Update Checks:**
1. Task file exists and is readable
2. Task ID format is valid
3. Update information is clear

**Post-Update Checks:**
1. Progress log has new entry with today's date
2. Subtask table accurately reflects progress
3. Completion % matches completed subtasks
4. Overall status aligns with completion %
5. _index.md shows correct status
6. "Updated" date is current

### Common Issues

**Issue:** Task not found
**Solution:** Verify ID, check _index.md for correct ID, offer alternatives

**Issue:** Unclear what progress was made
**Solution:** Ask user for specifics about work completed

**Issue:** Subtask not in original plan
**Solution:** Add new subtask to plan before marking complete

**Issue:** Completion % doesn't match subtasks
**Solution:** Recalculate based on completed vs total

## Reference Documentation

- Primary: `.github/instructions/memory-bank.instructions.md` (Task Management section)
- Related: `memory-bank/tasks/_index.md` (task status reference)
- Related: `memory-bank/activeContext.md` (current work context)

## Example Usage

**User Command:**
```
update task TASK001: completed subtasks 1.8 and 1.9, all tests passing
```

**Agent Response:**
1. "I'll update TASK001 with your progress."
2. [Reads task file]
3. [Updates subtask 1.8 and 1.9 to Complete]
4. [Adds progress log entry with details]
5. [Calculates new completion: 90%]
6. [Updates overall status]
7. [Presents summary]

**User Command:**
```
update task TASK003 - blocked on database access
```

**Agent Response:**
1. "Marking TASK003 as Blocked due to database access."
2. [Reads task file]
3. [Changes overall status to Blocked]
4. [Adds progress log with blocking reason]
5. [Moves to Blocked section in _index.md]
6. [Updates activeContext.md with blocker]
7. [Presents summary with next steps to unblock]

**User Command:**
```
update task TASK001 - completed!
```

**Agent Response:**
1. "Marking TASK001 as Completed."
2. [Verifies all subtasks are complete]
3. [Updates overall status to Completed - 100%]
4. [Adds completion entry to progress log]
5. [Moves to Completed section in _index.md with date]
6. [Updates activeContext.md with accomplishment]
7. [Presents celebration summary]
