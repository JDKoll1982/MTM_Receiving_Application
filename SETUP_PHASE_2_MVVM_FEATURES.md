# MTM Receiving Application - Phase 2: MVVM Features Development

**Purpose**: Build WinUI 3 MVVM features for receiving label management using the infrastructure from Phase 1.

**Prerequisites**: 
- ✅ Phase 1 (Infrastructure) completed
- ✅ All verification items from Phase 1 checked
- ✅ WinUI 3 project created
- ✅ CommunityToolkit.Mvvm NuGet package installed
- ✅ Dependency injection configured in App.xaml.cs
- ✅ Base ViewModel pattern established
♣
**Estimated Time**: 4-6 hours per major feature

---

## Overview

Phase 2 focuses on building the actual receiving label application using MVVM pattern with WinUI 3. This phase is **feature-driven** and integrates with the speckit workflow.

---

## MVVM Development Principles

When building features, follow these architectural guidelines:

### ViewModel Layer
- Inherit from BaseViewModel for common functionality
- Use observable properties for UI binding
- Implement commands for user actions
- Handle business logic orchestration
- Manage UI state (busy indicators, status messages)
- Never reference UI controls directly

### View Layer
- Use x:Bind for compile-time binding
- Maintain zero code-behind logic (except DI retrieval)
- Follow responsive design principles
- Use proper header/input/grid layouts
- Implement command bar patterns for actions

### Service Integration
- All database operations through DAO layer
- Error handling via IService_ErrorHandler
- Logging via ILoggingService
- Return Model_Dao_Result<T> from DAOs
- All async operations must be awaitable

---

## Feature Development Order

Develop features in this order (use speckit for each):

### Priority 1: Core Label Entry
1. **Receiving Label Entry** (`/speckit.specify Add receiving label entry form`)
    - Full CRUD for receiving lines
    - Material ID lookup integration
    - Date/location tracking
    - Multi-coil support

2. **Dunnage Label Entry** (`/speckit.specify Add dunnage label tracking`)
    - Packaging material tracking
    - Returnable container management
    - Quantity and condition recording

3. **Routing Label Entry** (`/speckit.specify Add internal routing labels`)
    - UPS/FedEx label generation
    - Destination routing
    - Package tracking number storage

### Priority 2: Data Management
4. **Save to History** (`/speckit.specify Implement save to history archive`)
    - Bulk move from current to history table
    - Timestamp archival
    - Clear current session

5. **Auto-Fill from History** (`/speckit.specify Add auto-fill previous values`)
    - Lookup most recent entry
    - Pre-populate common fields
    - Smart defaults based on context

6. **Sort for Printing** (`/speckit.specify Add label sorting functionality`)
    - Multi-level sort (Part ID, PO, Heat)
    - Print order optimization
    - Group by material type

### Priority 3: Label Generation
7. **Label Preview** (`/speckit.specify Add label preview before printing`)
    - Visual representation of label layout
    - Barcode/QR code display
    - Print settings configuration

8. **CSV Export for LabelView** (`/speckit.specify Export to CSV for LabelView`)
    - Format for LabelView software
    - Field mapping configuration
    - Batch export support

9. **Multi-Coil Label Generation** (`/speckit.specify Auto-generate coil labels`)
    - Generate N labels for coils on skid
    - Sequential numbering
    - Coil-specific metadata

### Priority 4: Supporting Features
10. **Search Historical Labels** (`/speckit.specify Add search for past labels`)
     - Filter by date range, part ID, PO
     - Full-text search capabilities
     - Export search results

11. **Infor Visual Integration** (`/speckit.specify Lookup part details from Infor Visual`)
     - Real-time part validation
     - Auto-complete part descriptions
     - UOM and pricing lookup

12. **User Settings** (`/speckit.specify Add user preferences and themes`)
     - Default values configuration
     - Theme selection (Light/Dark)
     - Printer defaults

---

## Speckit Workflow Integration

### Standard Feature Development Process

```
1. Specify Feature
    /speckit.specify [feature description]
    - Creates feature branch
    - Generates spec.md with requirements
    - Creates initial checklist

2. Technical Planning
    /speckit.plan
    - Breaks down into MVVM components
    - Identifies dependencies
    - Creates implementation tasks

3. Implementation
    - Follow tasks.md checklist
    - Use .github/instructions/*.instructions.md
    - Test incrementally

4. Verification
    - Run through verification checklist
    - Manual end-to-end testing
    - Code review

5. Integration
    - Create pull request
    - Merge to master
    - Update CHANGELOG.md
```

### Speckit Configuration Requirements

Ensure the following are in place:
- `.specify/config.json` with project settings
- `.specify/templates/spec-template.md` for consistent specs
- `.specify/templates/tasks-template.md` for task breakdown
- Branch naming convention: `feature/{shortName}-{number}`

---

## Verification Checklist

Before considering Phase 2 complete for ANY feature:

### Specification & Planning
- [ ] Spec created via `/speckit.specify`
- [ ] Technical plan created via `/speckit.plan`
- [ ] Requirements clearly defined
- [ ] Success criteria documented

### MVVM Architecture
- [ ] ViewModel follows BaseViewModel pattern
- [ ] All properties are observable
- [ ] Commands use CommunityToolkit.Mvvm attributes
- [ ] View uses x:Bind (not Binding)
- [ ] Zero business logic in code-behind

### Dependency Injection
- [ ] ViewModel registered in DI container
- [ ] View registered in DI container
- [ ] All dependencies injected via constructor
- [ ] Services properly scoped (Singleton/Transient)

### Data Access & Error Handling
- [ ] All database calls use DAO pattern
- [ ] DAOs return Model_Dao_Result<T>
- [ ] All database operations are async
- [ ] Error handling uses IService_ErrorHandler
- [ ] Logging uses ILoggingService
- [ ] User-facing errors show friendly messages

### Testing & Quality
- [ ] Feature tested manually end-to-end
- [ ] All validation rules enforced
- [ ] UI responds properly to state changes
- [ ] Busy indicators work during async operations
- [ ] Status messages display correctly

### Documentation & Integration
- [ ] GitHub instruction files updated if patterns changed
- [ ] Code comments for complex logic
- [ ] Pull request created with description
- [ ] CHANGELOG.md updated

---

## GitHub Instruction Files

Maintain these instruction files for AI agent guidance:

- `mvvm-viewmodel.instructions.md` - ViewModel patterns and best practices
- `mvvm-view.instructions.md` - XAML view construction guidelines
- `dependency-injection.instructions.md` - DI registration patterns
- `data-binding.instructions.md` - x:Bind and binding patterns
- `error-handling.instructions.md` - Error handling workflows
- `dao-patterns.instructions.md` - Data access object guidelines

---

## Next Steps After Feature Completion

1. **Merge feature branch** to master via pull request
2. **Update CHANGELOG.md** with feature summary
3. **Tag release** if milestone reached
4. **Move to next priority feature** using speckit workflow
5. **Repeat until all Priority 1-3 features complete**

---

## Phase 2 Completion Criteria

Phase 2 is complete when:

- [ ] All Priority 1 features implemented and tested
- [ ] All Priority 2 features implemented and tested
- [ ] All Priority 3 features implemented and tested
- [ ] Priority 4 features evaluated and implemented as needed
- [ ] All verification checklists pass for each feature
- [ ] Full end-to-end application workflow tested
- [ ] Documentation complete (AGENTS.md, copilot-instructions.md)
- [ ] Production deployment plan created

---

**Phase 2 MVVM Development**: Feature-driven, speckit-integrated, quality-focused! 🚀

Use `/speckit.specify` for each new feature to maintain consistency and documentation.
