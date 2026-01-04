---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments: ['_bmad-output/planning-artifacts/prd.md', 'docs/architecture.md', 'specs/001-routing-module/wizard-workflow-design.md', '_bmad-output/analysis/brainstorming-session-2026-01-04.md']
workflowType: 'architecture'
project_name: 'MTM_Receiving_Application'
user_name: 'John Koll'
date: '2026-01-04'
---

## Project Context Analysis

### Architectural Scope
*   **Core Challenge:** Transitioning from a legacy/undefined state to a structured "Greenfield" module within an existing Modular Monolith.
*   **Critical Constraint:**
    *   **MySQL:** Must maintain strict MVVM separation and use Stored Procedures for all data access.
    *   **Infor Visual (SQL Server):** Read-only access via raw SQL scripts located in `Database/InforVisualScripts/Queries/Routing/`.
*   **Complexity:** Moderate. The "Package First" wizard requires state management across steps, and the "Validation Toggle" introduces a global state that affects multiple views.
*   **Integration:** Needs to plug seamlessly into the existing `MainWindow` navigation and `App.xaml.cs` DI container.

### Project Scale Assessment
*   **Real-time:** No (Standard CRUD).
*   **Multi-tenancy:** No (Single tenant, multi-user).
*   **Compliance:** Internal manufacturing standards.
*   **Data Complexity:** Low (Normalized relational data).
*   **Interaction:** High (Wizard flow, Quick Add buttons, Smart Sorting).

## 2. Starter Architecture

### Selected Pattern: MTM Module Standard
We will strictly follow the existing Modular Monolith pattern used in `Module_Receiving` and `Module_Dunnage`.

### Module Structure
```
Module_Routing/
├── Data/               # DAOs (Dao_Routing_Label, Dao_Routing_Recipient)
├── Models/             # Domain Entities (Model_Routing_Label, Model_Routing_Recipient)
├── Services/           # Business Logic (Service_Routing_Workflow, Service_Routing_Wizard)
├── ViewModels/         # Presentation Logic
│   ├── ViewModel_Routing_Workflow.cs       # Shell ViewModel
│   ├── ViewModel_Routing_ModeSelection.cs  # Entry Point
│   ├── ViewModel_Routing_Step1.cs          # Package Details
│   ├── ViewModel_Routing_Step2.cs          # Recipient Selection
│   ├── ViewModel_Routing_Step3.cs          # Review
│   ├── ViewModel_Routing_ManualEntry.cs    # Grid View
│   └── ViewModel_Routing_EditMode.cs       # History View
├── Views/              # XAML UI
│   ├── View_Routing_Workflow.xaml          # Shell View (Contains all others)
│   ├── View_Routing_ModeSelection.xaml
│   ├── View_Routing_Step1.xaml
│   ├── View_Routing_Step2.xaml
│   ├── View_Routing_Step3.xaml
│   ├── View_Routing_ManualEntry.xaml
│   └── View_Routing_EditMode.xaml
├── Interfaces/         # Service Contracts (IService_Routing_Workflow)
└── Enums/              # Enum_Routing_WorkflowStep
```

### Base Classes & Standards
*   **ViewModels:** Inherit from `ViewModel_Shared_Base` (provides `IService_ErrorHandler`, `IService_LoggingUtility`).
*   **Models:** Inherit from `ObservableObject` (CommunityToolkit.Mvvm).
*   **Dependency Injection:** All services and ViewModels registered in `App.xaml.cs`.
*   **Navigation:** `ViewModel_Routing_Workflow` acts as the shell, managing visibility of child views based on `Enum_Routing_WorkflowStep`.
*   **View Composition:** `View_Routing_Workflow.xaml` contains all child views in a single Grid, toggling their `Visibility` property based on the current workflow step. This matches the pattern in `Module_Receiving` and `Module_Dunnage`.
*   **Shell Responsibility:** `View_Routing_Workflow` (Shell) hosts the **Global Navigation Buttons** (Next, Back, Reset), **Mode Selection Trigger**, and the **Notification Box**. Child views (Step 1, Step 2, etc.) only contain their specific content.

### Key Components
*   **Workflow Service:** `Service_Routing_Workflow` manages the state machine (CurrentStep, Navigation).
*   **Workflow ViewModel:** `ViewModel_Routing_Workflow` subscribes to `StepChanged` events to toggle visibility of XAML views (Wizard, Manual, Edit) and **controls the visibility/enabled state of the bottom bar buttons** (Next, Back) based on the current step (matching `Dunnage` workflow logic).
*   **Help Service:** Explicitly integrate `Service_Help` to populate the Notification Box and handle the Help Button logic across all views.

## 3. Core Architectural Decisions

### 3.1. Wizard State Management
**Decision:** Option A (Shared Service)
*   **Implementation:** `Service_Routing_Session` will act as the singleton container for the current `Model_Routing_Label` being constructed.
*   **Rationale:** Allows decoupled ViewModels (Step 1, Step 2, etc.) to access and modify the same data instance without complex parameter passing or tight coupling to a parent ViewModel.

### 3.2. Validation Toggle Propagation
**Decision:** Option A (Service-Based + Persistence)
*   **Implementation:** `IService_Routing_Session` will expose an `IsValidationEnabled` property.
*   **Persistence:** This setting will be saved to the `users` table (or `user_preferences`) so it persists across sessions.
*   **Behavior:** ViewModels will check `_sessionService.IsValidationEnabled` before executing validation logic.

### 3.3. "Quick Add" Calculation Strategy
**Decision:** Option A (Real-Time Query)
*   **Implementation:** The `sp_routing_recipient_get_top` stored procedure will execute a `COUNT(*)` query on the `routing_labels` table, grouped by recipient, ordered descending.
*   **Rationale:** Ensures 100% accuracy without the complexity of maintaining a separate counter cache. Given the expected volume (<10k labels/year), performance impact is negligible.

### 3.4. "Other" PO Reason Persistence
**Decision:** Option A (Dynamic Lookup with Prompt)
*   **Implementation:**
    1.  User selects "Other" and types a custom reason.
    2.  System checks if this reason already exists in `routing_po_reasons`.
    3.  If new, a dialog prompts: "Do you want to save '{Reason}' as a permanent option?"
    4.  **Yes:** Insert into `routing_po_reasons` and use ID.
    5.  **No:** Use a generic "Other" ID and store the text in a `custom_reason` column on the label.

## 4. Implementation Patterns & Consistency Rules

### 4.1. Naming Conventions (Strict Enforcement)
*   **Tables:** `routing_labels`, `routing_recipients`, `routing_po_reasons` (snake_case).
    *   `routing_labels` Columns: `id` (Label #), `po_number`, `item_description` (Package Description), `qty`, `recipient_id` (Deliver to), `recipient_department` (Department), `employee_id` (Employee), `created_date` (Date).
*   **Stored Procedures:** `sp_routing_{entity}_{action}` (e.g., `sp_routing_label_insert`).
*   **C# Classes:** `Dao_Routing_{Entity}`, `Model_Routing_{Entity}`, `Service_Routing_{Feature}`, `ViewModel_Routing_{View}`.
*   **XAML Views:** `View_Routing_{Feature}.xaml`.

### 4.2. Data Access Pattern
*   **Read:** DAOs return `Model_Dao_Result<T>`.
*   **Write:** DAOs return `Model_Dao_Result` (or `Model_Dao_Result<int>` for inserts).
*   **No Exceptions:** DAOs must catch exceptions and return a failure result.
*   **Async:** All DAO methods must be `async Task`.

### 4.3. Wizard Pattern (New)
*   **State:** `Service_Routing_Session` is the single source of truth.
*   **Navigation:** `ViewModel_Routing_Workflow` handles `StepChanged` events.
*   **Validation:** ViewModels check `_session.IsValidationEnabled` before `Validate()`.
*   **Focus Management:**
    *   Use `IService_Focus` to automatically set focus to the first input field when a wizard step becomes visible.
    *   Call `_focusService.AttachFocusOnVisibility(this, FirstControl)` in the View's code-behind or `Loaded` event.
    *   **Navigation Integration:** When "Next" or "Back" buttons are pressed in `ViewModel_Routing_Workflow`, explicitly trigger `_focusService.SetFocusFirstInput(CurrentView)` to ensure focus lands on the new step's first input.

### 4.4. Error Handling
*   **User Errors:** `_errorHandler.ShowUserError()` (InfoBar).
*   **System Errors:** `_errorHandler.HandleException()` (Log + Toast).

## 5. Project Structure & Boundaries

### 5.1. Database Layer (`Database/`)
*   `Schemas/04_schema_routing.sql`: Defines `routing_labels`, `routing_recipients`, `routing_po_reasons`.
*   `StoredProcedures/Routing/`:
    *   `sp_routing_label_insert.sql`, `sp_routing_label_get_history.sql`
    *   `sp_routing_recipient_get_top.sql`, `sp_routing_recipient_search.sql`
    *   `sp_routing_po_reason_get_all.sql`, `sp_routing_po_reason_insert.sql`
*   `InforVisualScripts/Queries/Routing/`:
    *   `01_ValidatePO.sql`

### 5.2. Data Access Layer (`Module_Routing/Data/`)
*   `Dao_Routing_Label.cs`: CRUD for labels.
*   `Dao_Routing_Recipient.cs`: Search and "Top 5" logic.
*   `Dao_Routing_POReason.cs`: Manage "Other" reasons.

### 5.3. Domain Layer (`Module_Routing/Models/`)
*   `Model_Routing_Label.cs`: The core entity.
*   `Model_Routing_Recipient.cs`: Lookup entity.
*   `Model_Routing_Session.cs`: **New** - Holds the wizard state.

### 5.4. Service Layer (`Module_Routing/Services/`)
*   `Service_Routing_Workflow.cs`: Manages navigation and mode switching.
*   `Service_Routing_Session.cs`: Implements `IService_Routing_Session` (State + Validation).
*   `Service_Routing_Data.cs`: Facade for DAOs. **Updated to support fetching PO Lines** (via `sp_routing_po_get_lines` or similar) to populate the Step 1 DataGrid.

### 5.5. Presentation Layer (`Module_Routing/ViewModels/` & `Views/`)
*   `ViewModel_Routing_Workflow.cs` / `View_Routing_Workflow.xaml`: The Shell.
*   `ViewModel_Routing_ModeSelection.cs` / `View_Routing_ModeSelection.xaml`: Mode choice + Validation Toggle.
*   `ViewModel_Routing_Wizard_Step1.cs` / `View_Routing_Wizard_Step1.xaml`: Package Details.
*   `ViewModel_Routing_Wizard_Step2.cs` / `View_Routing_Wizard_Step2.xaml`: Recipient.
*   `ViewModel_Routing_Wizard_Step3.cs` / `View_Routing_Wizard_Step3.xaml`: Review.
*   `ViewModel_Routing_ManualEntry.cs` / `View_Routing_ManualEntry.xaml`: Grid view.
*   `ViewModel_Routing_History.cs` / `View_Routing_History.xaml`: Edit mode.

### 5.6. Risk Analysis & Mitigations
*   **Risk:** Wizard steps growing in number, cluttering the ViewModels folder.
    *   **Mitigation:** We are intentionally keeping the structure flat to match `Module_Receiving`. If steps exceed 7-8, we will refactor into a `Wizard/` subfolder, but for now, consistency is priority.
*   **Risk:** Logic duplication between "Wizard" and "Manual Entry".
    *   **Mitigation:** All business logic (saving, validation rules) must reside in `Service_Routing_Session` or `Service_Routing_Data`. ViewModels should only handle UI state.
*   **Risk:** Wizard state loss on tab switch.
    *   **Mitigation:** `Service_Routing_Session` is a Singleton. State persists until explicitly cleared or the app closes.
*   **Risk:** Rapid "Next" clicks causing double submission.
    *   **Mitigation:** Bind "Next" button `IsEnabled` to `!IsBusy`. ViewModels must set `IsBusy = true` immediately upon command execution.
*   **Risk:** SQL Injection in "Other" reason.
    *   **Mitigation:** All inputs passed as `MySqlParameter` in Stored Procedures. No dynamic SQL string concatenation.
*   **Risk:** "Top 5" query performance degradation.
    *   **Mitigation:** Add index on `routing_labels(recipient_id)`. Monitor query time via `Service_LoggingUtility`.
*   **Risk:** User frustration with Mode Switching for simple edits.
    *   **Mitigation:** "Edit Mode" is a full-featured CRUD interface. Users can stay in Edit Mode if their primary job is managing history.

## 6. Validation Strategy

### 6.1. Automated Testing Infrastructure (New)
Since no test projects currently exist, we will establish a testing foundation:
1.  **Create Test Project:** Add a new xUnit test project `MTM_Receiving_Application.Tests`.
2.  **Dependencies:** Add `Moq` for mocking services and `FluentAssertions` for readable assertions.
3.  **Scope:** Focus initially on Unit Tests for `Service_Routing_Session` (State Machine) and `Service_Routing_Workflow` (Navigation).

### 6.2. Test Coverage Plan
*   **Unit Tests:**
    *   `Service_Routing_Session_Tests`: Verify state transitions, validation logic (enabled/disabled), and data persistence simulation.
    *   `ViewModel_Routing_Workflow_Tests`: Verify navigation commands trigger correct step changes.
    *   `RoutingViewModelTests`: Verify Command logic, State management (`IsBusy`), and Error handling.
    *   `RoutingServiceTests`: Verify Validation, Calculations, and Data Transformation.
*   **Integration Tests (Future):**
    *   `Dao_Routing_Label_Tests`: Verify CRUD operations against a local test database (if available).
    *   `Dao_RoutingTests`: Verify Insert, Update, Delete, and Constraints.
    *   `DunnageService_ExplosionTests`: Verify component explosion logic (Volvo Module).
    *   `Dunnage_LifecycleTests`: Verify CSV generation and singleton constraints.
    *   `Helper_Database_Tests`: Verify DBNull handling and connection retry logic.
    *   `InforVisual_QueryTests`: Verify read-only SQL queries execute successfully.

### 6.3. Manual Verification
*   **Wizard Flow:** Verify state retention between steps and final submission.
*   **Validation Toggle:** Verify that unchecking the box allows saving incomplete data.
*   **Focus Management:** Verify cursor lands on first input of each step.
*   **"Other" Logic:** Verify the prompt appears and saves correctly to the database.

### 6.4. Test Infrastructure Requirements
*   **Factories:** `RouteFactory`, `DunnageShipmentFactory` (using `Faker` for data generation).
*   **Fixtures:** `DatabaseFixture` (transaction rollback support).
*   **Mocks:** `MockLoggingService`, `MockErrorHandler`.



