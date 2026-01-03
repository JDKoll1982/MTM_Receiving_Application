# Dunnage Module

**Module**: Dunnage Labels  
**Status**: Specification Complete  
**Priority**: P3 (MVP)  
**Created**: 2026-01-03

## Overview

The Dunnage Module enables warehouse managers and receiving clerks to manage dunnage inventory, create dunnage labels, and administer dunnage types, parts, and specifications. The module includes both user workflows (label creation) and admin workflows (type/part management).

## Key Features

### User Workflow (MVP)
- ✅ **Type Selection**: Select dunnage type with Material.Icons display
- ✅ **Part Selection**: Choose associated part (filtered by inventoried items)
- ✅ **Details Entry**: Dynamic form based on type specifications
- ✅ **Quantity Entry**: Enter quantity for dunnage load
- ✅ **Review**: Review dunnage data before saving
- ✅ **Save**: Export to MySQL database and generate CSV for LabelView

### Admin Workflow (MVP)
- ✅ **Admin Types**: Create/edit/delete dunnage types with icons
- ✅ **Admin Parts**: Manage parts associated with types
- ✅ **Admin Specs**: Define custom specifications for each type
- ✅ **Admin Inventory**: Manage inventoried dunnage parts list

### Workflow Steps (User)
1. Type Selection → 2. Part Selection → 3. Details Entry → 4. Quantity Entry → 5. Review → 6. Save

## Architecture

### Module Structure
```
Module_Dunnage/
├── Models/          # Data models (Type, Part, Spec, Load, Inventory)
├── ViewModels/      # MVVM ViewModels (TypeSelection, PartSelection, Admin, etc.)
├── Views/           # XAML Views (14 views total)
├── Services/        # Business logic services
├── Data/            # DAO classes (database access)
└── Database/        # SQL schemas and stored procedures
```

### Key Components
- **IService_DunnageWorkflow**: Workflow state machine and step management
- **IService_DunnageAdminWorkflow**: Admin section navigation
- **IService_MySQL_Dunnage**: Database operations (types, parts, specs, loads, inventory)
- **IService_DunnageCSVWriter**: CSV export for LabelView
- **Dao_DunnageType, Dao_DunnagePart, etc.**: Database access layer

## Database Schema

### Tables
- `dunnage_types`: Dunnage type definitions with icons
- `dunnage_parts`: Parts associated with types
- `dunnage_specs`: Custom specifications per type
- `dunnage_loads`: Generated dunnage load records
- `inventoried_dunnage`: Parts marked as inventoried
- `dunnage_custom_fields`: Custom field definitions
- `dunnage_user_preferences`: User preferences

### Key Constraints
- Types have Material.Icons for visual identification
- Parts filtered by inventoried status in user workflow
- Custom specs enable dynamic form generation
- All operations via stored procedures

## User Workflows

### User Workflow
Standard workflow for creating dunnage labels with type selection and part filtering.

### Admin Workflow
Four-section admin interface:
1. **Types**: Manage dunnage types and icons
2. **Parts**: Manage parts and associations
3. **Specs**: Define custom specifications
4. **Inventoried List**: Manage inventoried parts

## Documentation

- **[spec.md](../011-module-reimplementation/spec.md)** - Complete feature specification (User Story 3)
- **[plan.md](../011-module-reimplementation/plan.md)** - Implementation plan
- **[data-model.md](../011-module-reimplementation/data-model.md)** - Database schema
- **[tasks.md](../011-module-reimplementation/tasks.md)** - Detailed task breakdown
- **[quickstart.md](quickstart.md)** - Developer quick start guide
- **[research.md](research.md)** - Legacy system analysis

## Contracts

Service interfaces defined in [contracts/](contracts/):
- `IService_DunnageWorkflow.cs` - Workflow orchestration
- `IService_DunnageAdminWorkflow.cs` - Admin navigation
- `IService_MySQL_Dunnage.cs` - Database operations
- `IService_DunnageCSVWriter.cs` - CSV export

## Mockups

UI mockups available in [../011-module-reimplementation/mockups/Dunnage/](../011-module-reimplementation/mockups/Dunnage/):
- View_Dunnage_TypeSelection.svg
- View_Dunnage_AdminTypes.svg
- (Additional mockups referenced in mockups/README.md)

## Dependencies

### Internal
- MVVM infrastructure (BaseViewModel, BaseWindow)
- Dependency Injection (App.xaml.cs)
- Error Handling (IService_ErrorHandler)
- Logging (ILoggingService)
- Authentication (employee number from context)
- Material.Icons.WinUI3 (for type icons)

### External
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

- Users can complete full dunnage workflow in <5 minutes
- Admin can manage types, parts, and specs efficiently
- Dynamic forms generated from type specifications
- Parts filtered by inventoried status
- CSV files compatible with LabelView 2022

## Related Modules

- **[012-volvo-module](../012-volvo-module/)** - Volvo dunnage requisition workflow
- **[013-receiving-module](../013-receiving-module/)** - Standard PO receiving workflow
- **[015-routing-module](../015-routing-module/)** - Internal routing labels
- **[016-reporting-module](../016-reporting-module/)** - Shared End-of-Day reporting

---

**Last Updated**: 2026-01-03  
**Maintainer**: MTM Development Team

