# MTM Receiving Application - Documentation Index

## Project Documentation Index

### Project Overview

- **Type**: Monolith (Desktop Application)
- **Primary Language**: C# (.NET 8)
- **Architecture**: Modular Monolith with Strict MVVM
- **Framework**: WinUI 3 / Windows App SDK
- **Repository**: [JDKoll1982/MTM_Receiving_Application](https://github.com/JDKoll1982/MTM_Receiving_Application)

### Quick Reference

| Category | Details |
|----------|---------|
| **Tech Stack** | WinUI 3, .NET 8, CommunityToolkit.Mvvm, MySQL, SQL Server (READ-ONLY) |
| **Entry Point** | App.xaml.cs (DI container setup), MainWindow.xaml |
| **Architecture Pattern** | Modular Monolith with MVVM and Dependency Injection |
| **Database Strategy** | Dual-database (MySQL for app data, SQL Server for ERP integration) |
| **Modules** | Core, Receiving, Dunnage, Shared, Settings, Routing (placeholder) |

### Module Structure

#### Module_Core (Infrastructure)

- **Purpose**: Shared services, authentication, database helpers, navigation
- **Key Services**: Error handling, logging, authentication, navigation
- **Data Access**: User authentication, Infor Visual ERP integration (READ-ONLY)

#### Module_Receiving (PO Receiving)

- **Purpose**: Purchase order receiving workflows
- **Workflows**: 8-step wizard (PO entry → validation → load details → weight/quantity → heat/lot → package type → review → label)
- **Data**: Receiving loads, receiving lines, package preferences
- **Integration**: Validates POs against Infor Visual ERP (READ-ONLY)

#### Module_Dunnage (Returnable Packaging)

- **Purpose**: Dunnage tracking and management
- **Workflows**: User workflow (6 steps), Admin dashboard (CRUD operations)
- **Data**: Dunnage types, parts, specs, inventory, transaction log
- **Features**: Incoming/Outgoing/Transfer/Count modes, custom fields, inventory tracking

#### Module_Shared (Reusable Components)

- **Purpose**: Shared UI components
- **Components**: Splash screen, login dialog, new user setup, help system, icon selector

#### Module_Settings (Configuration)

- **Purpose**: User preferences and application settings
- **Features**: Default mode selection, user preferences

#### Module_Routing (Future)

- **Purpose**: Warehouse routing and logistics (placeholder, not yet implemented)

---

## Generated Documentation

### Core Documentation

- [Project Overview](./project-overview.md) - Executive summary, tech stack, quick start
- [Architecture](./architecture.md) - Comprehensive architecture documentation
- [Source Tree Analysis](./source-tree-analysis.md) - Annotated directory structure
- [Development Guide](./development-guide.md) - Setup, build, and development workflow

### Technical Documentation

- [Data Models](./data-models.md) - Database schemas, dual-database strategy, C# models
- [Component Inventory](./component-inventory.md) - UI components, views, and design system

---

## Existing Documentation

### Project Files

- [AGENTS.md](../AGENTS.md) - AI agent development guidelines
  - MVVM architecture patterns
  - DAO patterns and database access rules
  - ViewModel creation guidelines
  - View (XAML) best practices
  - Common pitfalls to avoid

### Feature Specifications (Unimplemented)

- [specs/001-routing-module/](../specs/001-routing-module/) - Routing module specification
- [specs/002-volvo-module/](../specs/002-volvo-module/) - Volvo-specific workflow specification
- [specs/003-reporting-module/](../specs/003-reporting-module/) - Reporting module specification

### Database Documentation

- [Database/InforVisualScripts/README.md](../Database/InforVisualScripts/README.md) - Infor Visual ERP integration examples
- [Database/Schemas/](../Database/Schemas/) - MySQL table definitions (versioned)
- [Database/StoredProcedures/](../Database/StoredProcedures/) - All MySQL operations (by module)
- [Database/Migrations/](../Database/Migrations/) - Database version migrations

---

## Getting Started

### For Developers

1. **Read This First**:
   - [Project Overview](./project-overview.md) - Understand what the application does
   - [Architecture](./architecture.md) - Learn the architectural patterns
   - [AGENTS.md](../AGENTS.md) - Development guidelines and constraints

2. **Set Up Development Environment**:
   - Follow [Development Guide](./development-guide.md) for prerequisites and setup
   - Deploy database schemas from `Database/Schemas/`
   - Configure `appsettings.json` for database connections

3. **Explore the Codebase**:
   - [Source Tree Analysis](./source-tree-analysis.md) - Understand project structure
   - [Component Inventory](./component-inventory.md) - Browse UI components
   - [Data Models](./data-models.md) - Review database schemas

4. **Build and Run**:

   ```powershell
   dotnet build /p:Platform=x64
   dotnet run
   ```

### For AI-Assisted Development

**Primary Resources**:

- [AGENTS.md](../AGENTS.md) - Comprehensive AI agent guidelines
  - Architecture patterns (MVVM, DAO, Services)
  - Critical constraints (READ-ONLY SQL Server, stored procedures only for MySQL)
  - Code generation templates
  - Common pitfalls

**When Adding Features**:

1. Review [Architecture](./architecture.md) for module structure
2. Check [Data Models](./data-models.md) for database schema
3. Follow patterns in [AGENTS.md](../AGENTS.md)
4. Use existing modules as reference (Module_Receiving, Module_Dunnage)

**Critical Rules**:

- ⚠️ **NEVER write to Infor Visual (SQL Server)** - READ ONLY with `ApplicationIntent=ReadOnly`
- ✅ **Always use stored procedures for MySQL** - No raw SQL in C# code
- ✅ **All ViewModels must be partial classes** - Required for CommunityToolkit.Mvvm
- ✅ **Views use x:Bind, not Binding** - Compile-time binding
- ✅ **DAOs must be instance-based** - No static DAOs, register in DI

### For New Team Members

**Day 1**:

- Read [Project Overview](./project-overview.md)
- Review [Architecture](./architecture.md) sections on MVVM and Modules
- Set up development environment per [Development Guide](./development-guide.md)

**Week 1**:

- Study [AGENTS.md](../AGENTS.md) architecture patterns
- Review [Component Inventory](./component-inventory.md) to understand UI structure
- Explore existing modules: Module_Receiving and Module_Dunnage
- Run the application and test receiving/dunnage workflows

**Month 1**:

- Understand dual-database strategy from [Data Models](./data-models.md)
- Review database stored procedures in `Database/StoredProcedures/`
- Study Infor Visual integration (READ-ONLY) patterns
- Complete a small feature implementation following established patterns

---

## Quick Links

### Architecture & Design

- [Architecture Document](./architecture.md)
- [MVVM Pattern Guide](../AGENTS.md#architecture-patterns)
- [DAO Pattern Guide](../AGENTS.md#dao-pattern-mandatory)
- [Component Inventory](./component-inventory.md)

### Database

- [Data Models](./data-models.md)
- [MySQL Schemas](../Database/Schemas/)
- [Stored Procedures](../Database/StoredProcedures/)
- [Infor Visual Integration](../Database/InforVisualScripts/)

### Development

- [Development Guide](./development-guide.md)
- [Build & Test Commands](./development-guide.md#build--run)
- [XAML Troubleshooting](./development-guide.md#xaml-troubleshooting)

### Project Structure

- [Source Tree Analysis](./source-tree-analysis.md)
- [Module Organization](./source-tree-analysis.md#modular-architecture-pattern)

---

## Documentation Maintenance

### How This Documentation Was Generated

This documentation was created using an AI-assisted project documentation workflow with an **exhaustive scan** level:

- All source files were analyzed
- Database schemas and stored procedures were inventoried
- Module structure and dependencies were mapped
- Architecture patterns were extracted from codebase

### Updating Documentation

When making significant changes to the project:

1. Update relevant markdown files in `docs/`
2. Keep [AGENTS.md](../AGENTS.md) synchronized with architectural changes
3. Update database documentation when schemas change
4. Regenerate documentation using AI workflow if major refactoring occurs

### Documentation Structure

```bash
docs/
├── index.md                    # This file - master navigation
├── project-overview.md         # Executive summary
├── architecture.md             # Comprehensive architecture
├── source-tree-analysis.md     # Annotated directory structure
├── data-models.md              # Database schemas and models
├── component-inventory.md      # UI component catalog
├── development-guide.md        # Setup and development workflow
└── project-scan-report.json    # Workflow state (do not edit manually)
```

---

**Documentation Generated**: 2026-01-04  
**Scan Level**: Exhaustive (all source files analyzed)  
**Repository**: <https://github.com/JDKoll1982/MTM_Receiving_Application>  
**Developer**: John Koll
