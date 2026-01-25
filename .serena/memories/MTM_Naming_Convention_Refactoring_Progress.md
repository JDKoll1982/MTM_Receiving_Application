# MTM Receiving Module - 5-Part Naming Convention Refactoring Progress

## Objective
Systematically rename all files and classes in `Module_Receiving` to follow the 5-part naming standard:
`{Type}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

Where:
- **Type**: Class type (ViewModel, View, Service, Model, DAO, Helper, Enum, Command, Query, Handler, Validator)
- **Module**: Receiving
- **Mode**: Wizard (old 12-step), Manual (manual entry), EditMode (edit/review), or omitted for shared classes
- **CategoryType**: Why it exists (Display, Orchestration, Business, Repository, Entity, Navigation, Data, Copy, etc.)
- **DescriptiveName**: Clear, specific description of what it represents

## Completed ✅

### Commands (9 files)
- ✅ All renamed to `Command_Receiving_Wizard_*` format
- Files AND classes renamed
- Examples: `Command_Receiving_Wizard_Navigation_StartNewWorkflow`, `Command_Receiving_Wizard_Data_EnterOrderAndPart`

### Queries (5 files)
- ✅ All renamed to `Query_Receiving_Wizard_*` format
- Files AND classes renamed
- Examples: `Query_Receiving_Wizard_Get_CurrentSession`, `Query_Receiving_Wizard_Validate_CurrentStep`

### Handlers (14 files)
- ✅ All renamed to `Handler_Receiving_Wizard_*` format
- Files AND classes renamed
- Examples: `Handler_Receiving_Wizard_Copy_FieldsToEmptyCells`, `Handler_Receiving_Wizard_Data_SaveAndExportCSV`

### ViewModels (10 files - Old Wizard System)
- ✅ All renamed to `ViewModel_Receiving_Wizard_*` format
- File AND class names updated
- Example: `ViewModel_Receiving_Wizard_Display_PONumberEntry`
- Special case: `ViewModel_Receiving_EditMode_Interaction_EditHandler`

### Views (10 files + 10 XAML files - Old Wizard System)
- ✅ All code-behind and XAML files renamed to `View_Receiving_Wizard_*` format
- File AND class names updated

### Validators (6 files)
- ✅ All renamed to `Validator_Receiving_Wizard_*` format
- Files AND classes renamed
- Examples: `Validator_Receiving_Wizard_Navigation_StartNewWorkflow`, `Validator_Receiving_Wizard_Copy_FieldsToEmptyCells`

### DAOs (4 files completed, 2 pending)
- ✅ Renamed: `Dao_ReceivingLoad`, `Dao_ReceivingLine`, `Dao_QualityHold`, `Dao_PackageTypePreference`
- Format: `Dao_Receiving_Repository_*`
- ⏳ Pending: `Dao_ReceivingWorkflowSession`, `Dao_ReceivingLoadDetail` (reference issues)

**Total: ~58+ files and 100+ class references updated**

## Remaining Work (Not Yet Started)

### Services (18 files)
- Service_Receiving_Business_* files (MySQL operations)
- Service_Receiving_Infrastructure_* files (CSV, SessionManager, Settings)
- Need to categorize and rename to 5-part format

### Models (16+ files)
- Model_Receiving_Entity_* (domain models)
- Model_Receiving_DTO_* (data transfer objects)
- Model_Receiving_Result_* (operation results)
- Need proper categorization and naming

### Contracts/Interfaces (8 files)
- IService_MySQL_* interfaces
- IService_Receiving* interfaces  
- Need proper categorization with 5-part format

### Settings (2 files)
- ReceivingSettingsDefaults.cs
- ReceivingSettingsKeys.cs

## Key Patterns Established

### Wizard Mode (Old 12-Step System)
- All CQRS classes use: `{Type}_Receiving_Wizard_{CategoryType}_{Name}`
- All UI classes (ViewModels/Views) use: `{Type}_Receiving_Wizard_{CategoryType}_{Name}`
- All validators use: `Validator_Receiving_Wizard_{CategoryType}_{Name}`

### Shared/Generic Classes
- DAOs: `Dao_Receiving_Repository_{Name}` (Mode omitted)
- Services: To be determined (likely also omit Mode or use generic category)

### EditMode Classes
- ViewModel: `ViewModel_Receiving_EditMode_Interaction_EditHandler`
- View: `View_Receiving_EditMode_Interaction_EditHandler`

## Next Priority
1. Complete the 2 pending DAOs
2. Rename Services systematically  
3. Update Models with proper categorization
4. Update Contracts/Interfaces
5. Do final cross-reference sweep and build validation

## Build Status
- Last known: Build had compilation errors in EditMode ViewModel (legacy code with missing properties)
- Ready to validate once Services/Models renaming complete
