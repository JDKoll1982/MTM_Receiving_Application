# Module_Dunnage - Purpose and Overview

**Category**: Core Specification  
**Last Updated**: 2026-01-25  
**Related Documents**: [Data Flow](./data-flow.md), [Workflow Modes](../02-Workflow-Modes/)

## Purpose

Module_Dunnage is a specialized receiving application for tracking and managing dunnage materials (shipping containers, pallets, boxes, racks, and other reusable packaging materials) used in manufacturing operations. It provides flexible workflow modes, configurable dunnage types with dynamic specification fields, and CSV label generation for downstream systems.

### Core Functionality

The module provides four distinct workflow modes to accommodate different user needs and scenarios:

1. **Guided Mode (5-Step Wizard)**
   - Step-by-step workflow for standard dunnage receiving
   - Type Selection → Part Selection → Quantity Entry → Details Entry → Review
   - Best for: New users, occasional receiving, standard transactions
   - Optimized for clarity and guided data entry

2. **Manual Entry Mode**
   - Grid-based bulk entry interface
   - Spreadsheet-style operations for high-volume receiving
   - Best for: Power users, high-volume receiving (50+ loads)
   - Optimized for speed and efficiency

3. **Edit Mode**
   - Search, view, and modify historical dunnage transactions
   - Re-export capability for label reprinting
   - Best for: Supervisors, data correction, auditing
   - Optimized for data integrity and compliance

4. **Admin Mode**
   - Configure dunnage types, parts, and specifications
   - Manage inventory lists and system settings
   - Best for: Administrators, system configuration
   - Optimized for configuration management

## Key Features

### Configurable Dunnage Types
- Define unlimited dunnage types (pallets, boxes, racks, crates, etc.)
- Each type has unique icon for visual identification
- Active/inactive status per type
- Display ordering for user selection

### Dynamic Specification Fields
- Each dunnage type can have custom specification fields
- Field types supported:
  - Text (free-form entry)
  - Number (numeric values with validation)
  - Dropdown (predefined options)
  - Date (date picker)
- Required vs optional field designation
- Default values per field
- Field-specific validation rules

### Part-Type Associations
- Associate parts/components with dunnage types
- Multi-type support (one part can use multiple dunnage types)
- Quick part selection based on type
- Part filtering in workflows

### CSV Label Generation
- Automatic CSV export for label printing
- Dual-path export (local + network)
- Graceful fallback if network unavailable
- Re-export capability for corrections

### Inventory Tracking
- Track frequently used dunnage items
- Inventoried list for quick access
- Priority ordering for common items
- Quantity tracking over time

### Audit and Compliance
- Complete transaction history
- Edit tracking with user/timestamp
- Historical data search and retrieval
- Re-export for compliance needs

## User Personas

### Receiving Clerk (Standard User)
- Uses Guided Mode for daily dunnage receiving
- Processes 5-20 dunnage loads per transaction
- Benefits from step-by-step guidance
- Occasional user who values clarity

### Power User / High-Volume Clerk
- Uses Manual Entry Mode for bulk dunnage receiving
- Processes 50+ loads per transaction
- Proficient with keyboard shortcuts and grid entry
- Values speed and efficiency over guidance

### Supervisor / Data Corrector
- Uses Edit Mode to correct historical data
- Reviews and modifies completed transactions
- Maintains data quality and compliance
- Requires full audit trail visibility

### Administrator
- Uses Admin Mode for system configuration
- Manages dunnage types, parts, and specifications
- Configures inventory lists
- Maintains system settings

## Integration Points

### MySQL Database
- All dunnage transaction persistence
- Type, Part, and Spec configuration storage
- Inventory tracking data
- User preferences

### CSV Export System
- Label generation for dunnage tracking
- Standardized format for downstream consumption
- Local and network path support
- Re-export capability

### Module_Core Integration
- User session management
- Error handling and logging
- Settings persistence
- Navigation and routing

### Module_Settings.Dunnage Integration
- Type configuration
- Part configuration
- Specification field definitions
- User preference management
- System-level settings

## Success Metrics

### Efficiency
- Guided Mode: Complete 5-load transaction in <3 minutes
- Manual Entry Mode: Complete 50-load transaction in <2 minutes
- Edit Mode: Search and load transaction in <5 seconds

### Data Quality
- 100% validation success before save
- 0% invalid data persisted
- Complete audit trail for all modifications

### User Adoption
- 80%+ of power users adopt keyboard shortcuts within 1 week
- Successful mode selection on first use
- Minimal support requests after initial training

## Out of Scope

The following items are explicitly NOT part of Module_Dunnage:

- **Inventory Management**: Dunnage tracking only; inventory control handled separately
- **Purchasing**: Purchase order creation handled by purchasing system
- **Supplier Management**: Supplier data managed in separate system
- **Label Printing**: CSV generation only; printing handled by external system
- **ERP Integration**: No direct integration with ERP systems (data export only)
- **Advanced Analytics**: Basic reporting only; complex analytics handled by BI tools
- **Dunnage Lifecycle**: No tracking of dunnage condition, repairs, or retirement
- **Cost Tracking**: No cost/value tracking for dunnage materials

## Module Architecture

### MVVM Pattern
- ViewModels: Workflow orchestration and UI logic
- Views: XAML-based user interfaces
- Models: Entity and DataTransferObjects models
- Services: Business logic and workflow management
- DAOs: Data access layer

### Workflow Orchestration
- Service_DunnageWorkflow: Guided/Manual/Edit workflow management
- Service_DunnageAdminWorkflow: Admin workflow management
- State management via Model_DunnageSession
- Step-based navigation in Guided Mode

### Data Layer
- 8 DAOs for database operations
- MySQL persistence
- Result pattern for error handling
- Transaction support for multi-table operations

## Related Documentation

- [Data Flow](./data-flow.md) - Complete data flow architecture
- [Guided Mode Specification](../02-Workflow-Modes/001-guided-mode-specification.md) - Detailed workflow
- [Dynamic Specification Fields](../01-Business-Rules/dynamic-specification-fields.md) - Custom field system
- [Module_Settings.Dunnage Purpose](../../Module_Settings.Dunnage/00-Core/purpose-and-overview.md) - Settings overview

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete
