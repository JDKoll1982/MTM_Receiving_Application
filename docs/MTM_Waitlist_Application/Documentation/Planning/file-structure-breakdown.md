/AppRoot
│
├── Module_Core/
│   ├── Model_Core/
│   │   ├── Model_CoreMessagingService.cs
│   │   └── Model_CoreAppState.cs
│   ├── View_Core/
│   │   └── View_CoreStatusBar.xaml
│   └── ViewModel_Core/
│       └── ViewModel_CoreCoordinator.cs
│
├── Module_Login/
│   ├── Model_Login/
│   │   └── Model_UserAccount.cs
│   ├── View_Login/
│   │   └── View_LoginForm.xaml
│   └── ViewModel_Login/
│       └── ViewModel_LoginHandler.cs
│
├── Module_Operator/
│   ├── Model_Operator/
│   │   ├── Model_OperatorProfile.cs
│   │   └── Model_OperatorTask.cs
│   ├── View_Operator/
│   │   ├── View_OperatorDashboard.xaml
│   │   └── View_OperatorWizard.xaml
│   └── ViewModel_Operator/
│       ├── ViewModel_OperatorDashboard.cs
│       └── ViewModel_OperatorWizard.cs
│
├── Module_SetupTech/
│   ├── Model_SetupTech/
│   │   ├── Model_SetupTask.cs
│   │   └── Model_SetupChangeRequest.cs
│   ├── View_SetupTech/
│   │   └── View_SetupTechDashboard.xaml
│   └── ViewModel_SetupTech/
│       └── ViewModel_SetupTechDashboard.cs
│
├── Module_Leads/
│   ├── Model_Leads/
│   │   ├── Model_LeadProfile.cs
│   │   └── Model_AnalyticsRights.cs
│   ├── View_Leads/
│   │   └── View_LeadDashboard.xaml
│   └── ViewModel_Leads/
│       └── ViewModel_LeadDashboard.cs
│
├── Module_WaitList/
│   ├── Model_WaitList/
│   │   ├── Model_WaitListItem.cs
│   │   ├── Model_WaitListAnalytics.cs
│   │   └── Model_WaitListQuickAdd.cs
│   ├── View_WaitList/
│   │   ├── View_WaitListQueue.xaml
│   │   └── View_WaitListQuickAdd.xaml
│   └── ViewModel_WaitList/
│       ├── ViewModel_WaitListQueue.cs
│       └── ViewModel_WaitListQuickAdd.cs
│
├── Module_MaterialHandling/
│   ├── Model_MaterialHandling/
│   │   ├── Model_MaterialTask.cs
│   │   └── Model_ZoneAssignment.cs
│   ├── View_MaterialHandling/
│   │   └── View_MaterialHandlerPanel.xaml
│   └── ViewModel_MaterialHandling/
│       └── ViewModel_MaterialHandlerPanel.cs
│
├── Module_Quality/
│   ├── Model_Quality/
│   │   ├── Model_QualityTask.cs
│   │   ├── Model_QualityAlertConfig.cs
│   │   └── Model_QualityNotification.cs
│   ├── View_Quality/
│   │   └── View_QualityDashboard.xaml
│   └── ViewModel_Quality/
│       └── ViewModel_QualityDashboard.cs
│
├── Module_Logistics/
│   ├── Model_Logistics/
│   │   ├── Model_TruckTask.cs
│   │   ├── Model_VanTask.cs
│   │   └── Model_LocationMapping.cs
│   ├── View_Logistics/
│   │   └── View_LogisticsDashboard.xaml
│   └── ViewModel_Logistics/
│       └── ViewModel_LogisticsDashboard.cs
│
├── Module_Training/
│   ├── Model_Training/
│   │   ├── Model_TrainingProgram.cs
│   │   └── Model_TrainingSession.cs
│   ├── View_Training/
│   │   └── View_TrainingSteps.xaml
│   └── ViewModel_Training/
│       └── ViewModel_TrainingSteps.cs
│
├── Module_SystemIntegration/
│   ├── Model_SystemIntegration/
│   │   ├── Model_IntegrationConfig.cs
│   │   └── Model_IPMapping.cs
│   ├── View_SystemIntegration/
│   │   └── View_IntegrationStatus.xaml
│   └── ViewModel_SystemIntegration/
│       └── ViewModel_IntegrationStatus.cs
│
└── App.xaml
Key Departmental Modules:

Module_Operator: Press operators—dashboards, wizards, self-service.
Module_SetupTech: Setup technicians—change tasks, module reconfiguration, isolated changes.
Module_Leads: Production leads—analytics, supervisor dashboard, advanced rights.
Module_WaitList: Generalized task/wait queue for all, quick add, analytics split as component where needed.
Module_MaterialHandling: Material handlers—zone tasks, logs, projects, “quick add.”
Module_Quality: Quality control—quality-specific queue, alerting/config, notifications.
Module_Logistics: Truck/van/material movement, truck task assignment, red-flag logic, integration with item and destination checks.
Module_Training: Operator/tech/lead training, session records, in-app workflow.
Module_SystemIntegration: Backend integration, site/IP mapping.
Module_Core: System-wide communication/service bus, app state, status.
Module_Login: Authentication and account controls
