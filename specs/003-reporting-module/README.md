# Reporting Module

**Module**: End-of-Day Reporting  
**Status**: Specification Complete  
**Priority**: P1-P2 (MVP)  
**Created**: 2026-01-03

## Overview

The Reporting Module provides cross-module End-of-Day reporting capabilities for Receiving, Dunnage, Routing, and Volvo modules. It filters history data by date range, normalizes PO numbers, and exports to CSV or formatted email text for daily communication with stakeholders.

## Key Features

### Core Workflow (MVP)
- ✅ **Module Selection**: Checkbox selection for Receiving, Dunnage, Routing, Volvo modules
- ✅ **Date Range Filtering**: Select start and end dates for report data
- ✅ **Availability Check**: Check which modules have data for selected date range (disables checkboxes with no data)
- ✅ **PO Normalization**: Normalize PO numbers to standard format ("63150" → "PO-063150")
- ✅ **CSV Export**: Export filtered data to CSV matching MiniUPSLabel.csv structure
- ✅ **Email Formatting**: Format data as HTML table with alternating row colors grouped by date
- ✅ **Multi-Module Reports**: Generate reports for multiple modules simultaneously (tabbed interface)

### Enhanced Features
- ✅ **Date Grouping**: Group data by date with alternating row colors (#D3D3D3 / #FFFFFF)
- ✅ **Summary Statistics**: Display record counts and summary cards per module
- ✅ **Print Support**: Print-friendly formatting for reports

## Architecture

### Module Structure
```
Module_Reporting/                           ⬅️ To be created
├── Models/                                 # Reporting-specific models
│   └── Model_ReportRow.cs
├── ViewModels/                             # MVVM ViewModels
│   ├── ViewModel_Reporting_Main.cs
│   └── ViewModel_Reporting_ReportViewer.cs
├── Views/                                  # XAML Views
│   ├── View_Reporting_Main.xaml (.cs)
│   └── View_Reporting_ReportViewer.xaml (.cs)
├── Services/                               # Business logic services
│   └── Service_Reporting.cs
└── Data/                                   # DAO classes
    └── Dao_Reporting.cs

Module_Core/Contracts/Services/             ⬅️ Shared contracts
└── IService_Reporting.cs                   # Already exists

Database/Schemas/                           ⬅️ Database views
└── schema_reporting_views.sql
```

### Key Components
- **IService_Reporting**: Main reporting service (data retrieval, PO normalization, CSV/email formatting)
  - Located in: `Module_Core/Contracts/Services/IService_Reporting.cs`
- **Service_Reporting**: Implementation of reporting service
  - To be created in: `Module_Reporting/Services/Service_Reporting.cs`

## Database Schema

### Views
- `vw_receiving_history`: Flattened receiving history for reporting
- `vw_dunnage_history`: Flattened dunnage history for reporting
- `vw_routing_history`: Flattened routing history for reporting
- `vw_volvo_history`: Flattened Volvo history for reporting

### Key Features
- Read-only views for reporting (no direct table access)
- Date range filtering support
- PO number normalization in views or application layer

## User Workflows

### Report Generation Workflow
1. Select date range → 2. Check availability → 3. Select modules (checkboxes) → 4. Generate report → 5. Export/Print

### PO Normalization Algorithm
From `EndOfDayEmail.js`:
- **"63150"** → **"PO-063150"** (pad to 6 digits, add PO- prefix)
- **"063150B"** → **"PO-063150B"** (preserve suffix)
- **"Customer Supplied"** → **"Customer Supplied"** (pass through)
- **""** → **"No PO"** (empty string)

## Documentation

- **[spec.md](spec.md)** - Complete feature specification
- **[plan.md](plan.md)** - Implementation plan
- **[data-model.md](data-model.md)** - Database schema
- **[tasks.md](tasks.md)** - Detailed task breakdown
- **[quickstart.md](quickstart.md)** - Developer quick start guide
- **[research.md](research.md)** - Google Sheets system analysis

## Contracts

Service interface location:
- **Primary**: `Module_Core/Contracts/Services/IService_Reporting.cs` (already exists)
- **Local copy**: `specs/003-reporting-module/contracts/IService_Reporting.cs` (specification reference)

## UI Design

UI design follows standard WinUI 3 patterns:
- Date pickers for range selection
- Checkboxes for module selection
- DataGrid for report display
- Export and email formatting buttons

## Dependencies

### Internal
- MVVM infrastructure (BaseViewModel, BaseWindow)
- Dependency Injection (App.xaml.cs)
- Error Handling (IService_ErrorHandler)
- Logging (ILoggingService)
- Authentication (employee number from context)

### External
- **MySQL Database**: `mtm_receiving_application` database (read-only views)

### Other Modules
- **Receiving Module**: Provides history data via view
- **Dunnage Module**: Provides history data via view
- **Routing Module**: Provides history data via view
- **Volvo Module**: Provides history data via view

## Implementation Status

### ✅ Complete
- Specification documents (spec.md exists)
- Contract interfaces (IService_Reporting.cs exists)
- Database views design (in data-model.md)

### 🚧 In Progress
- Code implementation (ViewModels, Views, Services, DAOs)

## Success Criteria

- Users can generate reports for all modules within specified date ranges
- PO normalization handles all specified formats correctly (100% accuracy)
- CSV export includes correct columns and data for each module
- Email formatting applies date grouping and alternating colors
- Checkbox selection disables modules with no data for date range

## Related Modules

- **Receiving Module** - Standard PO receiving workflow (`Module_Receiving/`)
- **Dunnage Module** - Dunnage inventory management (`Module_Dunnage/`)
- **Routing Module** - Internal routing labels (spec: [001-routing-module](../001-routing-module/))
- **Volvo Module** - Volvo dunnage requisition workflow (spec: [002-volvo-module](../002-volvo-module/))

---

**Last Updated**: 2026-01-03  
**Maintainer**: MTM Development Team

