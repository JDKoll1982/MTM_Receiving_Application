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
ReportingModule/
├── Models/          # Data models (ReportRow, ReportData, etc.)
├── ViewModels/      # MVVM ViewModels (Reporting, ReportViewer)
├── Views/           # XAML Views (Reporting view, module sections)
├── Services/        # Business logic services
├── Data/            # DAO classes (database access)
└── Database/        # SQL views for cross-module queries
```

### Key Components
- **IService_Reporting**: Main reporting service (data retrieval, PO normalization, CSV/email formatting)
- **IService_ReceivingReporting**: Receiving module reporting integration
- **IService_DunnageReporting**: Dunnage module reporting integration
- **IService_RoutingReporting**: Routing module reporting integration
- **IService_VolvoReporting**: Volvo module reporting integration

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
- **[plan.md](../011-module-reimplementation/plan.md)** - Implementation plan
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[quickstart.md](quickstart.md)** - Developer quick start guide
- **[research.md](research.md)** - Google Sheets system analysis

## Contracts

Service interfaces defined in [contracts/](contracts/):
- `IService_Reporting.cs` - Main reporting service
- (Module-specific interfaces in respective module folders)

## Mockups

UI mockups available in [../011-module-reimplementation/mockups/](../011-module-reimplementation/mockups/):
- Reporting view mockups referenced in mockups/README.md

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

- **[012-volvo-module](../012-volvo-module/)** - Volvo dunnage requisition workflow
- **[013-receiving-module](../013-receiving-module/)** - Standard PO receiving workflow
- **[014-dunnage-module](../014-dunnage-module/)** - Dunnage inventory management
- **[015-routing-module](../015-routing-module/)** - Internal routing labels

---

**Last Updated**: 2026-01-03  
**Maintainer**: MTM Development Team

