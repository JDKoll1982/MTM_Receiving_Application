<!-- 
[DOC-META-START]
- File Name: tech_stack.md
- Description: Language, framework, NuGet packages, databases, and key helper classes.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 17-20: # Tech Stack
  - Line 21-26: ## Core
  - Line 27-37: ## Libraries
  - Line 38-46: ## Testing
  - Line 47-53: ## Databases
  - Line 54-59: ## Key Helper Classes
- Critical Notes: WinUI 3 on .NET 10; MySQL 5.7 read/write; Infor Visual read-only.
[DOC-META-END]
-->

# Tech Stack

Last Updated: 2026-03-21

## Core

- **Language:** C# 13
- **Platform:** .NET 10
- **UI Framework:** WinUI 3 (Windows App SDK 1.8.260209005)

## Libraries

| Package                                    | Version    | Purpose                                     |
| ------------------------------------------ | ---------- | ------------------------------------------- |
| `CommunityToolkit.Mvvm`                    | 8.4.0      | ObservableProperty, RelayCommand, MVVM base |
| `CommunityToolkit.WinUI.UI.Controls`       | 7.1.2      | WinUI helper controls                       |
| `CommunityToolkit.WinUI.Animations`        | 8.2.251219 | UI animations                               |
| `Microsoft.Extensions.DependencyInjection` | 9.0.2      | DI container                                |
| `Microsoft.Extensions.Hosting`             | 9.0.2      | Application host                            |
| `MySql.Data`                               | 9.6.0      | MySQL database access (ADO.NET)             |

## Testing

| Package                     | Version | Purpose                      |
| --------------------------- | ------- | ---------------------------- |
| `xunit`                     | latest  | Unit testing framework       |
| `Moq`                       | latest  | Mocking framework            |
| `FluentAssertions`          | 8.8.0   | Readable assertion library   |
| `xunit.runner.visualstudio` | latest  | VS Test Explorer integration |

## Databases

| Database                                | Access     | Purpose                               |
| --------------------------------------- | ---------- | ------------------------------------- |
| MySQL 5.7 (`mtm_receiving_application_test`) | READ/WRITE | Application data, audit trail         |
| SQL Server (Infor Visual MTMFG)         | READ ONLY  | ERP data lookup (POs, parts, vendors) |

## Key Helper Classes

- `Helper_Database_StoredProcedure` — Executes MySQL stored procedures (with auto-retry)
- `Helper_Database_Variables` — Provides connection strings
- `Model_Dao_Result_Factory` — Creates `Model_Dao_Result<T>` success/failure instances
- `ViewModel_Shared_Base` — Base class for all ViewModels (IsBusy, StatusMessage, error handler)
