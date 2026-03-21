# Tech Stack

Last Updated: 2026-03-21

## Core

- **Language:** C# 12
- **Platform:** .NET 8
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
| MySQL 5.7 (`mtm_receiving_application`) | READ/WRITE | Application data, audit trail         |
| SQL Server (Infor Visual MTMFG)         | READ ONLY  | ERP data lookup (POs, parts, vendors) |

## Key Helper Classes

- `Helper_Database_StoredProcedure` — Executes MySQL stored procedures (with auto-retry)
- `Helper_Database_Variables` — Provides connection strings
- `Model_Dao_Result_Factory` — Creates `Model_Dao_Result<T>` success/failure instances
- `ViewModel_Shared_Base` — Base class for all ViewModels (IsBusy, StatusMessage, error handler)
