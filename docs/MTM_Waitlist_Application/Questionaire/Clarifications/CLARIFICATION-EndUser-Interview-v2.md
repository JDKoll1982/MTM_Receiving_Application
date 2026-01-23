# MTM Waitlist Application – Stakeholder Decision Guide

**Date:** January 21, 2026  
**Version:** 1.0  
**Status:** Awaiting Stakeholder Input

---

## Document Overview

**Audience:** All stakeholders including Operators, Material Handlers, Production Leads, Plant Management, Quality Control, Setup Technicians, and IT/Admin staff.

**Purpose:** This document gathers all the open questions about the MTM Waitlist Application in one place for the meeting. Each question explains why it matters and gives a real example so everyone can make good decisions.

**Expected Outcome:** After this meeting, we should have answers to all the open questions so we can move forward with building the application.

---

## Instructions for Completion

1. **Answer in Plain Language:** Keep it simple. Explain it like you're talking to someone on the floor.
2. **Different by Role:** If the answer changes depending on the role (Operator, Lead, Manager), spell out each one.
3. **Don't Know:** If you're not sure, pick what seems best and say who needs to approve it.
4. **Who Decides:** For each answer, write down who made the call and when.

---

## Section 1: User Access & Authentication

### 1.1 Login Method

**Question:** How should users authenticate when logging into the application?

**Options:**

- Windows/Active Directory (automatic login using computer credentials)
- Username and password (manual entry each time)
- Hybrid approach (Windows where available, username/password as fallback)

> **Why This Matters:**  
> This choice affects setup cost and how much support is needed (like password resets). Windows/AD login means no passwords to manage, but it needs the right network setup.

> **Real-World Example:**  
> If floor workstations are shared kiosks without Active Directory access, manual username/password authentication will be required. Conversely, if supervisors work from domain-joined office computers, Windows authentication provides seamless access.

**Your Answer:**

---

### 1.2 Multiple Role Assignment

**Question:** Can a single user be assigned multiple roles simultaneously (e.g., both Operator and Production Lead, or Material Handler and Material Handler Lead)?

**Consider:** What combinations are common in your facility?

> **Why This Matters:**  
> If people can have multiple roles, it changes what screens they see and what they can do in the app.

> **Real-World Example:**  
> A Production Lead who also operates a press during off-peak hours may need both the Lead's oversight dashboard and the Operator's request submission interface. Similarly, a Material Handler Lead may need to both perform handler duties and supervise other handlers.

**Your Answer:**

---

### 1.3 Role Management Authority

**Question:** Who has authority to create, modify, or remove user roles? Are roles permanently defined or can they be edited over time?

**Consider:** Department managers, HR, IT, or a combination?

> **Why This Matters:**  
> Knowing who can change roles prevents people from getting access they shouldn't have and keeps a record of all changes.

> **Real-World Example:**  
> HR requests a temporary "Acting Lead" role for an employee covering a vacation. Who approves this change, how is it configured, and does it expire automatically?

**Your Answer:**

---

### 1.4 Permission Inheritance

**Question:** Should higher-level roles automatically include all permissions of lower-level roles?

**Example:** Should a Production Lead automatically have all Operator permissions, or must each be assigned separately?

> **Why This Matters:**  
> Having higher roles include lower role permissions makes setup easier, but you need to be careful not to give people access to things they shouldn't see.

> **Real-World Example:**  
> If Leads inherit Operator permissions, a single role assignment gives them both oversight and request-creation capabilities without maintaining two separate role assignments.

**Your Answer:**

---

### 1.5 Permission Scoping by Area/Shift

**Question:** Should permissions be restricted by zone, department, or shift? If yes, who configures these restrictions?

**Consider:** Zone-specific, shift-specific, or department-specific limitations

> **Why This Matters:**  
> Limiting permissions by zone/shift keeps people working in their assigned areas and makes it clear who's responsible for what.

> **Real-World Example:**  
> A Material Handler assigned to Zone 1 should not be able to claim or modify tasks designated for Zone 3 unless explicitly authorized for cross-zone work.

**Your Answer:**

---

### 1.6 Session Management Rules

**Question:** Define session timeout and multi-device rules:

- How long can a user remain idle before automatic logout?
- Can users be logged in on multiple devices simultaneously?
- Should shared kiosk terminals have different timeout rules than office workstations?

> **Why This Matters:**  
> Auto-logout times balance security (stopping someone from using an unattended computer) against not making people log in too often.

> **Real-World Example:**  
> A shared plant floor kiosk might auto-logout after 5 minutes of inactivity to prevent one user's session from being accessed by another. A supervisor's office computer might allow 30-minute idle timeout.

**Your Answer:**

---

## Section 2: Operator Request Submission

### 2.1 Request Type Restrictions

**Question:** Can operators submit requests for all categories, or should they be restricted to specific request types based on their role or area?

**Consider:** Material Handler requests, Quality inspections, Setup tasks, etc.

> **Why This Matters:**  
> Limiting who can submit what types of requests cuts down on wrong or useless requests, helping handlers get real work done faster.

> **Real-World Example:**  
> A press operator may be allowed to request material delivery and quality inspections, but not allowed to submit setup technician or maintenance requests which require different expertise.

**Your Answer:**

---

### 2.2 Required vs Optional Fields

**Question:** Which fields are mandatory to submit a request?

**Fields to Consider:**

- Work order number
- Part number
- Zone/location
- Priority level
- Description/notes

> **Why This Matters:**  
> Missing info means handlers have to hunt for details before they can start. But making too many fields required slows down creating requests.

> **Real-World Example:**  
> Without specifying a zone, a material handler must hunt for the operator's location. Without a work order, the request may not be tied to the correct job, affecting tracking and billing.

**Your Answer:**

---

### 2.3 Post-Assignment Editing

**Question:** After a request is assigned to a handler, can the operator still edit or cancel it? If yes, until what point?

**Options:**

- No changes after assignment
- Allow cancellation only (not editing)
- Allow changes until handler accepts/starts work
- Allow changes anytime with notification to handler

> **Why This Matters:**  
> Not allowing changes after assignment means handlers know the request won't change while they're working on it. Allowing changes gives flexibility for fixes but might mean redoing work.

> **Real-World Example:**  
> An operator submits a coil request, then realizes they entered the wrong part number after a handler has already accepted the task. Should they be able to update it, or must they cancel and resubmit?

**Your Answer:**

---

### 2.4 Favorites & Recent Requests

**Question:** Configure the favorites and recent requests feature:

- How many recent requests should be retained? (e.g., last 5, 10, 20)
- How long should recents be kept? (e.g., 7 days, 30 days)
- Can operators create shareable team templates that others can use?

> **Why This Matters:**  
> Favorites and recents make it faster to submit the same requests over and over. But too many items makes it hard to find what you need.

> **Real-World Example:**  
> Keep the last 10 requests for 7 days to help operators quickly resubmit common items (e.g., "Coils for Press 5"). Allow Lead-approved templates so the entire shift can access standardized requests.

**Your Answer:**

---

### 2.5 Work Order & Part Validation

**Question:** When work order or part number validation fails (or Infor Visual is unavailable), should the system:

- **Block submission** until validation passes?
- **Warn but allow** with a flag for manual review?
- **Accept and queue** for later validation when system recovers?

> **Why This Matters:**  
> Blocking bad entries keeps the data clean but could stop work if the validation system goes down. Allowing entries with a flag keeps things moving but might let bad data in.

> **Real-World Example:**  
> If Infor Visual is offline during a network issue, operators can still submit requests but they're marked "Pending Validation." When Visual comes back online, requests are validated automatically and flagged if invalid.

**Your Answer:**

---

## Section 3: Task Assignment & Handler Workflow

### 3.1 Assignment Authority

**Question:** Who can assign or reassign tasks?

**Options:**

- Automatic system assignment only
- Production Leads can manually assign/reassign
- Material Handlers can self-claim from a queue
- Combination of the above

> **Why This Matters:**  
> How tasks get assigned affects who's responsible, how fast work starts, and how the workload gets spread out. Auto-assign is fair; manual gives flexibility; self-claim lets handlers choose.

> **Real-World Example:**  
> System auto-assigns incoming requests during normal operations. Leads can manually reassign critical/urgent tasks to specific experienced handlers. Handlers can self-claim tasks when working in multiple zones.

**Your Answer:**

---

### 3.2 Auto-Assignment Strategy

**Question:** If using automatic assignment, which rule should take priority? List primary strategy and fallback options.

**Available Strategies:**

- **Round-robin:** Distribute evenly among all handlers
- **Least busy:** Assign to handler with fewest active tasks
- **Skill-based:** Match task type to handler qualifications
- **Zone proximity:** Assign to handler closest to request location
- **Priority override:** Critical tasks go to designated handlers

> **Why This Matters:**  
> The assignment rules directly affect how long requests wait, how busy handlers stay, and how fast tasks get done. Different rules work better for different goals.

> **Real-World Example:**  
> Primary: Assign to least busy handler in the correct zone. Fallback: If tie, choose handler closest to request location. Override: Production Lead can manually reassign critical tasks regardless of normal rules.

**Your Answer:**

---

### 3.3 Material Handler Lead Role

**Question:** Do you have Material Handler Leads as a distinct role? If yes, what are their responsibilities and permissions?

**Consider:**

- Can they reassign tasks among handlers?
- Do they approve handler time-off/breaks?
- Can they override auto-assignment?
- Do they handle escalations from handlers?
- Can they approve Quick Add entries?
- Do they have visibility into all handler performance metrics?

> **Why This Matters:**  
> Material Handler Leads supervise handlers on the floor and manage tasks as they happen. What they can do affects how flexible operations are and who's accountable for what.

> **Real-World Example:**  
> Material Handler Lead notices one handler is overwhelmed while another is idle. They can reassign tasks to balance workload. When a handler encounters an issue (wrong material location), they escalate to MH Lead who resolves it without involving Production Lead.

**Your Answer:**

---

### 3.4 Task Rejection by Handlers

**Question:** Can Material Handlers reject or return assigned tasks? Under what conditions?

**Consider:** Equipment unavailable, out of assigned zone, insufficient information, safety concern

> **Why This Matters:**  
> Letting handlers reject tasks keeps them from getting stuck on impossible jobs and shows when there's a bigger problem. But if there are too many rejections, it might mean the assignment rules are wrong or there aren't enough people.

> **Real-World Example:**  
> Handler can reject if: task is outside their zone without override approval, materials are unavailable, or equipment is inoperable. System logs rejection reason and reassigns or escalates to Lead.

**Your Answer:**

---

### 3.4 Zone Assignment Flexibility

**Question:** Configure zone assignment rules:

- Are handler-to-zone assignments permanent or temporary?
- Can handlers accept tasks outside their primary zone? Under what circumstances?
- How can a zone be marked as unavailable (e.g., aisle blocked, equipment down)?

> **Why This Matters:**  
> Zone flexibility affects response time and handler utilization. Rigid zones prevent cross-training opportunities but ensure accountability. Flexible zones improve coverage but may blur responsibility.

> **Real-World Example:**  
> Handlers have primary zones but can temporarily work other zones with Lead approval. If Zone 3 aisle is blocked by maintenance, Lead marks it unavailable, preventing new task assignments there until cleared.

**Your Answer:**

---

### 3.5 Quick Add for Unscheduled Work

**Question:** Should Material Handlers be allowed to log unscheduled work using "Quick Add"? If yes, define approval and reporting rules.

**Consider:**

- Who needs to approve Quick Add entries?
- Do Quick Add tasks appear in the main waitlist?
- Are they counted in performance metrics?

> **Why This Matters:**  
> Quick Add captures proactive work a Material Handler did that was not requested (e.g., picking up goods from a Press when the task to do so was not on the Waitlist). However, this must be controlled and/or monitored in order to prevent or address abuse such as false entries.

> **Real-World Example:**  
> Handler notices empty dunnage rack while delivering materials. Uses Quick Add to log "Dunnage Replenishment" task. Task requires Lead approval to count toward performance metrics and appears in reports flagged as proactive work.

**Your Answer:**

---

### 3.6 Task Completion Requirements

**Question:** Which fields are required when completing a task?

**Fields to Consider:**

- Time spent (auto-calculated or manual entry)
- Quantity delivered/processed
- Completion notes
- Issues encountered
- Photos of completed work

> **Why This Matters:**  
> Comprehensive completion data improves reporting, identifies recurring issues, and provides accountability. However, excessive data entry slows handlers and reduces productivity.

> **Real-World Example:**  
> Required: Time spent (auto-calculated from start/end), quantity. Optional: Notes, issues, photos. Photos become required if issue is selected or damage is reported.

**Your Answer:**

---

## Section 4: Role-Specific Workflows

### 4.1 Setup Technician Workflow

**Question:** Should Setup Technicians log root cause analysis and create follow-up maintenance tasks when diagnosing issues?

**Consider:**

- Logging diagnosis findings (e.g., "Die misaligned due to worn guide pins")
- Creating linked follow-up tasks for maintenance
- Updating original request with actual issue vs. reported issue

> **Why This Matters:**  
> Root cause logging prevents recurring problems and improves maintenance planning. Follow-up task creation ensures identified issues aren't forgotten.

> **Real-World Example:**  
> Operator reports "Die Stuck." Setup Tech diagnoses actual issue as "Die misaligned, worn guide pins." Tech logs root cause and creates follow-up task "Replace guide pins on Press 5" assigned to Maintenance.

**Your Answer:**

---

### 4.2 Quality Inspection Workflow

**Question:** Define the quality inspection workflow:

- Does an inspection request block production (operator must wait)?
- What must Quality record? (pass/fail, measurements, photos, defect codes)
- Who is responsible for moving Non-Conforming Material (NCM)?
- How is NCM flagged and tracked in the system?

> **Why This Matters:**  
> Clear inspection procedures prevent production delays, ensure traceability, and maintain quality standards. NCM handling affects material flow and compliance.

> **Real-World Example:**  
> Operator submits inspection request and continues working. Quality performs inspection and records: Pass/Fail, critical dimensions, photos if fail, defect code. If fail, system auto-creates NCM move task for Material Handler and flags parts with red visual indicator.

**Your Answer:**

---

### 4.3 Lead/Manager Intervention Powers

**Question:** What special actions can Production Leads, Material Handler Leads, and Plant Managers perform, and should these actions be specially logged?

**Possible Actions by Role:**

**Material Handler Leads:**
- Reassign tasks within their handler team?
- Change priority within their zone/area?
- Approve handler-submitted Quick Add entries?
- Override handler task rejections?

**Production Leads:**
- Manually reassign any task across all zones?
- Change priority of any existing task?
- Cancel tasks without handler approval?
- Create requests on behalf of operators?
- Bulk operations (cancel multiple, reassign all, etc.)?

**Plant Managers:**
- All above permissions?
- Additional strategic-level controls?

> **Why This Matters:**  
> Clear role hierarchy ensures appropriate intervention authority at each level. Material Handler Leads handle tactical floor issues; Production Leads manage cross-zone/strategic priorities; Plant Managers oversee exceptions. Audit trails prevent abuse and maintain accountability.

> **Real-World Example:**  
> Material Handler Lead reassigns tasks within Zone 1 handlers to balance workload. Production Lead escalates priority of safety-critical task affecting multiple zones. Plant Manager bulk-cancels all non-essential tasks during facility emergency. All interventions logged with timestamp, reason, and identity.

**Your Answer:**

---

## Section 5: User Interface & Experience

### 5.1 Navigation Pattern

**Question:** Which navigation style should the application use?

**Options:**

- **Sidebar menu:** Persistent navigation panel on left with all modules
- **Top tabs:** Horizontal tabs across top for switching views
- **Role-based:** Different layouts for different roles
- **Combination:** Sidebar for main modules, tabs within modules

> **Why This Matters:**  
> Navigation style affects learning curve, screen real estate usage, and user efficiency. Consistency reduces training time.

> **Real-World Example:**  
> Material Handlers get simple sidebar with "My Tasks," "Available Tasks," "Quick Add." Production Leads get tabs for "Dashboard," "Analytics," "Team Tasks," "Settings."

**Your Answer:**

---

### 5.2 Task List Display Format

**Question:** Configure task list display:

- Default view: List, Grid, or Card layout?
- Can users toggle between views?
- Which filters/sorts should be preset? (zone, status, priority, wait time)

> **Why This Matters:**  
> Display format affects information density and scanning speed. Lists show more items; cards show more detail per item.

> **Real-World Example:**  
> Default to list view for desktop users (shows 15-20 tasks at once). Allow card view toggle for mobile carts (larger touch targets). Preset filters: "My Zone," "High Priority," "Oldest First."

**Your Answer:**

---

### 5.3 List Pagination vs Infinite Scroll

**Question:** For long task lists, use pagination or infinite scroll?

**Options:**

- **Pagination:** Fixed page size (specify: 10, 25, 50, 100 items)
- **Infinite scroll:** Load more items automatically as user scrolls
- **Hybrid:** Load 50 items initially, then load more on scroll

> **Why This Matters:**  
> Pagination provides predictable performance on shared kiosks. Infinite scroll provides seamless browsing but may slow down with hundreds of items.

> **Real-World Example:**  
> Use pagination with 25 items per page for shared floor kiosks to maintain snappy performance. Office users get 50 items with load-more button.

**Your Answer:**

---

### 5.4 Autocomplete & Barcode Scanning

**Question:** Which fields should support autocomplete or barcode scanning?

**Fields to Consider:**

- Request categories
- Request types
- Zones
- Work order numbers
- Part numbers

> **Why This Matters:**  
> Autocomplete and scanning reduce typos, speed data entry, and improve data quality. However, each requires additional development and testing.

> **Real-World Example:**  
> Barcode scanning for work orders and part numbers (most error-prone). Autocomplete for zones (limited list, easy to type). Categories and request types use dropdowns (small, fixed lists).

**Your Answer:**

---

### 5.5 Role-Specific Dashboards

**Question:** Define key widgets/metrics for each role's dashboard:

**For Operators:**

- Recent requests submitted?
- Status of active requests?
- Favorite templates?
- Quick submit button?

**For Material Handlers:**

- My assigned tasks?
- Available tasks in my zones?
- Task timer for current work?
- Quick Add button?

**For Material Handler Leads:**

- Team task overview (all handlers in their area)?
- Handler availability/status (active, on break, offline)?
- Pending approvals (Quick Add, rejections)?
- Zone performance metrics (wait time, completion rate)?
- Task reassignment tools?

**For Production Leads:**

- Open task count by category?
- Average wait time?
- Handler availability?
- SLA breach alerts?

**For Quality:**

- Pending inspections?
- Failed inspection count?
- NCM summary?

> **Why This Matters:**  
> Role-optimized dashboards show only relevant information, reducing cognitive load and improving decision speed.

> **Real-World Example:**  
> Operator dashboard: Big "Create Request" button, "My Active Requests (3)" with status badges, "Recent Requests" for quick resubmit. Handler dashboard: "My Tasks (2)" with timer, "Available in Zone 1 (5)" sorted by wait time.

**Your Answer:**

---

## Section 6: Data Storage & History

### 6.1 Tracked Data Fields Confirmation

**Question:** Confirm the following data fields will be stored. Any objections or additions?

**Confirmed Fields:**

- Assignment source (auto-assigned vs. manual, and by whom)
- Timestamps: Created, Accepted, ETA, Completed
- Location: Zone → Workcenter (specific press/station)
- Attachments (photos, documents)
- Task dependencies (this task blocks/requires another)
- Recurring templates

> **Why This Matters:**  
> These fields enable accurate reporting, task routing, SLA tracking, and continuous improvement. They also support audit requirements.

> **Real-World Example:**  
> Photo attachment proves damaged part was received. Dependency prevents starting "Install new die" before "Remove old die" completes. ETA helps operators plan their work schedule.

**Your Answer:**

---

### 6.2 Zone Configuration

**Question:** Should zones be managed as a structured table with attributes, or remain simple text fields?

**If Structured Table:**

- Attributes: zone name, zone code, capacity, physical location, active/inactive flag
- Track history of zone changes (renaming, deactivation, reactivation)

> **Why This Matters:**  
> Structured zone data enables better reporting, capacity planning, and historical analysis. Simple text is easier initially but harder to maintain.

> **Real-World Example:**  
> Zone table shows "Zone 1" has capacity for 3 concurrent handlers, is located in "Building A - East Wing," currently active. History shows it was temporarily deactivated during floor renovation last month.

**Your Answer:**

---

### 6.3 Audit Trail Scope

**Question:** Define audit logging requirements:

- Which actions/field changes should be logged? (all changes, or only priority/assignment/status?)
- Store snapshots (entire record before/after) or field deltas (only changed fields)?
- Retention period? (90 days, 1 year, 2 years)
- Should audit logs be searchable/reportable by users?

> **Why This Matters:**  
> Comprehensive auditing supports investigations and compliance but increases storage costs. Searchability provides transparency but requires additional UI development.

> **Real-World Example:**  
> Log all changes to: assigned user, priority, status, cancellation. Track who made change and when. Keep searchable for 1 year for investigations. After 1 year, archive to read-only storage for 2 more years per compliance requirements.

**Your Answer:**

---

### 6.4 Time Estimate Complexity

**Question:** Should time estimates vary by conditions, or use a single average value?

**Options:**

- **Simple:** One estimate per request type (e.g., "Coil delivery = 15 minutes")
- **By Zone:** Different estimates per zone (Zone 1 farther away = 20 min, Zone 3 closer = 10 min)
- **By Role:** Different estimates by handler experience level
- **By Time of Day:** Night shift slower than day shift
- **Range:** Min/average/max instead of single number

> **Why This Matters:**  
> Dynamic estimates provide better ETAs and staffing predictions but add complexity to configuration and maintenance.

> **Real-World Example:**  
> Use zone-based estimates: Coil delivery to Zone 1 (far) = 20 min, Zone 3 (near warehouse) = 10 min. Track actual times to refine estimates quarterly.

**Your Answer:**

---

### 6.5 Bulk Operations

**Question:** Are bulk operations needed? If yes, which ones and who can execute them?

**Potential Bulk Actions:**

- Bulk cancel (e.g., cancel all low-priority tasks during emergency)
- Bulk reassign (e.g., reassign all from Handler A to Handler B during shift change)
- Bulk priority change (e.g., escalate all Zone 3 tasks due to urgent order)

> **Why This Matters:**  
> Bulk operations speed administration during exceptional situations but must be restricted to prevent accidental misuse.

> **Real-World Example:**  
> Storm warning received. Production Lead uses bulk cancel to clear non-critical tasks, freeing handlers for emergency prep work. Action requires confirmation dialog and logs all affected tasks.

**Your Answer:**

---

### 6.6 Data Retention & Archival

**Question:** Define data lifecycle:

- How long should active (in-progress or recent) tasks remain in primary database?
- How long should archived (completed/cancelled) tasks be retained?
- Must archived tasks be searchable, or archived to offline storage?
- Purge schedule for very old data?

> **Why This Matters:**  
> Data retention affects database performance, storage costs, and compliance requirements. Longer retention aids historical analysis but increases system complexity.

> **Real-World Example:**  
> Keep tasks in primary database: 90 days after completion. Archive completed tasks: 2 years in searchable archive for reporting. After 2 years, export to offline backup and purge from live system (if regulations allow).

**Your Answer:**

---

## Section 7: Analytics, Reporting & Alerts

### 7.1 Top Daily Metrics

**Question:** Select the top 5 metrics that matter most for daily operations:

**Available Metrics:**

- Average wait time (request creation to handler acceptance)
- Average completion time (handler acceptance to completion)
- SLA breaches (tasks exceeding target time)
- Current backlog (open task count)
- Handler utilization (% of shift time on tasks)
- Cancellation rate (% of requests cancelled)
- Rework/error rate (tasks rejected or redone)
- Zone load (task count by zone)

> **Why This Matters:**  
> Focusing on key metrics drives behavior and highlights problems. Too many metrics cause information overload.

> **Real-World Example:**  
> Top 5: (1) Average wait time, (2) SLA breaches, (3) Current backlog by category, (4) Handler utilization, (5) Cancellation rate. Display prominently on Lead dashboard.

**Your Answer:**

---

### 7.2 Performance Metrics Visibility

**Question:** Configure performance metric visibility:

- Should quality/rework metrics be calculated and displayed?
- Should handler performance comparisons be visible?
- If yes, who can see individual handler performance (only the handler, their lead, plant manager, everyone)?

> **Why This Matters:**  
> Performance metrics drive improvement but can create competitive or adversarial culture if not carefully managed. Transparency must be balanced with fairness.

> **Real-World Example:**  
> Track handler performance (average completion time, task count, quality ratings). Handlers see only their own stats. Leads see their team's individual stats for coaching. Plant Manager sees aggregate team comparisons, not individuals.

**Your Answer:**

---

### 7.3 Standard Reports

**Question:** Configure standard report schedule and delivery:

**Report Periods:** Daily, Weekly, Monthly, or custom?  
**Delivery Method:** In-app only, email, or both?  
**Format:** PDF (read-only), Excel (editable), CSV (data export), or multiple formats?

**Consider Reports For:**

- Daily: Task volume, wait times, SLA performance
- Weekly: Handler utilization, trend analysis
- Monthly: Strategic metrics, continuous improvement data

> **Why This Matters:**  
> Automated reports reduce ad-hoc requests and provide consistent data for decision-making. Format affects usability for different audiences.

> **Real-World Example:**  
> Daily: PDF summary emailed to Leads at 6 AM (previous day's stats). Weekly: Excel report emailed to Plant Manager on Monday morning (allows custom analysis). Monthly: PDF strategic report for management review meetings.

**Your Answer:**

---

### 7.4 Ad-Hoc Reporting Permissions

**Question:** Who can build, save, schedule, and share custom/ad-hoc reports?

**Permission Levels:**

- **View Only:** Can view shared reports only
- **Build:** Can create one-time queries for own use
- **Save:** Can save custom reports for reuse
- **Schedule:** Can schedule reports to run automatically
- **Share:** Can publish reports for others to view

> **Why This Matters:**  
> Unrestricted reporting allows heavy queries that slow the system. Restricted reporting prevents users from getting needed insights.

> **Real-World Example:**  
> Operators: View only. Material Handlers: View only. Leads: Build and save custom reports. Plant Manager: Full permissions including schedule and share.

**Your Answer:**

---

### 7.5 Chart Types & Interactivity

**Question:** Specify required chart types and interaction capabilities:

**Chart Types:** Bar, line, pie, heatmap, Gantt, other?  
**Interactivity:** Static images or clickable charts that filter/drill-down?  
**Preferred Charting Library:** If you have a preference (otherwise, let development choose best option)

> **Why This Matters:**  
> Interactive charts cost more to develop but provide richer analysis. Chart type affects clarity for different data patterns.

> **Real-World Example:**  
> Bar chart: task count by zone (click zone to see task details). Line chart: wait time trend over week (hover to see exact values). Heatmap: handler utilization by hour (identify peak/low times).

**Your Answer:**

---

### 7.6 Real-Time Data Updates

**Question:** Configure dashboard refresh behavior:

- Refresh rate for dashboards (every 10, 30, 60 seconds, or manual refresh only?)
- Use automatic refresh or push notifications for live updates?
- Can users pause auto-refresh during analysis?

> **Why This Matters:**  
> Frequent refresh provides current data but increases server load. Push updates are more efficient but require additional infrastructure.

> **Real-World Example:**  
> Dashboard auto-refreshes every 30 seconds for task counts and wait times. Users can click "Pause Updates" when reviewing data or taking screenshots. Critical alerts use push notifications regardless of refresh state.

**Your Answer:**

---

### 7.7 Alert Configuration

**Question:** Define alert conditions, channels, and recipients:

**Alert Conditions:** When should alerts fire?

- Task waiting more than X minutes (specify threshold)
- Backlog exceeds X tasks (specify threshold)
- No available handlers in a zone
- Handler utilization below X% (underutilized staff)
- SLA breach imminent or occurred

**Alert Channels:** How should users be notified?

- In-app toast/popup notification
- Email
- SMS/text message
- Desktop notification

**Recipients:** Who gets which alerts?

- All alerts to everyone?
- Role-based (Leads get operational alerts, Managers get strategic alerts)?
- Configurable per user?

**Threshold Configuration:** Can thresholds be adjusted, and by whom?

> **Why This Matters:**  
> Well-configured alerts prevent problems from escalating. Too many alerts cause "alert fatigue" and important notifications are ignored.

> **Real-World Example:**  
> Alert Production Lead (in-app toast + email) when: task waits 20+ minutes, backlog >15 tasks in their area. Alert Plant Manager (email only) when: SLA breach rate >10% for the day. Leads can adjust thresholds ±25%.

**Your Answer:**

---

### 7.8 Trends & Forecasting

**Question:** Are trend analysis and demand forecasting needed in the first release?

**Features:**

- **Trend Charts:** Historical patterns (wait times increasing, task volume by day of week)
- **Anomaly Detection:** Automatic alerts for unusual spikes or drops
- **Demand Forecasting:** Predict task volume based on historical patterns

> **Why This Matters:**  
> Trend analysis and forecasting support strategic planning but add significant development time. Consider deferring to later phase if basic reporting meets initial needs.

> **Real-World Example:**  
> Phase 1: Basic trend charts showing historical wait times and task volumes over time. Phase 2: Add anomaly detection (alert if Monday task volume 50% higher than normal). Phase 3: Add forecasting to predict staffing needs.

**Your Answer:**

---

## Section 8: Security, Privacy & Compliance

### 8.1 Password/PIN Policy

**Question:** If using username/password authentication (not pure Windows SSO), define password requirements:

- **Length:** Minimum characters (6, 8, 10, 12?)
- **Complexity:** Require letters + numbers? Require special characters?
- **History:** Prevent reusing last N passwords?
- **Expiration:** Passwords expire after X days?
- **Lockout:** Lock account after X failed attempts? For how long?
- **Reset:** Self-service reset, or require administrator/lead approval?

> **Why This Matters:**  
> Stronger requirements improve security but may lead to password resets, sticky notes, or help desk calls. Balance security with usability.

> **Real-World Example:**  
> Minimum 8 characters, letters + numbers required. No password expiration (NIST guidance). Lock after 5 failed attempts for 15 minutes. Self-service reset with security questions, or Lead can reset immediately.

**Your Answer:**

---

### 8.2 Data Privacy Controls

**Question:** Define privacy protections for sensitive information:

- Should task descriptions/notes be encrypted in the database?
- Should operator names be masked in certain views or exports?
- Should system log who viewed specific task details?

> **Why This Matters:**  
> Privacy protections guard against unauthorized access to sensitive information and support compliance with privacy regulations.

> **Real-World Example:**  
> Task descriptions not encrypted (no PII expected in normal use). Operator names visible to handlers and leads, but masked on manager-level reports (show count, not individuals). Log access to tasks marked "Quality Issue" or "Safety Concern."

**Your Answer:**

---

### 8.3 Deletion Policy

**Question:** Configure deletion capabilities and behavior:

**Deletion Type:**

- **Soft Delete:** Mark as deleted but retain in database for recovery
- **Hard Delete:** Permanently remove from database

**Who Can Delete:** What roles can delete tasks? What about users?

**History Preservation:** If a task is deleted, what happens to its history and related records?

> **Why This Matters:**  
> Soft delete supports audit requirements and mistake recovery. Hard delete reduces database bloat but eliminates ability to restore or investigate.

> **Real-World Example:**  
> Soft delete only for tasks and users. Deleted items hidden from normal views but remain in database and audit logs. Only IT Administrators can hard-delete after 2-year retention period. Task history preserved even if task deleted.

**Your Answer:**

---

### 8.4 Session & Privilege Audit Logging

**Question:** Should the system log user session and privilege changes?

**Log Events:**

- User login/logout (timestamp, workstation)
- Role/privilege changes (what changed, who authorized, when)
- Permission-level actions (who viewed what restricted data)

> **Why This Matters:**  
> Session and privilege logging provides accountability for security investigations and compliance audits.

> **Real-World Example:**  
> Log all logins (user, timestamp, workstation IP) for 90 days. Log privilege changes (who granted Acting Lead role to whom, when, duration) for 1 year. Log access to restricted reports or bulk operations indefinitely.

**Your Answer:**

---

## Section 9: Integration with Infor Visual

### 9.1 Validation Behavior During Outages

**Question:** When Infor Visual is slow or unavailable, how should work order and part number validation behave?

**Options:**

- **Block:** Prevent request submission until validation succeeds
- **Warn:** Show warning but allow submission with flag
- **Queue:** Accept request and validate asynchronously when Visual recovers

> **Why This Matters:**  
> Blocking ensures data quality but halts operations during outages. Allowing submissions maintains workflow but risks invalid data entering the system.

> **Real-World Example:**  
> Default: Warn and allow. Show red flag icon on request marked "Pending Validation." Once Visual is available, system validates queued requests. If invalid, assign to Lead for review and correction before handler dispatch.

**Your Answer:**

---

### 9.2 Reference Data Refresh Strategy

**Question:** How often should work order and part number data be refreshed from Infor Visual?

**Options:**

- **Real-time query:** Look up in Visual every time user searches (slowest, most current)
- **Cached with nightly refresh:** Copy data to local database each night (fast, 24hr lag)
- **Cached with on-demand refresh:** Daily cache + manual refresh button for urgent updates
- **Change notifications:** Visual notifies this app when critical data changes

> **Why This Matters:**  
> Real-time queries ensure freshness but impact performance. Caching provides speed but data may be stale. Notification-based updates balance both but require integration work.

> **Real-World Example:**  
> Nightly batch: Copy active work orders and parts to local cache at 2 AM. Provides fast autocomplete during day. Leads can trigger manual refresh if urgent new work order issued. System auto-retries if nightly batch fails.

**Your Answer:**

---

## Section 10: Performance & Scalability

### 10.1 Expected Volume & Concurrency

**Question:** Provide estimates for system sizing:

- **Daily task volume:** Approximately how many tasks per day? (100, 500, 1000, 5000?)
- **Peak concurrent users:** Maximum simultaneous users during busiest shift? (10, 25, 50, 100?)
- **Peak task creation rate:** Maximum tasks created in one hour? (e.g., shift start surge)

> **Why This Matters:**  
> Volume estimates drive infrastructure sizing, database design, and performance testing targets. Underestimating leads to slow performance; overestimating wastes hosting costs.

> **Real-World Example:**  
> Estimated 500 tasks/day across 3 shifts. Peak of 60 simultaneous users at shift change (7 AM, 3 PM, 11 PM). Task creation surge of 150 tasks/hour at day shift start.

**Your Answer:**

---

### 10.2 Performance Targets

**Question:** Define acceptable response times:

- **Task list load time:** Maximum acceptable time to load task list (1, 2, 5 seconds?)
- **Dashboard refresh:** How fast should dashboard update? (< 1 second, < 2 seconds?)
- **Search results:** Acceptable time to return search results? (1, 3, 5 seconds?)
- **Report generation:** Acceptable wait time for standard reports? (5, 10, 30 seconds?)

> **Why This Matters:**  
> Performance targets drive technical architecture decisions and testing criteria. Overly aggressive targets increase cost; too lenient targets frustrate users.

> **Real-World Example:**  
> Task list with 25 items: 2 seconds maximum. Dashboard refresh: 1 second. Search with filters: 3 seconds. Standard daily report: 10 seconds. Monthly report with charts: 30 seconds.

**Your Answer:**

---

## Decision Summary Table

After completing all sections, summarize key decisions here for quick reference:

| **Category** | **Decision** | **Decided By** | **Date** |
|--------------|--------------|----------------|----------|
| Authentication Method | | | |
| Multi-Role Users | | | |
| Permission Inheritance | | | |
| Session Timeout (Kiosk) | | | |
| Session Timeout (Office) | | | |
| Request Type Restrictions | | | |
| Required Fields | | | |
| Post-Assignment Editing | | | |
| Auto-Assignment Strategy | | | |
| Task Rejection Allowed | | | |
| Zone Flexibility | | | |
| Quick Add Enabled | | | |
| Completion Required Fields | | | |
| Setup Tech Root Cause Logging | | | |
| Quality Inspection Blocking | | | |
| Lead Intervention Powers | | | |
| Navigation Style | | | |
| Task List Default View | | | |
| Pagination Size | | | |
| Barcode Scanning Fields | | | |
| Top 5 Daily Metrics | | | |
| Performance Visibility | | | |
| Standard Report Schedule | | | |
| Ad-Hoc Report Permissions | | | |
| Chart Interactivity | | | |
| Dashboard Refresh Rate | | | |
| Alert Conditions | | | |
| Trend/Forecast in Phase 1 | | | |
| Password Policy | | | |
| Data Privacy Controls | | | |
| Deletion Policy | | | |
| Audit Logging Scope | | | |
| Visual Outage Behavior | | | |
| Data Refresh Strategy | | | |
| Expected Daily Volume | | | |
| Performance Targets | | | |

---

## Next Steps

1. **Schedule Review Meeting:** Coordinate with all stakeholders to review this document together.
2. **Complete Answers:** Work through each section, documenting decisions and rationale.
3. **Identify Gaps:** Note any questions that require additional research or discussion.
4. **Assign Follow-Up:** For unresolved items, assign ownership and target resolution dates.
5. **Distribute Final Decisions:** Share completed decision guide with development team to begin implementation planning.

---

**Document Control:**

- **Created:** January 21, 2026
- **Last Modified:** [Date of completion]
- **Approved By:** [Name, Title]
- **Distribution:** All Stakeholders, Development Team, Project Management

---

**End of Document**
