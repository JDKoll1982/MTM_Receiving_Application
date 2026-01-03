# Routing Module

**Module**: Internal Routing Labels  
**Status**: Specification Complete  
**Priority**: P4 (MVP)  
**Created**: 2026-01-03

## Overview

The Routing Module enables receiving clerks to create internal routing labels for inter-department delivery. Labels are similar to UPS/FedEx shipping labels but for internal use only, routing received materials to specific departments or individuals.

## Key Features

### Core Workflow (MVP)
- ✅ **Label Entry**: Enter routing label details (Deliver To, Department, Description, PO, Work Order)
- ✅ **Auto-Fill**: Department auto-fills when selecting recipient with default department
- ✅ **PO Formatting**: Auto-format PO numbers ("63150" → "PO-063150")
- ✅ **Label Numbering**: Auto-increment label numbers per session
- ✅ **Duplicate Row**: Copy all fields to new row with incremented label number
- ✅ **Print Labels**: Export to CSV for LabelView printing (template: "Expo - Mini UPS Label ver. 1.0")
- ✅ **Save to History**: Archive labels to history table and clear current entries
- ✅ **History View**: View archived labels grouped by date with alternating colors

### Enhanced Features (from Google Sheets)
- ✅ **Recipient Lookup**: Dropdown populated from routing_recipients table
- ✅ **Department Auto-Lookup**: Auto-fill department from recipient's default
- ✅ **History Archival**: Archive today's labels to history with confirmation
- ✅ **Date Grouping**: History grouped by date with alternating row colors

## Architecture

### Module Structure
```
RoutingModule/
├── Models/          # Data models (Label, Recipient, History)
├── ViewModels/      # MVVM ViewModels (LabelEntry, History, RecipientAdmin)
├── Views/           # XAML Views (5 views total)
├── Services/        # Business logic services
├── Data/            # DAO classes (database access)
└── Database/        # SQL schemas and stored procedures
```

### Key Components
- **IService_Routing**: Main routing label service (entry, queue management, CSV export)
- **IService_Routing_History**: History archival and retrieval service
- **IService_Routing_RecipientLookup**: Recipient and department lookup service
- **Dao_RoutingLabel**: Database access for labels
- **Dao_RoutingRecipient**: Database access for recipients

## Database Schema

### Tables
- `routing_labels`: Current/today labels (is_archived = 0)
- `routing_labels_history`: Archived labels (is_archived = 1)
- `routing_recipients`: Recipient lookup table with default departments

### Key Constraints
- Only one active label queue at a time (today's labels)
- Labels archived to history when "Save to History" clicked
- Recipients have optional default departments for auto-fill
- PO numbers stored as VARCHAR(20) to support various formats

## User Workflows

### Label Entry Workflow
1. Enter label details → 2. Add to Queue → 3. Print Labels → 4. Save to History

### History Management
- View archived labels filtered by date range
- Labels grouped by date with alternating colors
- Export history to CSV

### Recipient Administration
- Manage routing recipients and default departments
- Add/edit/delete recipients

## Documentation

- **[spec.md](../011-module-reimplementation/spec.md)** - Complete feature specification (User Story 4)
- **[plan.md](../011-module-reimplementation/plan.md)** - Implementation plan
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[quickstart.md](quickstart.md)** - Developer quick start guide
- **[research.md](research.md)** - Google Sheets system analysis

## Contracts

Service interfaces defined in [contracts/](contracts/):
- `IService_Routing.cs` - Main routing service
- `IService_Routing_History.cs` - History archival service
- `IService_Routing_RecipientLookup.cs` - Recipient lookup service

## Mockups

UI mockups available in [../011-module-reimplementation/mockups/Routing/](../011-module-reimplementation/mockups/Routing/):
- View_Routing_LabelEntry.svg
- View_Routing_History.svg
- (Additional mockups referenced in mockups/README.md)

## Dependencies

### Internal
- MVVM infrastructure (BaseViewModel, BaseWindow)
- Dependency Injection (App.xaml.cs)
- Error Handling (IService_ErrorHandler)
- Logging (ILoggingService)
- Authentication (employee number from context)

### External
- **MySQL Database**: `mtm_receiving_application` database
- **LabelView 2022**: CSV import for label printing (template: "Expo - Mini UPS Label ver. 1.0")

### Other Modules
- **Reporting Module**: Shared End-of-Day reporting infrastructure

## Implementation Status

### ✅ Complete
- Specification documents (referenced in 011-module-reimplementation)
- Contract interfaces (to be created)
- Mockups (SVG wireframes in 011-module-reimplementation/mockups/)

### 🚧 In Progress
- Code implementation (ViewModels, Views, Services, DAOs)

## Success Criteria

- Users can create routing labels in <2 minutes per label
- Department auto-fills when selecting recipient with default
- Labels export to CSV compatible with LabelView 2022
- History archival moves labels without data loss
- History view displays labels grouped by date with alternating colors

## Related Modules

- **[012-volvo-module](../012-volvo-module/)** - Volvo dunnage requisition workflow
- **[013-receiving-module](../013-receiving-module/)** - Standard PO receiving workflow
- **[014-dunnage-module](../014-dunnage-module/)** - Dunnage inventory management
- **[016-reporting-module](../016-reporting-module/)** - Shared End-of-Day reporting

---

**Last Updated**: 2026-01-03  
**Maintainer**: MTM Development Team

