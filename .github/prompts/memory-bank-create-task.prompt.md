---
description: 'Create a new task file in the Memory Bank with unique ID, thought process, implementation plan, and index entry'
name: 'create-task'
agent: 'agent'
tools: ['create_file', 'replace_string_in_file', 'read_file']
argument-hint: 'Task description (e.g., "Implement LoggingBehaviorTests")'
---

# Create Task

## Mission

Create a comprehensive task file in the Memory Bank that preserves the complete thought process, implementation plan, and tracking structure for a new unit of work. Generate a unique Task ID and update the task index.

## Scope & Preconditions

**When to Use:**
- User explicitly requests "add task" or "create task"
- Starting work that will span multiple sessions
- Need to track complex implementation with subtasks
- Want to preserve decision-making process

**Preconditions:**
- Memory Bank exists with `memory-bank/tasks/` folder
- Task index file exists at `memory-bank/tasks/_index.md`

**If Tasks Folder Missing:**
- Create `memory-bank/tasks/` folder
- Create `memory-bank/tasks/_index.md` with template structure

## Inputs

**Required:**
- `${input:taskDescription}` - Brief description of the task (e.g., "Fix AuditBehaviorTests", "Implement user authentication")

**Optional:**
- `${input:priority[:Medium]}` - Task priority (Low, Medium, High, Critical)
- `${input:tags}` - Comma-separated tags for filtering (e.g., "testing,core,behaviors")
- User-provided implementation details or constraints

## Workflow

### Step 1: Generate Unique Task ID

Read `memory-bank/tasks/_index.md` to determine next available ID:
1. Scan all existing task IDs (format: TASKXXX where XXX is zero-padded number)
2. Find highest number used
3. Increment by 1
4. Format as `TASK001`, `TASK002`, etc.

**Example:**
- Existing tasks: TASK001, TASK002, TASK005
- Next ID: TASK006

**Validation:** Ensure ID is unique across all task files.

### Step 2: Create Task Filename

**Format:** `TASKID-taskname.md`

**Rules:**
- Use kebab-case for task name
- Keep name concise but descriptive
- No spaces or special characters
- Example: `TASK001-fix-auditbehaviortests.md`

**Derive from input:**
```
Input: "Implement LoggingBehaviorTests"
Filename: TASK003-implement-loggingbehaviortests.md
```

### Step 3: Develop Thought Process

Document the reasoning behind the approach:
- Why is this work needed?
- What problem does it solve?
- What alternatives were considered?
- Why was this approach chosen?
- What are the key challenges?
- What constraints exist?

**Capture from:**
- Conversation history with user
- Related issues or requirements
- Technical constraints identified
- Dependencies on other work

### Step 4: Create Implementation Plan

Break down the task into concrete subtasks:
- Each subtask is atomic and testable
- Subtasks are in logical execution order
- Dependencies between subtasks are clear
- Each subtask has clear success criteria

**Format:**
```markdown
## Implementation Plan
1. Analyze existing implementation to understand patterns
2. Create concrete test types (TestRequest, TestResponse)
3. Implement test for primary functionality
4. Implement tests for edge cases
5. Fix compilation errors
6. Verify all tests pass
```

**Guidelines:**
- 5-15 subtasks is typical
- Too few = not detailed enough
- Too many = break into multiple tasks
- Use action verbs (Analyze, Create, Implement, Fix, Verify)

### Step 5: Create Task File

Use the Memory Bank task template:

```markdown
# [TASKID] - [Task Name]

**Status:** Pending  
**Added:** [Today's Date YYYY-MM-DD]  
**Updated:** [Today's Date YYYY-MM-DD]

## Original Request
[The user's original task description verbatim]

## Thought Process
[Documentation of reasoning and approach - from Step 3]

## Implementation Plan
[Numbered list of subtasks - from Step 4]

## Progress Tracking

**Overall Status:** Not Started - 0%

### Subtasks
| ID | Description | Status | Updated | Notes |
|----|-------------|--------|---------|-------|
| 1.1 | [First subtask from plan] | Not Started | [Date] | |
| 1.2 | [Second subtask from plan] | Not Started | [Date] | |
| ... | ... | ... | ... | |

## Progress Log
### [Today's Date]
- Task created
- Initial implementation plan developed
- Awaiting execution
```

**Action:** Use `create_file` with path `memory-bank/tasks/{filename}`

### Step 6: Update Task Index

Add new task to `memory-bank/tasks/_index.md` in the **Pending** section:

**Format:**
```markdown
## Pending
- [TASKID] Task Name - Brief status or context
```

**Example:**
```markdown
## Pending
- [TASK003] Implement LoggingBehaviorTests - Unit tests for LoggingBehavior
```

**Placement Rules:**
- New tasks always go in "Pending" section
- Add to end of Pending list (newest last)
- Maintain sorting: In Progress → Pending → Completed → Blocked → Abandoned

**Action:** Use `replace_string_in_file` to add entry under `## Pending` header.

### Step 7: Update Active Context

Add reference to new task in `memory-bank/activeContext.md`:

**In "Next Steps" section:**
```markdown
## Next Steps

### Immediate Tasks
1. ✅ Document work in Memory Bank
2. 🆕 TASK003: Implement LoggingBehaviorTests
```

**Action:** Use `replace_string_in_file` to update Next Steps.

### Step 8: Present Task Summary

Show user the created task structure:

```markdown
## Task Created: TASK003

**File:** memory-bank/tasks/TASK003-implement-loggingbehaviortests.md  
**Status:** Pending  
**Priority:** Medium

### Implementation Plan (6 subtasks):
1. Analyze LoggingBehavior implementation
2. Create test request/response types
3. Implement core logging tests
4. Implement timing tests
5. Implement exception handling tests
6. Verify all tests pass

### Next Actions:
- Review implementation plan
- Begin with subtask 1.1 when ready
- Track progress with "update task TASK003"

Task added to:
- ✅ memory-bank/tasks/_index.md (Pending section)
- ✅ memory-bank/activeContext.md (Next Steps)
```

## Output Expectations

**Format:**
- Markdown file following Memory Bank task template
- Clear section structure with headers
- Table for subtask tracking
- Dated progress log entries

**Location:**
- Task file: `memory-bank/tasks/TASKID-taskname.md`
- Index updated: `memory-bank/tasks/_index.md`
- Context updated: `memory-bank/activeContext.md`

**Success Criteria:**
- Unique Task ID generated
- Task file created with complete structure
- Implementation plan has 5+ concrete subtasks
- Task added to index in Pending section
- activeContext.md updated with next step

**Failure Triggers:**
- Cannot determine next Task ID
- Task description too vague to plan
- Tasks folder doesn't exist and can't create

## Quality Assurance

### Validation Steps

**Pre-Creation Checks:**
1. Task ID is unique (check _index.md)
2. Task description is clear and actionable
3. Implementation plan is detailed enough
4. No duplicate task for same work

**Post-Creation Checks:**
1. Task file exists at correct path
2. Task appears in _index.md Pending section
3. activeContext.md references new task
4. All required sections present in task file
5. Date format is YYYY-MM-DD

### Common Issues

**Issue:** Task description too broad
**Solution:** Ask user to clarify scope or break into multiple tasks

**Issue:** Implementation plan unclear
**Solution:** Discuss approach with user before creating

**Issue:** Duplicate task exists
**Solution:** Update existing task instead of creating new one

## Reference Documentation

- Primary: `.github/instructions/memory-bank.instructions.md` (Task Management section)
- Template: Section "Individual Task Structure" in memory-bank.instructions.md
- Related: `memory-bank/tasks/_index.md` (existing tasks reference)

## Example Usage

**User Command:**
```
create task: Implement LoggingBehaviorTests
```

**Agent Response:**
1. "I'll create a new task for implementing LoggingBehaviorTests."
2. [Reads _index.md to get next ID: TASK003]
3. [Discusses approach with user if needed]
4. [Develops implementation plan]
5. [Creates task file with complete structure]
6. [Updates _index.md with new entry]
7. [Updates activeContext.md]
8. [Presents summary to user]

**User Command with Details:**
```
add task: Fix validation bug in ReceivingWorkflow - priority high
```

**Agent Response:**
1. "Creating high-priority task for validation bug fix."
2. [Generates TASK004]
3. [Documents bug context in Thought Process]
4. [Creates focused implementation plan]
5. [Marks priority as High in task file]
6. [Updates index and context]
7. [Presents summary with urgency noted]
