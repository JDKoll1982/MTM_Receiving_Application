# Clarification Questions - Workflow & User Experience

**Date**: January 21, 2026  
**Category**: User Experience & Business Logic  
**Priority**: High

---

## Overview

This document contains questions requiring clarification about workflow behavior, user interactions, and business rules for the MTM Waitlist Application.

---

## 1. Operator Workflows

### 1.1 Task Creation Wizard

**Question**: What is the exact step sequence for creating a task request?

**Current Understanding** (from mockups):
1. Category Selection
2. Request Type Selection  
3. Work Order Entry
4. Part Number Entry
5. Details Entry
6. Review

**Need Clarification**:
- [ ] Can steps be **skipped** based on request type (e.g., no work order needed)?
- [ ] Can users **jump to any step** or must they proceed sequentially?
- [ ] Can users **save drafts** and resume later?
- [ ] Should there be **validation at each step** or only at final submit?
- [ ] What happens if user **navigates away** mid-wizard?

**Proposed Conditional Workflow**:
```
IF request_type.requires_work_order = TRUE
    THEN show Work Order Entry step
    ELSE skip to next step

IF request_type.requires_part_number = TRUE
    THEN show Part Number Entry step
    ELSE skip to next step

IF request_type.requires_zone = TRUE
    THEN show Zone Selection step
    ELSE skip to next step
```

**Impact**: Workflow service complexity, UI navigation logic

---

### 1.2 Favorites & Recents

**Question**: How should favorites and recents work?

**Current Understanding**:
- Operators can mark frequent requests as favorites
- Recent requests are shown for quick re-creation

**Need Clarification**:
- [ ] What makes a request a **"favorite"** (manual flag vs frequency-based)?
- [ ] How many **recent requests** should be shown? (5? 10? 20?)
- [ ] How long should **recents be retained**? (24 hours? 7 days? forever?)
- [ ] Should favorites have **custom names** ("My usual coil request")?
- [ ] Can favorites be **shared** among operators (team templates)?
- [ ] Should clicking a recent/favorite **pre-fill wizard** or **auto-submit**?

**Proposed Favorites Table**:
```sql
CREATE TABLE operator_favorites (
    favorite_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    favorite_name VARCHAR(100),  -- Custom name
    category_id INT NOT NULL,
    request_type_id INT NOT NULL,
    work_order_number VARCHAR(50),  -- Template value
    part_number VARCHAR(50),
    description TEXT,
    zone VARCHAR(20),
    priority VARCHAR(20),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    use_count INT DEFAULT 0,  -- Track usage
    last_used_at DATETIME,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
```

**Impact**: Database schema, UI design, usage tracking

---

### 1.3 Work Order Validation

**Question**: What level of work order validation is required?

**Need Clarification**:
- [ ] Should work order be **validated against Infor Visual** before submit?
- [ ] What if work order **doesn't exist** in Infor Visual (block vs warn)?
- [ ] Should we **pull part number from work order** automatically?
- [ ] Should we **validate work order status** (active, completed, closed)?
- [ ] What if **Infor Visual is unavailable** (allow bypass? queue for validation)?

**Proposed Validation Levels**:

**Level 1: Format Validation**
- Check work order matches expected format (e.g., "WO12345")

**Level 2: Existence Check**
- Query Infor Visual to confirm work order exists

**Level 3: Status Validation**
- Confirm work order is active/open (not completed)

**Level 4: Context Validation**
- Confirm work order is for the correct part number
- Confirm work order is assigned to the correct machine/area

**Impact**: Integration complexity, user experience, data quality

---

### 1.4 Operator Multi-Request Submission

**Question**: Can operators create multiple requests at once?

**Need Clarification**:
- [ ] Can operator **submit multiple requests** in one session?
- [ ] Should there be a **batch entry mode** (enter 5 coil requests at once)?
- [ ] Should requests be **linked** if created together?
- [ ] Can operator **duplicate** a recent request multiple times?

**Impact**: UI workflow, database transactions

---

## 2. Material Handler Workflows

### 2.1 Task Claiming

**Question**: How do material handlers claim tasks?

**Current Understanding**:
- Tasks are auto-assigned or manually assigned

**Need Clarification**:
- [ ] Can material handlers **self-assign** from queue?
- [ ] Can material handlers **reject** auto-assigned tasks?
- [ ] Can material handlers **swap** tasks with each other?
- [ ] Can material handlers **put tasks on hold** (bathroom break)?
- [ ] What happens if handler **doesn't start** assigned task in X minutes?

**Proposed Task States**:
```
Pending → Assigned → InProgress → Completed
         ↓                ↓
      Rejected        OnHold
```

**Impact**: Task lifecycle management, reassignment logic

---

### 2.2 Zone-Based Routing

**Question**: How does zone-based task assignment work?

**Need Clarification**:
- [ ] Are material handlers **permanently assigned** to zones?
- [ ] Can material handlers **temporarily work** in other zones?
- [ ] Should tasks be **routed to nearest available** handler?
- [ ] What if a zone has **no available handlers**?
- [ ] Can handlers **mark zones as unavailable** (blocked aisle)?

**Proposed Zone Assignment**:
```sql
CREATE TABLE material_handler_zones (
    assignment_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    zone VARCHAR(20) NOT NULL,
    is_primary_zone TINYINT(1) DEFAULT 0,
    effective_from DATETIME DEFAULT CURRENT_TIMESTAMP,
    effective_to DATETIME,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
```

**Impact**: Assignment algorithm, handler workload balancing

---

### 2.3 Quick Add for Non-Waitlist Tasks

**Question**: How should "Quick Add" work for material handlers?

**Current Understanding** (from mockups):
- Material handlers can log tasks that weren't requested (proactive)

**Need Clarification**:
- [ ] What is the **purpose** of Quick Add (credit tracking? reporting?)?
- [ ] Should Quick Add tasks be **shown in waitlist**?
- [ ] Should Quick Add tasks have **same fields** as regular requests?
- [ ] Can Quick Add tasks be **retroactive** (log completed work)?
- [ ] Should Quick Add require **supervisor approval**?

**Example Use Cases**:
```
Use Case 1: Noticed empty coil rack while passing by
- Handler adds "Coil replenishment" task
- Marks as completed immediately
- Gets credit for proactive work

Use Case 2: Found spilled parts
- Handler adds "Scrap cleanup" task
- Marks as in-progress
- Completes when done
```

**Impact**: Task workflow, performance tracking, credit system

---

### 2.4 Task Completion & Notes

**Question**: What information is required at task completion?

**Need Clarification**:
- [ ] Are **completion notes required** or optional?
- [ ] Should handler **confirm part number/quantity** delivered?
- [ ] Should handler **rate task difficulty**?
- [ ] Should handler **report issues encountered**?
- [ ] Can handler **attach photos** of completed work?

**Proposed Completion Form**:
```
Required:
- Status: Completed/Cancelled
- Actual time spent (auto-calculated from start/end)

Optional:
- Completion notes (free text)
- Quantity delivered (if applicable)
- Issues encountered (dropdown + notes)
- Photo attachments
```

**Impact**: Data collection, reporting quality, storage

---

## 3. Setup Technician Workflows

### 3.1 Die Issue Diagnosis

**Question**: Should setup technicians diagnose issues in the app?

**Need Clarification**:
- [ ] Should techs **update task details** after inspection?
- [ ] Should techs **escalate** to maintenance if needed?
- [ ] Should techs **log root cause** of issue?
- [ ] Should techs **create follow-up tasks**?

**Example Workflow**:
```
1. Operator reports "Die Stuck"
2. Setup Tech receives task
3. Tech inspects and determines actual issue:
   - Not stuck, just misaligned
   - Update task: "Die Misalignment"
   - Log root cause: "Worn guide pins"
   - Create maintenance request: "Replace guide pins"
4. Tech completes alignment task
5. Maintenance gets follow-up task
```

**Impact**: Task lifecycle, integration with maintenance system

---

### 3.2 Setup Tech Priorities

**Question**: How should setup technicians prioritize tasks?

**Need Clarification**:
- [ ] Should **machine downtime** auto-escalate priority?
- [ ] Should **production schedule** influence priority?
- [ ] Can setup techs **re-prioritize** their queue?
- [ ] Should safety issues **jump to front of queue**?

**Impact**: Prioritization algorithm, integration with production system

---

## 4. Quality Workflows

### 4.1 Inspection Requests

**Question**: What happens after quality creates an inspection request?

**Need Clarification**:
- [ ] Does inspection task **block production** (operator waits)?
- [ ] Should quality provide **immediate feedback** (pass/fail)?
- [ ] Should quality **log measurements/results**?
- [ ] Can quality **reject parts** and create NCM?
- [ ] Should quality results be **sent to operator**?

**Proposed Inspection Workflow**:
```
1. Operator creates "Inspection Request"
2. Operator marks parts for inspection (physical tag)
3. Quality receives task in their queue
4. Quality performs inspection
5. Quality logs results in app:
   - Pass/Fail
   - Measurements
   - Photos
   - Defect codes (if fail)
6. IF fail:
   - Auto-create NCM task for material handler
   - Notify operator (parts rejected)
7. Quality completes inspection task
```

**Impact**: Integration with quality module, operator notifications

---

### 4.2 NCM Handling

**Question**: How should Non-Conforming Material be handled in waitlist?

**Need Clarification**:
- [ ] Should NCM be a **separate task category** or **linked to inspection**?
- [ ] Who is responsible for **moving NCM** (quality? material handler?)?
- [ ] Should NCM tasks have **special visual indicators** (red flag)?
- [ ] Can NCM tasks be **held** pending disposition decision?
- [ ] Should NCM tasks auto-notify **production lead**?

**Impact**: Task categorization, notification system

---

## 5. Production Lead Workflows

### 5.1 Analytics Dashboard

**Question**: What analytics should production leads see?

**Current Understanding** (from mockups):
- Analytics access is restricted to leads

**Need Clarification**:
- [ ] What **time range** for analytics (today? this week? custom)?
- [ ] What **metrics** are most important:
  - Average wait time by category?
  - Task completion rate?
  - Handler performance?
  - Bottleneck identification?
- [ ] Should analytics be **real-time** or **periodic refresh**?
- [ ] Can leads **export analytics** to Excel/PDF?
- [ ] Should leads see **predictive metrics** (estimated completion times)?

**Proposed Metrics Dashboard**:
```
Real-Time Metrics:
- Active tasks count by category
- Average wait time (overall and by category)
- Tasks completed today
- Tasks created today
- Open backlog

Historical Metrics:
- Trend charts (tasks over time)
- Handler performance rankings
- Zone utilization heatmap
- Time standard accuracy (estimated vs actual)

Alerts:
- Tasks exceeding SLA
- Zones with no available handlers
- Unusual spike in certain request types
```

**Impact**: Database queries, reporting complexity, refresh frequency

---

### 5.2 Lead Intervention

**Question**: What actions can production leads take on tasks?

**Need Clarification**:
- [ ] Can leads **manually reassign** any task?
- [ ] Can leads **change priority** of existing tasks?
- [ ] Can leads **cancel** tasks if no longer needed?
- [ ] Can leads **create tasks** on behalf of operators?
- [ ] Can leads **edit task details** after creation?
- [ ] Should lead interventions be **logged** differently?

**Impact**: Privilege system, audit trail

---

## 6. Shared Workflows

### 6.1 Task Status Updates

**Question**: How should task status changes be communicated?

**Need Clarification**:
- [ ] Should **operator be notified** when task is assigned?
- [ ] Should **operator be notified** when task is completed?
- [ ] Should **operator be notified** if task is cancelled?
- [ ] What **notification methods** (in-app? email? SMS?)?
- [ ] Can users **customize notification preferences**?

**Proposed Notification Events**:
```
Operator:
- Task assigned to handler (in-app toast)
- Task completed (in-app + email)
- Task cancelled (in-app + email)
- Task taking longer than expected (in-app)

Handler:
- Task auto-assigned to you (in-app)
- Task escalated to high priority (in-app + sound alert)
- New task in your zone (optional in-app)

Lead:
- Task exceeding SLA (in-app + email)
- Multiple tasks in backlog (email summary)
- Handler marked as unavailable (in-app)
```

**Impact**: Notification service design, user preferences

---

### 6.2 Task Search & Filtering

**Question**: What search capabilities should be available?

**Need Clarification**:
- [ ] Should users be able to **search by work order**?
- [ ] Should users be able to **search by part number**?
- [ ] Should users be able to **filter by date range**?
- [ ] Should users be able to **filter by multiple criteria simultaneously**?
- [ ] Should search be **real-time** (as-you-type) or **explicit** (click Search)?
- [ ] Should **saved searches** be supported?

**Proposed Search/Filter UI**:
```
Quick Filters (buttons):
- My Tasks
- Active
- Completed Today
- Overdue

Advanced Filters (expandable panel):
- Category: [multi-select dropdown]
- Zone: [multi-select dropdown]
- Priority: [multi-select dropdown]
- Status: [multi-select dropdown]
- Date Range: [from] to [to]
- Work Order: [text input]
- Part Number: [text input]

Sort By:
- Created Date (newest/oldest)
- Wait Time (longest/shortest)
- Priority (highest/lowest)
```

**Impact**: Query performance, UI complexity

---

### 6.3 Task History & Audit

**Question**: What historical information should be visible to users?

**Need Clarification**:
- [ ] Can users see **all changes** to a task (full audit trail)?
- [ ] Should users see **who made each change**?
- [ ] Should users see **task lifecycle timeline** (visual)?
- [ ] Can users **filter history** (show only status changes)?
- [ ] Should history be **exportable**?

**Proposed Task Detail View**:
```
Task Information:
- Current status
- Current assignee
- Work order, part number
- Description

Timeline (visual):
- Created by [User] at [Time]
- Assigned to [User] at [Time]
- Started by [User] at [Time]
- Priority changed from Normal to Urgent by [Lead] at [Time]
- Completed by [User] at [Time]

Change Log (table):
| Field | Old Value | New Value | Changed By | Changed At |
|-------|-----------|-----------|------------|------------|
| Priority | Normal | Urgent | Lead1 | 10:30 AM |
| Assigned To | NULL | Handler5 | System | 10:31 AM |
```

**Impact**: Database queries, UI design, data privacy

---

## 7. Error Handling & Edge Cases

### 7.1 Duplicate Requests

**Question**: How should duplicate requests be handled?

**Need Clarification**:
- [ ] Should system **detect potential duplicates** (same work order + category)?
- [ ] Should system **warn operator** before creating duplicate?
- [ ] Should system **block duplicate** or allow with confirmation?
- [ ] Should system **auto-link related requests**?

**Example Scenario**:
```
Operator1 creates: "Coils needed for WO12345"
5 minutes later, Operator2 creates: "Coils needed for WO12345"

Question: Is this duplicate or legitimate (need more coils)?
How do we detect and handle?
```

**Impact**: Validation logic, database queries

---

### 7.2 Task Cancellation

**Question**: Who can cancel tasks and under what conditions?

**Need Clarification**:
- [ ] Can **operators cancel** their own requests (before assigned)?
- [ ] Can **operators cancel** their requests (after assigned)?
- [ ] Can **handlers cancel** tasks assigned to them?
- [ ] Can **leads cancel** any task?
- [ ] Should cancelled tasks be **hidden** or **shown with status**?
- [ ] Should cancellation **require a reason**?

**Proposed Cancellation Rules**:
```
Operator can cancel:
- Own request if status = Pending

Handler can cancel:
- Assigned task with reason "Unable to complete"
- Sends task back to Pending for reassignment

Lead can cancel:
- Any task with reason
- Task disappears from queues but remains in history
```

**Impact**: Business rules, UI availability

---

### 7.3 Partial Completion

**Question**: Can tasks be partially completed?

**Need Clarification**:
- [ ] If operator requests 5 coils, can handler deliver **3 now, 2 later**?
- [ ] Should **partial completion** split into two tasks?
- [ ] Should partial completion be a **separate status**?
- [ ] How does partial completion affect **time tracking**?

**Impact**: Task lifecycle, reporting accuracy

---

### 7.4 System Downtime

**Question**: What happens if the app is unavailable?

**Need Clarification**:
- [ ] Should there be a **fallback manual process** (paper forms)?
- [ ] Can tasks be **created retroactively** after system comes back up?
- [ ] Should app **queue actions offline** and sync when connected?
- [ ] Who is responsible for **entering offline tasks** into system?

**Impact**: Offline support, data entry burden

---

## 8. Integration Points

### 8.1 Infor Visual Integration

**Question**: What data flows between Waitlist App and Infor Visual?

**Need Clarification**:
- [ ] **Read from Infor Visual**:
  - Work order details (part number, quantity, due date)?
  - Part master data (description, unit of measure)?
  - Production schedule?
- [ ] **Write to Infor Visual**:
  - Task completion (update work order status)?
  - Material movement (update inventory)?
  - Time tracking (labor hours)?
- [ ] **Sync frequency**:
  - Real-time?
  - Batch (hourly/daily)?
  - On-demand?

**Impact**: Integration architecture, data consistency

---

### 8.2 Existing "Tables Ready" System

**Question**: How do we migrate from existing system?

**Need Clarification**:
- [ ] Will there be a **parallel operation period** (both systems running)?
- [ ] Should we **import historical data** from old system?
- [ ] What is the **cutover date**?
- [ ] Who **trains users** on new system?
- [ ] What is the **rollback plan** if adoption fails?

**Impact**: Migration strategy, training plan

---

## 9. Performance & Scalability

### 9.1 Concurrent Users

**Question**: What is the expected user load?

**Need Clarification**:
- [ ] How many **concurrent users** at peak?
- [ ] How many **requests per hour** at peak?
- [ ] How many **total users** in system?
- [ ] Are there **usage spikes** (shift changes, breaks)?

**Impact**: Infrastructure sizing, performance optimization

---

### 9.2 Data Volume

**Question**: What is the expected data growth?

**Need Clarification**:
- [ ] How many **tasks per day**?
- [ ] How long should **active tasks** be retained?
- [ ] When should tasks be **archived**?
- [ ] What is the **retention policy** for archived tasks?

**Impact**: Database sizing, archival strategy

---

## 10. Reporting Requirements

### 10.1 Standard Reports

**Question**: What built-in reports are needed?

**Need Clarification**:
- [ ] **Daily Summary**: Tasks created, completed, pending
- [ ] **Handler Performance**: Tasks per handler, avg time, quality
- [ ] **Zone Utilization**: Tasks per zone, wait times
- [ ] **Time Standard Accuracy**: Estimated vs actual times
- [ ] **Category Breakdown**: Distribution by request type
- [ ] **SLA Compliance**: Percentage meeting target times

**Impact**: Reporting infrastructure, database queries

---

### 10.2 Custom Reports

**Question**: Should users be able to create custom reports?

**Need Clarification**:
- [ ] Can users **select fields** to include?
- [ ] Can users **apply filters** and save?
- [ ] Can users **schedule reports** (daily email)?
- [ ] What **export formats** (PDF, Excel, CSV)?

**Impact**: Reporting UI complexity, export services

---

## Action Items

### Critical (Before Design Finalization)
1. [ ] Define exact wizard steps and conditional flow
2. [ ] Clarify work order validation requirements
3. [ ] Specify zone-based routing logic
4. [ ] Determine task state machine (pending → assigned → completed)

### High Priority (Before Development)
5. [ ] Define notification events and methods
6. [ ] Specify search/filter capabilities
7. [ ] Clarify analytics dashboard metrics
8. [ ] Define task cancellation rules

### Medium Priority (Before Beta)
9. [ ] Favorites and recents functionality details
10. [ ] Quick Add workflow for material handlers
11. [ ] Inspection workflow integration
12. [ ] Lead intervention capabilities

### Low Priority (Before Production)
13. [ ] Partial completion handling
14. [ ] Custom reporting requirements
15. [ ] Offline support strategy
16. [ ] Migration from old system

---

## Next Steps

1. **User Story Workshops**: Conduct detailed workflow sessions with operators, handlers, leads
2. **Prototype Review**: Show mockups and gather feedback on workflows
3. **Document Decisions**: Update this file with finalized workflows
4. **Create Flow Diagrams**: Visual workflow documentation
5. **Write Acceptance Criteria**: Specific test scenarios for each workflow
6. **Implement Workflows**: Code workflow services and ViewModels

---

**Document Owner**: UX Designer / Business Analyst  
**Review Date**: [To Be Scheduled]  
**Status**: Pending Stakeholder Input
