# Feature Specification: LabelView Path Settings And Label Launching

**Feature Branch**: `[001-labelview-path-settings]`  
**Created**: 2026-04-29  
**Status**: Draft  
**Input**: User description: "Create new settings views for Module_Receiving, Dunnage and Volvo that the user can use to state where label files are located, create new cards in each settings navigation page to access these settings pages, module_receiving: add 2 buttons to the bottom row of View_Receiving_Workflow.xaml each button will open its corresponding label file. The 2 labels are called Receiving Label and Mini-Receiving Label. If the user has not set the label path for either of these label files it should redirect them to the settings page, otherwise it should open the label file accordingly. Add a settings page and nav card in mode settings core where the user can enter where LabelView 2022 is installed. The default location is C:\Program Files (x86)\Teklynx\LABELVIEW 2022\LV.exe and it should only look for LV.exe. Before any label file is opened it needs to first check if LV.exe exists. If it cannot find it using the default path, go to the settings page for stating where LV.exe lives. Place a note in that settings page that if LabelView is not installed to contact IT to have it installed. For Module_Dunnage the label will be named Dunnage Label and for Module_Volvo it is called Volvo Label." 

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure LabelView Executable Location (Priority: P1)

As a user who needs to launch label files from the application, I need a core settings page where I can confirm or override the LabelView executable location so the application can reliably open label files with the correct program.

**Why this priority**: No label-launch workflow can succeed unless the application can resolve a valid `LV.exe` path first.

**Independent Test**: A user can open the core settings area, see the default LabelView path, enter or update a custom path that points to `LV.exe`, save it, reopen the page, and verify the saved value is still available.

**Acceptance Scenarios**:

1. **Given** the user opens the core settings navigation area, **When** they view the LabelView settings card, **Then** they can navigate to a dedicated settings page for the LabelView executable.
2. **Given** the user opens the LabelView settings page, **When** no custom executable path has been saved yet, **Then** the page shows the default path `C:\Program Files (x86)\Teklynx\LABELVIEW 2022\LV.exe` as the expected installation location.
3. **Given** the user enters a custom executable path, **When** they save it, **Then** the system accepts the value only if it resolves to a file named `LV.exe`.
4. **Given** the user views the LabelView settings page, **When** the page is displayed, **Then** it shows a note instructing the user to contact IT if LabelView is not installed.

---

### User Story 2 - Configure Module Label File Locations (Priority: P2)

As a user managing label templates, I need module-specific settings pages for Receiving, Dunnage, and Volvo so I can define where each label file lives and maintain those paths without editing files outside the app.

**Why this priority**: Label launching depends on correct label file locations, and each module owns a distinct set of labels.

**Independent Test**: A user can open each module's settings navigation page, access a new label settings card, enter label paths, save them, and verify the saved values remain available on return.

**Acceptance Scenarios**:

1. **Given** the user is in Receiving settings, **When** they open the new label settings page, **Then** they can configure paths for `Receiving Label` and `Mini-Receiving Label`.
2. **Given** the user is in Dunnage settings, **When** they open the new label settings page, **Then** they can configure a path for `Dunnage Label`.
3. **Given** the user is in Volvo settings, **When** they open the new label settings page, **Then** they can configure a path for `Volvo Label`.
4. **Given** each module settings landing page, **When** the navigation cards are shown, **Then** a new card exists that routes the user to that module's label settings page.

---

### User Story 3 - Launch Receiving Labels From The Workflow (Priority: P3)

As a Receiving user working in the main workflow, I need quick-access buttons for the Receiving labels so I can open the correct label file from the bottom action row without leaving the workflow unless setup is missing.

**Why this priority**: This is the direct user-facing productivity gain, but it depends on the shared executable setting and module label-path settings being in place.

**Independent Test**: A user can open the Receiving workflow, use the two new bottom-row buttons, and either launch the correct label through LabelView or be redirected to the specific settings page needed to complete setup.

**Acceptance Scenarios**:

1. **Given** a valid LabelView executable is available and the `Receiving Label` path is configured, **When** the user selects the `Receiving Label` button in the workflow, **Then** the application opens that label file through LabelView.
2. **Given** a valid LabelView executable is available and the `Mini-Receiving Label` path is configured, **When** the user selects the `Mini-Receiving Label` button in the workflow, **Then** the application opens that label file through LabelView.
3. **Given** the selected Receiving label path has not been configured, **When** the user selects its button, **Then** the application redirects them to the Receiving label settings page instead of attempting a launch.
4. **Given** no valid LabelView executable can be resolved, **When** the user selects either Receiving label button, **Then** the application redirects them to the core LabelView settings page before attempting to open any label file.

---

### Edge Cases

- A previously saved LabelView path exists but no longer points to a file named `LV.exe`.
- The default LabelView path exists and is valid while no custom path has been configured.
- A label file path has been saved but the file no longer exists at launch time.
- Only one of the two Receiving labels is configured and the user selects the unconfigured one.
- The user enters a path that points to a folder, a different executable name, or a non-label file.
- The user lacks permission to open the resolved executable or label file.
- A redirect target exists in settings navigation, but the settings page was not yet initialized in the current session.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a new settings page in the core settings area for configuring the LabelView 2022 executable path.
- **FR-002**: The core settings navigation MUST include a new card that routes the user to the LabelView executable settings page.
- **FR-003**: The core LabelView settings page MUST display the default executable location as `C:\Program Files (x86)\Teklynx\LABELVIEW 2022\LV.exe`.
- **FR-004**: The system MUST only accept executable paths that resolve to a file named `LV.exe`.
- **FR-005**: The core LabelView settings page MUST display a note telling the user to contact IT if LabelView is not installed.
- **FR-006**: The system MUST provide a Receiving label settings page with configurable paths for `Receiving Label` and `Mini-Receiving Label`.
- **FR-007**: The Receiving settings navigation page MUST include a new card that routes the user to the Receiving label settings page.
- **FR-008**: The system MUST provide a Dunnage label settings page with a configurable path for `Dunnage Label`.
- **FR-009**: The Dunnage settings navigation page MUST include a new card that routes the user to the Dunnage label settings page.
- **FR-010**: The system MUST provide a Volvo label settings page with a configurable path for `Volvo Label`.
- **FR-011**: The Volvo settings navigation page MUST include a new card that routes the user to the Volvo label settings page.
- **FR-012**: The system MUST persist the saved LabelView executable path and all saved label file paths across application sessions.
- **FR-013**: The Receiving workflow bottom action row in `Module_Receiving/Views/View_Receiving_Workflow.xaml` MUST include two new buttons labeled `Receiving Label` and `Mini-Receiving Label`.
- **FR-014**: Before launching any label file, the system MUST resolve a valid LabelView executable by first using a saved executable path when valid, otherwise checking the default path, and otherwise redirecting the user to the core LabelView settings page.
- **FR-015**: The system MUST validate the selected label's configured path before launch and redirect the user to the corresponding module label settings page if the path is missing or invalid.
- **FR-016**: When both a valid LabelView executable and a valid target label path are available, the system MUST open the selected label file using LabelView.
- **FR-017**: The system MUST NOT attempt to open a label file when either the required executable or the selected label path cannot be validated.
- **FR-018**: Redirect behavior from the Receiving workflow MUST take the user directly to the missing configuration page relevant to the failed pre-launch check.
- **FR-019**: The system MUST preserve the existing workflow navigation buttons and bottom-row behavior while adding the two new Receiving label buttons.
- **FR-020**: The system MUST support distinct saved label file paths for each configured label name rather than sharing one path across modules.

### Key Entities *(include if feature involves data)*

- **LabelView Executable Setting**: Stores the resolved or user-entered path to `LV.exe`, including the default fallback location used when no custom path is available.
- **Module Label Path Setting**: Stores a label name and its file path for a specific module, including `Receiving Label`, `Mini-Receiving Label`, `Dunnage Label`, and `Volvo Label`.
- **Label Launch Request**: Represents a user action to open one specific label from the UI and the pre-launch validation outcome that either launches the file or redirects to settings.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can navigate to the new LabelView settings page from core settings in no more than 2 interactions.
- **SC-002**: A user can navigate to each new module label settings page from its module settings landing page in no more than 2 interactions.
- **SC-003**: When valid executable and label paths are present, each Receiving workflow label button opens the intended label through LabelView in a single user action.
- **SC-004**: When setup is incomplete, selecting a Receiving workflow label button routes the user to the correct settings page without attempting an invalid launch in 100% of tested scenarios.
- **SC-005**: Saved executable and label paths remain available after closing and reopening the application.