# UI Change + Logic Change Export

- Generated: 2026-03-22T23:19:15.186Z
- Form: ui-change-logic-change
- Feature: New User Setup
- Sub-feature: (none selected)
- Module: Module_Shared
- Prompt File: .github/prompts/copilotforms-ui-change-logic-change.prompt.md
- Instruction File: .github/instructions/copilotforms-ui-change-logic-change.instructions.md
- Output Folder: docs/CopilotForms/outputs/ui-change-logic-change

## Human Summary

This ui change + logic change is for New User Setup in Module_Shared. Short title: Update User Creation.

## Agent Flags

- Serena: ✔ Mandatory
- Noob Mode: ✕ Off

> **Serena mandatory** — you MUST use Serena MCP tools (`get_symbols_overview`, `find_symbol`, `find_referencing_symbols`, `replace_symbol_body`, etc.) for all code navigation and editing. Do not read full files when Serena can provide targeted symbol access.

## Catalog Context

- Summary: Shared first-run setup dialog used to guide a new user through initial application setup tasks.
- Owner: Platform Team
- Related Files: Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs, Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml, Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml.cs
- Related Services: —

## Suggested Starting Points

- Module_Shared/ViewModels/ViewModel_Shared_NewUserSetup.cs
- Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml
- Module_Shared/Views/View_Shared_NewUserSetupDialog.xaml.cs

## Required Metadata Follow-Up

- As part of this request, review and update the CopilotForms metadata for the edited module if it is stale.
- Module to review: Module_Shared
- Metadata path: docs/CopilotForms/data/copilot-forms.config.json
- Metadata path: docs/CopilotForms/data/module-metadata/Module_Shared/

## Structured Request

## Combined Request Summary

### Short request title
Update User Creation

### Who is this mainly for?
Mixed users

### Which workflow area is this in?
New User Setup

### Which screen or step is most affected?
_Not provided_

### What feels wrong on the screen today?
I want to change this to have a First and Last Name field, the Infor Visual Login Data needs to be encripted on the database as well

### What rule or behavior is wrong today?
No last name field, users credencials need to be encripted ont he database

## Target Outcome

### What should the user see or do more easily?
Use enters first and last name instead of both at once, app then combines them into the full name with smart formatting.

### What should the system do differently?
example: User types FN: john LN: koll, outputs John Koll, FN: JOHN LN: KOLL outputs John Koll, FN: jOhN LN: KoLL outputs John Koll, user's pin and infor visual user name and password need to be encripted server side

### Which parts of the screen are involved?
- None provided

### What kind of logic concerns are involved?
- None provided

### What must stay the same while changing both layers?
Everything other than First and Last name inputs, smart name formatting and added encription (removal of the Infor Visual raw string warning)

### Suggested files or layers to inspect first
- Module_Shared/ViewModels/ViewModel_Shared_Base.cs
- Module_Shared/ViewModels/ViewModel_Shared_MainWindow.cs
- Module_Shared/ViewModels/ViewModel_Shared_SharedTerminalLogin.cs
- Infrastructure/DependencyInjection
- Infrastructure/Configuration

## Definition Of Done

### Which scenarios should prove the combined change worked?
- I want to change this to have a First and Last Name field, the Infor Visual Login Data needs to be encripted on the database as well

### What validation or testing should happen?
- None provided

## Machine Data

```json
{
  "formId": "ui-change-logic-change",
  "featureId": "shared-new-user-setup",
  "values": {
    "serena-mode": "mandatory",
    "noob-mode": false,
    "requestTitle": "Update User Creation",
    "audience": "Mixed users",
    "workflowArea": "New User Setup",
    "affectedScreen": "",
    "currentUiProblem": "I want to change this to have a First and Last Name field, the Infor Visual Login Data needs to be encripted on the database as well",
    "currentLogicProblem": "No last name field, users credencials need to be encripted ont he database",
    "desiredUiOutcome": "Use enters first and last name instead of both at once, app then combines them into the full name with smart formatting.",
    "desiredLogicOutcome": "example: User types FN: john LN: koll, outputs John Koll, FN: JOHN LN: KOLL outputs John Koll, FN: jOhN LN: KoLL outputs John Koll, user's pin and infor visual user name and password need to be encripted server side",
    "uiElements": [],
    "logicConcernTypes": [],
    "constraints": "Everything other than First and Last name inputs, smart name formatting and added encription (removal of the Infor Visual raw string warning)",
    "candidateFiles": [
      "Module_Shared/ViewModels/ViewModel_Shared_Base.cs",
      "Module_Shared/ViewModels/ViewModel_Shared_MainWindow.cs",
      "Module_Shared/ViewModels/ViewModel_Shared_SharedTerminalLogin.cs",
      "Infrastructure/DependencyInjection",
      "Infrastructure/Configuration"
    ],
    "acceptanceCriteria": [
      "I want to change this to have a First and Last Name field, the Infor Visual Login Data needs to be encripted on the database as well"
    ],
    "verificationNeeds": []
  }
}
```

## Copilot Execution Note

Use the linked prompt file in Copilot Chat if prompt files are available in your setup. If not, paste this export into chat and mention the prompt file path manually. Treat this export as the source of truth.