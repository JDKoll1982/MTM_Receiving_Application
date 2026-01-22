# Service Locator Anti-Pattern Elimination Plan

## Executive Summary

This plan addresses the elimination of **141 instances** of the Service Locator anti-pattern (`App.GetService<T>()`) found throughout the MTM Receiving Application codebase. The Service Locator pattern undermines dependency injection benefits and makes code difficult to test and maintain.

**Goal**: Replace all Service Locator calls with proper constructor injection, following SOLID principles and DDD best practices.

---

## Problem Statement

### Current State
- **141 instances** of `App.GetService<T>()` scattered across Views, ViewModels, and services
- Dependencies are hidden, making testing impossible
- Violates Dependency Inversion Principle
- Creates tight coupling to DI container
- Makes dependency graphs invisible

### Impact
- **Testability**: Cannot mock dependencies for unit tests
- **Maintainability**: Hidden dependencies make refactoring risky
- **Clarity**: Dependency requirements are not explicit
- **SOLID Violations**: Breaks Dependency Inversion and Interface Segregation principles

### Target State
- **Zero Service Locator calls** in production code
- All dependencies injected via constructors
- Clear, explicit dependency graphs
- Fully testable codebase
- SOLID-compliant architecture

---

## Architecture Analysis

### Affected Areas (From Build Errors)
1. **Module_Dunnage** - ~30 instances
   - Views: WorkflowView, AdminTypesView, ReviewView, EditModeView, etc.
   - ViewModels: AdminInventoryViewModel
   - Dialogs: AddTypeDialog, AddToInventoriedListDialog

2. **Module_Settings** - ~50 instances
   - Receiving, Dunnage, Routing, Reporting, Volvo, DeveloperTools
   - All NavigationHub and feature page Views

3. **Module_Routing** - ~10 instances
   - RoutingWizardContainerView, RoutingWizardStep1View, etc.
   - RoutingModeSelectionView, RoutingManualEntryView

4. **Module_Receiving** - ~15 instances
   - PackageType view and others

5. **Module_Reporting** - ~5 instances
   - Main view

6. **Module_Shared** - ~5 instances
   - ViewModel_Shared_Base

7. **Module_Core** - ~5 instances
   - Settings views

8. **Module_Volvo** - ~5 instances
   - Volvo views

### Dependency Patterns Identified

**Pattern 1: Simple View (1-2 dependencies)**
```csharp
// Current - Service Locator
public MyView()
{
    InitializeComponent();
    ViewModel = App.GetService<MyViewModel>();
}

// Target - Constructor Injection
public MyView(MyViewModel viewModel)
{
    InitializeComponent();
    ViewModel = viewModel;
    DataContext = viewModel;
}
```

**Pattern 2: View with Services (3-5 dependencies)**
```csharp
// Current - Service Locator
public MyView()
{
    InitializeComponent();
    ViewModel = App.GetService<MyViewModel>();
    _focusService = App.GetService<IService_Focus>();
    _workflowService = App.GetService<IService_Workflow>();
}

// Target - Constructor Injection
public MyView(
    MyViewModel viewModel,
    IService_Focus focusService,
    IService_Workflow workflowService)
{
    InitializeComponent();
    ViewModel = viewModel;
    _focusService = focusService;
    _workflowService = workflowService;
    DataContext = viewModel;
}
```

**Pattern 3: ViewModel Creating Dialogs**
```csharp
// Current - Service Locator
private async Task ShowDialogAsync()
{
    var dialog = App.GetService<MyDialog>();
    await dialog.ShowAsync();
}

// Target - Factory Pattern or Direct Injection
private readonly Func<MyDialog> _dialogFactory;

public MyViewModel(Func<MyDialog> dialogFactory)
{
    _dialogFactory = dialogFactory;
}

private async Task ShowDialogAsync()
{
    var dialog = _dialogFactory();
    await dialog.ShowAsync();
}
```

**Pattern 4: Shared Base Class**
```csharp
// Current - Service Locator in base class
protected void ShowNotification(string message)
{
    var notificationService = App.GetService<IService_Notification>();
    notificationService.Show(message);
}

// Target - Inject in base class
protected readonly IService_Notification NotificationService;

protected ViewModel_Shared_Base(IService_Notification notificationService)
{
    NotificationService = notificationService;
}
```

---

## Refactoring Strategy

### Phased Approach

**Why Phased?**
- 141 files is too large to change at once
- Allows incremental testing and validation
- Reduces risk of breaking functionality
- Makes code review manageable
- Enables rollback if issues arise

### Phase Breakdown

#### Phase 1: Simple Views (Estimated: 20 files, 1 hour)
**Scope**: Views with 1-2 dependencies (ViewModel only or ViewModel + 1 service)

**Files**:
- Settings overview pages (~20 files)
- Simple navigation views
- Reporting views

**Approach**:
1. Add constructor parameters for dependencies
2. Update DI registration from Transient to Transient with factory
3. Build and test after each file

**Example**:
```csharp
// Before
services.AddTransient<View_Settings_Routing_SettingsOverview>();

// After
services.AddTransient<View_Settings_Routing_SettingsOverview>(sp =>
{
    var viewModel = sp.GetRequiredService<ViewModel_Settings_Routing_SettingsOverview>();
    return new View_Settings_Routing_SettingsOverview(viewModel);
});
```

#### Phase 2: Moderate Views (Estimated: 30 files, 2 hours)
**Scope**: Views with 3-4 dependencies

**Files**:
- Dunnage workflow views
- Receiving workflow views
- Routing wizard steps

**Approach**:
1. Identify all dependencies from Service Locator calls
2. Add constructor parameters
3. Update DI registration
4. Test workflows end-to-end

#### Phase 3: Complex Views (Estimated: 20 files, 2 hours)
**Scope**: Views with 5+ dependencies or special cases

**Files**:
- Dunnage AdminTypesView (7 dependencies)
- Dunnage WorkflowView (5+ dependencies)
- Views with event handlers using Service Locator

**Approach**:
1. Carefully map all dependencies
2. Consider splitting if >7 dependencies (violates Object Calisthenics Rule 8)
3. Refactor to facade pattern if needed

#### Phase 4: ViewModels with Service Locator (Estimated: 10 files, 1 hour)
**Scope**: ViewModels that dynamically create dialogs or services

**Files**:
- ViewModel_Dunnage_AdminInventoryViewModel
- ViewModel_Shared_Base (affects many derived classes)

**Approach**:
1. Use factory pattern for dialogs: `Func<TDialog>`
2. Inject factories in ViewModel constructor
3. Update all derived classes if base class changes

#### Phase 5: View Registration Updates (Estimated: 1 hour)
**Scope**: Update ModuleServicesExtensions to register Views with dependencies

**Approach**:
1. For each refactored View, update registration
2. Use factory pattern in DI registration
3. Ensure all dependencies are resolvable

#### Phase 6: Cleanup (Estimated: 30 minutes)
**Scope**: Remove deprecated GetService method and verify

**Approach**:
1. Remove `[Obsolete]` GetService method from App.xaml.cs
2. Build solution
3. Fix any remaining stragglers
4. Verify zero Service Locator calls

---

## Implementation Guidelines

### Constructor Injection Best Practices

**Rule 1: Explicit Dependencies**
- All dependencies must be constructor parameters
- No hidden dependencies

**Rule 2: Interface Over Implementation**
- Inject interfaces, not concrete classes
- Enables mocking and testing

**Rule 3: Limit Constructor Parameters**
- Maximum 7 parameters (Object Calisthenics Rule 8)
- If >7, refactor to facade or composite pattern

**Rule 4: Null Checks**
- Use `ArgumentNullException.ThrowIfNull()` for C# 11+
- Guard clauses at constructor top

**Rule 5: Immutable Dependencies**
- Use `readonly` fields for injected dependencies
- Dependencies should not change during object lifetime

### DDD Alignment

**Dependency Direction**:
- Views depend on ViewModels (Application Layer)
- ViewModels depend on Services (Domain/Application Layer)
- Services depend on Repositories (Infrastructure Layer)

**No Reverse Dependencies**:
- Domain should never depend on Application or Infrastructure
- Application should never depend on Infrastructure (use interfaces)

### Testing Strategy

**Unit Tests**:
```csharp
[Fact]
public void Constructor_WithNullViewModel_ThrowsArgumentNullException()
{
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new MyView(null!));
}

[Fact]
public void Constructor_WithValidViewModel_SetsDataContext()
{
    // Arrange
    var mockViewModel = new Mock<IMyViewModel>();
    
    // Act
    var view = new MyView(mockViewModel.Object);
    
    // Assert
    Assert.NotNull(view.DataContext);
    Assert.Equal(mockViewModel.Object, view.ViewModel);
}
```

**Integration Tests**:
```csharp
[Fact]
public void View_CanBeResolvedFromDI()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddModuleServices(configuration);
    var provider = services.BuildServiceProvider();
    
    // Act
    var view = provider.GetRequiredService<MyView>();
    
    // Assert
    Assert.NotNull(view);
    Assert.NotNull(view.ViewModel);
}
```

---

## Risk Assessment

### Low Risk
- Simple views with 1-2 dependencies
- Views that are rarely used
- Settings pages

**Mitigation**: Test thoroughly, but low impact if broken

### Medium Risk
- Workflow views (Dunnage, Receiving, Routing)
- Complex dialogs
- Admin views

**Mitigation**: Test workflows end-to-end after each phase

### High Risk
- ViewModel_Shared_Base (affects many derived classes)
- Views in critical user flows
- Dialog creation patterns

**Mitigation**:
- Careful analysis before refactoring
- Extensive testing
- Staged rollout
- Ready rollback plan

---

## Success Criteria

### Quantitative
- [ ] **Zero** `App.GetService<T>()` calls in codebase
- [ ] **100%** of Views use constructor injection
- [ ] **100%** of ViewModels use constructor injection
- [ ] Build succeeds with zero warnings related to Service Locator
- [ ] All 141 instances refactored and tested

### Qualitative
- [ ] Dependencies are explicit and visible
- [ ] Code is testable with mock dependencies
- [ ] SOLID principles followed
- [ ] DDD dependency direction maintained
- [ ] Object Calisthenics rules respected (max 2 instance variables per class*)

\* *Note: ILogger not counted as instance variable per Object Calisthenics exemption*

### Runtime Validation
- [ ] Application starts without errors
- [ ] All workflows function correctly
- [ ] Dialogs open and close properly
- [ ] No null reference exceptions
- [ ] Performance not degraded

---

## Timeline Estimate

| Phase | Files | Estimated Time | Cumulative |
|-------|-------|----------------|------------|
| Phase 1: Simple Views | 20 | 1 hour | 1 hour |
| Phase 2: Moderate Views | 30 | 2 hours | 3 hours |
| Phase 3: Complex Views | 20 | 2 hours | 5 hours |
| Phase 4: ViewModels | 10 | 1 hour | 6 hours |
| Phase 5: DI Registration | All | 1 hour | 7 hours |
| Phase 6: Cleanup & Testing | All | 1 hour | 8 hours |

**Total Estimated Time**: 8 hours (1 full workday)

**Actual Time May Vary**: Expect 10-12 hours with testing and unexpected issues

---

## Rollback Strategy

If critical issues arise:
1. **Git**: Use git to revert to last working commit
2. **Phase-based**: Each phase is a separate commit
3. **Feature Flag**: If needed, keep deprecated GetService temporarily

---

## Post-Implementation

### Documentation Updates
- [ ] Update architecture documentation
- [ ] Document constructor injection pattern for team
- [ ] Update coding guidelines
- [ ] Add examples to developer onboarding

### Future Prevention
- [ ] Add analyzer rule to detect `App.GetService<T>()`
- [ ] Code review checklist item for constructor injection
- [ ] Update PR template to check for Service Locator

---

## References

- **Service Locator is an Anti-Pattern**: https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
- **Dependency Injection Principles**: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
- **SOLID Principles**: https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles
- **Object Calisthenics Rules**: .github/instructions/object-calisthenics.instructions.md

---

**Status**: Ready for Implementation  
**Priority**: High (Technical Debt Reduction)  
**Estimated Effort**: 8-12 hours  
**Risk Level**: Medium  
**Business Value**: Improved code quality, testability, and maintainability

---

*Plan Created: 2024*  
*Author: Someone who understands dependency injection actually matters*  
*Approved By: The SOLID Principles Police*
