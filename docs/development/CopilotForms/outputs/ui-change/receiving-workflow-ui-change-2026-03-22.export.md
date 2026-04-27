# UI Change Request Export

- Generated: 2026-03-22T14:16:08.319Z
- Form: ui-change
- Feature: Receiving Workflow
- Sub-feature: Manual Entry
- Module: Module_Receiving
- Prompt File: .github/prompts/copilotforms-ui-change.prompt.md
- Instruction File: .github/instructions/copilotforms-ui-change.instructions.md
- Output Folder: docs/CopilotForms/outputs/ui-change

## Human Summary

This ui change request is for Manual Entry in Module_Receiving. Short title: Fix Auto-Fill logic. Main problem or request: When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well. Desired result: Clicking Auto-Fill should fill all blank fields with the data that is above it

## Agent Flags

- Serena: – Up to AI
- Noob Mode: ✕ Off

## Catalog Context

- Summary: Primary receiving flow for queueing, editing, and archiving receiving label data.
- Owner: Receiving Team
- Related Files: Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs, Module_Receiving/Services/Service_ReceivingWorkflow.cs, Module_Receiving/Data/Dao_ReceivingLabelData.cs, Module_Receiving/Views/View_Receiving_Workflow.xaml
- Related Services: IService_ReceivingWorkflow, IService_MySQL_Receiving

## Suggested Starting Points

- Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs
- Module_Receiving/Services/Service_ReceivingWorkflow.cs
- Module_Receiving/Data/Dao_ReceivingLabelData.cs
- Module_Receiving/Views/View_Receiving_Workflow.xaml

## Required Metadata Follow-Up

- As part of this request, review and update the CopilotForms metadata for the edited module if it is stale.
- Module to review: Module_Receiving
- Metadata path: docs/CopilotForms/data/copilot-forms.config.json
- Metadata path: docs/CopilotForms/data/module-metadata/Module_Receiving/

## Structured Request

## Request Summary

### Short name for this UI change
Fix Auto-Fill logic

### Who is this screen mainly for?
Receiving Clerk

### What feels wrong today, and what should be different?
When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well.

### What should be easier after this change?
Clicking Auto-Fill should fill all blank fields with the data that is above it

## Screen And Interaction Detail

### Which workflow area is this in?
Manual Entry

### Which screen or step should change?
Manual Entry

### What does it mostly look like today?
DataGrid

### What should it mostly look like after the change?
DataGrid

### Which parts of the screen are involved?
- Manual entry fields

### What should the user be able to do more easily?
- Enter receiving data manually

### Layout notes in plain English
_Not provided_

### What interaction changes are needed?
_Not provided_

### What text on the screen should change?
When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well.

### Which usability needs matter here?
- None provided

### Anything else the user may struggle with?
_Not provided_

## Engineering Constraints

### Suggested files to review first
- Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml
- Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs
- Module_Receiving/Services/Service_ReceivingWorkflow.cs
- Module_Receiving/Data/Dao_ReceivingLabelData.cs
- Module_Receiving/Views/View_Receiving_Workflow.xaml

### What should stay the same?
- None provided

### Anything else that must stay the same?
When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well.

### How will we know this UI change is done?
- The workflow remains step-based
- The user can still complete the same task from start to finish
- No business logic or save behavior changes
- The UI is easier to understand at a glance

## Machine Data

```json
{
  "formId": "ui-change",
  "featureId": "receiving-workflow",
  "values": {
    "serena-mode": "ai-decides",
    "noob-mode": false,
    "requestTitle": "Fix Auto-Fill logic",
    "audience": "Receiving Clerk",
    "changeSummary": "When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well.",
    "successLooksLike": "Clicking Auto-Fill should fill all blank fields with the data that is above it",
    "workflowArea": "Manual Entry",
    "affectedScreens": "Manual Entry",
    "currentUiPattern": "DataGrid",
    "desiredUiPattern": "DataGrid",
    "uiElements": [
      "Manual entry fields"
    ],
    "userTasksToSupport": [
      "Enter receiving data manually"
    ],
    "layoutNotes": "",
    "interactionChanges": "",
    "copyChanges": "When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well.",
    "accessibilityNeeds": [],
    "accessibilityNotes": "",
    "filesToReview": [
      "Module_Receiving/ViewModels/ViewModel_Receiving_ManualEntry.cs",
      "Module_Receiving/Views/View_Receiving_ManualEntry.xaml",
      "Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs",
      "Module_Receiving/Services/Service_ReceivingWorkflow.cs",
      "Module_Receiving/Data/Dao_ReceivingLabelData.cs",
      "Module_Receiving/Views/View_Receiving_Workflow.xaml"
    ],
    "doNotChange": [],
    "extraConstraints": "When clicking the Auto-Fill button, it autofill everything but the Part ID, it needs to also auto fill that as well.",
    "acceptanceCriteria": [
      "The workflow remains step-based",
      "The user can still complete the same task from start to finish",
      "No business logic or save behavior changes",
      "The UI is easier to understand at a glance"
    ]
  }
}
```

## Copilot Execution Note

Use the linked prompt file in Copilot Chat if prompt files are available in your setup. If not, paste this export into chat and mention the prompt file path manually. Treat this export as the source of truth.