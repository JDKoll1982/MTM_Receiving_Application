# MTM Receiving Application - Project Overview

## Project Summary

**MTM Receiving Application** is a WinUI 3 desktop application designed for manufacturing receiving operations at MTM Manufacturing. It streamlines the process of receiving materials against purchase orders, managing returnable packaging (dunnage), and generating labels for incoming shipments.

**Repository**: [JDKoll1982/MTM_Receiving_Application](https://github.com/JDKoll1982/MTM_Receiving_Application)  
**Primary Language**: C# (88.2%)  
**Framework**: WinUI 3 / Windows App SDK  
**Target Platform**: Windows 10/11 (build 17763+)

## Purpose

The application serves manufacturing floor operators at touchscreen terminals to:
1. **Receive Materials**: Validate purchase orders against ERP, capture weight/quantity, generate receiving labels
2. **Manage Dunnage**: Track returnable packaging (bins, racks, pallets) with incoming/outgoing/transfer/count workflows
3. **Generate Labels**: Create CSV files for LabelView 2022 label printing system
4. **Admin Operations**: Manage dunnage types, parts, and inventory levels

## Technology Stack Summary

| Category | Technology | Version | Purpose |
|----------|-----------|---------|---------|
| **Framework** | .NET | 8.0 | Runtime platform |
| **UI Framework** | WinUI 3 | Windows App SDK 1.8+ | Native Windows desktop UI |
| **Language** | C# | 12 | Application code |
| **MVVM** | CommunityToolkit.Mvvm | 8.2.2 | MVVM source generators |
| **DI Container** | Microsoft.Extensions.DependencyInjection | 8.0 | Dependency injection |
| **Database (App)** | MySQL | 8.x | Application data store |
| **Database (ERP)** | SQL Server | 2019+ | Infor Visual ERP (READ-ONLY) |
| **MySQL Provider** | MySql.Data | 9.4.0 | MySQL ADO.NET driver |
| **SQL Provider** | Microsoft.Data.SqlClient | 5.2.2 | SQL Server driver |
| **CSV** | CsvHelper | 33.0.1 | Label file generation |
| **Icons** | Material.Icons.WinUI3 | 2.4.1 | Material Design icons |
| **Telemetry** | OpenTelemetry | 1.14.0 | Observability (future) |

## Architecture Type

**Pattern**: Modular Monolith with Strict MVVM

The application is a single executable with modular feature separation:
- Each feature module has its own Models, Views, ViewModels, Services, and Data layers
- Strict MVVM architecture enforced via CommunityToolkit.Mvvm source generators
- Dependency Injection for all cross-module dependencies
- No code-behind logic in Views (XAML-only)
- All database operations via stored procedures (MySQL) or parameterized queries (SQL Server READ-ONLY)

## Repository Structure

```
MTM_Receiving_Application/
├── Module_Core/         # Infrastructure (auth, logging, navigation, error handling)
├── Module_Receiving/    # Purchase order receiving workflows
├── Module_Dunnage/      # Returnable packaging management
├── Module_Shared/       # Shared UI components (login, splash, help)
├── Module_Settings/     # User preferences and configuration
├── Module_Routing/      # Future: Routing module (placeholder)
├── Database/            # SQL schemas, stored procedures, test data
│   ├── Schemas/         # MySQL table definitions
│   ├── StoredProcedures/# All MySQL operations
│   ├── InforVisualScripts/ # SQL Server integration examples
│   └── Migrations/      # Database version migrations
├── Assets/              # Icons, fonts, images
├── Scripts/             # PowerShell utility scripts
├── specs/               # Feature specifications (unimplemented features)
├── docs/                # Generated project documentation
└── _bmad/               # BMad methodology files (AI-assisted development)
```

## Key Features

### Receiving Workflow
- **PO Entry & Validation**: Validate purchase orders against Infor Visual ERP
- **Load Details**: Capture carrier, packing slip, and shipment information
- **Weight & Quantity**: Enter received quantities with weight validation
- **Heat/Lot Numbers**: Track material certifications
- **Package Type Selection**: User-defined package configurations
- **Label Generation**: Automatic CSV creation for LabelView printing

### Dunnage Management
- **Transaction Modes**: Incoming, Outgoing, Transfer, Count
- **Type & Part Selection**: Hierarchical selection (Type → Part)
- **Quantity Entry**: Large touch-optimized number entry
- **Custom Fields**: Extensible metadata per part
- **Admin Dashboard**: CRUD operations for types, parts, and inventory
- **Inventory Tracking**: Real-time inventory levels with transaction history

### Authentication & User Management
- **Windows Integration**: Auto-detects Windows username
- **Shared Terminal Support**: PIN-based authentication
- **User Preferences**: Default modes, department assignments
- **First-Time Setup**: Guided user onboarding

## Database Strategy

### Dual-Database Architecture

**MySQL (mtm_receiving_application)** - Application Data
- **Purpose**: Transactional data for receiving and dunnage operations
- **Access**: Full READ/WRITE
- **Operations**: Via stored procedures only (no raw SQL in C# code)
- **Server**: 172.16.1.104:3306

**SQL Server (Infor Visual ERP - MTMFG)** - Master Data
- **Purpose**: Purchase order and part data validation
- **Access**: READ-ONLY (`ApplicationIntent=ReadOnly`)
- **Operations**: Direct queries (SELECT only)
- **Server**: VISUAL
- **Site**: 002 (MTM warehouse)

## Module Organization

Each module follows a consistent structure:

### Module_Core (Infrastructure)
- **Services**: Error handling, logging, authentication, navigation
- **Data**: User authentication, ERP integration (READ-ONLY)
- **Helpers**: Database connection management, stored procedure execution

### Module_Receiving (PO Receiving)
- **Workflows**: 8-step wizard (Mode → PO → Load → Weight → Heat → Package → Review → Label)
- **Services**: Workflow orchestration, validation, CSV generation
- **Data**: Receiving loads/lines, package preferences
- **ViewModels**: 10 ViewModels (workflow steps + manual/edit modes)

### Module_Dunnage (Packaging Management)
- **Workflows**: 6-step user workflow, admin dashboard
- **Services**: Workflow orchestration, admin operations, CSV generation
- **Data**: Dunnage types/parts/specs, inventory, transactions
- **ViewModels**: 20+ ViewModels (workflow + admin + dialogs)

### Module_Shared (Reusable Components)
- **Components**: Splash screen, login dialog, help system, icon picker
- **ViewModels**: 5 shared ViewModels

### Module_Settings (Configuration)
- **Features**: Default mode selection, user preferences
- **ViewModels**: 3 settings ViewModels

## Development Resources

| Resource | Location | Description |
|----------|----------|-------------|
| **Architecture** | [docs/architecture.md](./architecture.md) | Detailed architecture documentation |
| **Data Models** | [docs/data-models.md](./data-models.md) | Database schemas and C# models |
| **Source Tree** | [docs/source-tree-analysis.md](./source-tree-analysis.md) | Annotated directory structure |
| **Components** | [docs/component-inventory.md](./component-inventory.md) | UI component catalog |
| **Development Guide** | [docs/development-guide.md](./development-guide.md) | Setup and build instructions |
| **AI Agent Guide** | [AGENTS.md](../AGENTS.md) | Guidelines for AI-assisted development |

## Quick Start

### Prerequisites
- Windows 10/11 (build 17763+)
- Visual Studio 2022 with .NET Desktop Development workload
- .NET 8 SDK
- MySQL Server 8.x (access to 172.16.1.104:3306)
- SQL Server (optional, can use mock data)

### Build & Run
```powershell
# Clone repository
git clone https://github.com/JDKoll1982/MTM_Receiving_Application.git
cd MTM_Receiving_Application

# Restore packages
dotnet restore

# Build
dotnet build /p:Platform=x64

# Run
dotnet run
```

### Database Setup
```powershell
# Deploy MySQL schemas and stored procedures
cd Database
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Schemas/02_create_authentication_tables.sql
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Schemas/03_create_receiving_tables.sql
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application < Schemas/07_create_dunnage_tables_v2.sql
# Deploy stored procedures (see development-guide.md)
```

## Testing

```powershell
# Run all tests
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests (requires MySQL)
dotnet test --filter "FullyQualifiedName~Integration"
```

## Deployment

**Target Environment**: Manufacturing floor touchscreen terminals

**Deployment Steps**:
1. Build release package: `dotnet build -c Release /p:Platform=x64`
2. Copy application to target terminal
3. Configure `appsettings.json` with database connections
4. Deploy MySQL schemas and stored procedures
5. Configure LabelView CSV monitoring

## Project Status

### Implemented Modules
- ✅ Module_Core (Infrastructure)
- ✅ Module_Receiving (PO Receiving workflows)
- ✅ Module_Dunnage (Packaging management with admin dashboard)
- ✅ Module_Shared (Reusable UI components)
- ✅ Module_Settings (User preferences)

### Planned Features (specs/)
- 📋 Module_Routing: Warehouse routing and logistics
- 📋 Volvo Module: Volvo-specific receiving workflows
- 📋 Reporting Module: Analytics and reporting dashboard

## Contributing

This is an internal MTM Manufacturing application. For development guidelines, see [AGENTS.md](../AGENTS.md) which contains:
- MVVM architecture patterns
- DAO patterns and database access rules
- ViewModel creation guidelines
- View (XAML) best practices
- Common pitfalls to avoid

## License

Internal MTM Manufacturing application. All rights reserved.

## Contact

**Developer**: John Koll  
**Repository**: https://github.com/JDKoll1982/MTM_Receiving_Application

---

*Last Updated: 2026-01-04*  
*Documentation Generated: AI-assisted project documentation workflow*
