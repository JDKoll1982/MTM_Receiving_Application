# Module_Receiving - Clarification Questions

## Purpose

This document contains critical questions that need answers before beginning the Module_Receiving rebuild. These questions will shape architectural decisions, library selections, and implementation strategies.

---

## 1. Architecture & Design Decisions

### 1.1 CQRS Pattern (MediatR)

**Question:** Should we adopt MediatR for CQRS pattern, or keep the existing Service layer pattern?

**Context:**

- MediatR would dramatically reduce service file count in Module_Core
- Each service method becomes a separate handler class
- Adds pipeline behaviors for logging, validation, transactions
- Aligns with Clean Architecture principles

**Options:**

- **A) Full MediatR adoption** - All service methods become handlers
- **B) Hybrid approach** - Use MediatR for complex operations, keep simple services
- **C) Keep existing pattern** - Maintain Service layer, improve existing structure

**Impact:** High - affects entire module architecture

**Recommendation:** Option A (Full MediatR) for maximum modularity and service reduction

---

### 1.2 Navigation Library

**Question:** Which navigation approach should we use?

**Options:**

- **A) Uno.Extensions.Navigation.WinUI** (553k downloads, MVVM-first)
- **B) Mvvm.Navigation.WinUI** (5.9k downloads, source generator)
- **C) Singulink.UI.Navigation.WinUI** (4.5k downloads, type-safe)
- **D) Custom lightweight service** (maintain existing pattern)

**Current State:**

- `Service_ReceivingWorkflow` handles navigation currently
- Navigation is tightly coupled to workflow state

**Impact:** Medium - affects ViewModels and workflow management

**Recommendation:** Option A (Uno.Extensions) or Option D (Custom) for consistency with existing DI patterns

---

### 1.3 Module_Core Service Dependencies

**Question:** Which services should remain in Module_Core vs. move to Module_Receiving?

**Current Module_Core Services:**

- `IService_ErrorHandler` - Used by ALL modules
- `IService_LoggingUtility` - Used by ALL modules  
- `IService_Window` - Used by ALL modules
- `IService_Dispatcher` - Used by ALL modules
- `IService_MySQL_ReceivingLine` - **Receiving-specific**
- `IService_ReceivingValidation` - **Receiving-specific**
- `IService_ReceivingWorkflow` - **Receiving-specific**
- `IService_SessionManager` - **Receiving-specific**
- `IService_CSVWriter` - **Generic, could be Core or Receiving**

**Options:**

- **A) Keep only cross-module services in Module_Core** (ErrorHandler, Window, Dispatcher)
- **B) Move Receiving-specific services to Module_Receiving**
- **C) Create Module_Receiving.Contracts for interfaces, implement in Module_Receiving**

**Impact:** High - affects dependency structure

**Recommendation:** Option B + C - Move Receiving services, use Contracts folder

---

### 1.4 Defaults Folder Structure

**Question:** What types of default settings should live in the Defaults folder?

**Potential Defaults:**

- Package type presets (Standard Box, Pallet, Custom)
- Validation rule configurations
- Workflow step configurations
- UI display settings
- Timeout values
- Auto-save intervals

**Should defaults be:**

- **A) Hardcoded models** (classes with const values)
- **B) Configuration files** (JSON/XML loaded at startup)
- **C) Database-driven** (read from settings table)

**Impact:** Low-Medium - affects maintainability

**Recommendation:** Option A (Hardcoded models) for simplicity, with migration path to Option C

---

## 2. Database & Data Access

### 2.1 Stored Procedure Modifications

**Question:** Can we modify existing stored procedures, or are they locked/versioned?

**Context:**

- Some stored procedures may need parameter changes for new features
- May need new stored procedures for additional queries
- Infor Visual stored procedures are READ-ONLY

**Options:**

- **A) Full freedom** - Can modify/add any stored procedure
- **B) Create new procedures only** - Cannot modify existing
- **C) Requires approval process** - Submit change requests

**Impact:** Medium - affects data layer flexibility

**Required Information:**

- [ ] Who owns stored procedure definitions?
- [ ] Is there a version control process?
- [ ] Are there downstream dependencies (other applications)?

---

### 2.2 Database Migration Strategy

**Question:** How do we handle database schema changes during the rebuild?

**Considerations:**

- New tables/columns needed?
- Backward compatibility required?
- Rollback strategy?

**Options:**

- **A) Blue-Green deployment** - New schema alongside old
- **B) In-place migration** - Direct schema updates
- **C) No schema changes** - Work with existing structure only

**Impact:** High if schema changes needed

**Required Information:**

- [ ] Can we add new columns to existing tables?
- [ ] Can we create new tables?
- [ ] What is the rollback plan if deployment fails?

---

### 2.3 Integration Testing Database

**Question:** Is there a dedicated test database for integration tests?

**Context:**

- Integration tests for DAOs require a real database
- Don't want to pollute production/development data

**Options:**

- **A) Dedicated test database** (mtm_receiving_application_test)
- **B) In-memory database** (SQLite with compatibility layer)
- **C) Mock database responses** (no real database in tests)

**Impact:** Medium - affects test strategy

**Recommendation:** Option A (Dedicated test database)

---

## 3. Dependencies & Packages

### 3.1 NuGet Package Approval

**Question:** Can we introduce new NuGet packages, or is there an approval process?

**Packages to Add:**

- MediatR (CQRS pattern)
- Serilog (structured logging)
- FluentValidation (validation rules)
- CsvHelper (CSV export)
- Scrutor (DI assembly scanning)
- Polly (resilience/retry policies) - Optional

**Options:**

- **A) Full approval** - Add any packages needed
- **B) Security review required** - Submit for review
- **C) Restricted list only** - Can only use pre-approved packages

**Impact:** High - affects library selection

**Required Information:**

- [ ] Is there a package approval process?
- [ ] Are there licensing concerns?
- [ ] Are there security/compliance requirements?

---

### 3.2 Third-Party Library Licensing

**Question:** Are there any licensing restrictions on third-party libraries?

**Context:**

- All recommended libraries are Apache 2.0 or MIT licensed
- MediatR is now commercial (requires license key for production)

**Required Information:**

- [ ] Can we use commercial libraries?
- [ ] Budget for MediatR license key?
- [ ] Open-source policy?

**Impact:** Medium - may affect MediatR selection

---

## 4. Validation & Business Logic

### 4.1 Validation Layer

**Question:** Where should validation logic reside?

**Options:**

- **A) ViewModel layer** - Validate before sending to service
- **B) Service layer** - Validate in business logic
- **C) DAO layer** - Validate before database access
- **D) Pipeline behavior (MediatR)** - Validate in request pipeline
- **E) Multiple layers** - ViewModel for UX, Service for business rules

**Current State:**

- `Service_ReceivingValidation` handles most validation
- Some validation in ViewModels

**Impact:** Medium - affects error handling UX

**Recommendation:** Option E (ViewModel for UX, MediatR pipeline for business rules)

---

### 4.2 Existing Validation Rules

**Question:** Are current validation rules documented?

**Context:**

- Need to know what validation currently exists
- Ensure no regressions when migrating to FluentValidation

**Required Information:**

- [ ] Documentation of validation rules?
- [ ] Can we change validation rules, or are they fixed?
- [ ] Are there integration tests covering validation?

**Impact:** Medium - affects migration safety

---

## 5. Testing & Quality

### 5.1 Test Coverage Requirements

**Question:** What is the required test coverage percentage?

**Options:**

- **A) 80%+ coverage** (industry standard)
- **B) 60-80% coverage** (pragmatic)
- **C) No specific requirement**

**Impact:** Low-Medium - affects test writing effort

**Recommendation:** 80% coverage for DAOs, Services, ViewModels

---

### 5.2 Test Strategy

**Question:** Should tests focus on unit tests (mocked) or integration tests (real DB)?

**Options:**

- **A) Unit tests only** - Mock all dependencies
- **B) Integration tests only** - Test against real database
- **C) Hybrid** - Unit tests for logic, integration tests for DAOs

**Impact:** Medium - affects test infrastructure

**Recommendation:** Option C (Hybrid approach)

---

### 5.3 Test Database Data

**Question:** How do we manage test data for integration tests?

**Options:**

- **A) Seed data scripts** - Run before tests
- **B) In-test data creation** - Create in test setup
- **C) Fixture data** - Pre-populated test database

**Impact:** Low - affects test maintenance

**Recommendation:** Option B (In-test data creation) with transaction rollback

---

## 6. UI/UX Considerations

### 6.1 Workflow Navigation Changes

**Question:** Can we modify the receiving workflow, or must it match current exactly?

**Current Workflow:**

```
Mode Selection → PO Entry → Weight/Quantity → Package Type → Heat Lot → Review
                ↓
            Load Entry → Review
```

**Potential Changes:**

- Skip optional steps (Heat Lot if not applicable)
- Conditional navigation based on mode
- Back button behavior

**Options:**

- **A) Exact match required** - No changes to workflow
- **B) Minor improvements allowed** - Optimize UX within current flow
- **C) Full redesign permitted** - Rethink workflow entirely

**Impact:** High if changes allowed

**Required Information:**

- [ ] User feedback on current workflow?
- [ ] Pain points in existing flow?
- [ ] Stakeholder approval needed for changes?

---

### 6.2 Performance Requirements

**Question:** Are there specific performance requirements?

**Metrics to Consider:**

- Page load time (< X seconds)
- Database query response time
- CSV export time for large datasets
- UI responsiveness during async operations

**Options:**

- **A) No specific requirements** - Best effort
- **B) Soft targets** - Aim for X seconds but not critical
- **C) Hard SLAs** - Must meet X seconds or fail

**Impact:** Medium - affects optimization effort

**Required Information:**

- [ ] Current performance metrics?
- [ ] User complaints about slowness?
- [ ] Target hardware specifications?

---

## 7. Deployment & Operations

### 7.1 Deployment Process

**Question:** What is the deployment process for Module_Receiving changes?

**Required Information:**

- [ ] CI/CD pipeline in place?
- [ ] Manual deployment steps?
- [ ] Deployment frequency (daily, weekly, monthly)?
- [ ] Rollback procedure?

**Impact:** Medium - affects release planning

---

### 7.2 Backward Compatibility

**Question:** Must the rebuilt module be backward compatible with existing data?

**Considerations:**

- Database schema compatibility
- Existing receiving sessions in progress
- Historical data access

**Options:**

- **A) Full backward compatibility** - Must work with all existing data
- **B) Migration script** - One-time data migration allowed
- **C) Fresh start** - Archive old data, start clean

**Impact:** High - affects implementation complexity

**Recommendation:** Option A (Full backward compatibility)

---

### 7.3 Feature Flags

**Question:** Should we use feature flags for gradual rollout?

**Context:**

- Allow enabling new features per user/role
- Safer deployment strategy
- Easier rollback

**Options:**

- **A) Feature flags for all major changes**
- **B) Feature flags for high-risk changes only**
- **C) No feature flags** - All-or-nothing deployment

**Impact:** Low-Medium - affects deployment strategy

**Recommendation:** Option B for high-risk changes (MediatR migration, new navigation)

---

## 8. Team & Collaboration

### 8.1 Code Review Process

**Question:** What is the code review process?

**Required Information:**

- [ ] Required number of reviewers?
- [ ] Review checklist/criteria?
- [ ] Constitutional compliance verification?

**Impact:** Low - affects development velocity

---

### 8.2 Development Environment

**Question:** What is the standard development environment?

**Required Information:**

- [ ] Visual Studio version?
- [ ] .NET SDK version?
- [ ] Required VS extensions?
- [ ] Database access for developers?

**Impact:** Low - affects setup instructions

---

## 9. Documentation

### 9.1 Documentation Standards

**Question:** Are there documentation standards to follow?

**Considerations:**

- PlantUML for diagrams (already established)
- XML documentation for public APIs
- README structure
- Changelog format

**Required Information:**

- [ ] Existing documentation templates?
- [ ] Documentation review process?

**Impact:** Low - affects documentation writing

---

### 9.2 Architecture Decision Records (ADRs)

**Question:** Should we create ADRs for major decisions?

**Context:**

- ADRs document why decisions were made
- Useful for future reference
- Lightweight markdown format

**Options:**

- **A) Create ADRs for all major decisions**
- **B) No ADRs, document in code comments**
- **C) Document in ARCHITECTURE.md**

**Impact:** Low - affects documentation overhead

**Recommendation:** Option C (Document in ARCHITECTURE.md)

---

## 10. Timeline & Scope

### 10.1 Timeline

**Question:** What is the expected timeline for completion?

**Rough Estimates:**

- Phase 1 (Foundation): 1 week
- Phase 2 (Data Layer): 1-2 weeks
- Phase 3 (ViewModels): 1-2 weeks
- Phase 4 (Services): 1 week
- Phase 5 (Testing): 1 week
- Phase 6 (Deployment): 1 week

**Total:** 6-8 weeks for full rebuild

**Required Information:**

- [ ] Hard deadline?
- [ ] Resources available (developers, testers)?
- [ ] Can we work incrementally, or must it be a big-bang release?

**Impact:** High - affects scope decisions

---

### 10.2 Scope

**Question:** Is this a full rebuild, or incremental improvements?

**Options:**

- **A) Full rebuild** - Start fresh, apply all new patterns
- **B) Incremental refactor** - Improve piece by piece
- **C) Proof of concept first** - One feature end-to-end, then expand

**Impact:** Very High - affects entire approach

**Recommendation:** Option C (POC with PO Entry workflow, then expand)

---

## Summary: Critical Questions Needing Immediate Answers

### High Priority (Blocks Development)

1. ✅ **CQRS Pattern:** MediatR or Service layer?
2. ✅ **Service Location:** Which services move to Module_Receiving?
3. ✅ **NuGet Approval:** Can we add MediatR, Serilog, FluentValidation?
4. ✅ **Database Modifications:** Can we change stored procedures?
5. ✅ **Backward Compatibility:** Required or optional?

### Medium Priority (Affects Architecture)

6. ⚠️ **Navigation Library:** Uno.Extensions, custom, or other?
2. ⚠️ **Test Strategy:** Unit vs Integration test balance?
3. ⚠️ **Validation Layer:** Where does validation logic live?
4. ⚠️ **Performance Requirements:** Any hard SLAs?

### Low Priority (Nice to Know)

10. ℹ️ **Timeline:** How long do we have?
2. ℹ️ **Test Coverage:** What percentage required?
3. ℹ️ **Documentation Standards:** Existing templates?

---

## Next Steps

1. **Answer critical questions** (1-5)
2. **Review with team** and stakeholders
3. **Document answers** in ARCHITECTURE.md
4. **Proceed to implementation planning**
