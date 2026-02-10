# Impact Map - Module_Receiving

**Last Updated: 2025-01-15**

This file maps out what parts of the system are touched or affected when Module_Receiving changes. Use this to assess blast radius of changes and ensure proper testing and coordination.

---

## Upstream Dependencies (What Receiving Depends On)

### Module_Core
**Components used**:
- `ViewModel_Shared_Base` - Base class for all ViewModels
- `IService_ErrorHandler` - Error handling and user notifications
- `IService_LoggingUtility` - Application logging
- `IService_InforVisual` - ERP data access
- `IService_Dispatcher` - UI thread operations
- `IService_Help` - Help content system
- `Helper_Database_Variables` - Connection string management
- `Helper_Database_StoredProcedure` - MySQL stored procedure execution
- `Enum_ReceivingWorkflowStep` - Workflow step enumeration
- `Enum_ErrorSeverity`, `Enum_ValidationSeverity` - Error classification
- Various converters (BoolToVisibility, etc.)

**Impact if Core changes**:
- **Base class changes**: Must retest all ViewModels
- **Service interface changes**: Update all ViewModels and Services
- **Enum changes**: May require workflow logic updates
- **Helper changes**: Verify database operations still work

### Module_Shared
**Components used**:
- `View_Shared_HelpDialog` - Help content display
- `ViewModel_Shared_Base` - Alternative base for some scenarios

**Impact if Shared changes**:
- Help dialog changes affect help button functionality
- Base class changes require ViewModel retesting

### Infrastructure
**Components used**:
- Dependency Injection setup
- Database configuration
- Logging configuration

**Impact if Infrastructure changes**:
- DI changes may require service registration updates
- Database config changes affect connection strings
- Logging changes affect debug/troubleshooting

### External Dependencies
**NuGet packages**:
- `CommunityToolkit.Mvvm` - MVVM attributes and base classes
- `CommunityToolkit.WinUI.UI.Controls` - DataGrid control
- `CsvHelper` - CSV file generation
- `MySql.Data` - MySQL database access
- `Microsoft.Data.SqlClient` - SQL Server (ERP) access

**Impact if packages update**:
- Major version changes may require code updates
- Breaking changes in MVVM toolkit affect all ViewModels
- CSV format changes require label printer coordination
- Database driver changes require connection testing

---

## Downstream Dependencies (What Depends on Receiving)

### Module_Reporting
**How it depends**:
- Queries receiving database for reports
- Displays receiving history and metrics

**Impact of Receiving changes**:
- **Database schema changes**: Report queries must update
- **Field name changes**: Report columns may break
- **Status enum changes**: Report filters need updating

**Testing required**:
- Run all receiving-related reports
- Verify date ranges and filters work
- Check exported data integrity

### Module_Dunnage
**How it depends**:
- May integrate with receiving for returnable container tracking
- Shares package type data

**Impact of Receiving changes**:
- **Package type changes**: Dunnage must recognize new types
- **Session format changes**: If dunnage reads receiving session

**Testing required**:
- Test package type consistency
- Verify dunnage integration points

### Label Printing System (External)
**How it depends**:
- Reads CSV files from network path
- Expects specific CSV format and field names

**Impact of Receiving changes**:
- **CSV format changes**: Labels may print incorrectly or fail
- **Path changes**: Printers may not find files
- **Field name changes**: Label templates must update

**Testing required**:
- Test label printing after changes
- Verify CSV format with print system vendor
- Test both local and network path scenarios

### ERP System (Infor Visual)
**How it depends**:
- Receiving validates POs against ERP
- ERP may consume receiving data (reverse integration)

**Impact of Receiving changes**:
- **PO validation changes**: May reject valid POs
- **Data sync changes**: ERP may expect different format

**Testing required**:
- Validate all PO lookup scenarios
- Check for any reverse integration to ERP
- Verify data matches ERP expectations

---

## Internal Component Dependencies

### Views → ViewModels
**Coupling**:
- Each View is tightly bound to its ViewModel
- XAML uses x:Bind to ViewModel properties

**Impact**:
- **Property renames**: Must update XAML bindings
- **Command renames**: Must update XAML button bindings
- **Property type changes**: May require converter updates

### ViewModels → Services
**Coupling**:
- ViewModels call IService interfaces
- Services injected via constructor

**Impact**:
- **Service interface changes**: Must update all calling ViewModels
- **Return type changes**: Update ViewModel result handling
- **New parameters**: Update all service call sites

### Services → DAOs
**Coupling**:
- Services call DAO methods
- DAOs injected via constructor

**Impact**:
- **DAO signature changes**: Update service calls
- **Result format changes**: Update service result handling
- **Stored procedure changes**: Update DAO implementations

### DAOs → Database
**Coupling**:
- DAOs call MySQL stored procedures
- Stored procedures defined in database

**Impact**:
- **Stored procedure signature changes**: Update DAO parameter mapping
- **Return structure changes**: Update result parsing
- **Database moves**: Update connection strings

---

## Change Impact Matrix

| Change Type | Module_Routing | Module_Reporting | Label System | ERP | Internal Views | Internal ViewModels |
|-------------|---------------|------------------|--------------|-----|----------------|---------------------|
| Add new field | Low | Low | None | None | Medium | Medium |
| Change database schema | High | High | None | Low | Low | Low |
| Change CSV format | None | None | High | None | None | Low |
| Change workflow steps | None | None | None | None | High | High |
| Rename ViewModel property | None | None | None | None | High | Medium |
| Change service interface | None | None | None | None | Low | High |
| Update stored procedure | Low | Low | None | None | None | Low |
| Change package types | Medium | Low | Medium | None | Medium | Medium |

**Legend**:
- **None**: No expected impact
- **Low**: Minor impact, localized changes
- **Medium**: Moderate impact, coordinated testing needed
- **High**: Significant impact, cross-team coordination required

---

## Testing Blast Radius

### Minimal Change (e.g., fix typo in button label)
**Test**:
- Affected screen only
- No regression testing needed

### Isolated Logic Change (e.g., adjust validation rule)
**Test**:
- Affected workflow step
- Related validation scenarios
- Unit tests for changed method

### Service Layer Change
**Test**:
- All ViewModels calling the service
- Integration tests with database
- Manual smoke test of workflows

### Database Schema Change
**Test**:
- **Mandatory**: Full receiving workflow end-to-end
- All downstream modules (Routing, Reporting)
- Database migration script in test environment
- Rollback script verified

### CSV Format Change
**Test**:
- **Mandatory**: Label printing system integration
- Local and network CSV generation
- All package types and edge cases
- Coordinate with label system vendor

---

## Communication and Coordination Matrix

| Change Type | Notify Users | Notify Support | Notify IT | Notify Routing Team | Notify Label Vendor |
|-------------|--------------|----------------|-----------|---------------------|---------------------|
| UI change | Yes | Yes | No | No | No |
| Workflow change | Yes | Yes | No | Maybe | No |
| Database change | No | Yes | Yes | Yes | No |
| CSV change | No | Yes | Yes | Yes | Yes |
| Performance change | No | Yes | Yes | No | No |
| Integration change | Maybe | Yes | Yes | Yes | Maybe |

---

## Rollback Dependencies

If Module_Receiving must be rolled back:

**Can rollback independently**:
- UI changes (if database unchanged)
- ViewModel logic (if services unchanged)
- Validation rules (if database unchanged)

**Requires coordinated rollback**:
- Database schema changes (Routing and Reporting may need rollback too)
- CSV format changes (Label system may need template rollback)
- Integration changes (ERP queries may need version alignment)

**Cannot rollback without data loss**:
- Data migrations (must have tested rollback script)
- Enum changes that affected saved records

---

## Future Integration Points

**Planned integrations** (update as known):
- [ ] Volvo module integration for supplier-specific receiving
- [ ] Mobile device support for receiving on shop floor
- [ ] Barcode scanning integration
- [ ] EDI integration for advance ship notices

**Each integration adds**:
- New dependency relationship
- New testing requirements
- New coordination needs

**Update this map when**:
- New integration is added
- Module is refactored
- Dependency structure changes
- External system changes
