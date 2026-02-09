---
description: 'Display filtered list of tasks from Memory Bank with status, completion, and next actions'
name: 'show-tasks'
agent: 'ask'
tools: ['read_file']
argument-hint: 'Filter (all|active|pending|completed|blocked|recent|tag:name|priority:level)'
---

# Show Tasks

## Mission

Display a filtered, formatted list of tasks from the Memory Bank task index, showing status, completion percentage, last updated date, and next pending subtask for quick overview and planning.

## Scope & Preconditions

**When to Use:**
- User requests "show tasks [filter]"
- Quick overview of work in progress
- Planning next work session
- Reviewing completed work
- Identifying blocked tasks

**Preconditions:**
- Task index exists at `memory-bank/tasks/_index.md`
- At least one task has been created

**If No Tasks:**
- Inform user no tasks exist yet
- Suggest using "create task" to add work items

## Inputs

**Required:**
- `${input:filter[:all]}` - Filter type for task display

**Valid Filters:**
- `all` - Show all tasks regardless of status (default)
- `active` - Show only "In Progress" tasks
- `pending` - Show only "Pending" tasks  
- `completed` - Show only "Completed" tasks
- `blocked` - Show only "Blocked" tasks
- `recent` - Show tasks updated in last 7 days
- `tag:tagname` - Show tasks with specific tag (if tags implemented)
- `priority:level` - Show tasks with specific priority (if priority implemented)

## Workflow

### Step 1: Read Task Index

**Action:** Use `read_file` to load `memory-bank/tasks/_index.md`

**Parse Structure:**
```markdown
## In Progress
- [TASKID] Task Name - Status description

## Pending
- [TASKID] Task Name - Status description

## Completed
- [TASKID] Task Name - Completed on YYYY-MM-DD

## Blocked
- [TASKID] Task Name - Blocking reason

## Abandoned
- [TASKID] Task Name - Abandonment reason
```

**Extract:**
- Task ID (e.g., TASK001)
- Task name
- Status section
- Additional context (completion date, blocking reason, etc.)

### Step 2: Apply Filter

Based on `${input:filter}`, select which tasks to display:

**Filter: "all"**
- Include tasks from all sections
- Sort by status (In Progress → Pending → Blocked → Completed → Abandoned)

**Filter: "active"**
- Include only tasks from "In Progress" section

**Filter: "pending"**
- Include only tasks from "Pending" section

**Filter: "completed"**
- Include only tasks from "Completed" section

**Filter: "blocked"**
- Include only tasks from "Blocked" section

**Filter: "recent"**
- Read each task file to check "Updated" date
- Include tasks updated within last 7 days
- Sort by most recently updated first

**Filter: "tag:tagname"**
- Read each task file to check for tag metadata
- Include tasks with matching tag
- Note: Requires task files to have tag metadata

**Filter: "priority:level"**
- Read each task file to check priority metadata
- Include tasks with matching priority
- Note: Requires task files to have priority metadata

### Step 3: Enrich Task Information (If Detailed View)

For each task in filtered list, optionally read task file to get:
- Overall completion percentage
- Last updated date
- Next pending subtask
- Current blocking issue (if blocked)

**Decision:** Only read full task files if:
- Filter is "active" or "recent" (limited set)
- User requested detailed view
- Otherwise, use summary from _index.md

### Step 4: Format Output

**Compact Format** (for large lists):
```markdown
## Tasks: [Filter Applied]

**In Progress (2)**
- [TASK003] Implement LoggingBehaviorTests - 40% complete
- [TASK005] Refactor DAO layer - 65% complete

**Pending (3)**
- [TASK006] Add validation tests
- [TASK007] Update documentation
- [TASK008] Performance optimization

**Completed (1)**
- [TASK001] Fix AuditBehaviorTests - Completed 2025-01-19
```

**Detailed Format** (for specific filters):
```markdown
## Active Tasks (2)

### TASK003: Implement LoggingBehaviorTests
**Status:** In Progress - 40%  
**Updated:** 2025-01-19  
**Next:** Subtask 1.4 - Implement timing tests  
**Note:** Applying patterns from TASK001

### TASK005: Refactor DAO layer
**Status:** In Progress - 65%  
**Updated:** 2025-01-18  
**Next:** Subtask 2.7 - Update integration tests  
**Note:** Breaking change in Model_Dao_Result signature
```

**Summary Statistics:**
```markdown
## Task Summary
- Total Tasks: 8
- In Progress: 2 (25%)
- Pending: 3 (37.5%)
- Completed: 1 (12.5%)
- Blocked: 1 (12.5%)
- Abandoned: 1 (12.5%)
```

### Step 5: Present Results

Display formatted task list to user with:
- Filter applied (e.g., "Showing: Active Tasks")
- Task count for current filter
- Relevant task details
- Summary statistics (if showing all)
- Suggested next actions

**For "active" filter:**
```markdown
## Active Tasks (2)

[Detailed task information]

**Suggested Next Actions:**
- Continue TASK003 subtask 1.4
- Review blocker on TASK005
```

**For "completed" filter:**
```markdown
## Completed Tasks (5)

[List with completion dates]

**Recent Accomplishments:**
- 3 tasks completed this week
- 2 tasks completed last week
```

**For "blocked" filter:**
```markdown
## Blocked Tasks (1)

[Detailed blocking information]

**Action Required:**
- TASK005: Waiting for database access credentials
  - Contact: IT team
  - ETA: Unknown
```

### Step 6: Offer Follow-Up Actions

Based on filter results, suggest relevant actions:

**If showing active tasks:**
- "Continue work on TASKXXX?"
- "Update progress on any of these?"

**If showing pending tasks:**
- "Ready to start TASKXXX?"
- "Want to prioritize any of these?"

**If showing blocked tasks:**
- "Need help unblocking TASKXXX?"
- "Should any be marked as abandoned?"

**If no tasks match filter:**
- "No [filter] tasks found."
- "Want to create a new task?"
- "Try different filter?"

## Output Expectations

**Format:**
- Markdown formatted task list
- Clear section headers
- Consistent task entry format
- Summary statistics where relevant

**Success Criteria:**
- Correct tasks shown for filter applied
- Task information is current
- Easy to scan and understand
- Actionable information provided

**Failure Triggers:**
- Task index file not found
- Invalid filter specified
- Cannot parse task entries

## Quality Assurance

### Validation Steps

**Pre-Display Checks:**
1. Task index file exists and is readable
2. Filter parameter is valid
3. Task entries can be parsed

**Post-Display Checks:**
1. Task count matches filter criteria
2. All displayed tasks have required information
3. Dates are in correct format
4. No duplicate tasks shown

### Common Issues

**Issue:** Filter produces no results
**Solution:** Inform user, suggest alternative filters

**Issue:** Task file missing for enrichment
**Solution:** Use summary from index, note file missing

**Issue:** Invalid filter specified
**Solution:** Show valid filter options, use "all" as default

**Issue:** Task index is malformed
**Solution:** Parse what's possible, warn about format issues

## Reference Documentation

- Primary: `.github/instructions/memory-bank.instructions.md` (Task Management section)
- Related: `memory-bank/tasks/_index.md` (task index structure)
- Related: Individual task files in `memory-bank/tasks/`

## Example Usage

**User Command:**
```
show tasks
```

**Agent Response:**
```markdown
## All Tasks (8 total)

**In Progress (2)**
- [TASK003] Implement LoggingBehaviorTests - 40%
- [TASK005] Refactor DAO layer - 65%

**Pending (3)**
- [TASK006] Add validation tests
- [TASK007] Update documentation  
- [TASK008] Performance optimization

**Completed (2)**
- [TASK001] Fix AuditBehaviorTests - Completed 2025-01-19
- [TASK002] Update dependencies - Completed 2025-01-18

**Blocked (1)**
- [TASK004] Database migration - Waiting for credentials

**Task Summary:**
- 25% in progress
- 37.5% pending
- 25% completed
```

**User Command:**
```
show tasks active
```

**Agent Response:**
```markdown
## Active Tasks (2)

### TASK003: Implement LoggingBehaviorTests  
**Status:** In Progress - 40%  
**Updated:** 2025-01-19  
**Next Subtask:** 1.4 - Implement timing tests  
**Context:** Applying MediatR 14.0 testing patterns from TASK001

### TASK005: Refactor DAO layer  
**Status:** In Progress - 65%  
**Updated:** 2025-01-18  
**Next Subtask:** 2.7 - Update integration tests  
**Context:** Breaking change in Model_Dao_Result signature

**Suggested Actions:**
- Continue TASK003 subtask 1.4
- Review breaking changes in TASK005
```

**User Command:**
```
show tasks recent
```

**Agent Response:**
```markdown
## Recently Updated Tasks (4)

Updated in last 7 days:

1. **TASK003** - Implement LoggingBehaviorTests  
   Status: In Progress - 40%  
   Last Updated: 2025-01-19 (today)

2. **TASK001** - Fix AuditBehaviorTests  
   Status: Completed - 100%  
   Last Updated: 2025-01-19 (today)

3. **TASK005** - Refactor DAO layer  
   Status: In Progress - 65%  
   Last Updated: 2025-01-18 (1 day ago)

4. **TASK002** - Update dependencies  
   Status: Completed - 100%  
   Last Updated: 2025-01-18 (1 day ago)

**This Week's Progress:**
- 2 tasks completed
- 2 tasks advanced significantly
- 40% of active work updated today
```

**User Command:**
```
show tasks completed
```

**Agent Response:**
```markdown
## Completed Tasks (2)

- [TASK001] Fix AuditBehaviorTests - Completed on 2025-01-19  
  → Implemented 5 comprehensive tests, established MediatR 14.0 patterns

- [TASK002] Update dependencies - Completed on 2025-01-18  
  → Updated MediatR to 14.0.0, FluentAssertions to 8.8.0

**Recent Accomplishments:**
- 2 tasks completed this week
- 100% test coverage for AuditBehavior
- All dependencies current

**Success Rate:**
- 25% of total tasks completed
- Average time to completion: 2 days
```
