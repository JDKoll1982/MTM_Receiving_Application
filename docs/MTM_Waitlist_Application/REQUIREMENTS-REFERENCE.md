# MTM Waitlist Application - Requirements Reference

**Last Updated:** January 21, 2026  
**Status:** Requirements Gathering Phase

---

## Executive Summary

This document consolidates all known requirements and design decisions for the MTM Waitlist Application - a custom manufacturing operations management system being built to replace the current tablesready.com solution.

---

## Current State

### Existing System

- **Platform:** tablesready.com
- **Status:** Currently in use, to be replaced
- **Goal:** Build custom WinUI 3 application tailored to MTM's specific workflows

### Technology Stack (New System)

- **Framework:** WinUI 3 (Windows App SDK 1.6+)
- **Language:** C# 12
- **Platform:** .NET 8
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Primary Database:** MySQL 8.0 (READ/WRITE)
- **Integration Database:** SQL Server/Infor Visual (READ ONLY)
- **Testing:** xUnit with FluentAssertions

---

## Organizational Structure

### Departments

1. **Production**
   - Press Operators
   - Assembly (branch of Production)
   - General Production

2. **Material Handling**
   - Material Handlers
   - Material Handler Leads (Material Handler with added responsibilities)

3. **Die Shop**

4. **Fabrication & Welding**

5. **Quality Control**

6. **Setup Technicians**

7. **Inventory Specialists**
   - Responsible for maintaining inventory counts
   - Ensuring material is on the floor before production begins
   - Ordering dunnage when needed

8. **Outside Service Coordinator**
   - Coordinates parts sent to 3rd parties for work
   - Manages parts returning from outside services

9. **IT Department**

10. **Management**
    - Production Leads
    - Production Managers
    - Plant Managers

---

## Key Design Principles

### 1. Data Entry Minimization

- **Goal:** Operators click what they need without typing
- **Method:** Pull proprietary data from databases (part IDs, locations, descriptions)
- **Fallback:** Allow custom requests for items outside app's predefined options
- **Data Sources:** MySQL and Infor Visual databases

### 2. Shared Workstation Considerations

- Operator computers are ALL shared workstations
- Authentication must accommodate this (likely username/password vs Windows/AD)
- Session management critical for security on shared devices
- Auto-logout times must be carefully configured

### 3. Manufacturing Floor Environment

- **Plant Noise:** Very loud environment
- **Critical Constraint:** NO audio/sound notifications possible
- **Requirement:** ALL notifications must be visual (popups, badges, color changes, etc.)

### 4. Role-Based Workflows

- Material Handler Leads are Material Handlers with additional supervisory responsibilities
- Different roles need different screens and permissions
- Permission inheritance should be considered for Lead roles

---

## Feature Toggles & Optional Components

### 1. Zones (Optional - Initially OFF)

- Zone-based task assignment and filtering
- Zone-based permission scoping
- **Initial State:** Feature disabled
- **Future:** Can be enabled when ready

### 2. Auto-Assignment (Toggleable)

- System can automatically assign tasks to Material Handlers
- **Toggle Control:** Material Handler Lead, Production Manager, or Plant Manager
- **Alternative:** Manual assignment or self-claim queue
- **Important:** Even if management wants auto-assign, user input matters for feature design

### 3. Quick Add (Material Handler Proactive Work)

- Allows Material Handlers to log unscheduled work they performed
- Examples: Restocking noticed while passing, proactive deliveries
- **Requirement:** Must be controlled/monitored to prevent abuse (false entries)
- **Approval:** Likely requires Lead review/approval

---

## Critical Workflows

### Operator Request Submission

**Goal:** Minimize operator data entry

**Request Types Available to Operators:**
- Material delivery (coils, blanks, etc.)
- Dunnage delivery (containers, pallets, etc.)
- Quality inspection requests
- Setup technician help
- Maintenance/repair
- Tool/die issues
- Parts to Outside Service
- Custom/Other (fallback for anything not predefined)

**Data Auto-Population:**
- Part IDs from database
- Zone/location from database
- Work order association
- Standard descriptions/notes
- Operator adds quantity, priority, custom notes if needed

**Post-Submission Editing:**
- Need to decide if operators can edit/cancel after assignment
- Balance between flexibility and handler workflow stability

**Out-of-Area Requests:**
- Operators may need to request help for areas outside their normal workstation
- Need to determine how this workflow should function
- May require additional approvals or different routing

### Material Handler Task Execution

**Task Assignment Options:**
1. **Auto-Assignment** (toggleable by management)
   - Rules: Round-robin, least busy, zone-based, skill match, or combo
   - Override capability for Leads on critical tasks
2. **Manual Assignment** by Material Handler Lead or Production Lead
3. **Self-Claim** from queue

**Task Acceptance:**
- Handlers need ability to reject tasks with valid reasons:
  - Equipment unavailable/broken
  - Missing tools/equipment
  - Outside assigned area without permission
  - Incorrect/missing information
  - Safety concerns
- Rejected tasks go back to queue or escalate to Lead

**Task Execution:**
- Start timer when beginning task
- Complete with:
  - Auto-calculated time
  - Quantity delivered/processed
  - Notes (optional unless issues)
  - Photos (optional unless damage/NCM)
  - Location where delivered

**Communication Needs:**
- If handler needs more info, how to contact operator?
  - In-app note/question
  - Visual notification to operator
  - Lead escalation path

### Quality Inspection Workflow

**Inspection Requests:**
- Operator submits inspection request
- Specify: What needs inspection, why (first part, suspect, periodic, audit)
- Indicate: Can production continue while waiting?

**NCM (Non-Conforming Material) Process:**
- **Current Reality:** Parts marked as NCM at the press/area where they are
- **Not Currently:** "Quality marks parts as NCM" (but could be a feature if Quality wants)
- If Quality inspection fails, NCM flagging workflow TBD
- Material Handler may need to move NCM to designated area
- Tracking and traceability required

### Outside Service Coordination

**Operator Role:**
- Can operators directly request parts be sent to Outside Service?
- Or must this go through Outside Service Coordinator?
- Approval workflow TBD

**Material Handler Role:**
- Deliver parts to staging area
- Log the move in app
- Coordinator handles paperwork and 3rd party coordination

**Coordinator Role:**
- Sees parts ready for shipment
- Arranges pickup with 3rd party
- Tracks parts while out
- Logs return and routes back to production

### Inventory Specialist Workflow

**Pre-Production Material Availability:**
- Ensure material is on floor before job starts
- How do operators notify Inventory if material missing?
  - Waitlist request to Inventory Specialist?
  - Direct communication outside app?
  - Lead escalation?

**Dunnage Management:**
- Order dunnage when running low
- Material Handlers may Quick Add dunnage restocking
- Inventory tracks dunnage inventory levels

---

## User Interface Requirements

### Visual-Only Notifications

- **No audio notifications** (plant too loud)
- Use visual cues:
  - Color coding (green/yellow/red for status)
  - Badge counts on icons
  - Popup toasts (visual only)
  - Flashing/pulsing indicators for urgent items
  - Status bar updates

### Print-Friendly Forms

- All questionnaires must be printable HTML
- Export user answers to configurable destination
- Easy placeholder for answer export location (find/replace)

### Dashboard Requirements

**Operators:**
- Large "Create Request" button
- Active requests with status
- Recent/favorite requests for one-click resubmit
- Visual notifications

**Material Handlers:**
- My assigned tasks with timer
- Available tasks (if self-claim enabled)
- Quick Add button
- Zone/area selector (when zones enabled)

**Material Handler Leads:**
- Team task overview (all handlers in their area)
- Handler availability/status (active, on break, offline)
- Pending approvals (Quick Add, task rejections)
- Zone/area performance metrics
- Task reassignment tools

**Production Leads:**
- Cross-zone task visibility
- SLA breach alerts
- Handler utilization
- Priority escalation tools
- Bulk operations (reassign, cancel, priority change)

**Quality:**
- Pending inspections
- Inspection history
- NCM tracking
- Pass/fail rates

**Inventory Specialists:**
- Material availability alerts
- Pre-production material status
- Dunnage inventory levels
- Incoming material tracking

**Outside Service Coordinator:**
- Parts ready for shipment
- Parts at 3rd party (status tracking)
- Parts returning
- 3rd party performance metrics

**IT Department:**
- System health monitoring
- User management
- Role/permission administration
- Database connection status
- Error logs and diagnostics

---

## Data Integration

### Infor Visual (SQL Server - READ ONLY)

- Work order data
- Part master data
- BOM (Bill of Materials)
- Inventory levels
- Production schedules
- **Critical:** No writes allowed - read-only queries only
- **Offline Handling:** When Visual unavailable, allow requests with "Pending Validation" flag

### MySQL (Primary Database - READ/WRITE)

- All waitlist/task data
- User accounts and roles
- Request history
- Performance metrics
- Configuration settings
- Audit logs
- **Stored Procedures:** All MySQL access via stored procedures only (no raw SQL in C#)

---

## Security & Privacy

### Authentication

- Shared workstations require username/password (not Windows/AD)
- Auto-logout on inactivity (shorter timeout for shared kiosks)
- Session management for multiple device login

### Authorization

- Role-based permissions
- Permission inheritance (Leads inherit base role permissions?)
- Zone/area/shift scoping (when zones enabled)
- Audit logging for all permission changes

### Data Privacy

- Who can see individual performance metrics?
- Handlers see own stats only?
- Leads see team stats?
- Managers see aggregate data?

---

## Performance & Reporting

### Key Metrics (Top 5)

- Average wait time (request creation to handler acceptance)
- Average completion time (acceptance to done)
- SLA breaches (tasks exceeding target time)
- Current backlog (open task count)
- Handler utilization (% of shift time on tasks)

### Additional Metrics

- Cancellation rate
- Rework/error rate
- Zone load (when zones enabled)
- Task count by type
- Peak time analysis

### Reports

- Daily (to Leads)
- Weekly (to Production Manager)
- Monthly (to Plant Manager)
- Ad-hoc (permission-based)
- Format: PDF (read-only), Excel (analysis), CSV (data export)

### Alerts (Visual Only)

- Task waiting too long (threshold TBD)
- Backlog exceeding limit
- No handlers available
- SLA breach imminent
- Equipment/system issues

---

## Data Retention

### Active Data

- Keep in primary database: TBD (likely 90 days)
- Full performance and searchability

### Archived Data

- Searchable archive: TBD (likely 1-2 years)
- Read-only access
- Slower query performance acceptable

### Audit Logs

- Critical actions: Indefinite retention
- General actions: TBD (likely 1 year searchable, then archive)
- Login/logout: TBD

---

## Open Questions & Decisions Needed

### Authentication & Access

1. Exact auto-logout timeouts (kiosk vs office)
2. Multi-device login policy
3. Password complexity requirements
4. Permission inheritance model

### Task Assignment

1. Default assignment method (auto vs manual vs self-claim)
2. Auto-assignment algorithm (if enabled)
3. Task rejection rules and workflow
4. Cross-zone work authorization process

### Request Workflows

1. Can operators edit requests post-assignment?
2. Required vs optional fields for each request type
3. Priority escalation rules
4. Out-of-area request workflow

### Data & Reporting

1. Exact data retention periods
2. Archive searchability requirements
3. Who can see individual vs aggregate performance data
4. Standard report distribution lists

### Quality & NCM

1. NCM marking workflow (operator vs quality)
2. NCM movement responsibilities
3. Quality inspection blocking vs non-blocking
4. Inspection documentation requirements

### Integration

1. Infor Visual offline handling strategy
2. Data refresh frequency
3. Validation failure workflow

### Features & Scope

1. When to enable Zones feature
2. Auto-assignment default state
3. Quick Add approval workflow
4. Performance metric visibility rules

---

## Next Steps

1. **Distribute Questionnaires:**
   - Core (All Roles)
   - Operator
   - Material Handler
   - Material Handler Lead (Material Handler + additional questions)
   - Production Lead
   - Quality
   - Setup Technician
   - Inventory Specialist
   - Outside Service Coordinator
   - IT Department
   - Die Shop
   - Fabrication & Welding
   - Assembly

2. **Collect Responses:**
   - Set deadline for questionnaire completion
   - Assign point of contact for questions

3. **Review & Consolidate:**
   - Resolve conflicting answers
   - Identify gaps
   - Prioritize features for Phase 1

4. **Design & Planning:**
   - Create detailed specifications
   - Database schema design
   - UI mockups
   - Integration architecture

5. **Development:**
   - Follow MVVM patterns per project constitution
   - Build incrementally
   - Test with real users early and often

---

## AI Agent Notes

### Language & Tone

- Keep all user-facing documents simple and accessible
- Use manufacturing floor language, not technical jargon
- Examples should reflect real plant scenarios
- Allow users to suggest alternatives - these are ideas, not final decisions

### Question Framing

- Reference current state with tablesready.com
- Ask what works and what doesn't
- Avoid assuming concrete/permanent solutions
- Include "Other idea" options frequently
- Management decision notes where applicable

### Technical Constraints

- No audio notifications (plant noise)
- Shared workstations require special auth handling
- Zones initially disabled (future feature)
- Auto-assignment is toggleable
- All database access follows project architecture rules

### Export Functionality

- All questionnaires must export to configurable location
- Use consistent placeholder: `%%EXPORT_DESTINATION%%` for easy find/replace
- HTML format for print-friendliness
- Include JavaScript to collect and export answers

---

**Document Control:**
- **Created:** January 21, 2026
- **Purpose:** Reference document for AI agents and developers
- **Scope:** Complete requirements knowledge base as of this date
- **Update Frequency:** As new information is gathered

---

**End of Reference Document**
