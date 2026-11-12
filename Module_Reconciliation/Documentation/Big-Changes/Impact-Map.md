# Impact Map

Last Updated: 2026-11-12

## Added

- `Module_Reconciliation/` — models, contracts, data access, collaborators, view models, views, converters, settings keys, navigation title provider.
- `Module_Settings.Reconciliation/` — the module's settings page and view model.
- `Database/Database_Deployment/Sql_Files/StoredProcedures/Reconciliation/` — three procedures.
- `Database/Database_Deployment/Sql_Files/Migrations/19_Migration_reconciliation_stored_procedures.sql` and its deployment wrapper.
- `Database/InforVisualScripts/Queries/Reconciliation/` — `18a`, `18b`, `18c`.
- Tests under `MTM_Receiving_Application.Tests/Unit/Module_Reconciliation/`.

## Changed

- `Module_Receiving/Contracts/IService_ReceivingLocationReconciliation.cs` — one method remains, with its signature unchanged.
- `Module_Receiving/Services/Service_ReceivingLocationReconciliation.cs` — recommendation kept, reconcile methods and the ignored-locations read deleted.
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` — the reconciliation step is gone.
- `Module_Receiving/Views/View_Receiving_Workflow.xaml` — the reconciliation host is gone.
- `Module_Receiving/Views/View_Receiving_ModeSelection.xaml` — the reconciliation card is gone.
- `Module_Receiving/Settings/ReceivingSettingsKeys.cs` and `ReceivingSettingsDefaults.cs` — two keys removed.
- `Module_Settings.Receiving/` — the user preferences, part formatting, business rules and category hub no longer mention reconciliation.
- `Module_Core/Models/Enums/Enum_ReceivingWorkflowStep.cs` — `ReconciliationReview` removed.
- `Module_Core/Services/Help/Service_Help.cs` — three entries removed.
- `Module_Core/Contracts/Services/IService_InforVisual.cs`, `Dao_InforVisualConnection.cs`, `Service_InforVisualConnect.cs` — query 17 and 18 plumbing removed.
- `Module_Core/Themes/ModuleAccentBrushes.xaml` — reconciliation accent triple added.
- `Infrastructure/DependencyInjection/ModuleServicesExtensions.cs` — the module's registrations added and the four old ones removed.
- `MainWindow.xaml` and `MainWindow.xaml.cs` — one navigation entry, accent mapping, launcher and settings routes.
- `MTM_Receiving_Application.Tests/Unit/Module_Receiving/Services/Service_ReceivingLocationReconciliationTests.cs` — trimmed to the recommendation cases.

## Deleted

- Receiving: the reconciliation review view model, its dialog and code-behind, its tests, and the two reconciliation models.
- Settings: the ignored-locations settings view and its code-behind.
- Core: the query 17 model and the query 18 movement model.
- Queries: `17_GetReceivingLocationTransactionHistory.sql` and `18_GetReceivingLocationTransferMovements.sql`.

## Deliberately untouched

- Query 16 and the entry-time recommendation.
- Receiving's edit-mode ownership rule.
- Receiving's proposed UI changes document and its renders.
- The legacy `docs/` folders in the other modules.

## Data

No tables and no columns were added or removed. The three procedures are the only new database objects.
