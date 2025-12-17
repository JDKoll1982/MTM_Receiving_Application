# Service Contracts

This directory contains interface definitions for all services used in the receiving workflow feature.

## Services Overview

### Database Services
- **IService_InforVisual.md** - Query PO and Part data from Infor Visual (SQL Server)
- **IService_MySQL_Receiving.md** - Save receiving loads to MySQL database
- **IService_MySQL_PackagePreferences.md** - Manage package type preferences

### Application Services
- **IService_SessionManager.md** - Persist and restore session state via JSON
- **IService_CSVWriter.md** - Write receiving data to CSV files
- **IService_ReceivingValidation.md** - Validate receiving data and business rules
- **IService_ReceivingWorkflow.md** - Orchestrate the workflow state machine

## Implementation Notes

All services follow async/await patterns for I/O operations. Services are registered in `App.xaml.cs` using dependency injection and injected into ViewModels via constructor injection.

See individual contract files (.md format) for detailed method signatures and documentation. These are planning documents and will be converted to actual .cs files during implementation.
