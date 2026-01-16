# MTM Receiving Application - Data Models

## Database Architecture

### Dual-Database Strategy

The application uses two databases:

1. **MySQL (mtm_receiving_application)**: Primary operational store - READ/WRITE
2. **SQL Server (Infor Visual ERP - VISUAL/MTMFG)**: Source of truth for POs and Parts - READ ONLY

## MySQL Schema (Application Data)

### Authentication Schema

- **users**: User credentials, PINs, and preferences
  - Stores Windows username mappings
  - Department associations
  - Default workflow modes (Receiving/Dunnage)
- **departments**: Department definitions

### Receiving Schema

- **receiving_history**: Header-level receiving transactions
  - PO number, carrier, packing slip
  - Created by user and timestamp
- **receiving_lines**: Line-item details for each load
  - Weight, quantity, heat/lot numbers
  - Package type preferences
  - Links to PO line items
- **receiving_package_types**: User-defined package configurations

### Dunnage Schema

- **dunnage_types**: Categories of returnable packaging (bins, racks, pallets)
  - Icon associations (Material Design)
- **dunnage_parts**: Specific part numbers within each type
  - Home locations
  - Custom field definitions
- **dunnage_specs**: Specification templates for parts
- **dunnage_requires_inventory**: Current inventory counts
- **dunnage_history**: Transaction log for dunnage movements
  - Mode (Incoming, Outgoing, Transfer, Count)
- **dunnage_custom_fields**: Extensible metadata

## SQL Server Schema (Infor Visual ERP - READ ONLY)

### Core Tables (Read-Only Access)

- **po_detail**: Purchase Order line items
  - Site reference: '002' (MTM warehouse)
- **part**: Part master data
  - Descriptions, specifications
- **vendor**: Vendor information

### Integration Points

- PO validation via `Dao_InforVisualPO`
- Part lookups via `Dao_InforVisualPart`
- **CRITICAL**: All queries include `ApplicationIntent=ReadOnly` in connection string

## C# Models

### Module_Receiving Models

- `Model_ReceivingLoad`: Header for receiving transaction
- `Model_ReceivingLine`: Line-item details
- `Model_ReceivingSession`: Workflow state management
- `Model_InforVisualPO`: ERP purchase order data
- `Model_InforVisualPart`: ERP part data
- `Model_PackageTypePreference`: User package configurations
- `Model_ReceivingValidationResult`: Validation results
- `Model_CSVWriteResult`: Label generation results

### Module_Dunnage Models

- `Model_DunnageLoad`: Dunnage transaction header
- `Model_DunnageLine`: Transaction line items
- `Model_DunnageType`: Packaging categories
- `Model_DunnagePart`: Part numbers with home locations
- `Model_DunnageSpec`: Specification templates
- `Model_InventoriedDunnage`: Current inventory state
- `Model_CustomFieldDefinition`: Extensible metadata
- `Model_DunnageSession`: Workflow state management

### Module_Core Models

- `Model_User`: Authenticated user data
- `Model_Department`: Department definitions
- `Model_Application_Variables`: App-wide configuration

### Module_Shared Models

- Result wrappers for service operations
- Common enumerations

## Migration Strategy

- **MySQL**: Versioned SQL scripts in `Database/Schemas/`
  - Executed sequentially (01_, 02_, 03_...)
  - Migrations tracked in `Database/Migrations/`
- **SQL Server**: No migrations (Read-Only ERP system)

## Data Flow

1. User enters PO number → Validated against Infor Visual (Read-Only)
2. Part details retrieved from Infor Visual
3. Receiving data captured and stored in MySQL
4. CSV files generated for label printing (LabelView 2022)
5. Dunnage movements logged in MySQL
