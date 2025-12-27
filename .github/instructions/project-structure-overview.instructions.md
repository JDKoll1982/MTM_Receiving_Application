# Project Structure & Instruction Map

**Category**: General
**Last Updated**: December 26, 2025
**Applies To**: Entire Solution

## Overview

This document maps the project structure to the specific instruction files that govern each area. Use this to quickly find the relevant standards for any file you are working on.

## Directory to Instruction Map

| Directory / File Pattern | Instruction File |
| :--- | :--- |
| **General** | |
| `App.xaml.cs` | `mvvm-dependency-injection.instructions.md` |
| `*.xaml`, `*.xaml.cs` (General) | `xaml-troubleshooting.instructions.md` |
| `*.cs` (General) | `serena-semantic-tools.instructions.md` |
| **Architecture** | |
| `ViewModels/**/*ViewModel.cs` | `mvvm-viewmodels.instructions.md` |
| `Views/**/*.xaml` | `mvvm-views.instructions.md` |
| `Models/**/*.cs` | `mvvm-models.instructions.md` |
| `Data/**/Dao_*.cs` | `dao-pattern.instructions.md` |
| **Services (Core)** | |
| `Services/Authentication/*` | `authentication-services.instructions.md` |
| `Services/Startup/*` | `startup-services.instructions.md` |
| `Services/Database/Service_ErrorHandler.cs` | `error-handling.instructions.md` |
| `Services/Database/LoggingUtility.cs` | `logging-standards.instructions.md` |
| `Services/DispatcherService.cs` | `dispatcher-service.instructions.md` |
| `Services/WindowService.cs` | `window-management-services.instructions.md` |
| `Services/Service_Pagination.cs` | `pagination-services.instructions.md` |
| `Services/DispatcherTimerWrapper.cs` | `timer-services.instructions.md` |
| **Services (Business Logic)** | |
| `Services/Database/Service_MySQL_*.cs` | `mysql-service-pattern.instructions.md` |
| `Services/Database/Service_InforVisual.cs` | `infor-visual-integration.instructions.md` |
| `Services/Receiving/Service_*Workflow.cs` | `receiving-workflow-state-machine.instructions.md` |
| `Services/Receiving/Service_*Validation.cs` | `receiving-validation.instructions.md` |
| `Services/Receiving/Service_*CSVWriter.cs` | `csv-export-services.instructions.md` |
| `Services/Receiving/Service_SessionManager.cs` | `workflow-session-persistence.instructions.md` |
| `Services/Database/Service_UserSessionManager.cs` | `user-session-management.instructions.md` |
| **UI & Helpers** | |
| `Converters/*.cs` | `xaml-converters.instructions.md` |
| `Helpers/Database/*.cs` | `database-helpers.instructions.md` |
| `ViewModels/Receiving/ReceivingWorkflowViewModel.cs` | `receiving-ui-workflow.instructions.md` |
| `**/*Window.xaml.cs` | `window-sizing.instructions.md` |
| **Testing** | |
| `MTM_Receiving_Application.Tests/**/*` | `unit-testing-standards.instructions.md` |

## Key Principles

1.  **Granularity**: We prefer many small, specific instruction files over one giant file.
2.  **Context**: Agents should read the specific instruction file relevant to the task at hand.
3.  **Validation**: Always verify your code against the standards in these files.
