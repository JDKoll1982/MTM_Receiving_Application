# Volvo Dunnage Requisition Module

**Module**: Volvo Shipping Labels  
**Status**: Specification Complete  
**Priority**: P1-P3 (MVP), P4 (Future)  
**Created**: 2026-01-03

## Overview

The Volvo Dunnage Requisition Module enables users to validate Volvo dunnage shipments against packlists, track discrepancies, generate shipping labels, and create PO requisition emails for the purchasing department. This module handles customer-supplied dunnage that arrives **before** a PO is created, requiring users to generate requisition data that purchasing uses to create the PO.

## Key Features

### Core Workflow (MVP)
- ✅ **Shipment Entry**: Enter parts and skid counts from physical shipment
- ✅ **Discrepancy Tracking**: Compare Volvo packlist vs. actual received quantities
- ✅ **Component Explosion**: Automatically calculate total pieces needed using master data
- ✅ **Label Generation**: Export CSV file for LabelView 2022 import
- ✅ **Email Formatting**: Generate formatted PO requisition email for purchasing
- ✅ **PO Completion**: Enter PO and Receiver numbers after purchasing responds
- ✅ **Master Data Management**: Admin interface for parts catalog and components
- ✅ **History Management**: View, edit, and export historical shipments
- ✅ **Reporting Integration**: End-of-Day reports with Pending PO and Completed sections

### Future Enhancements (P4)
- 📦 Packlist Summary View (aggregated tracking by date/PO)

## Business Context

**Problem**: Volvo ships dunnage to MTM as customer-supplied material (no PO exists yet). Users need to:
1. Validate what was received against Volvo's packlist
2. Calculate total pieces needed (including component explosion)
3. Generate labels for physical dunnage
4. Send formatted requisition email to purchasing
5. Complete shipment record after purchasing creates PO

**Solution**: WinUI 3 desktop application module that:
- Tracks shipments with discrepancy detection
- Calculates component explosion from master data
- Generates LabelView-compatible CSV files
- Formats email text ready for copy/paste
- Integrates with shared End-of-Day reporting

## Architecture

### Module Structure
```
VolvoModule/
├── Models/          # Data models (Shipment, Line, Part, Component)
├── ViewModels/      # MVVM ViewModels (Entry, Review, History, Settings)
├── Views/           # XAML Views (Entry, Review, History, Settings)
├── Services/        # Business logic services
├── Data/            # DAO classes (database access)
└── Database/        # SQL schemas and stored procedures
```

### Key Components
- **IService_Volvo**: Main workflow service (component explosion, label generation, email formatting)
- **IService_VolvoMasterData**: Parts catalog management (CRUD, CSV import/export)
- **IService_VolvoReporting**: Reporting integration for End-of-Day reports
- **Dao_VolvoShipment**: Database access for shipments
- **Dao_VolvoPart**: Database access for parts master data

## Database Schema

### Tables
- `volvo_shipments`: Shipment headers (date, shipment#, PO, receiver, status)
- `volvo_shipment_lines`: Part line items with skid counts and discrepancies
- `volvo_parts_master`: Parts catalog (part number, quantity per skid)
- `volvo_part_components`: Component relationships (parent → component)

### Key Constraints
- Only **1 pending PO** shipment allowed at a time
- `calculated_piece_count` stored at creation (historical integrity)
- Parts deactivated (not deleted) to preserve history

## User Workflows

See [workflows/](workflows/) directory for detailed PlantUML diagrams:
1. **Label Generation** - Main shipment entry → component explosion → label generation → email → save
2. **History Archival** - View/edit historical shipments with filtering
3. **Master Data Admin** - Manage parts catalog via Settings
4. **Reporting** - Integration with shared End-of-Day reporting module
5. **Customer Packlist Export** - Future enhancement (optional)

## Documentation

- **[spec.md](spec.md)** - Complete feature specification with user stories and requirements
- **[plan.md](plan.md)** - Implementation plan and technical architecture
- **[data-model.md](data-model.md)** - Database schema with ERD and stored procedures
- **[tasks.md](tasks.md)** - Detailed task breakdown by phase (84 tasks)
- **[quickstart.md](quickstart.md)** - Developer quick start guide
- **[research.md](research.md)** - Analysis of Google Sheets source system

## Contracts

Service interfaces defined in [contracts/](contracts/):
- `IService_Volvo.cs` - Main workflow service
- `IService_VolvoMasterData.cs` - Parts catalog management
- `IService_VolvoReporting.cs` - Reporting integration

## Dependencies

### Internal
- MVVM infrastructure (BaseViewModel, BaseWindow)
- Dependency Injection (App.xaml.cs)
- Error Handling (IService_ErrorHandler)
- Logging (ILoggingService)
- Authentication (employee number from context)

### External
- **LabelView 2022**: CSV import for label printing
- **Infor Visual**: User manually receives PO after purchasing creates it
- **MySQL Database**: `mtm_receiving_application` database

### Other Modules
- **Reporting Module**: Shared End-of-Day reporting infrastructure
- **Settings Infrastructure**: Volvo settings tab integration

## Implementation Status

### ✅ Complete
- Specification documents (spec, plan, data-model, tasks)
- Workflow diagrams (5 PlantUML files)
- Contract interfaces (3 service interfaces)
- Database schema design

### 🚧 In Progress
- Implementation tasks (84 tasks across 9 phases)

### 📋 Pending
- Code implementation (ViewModels, Views, Services, DAOs)
- Database deployment (tables, stored procedures, test data)
- Mockups (SVG wireframes for UI)

## Success Criteria

- Users can complete full workflow (entry → labels → email → save) in <5 minutes
- Component explosion calculations are 100% accurate
- CSV files compatible with LabelView 2022
- Email format matches Google Sheets output
- Historical data integrity maintained (master data changes don't affect old shipments)
- Only one pending PO allowed at a time

## Related Modules

- **[013-receiving-module](../013-receiving-module/)** - Standard PO receiving workflow
- **[014-dunnage-module](../014-dunnage-module/)** - Dunnage inventory management
- **[015-routing-module](../015-routing-module/)** - Shipping label routing
- **[016-reporting-module](../016-reporting-module/)** - Shared End-of-Day reporting

---

**Last Updated**: 2026-01-03  
**Maintainer**: MTM Development Team

