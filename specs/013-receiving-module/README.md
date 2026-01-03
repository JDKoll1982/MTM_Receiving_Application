# Receiving Module

**Module**: Receiving Labels  
**Status**: Specification Complete  
**Priority**: P2 (MVP)  
**Created**: 2026-01-03

## Overview

The Receiving Module enables receiving clerks to process incoming materials from purchase orders (POs) through a guided workflow. Users enter PO numbers, validate against Infor Visual (read-only), enter package details, weights, quantities, heat/lot numbers, and generate shipping labels for LabelView 2022.

## Key Features

### Core Workflow (MVP)
- ✅ **Mode Selection**: Choose between Guided (step-by-step) or Manual (single-screen) entry
- ✅ **PO Entry**: Enter and validate PO numbers against Infor Visual database
- ✅ **Package Type**: Select Box, Pallet, or Loose Parts
- ✅ **Load Entry**: Enter load numbers for tracking
- ✅ **Weight/Quantity**: Enter measurements and quantities
- ✅ **Heat/Lot**: Enter traceability information
- ✅ **Review**: Data grid review with summary before saving
- ✅ **Save**: Export to MySQL database and generate CSV for LabelView

### Workflow Steps
1. Mode Selection → 2. PO Entry → 3. Package Type → 4. Load Entry → 5. Weight/Quantity → 6. Heat/Lot → 7. Review → 8. Save

## Architecture

### Module Structure
```
Module_Receiving/
├── Models/          # Data models (Session, Load, Package, etc.)
├── ViewModels/      # MVVM ViewModels (ModeSelection, POEntry, Review, etc.)
├── Views/           # XAML Views (10 views total)
├── Services/        # Business logic services
├── Data/            # DAO classes (database access)
└── Database/        # SQL schemas and stored procedures
```

### Key Components
- **IService_ReceivingWorkflow**: Workflow state machine and step management
- **IService_InforVisual**: Read-only queries to Infor Visual SQL Server database
- **IService_MySQL_Receiving**: Database operations for saving receiving loads
- **IService_ReceivingValidation**: Business rule validation
- **Dao_ReceivingLoad**: Database access layer

## Database Schema

### Tables
- `receiving_loads`: Main receiving load records
- `receiving_sessions`: Workflow session state
- Views: `vw_receiving_history` (for reporting)

### Key Constraints
- PO validation against Infor Visual (READ ONLY)
- Stored procedures for all MySQL operations
- Session persistence for workflow state

## User Workflows

### Guided Mode
Step-by-step workflow with progress indicators and validation at each step.

### Manual Mode
Single-screen form for experienced users who can enter all data at once.

## Documentation

- **[spec.md](../011-module-reimplementation/spec.md)** - Complete feature specification (User Story 2)
- **[plan.md](../011-module-reimplementation/plan.md)** - Implementation plan
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[quickstart.md](quickstart.md)** - Developer quick start guide
- **[research.md](research.md)** - Legacy system analysis

## Contracts

Service interfaces defined in [contracts/](contracts/):
- `IService_ReceivingWorkflow.cs` - Workflow orchestration
- `IService_InforVisual.cs` - Infor Visual queries (read-only)
- `IService_MySQL_Receiving.cs` - Database operations
- `IService_ReceivingValidation.cs` - Business rule validation

## Mockups

UI mockups available in [../011-module-reimplementation/mockups/Receiving/](../011-module-reimplementation/mockups/Receiving/):
- View_Receiving_ModeSelection.svg
- View_Receiving_POEntry.svg
- View_Receiving_PackageType.svg
- View_Receiving_LoadEntry.svg
- View_Receiving_WeightQuantity.svg
- View_Receiving_HeatLot.svg
- View_Receiving_Review.svg
- View_Receiving_ManualEntry.svg

## Dependencies

### Internal
- MVVM infrastructure (BaseViewModel, BaseWindow)
- Dependency Injection (App.xaml.cs)
- Error Handling (IService_ErrorHandler)
- Logging (ILoggingService)
- Authentication (employee number from context)

### External
- **Infor Visual (SQL Server)**: READ ONLY queries for PO and Part validation
- **MySQL Database**: `mtm_receiving_application` database
- **LabelView 2022**: CSV import for label printing

### Other Modules
- **Reporting Module**: Shared End-of-Day reporting infrastructure

## Implementation Status

### ✅ Complete
- Specification documents (referenced in 011-module-reimplementation)
- Contract interfaces (existing in Contracts/Services/)
- Mockups (SVG wireframes in 011-module-reimplementation/mockups/)

### 🚧 In Progress
- Code implementation (ViewModels, Views, Services, DAOs)

## Success Criteria

- Users can complete full receiving workflow in <5 minutes for typical PO
- PO validation queries Infor Visual successfully (read-only)
- Data saves to MySQL using stored procedures
- CSV files compatible with LabelView 2022
- Session state persists correctly (fixes "Add Another Part" bug)

## Related Modules

- **[012-volvo-module](../012-volvo-module/)** - Volvo dunnage requisition workflow
- **[014-dunnage-module](../014-dunnage-module/)** - Dunnage inventory management
- **[015-routing-module](../015-routing-module/)** - Internal routing labels
- **[016-reporting-module](../016-reporting-module/)** - Shared End-of-Day reporting

---

**Last Updated**: 2026-01-03  
**Maintainer**: MTM Development Team

