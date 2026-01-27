# Module_Receiving Complete File Structure

**Version:** 1.0  
**Last Updated:** 2026-01-25  
**Purpose:** Complete folder and file tree for Module_Receiving implementation from scratch

## Visual Studio Solution Structure

```
MTM_Receiving_Application/
├── MTM_Receiving_Application.csproj
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
│
├── Module_Receiving/
│   ├── ViewModels/
│   │   ├── Hub/
│   │   │   ├── ViewModel_Receiving_Hub_Orchestration_MainWorkflow.cs
│   │   │   └── ViewModel_Receiving_Hub_Display_ModeSelection.cs
│   │   │
│   │   ├── Wizard/
│   │   │   ├── Orchestration/
│   │   │   │   └── ViewModel_Receiving_Wizard_Orchestration_MainWorkflow.cs
│   │   │   │
│   │   │   ├── Step1/
│   │   │   │   ├── ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
│   │   │   │   ├── ViewModel_Receiving_Wizard_Display_PartSelection.cs
│   │   │   │   └── ViewModel_Receiving_Wizard_Display_LoadCountEntry.cs
│   │   │   │
│   │   │   ├── Step2/
│   │   │   │   ├── ViewModel_Receiving_Wizard_Display_LoadDetailsGrid.cs
│   │   │   │   ├── ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations.cs
│   │   │   │   └── ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog.cs
│   │   │   │
│   │   │   └── Step3/
│   │   │       ├── ViewModel_Receiving_Wizard_Display_ReviewSummary.cs
│   │   │       ├── ViewModel_Receiving_Wizard_Orchestration_SaveOperation.cs
│   │   │       └── ViewModel_Receiving_Wizard_Display_CompletionScreen.cs
│   │   │
│   │   ├── Manual/
│   │   │   ├── ViewModel_Receiving_Manual_Orchestration_MainWorkflow.cs
│   │   │   ├── ViewModel_Receiving_Manual_Display_GridInterface.cs
│   │   │   ├── ViewModel_Receiving_Manual_Interaction_BulkOperations.cs
│   │   │   └── ViewModel_Receiving_Manual_Dialog_PreGridConfiguration.cs
│   │   │
│   │   └── Edit/
│   │       ├── ViewModel_Receiving_Edit_Orchestration_MainWorkflow.cs
│   │       ├── ViewModel_Receiving_Edit_Display_TransactionSearch.cs
│   │       ├── ViewModel_Receiving_Edit_Display_TransactionDetails.cs
│   │       └── ViewModel_Receiving_Edit_Display_AuditTrail.cs
│   │
│   ├── Views/
│   │   ├── Hub/
│   │   │   ├── View_Receiving_Hub_Orchestration_MainWorkflow.xaml
│   │   │   ├── View_Receiving_Hub_Orchestration_MainWorkflow.xaml.cs
│   │   │   ├── View_Receiving_Hub_Display_ModeSelection.xaml
│   │   │   └── View_Receiving_Hub_Display_ModeSelection.xaml.cs
│   │   │
│   │   ├── Wizard/
│   │   │   ├── Orchestration/
│   │   │   │   ├── View_Receiving_Wizard_Orchestration_MainWorkflow.xaml
│   │   │   │   └── View_Receiving_Wizard_Orchestration_MainWorkflow.xaml.cs
│   │   │   │
│   │   │   ├── Step1/
│   │   │   │   ├── View_Receiving_Wizard_Display_PONumberEntry.xaml
│   │   │   │   ├── View_Receiving_Wizard_Display_PONumberEntry.xaml.cs
│   │   │   │   ├── View_Receiving_Wizard_Display_PartSelection.xaml
│   │   │   │   ├── View_Receiving_Wizard_Display_PartSelection.xaml.cs
│   │   │   │   ├── View_Receiving_Wizard_Display_LoadCountEntry.xaml
│   │   │   │   └── View_Receiving_Wizard_Display_LoadCountEntry.xaml.cs
│   │   │   │
│   │   │   ├── Step2/
│   │   │   │   ├── View_Receiving_Wizard_Display_LoadDetailsGrid.xaml
│   │   │   │   ├── View_Receiving_Wizard_Display_LoadDetailsGrid.xaml.cs
│   │   │   │   ├── View_Receiving_Wizard_Dialog_CopyPreviewDialog.xaml
│   │   │   │   └── View_Receiving_Wizard_Dialog_CopyPreviewDialog.xaml.cs
│   │   │   │
│   │   │   └── Step3/
│   │   │       ├── View_Receiving_Wizard_Display_ReviewSummary.xaml
│   │   │       ├── View_Receiving_Wizard_Display_ReviewSummary.xaml.cs
│   │   │       ├── View_Receiving_Wizard_Display_CompletionScreen.xaml
│   │   │       └── View_Receiving_Wizard_Display_CompletionScreen.xaml.cs
│   │   │
│   │   ├── Manual/
│   │   │   ├── View_Receiving_Manual_Orchestration_MainWorkflow.xaml
│   │   │   ├── View_Receiving_Manual_Orchestration_MainWorkflow.xaml.cs
│   │   │   ├── View_Receiving_Manual_Display_GridInterface.xaml
│   │   │   ├── View_Receiving_Manual_Display_GridInterface.xaml.cs
│   │   │   ├── View_Receiving_Manual_Dialog_PreGridConfiguration.xaml
│   │   │   └── View_Receiving_Manual_Dialog_PreGridConfiguration.xaml.cs
│   │   │
│   │   └── Edit/
│   │       ├── View_Receiving_Edit_Orchestration_MainWorkflow.xaml
│   │       ├── View_Receiving_Edit_Orchestration_MainWorkflow.xaml.cs
│   │       ├── View_Receiving_Edit_Display_TransactionSearch.xaml
│   │       ├── View_Receiving_Edit_Display_TransactionSearch.xaml.cs
│   │       ├── View_Receiving_Edit_Display_TransactionDetails.xaml
│   │       ├── View_Receiving_Edit_Display_TransactionDetails.xaml.cs
│   │       ├── View_Receiving_Edit_Display_AuditTrail.xaml
│   │       └── View_Receiving_Edit_Display_AuditTrail.xaml.cs
│   │
│   ├── Requests/                                      ← CQRS Commands & Queries
│   │   ├── Commands/                                  ← Write operations
│   │   │   ├── SaveReceivingTransactionCommand.cs
│   │   │   ├── UpdateReceivingLineCommand.cs
│   │   │   ├── DeleteReceivingLineCommand.cs
│   │   │   ├── BulkCopyFieldsCommand.cs
│   │   │   ├── ClearAutoFilledFieldsCommand.cs
│   │   │   ├── SaveWorkflowSessionCommand.cs
│   │   │   └── CompleteWorkflowCommand.cs
│   │   │
│   │   └── Queries/                                   ← Read operations
│   │       ├── GetReceivingLinesByPOQuery.cs
│   │       ├── GetReceivingTransactionByIdQuery.cs
│   │       ├── GetPartDetailsQuery.cs
│   │       ├── GetWorkflowSessionQuery.cs
│   │       ├── SearchTransactionsQuery.cs
│   │       ├── GetAuditLogQuery.cs
│   │       └── ValidatePONumberQuery.cs
│   │
│   ├── Handlers/                                      ← CQRS Command/Query Handlers
│   │   ├── Commands/
│   │   │   ├── SaveReceivingTransactionCommandHandler.cs
│   │   │   ├── UpdateReceivingLineCommandHandler.cs
│   │   │   ├── DeleteReceivingLineCommandHandler.cs
│   │   │   ├── BulkCopyFieldsCommandHandler.cs
│   │   │   ├── ClearAutoFilledFieldsCommandHandler.cs
│   │   │   ├── SaveWorkflowSessionCommandHandler.cs
│   │   │   └── CompleteWorkflowCommandHandler.cs
│   │   │
│   │   └── Queries/
│   │       ├── GetReceivingLinesByPOQueryHandler.cs
│   │       ├── GetReceivingTransactionByIdQueryHandler.cs
│   │       ├── GetPartDetailsQueryHandler.cs
│   │       ├── GetWorkflowSessionQueryHandler.cs
│   │       ├── SearchTransactionsQueryHandler.cs
│   │       ├── GetAuditLogQueryHandler.cs
│   │       └── ValidatePONumberQueryHandler.cs
│   │
│   ├── Validators/                                    ← FluentValidation Validators
│   │   ├── SaveReceivingTransactionCommandValidator.cs
│   │   ├── UpdateReceivingLineCommandValidator.cs
│   │   ├── BulkCopyFieldsCommandValidator.cs
│   │   ├── GetReceivingLinesByPOQueryValidator.cs
│   │   ├── SearchTransactionsQueryValidator.cs
│   │   └── ValidatePONumberQueryValidator.cs
│   │
│   ├── Models/
│   │   ├── Entities/
│   │   │   ├── Model_Receiving_Entity_ReceivingLine.cs
│   │   │   ├── Model_Receiving_Entity_ReceivingTransaction.cs
│   │   │   ├── Model_Receiving_Entity_ReceivingLoad.cs
│   │   │   ├── Model_Receiving_Entity_CompletedTransaction.cs
│   │   │   ├── Model_Receiving_Entity_WorkflowSession.cs
│   │   │   ├── Model_Receiving_Entity_LoadDetail.cs
│   │   │   └── Model_Receiving_Entity_AuditLogEntry.cs
│   │   │
│   │   ├── DataTransferObjects/
│   │   │   ├── Model_Receiving_DataTransferObjects_PONumberValidationResult.cs
│   │   │   ├── Model_Receiving_DataTransferObjects_PartSearchResult.cs
│   │   │   ├── Model_Receiving_DataTransferObjects_BulkCopyOperation.cs
│   │   │   ├── Model_Receiving_DataTransferObjects_SaveOperationSummary.cs
│   │   │   ├── Model_Receiving_DataTransferObjects_TransactionSearchCriteria.cs
│   │   │   └── Model_Receiving_DataTransferObjects_LoadGridRow.cs
│   │   │
│   │   └── Results/
│   │       ├── Model_Receiving_Result_SaveOperation.cs
│   │       ├── Model_Receiving_Result_BulkCopyOperation.cs
│   │       └── Model_Receiving_Result_ValidationSummary.cs
│   │
│   ├── Data/                                          ← DAOs (Data Access Objects)
│   │   ├── Dao_Receiving_Repository_ReceivingLine.cs
│   │   ├── Dao_Receiving_Repository_ReceivingTransaction.cs
│   │   ├── Dao_Receiving_Repository_ReceivingLoad.cs
│   │   ├── Dao_Receiving_Repository_CompletedTransaction.cs
│   │   ├── Dao_Receiving_Repository_WorkflowSession.cs
│   │   └── Dao_Receiving_Repository_AuditLog.cs
│   │
│   └── Contracts/                                     ← Legacy Service Interfaces (if needed)
│       ├── IService_Receiving_Infrastructure_Settings.cs
│       ├── IService_Receiving_Infrastructure_CSVExport.cs
│       └── IService_Receiving_Infrastructure_SessionManagement.cs
│
├── Module_Shared/
│   ├── Helpers/
│   │   ├── Database/
│   │   │   ├── Helper_Database_Infrastructure_StoredProcedureExecution.cs
│   │   │   ├── Helper_Database_Infrastructure_ConnectionManagement.cs
│   │   │   └── Helper_Database_Infrastructure_ParameterBuilder.cs
│   │   │
│   │   ├── Validation/
│   │   │   ├── Helper_Validation_Business_PONumber.cs
│   │   │   ├── Helper_Validation_Business_PartNumber.cs
│   │   │   ├── Helper_Validation_Business_Weight.cs
│   │   │   └── Helper_Validation_Business_HeatLot.cs
│   │   │
│   │   ├── FileIO/
│   │   │   ├── Helper_FileIO_Infrastructure_CSVWriter.cs
│   │   │   └── Helper_FileIO_Infrastructure_CSVReader.cs
│   │   │
│   │   ├── Transformation/
│   │   │   ├── Helper_Transformation_Infrastructure_EntityMapper.cs
│   │   │   └── Helper_Transformation_Infrastructure_DataTransferObjectsConverter.cs
│   │   │
│   │   └── UI/
│   │       ├── Helper_UI_Infrastructure_WindowSizeAndStartupLocation.cs
│   │       └── Helper_UI_Infrastructure_FocusManagement.cs
│   │
│   ├── Enums/
│   │   ├── Enum_Receiving_State_WorkflowStep.cs
│   │   ├── Enum_Receiving_Mode_WorkflowMode.cs
│   │   ├── Enum_Receiving_Type_CopyFieldSelection.cs
│   │   ├── Enum_Receiving_Type_PackageType.cs
│   │   ├── Enum_Receiving_Type_PartType.cs
│   │   ├── Enum_Receiving_State_TransactionStatus.cs
│   │   ├── Enum_Shared_Severity_ErrorSeverity.cs
│   │   └── Enum_Shared_Type_ValidationResult.cs
│   │
│   └── ViewModels/
│       └── ViewModel_Shared_Base.cs
│
├── Module_Core/
│   ├── Behaviors/                                     ← MediatR Pipeline Behaviors
│   │   ├── LoggingBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   └── AuditBehavior.cs
│   │
│   ├── Models/
│   │   └── Core/
│   │       ├── Model_Dao_Result.cs
│   │       ├── Model_Dao_Result_Generic.cs
│   │       └── Model_Dao_Result_Factory.cs
│   │
│   └── Contracts/
│       └── Services/
│           ├── IService_ErrorHandler.cs
│           ├── IService_LoggingUtility.cs
│           └── IService_SessionManager.cs
│
├── Infrastructure/
│   └── DependencyInjection/
│       └── CqrsInfrastructureExtensions.cs
│
├── Converters/
│   ├── Converter_UI_BoolToVisibility.cs
│   ├── Converter_UI_InverseBool.cs
│   ├── Converter_UI_NullToVisibility.cs
│   └── Converter_UI_EnumToString.cs
│
├── Resources/
│   ├── Styles/
│   │   ├── ButtonStyles.xaml
│   │   ├── TextBoxStyles.xaml
│   │   ├── DataGridStyles.xaml
│   │   └── Colors.xaml
│   │
│   └── Images/
│       ├── icon_guided_mode.png
│       ├── icon_manual_mode.png
│       ├── icon_edit_mode.png
│       └── logo_mtm.png
│
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

## Test Project Structure

```
MTM_Receiving_Application.Tests/
├── MTM_Receiving_Application.Tests.csproj
│
├── Module_Receiving/
│   ├── ViewModels/
│   │   ├── Hub/
│   │   │   └── ViewModel_Receiving_Hub_Orchestration_MainWorkflowTests.cs
│   │   │
│   │   ├── Wizard/
│   │   │   ├── ViewModel_Receiving_Wizard_Display_PONumberEntryTests.cs
│   │   │   ├── ViewModel_Receiving_Wizard_Display_PartSelectionTests.cs
│   │   │   └── ViewModel_Receiving_Wizard_Display_LoadDetailsGridTests.cs
│   │   │
│   │   ├── Manual/
│   │   │   └── ViewModel_Receiving_Manual_Display_GridInterfaceTests.cs
│   │   │
│   │   └── Edit/
│   │       └── ViewModel_Receiving_Edit_Display_TransactionSearchTests.cs
│   │
│   ├── Handlers/
│   │   ├── Commands/
│   │   │   ├── SaveReceivingTransactionCommandHandlerTests.cs
│   │   │   ├── UpdateReceivingLineCommandHandlerTests.cs
│   │   │   ├── BulkCopyFieldsCommandHandlerTests.cs
│   │   │   └── CompleteWorkflowCommandHandlerTests.cs
│   │   │
│   │   └── Queries/
│   │       ├── GetReceivingLinesByPOQueryHandlerTests.cs
│   │       ├── GetPartDetailsQueryHandlerTests.cs
│   │       └── SearchTransactionsQueryHandlerTests.cs
│   │
│   ├── Data/                                          ← DAO Integration Tests
│   │   ├── Integration/
│   │   │   ├── Dao_Receiving_Repository_ReceivingLineIntegrationTests.cs
│   │   │   ├── Dao_Receiving_Repository_ReceivingTransactionIntegrationTests.cs
│   │   │   └── DatabaseFixture.cs
│   │   │
│   │   └── xunit.runner.json
│   │
│   └── Validators/
│       ├── SaveReceivingTransactionCommandValidatorTests.cs
│       ├── UpdateReceivingLineCommandValidatorTests.cs
│       ├── BulkCopyFieldsCommandValidatorTests.cs
│       └── ValidatePONumberQueryValidatorTests.cs
│
├── Module_Shared/
│   ├── Helpers/
│   │   ├── Helper_Validation_Business_PONumberTests.cs
│   │   ├── Helper_Validation_Business_PartNumberTests.cs
│   │   └── Helper_Database_Infrastructure_StoredProcedureExecutionTests.cs
│   │
│   └── Services/
│       └── Service_Shared_Infrastructure_ErrorHandlerTests.cs
│
└── TestData/
    ├── SamplePONumbers.csv
    ├── SamplePartNumbers.csv
    └── SampleReceivingTransactions.json
```

## Database Project Structure

```
MTM_Receiving_Database/
├── MTM_Receiving_Database.sqlproj
│
├── Tables/
│   ├── tbl_Receiving_Transaction.sql
│   ├── tbl_Receiving_Line.sql
│   ├── tbl_Receiving_Load.sql
│   ├── tbl_Receiving_CompletedTransaction.sql
│   └── tbl_Receiving_AuditLog.sql
│
├── StoredProcedures/
│   ├── ReceivingLine/
│   │   ├── sp_Receiving_Line_Insert.sql
│   │   ├── sp_Receiving_Line_Update.sql
│   │   ├── sp_Receiving_Line_Delete.sql
│   │   ├── sp_Receiving_Line_SelectByPO.sql
│   │   └── sp_Receiving_Line_SelectById.sql
│   │
│   ├── ReceivingTransaction/
│   │   ├── sp_Receiving_Transaction_Insert.sql
│   │   ├── sp_Receiving_Transaction_Update.sql
│   │   ├── sp_Receiving_Transaction_SelectByDateRange.sql
│   │   └── sp_Receiving_Transaction_SelectById.sql
│   │
│   └── CompletedTransaction/
│       ├── sp_Receiving_CompletedTransaction_Insert.sql
│       └── sp_Receiving_CompletedTransaction_SelectByPO.sql
│
├── Views/
│   └── vw_Receiving_TransactionSummary.sql
│
├── Functions/
│   └── fn_Receiving_CalculateTotalWeight.sql
│
└── Scripts/
    ├── Seed/
    │   └── SeedPartTypes.sql
    │
    └── Migration/
        └── 001_InitialSchema.sql
```

## Configuration Files

```
MTM_Receiving_Application/
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── nuget.config
│
└── .github/
    ├── copilot-instructions.md
    ├── CONSTITUTION.md
    └── workflows/
        ├── build.yml
        └── test.yml
```

## File Count Summary

**Production Code:**
- ViewModels: ~25 files
- Views (XAML + code-behind): ~50 files (25 XAML + 25 .cs)
- Requests (Commands + Queries): ~14 files
- Handlers (Command + Query Handlers): ~14 files
- Validators: ~6 files
- Models: ~15 files
- DAOs: ~6 files
- Helpers: ~15 files
- Enums: ~8 files
- Contracts (legacy service interfaces): ~3 files
- Module_Core Behaviors: ~3 files (shared)
- **Total Production:** ~159 files

**Test Code:**
- ViewModel Tests: ~10 files
- Handler Tests (Commands + Queries): ~10 files
- DAO Integration Tests: ~3 files
- Helper Tests: ~8 files
- Validator Tests: ~4 files
- **Total Test:** ~35 files

**Database:**
- Tables: ~5 files
- Stored Procedures: ~20 files
- Views: ~1 file
- **Total Database:** ~26 files

**Configuration:**
- ~10 files

**Grand Total:** ~230 files

## Implementation Checklist

**Phase 1: Foundation**
- [ ] Create all folder structures
- [ ] Create all Enum files
- [ ] Create all Model files
- [ ] Create all Helper files

**Phase 2: Data Layer**
- [ ] Create database tables
- [ ] Create stored procedures
- [ ] Create all DAO files
- [ ] Create DAO integration tests

**Phase 3: Business Layer**
- [ ] Create all Service interface files
- [ ] Create all Service implementation files
- [ ] Create Service unit tests

**Phase 4: Presentation Layer**
- [ ] Create all ViewModel files
- [ ] Create all View XAML files
- [ ] Create all View code-behind files
- [ ] Create ViewModel unit tests

**Phase 5: Integration**
- [ ] Register all DI components
- [ ] Configure appsettings.json
- [ ] Integration testing
- [ ] End-to-end testing

## Notes

- All file names MUST match class names exactly
- All folders use PascalCase (no underscores or hyphens)
- XAML files always paired with .xaml.cs code-behind
- Interface files (I*) always paired with implementation files
- Test files append "Tests" suffix to class name
- Database files use descriptive names matching table/SP names
