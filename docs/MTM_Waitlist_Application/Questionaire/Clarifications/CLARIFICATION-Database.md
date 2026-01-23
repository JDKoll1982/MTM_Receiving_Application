# Clarification Questions - Database Schema & Design

**Date**: January 21, 2026  
**Category**: Database Architecture  
**Priority**: High

---

## Overview

This document contains questions requiring clarification about the database schema, stored procedures, and data access patterns for the MTM Waitlist Application.

---

## Decisions to Date

- Track assignment source on each task (auto-assigned vs manual) and who performed manual assignment.
- Capture both estimated completion time and actuals with Created, Accepted, Completed, and ETA timestamps.
- Store location details as Zone → Workcenter (MTM press/workstation names, distinct from computer names).
- Allow task attachments (photos/documents) for richer context.
- Prepare for task dependencies even if initially unused.
- Support recurring task templates to reduce request friction.

## Open Questions Snapshot

- Zone management: reference table vs string, attributes, lifecycle, and history.
- Audit scope: field coverage, snapshots, retention, and queryability.
- Auto-assignment rules and strategy selection (round-robin, load, skill, proximity, priority overrides).
- Infor Visual validation approach and outage handling for work orders/parts.
- Archival and data retention strategy for task data and audit history.

---

## 1. Database Structure

### 1.1 Task Request Extended Fields

**Question**: What additional fields beyond the basic task request should be stored?

**Current Understanding**:

- Basic fields: category, type, work order, part number, description, zone, priority, status
- Timestamps: created_at, started_at, completed_at

**Need Clarification**:

- [Y] Should we track **who assigned** the task (auto-assignment vs manual)?
- [Custom] Do we need **estimated completion time** vs **actual completion time**? Time Created, Time Accepted, Time Completed and ETA
- [Y] Should we store **location details** beyond zone (e.g., specific press number, area)? Yes - Zone -> Workcenter (MTM name for presses and work stations, not be be confused with a computer)
- [Y] Do we need **attachment support** (photos, documents)? 
- [Y] Should we track **task dependencies** (e.g., this task blocks another)? Establish the groundwork as we may end up using it down the road
- [Y] Do we need **recurring task templates**?

**Impact**: Table schema design, stored procedure complexity

---

### 1.2 Zone Management

**Question**: How should zones be managed in the database?

**Current Understanding**:

- Zones are referenced as strings (e.g., "Zone 1", "Zone 2")

**Need Clarification**:

- [Y] Should zones be a **reference table** (`zones` table) instead of varchar?
- [Y] Do zones have **attributes** (e.g., capacity, current load, assigned handlers)? what workstations are in said zone
- [Y] Can zones be **dynamically created/modified** by users? By "Super Admin" only (Plant manager and above)
- [Y] Do zones map to **physical floor locations**?
- [N] Should we store **zone history** (changes over time)?

**Proposed Schema Options**:

**Option A: Simple String**

```sql
zone VARCHAR(20)  -- Current approach
```

**Option B: Reference Table**

```sql
CREATE TABLE zones (
    zone_id INT AUTO_INCREMENT PRIMARY KEY,
    zone_name VARCHAR(50) NOT NULL UNIQUE,
    zone_code VARCHAR(10) NOT NULL,
    building VARCHAR(50),
    floor_number INT,
    is_active TINYINT(1) DEFAULT 1
);

-- In task_requests
zone_id INT,
FOREIGN KEY (zone_id) REFERENCES zones(zone_id)
```

**Impact**: Data integrity, reporting flexibility, admin UI requirements

---

### 1.3 Audit Trail Requirements

**Question**: What level of audit trail is required?

**Current Understanding**:

- `task_request_history` table tracks field changes

**Need Clarification**:

- [ ] Should we track **every field change** or only critical ones?
- [ ] Do we need **before/after snapshots** of entire record?
- [ ] Should audit logs be **queryable for reporting**?
- [ ] What is the **retention period** for audit data?
- [ ] Do we need **user action logging** (logins, searches, views)?
- [ ] Should we track **system actions** (auto-assignments, timeouts)?

**Storage Considerations**:

- High-detail audit logs can grow quickly
- May need separate archive strategy

**Impact**: Storage requirements, query performance, compliance

---

### 1.4 Time Standards & Estimates

**Question**: How should time estimates be managed?

**Current Understanding**:

- `request_types` table has `estimated_minutes` field
- Task requests store estimated and actual duration

**Need Clarification**:

- [ ] Can time estimates **vary by zone, user, or time of day**?
- [ ] Should we track **historical average times** for refinement?
- [ ] Do we need **separate time standards table** with conditions?
- [ ] Should estimates include **travel time, setup time, cleanup time**?
- [ ] Do we need **time range** (min/max/avg) instead of single value?

**Proposed Complex Time Standards Table**:

```sql
CREATE TABLE time_standards (
    standard_id INT AUTO_INCREMENT PRIMARY KEY,
    request_type_id INT NOT NULL,
    zone_id INT,  -- NULL = applies to all zones
    user_role_id INT,  -- NULL = applies to all roles
    time_of_day_start TIME,  -- NULL = any time
    time_of_day_end TIME,
    estimated_minutes INT NOT NULL,
    confidence_level DECIMAL(3,2),  -- 0.0 to 1.0
    based_on_sample_count INT,
    last_updated DATETIME,
    FOREIGN KEY (request_type_id) REFERENCES request_types(request_type_id)
);
```

**Impact**: Complexity of estimation logic, reporting accuracy

---

## 2. Stored Procedures

### 2.1 Auto-Assignment Logic

**Question**: What rules govern auto-assignment of tasks?

**Current Understanding**:

- Tasks can be auto-assigned based on zone/category

**Need Clarification**:

- [ ] **Round-robin** assignment among available handlers?
- [ ] **Load balancing** (assign to least busy handler)?
- [ ] **Skill-based** routing (certain handlers for certain types)?
- [ ] **Availability status** (handlers can mark themselves unavailable)?
- [ ] **Geographic proximity** (assign nearest handler)?
- [ ] **Priority override** (critical tasks go to specific people)?

**Proposed Stored Procedure Signature**:

```sql
CREATE PROCEDURE sp_TaskRequest_AutoAssign(
    IN p_request_id INT,
    IN p_assignment_strategy ENUM('RoundRobin', 'LeastBusy', 'SkillBased', 'Nearest'),
    OUT p_assigned_user_id INT
)
```

**Impact**: Stored procedure complexity, user management tables

---

### 2.2 Task Filtering & Querying

**Question**: What filtering capabilities are needed for task queries?

**Current Understanding**:

- Basic filtering by category, status

**Need Clarification**:

- [ ] **Multi-criteria filtering** (e.g., category AND zone AND priority)?
- [ ] **Date range filtering** (created between dates)?
- [ ] **Full-text search** on description/notes?
- [ ] **Saved filter presets** per user?
- [ ] **Dynamic sorting** (sort by wait time, priority, zone)?
- [ ] **Pagination support** for large result sets?

**Proposed Stored Procedure**:

```sql
CREATE PROCEDURE sp_TaskRequest_Search(
    IN p_categories JSON,  -- ['MaterialHandler', 'Quality']
    IN p_zones JSON,       -- ['Zone1', 'Zone2']
    IN p_priorities JSON,  -- ['Critical', 'Urgent']
    IN p_statuses JSON,    -- ['Pending', 'InProgress']
    IN p_date_from DATETIME,
    IN p_date_to DATETIME,
    IN p_search_text VARCHAR(255),
    IN p_sort_by VARCHAR(50),
    IN p_sort_direction VARCHAR(4),  -- 'ASC' or 'DESC'
    IN p_page_size INT,
    IN p_page_number INT,
    OUT p_total_count INT
)
```

**Impact**: Query performance, index strategy

---

### 2.3 Bulk Operations

**Question**: Do we need bulk operation stored procedures?

**Need Clarification**:

- [ ] **Bulk status updates** (e.g., cancel all pending tasks for a zone)?
- [ ] **Bulk reassignment** (reassign all tasks from one handler to another)?
- [ ] **Bulk priority changes** (elevate priority for all tasks in critical zone)?
- [ ] **Batch imports** (import tasks from external system)?

**Impact**: Stored procedure design, transaction handling

---

## 3. Reference Data

### 3.1 Request Type Configuration

**Question**: How configurable should request types be?

**Current Understanding**:

- Request types are seeded in database with basic properties

**Need Clarification**:

- [ ] Can **administrators add new request types** without code changes?
- [ ] Should request types have **custom fields** (dynamic schema)?
- [ ] Do request types need **workflow templates** (multi-step processes)?
- [ ] Should request types have **notification templates**?
- [ ] Can request types be **temporarily disabled** without deletion?

**Impact**: Admin UI requirements, data validation complexity

---

### 3.2 Priority Levels

**Question**: Should priorities be hardcoded or table-driven?

**Current Understanding**:

- Priorities: Normal, Urgent, Critical (as strings)

**Need Clarification**:

- [ ] Should priorities be in a **reference table** for flexibility?
- [ ] Do priorities need **numeric weights** for sorting?
- [ ] Can priorities have **different meanings per category**?
- [ ] Should priorities affect **SLA timers** automatically?
- [ ] Do we need **priority escalation rules** (auto-escalate after X minutes)?

**Proposed Priority Table**:

```sql
CREATE TABLE priorities (
    priority_id INT AUTO_INCREMENT PRIMARY KEY,
    priority_name VARCHAR(50) NOT NULL UNIQUE,
    priority_level INT NOT NULL,  -- 1=highest, 5=lowest
    display_color VARCHAR(7),  -- Hex color code
    sla_minutes INT,
    auto_escalate TINYINT(1) DEFAULT 0,
    escalate_after_minutes INT,
    escalate_to_priority_id INT,
    FOREIGN KEY (escalate_to_priority_id) REFERENCES priorities(priority_id)
);
```

**Impact**: Flexibility vs simplicity, escalation logic

---

## 4. Performance & Optimization

### 4.1 Index Strategy

**Question**: What indexes are needed for performance?

**Need Clarification**:

- [ ] **Expected concurrent users**? (10? 100? 1000?)
- [ ] **Expected task volume**? (100/day? 1000/day? 10000/day?)
- [ ] **Query patterns** (mostly SELECT by status? by zone? by date?)
- [ ] **Report generation frequency**? (real-time? hourly? daily?)

**Proposed Indexes Based on Common Queries**:

```sql
-- Composite index for common filter
CREATE INDEX idx_task_status_zone_priority 
ON task_requests(status, zone, priority);

-- Index for date range queries
CREATE INDEX idx_task_created_at 
ON task_requests(created_at DESC);

-- Index for user assignment queries
CREATE INDEX idx_task_assigned_to_status 
ON task_requests(assigned_to_user_id, status);
```

**Impact**: Query performance, storage size, write performance

---

### 4.2 Archival Strategy

**Question**: How should old data be archived?

**Need Clarification**:

- [ ] **Retention period** for active data? (30 days? 90 days? 1 year?)
- [ ] **Archive strategy** (separate table? separate database? offline storage?)
- [ ] Should archived data be **queryable**?
- [ ] **Purge schedule** for very old data?
- [ ] **Legal/compliance requirements** for data retention?

**Proposed Archival Tables**:

```sql
CREATE TABLE task_requests_archive (
    -- Same schema as task_requests
    archived_at DATETIME DEFAULT CURRENT_TIMESTAMP
) PARTITION BY RANGE (YEAR(archived_at)) (
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027),
    PARTITION p2027 VALUES LESS THAN (2028)
);
```

**Impact**: Database maintenance, reporting complexity

---

## 5. Integration with Infor Visual

### 5.1 Data Validation

**Question**: How should we validate work order and part numbers against Infor Visual?

**Current Understanding**:

- Infor Visual is READ-ONLY
- Connection: SQL Server

**Need Clarification**:

- [ ] Should validation be **synchronous** (block submit if invalid)?
- [ ] Should validation be **asynchronous** (allow submit, flag later)?
- [ ] What if **Infor Visual is unavailable**? (allow bypass? queue for later?)
- [ ] Should we **cache valid work orders/parts** for offline validation?
- [ ] Do we need **fuzzy matching** for part numbers (typo tolerance)?

**Impact**: User experience, data quality, system resilience

---

### 5.2 Data Refresh Strategy

**Question**: How should we refresh reference data from Infor Visual?

**Need Clarification**:

- [ ] **Refresh frequency** for work orders? (real-time? hourly? daily?)
- [ ] **Refresh frequency** for part numbers?
- [ ] Should we store **snapshots** in MySQL or query on-demand?
- [ ] Do we need **change detection** (notify on work order status changes)?

**Impact**: Database size, network load, data freshness

---

## 6. Security & Compliance

### 6.1 Data Privacy

**Question**: What PII/sensitive data needs protection?

**Need Clarification**:

- [ ] Should **task descriptions** be encrypted (may contain sensitive info)?
- [ ] Should **user names** be masked in certain contexts?
- [ ] Do we need **data access logging** (who viewed what)?
- [ ] Are there **GDPR/privacy requirements**?

**Impact**: Encryption strategy, audit logging

---

### 6.2 Data Deletion

**Question**: What is the process for deleting tasks/users?

**Need Clarification**:

- [ ] **Soft delete** (mark as deleted) vs **hard delete** (permanently remove)?
- [ ] Can completed tasks be **deleted** or only **archived**?
- [ ] What happens to **associated history** when a task is deleted?
- [ ] Can users **delete their own requests**?

**Impact**: Database constraints, audit integrity

---

## Action Items

### High Priority (Before Schema Finalization)

1. [ ] Clarify zone management approach (reference table vs string)
2. [ ] Define auto-assignment rules
3. [ ] Determine audit trail requirements
4. [ ] Confirm Infor Visual validation strategy

### Medium Priority (Before SP Development)

5. [ ] Specify filtering/querying requirements
6. [ ] Define time standards complexity
7. [ ] Determine bulk operation needs
8. [ ] Clarify archival strategy

### Low Priority (Can Be Addressed Later)

9. [ ] Request type extensibility
10. [ ] Priority level configuration
11. [ ] Performance optimization indexes
12. [ ] Data privacy requirements

---

## Next Steps

1. **Review with Stakeholders**: Present questions to operations team
2. **Document Decisions**: Record answers in this document
3. **Update Schema**: Revise database schema based on decisions
4. **Generate DDL**: Create final SQL scripts
5. **Test Schema**: Validate with sample data
6. **Implement DAOs**: Create data access layer

---

**Document Owner**: Database Architect  
**Review Date**: [To Be Scheduled]  
**Status**: Pending Stakeholder Input
