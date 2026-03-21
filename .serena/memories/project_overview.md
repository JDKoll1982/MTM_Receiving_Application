# Project Overview

Last Updated: 2026-03-21

**Project Name:** MTM Receiving Application

**Purpose:**
A modern WinUI 3 / .NET 8 desktop application for manufacturing receiving operations. It handles
authentication, label generation (receiving, dunnage, Volvo), reporting, bulk inventory management,
and integrates with MySQL (application data) and SQL Server / Infor Visual ERP (read-only lookup).

## Key Features

- **Multi-tier Authentication:** Personal workstation (Windows auto-login), shared terminal (PIN), new user creation.
- **Session Management:** Automatic timeout (30 min personal / 15 min shared), activity tracking.
- **Label Generation:** Receiving, Dunnage, and Volvo labels.
- **Receiving Workflow:** Guided multi-step process for PO/part lookup, quantity entry, and label output.
- **Reporting:** Configurable reports over MySQL data.
- **Bulk Inventory Management:** Batch operations for inventory adjustments.
- **ShipRec Tools:** Shipping/receiving utility operations.
- **Settings:** Layered settings UI split across multiple modules.
- **ERP Integration:** Read-only lookup of purchase orders, parts, and vendors from Infor Visual (MTMFG SQL Server).

## Module Inventory

| Module                           | Purpose                                                                  |
| -------------------------------- | ------------------------------------------------------------------------ |
| `Module_Core`                    | Shared infrastructure — base classes, helpers, DI Extensions, converters |
| `Module_Shared`                  | Cross-module shared ViewModels, Views, and Models                        |
| `Module_Receiving`               | Receiving workflow, PO lookup, label generation                          |
| `Module_Dunnage`                 | Dunnage label workflow and management                                    |
| `Module_Volvo`                   | Volvo-specific label generation and integration                          |
| `Module_Bulk_Inventory`          | Bulk inventory transaction entry                                         |
| `Module_ShipRec_Tools`           | Shipping and receiving utility tools                                     |
| `Module_Reporting`               | Report configuration, generation, and viewing                            |
| `Module_Settings.Core`           | Core application settings (users, workstations)                          |
| `Module_Settings.DeveloperTools` | Developer diagnostics and debug tools                                    |
| `Module_Settings.Dunnage`        | Dunnage-specific settings                                                |
| `Module_Settings.Receiving`      | Receiving-specific settings                                              |
| `Module_Settings.Reporting`      | Reporting settings                                                       |
| `Module_Settings.Volvo`          | Volvo-specific settings                                                  |

## Architecture

- **Pattern:** MVVM — View (XAML) → ViewModel → Service → DAO → Database
- **UI Framework:** WinUI 3 (Windows App SDK 1.8)
- **DI Container:** Microsoft.Extensions.DependencyInjection, registered in `Infrastructure/DependencyInjection/` extension methods (wired up from `App.xaml.cs`)

## Databases

| Database                                | Connection                                        | Access                              |
| --------------------------------------- | ------------------------------------------------- | ----------------------------------- |
| MySQL 5.7 (`mtm_receiving_application`) | `Helper_Database_Variables.GetConnectionString()` | READ/WRITE via stored procedures    |
| SQL Server (Infor Visual MTMFG)         | `ApplicationIntent=ReadOnly` required             | READ ONLY — no INSERT/UPDATE/DELETE |
