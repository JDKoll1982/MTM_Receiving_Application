# Feature Logic Correction Export

- Generated: 2026-03-23T21:49:46.271Z
- Form: logic-correction
- Feature: Receiving Workflow
- Sub-feature: Load Entry
- Module: Module_Receiving
- Prompt File: .github/prompts/copilotforms/copilotforms-logic-correction.prompt.md
- Instruction File: .github/instructions/copilotforms/copilotforms-logic-correction.instructions.md
- Output Folder: docs/CopilotForms/outputs/logic-correction

## Human Summary

This feature logic correction is for Load Entry in Module_Receiving. Main problem or request: It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed.  If not, then use the Fuzzy Search feature in Module_Core to bring up a dialog window with all matching values.  Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations.

## Agent Flags

- Serena: – Up to AI
- Noob Mode: ✕ Off

## Catalog Context

- Summary: Primary guided, manual, and edit receiving flow with settings-driven startup, validation gates, persisted session restore, and final save orchestration.
- Owner: Receiving Team
- Related Files: Module_Receiving/Contracts/IService_ReceivingWorkflow.cs, Module_Receiving/Contracts/IService_ReceivingSettings.cs, Module_Receiving/Contracts/IService_ReceivingValidation.cs, Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs, Module_Receiving/Services/Service_ReceivingWorkflow.cs, Module_Receiving/Services/Service_ReceivingSettings.cs, Module_Receiving/Services/Service_ReceivingValidation.cs, Module_Receiving/Services/Service_SessionManager.cs, Module_Receiving/Services/Service_MySQL_Receiving.cs, Module_Receiving/Services/Service_ReceivingLabelData.cs, Module_Receiving/Data/Dao_ReceivingLoad.cs, Module_Receiving/Data/Dao_ReceivingLabelData.cs, Module_Receiving/Models/Model_Application_Variables.cs, Module_Receiving/Models/Model_ReceivingSession.cs, Module_Receiving/Models/Model_ReceivingLoad.cs, Module_Receiving/Models/Model_ReceivingValidationResult.cs, Module_Receiving/Models/Model_UserPreference.cs, Module_Receiving/Models/Model_WorkflowStepResult.cs, Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs, Module_Receiving/Models/Model_SaveResult.cs, Module_Receiving/Settings/ReceivingSettingsKeys.cs, Module_Receiving/Settings/ReceivingSettingsDefaults.cs, Module_Receiving/Views/View_Receiving_Workflow.xaml, Module_Receiving/Views/View_Receiving_Workflow.xaml.cs, Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs, Module_Receiving/Views/View_Receiving_POEntry.xaml.cs, Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs, Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs, Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs, Module_Receiving/Views/View_Receiving_PackageType.xaml.cs, Module_Receiving/Views/View_Receiving_Review.xaml.cs, Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs
- Related Services: IService_ReceivingWorkflow, IService_ReceivingSettings, IService_ReceivingValidation, IService_MySQL_Receiving, Service_ReceivingWorkflow, Service_ReceivingSettings, Service_ReceivingValidation, Service_SessionManager, Service_MySQL_Receiving, Service_ReceivingLabelData

## Suggested Starting Points

- Module_Receiving/Contracts/IService_ReceivingWorkflow.cs
- Module_Receiving/Contracts/IService_ReceivingSettings.cs
- Module_Receiving/Contracts/IService_ReceivingValidation.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs
- Module_Receiving/Services/Service_ReceivingWorkflow.cs
- Module_Receiving/Services/Service_ReceivingSettings.cs
- Module_Receiving/Services/Service_ReceivingValidation.cs
- Module_Receiving/Services/Service_SessionManager.cs
- Module_Receiving/Services/Service_MySQL_Receiving.cs
- Module_Receiving/Services/Service_ReceivingLabelData.cs
- Module_Receiving/Data/Dao_ReceivingLoad.cs
- Module_Receiving/Data/Dao_ReceivingLabelData.cs
- Module_Receiving/Models/Model_Application_Variables.cs
- Module_Receiving/Models/Model_ReceivingSession.cs
- Module_Receiving/Models/Model_ReceivingLoad.cs
- Module_Receiving/Models/Model_ReceivingValidationResult.cs
- Module_Receiving/Models/Model_UserPreference.cs
- Module_Receiving/Models/Model_WorkflowStepResult.cs
- Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs
- Module_Receiving/Models/Model_SaveResult.cs
- Module_Receiving/Settings/ReceivingSettingsKeys.cs
- Module_Receiving/Settings/ReceivingSettingsDefaults.cs
- Module_Receiving/Views/View_Receiving_Workflow.xaml
- Module_Receiving/Views/View_Receiving_Workflow.xaml.cs
- Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs
- Module_Receiving/Views/View_Receiving_POEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs
- Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs
- Module_Receiving/Views/View_Receiving_PackageType.xaml.cs
- Module_Receiving/Views/View_Receiving_Review.xaml.cs
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs

## Required Metadata Follow-Up

- As part of this request, review and update the CopilotForms metadata for the edited module if it is stale.
- Module to review: Module_Receiving
- Metadata path: docs/CopilotForms/data/copilot-forms.config.json
- Metadata path: docs/CopilotForms/data/module-metadata/Module_Receiving/

## Structured Request

## Intent Versus Reality

### Which workflow area is this in?
Guided receiving workflow

### Which screen or step is involved?
View_Receiving_LoadEntry.xaml

### What kind of logic problem is this?
- Validation rule mismatch
- State transition is wrong
- Business rule sequence is wrong

### What is this feature supposed to do?
It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed.  If not, then use the Fuzzy Search feature in Module_Core to bring up a dialog window with all matching values.  Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations.

### What does it do today instead?
Currently If the user attempts to type it automatticly deletes the user's input

### Which rule or expectation is being broken?
Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations.

## Context And Boundaries

### Which conditions make this matter?
- Step 1: User Enters a Location, or part of a location
- Step 2: User Leave Location Text Filed
- Step 3: App checks what user typed against the infor visual database
- Step 4: Exists = Yes then Use what they entered
- Step 5: Does not Exist = Use Fuzzy Search Dialog using the logic provided above to generate a list of locations that are similar to what the user typed.  If nothing is found state as much.

### What data or records affect this behavior?
Infor Visual's Location Tables in the Infor Visual Database.
CSV Files to assist in schema searching:
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_CheckConstraints.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_ColumnDetails.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_DefaultConstraints.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_FKs.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_Indexes.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_PKs.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_TableRowCounts.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_Tables.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_Triggers.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_UniqueConstraints.csv
MTM_Waitlist_Application\Documents\InforVisualRelated\CSV_Documents\MTMFG_Schema_Views.csv

### Suggested layers or services involved
- Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml
- Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs
- Module_Receiving/Contracts/IService_ReceivingWorkflow.cs
- Module_Receiving/Contracts/IService_ReceivingSettings.cs
- Module_Receiving/Contracts/IService_ReceivingValidation.cs
- Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs
- Module_Receiving/Services/Service_ReceivingWorkflow.cs
- Module_Receiving/Services/Service_ReceivingSettings.cs
- Module_Receiving/Services/Service_ReceivingValidation.cs
- Module_Receiving/Services/Service_SessionManager.cs
- Module_Receiving/Services/Service_MySQL_Receiving.cs
- Module_Receiving/Services/Service_ReceivingLabelData.cs
- Module_Receiving/Data/Dao_ReceivingLoad.cs
- Module_Receiving/Data/Dao_ReceivingLabelData.cs
- Module_Receiving/Models/Model_Application_Variables.cs
- Module_Receiving/Models/Model_ReceivingSession.cs
- Module_Receiving/Models/Model_ReceivingLoad.cs
- Module_Receiving/Models/Model_ReceivingValidationResult.cs
- Module_Receiving/Models/Model_UserPreference.cs
- Module_Receiving/Models/Model_WorkflowStepResult.cs
- Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs
- Module_Receiving/Models/Model_SaveResult.cs
- Module_Receiving/Settings/ReceivingSettingsKeys.cs
- Module_Receiving/Settings/ReceivingSettingsDefaults.cs
- Module_Receiving/Views/View_Receiving_Workflow.xaml
- Module_Receiving/Views/View_Receiving_Workflow.xaml.cs
- Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs
- Module_Receiving/Views/View_Receiving_POEntry.xaml.cs
- Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs
- Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs
- Module_Receiving/Views/View_Receiving_PackageType.xaml.cs
- Module_Receiving/Views/View_Receiving_Review.xaml.cs
- Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs

### What should stay the same while fixing this?
- None provided

### Any other constraints to preserve?
_Not provided_

## Correction Plan

### What rules should the feature follow instead?
- It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed.  If not, then use the Fuzzy Search feature in Module_Core to bring up a dialog window with all matching values.  Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations.

### Which scenarios should prove the fix?
- The workflow remains step-based
- The user can still complete the same task from start to finish
- No business logic or save behavior changes
- The UI is easier to understand at a glance

### What must not break while fixing this?
It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed.

## Machine Data

```json
{
  "formId": "logic-correction",
  "featureId": "receiving-workflow",
  "values": {
    "serena-mode": "ai-decides",
    "noob-mode": false,
    "workflowArea": "Guided receiving workflow",
    "affectedScreen": "View_Receiving_LoadEntry.xaml",
    "logicConcernTypes": [
      "Validation rule mismatch",
      "State transition is wrong",
      "Business rule sequence is wrong"
    ],
    "workflowIntent": "It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed.  If not, then use the Fuzzy Search feature in Module_Core to bring up a dialog window with all matching values.  Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations.",
    "actualBehavior": "Currently If the user attempts to type it automatticly deletes the user's input",
    "businessRuleMismatch": "Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations.",
    "inputConditions": [
      "Step 1: User Enters a Location, or part of a location",
      "Step 2: User Leave Location Text Filed",
      "Step 3: App checks what user typed against the infor visual database",
      "Step 4: Exists = Yes then Use what they entered",
      "Step 5: Does not Exist = Use Fuzzy Search Dialog using the logic provided above to generate a list of locations that are similar to what the user typed.  If nothing is found state as much."
    ],
    "relatedData": "Infor Visual's Location Tables in the Infor Visual Database.\nCSV Files to assist in schema searching:\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_CheckConstraints.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_ColumnDetails.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_DefaultConstraints.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_FKs.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_Indexes.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_PKs.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_TableRowCounts.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_Tables.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_Triggers.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_UniqueConstraints.csv\nMTM_Waitlist_Application\\Documents\\InforVisualRelated\\CSV_Documents\\MTMFG_Schema_Views.csv",
    "layersInvolved": [
      "Module_Receiving/ViewModels/ViewModel_Receiving_LoadEntry.cs",
      "Module_Receiving/Views/View_Receiving_LoadEntry.xaml",
      "Module_Receiving/Views/View_Receiving_LoadEntry.xaml.cs",
      "Module_Receiving/Contracts/IService_ReceivingWorkflow.cs",
      "Module_Receiving/Contracts/IService_ReceivingSettings.cs",
      "Module_Receiving/Contracts/IService_ReceivingValidation.cs",
      "Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs",
      "Module_Receiving/Services/Service_ReceivingWorkflow.cs",
      "Module_Receiving/Services/Service_ReceivingSettings.cs",
      "Module_Receiving/Services/Service_ReceivingValidation.cs",
      "Module_Receiving/Services/Service_SessionManager.cs",
      "Module_Receiving/Services/Service_MySQL_Receiving.cs",
      "Module_Receiving/Services/Service_ReceivingLabelData.cs",
      "Module_Receiving/Data/Dao_ReceivingLoad.cs",
      "Module_Receiving/Data/Dao_ReceivingLabelData.cs",
      "Module_Receiving/Models/Model_Application_Variables.cs",
      "Module_Receiving/Models/Model_ReceivingSession.cs",
      "Module_Receiving/Models/Model_ReceivingLoad.cs",
      "Module_Receiving/Models/Model_ReceivingValidationResult.cs",
      "Module_Receiving/Models/Model_UserPreference.cs",
      "Module_Receiving/Models/Model_WorkflowStepResult.cs",
      "Module_Receiving/Models/Model_ReceivingWorkflowStepResult.cs",
      "Module_Receiving/Models/Model_SaveResult.cs",
      "Module_Receiving/Settings/ReceivingSettingsKeys.cs",
      "Module_Receiving/Settings/ReceivingSettingsDefaults.cs",
      "Module_Receiving/Views/View_Receiving_Workflow.xaml",
      "Module_Receiving/Views/View_Receiving_Workflow.xaml.cs",
      "Module_Receiving/Views/View_Receiving_ModeSelection.xaml.cs",
      "Module_Receiving/Views/View_Receiving_POEntry.xaml.cs",
      "Module_Receiving/Views/View_Receiving_WeightQuantity.xaml.cs",
      "Module_Receiving/Views/View_Receiving_HeatLot.xaml.cs",
      "Module_Receiving/Views/View_Receiving_PackageType.xaml.cs",
      "Module_Receiving/Views/View_Receiving_Review.xaml.cs",
      "Module_Receiving/Views/View_Receiving_ManualEntry.xaml.cs"
    ],
    "constraints": [],
    "extraConstraints": "",
    "desiredRuleSet": [
      "It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed.  If not, then use the Fuzzy Search feature in Module_Core to bring up a dialog window with all matching values.  Most of our locations include dashes in them so I want to be able to allow the user to skip dashes example: If the user enters va001 that would be V-A0-01, {1 to 2 letters}-{Letter + Number}-{2 numbers}, dda001 = DD-A0-01, xa0 = X-A0-00, some locations contain just {1 to 2 letters}-{2 numbers} Examples: U04 = U-04, R05 = R-05, DD00 = DD-00, the app should not attempt to format the location prior to fuzzy searching, the search logic should be updated for search ability of these types of locations."
    ],
    "testCases": [
      "The workflow remains step-based",
      "The user can still complete the same task from start to finish",
      "No business logic or save behavior changes",
      "The UI is easier to understand at a glance"
    ],
    "doNotRegress": "It should allow the user to type anything, then when focus is lost, if the location entered matches a location found on the infor visual database then use what they typed."
  }
}
```

## Copilot Execution Note

Use the linked prompt file in Copilot Chat if prompt files are available in your setup. If not, paste this export into chat and mention the prompt file path manually. Treat this export as the source of truth.