# Clarification Questions - Authentication & Authorization

**Date**: January 21, 2026  
**Category**: Security & Access Control  
**Priority**: Critical

---

## Overview

This document contains questions requiring clarification about authentication mechanisms, authorization rules, privilege systems, and session management for the MTM Waitlist Application.

---

## 1. Authentication Strategy

### 1.1 Authentication Method

**Question**: How should users authenticate?

**Options**:

**Option A: Windows Authentication (Active Directory)**
```csharp
// Automatic login using Windows credentials
var windowsIdentity = WindowsIdentity.GetCurrent();
var username = windowsIdentity.Name; // DOMAIN\username
```
- **Pros**: No password management, SSO experience, secure
- **Cons**: Windows-only, requires AD setup

**Option B: Username/Password (Custom)**
```csharp
// User enters username/password
// Store hashed password in MySQL database
var hashedPassword = BCrypt.HashPassword(password);
```
- **Pros**: Platform-independent, full control
- **Cons**: Password management burden, security responsibility

**Option C: Hybrid (Windows + Fallback)**
```csharp
// Try Windows auth first, fallback to username/password
if (windowsAuthAvailable) {
    UseWindowsAuth();
} else {
    UsePasswordAuth();
}
```
- **Pros**: Flexibility, works in all scenarios
- **Cons**: More complex implementation

**Need Clarification**:
- [ ] Which authentication method should be used?
- [ ] Is **Active Directory** available in the environment?
- [ ] Should there be a **"Remember Me"** feature?
- [ ] Should there be **password reset** functionality (email? admin?)?
- [ ] Should there be **session timeout** (auto-logout after inactivity)?
- [ ] What is the **session duration** (8 hours? 12 hours? 24 hours?)?

**Impact**: Authentication service design, security architecture, user experience

---

### 1.2 Employee Number Validation

**Question**: Should employee numbers be validated against Infor Visual?

**Need Clarification**:
- [ ] Is there an **employee master table** in Infor Visual?
- [ ] Should user registration **sync from Infor Visual**?
- [ ] Should employee number be **auto-populated** from Windows username?
- [ ] What happens if employee number **not found** in Infor Visual?

**Impact**: User provisioning, data integrity

---

## 2. Authorization & Privileges

### 2.1 Privilege System Architecture

**Question**: How granular should the privilege system be?

**Proposed Architecture (Option A: Role-Based Simple)**:
```
Roles (Fixed):
- Operator
- MaterialHandler
- MaterialHandlerLead
- SetupTechnician
- Quality
- OperatorLead
- ProductionLead
- PlantManager
- Administrator

Each role has predefined permissions
```

**Proposed Architecture (Option B: Role + Privilege Granular)**:
```
Roles (Configurable):
- Operator
- MaterialHandler
- ...

Privileges (Granular):
- task.create
- task.view.own
- task.view.all
- task.assign
- task.reassign
- task.cancel.own
- task.cancel.all
- analytics.view.basic
- analytics.view.advanced
- users.manage
- settings.edit
- zones.manage
- categories.manage
```

**Need Clarification**:
- [ ] Should roles be **fixed** or **configurable**?
- [ ] Should privileges be **per-feature** (create, view, edit, delete)?
- [ ] Can users have **multiple roles**?
- [ ] Can roles have **temporary assignments** (acting supervisor)?
- [ ] Should there be **role hierarchy** (Plant Manager > Production Lead > Operator)?

**Impact**: Authorization complexity, admin flexibility, security granularity

---

### 2.2 Permission Inheritance

**Question**: Should permissions be inherited in role hierarchy?

**Example Hierarchy**:
```
PlantManager
  └─ ProductionLead
      └─ OperatorLead
          └─ Operator

Question: If PlantManager can view all tasks, 
should ProductionLead automatically inherit this?
```

**Need Clarification**:
- [ ] Should there be **role inheritance** (child roles get parent permissions)?
- [ ] Should there be **permission overrides** (deny specific permission for specific user)?
- [ ] Should there be **context-based permissions** (can only edit own department's tasks)?

**Impact**: Permission evaluation complexity, flexibility

---

### 2.3 Data-Level Permissions

**Question**: Should permissions be data-dependent?

**Examples**:
```
Operator:
- Can view: Own tasks only
- Can create: Tasks for own zone only
- Can cancel: Own tasks only (within 10 minutes)

OperatorLead:
- Can view: All tasks in own department
- Can reassign: Tasks within own department
- Can cancel: Any task in own department

ProductionLead:
- Can view: All tasks across all departments
- Can reassign: Any task
- Can cancel: Any task
```

**Need Clarification**:
- [ ] Should permissions be **filtered by zone**?
- [ ] Should permissions be **filtered by department**?
- [ ] Should permissions be **filtered by shift**?
- [ ] Should there be **time-based restrictions** (can only cancel within X minutes)?
- [ ] Should there be **ownership rules** (can only edit own tasks)?

**Impact**: Query complexity, authorization checks

---

## 3. Role Definitions

### 3.1 Operator Permissions

**Question**: What can operators do?

**Proposed Permissions**:
- [ ] **Create task requests** (for all categories? own category only?)
- [ ] **View own requests** (always? or only recent?)
- [ ] **View request status** (real-time updates?)
- [ ] **Cancel own requests** (within time limit?)
- [ ] **Edit own requests** (before assignment? always?)
- [ ] **Re-submit previous requests** (as templates?)
- [ ] **Favorite templates** (save for quick re-use?)
- [ ] **View analytics** (own performance? overall stats?)

**Need Clarification**:
- [ ] Can operators create requests for **any category** or only **specific categories**?
- [ ] Can operators **cancel tasks** after they've been assigned?
- [ ] Should operators see **who is assigned** to their task?
- [ ] Should operators be able to **rate/feedback** on completed tasks?

**Impact**: Operator workflow, UI design

---

### 3.2 Material Handler Permissions

**Question**: What can material handlers do?

**Proposed Permissions**:
- [ ] **View available tasks** (for own zones? all zones?)
- [ ] **Claim tasks** (from queue)
- [ ] **View assigned tasks** (own tasks only? team tasks?)
- [ ] **Start task** (mark in-progress)
- [ ] **Complete task** (with notes?)
- [ ] **Mark task on-hold** (with reason?)
- [ ] **Request assistance** (escalate to lead?)
- [ ] **Add quick tasks** (non-requested work?)
- [ ] **View zone status** (tasks per zone?)

**Need Clarification**:
- [ ] Can handlers **reject assigned tasks**?
- [ ] Can handlers **reassign tasks** to another handler?
- [ ] Should handlers see **task priority** (to choose which to claim)?
- [ ] Should handlers see **estimated duration** before claiming?
- [ ] Can handlers claim **multiple tasks** simultaneously?

**Impact**: Handler workflow, task assignment

---

### 3.3 Material Handler Lead Permissions

**Question**: What additional permissions do leads have?

**Proposed Permissions**:
- [ ] **All Material Handler permissions** +
- [ ] **Assign tasks manually** (override auto-assignment)
- [ ] **Reassign tasks** (between handlers)
- [ ] **View all handler tasks** (not just own)
- [ ] **View handler performance** (completion times, counts)
- [ ] **Manage zone assignments** (assign handlers to zones)
- [ ] **Override task status** (force complete, cancel)
- [ ] **View analytics** (team performance, bottlenecks)

**Need Clarification**:
- [ ] Can leads **create tasks** on behalf of operators?
- [ ] Can leads **edit task details** after creation?
- [ ] Can leads **pause auto-assignment** (manual mode)?
- [ ] Can leads **set handler availability** (mark as offline)?

**Impact**: Lead responsibilities, oversight capabilities

---

### 3.4 Production Lead & Plant Manager Permissions

**Question**: What visibility and control do leadership roles have?

**Proposed Permissions**:

**Production Lead**:
- [ ] **View all tasks** (across all categories)
- [ ] **View analytics** (department-level, plant-level?)
- [ ] **Reassign tasks** (across departments?)
- [ ] **Manage priorities** (escalate, downgrade?)
- [ ] **View handler status** (who's working on what?)
- [ ] **Cancel any task** (with reason)

**Plant Manager**:
- [ ] **All Production Lead permissions** +
- [ ] **Manage users** (add, deactivate, change roles)
- [ ] **Manage settings** (categories, types, zones)
- [ ] **View advanced analytics** (trends, predictions)
- [ ] **Export reports** (CSV, Excel?)
- [ ] **Manage system configuration**

**Need Clarification**:
- [ ] Can Production Lead **manage users** (or only Plant Manager)?
- [ ] Can Production Lead **edit settings** (or read-only)?
- [ ] Should analytics be **filtered by department** for Production Lead?
- [ ] Should Plant Manager have **audit log access**?

**Impact**: Administrative features, reporting requirements

---

## 4. Session Management

### 4.1 Session Storage

**Question**: How should sessions be stored?

**Option A: In-Memory (Application State)**
```csharp
// Sessions stored in app memory
// Lost on app restart
private static Dictionary<string, UserSession> activeSessions;
```

**Option B: Database (MySQL sessions table)**
```sql
CREATE TABLE user_sessions (
    session_id VARCHAR(100) PRIMARY KEY,
    user_id INT NOT NULL,
    login_at DATETIME,
    last_activity_at DATETIME,
    ip_address VARCHAR(50),
    device_info VARCHAR(200),
    is_active TINYINT(1) DEFAULT 1
);
```

**Option C: Distributed Cache (Redis)**
```
Store sessions in Redis for:
- Persistence across app restarts
- Multi-instance support (load balancing)
```

**Need Clarification**:
- [ ] Should sessions **persist across app restarts**?
- [ ] Should users be able to have **multiple sessions** (multiple devices)?
- [ ] Should there be **concurrent session limits** (max 1? max 3?)?
- [ ] Should sessions have **idle timeout** (auto-logout after inactivity)?
- [ ] Should there be **absolute timeout** (force re-login after X hours)?

**Impact**: Session service architecture, scalability

---

### 4.2 Session Timeout

**Question**: How should session timeout work?

**Proposed Timeout Rules**:
```
Idle Timeout: 30 minutes (no activity)
Absolute Timeout: 12 hours (force re-login)
Warning: Show warning 2 minutes before timeout
```

**Need Clarification**:
- [ ] What is the **idle timeout** duration?
- [ ] What is the **absolute timeout** duration?
- [ ] Should timeout be **role-based** (longer for leads/managers)?
- [ ] Should there be a **"Stay signed in"** option?
- [ ] What actions **reset the idle timer** (any click? specific actions?)?
- [ ] Should users be **warned** before timeout?

**Impact**: User experience, security, session service

---

### 4.3 Logout Behavior

**Question**: What should happen on logout?

**Need Clarification**:
- [ ] Should logout **invalidate session** immediately?
- [ ] Should logout **save user state** (for next login)?
- [ ] Should there be **"Logout all sessions"** (other devices)?
- [ ] Should logout be **audited** (log who logged out when)?
- [ ] What happens to **in-progress tasks** on logout?

**Impact**: Security, user experience

---

## 5. Security Considerations

### 5.1 Password Policy

**Question**: What password requirements should be enforced? *(If using password auth)*

**Proposed Policy**:
```
Minimum length: 8 characters
Require: Uppercase, lowercase, number, special character
Password history: Cannot reuse last 5 passwords
Expiration: 90 days
Lockout: After 5 failed attempts, lock for 15 minutes
```

**Need Clarification**:
- [ ] What are the **password complexity requirements**?
- [ ] Should passwords **expire**?
- [ ] Should there be **password history** (prevent reuse)?
- [ ] Should there be **account lockout** after failed attempts?
- [ ] Who can **reset passwords** (self-service? admin only?)?

**Impact**: Security service, user experience

---

### 5.2 Role Assignment Security

**Question**: Who can assign/change user roles?

**Need Clarification**:
- [ ] Can users **self-assign roles** (on first login)?
- [ ] Can **leads assign roles** (within their team)?
- [ ] Can **only admins assign roles** (centralized)?
- [ ] Should role changes be **audited**?
- [ ] Should role changes require **approval**?

**Impact**: User management workflow, audit requirements

---

### 5.3 Data Access Security

**Question**: How should sensitive data be protected?

**Need Clarification**:
- [ ] Should **connection strings** be encrypted in appsettings.json?
- [ ] Should **sensitive data** (employee numbers, emails) be encrypted in database?
- [ ] Should **audit logs** be tamper-proof (write-only, signed)?
- [ ] Should **failed login attempts** be logged?
- [ ] Should there be **IP whitelisting** (restrict to plant network)?

**Impact**: Security architecture, compliance

---

## 6. Authorization Patterns

### 6.1 Permission Checking

**Question**: How should permission checks be implemented?

**Option A: Attribute-Based**
```csharp
[RequirePermission("task.create")]
public async Task CreateTaskAsync(TaskRequest request)
{
    // Method only executes if user has permission
}
```

**Option B: Service-Based**
```csharp
public async Task CreateTaskAsync(TaskRequest request)
{
    if (!await _authService.HasPermissionAsync("task.create"))
    {
        throw new UnauthorizedException();
    }
    // Proceed
}
```

**Option C: Policy-Based (.NET Authorization)**
```csharp
[Authorize(Policy = "CanCreateTasks")]
public async Task CreateTaskAsync(TaskRequest request)
{
    // Uses .NET authorization policies
}
```

**Need Clarification**:
- [ ] Which authorization pattern should be used?
- [ ] Should permissions be **checked at UI level** (hide buttons)?
- [ ] Should permissions be **checked at service level**?
- [ ] Should permissions be **checked at DAO level** (defense in depth)?
- [ ] Should permission denied show **error message** or **silently fail**?

**Impact**: Code architecture, security depth

---

### 6.2 Context-Based Authorization

**Question**: Should authorization consider context (data state)?

**Examples**:
```csharp
// Example 1: Time-based
CanCancelTask if:
- User is operator AND
- Task was created by user AND
- Task was created less than 10 minutes ago AND
- Task status is 'Pending'

// Example 2: Ownership-based
CanEditTask if:
- User is task owner OR
- User is handler assigned to task OR
- User is lead of handler's team

// Example 3: State-based
CanReassignTask if:
- Task status is 'Assigned' OR 'OnHold'
- Task is not 'Completed' or 'Cancelled'
```

**Need Clarification**:
- [ ] Should authorization rules consider **task state**?
- [ ] Should authorization rules consider **ownership**?
- [ ] Should authorization rules consider **time constraints**?
- [ ] Should authorization rules be **configurable** (admin-editable)?

**Impact**: Authorization complexity, rule engine

---

## 7. Reusable Components from MTM Receiving

### 7.1 Authentication Service

**Question**: Can we reuse MTM Receiving's authentication service?

**MTM Receiving Has**:
- `IService_Authentication`
- `Service_Authentication`
- Windows authentication support
- Session tracking

**Need Clarification**:
- [ ] Should we **copy and adapt** MTM Receiving's auth service?
- [ ] Should we **extract to shared library** (for both apps)?
- [ ] What **changes are needed** for privilege system?
- [ ] Should we **maintain consistency** with MTM Receiving patterns?

**Impact**: Code reuse, maintainability, consistency

---

### 7.2 Session Management Service

**Question**: Can we reuse MTM Receiving's session management?

**MTM Receiving Has**:
- `IService_SessionManager`
- `Service_SessionManager`
- User session tracking
- Session persistence

**Need Clarification**:
- [ ] Should we **reuse as-is** or **modify**?
- [ ] Does MTM Receiving session service support **multiple sessions per user**?
- [ ] Does it support **timeout configuration**?
- [ ] Should we **share session service code**?

**Impact**: Development time, code consistency

---

## 8. User Provisioning

### 8.1 New User Registration

**Question**: How are new users added to the system?

**Option A: Self-Registration (First Login)**
```
1. User launches app
2. Authenticates with Windows/password
3. Presented with "New User Setup" wizard
4. Selects primary role
5. Admin approves (email notification)
6. User granted access
```

**Option B: Admin Pre-Provisioning**
```
1. Admin adds user in "User Management" screen
2. Sets username, employee number, role
3. User can now login
```

**Option C: Import from Infor Visual**
```
1. Admin runs "Sync Users from Infor Visual"
2. System imports all employees
3. Admin assigns roles to imported users
```

**Need Clarification**:
- [ ] Which provisioning approach?
- [ ] Can users **self-register**?
- [ ] Does registration require **approval**?
- [ ] Should users be **synced from Infor Visual**?
- [ ] What happens to **inactive employees** (auto-deactivate)?

**Impact**: User onboarding process, admin workload

---

### 8.2 Role Assignment

**Question**: When and how are roles assigned to users?

**Need Clarification**:
- [ ] Are roles assigned **on first login** (self-select)?
- [ ] Are roles assigned **by admin** (before first login)?
- [ ] Are roles assigned **by lead** (for their team)?
- [ ] Can users **request role changes**?
- [ ] Should role changes be **logged/audited**?

**Impact**: Admin interface, workflow

---

## 9. Compliance & Auditing

### 9.1 Audit Requirements

**Question**: What actions should be audited?

**Proposed Audit Events**:
```
Authentication:
- User login (success/failure)
- User logout
- Password change
- Session timeout

Authorization:
- Role assignment
- Permission change
- Failed permission check (access denied)

Data Changes:
- Task created/modified/deleted
- User created/modified/deactivated
- Setting changed
```

**Need Clarification**:
- [ ] What **authentication events** should be logged?
- [ ] What **authorization events** should be logged?
- [ ] What **data changes** should be audited?
- [ ] Should **read operations** be audited (who viewed what)?
- [ ] How long should **audit logs be retained**?
- [ ] Who can **view audit logs**?

**Impact**: Audit service design, storage requirements

---

### 9.2 Compliance Requirements

**Question**: Are there compliance requirements (GDPR, SOX, etc.)?

**Need Clarification**:
- [ ] Are there **regulatory compliance** requirements?
- [ ] Is this system subject to **SOX** (financial controls)?
- [ ] Is there **PII** (personally identifiable information) that must be protected?
- [ ] Are there **data retention policies**?
- [ ] Are there **right to be forgotten** requirements (GDPR)?

**Impact**: Data handling, privacy, security

---

## 10. Advanced Scenarios

### 10.1 Delegated Access

**Question**: Can users delegate their access to others?

**Example**:
```
Production Lead on vacation:
- Delegates permissions to Acting Lead for 1 week
- Acting Lead gets temporary Production Lead permissions
- Permissions automatically revert after delegation period
```

**Need Clarification**:
- [ ] Should **permission delegation** be supported?
- [ ] Should delegation be **temporary** (with expiration)?
- [ ] Should delegation be **audited**?
- [ ] Can delegation be **revoked early**?

**Impact**: Authorization complexity, delegation UI

---

### 10.2 Emergency Access

**Question**: What if someone needs access outside their normal permissions?

**Need Clarification**:
- [ ] Should there be **"break glass"** emergency access?
- [ ] Who can **grant emergency access**?
- [ ] Should emergency access be **time-limited**?
- [ ] Should emergency access be **audited** (extra scrutiny)?

**Impact**: Security vs operational flexibility

---

## Action Items

### Critical (Before Development)
1. [ ] Confirm authentication method (Windows vs password)
2. [ ] Define role structure (fixed vs configurable)
3. [ ] Define privilege granularity (role-based vs feature-based)
4. [ ] Define session management approach (storage, timeout)

### High Priority (Before Alpha)
5. [ ] Define operator permissions (what can they do?)
6. [ ] Define handler permissions (claim, complete, reassign?)
7. [ ] Define lead permissions (oversight, management)
8. [ ] Define admin permissions (user management, settings)
9. [ ] Define audit requirements (what to log, retention)

### Medium Priority (Before Beta)
10. [ ] Implement password policy (if using password auth)
11. [ ] Define role assignment workflow
12. [ ] Define user provisioning process
13. [ ] Implement permission checking pattern

### Low Priority (Before Production)
14. [ ] Delegation support (if needed)
15. [ ] Emergency access (if needed)
16. [ ] Compliance review (GDPR, SOX)

---

## Next Steps

1. **Security Review Meeting**: Validate security decisions with stakeholders
2. **Define Authorization Matrix**: Map roles to permissions in spreadsheet
3. **Create ER Diagram**: Database schema for users, roles, sessions
4. **Implement Auth Service**: Code authentication and authorization
5. **Test Security**: Penetration testing, permission validation
6. **Document Security**: Security architecture documentation

---

**Document Owner**: Security Architect / Lead Developer  
**Review Date**: [To Be Scheduled]  
**Status**: Pending Stakeholder Input
