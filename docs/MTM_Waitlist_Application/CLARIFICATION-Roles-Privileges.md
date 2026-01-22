# Clarification Questions - User Roles & Privileges

**Date**: January 21, 2026  
**Category**: Security & Access Control  
**Priority**: High

---

## Overview

This document contains questions requiring clarification about the role-based access control system, privilege management, and user authentication for the MTM Waitlist Application.

---

## 1. Role Hierarchy & Structure

### 1.1 Role Inheritance

**Question**: Do roles support inheritance or hierarchical permissions?

**Current Understanding**:
- Flat role structure with direct role-to-privilege mapping
- User can have multiple roles

**Need Clarification**:
- [ ] Should **ProductionLead** automatically inherit **Operator** privileges?
- [ ] Should **PlantManager** automatically inherit ALL role privileges?
- [ ] Can roles have **parent-child relationships**?
- [ ] Should we support **role composition** (e.g., "Lead" = "Operator" + "Analytics")?

**Example Scenarios**:

**Scenario 1: Lead as Operator**

```
ProductionLead needs to:
- Create task requests (as Operator)
- View analytics (as Lead)
- Assign tasks to others

Question: Should they have TWO roles (Operator + ProductionLead) 
or should ProductionLead automatically include Operator privileges?
```

**Scenario 2: Cross-Department Roles**

```
Some users may work in multiple departments:
- Material Handler on Day Shift
- Setup Technician on Night Shift

Question: Can one user have both roles active simultaneously?
How do we handle role-based filtering in this case?
```

**Impact**: Database schema (role hierarchy table), privilege resolution logic, UI complexity

---

### 1.2 Role Granularity

**Question**: How granular should roles be?

**Current Roles**:
- Operator
- MaterialHandler
- MaterialHandlerLead
- SetupTechnician
- Quality
- ProductionLead
- OperatorLead
- PlantManager

**Need Clarification**:
- [ ] Do we need **shift-specific roles** (DayShiftLead vs NightShiftLead)?
- [ ] Do we need **department-specific roles** (PressOperator vs AssemblyOperator)?
- [ ] Should roles be **location-bound** (Building1MaterialHandler)?
- [ ] Do we need **temporary role assignments** (ActingLead for a day)?

**Proposed Granular Roles**:

```sql
-- Option A: Specific Roles
roles:
- PressOperatorDayShift
- PressOperatorNightShift
- MaterialHandlerZone1
- MaterialHandlerZone2
...

-- Option B: Role + Context
roles: Operator, MaterialHandler, Lead
user_role_contexts:
- user_id, role_id, shift, department, zone, effective_from, effective_to
```

**Impact**: Number of roles to manage, complexity of UI, flexibility

---

## 2. Privilege System

### 2.1 Privilege Types

**Question**: Should we have different types of privileges?

**Current Understanding**:
- Flat privilege list (CREATE_TASK_REQUEST, VIEW_ANALYTICS, etc.)

**Need Clarification**:
- [ ] Do we need **data-level privileges** (view only own vs view all)?
- [ ] Do we need **field-level privileges** (can edit priority but not category)?
- [ ] Should privileges have **scope** (zone-specific, shift-specific)?
- [ ] Do we need **time-bound privileges** (temporary access)?

**Proposed Privilege Categories**:

**Category 1: Action Privileges**
- CREATE_TASK_REQUEST
- EDIT_TASK_REQUEST
- DELETE_TASK_REQUEST
- ASSIGN_TASK
- COMPLETE_TASK

**Category 2: View Privileges**
- VIEW_OWN_REQUESTS (data scope)
- VIEW_TEAM_REQUESTS (data scope)
- VIEW_ALL_REQUESTS (data scope)
- VIEW_ANALYTICS
- VIEW_AUDIT_LOGS

**Category 3: Admin Privileges**
- MANAGE_USERS
- MANAGE_ROLES
- MANAGE_REQUEST_TYPES
- MANAGE_SETTINGS
- EXPORT_DATA

**Question**: Should these be separate privilege types in DB?

**Impact**: Privilege checking logic, performance, database schema

---

### 2.2 Privilege Scoping

**Question**: How should privileges be scoped to data?

**Example Scenarios**:

**Scenario 1: Material Handler Privileges**

```
MaterialHandler can fulfill tasks, but should they:
- See tasks from ALL zones?
- See tasks from ONLY their assigned zones?
- See tasks assigned to them specifically?
```

**Scenario 2: Lead Viewing Tasks**

```
ProductionLead can view analytics, but should they see:
- All tasks across all zones?
- Only tasks in their department?
- Only tasks from their shift?
```

**Need Clarification**:
- [ ] Should privilege include **zone filter**?
- [ ] Should privilege include **shift filter**?
- [ ] Should privilege include **department filter**?
- [ ] How do we handle **multi-zone assignments**?

**Proposed Privilege Scope Table**:

```sql
CREATE TABLE user_privilege_scopes (
    scope_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    privilege_id INT NOT NULL,
    scope_type ENUM('Zone', 'Shift', 'Department', 'Global'),
    scope_value VARCHAR(50),  -- e.g., "Zone1", "DayShift"
    effective_from DATETIME DEFAULT CURRENT_TIMESTAMP,
    effective_to DATETIME,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (privilege_id) REFERENCES privileges(privilege_id)
);

-- Example data:
-- MaterialHandler can fulfill tasks in Zone1 and Zone2
user_privilege_scopes:
- user_id=5, privilege_id=FULFILL_MATERIAL_TASKS, scope_type=Zone, scope_value=Zone1
- user_id=5, privilege_id=FULFILL_MATERIAL_TASKS, scope_type=Zone, scope_value=Zone2
```

**Impact**: Query complexity, UI filtering logic, performance

---

### 2.3 Dynamic Privilege Assignment

**Question**: Can privileges be granted/revoked dynamically?

**Need Clarification**:
- [ ] Can **users temporarily delegate** their privileges?
- [ ] Can **leads grant temporary access** to team members?
- [ ] Should privileges have **expiration dates**?
- [ ] Do we need **approval workflow** for privilege changes?

**Example Use Case**:

```
ProductionLead is going on vacation.
They want to temporarily grant their analytics privileges 
to another team member for 2 weeks.

Questions:
- Should this be allowed?
- Who approves?
- How is it tracked?
- What happens when it expires?
```

**Impact**: Admin UI requirements, audit complexity, security risk

---

## 3. Authentication

### 3.1 Multi-Device Login

**Question**: Can users be logged into multiple devices simultaneously?

**Need Clarification**:
- [ ] Should we **enforce single session** (logout on new login)?
- [ ] Allow **multiple concurrent sessions** (personal PC + shared terminal)?
- [ ] Track **all active sessions** per user?
- [ ] Provide **"logout all devices"** functionality?

**Impact**: Session management complexity, security implications

---

### 3.2 Workstation vs User Authentication

**Question**: How do we distinguish personal workstations from shared terminals?

**Current Understanding**:
- Personal workstations: Windows username auto-login
- Shared terminals: Badge + PIN

**Need Clarification**:
- [ ] How is a workstation **registered as personal vs shared**?
- [ ] Can setting be **changed dynamically** (computer becomes shared)?
- [ ] Should we **detect workstation type automatically** (IP, hostname)?
- [ ] What if **multiple people share a personal workstation**?

**Proposed Workstation Registration**:

```sql
CREATE TABLE workstations (
    workstation_id INT AUTO_INCREMENT PRIMARY KEY,
    hostname VARCHAR(100) NOT NULL UNIQUE,
    ip_address VARCHAR(45),
    workstation_type ENUM('Personal', 'Shared') NOT NULL,
    assigned_user_id INT,  -- NULL for shared
    location VARCHAR(100),
    registered_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_seen_at DATETIME,
    is_active TINYINT(1) DEFAULT 1,
    FOREIGN KEY (assigned_user_id) REFERENCES users(user_id)
);

-- Auto-detection based on IP ranges?
CREATE TABLE ip_ranges (
    range_id INT AUTO_INCREMENT PRIMARY KEY,
    ip_start VARCHAR(45) NOT NULL,
    ip_end VARCHAR(45) NOT NULL,
    default_workstation_type ENUM('Personal', 'Shared'),
    location VARCHAR(100)
);
```

**Impact**: Deployment complexity, security, user experience

---

### 3.3 Badge Number Management

**Question**: How are badge numbers managed and validated?

**Need Clarification**:
- [ ] Are badge numbers **numeric only** or alphanumeric?
- [ ] What is the **badge number format** (5 digits? variable length)?
- [ ] Can badge numbers be **reused** after employee leaves?
- [ ] Do we need **badge barcode scanning support**?
- [ ] Should we support **temporary badges** (contractors)?

**Impact**: Validation logic, user creation workflow

---

### 3.4 PIN Security

**Question**: What are the PIN security requirements?

**Need Clarification**:
- [ ] **PIN length** (4 digits? 6 digits? variable)?
- [ ] **PIN complexity** (numeric only? allow letters?)?
- [ ] **PIN expiration** (must change every X days)?
- [ ] **PIN reuse prevention** (can't use last N PINs)?
- [ ] **Failed attempt lockout** (lock account after X failures)?
- [ ] **PIN reset process** (who can reset? self-service?)?

**Proposed PIN Policy Table**:

```sql
CREATE TABLE pin_policy (
    policy_id INT AUTO_INCREMENT PRIMARY KEY,
    min_length INT DEFAULT 4,
    max_length INT DEFAULT 6,
    require_numeric TINYINT(1) DEFAULT 1,
    require_letter TINYINT(1) DEFAULT 0,
    expiration_days INT DEFAULT 90,
    prevent_reuse_count INT DEFAULT 5,
    max_failed_attempts INT DEFAULT 3,
    lockout_duration_minutes INT DEFAULT 30
);

-- Track PIN history for reuse prevention
CREATE TABLE pin_history (
    history_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    pin_hash VARCHAR(255) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
```

**Impact**: Security level, user experience, support burden

---

## 4. Session Management

### 4.1 Timeout Behavior

**Question**: What should happen when a session times out?

**Current Understanding**:
- Personal workstation: 30-minute timeout
- Shared terminal: 15-minute timeout
- Activity tracking (mouse, keyboard)

**Need Clarification**:
- [ ] Should timeout **force logout** or just **lock screen**?
- [ ] Should **in-progress work be saved** before timeout?
- [ ] Can users **extend their session** before timeout?
- [ ] Should we show **countdown warning** before timeout?
- [ ] What happens to **incomplete task requests** on timeout?

**Impact**: Data loss prevention, user experience

---

### 4.2 Activity Tracking

**Question**: What constitutes "activity" for session management?

**Current Understanding**:
- Mouse movement
- Keyboard input

**Need Clarification**:
- [ ] Should **viewing data** (scrolling) count as activity?
- [ ] Should **background operations** (auto-refresh) reset timer?
- [ ] Should activity tracking be **per-module** or global?
- [ ] Do we need to **distinguish idle from away**?

**Impact**: Session timeout frequency, user interruptions

---

### 4.3 Session Data Storage

**Question**: What session data needs to be persisted?

**Need Clarification**:
- [ ] Should **current workflow step** be saved (resume on re-login)?
- [ ] Should **draft task requests** be saved?
- [ ] Should **filter/sort preferences** be saved per session?
- [ ] Should **recent views** be tracked?
- [ ] What is the **session data retention period**?

**Proposed Session Store**:

```sql
CREATE TABLE user_sessions (
    session_id VARCHAR(64) PRIMARY KEY,
    user_id INT NOT NULL,
    workstation_id INT,
    session_type ENUM('Personal', 'Shared'),
    login_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_activity_at DATETIME,
    expires_at DATETIME,
    session_data JSON,  -- Store current state
    is_active TINYINT(1) DEFAULT 1,
    FOREIGN KEY (user_id) REFERENCES users(user_id),
    FOREIGN KEY (workstation_id) REFERENCES workstations(workstation_id)
);
```

**Impact**: Database storage, resume functionality

---

## 5. Role-Based UI Visibility

### 5.1 Navigation Menu Filtering

**Question**: How should navigation menus adapt to user roles?

**Need Clarification**:
- [ ] Should unavailable menu items be **hidden** or **disabled**?
- [ ] Should we show **"coming soon"** placeholders for partial privileges?
- [ ] How do we handle **context-sensitive menus** (different per module)?
- [ ] Should users see **all modules** or only those they have access to?

**Example**:

```
Operator logs in. They should see:
✅ Create Request
✅ My Requests
❌ Material Handler Queue (hidden? disabled?)
❌ Analytics (hidden? disabled?)

Material Handler logs in. They should see:
❌ Create Request (hidden? disabled?)
❌ My Requests (hidden? disabled?)
✅ Material Handler Queue
❌ Analytics (hidden? disabled?)
```

**Impact**: Navigation service design, XAML templates

---

### 5.2 Role-Based View Visibility Matrix

**Question**: Confirm the visibility matrix from mockups is complete

**Current Matrix** (from ROLE_VISIBILITY_MATRIX.md):

| Role | Can See Categories | Filter Options |
|------|-------------------|----------------|
| Operator | All 4 | N/A |
| MaterialHandler | MH only | None |
| MaterialHandlerLead | MH + QC | MH/QC/All toggle |
| SetupTechnician | ST only | None |
| Quality | QC only | None |
| ProductionLead | PL + MH + QC | PL/MH/QC/All toggle |
| OperatorLead | PL + MH + QC | PL/MH/QC/All toggle |
| PlantManager | All 4 | MH/ST/QC/PL/All toggle |

**Need Clarification**:
- [ ] Is this matrix **finalized**?
- [ ] Should **OperatorLead** have different visibility than **ProductionLead**?
- [ ] Do we need **MaterialHandlerLead** to see **SetupTechnician** tasks?
- [ ] Should **Quality** be able to see **MaterialHandler** NCM tasks?

**Impact**: Filtering logic, UI implementation

---

## 6. User Management

### 6.1 User Creation Workflow

**Question**: How are new users added to the system?

**Need Clarification**:
- [ ] Can users **self-register** (new user creation flow)?
- [ ] Do users need **approval** before activation?
- [ ] Who can **create new users** (PlantManager? HR? IT)?
- [ ] What **default role** should new users have (if any)?
- [ ] Should new users have **limited access** until fully onboarded?

**Proposed User Creation Workflow**:

```
Option A: Self-Service
1. User scans badge on shared terminal
2. System detects badge not in DB
3. User enters first name, last name, creates PIN
4. System assigns default "Operator" role
5. Lead/Manager approves or modifies role

Option B: Admin-Created
1. PlantManager/Admin opens User Management
2. Enters badge number, name, assigns role(s)
3. System generates temporary PIN
4. User logs in and is forced to change PIN
```

**Impact**: UI requirements, security workflow

---

### 6.2 User Deactivation

**Question**: What happens when a user leaves the company?

**Need Clarification**:
- [ ] Should users be **soft-deleted** (is_active=0) or **hard-deleted**?
- [ ] What happens to **tasks created by** that user?
- [ ] What happens to **tasks assigned to** that user?
- [ ] Should **historical data** retain user references?
- [ ] Can deactivated users be **reactivated**?

**Impact**: Data integrity, audit trail

---

### 6.3 Bulk User Operations

**Question**: Do we need bulk user management capabilities?

**Need Clarification**:
- [ ] **Bulk role assignment** (assign all operators to OperatorLead)?
- [ ] **Bulk deactivation** (terminate all temp workers)?
- [ ] **Import from HR system** (CSV import)?
- [ ] **Role migration** (promote all MaterialHandlers to MaterialHandlerLead)?

**Impact**: Admin UI complexity, data migration tools

---

## 7. Privilege Enforcement

### 7.1 Enforcement Points

**Question**: Where should privilege checks be enforced?

**Need Clarification**:
- [ ] **UI Level**: Hide/disable features user can't access
- [ ] **ViewModel Level**: Check privilege before executing command
- [ ] **Service Level**: Validate privilege in business logic
- [ ] **DAO Level**: Filter data based on user privileges
- [ ] **Database Level**: Use database views/stored procedures for row-level security

**Recommendation**: Enforce at **multiple levels** (defense in depth):
- UI: Better UX (don't show unavailable features)
- ViewModel: Prevent tampering
- Service: Business logic validation
- DAO: Data access security

**Impact**: Code complexity, performance

---

### 7.2 Privilege Checking Performance

**Question**: How to optimize privilege checks?

**Need Clarification**:
- [ ] Should privileges be **cached in memory** after login?
- [ ] Should privileges be **reloaded on each check** (for real-time updates)?
- [ ] What is the **cache expiration policy**?
- [ ] How do we handle **privilege changes** while user is logged in?

**Proposed Caching Strategy**:

```csharp
public class Service_PrivilegeManager
{
    private readonly Dictionary<int, List<string>> _userPrivilegeCache;
    private readonly Dictionary<int, DateTime> _cacheExpiry;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(15);

    public async Task<bool> UserHasPrivilegeAsync(int userId, string privilegeName)
    {
        // Check cache first
        if (_userPrivilegeCache.TryGetValue(userId, out var privileges))
        {
            if (_cacheExpiry[userId] > DateTime.Now)
            {
                return privileges.Contains(privilegeName);
            }
        }

        // Reload from database
        // Update cache
        // Return result
    }

    public void InvalidateUserCache(int userId)
    {
        _userPrivilegeCache.Remove(userId);
    }
}
```

**Impact**: Performance vs real-time accuracy trade-off

---

## 8. Special Cases

### 8.1 Plant Manager Override

**Question**: Can PlantManager override any restriction?

**Need Clarification**:
- [ ] Can PlantManager **edit any task** regardless of category?
- [ ] Can PlantManager **assign tasks to anyone**?
- [ ] Can PlantManager **delete completed tasks**?
- [ ] Should PlantManager actions be **flagged in audit log**?
- [ ] Are there **any restrictions** on PlantManager?

**Impact**: God-mode privileges, audit requirements

---

### 8.2 Emergency Access

**Question**: How do we handle emergency situations?

**Example Scenarios**:

```
Scenario 1: All MaterialHandlers are absent
Question: Can ProductionLead temporarily fulfill material tasks?

Scenario 2: System is down, need manual override
Question: Is there a "break glass" admin account?

Scenario 3: User locked out due to failed PIN attempts
Question: How is access restored?
```

**Need Clarification**:
- [ ] Do we need **temporary privilege elevation**?
- [ ] Do we need **emergency admin account**?
- [ ] Should emergency actions be **logged differently**?

**Impact**: Security vs operational continuity

---

## Action Items

### Critical (Before Implementation)

1. [ ] Confirm role visibility matrix
2. [ ] Define privilege scoping requirements
3. [ ] Specify authentication workflows (personal vs shared)
4. [ ] Determine session timeout behavior

### High Priority (Before Beta)

1. [ ] Clarify user management workflows
2. [ ] Define privilege caching strategy
3. [ ] Specify multi-device login policy
4. [ ] Determine PlantManager override capabilities

### Medium Priority (Before Production)

1. [ ] PIN security policy details
2. [ ] Role inheritance design
3. [ ] Emergency access procedures
4. [ ] Bulk user operation requirements

---

## Next Steps

1. **Schedule Review Meeting**: Operations, IT, HR
2. **Document Decisions**: Update this file with answers
3. **Design Database Schema**: Finalize users/roles/privileges tables
4. **Implement Auth System**: Code authentication and privilege services
5. **Create Admin UI**: User management interface
6. **Security Testing**: Penetration testing and audit

---

**Document Owner**: Security Architect  
**Review Date**: [To Be Scheduled]  
**Status**: Pending Stakeholder Input
