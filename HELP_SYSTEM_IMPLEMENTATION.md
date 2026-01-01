# Centralized Help System Implementation Plan

## Overview
Implement a comprehensive, centralized help system to replace hard-coded tips, tooltips, placeholders, and help content throughout the MTM Receiving Application. The system will consist of a help service, help dialog, content model, and integration with all existing views.

**Implementation Context**: This plan is designed for execution using Serena MCP tools in an IDE context (VSCode with Serena integration). The implementation leverages Serena's symbol-level editing capabilities to maximize token efficiency and precision.

---

## Task Checklist

### Format: `[ID] [P?] [Phase] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Phase]**: Which phase this task belongs to
- Check off tasks as completed

---

## Phase 1: Core Infrastructure (Foundation)

**Purpose**: Create models, enums, and service interface

### Preparation
- [ ] T001 [Prep] Verify onboarding complete via `mcp_oraios_serena_check_onboarding_performed`
- [ ] T002 [Prep] Read memories: `dialog_patterns`, `help_content_inventory`, `service_infrastructure`
- [ ] T003 [Prep] Review existing patterns via `get_symbols_overview` on reference files

### Models & Enums
- [ ] T004 [P] [Phase1] Create `Models/Core/Model_HelpContent.cs` (partial class, ObservableObject)
- [ ] T005 [P] [Phase1] Create `Models/Enums/Enum_HelpType.cs` (Tip, Info, Warning, Tutorial, Reference)
- [ ] T006 [P] [Phase1] Create `Models/Enums/Enum_HelpSeverity.cs` (Info, Warning, Critical)

### Service Interface
- [ ] T007 [Phase1] Update `Contracts/Services/IService_Help.cs` - Add ShowHelpAsync methods
- [ ] T008 [Phase1] Add GetHelpContent, GetHelpByCategory, SearchHelp methods to interface
- [ ] T009 [Phase1] Add IsDismissedAsync/SetDismissedAsync methods to interface

### Service Implementation
- [ ] T010 [Phase1] Create `Services/Help/` directory
- [ ] T011 [Phase1] Create `Services/Help/Service_Help.cs` with dependency injection
- [ ] T012 [Phase1] Implement constructor and initialize help content cache
- [ ] T013 [Phase1] Implement basic content retrieval methods (stubs)

**Checkpoint**: Core models and service structure complete, compiles successfully

---

## Phase 2: Help Dialog UI

**Purpose**: Create reusable help dialog with ViewModel

### ViewModel
- [ ] T014 [Phase2] Create `ViewModels/Shared/Shared_HelpDialogViewModel.cs`
- [ ] T015 [Phase2] Add observable properties (HelpContent, IsRelatedHelpAvailable, etc.)
- [ ] T016 [Phase2] Implement ViewRelatedTopicCommand and CopyContentCommand
- [ ] T017 [Phase2] Implement LoadHelpContent method

### XAML View
- [ ] T018 [Phase2] Create `Views/Shared/Shared_HelpDialog.xaml` (ContentDialog)
- [ ] T019 [Phase2] Add header section with icon and title binding
- [ ] T020 [Phase2] Add scrollable content section with RichTextBlock
- [ ] T021 [Phase2] Add related topics section with ListView
- [ ] T022 [Phase2] Add footer with "Don't show again" checkbox

### Code-Behind
- [ ] T023 [Phase2] Create `Views/Shared/Shared_HelpDialog.xaml.cs`
- [ ] T024 [Phase2] Implement ViewModel injection and SetHelpContent method

**Checkpoint**: Help dialog UI complete and can display test content

---

## Phase 3: DI Registration

**Purpose**: Register all new components in dependency injection

- [ ] T025 [P] [Phase3] Register IService_Help as Singleton in `App.xaml.cs` (after IService_Window)
- [ ] T026 [P] [Phase3] Register Shared_HelpDialogViewModel as Transient (ViewModels section)
- [ ] T027 [P] [Phase3] Register Shared_HelpDialog as Transient (Views/Shared section)
- [ ] T028 [Phase3] Build project to verify DI registration compiles
- [ ] T029 [Phase3] Test service resolution via App.GetService<IService_Help>()

**Checkpoint**: All components registered and resolvable via DI

---

## Phase 4: Content Population

**Purpose**: Populate service with all help content

### Dunnage Workflow Content
- [ ] T030 [Phase4] Populate Dunnage.ModeSelection help content (6 keys)
- [ ] T031 [Phase4] Populate Dunnage.TypeSelection help content (2 keys)
- [ ] T032 [Phase4] Populate Dunnage.PartSelection help content (3 keys)
- [ ] T033 [Phase4] Populate Dunnage.QuantityEntry help content (2 keys)
- [ ] T034 [Phase4] Populate Dunnage.DetailsEntry help content (3 keys)
- [ ] T035 [Phase4] Populate Dunnage.Review help content (2 keys)
- [ ] T036 [Phase4] Populate Dunnage.ManualEntry help content (5 keys)
- [ ] T037 [Phase4] Populate Dunnage.EditMode help content (4 keys)

### Receiving Workflow Content
- [ ] T038 [Phase4] Populate Receiving.ModeSelection help content
- [ ] T039 [Phase4] Populate Receiving.POEntry help content (3 keys)
- [ ] T040 [Phase4] Populate Receiving.WeightQuantity help content (2 keys)
- [ ] T041 [Phase4] Populate Receiving.HeatLot help content (2 keys)
- [ ] T042 [Phase4] Populate Receiving workflow remaining help content

### Admin & Dialog Content
- [ ] T043 [Phase4] Populate Admin.Types help content (5 keys)
- [ ] T044 [Phase4] Populate Admin.Parts and Inventory help content
- [ ] T045 [Phase4] Populate Admin.QuickAdd dialog help content (2 keys)

### Tooltips, Placeholders, Tips
- [ ] T046 [Phase4] Implement GetTooltip with all button tooltips (20+ keys)
- [ ] T047 [Phase4] Implement GetPlaceholder with all field placeholders (15+ keys)
- [ ] T048 [Phase4] Implement GetTip with all view tips (8+ keys)

**Checkpoint**: All help content populated and retrievable via service

---

## Phase 5: View Integration - Dunnage

**Purpose**: Replace hard-coded content with service bindings

### Dunnage XAML Updates
- [ ] T049 [P] [Phase5] Update `Dunnage_QuantityEntryView.xaml` - Replace tip text with binding
- [ ] T050 [P] [Phase5] Update `Dunnage_PartSelectionView.xaml` - Replace tooltips and placeholders
- [ ] T051 [P] [Phase5] Update `Dunnage_DetailsEntryView.xaml` - Replace placeholders
- [ ] T052 [P] [Phase5] Update `Dunnage_TypeSelectionView.xaml` - Replace pagination tooltips
- [ ] T053 [P] [Phase5] Update `Dunnage_ReviewView.xaml` - Replace InfoBar content
- [ ] T054 [P] [Phase5] Update `Dunnage_ModeSelectionView.xaml` - Replace quick-access tooltips
- [ ] T055 [P] [Phase5] Update `Dunnage_ManualEntryView.xaml` - Replace operation tooltips
- [ ] T056 [P] [Phase5] Update `Dunnage_EditModeView.xaml` - Replace filter tooltips

### Dunnage ViewModel Updates
- [ ] T057 [Phase5] Update all 8 Dunnage ViewModels - Inject IService_Help
- [ ] T058 [Phase5] Add ShowHelpCommand to each workflow ViewModel
- [ ] T059 [Phase5] Add GetTooltip/GetPlaceholder/GetTip helper properties

**Checkpoint**: Dunnage views display dynamic help content from service

---

## Phase 6: View Integration - Receiving

**Purpose**: Replace hard-coded content with service bindings

### Receiving XAML Updates
- [ ] T060 [P] [Phase6] Update `Receiving_POEntryView.xaml` - Replace placeholders
- [ ] T061 [P] [Phase6] Update `Receiving_WeightQuantityView.xaml` - Replace InfoBar and placeholders
- [ ] T062 [P] [Phase6] Update `Receiving_HeatLotView.xaml` - Replace placeholders
- [ ] T063 [P] [Phase6] Update `Receiving_ModeSelectionView.xaml` - Replace tooltips
- [ ] T064 [P] [Phase6] Update `Receiving_ManualEntryView.xaml` - Replace tooltips
- [ ] T065 [P] [Phase6] Update `Receiving_EditModeView.xaml` - Replace tooltips

### Receiving ViewModel Updates
- [ ] T066 [Phase6] Update all 11 Receiving ViewModels - Inject IService_Help
- [ ] T067 [Phase6] Add ShowHelpCommand to each workflow ViewModel
- [ ] T068 [Phase6] Add helper properties for dynamic content

**Checkpoint**: Receiving views display dynamic help content from service

---

## Phase 7: Workflow Help Integration

**Purpose**: Add contextual help buttons to workflow views

- [ ] T069 [Phase7] Update `Dunnage_WorkflowView.xaml` - Replace TeachingTip with Help button
- [ ] T070 [Phase7] Update `Receiving_WorkflowView.xaml` - Replace TeachingTip with Help button
- [ ] T071 [Phase7] Wire Help button to ShowHelpCommand in ViewModels
- [ ] T072 [Phase7] Implement context-sensitive help based on current workflow step

**Checkpoint**: Help dialogs show correct content for each workflow step

---

## Phase 8: Advanced Features (Optional)

**Purpose**: Enhanced functionality

- [ ] T073 [P] [Phase8] Implement dismissed tips persistence (via IService_UserPreferences)
- [ ] T074 [P] [Phase8] Implement SearchHelp functionality
- [ ] T075 [P] [Phase8] Add F1 keyboard shortcut to all workflow views
- [ ] T076 [P] [Phase8] Implement help search dialog (optional enhancement)

**Checkpoint**: Advanced features operational

---

## Phase 9: Testing & Validation

**Purpose**: Comprehensive testing

### Build Verification
- [ ] T077 [Phase9] Run `dotnet build` - Verify no compilation errors
- [ ] T078 [Phase9] Verify all using statements correct
- [ ] T079 [Phase9] Check XAML compilation (no WMC errors)

### Runtime Testing
- [ ] T080 [Phase9] Test help dialog from each Dunnage workflow step (8 tests)
- [ ] T081 [Phase9] Test help dialog from each Receiving workflow step (11 tests)
- [ ] T082 [Phase9] Verify F1 keyboard shortcut (if implemented)
- [ ] T083 [Phase9] Test "Don't show again" persistence (if implemented)
- [ ] T084 [Phase9] Test related topics navigation
- [ ] T085 [Phase9] Verify all tooltips display correctly
- [ ] T086 [Phase9] Verify all placeholders display correctly

### Content Review
- [ ] T087 [Phase9] Review all help content for accuracy
- [ ] T088 [Phase9] Verify step-by-step instructions are clear
- [ ] T089 [Phase9] Check for typos and grammar
- [ ] T090 [Phase9] Ensure consistent terminology
- [ ] T091 [Phase9] Verify icon choices are appropriate

**Checkpoint**: All tests pass, help system fully functional

---

## Phase 10: Documentation & Cleanup

**Purpose**: Update documentation and create memories

- [ ] T092 [P] [Phase10] Create Serena memory: `help_system_architecture`
- [ ] T093 [P] [Phase10] Update Serena memory: `service_infrastructure`
- [ ] T094 [P] [Phase10] Create Serena memory: `xaml_binding_patterns`
- [ ] T095 [Phase10] Update README.md with help system usage
- [ ] T096 [Phase10] Code cleanup and refactoring
- [ ] T097 [Phase10] Remove hard-coded content remnants

**Checkpoint**: Documentation complete, memories created

---

## Dependencies & Execution Order

### Phase Dependencies
1. **Phase 1 (Core)**: No dependencies - start immediately
2. **Phase 2 (Dialog)**: Depends on Phase 1 completion
3. **Phase 3 (DI)**: Depends on Phase 1 & 2 completion - REQUIRED before runtime testing
4. **Phase 4 (Content)**: Can run parallel with Phase 2, depends on Phase 1
5. **Phase 5 (Dunnage)**: Depends on Phase 3 & 4 completion
6. **Phase 6 (Receiving)**: Depends on Phase 3 & 4 completion (can run parallel with Phase 5)
7. **Phase 7 (Workflow)**: Depends on Phase 5 & 6 completion
8. **Phase 8 (Advanced)**: Depends on Phase 7 completion - OPTIONAL
9. **Phase 9 (Testing)**: Depends on Phase 7 (or 8 if included) completion
10. **Phase 10 (Docs)**: Depends on Phase 9 completion

### Parallel Opportunities
- T004-T006: All model/enum files can be created in parallel
- T025-T027: All DI registrations can be done in parallel
- T030-T048: Content population tasks can run in parallel
- T049-T056: All Dunnage XAML updates can run in parallel
- T060-T065: All Receiving XAML updates can run in parallel
- Phase 5 and Phase 6 can run in parallel (different file sets)
- T092-T094: Memory creation tasks can run in parallel

---

## Quality Gates

**Before marking feature complete, verify:**

### Code Quality
- [ ] All compilation errors resolved
- [ ] No warnings in build output
- [ ] Code follows existing patterns (Serena memories verified)
- [ ] All using statements correct
- [ ] No hard-coded help content remains

### Architecture Compliance
- [ ] **MVVM**: ViewModels contain help logic, Views only bind
- [ ] **Dependency Injection**: IService_Help registered as Singleton
- [ ] **Service Pattern**: Service implements interface correctly
- [ ] **Dialog Pattern**: ContentDialog follows existing patterns
- [ ] **Binding Pattern**: All XAML uses x:Bind (compile-time binding)

### Functionality
- [ ] Help dialog displays from all workflow steps
- [ ] Tooltips display dynamically
- [ ] Placeholders display dynamically
- [ ] Tips display dynamically
- [ ] Related topics navigation works
- [ ] F1 shortcut works (if implemented)
- [ ] Dismissed tips persist (if implemented)

### Content Quality
- [ ] All help content accurate
- [ ] No typos or grammar errors
- [ ] Consistent terminology throughout
- [ ] Examples included where appropriate
- [ ] Icon choices appropriate

### Documentation
- [ ] Serena memories created/updated
- [ ] Implementation plan followed
- [ ] README.md updated (if needed)
- [ ] Code comments added for complex logic

---

## Serena Workflow Guidelines

### Before Starting
1. **Check Onboarding**: Call `mcp_oraios_serena_check_onboarding_performed` to verify project setup
2. **Read Relevant Memories**: Review existing memories: `dialog_patterns`, `help_content_inventory`, `service_infrastructure`
3. **Think Tools**: Use `mcp_oraios_serena_think_about_task_adherence` before major changes

### During Implementation
1. **Symbol-Level Operations**: Use `mcp_oraios_serena_find_symbol`, `mcp_oraios_serena_insert_after_symbol`, `mcp_oraios_serena_replace_symbol_body` instead of reading entire files
2. **Avoid Full File Reads**: Use `mcp_oraios_serena_get_symbols_overview` to understand file structure before editing
3. **Pattern Search**: Use `mcp_oraios_serena_search_for_pattern` for finding text patterns across multiple files
4. **Think Before Editing**: Call `mcp_oraios_serena_think_about_collected_information` after gathering context
5. **Verify Completion**: Call `mcp_oraios_serena_think_about_whether_you_are_done` when finished

### Tool Strategy
- **Find symbols**: `mcp_oraios_serena_find_symbol` with `name_path_pattern` and optional `relative_path` to scope search
- **Insert code**: `mcp_oraios_serena_insert_after_symbol` or `insert_before_symbol` for adding new methods/properties
- **Replace code**: `mcp_oraios_serena_replace_symbol_body` for editing existing method/class bodies
- **Rename refactoring**: `mcp_oraios_serena_rename_symbol` for safe renaming across codebase
- **Content replacement**: `mcp_oraios_serena_replace_content` with regex mode for XAML and text changes

---

## Phase 1: Core Infrastructure

### Preparation Step: Verify Context
**Serena Tools**: `mcp_oraios_serena_check_onboarding_performed`, `mcp_oraios_serena_list_memories`

**Actions**:
1. Verify onboarding is complete
2. Read relevant memories: `dialog_patterns`, `help_content_inventory`, `service_infrastructure`, `mvvm_guide`
3. List directory structure for Models/Core and Models/Enums to understand existing patterns

### Task 1.1: Create Help Content Model
**File**: `Models/Core/Model_HelpContent.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_get_symbols_overview` on `Models/Core/Model_Dao_Result.cs` to understand model pattern
2. Use `mcp_oraios_serena_find_symbol` on `Model_DunnageType` to see MaterialIconKind usage pattern
3. Create new file with proper structure

**Requirements**:
- Create partial class inheriting from `ObservableObject`
- Use `[ObservableProperty]` attributes from CommunityToolkit.Mvvm
- Include the following properties:
  - `Key` (string) - Unique identifier for the help content
  - `Title` (string) - Display title for the help topic
  - `Content` (string) - Main help text content (supports rich text/markdown)
  - `Category` (string) - Category (e.g., "Dunnage", "Receiving", "Admin")
  - `HelpType` (Enum_HelpType) - Type of help (Tip, Info, Warning, Tutorial, Reference)
  - `Icon` (string) - Material Design icon name for visual representation
  - `Severity` (Enum_HelpSeverity) - Severity level (Info, Warning, Critical)
  - `RelatedKeys` (List<string>) - List of related help topic keys
  - `LastUpdated` (DateTime) - When the content was last modified

**Notes**:
- Follow existing model patterns in `Models/Core/` directory
- Use Material.Icons MaterialIconKind for icon parsing (see Model_DunnageType pattern)
- Initialize collections to empty lists, not null
- Add XML documentation comments

---

### Task 1.2: Create Help Enumerations
**File**: `Models/Enums/Enum_HelpType.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` to examine existing enum pattern (e.g., `Enum_DunnageWorkflowStep`)
2. Create new enum files following exact pattern

**Values**:
- Tip = 0 (Quick tips and best practices)
- Info = 1 (Informational content)
- Warning = 2 (Important warnings and cautions)
- Tutorial = 3 (Step-by-step instructions)
- Reference = 4 (Detailed reference material)

**File**: `Models/Enums/Enum_HelpSeverity.cs`

**Values**:
- Info = 0 (General information)
- Warning = 1 (Important to know)
- Critical = 2 (Critical information, must read)

**Notes**:
- Follow existing enum patterns in `Models/Enums/`
- Add XML comments for each value

---

### Task 1.3: Update Help Service Interface
**File**: `Contracts/Services/IService_Help.cs` (already exists)

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` with name_path_pattern `IService_Help` to get current interface body
2. Use `mcp_oraios_serena_insert_before_symbol` targeting the closing brace to add new methods
3. Verify all method signatures match existing service interface patterns

**Methods to Add**:
```
Task ShowHelpAsync(string helpKey)
- Displays help dialog with content for the specified key
- Parameters: helpKey (string) - The unique key for the help content
- Returns: Task for async operation

Task ShowContextualHelpAsync(Enum_DunnageWorkflowStep step)
- Shows help for specific dunnage workflow step
- Parameters: step - The workflow step enum
- Returns: Task

Task ShowContextualHelpAsync(Enum_ReceivingWorkflowStep step)  
- Shows help for specific receiving workflow step
- Parameters: step - The workflow step enum
- Returns: Task

Model_HelpContent? GetHelpContent(string key)
- Retrieves help content by key without showing dialog
- Parameters: key - The unique help key
- Returns: Model_HelpContent if found, null otherwise

List<Model_HelpContent> GetHelpByCategory(string category)
- Gets all help content for a category
- Parameters: category - Category name
- Returns: List of help content items

List<Model_HelpContent> SearchHelp(string searchTerm)
- Searches help content by title or content
- Parameters: searchTerm - Search string
- Returns: List of matching help content items

Task<bool> IsDismissedAsync(string helpKey)
- Checks if user has dismissed a tip/help permanently
- Parameters: helpKey - The help key to check
- Returns: true if dismissed, false otherwise

Task SetDismissedAsync(string helpKey, bool isDismissed)
- Sets whether a tip/help is dismissed
- Parameters: helpKey, isDismissed flag
- Returns: Task

string GetPlaceholder(string fieldKey) [KEEP EXISTING]
string GetTooltip(string elementKey) [KEEP EXISTING]
string GetTip(string viewKey) [KEEP EXISTING]
```

**Notes**:
- Keep all existing method signatures
- Add XML documentation for all new methods
- Methods should not throw exceptions - return null/empty on errors

---

### Task 1.4: Create Help Service Implementation
**File**: `Services/Help/Service_Help.cs` (new directory: Services/Help/)

**Serena Approach**:
1. Create new directory using `mcp_oraios_serena_list_dir` to verify structure
2. Use `mcp_oraios_serena_find_symbol` on existing service (e.g., `Service_DunnageWorkflow`) to understand pattern
3. Create complete service file with proper dependency injection pattern

**Dependencies to Inject**:
- `IService_Window` - For getting XamlRoot
- `IService_LoggingUtility` - For logging operations
- `IService_Dispatcher` - For UI thread operations

**Constructor**:
- Accept all three dependencies
- Initialize internal help content dictionary
- Load all help content into memory (call private InitializeHelpContent method)

**Private Fields**:
- `Dictionary<string, Model_HelpContent> _helpContentCache` - In-memory cache of all help
- `HashSet<string> _dismissedTips` - Set of dismissed help keys (could be loaded from preferences)

**Implementation Notes**:
- InitializeHelpContent method should populate the dictionary with all help content
- Use switch statements or dictionary initialization for content
- Group content by category (Dunnage, Receiving, Admin, etc.)
- For ShowHelpAsync methods:
  1. Retrieve content from cache
  2. If not found, log error and return
  3. Create HelpDialog instance via App.GetService
  4. Set dialog's HelpContent property
  5. Get XamlRoot from IService_Window
  6. Set dialog.XamlRoot
  7. Call dialog.ShowAsync()
  8. Log the help display event

**Content Structure Example**:
Create static content for all existing tips, tooltips, and help from codebase:
- Dunnage workflow steps (8 steps)
- Receiving workflow steps (11 steps)
- All tooltips from search results
- All placeholders from search results
- All InfoBar messages
- Admin section help
- Dialog-specific help

---

## Phase 2: Help Dialog UI

### Task 2.1: Create Help Dialog ViewModel
**File**: `ViewModels/Shared/Shared_HelpDialogViewModel.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` on `Shared_BaseViewModel` to understand constructor pattern
2. Use `mcp_oraios_serena_find_symbol` on existing ViewModel (e.g., `Dunnage_TypeSelectionViewModel`) for command pattern
3. Create new ViewModel file following exact patterns

**Base Class**: Inherit from `Shared_BaseViewModel`

**Constructor Parameters**:
- `IService_ErrorHandler errorHandler`
- `IService_LoggingUtility logger`
- Call base constructor

**Observable Properties**:
- `HelpContent` (Model_HelpContent) - The help content to display
- `IsRelatedHelpAvailable` (bool) - Whether there are related topics
- `RelatedTopics` (ObservableCollection<Model_HelpContent>) - Related help topics
- `CanDismiss` (bool) - Whether "Don't show again" option is available
- `IsDismissed` (bool) - Whether user has dismissed this help

**Commands**:
- `ViewRelatedTopicCommand` - Takes Model_HelpContent parameter, loads new content
- `CopyContentCommand` - Copies help content to clipboard

**Methods**:
- `LoadHelpContent(Model_HelpContent content)` - Sets HelpContent and loads related topics
- `OnDismissedChanged` - Partial method for property change handling

**Notes**:
- Use CommunityToolkit.Mvvm `[ObservableProperty]` and `[RelayCommand]` attributes
- Initialize collections in constructor
- Add XML documentation

---

### Task 2.2: Create Help Dialog XAML
**File**: `Views/Shared/Shared_HelpDialog.xaml`

**Serena Approach**:
1. Use `mcp_oraios_serena_search_for_pattern` to find existing ContentDialog XAML pattern
2. Read `Views/Shared/Shared_SharedTerminalLoginDialog.xaml` as reference
3. Create new XAML file following exact namespace and structure patterns
4. Use `x:Bind` for all bindings (Mode=OneWay for display, Mode=TwoWay for input)

**Root Element**: `ContentDialog`

**Properties**:
- Namespace declarations (include Material.Icons.WinUI3 as `mi`)
- `x:Class="MTM_Receiving_Application.Views.Shared.Shared_HelpDialog"`
- `Title="{x:Bind ViewModel.HelpContent.Title, Mode=OneWay}"`
- `PrimaryButtonText="Close"`
- `DefaultButton="Primary"`
- `Width="700"`
- `Height="600"`

**Layout Structure**:
1. **Header Section**:
   - StackPanel with Orientation="Horizontal"
   - Material Icon using `{x:Bind ViewModel.HelpContent.Icon, Mode=OneWay}`
   - Title TextBlock (large, bold)
   - Severity badge (InfoBadge or colored border based on HelpType)

2. **Content Section**:
   - ScrollViewer with VerticalScrollBarVisibility="Auto"
   - RichTextBlock or TextBlock for content display
   - Support line breaks and formatting
   - Use `{x:Bind ViewModel.HelpContent.Content, Mode=OneWay}`

3. **Related Topics Section** (Conditional visibility):
   - Visible when `{x:Bind ViewModel.IsRelatedHelpAvailable, Mode=OneWay}`
   - Expander control with Header="Related Topics"
   - ListView bound to `{x:Bind ViewModel.RelatedTopics, Mode=OneWay}`
   - ItemTemplate showing icon and title
   - Item click triggers ViewRelatedTopicCommand

4. **Footer Section**:
   - StackPanel with Orientation="Horizontal", HorizontalAlignment="Stretch"
   - CheckBox "Don't show this again" - Visible when `{x:Bind ViewModel.CanDismiss, Mode=OneWay}`
   - Bound to `{x:Bind ViewModel.IsDismissed, Mode=TwoWay}`
   - Copy button (optional) - Triggers CopyContentCommand

**Resources**:
- Add EmptyStringToVisibilityConverter if needed
- Style definitions for consistent appearance

**Notes**:
- Follow WinUI 3 ContentDialog patterns from existing dialogs
- Use `x:Bind` for all bindings (compile-time)
- Use Mode=OneWay for read-only, Mode=TwoWay for user input
- Match styling from other application dialogs

---

### Task 2.3: Create Help Dialog Code-Behind
**File**: `Views/Shared/Shared_HelpDialog.xaml.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` on `Shared_SharedTerminalLoginDialog` class to see code-behind pattern
2. Create minimal code-behind following exact pattern (ViewModel retrieval, InitializeComponent)

**Class Declaration**:
- `public sealed partial class Shared_HelpDialog : ContentDialog`

**Properties**:
- `public Shared_HelpDialogViewModel ViewModel { get; }`

**Constructor**:
```
public Shared_HelpDialog()
{
    ViewModel = App.GetService<Shared_HelpDialogViewModel>();
    InitializeComponent();
}
```

**Methods**:
- `public void SetHelpContent(Model_HelpContent content)` - Calls ViewModel.LoadHelpContent
- Optional: Event handlers for dialog closing if needed

**Notes**:
- Minimal code-behind - delegate to ViewModel
- No business logic in code-behind
- Follow existing dialog patterns from Shared_SharedTerminalLoginDialog

---

## Phase 3: DI Registration

### Task 3.1: Register Help Service
**File**: `App.xaml.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_search_for_pattern` with pattern `services\.AddSingleton<IService_` to find service registration section
2. Use `mcp_oraios_serena_insert_after_symbol` or `replace_content` with regex to add registration after IService_Window
3. Call `mcp_oraios_serena_think_about_collected_information` to verify correct placement

**Location**: In ConfigureServices method, after IService_Window registration, before ViewModels

**Code**:
```
services.AddSingleton<IService_Help, Service_Help>();
```

**Notes**:
- Add in the "Services" section (around line 100-117)
- Add using statement for Services.Help namespace if needed
- Verify build succeeds after registration

---

### Task 3.2: Register Help Dialog Components
**File**: `App.xaml.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_search_for_pattern` to find ViewModel and Dialog registration sections
2. Use `mcp_oraios_serena_replace_content` with regex mode to add registrations in appropriate sections

**Location**: In ViewModels section

**Code**:
```
services.AddTransient<Shared_HelpDialogViewModel>();
```

**Location**: In Views/Shared section

**Code**:
```
services.AddTransient<Views.Shared.Shared_HelpDialog>();
```

**Notes**:
- Add ViewModel registration around line 121-124 (with other Shared ViewModels)
- Add Dialog registration around line 177-179 (with other Shared dialogs)

---

## Phase 4: Content Population

### Task 4.1: Populate Dunnage Workflow Help
**File**: `Services/Help/Service_Help.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` with name_path_pattern `InitializeHelpContent` to locate method
2. Use `mcp_oraios_serena_replace_symbol_body` to update method with all help content
3. Reference existing `help_content_inventory` memory for content to migrate

**Method**: `InitializeHelpContent`

**Content Keys to Create**:
1. `Dunnage.ModeSelection` - Explain Guided vs Manual vs Edit modes
2. `Dunnage.ModeSelection.Guided` - When to use Guided Wizard
3. `Dunnage.ModeSelection.Manual` - When to use Manual Entry
4. `Dunnage.ModeSelection.Edit` - When to use Edit Mode
5. `Dunnage.TypeSelection` - How to select dunnage types, pagination
6. `Dunnage.TypeSelection.AddNew` - How to add new types
7. `Dunnage.PartSelection` - Part selection process
8. `Dunnage.PartSelection.AddNew` - How to add new parts
9. `Dunnage.PartSelection.Inventory` - Inventory vs non-inventory parts
10. `Dunnage.QuantityEntry` - Quantity entry explanation
11. `Dunnage.QuantityEntry.Labels` - Label generation process
12. `Dunnage.DetailsEntry` - Details entry fields
13. `Dunnage.DetailsEntry.PONumber` - PO number format
14. `Dunnage.DetailsEntry.Location` - Location codes
15. `Dunnage.Review` - Review process
16. `Dunnage.Review.Save` - What happens after save
17. `Dunnage.ManualEntry` - Manual entry mode overview
18. `Dunnage.ManualEntry.BulkAdd` - Add multiple rows feature
19. `Dunnage.ManualEntry.AutoFill` - Auto-fill feature
20. `Dunnage.ManualEntry.FillBlanks` - Fill blank spaces feature
21. `Dunnage.ManualEntry.Sort` - Sorting for printing
22. `Dunnage.EditMode` - Edit mode overview
23. `Dunnage.EditMode.LoadSession` - Load from session memory
24. `Dunnage.EditMode.LoadLabels` - Load from CSV export
25. `Dunnage.EditMode.LoadHistory` - Load historical data
26. `Dunnage.EditMode.DateFilter` - Date filtering options

**Content Structure**:
```
new Model_HelpContent
{
    Key = "Dunnage.ModeSelection",
    Title = "Select Entry Mode",
    Content = "Choose how you want to enter dunnage receiving data:\n\n• Guided Wizard - Step-by-step process for single transactions\n• Manual Entry - Bulk grid entry for multiple transactions\n• Edit Mode - Review and edit historical data",
    Category = "Dunnage Workflow",
    HelpType = Enum_HelpType.Info,
    Icon = "HelpCircle",
    Severity = Enum_HelpSeverity.Info,
    RelatedKeys = new List<string> { "Dunnage.ModeSelection.Guided", "Dunnage.ModeSelection.Manual", "Dunnage.ModeSelection.Edit" },
    LastUpdated = DateTime.Now
}
```

**Notes**:
- Use existing tip/tooltip content as basis
- Expand with detailed explanations
- Include step-by-step instructions where appropriate
- Add related keys to link topics
- Use appropriate icons from Material Design Icons

---

### Task 4.2: Populate Receiving Workflow Help
**File**: `Services/Help/Service_Help.cs`

**Method**: `InitializeHelpContent`

**Content Keys to Create**:
1. `Receiving.ModeSelection` - Mode selection overview
2. `Receiving.POEntry` - PO number entry
3. `Receiving.POEntry.Format` - PO number format rules
4. `Receiving.POEntry.InforVisual` - ERP integration explanation
5. `Receiving.PartSelection` - Part selection from PO
6. `Receiving.LoadEntry` - Load number entry
7. `Receiving.WeightQuantity` - Weight vs quantity selection
8. `Receiving.WeightQuantity.Units` - Unit of measure
9. `Receiving.HeatLot` - Heat/lot number entry
10. `Receiving.HeatLot.Format` - Heat/lot format requirements
11. `Receiving.PackageType` - Package type selection
12. `Receiving.Review` - Review before save
13. `Receiving.Review.CSV` - CSV export process
14. `Receiving.Review.Labels` - Label printing
15. `Receiving.ManualEntry` - Manual entry overview
16. `Receiving.EditMode` - Edit mode overview

**Notes**:
- Similar structure to dunnage help
- Include Infor Visual integration details
- Explain weight vs quantity clearly
- Add format examples for PO numbers

---

### Task 4.3: Populate Admin Help
**File**: `Services/Help/Service_Help.cs`

**Content Keys to Create**:
1. `Admin.Types` - Type management overview
2. `Admin.Types.Create` - Creating new types
3. `Admin.Types.Edit` - Editing existing types
4. `Admin.Types.Delete` - Deleting types and impact
5. `Admin.Types.Icon` - Icon selection
6. `Admin.Types.Specs` - Specification fields
7. `Admin.Parts` - Part management overview
8. `Admin.Parts.Search` - Search capabilities
9. `Admin.Inventory` - Inventory management
10. `Admin.QuickAdd.Type` - Quick add type dialog help
11. `Admin.QuickAdd.Part` - Quick add part dialog help

---

### Task 4.4: Populate Tooltips and Placeholders
**File**: `Services/Help/Service_Help.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` to locate `GetTooltip`, `GetPlaceholder`, `GetTip` methods
2. Use `mcp_oraios_serena_replace_symbol_body` to implement switch/dictionary logic
3. Extract all existing hard-coded strings from XAML using `mcp_oraios_serena_search_for_pattern`

**Methods**: `GetTooltip`, `GetPlaceholder`, `GetTip`

**Implementation**:
Create switch statements or dictionaries returning strings:

**Tooltips**:
- "Button.Refresh" → "Refresh parts list"
- "Button.FirstPage" → "First Page"
- "Button.PreviousPage" → "Previous Page"
- "Button.NextPage" → "Next Page"
- "Button.LastPage" → "Last Page"
- "Button.QuickAccess.Guided" → "Skip mode selection and go directly to Guided Wizard"
- And all others from help_content_inventory memory

**Placeholders**:
- "Field.PONumber" → "Enter PO (e.g., 66868 or PO-066868)"
- "Field.PartID" → "Enter Part ID (e.g., MMC-001, MMF-456)"
- "Field.Quantity" → "Enter quantity..."
- "Field.Location" → "Enter location..."
- And all others from help_content_inventory memory

**Tips**:
- "View.QuantityEntry" → Current tip text
- And all others from help_content_inventory memory

**Notes**:
- Return empty string if key not found
- Log warning if key not found
- Keep content concise for tooltips
- Use examples in placeholders

---

## Phase 5: View Integration

### Task 5.1: Update Dunnage Workflow Views
**Files to Update**:
- `Views/Dunnage/Dunnage_QuantityEntryView.xaml`
- `Views/Dunnage/Dunnage_PartSelectionView.xaml`
- `Views/Dunnage/Dunnage_DetailsEntryView.xaml`
- `Views/Dunnage/Dunnage_TypeSelectionView.xaml`
- `Views/Dunnage/Dunnage_ReviewView.xaml`
- `Views/Dunnage/Dunnage_ModeSelectionView.xaml`
- `Views/Dunnage/Dunnage_ManualEntryView.xaml`
- `Views/Dunnage/Dunnage_EditModeView.xaml`

**Serena Approach**:
1. Use `mcp_oraios_serena_search_for_pattern` to find all hard-coded `PlaceholderText="`, `ToolTipService.ToolTip="`, tip `TextBlock` elements
2. Use `mcp_oraios_serena_replace_content` with regex mode to replace literal strings with ViewModel bindings
3. Pattern: Replace `PlaceholderText="literal"` with `PlaceholderText="{x:Bind ViewModel.GetPlaceholder('Key'), Mode=OneWay}"`
4. Pattern: Replace `ToolTipService.ToolTip="literal"` with binding to ViewModel method

**Changes for Each View**:
1. Replace hard-coded tip TextBlocks:
   - Change Text binding from literal to: `{x:Bind ViewModel.TipText, Mode=OneWay}`
   
2. Replace hard-coded tooltips:
   - Change ToolTipService.ToolTip from literal to: `{x:Bind ViewModel.GetTooltip('Button.Name'), Mode=OneWay}`
   
3. Replace hard-coded placeholders:
   - Change PlaceholderText from literal to: `{x:Bind ViewModel.GetPlaceholder('Field.Name'), Mode=OneWay}`

4. Add Help button:
   - In workflow views, bind to ViewModel.ShowHelpCommand
   - Icon: HelpCircle or similar
   - ToolTip: "Click for help about current step"

**Notes**:
- Keep existing layout structure
- Only replace content sources
- Verify bindings compile with x:Bind

---

### Task 5.2: Update Receiving Workflow Views
**Files to Update**:
- Similar pattern to Task 5.1 for all Receiving views
- `Views/Receiving/Receiving_POEntryView.xaml`
- `Views/Receiving/Receiving_WeightQuantityView.xaml`
- `Views/Receiving/Receiving_HeatLotView.xaml`
- `Views/Receiving/Receiving_ModeSelectionView.xaml`
- `Views/Receiving/Receiving_ManualEntryView.xaml`
- `Views/Receiving/Receiving_EditModeView.xaml`

**Same Changes as Task 5.1**

---

### Task 5.3: Update ViewModel Base Class (Optional)
**File**: `ViewModels/Shared/Shared_BaseViewModel.cs`

**Serena Approach**:
1. Use `mcp_oraios_serena_find_symbol` with name_path_pattern `Shared_BaseViewModel` and `include_body=true`
2. Use `mcp_oraios_serena_insert_after_symbol` to add new field after `_logger` field
3. Use `mcp_oraios_serena_replace_symbol_body` on constructor to add new parameter
4. Use `mcp_oraios_serena_insert_after_symbol` to add helper methods after constructor

**Add Protected Field**:
```
protected readonly IService_Help _helpService;
```

**Update Constructor**:
Add IService_Help parameter, store in field

**Add Protected Method**:
```
protected string GetTooltip(string key) => _helpService.GetTooltip(key);
protected string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);
protected string GetTip(string key) => _helpService.GetTip(key);
```

**Notes**:
- This is OPTIONAL - allows ViewModels to access help without direct service injection
- Alternative: Each ViewModel injects IService_Help directly
- Decide based on code consistency preference

---

### Task 5.4: Update Workflow ViewModels
**Files to Update**:
- All Dunnage workflow ViewModels (8 files)
- All Receiving workflow ViewModels (11 files)

**Serena Approach**:
1. Use `mcp_oraios_serena_find_file` with pattern `*ViewModel.cs` in `ViewModels/Dunnage` and `ViewModels/Receiving`
2. For each ViewModel:
   - Use `mcp_oraios_serena_find_symbol` to get constructor
   - Use `mcp_oraios_serena_replace_symbol_body` on constructor to add IService_Help parameter
   - Use `mcp_oraios_serena_insert_after_symbol` to add ShowHelpCommand after other commands
3. Use `mcp_oraios_serena_find_referencing_symbols` if needed to understand ViewModel dependencies

**Changes for Each ViewModel**:
1. Inject IService_Help in constructor (if not using BaseViewModel approach)
2. Add ShowHelpCommand:
   ```
   [RelayCommand]
   private async Task ShowHelpAsync()
   {
       await _helpService.ShowContextualHelpAsync([CurrentWorkflowStep]);
   }
   ```
3. Add properties for dynamic content (if not using methods):
   ```
   public string TipText => _helpService.GetTip("View.[ViewName]");
   ```

**Notes**:
- Use appropriate workflow step from ViewModel context
- For Dunnage: Use `Enum_DunnageWorkflowStep`
- For Receiving: Use `Enum_ReceivingWorkflowStep`
- Ensure ViewModel has reference to current step

---

## Phase 6: Advanced Features

### Task 6.1: Implement Dismissed Tips Persistence
**File**: `Services/Help/Service_Help.cs`

**Storage Strategy**:
Option A: Use IService_UserPreferences to store dismissed list
Option B: Create dedicated table in MySQL database
Option C: Store in JSON file in AppData

**Implementation**:
1. Load dismissed tips on service initialization
2. Update IsDismissedAsync to check against storage
3. Update SetDismissedAsync to persist to storage
4. In ShowHelpAsync, check if dismissed before showing

**Notes**:
- Decide on storage strategy based on existing patterns
- User preferences are per-user, not global
- Consider offline scenario

---

### Task 6.2: Implement Help Search
**File**: `Services/Help/Service_Help.cs`

**Method**: `SearchHelp(string searchTerm)`

**Implementation**:
1. Search through all help content in _helpContentCache
2. Match against Title and Content properties
3. Use case-insensitive string contains
4. Return ordered by relevance (exact title match first, then content match)
5. Limit results to top 20

**Optional Enhancement**:
- Create separate search dialog
- Add search box to main help dialog
- Implement keyword highlighting in results

---

### Task 6.3: Add Keyboard Shortcut (F1)
**Files to Update**:
- All main workflow views

**Implementation**:
1. In XAML, add KeyboardAccelerator to root element:
   ```xml
   <Page.KeyboardAccelerators>
       <KeyboardAccelerator Key="F1" Invoked="OnHelpKeyPressed"/>
   </Page.KeyboardAccelerators>
   ```

2. In code-behind:
   ```csharp
   private async void OnHelpKeyPressed(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
   {
       await ViewModel.ShowHelpCommand.ExecuteAsync(null);
       args.Handled = true;
   }
   ```

**Notes**:
- F1 is standard help shortcut
- Ensure keyboard accelerator doesn't conflict with existing shortcuts
- Mark args.Handled = true to prevent event bubbling

---

### Task 6.4: Add Help Icon to Workflow Progress
**Files to Update**:
- `Views/Dunnage/Dunnage_WorkflowView.xaml`
- `Views/Receiving/Receiving_WorkflowView.xaml`

**Current State**:
Both views have help button with TeachingTip showing help content

**Changes**:
1. Replace TeachingTip with ShowHelpCommand binding
2. Update button click handler to call help service
3. Remove hard-coded help content from XAML
4. Help content comes from service based on current workflow step

**Notes**:
- Preserve existing button location and styling
- Ensure icon is Material Design HelpCircle or similar
- Button should be visible on all workflow steps

---

## Phase 7: Testing & Validation

### Task 7.1: Build Verification
1. Run `dotnet build` to verify no compilation errors
2. Verify all using statements are correct
3. Check for XAML compilation errors
4. Verify DI registration is correct

### Task 7.2: Runtime Testing
1. Test help dialog display from each workflow step
2. Verify F1 keyboard shortcut works
3. Test "Don't show again" checkbox persistence
4. Test related topics navigation
5. Verify all tooltips display correctly
6. Verify all placeholders display correctly
7. Test help content readability and formatting

### Task 7.3: Content Review
1. Review all help content for accuracy
2. Verify step-by-step instructions are clear
3. Check for typos and grammar
4. Ensure consistent terminology
5. Verify icon choices are appropriate

---

## Implementation Order Recommendation

### Recommended Serena Workflow

1. **Phase 1 (Core)**: Models, Enums, Service Interface
   - Use `mcp_oraios_serena_think_about_task_adherence` before starting
   - Create files following existing patterns discovered via `find_symbol`
   - Verify compilation after each file creation

2. **Phase 3 (DI)**: Registration
   - Critical: Must be done before runtime testing
   - Use `search_for_pattern` to find exact registration locations
   - Test that service can be resolved via DI

3. **Phase 1.4 (Service)**: Implement Service_Help
   - Create with basic content structure first
   - Use `mcp_oraios_serena_replace_symbol_body` to populate content iteratively

4. **Phase 2 (Dialog)**: Create HelpDialog UI
   - Reference existing dialog patterns via `find_symbol`
   - Build incrementally: ViewModel → XAML → Code-behind

5. **Phase 4 (Content)**: Populate all help content
   - Extract existing content via `search_for_pattern` 
   - Migrate to service using `replace_symbol_body`
   - Call `think_about_collected_information` after gathering all content

6. **Phase 5 (Integration)**: Update views and ViewModels
   - Use batch operations with `replace_content` regex mode for XAML
   - Update ViewModels systematically using `find_file` + `replace_symbol_body`
   - Verify bindings compile after each change

7. **Phase 6 (Advanced)**: Add advanced features
   - Implement after core functionality is working

8. **Phase 7 (Testing)**: Comprehensive testing
   - Use `think_about_whether_you_are_done` before declaring complete

### Efficiency Tips
- **Minimize File Reads**: Always use `get_symbols_overview` before `find_symbol` with `include_body=true`
- **Batch Similar Operations**: Use `find_file` to get all target files, then process with same edit pattern
- **Use Regex Mode**: For XAML string replacement, `replace_content` with regex is most efficient
- **Scope Searches**: Always provide `relative_path` when you know the location
- **Think Tools**: Use thinking tools at decision points to maintain context

---

## File Checklist

### New Files to Create
- [ ] Models/Core/Model_HelpContent.cs
- [ ] Models/Enums/Enum_HelpType.cs
- [ ] Models/Enums/Enum_HelpSeverity.cs
- [ ] Services/Help/Service_Help.cs (new directory)
- [ ] ViewModels/Shared/Shared_HelpDialogViewModel.cs
- [ ] Views/Shared/Shared_HelpDialog.xaml
- [ ] Views/Shared/Shared_HelpDialog.xaml.cs

### Existing Files to Modify
- [ ] Contracts/Services/IService_Help.cs (add methods)
- [ ] App.xaml.cs (DI registration)
- [ ] ViewModels/Shared/Shared_BaseViewModel.cs (optional - add help service)
- [ ] All Dunnage workflow ViewModels (8 files)
- [ ] All Dunnage workflow Views (8 files)
- [ ] All Receiving workflow ViewModels (11 files)
- [ ] All Receiving workflow Views (11 files)
- [ ] Views/Dunnage/Dunnage_WorkflowView.xaml
- [ ] Views/Receiving/Receiving_WorkflowView.xaml

---

## Dependencies & References

### Required NuGet Packages (Already Installed)
- CommunityToolkit.Mvvm (for ObservableProperty, RelayCommand)
- Material.Icons.WinUI3 (for Material Design icons)
- Microsoft.Extensions.DependencyInjection (for DI)

### Key Classes to Reference
- `Shared_BaseViewModel` - Base class for all ViewModels
- `App.GetService<T>()` - Service retrieval method
- `IService_Window.GetXamlRoot()` - For ContentDialog XamlRoot
- `Enum_DunnageWorkflowStep` - Dunnage workflow steps
- `Enum_ReceivingWorkflowStep` - Receiving workflow steps
- `MaterialIconKind` - Icon enum from Material.Icons

### Critical Patterns to Follow
- All ViewModels must be partial classes
- All ViewModels inherit from Shared_BaseViewModel
- All async methods use try-catch-finally with IsBusy flag
- All DAOs return Model_Dao_Result or Model_Dao_Result<T>
- All ContentDialogs require XamlRoot to be set
- All XAML bindings use x:Bind (compile-time), not Binding (runtime)
- All property changes use [ObservableProperty] attribute
- All commands use [RelayCommand] attribute

---

## Notes for Implementation with Serena

### Serena Tool Selection Guide

1. **When to use `find_symbol`**:
   - Finding specific classes, methods, properties by name
   - Understanding existing code patterns before creating similar code
   - Getting symbol body for editing
   - Use with `substring_matching=true` when you don't know exact name

2. **When to use `get_symbols_overview`**:
   - Understanding file structure before detailed reading
   - Getting list of all top-level symbols in a file
   - Determining what needs to be edited
   - Use with `depth=1` to see children (e.g., methods in a class)

3. **When to use `search_for_pattern`**:
   - Finding hard-coded strings in XAML files
   - Locating all instances of a pattern across multiple files
   - Discovering existing tooltips, placeholders, tips
   - Use with `restrict_search_to_code_files=false` for XAML
   - Use `context_lines_before/after` to see surrounding context

4. **When to use `replace_content`**:
   - Bulk string replacement in XAML
   - Changing hard-coded text to bindings
   - Use `mode="regex"` with wildcards to avoid specifying exact text
   - Use `allow_multiple_occurrences=true` cautiously

5. **When to use `insert_after_symbol` / `insert_before_symbol`**:
   - Adding new methods to a class
   - Adding new properties to a ViewModel
   - Inserting using statements at top of file
   - Inserting DI registrations in specific order

6. **When to use `replace_symbol_body`**:
   - Completely replacing a method implementation
   - Updating constructor with new parameters
   - Changing class body while preserving signature

7. **When to use `rename_symbol`**:
   - Renaming a class, method, or property across entire codebase
   - Ensures all references are updated
   - Language server-based, safe refactoring

8. **When to use `find_referencing_symbols`**:
   - Understanding how a method/class is used
   - Finding all call sites before making breaking changes
   - Discovering dependencies

9. **When to use thinking tools**:
   - `think_about_task_adherence`: Before major changes, ensure still on track
   - `think_about_collected_information`: After gathering context, verify it's sufficient
   - `think_about_whether_you_are_done`: Before finishing, confirm all requirements met

### Token Efficiency Best Practices

1. **Avoid Full File Reads**: Use symbol-level tools instead of reading entire files
2. **Scope Your Searches**: Always provide `relative_path` when location is known
3. **Use Overviews First**: Call `get_symbols_overview` before `find_symbol` with body
4. **Batch Operations**: Process multiple similar files with same pattern
5. **Leverage Memory**: Read project memories for context instead of re-discovering patterns
6. **Think Before Acting**: Use thinking tools to plan before executing

### Pattern Discovery Strategy

Before creating any new code:
1. Use `find_symbol` to find similar existing code
2. Examine pattern in similar files (e.g., other ViewModels, Services, Models)
3. Extract patterns for: constructors, properties, methods, DI registration
4. Apply same pattern to new code
5. This ensures consistency and correctness

### Error Handling

When Serena tool calls fail:
1. **Read the error message carefully** - It often indicates the specific issue
2. **Verify symbol names** - Check spelling and casing are exact
3. **Check file paths** - Ensure paths are relative to project root
4. **Verify context** - Some tools have been collected before using
5. **Try alternative approach** - If `find_symbol` fails, try `search_for_pattern`

### Build Verification Strategy

After each phase:
1. Create or update files
2. Run build to check for compilation errors
3. Fix any errors before proceeding
4. Use `mcp_oraios_serena_think_about_collected_information` to reflect on issues

---

## Serena Memory Updates

After completion, update these memories:

### Create New Memory: `help_system_architecture`
Document the completed help system architecture:
- Service structure and content organization
- Dialog patterns and usage
- Integration points with ViewModels and Views
- How to add new help content
- How to update existing content

### Update Memory: `service_infrastructure`
Add section on Help Service:
- Registration pattern
- Dependency injection
- Content management strategy
- Dialog display pattern

### Create Memory: `xaml_binding_patterns`
Document binding patterns used:
- Dynamic content binding to ViewModels
- x:Bind syntax for tooltips, placeholders, tips
- Method binding vs property binding
- When to use Mode=OneWay vs Mode=TwoWay

---

## Critical Patterns to Follow
   - Never throw exceptions in service methods
   - Return null or empty results on errors
   - Log errors via IService_LoggingUtility
   - Use IService_ErrorHandler for user-facing errors

4. **Null Safety**:
   - Initialize all collections (Lists, ObservableCollections)
   - Use null-conditional operators (?)
   - Check for null before accessing properties

5. **Async Patterns**:
   - All async methods end with "Async"
   - Always use try-catch-finally
   - Set IsBusy flag in try/finally blocks

6. **Material Icons**:
   - Icon names are strings (e.g., "HelpCircle", "Information")
   - Parse to MaterialIconKind enum using Enum.TryParse
   - Default to PackageVariantClosed or HelpCircle if parsing fails

7. **Help Content Quality**:
   - Write for manufacturing users, not developers
   - Use simple, clear language
   - Include examples where appropriate
   - Keep tooltips under 100 characters
   - Keep tips under 200 characters
   - Help content can be longer with step-by-step instructions

8. **Testing Strategy**:
   - Test each component in isolation first
   - Verify DI registration before runtime testing
   - Build frequently to catch compilation errors early
   - Test keyboard shortcuts separately from mouse clicks

9. **Content Organization**:
   - Group related help topics by category
   - Use consistent key naming (Category.Topic.SubTopic)
   - Link related topics via RelatedKeys property
   - Order content logically (mode selection → details → completion)

10. **Performance**:
    - All help content loaded once at startup (singleton service)
    - In-memory cache for fast retrieval
    - No database queries for help content (unless implementing persistence)
    - Lazy-load related topics only when needed
