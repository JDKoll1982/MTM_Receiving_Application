# Reusable Services - Developer Guide

## Core Services

### 1. Logging Service
**Files**:
- `Services/Logging/ILoggingService.cs`
- `Services/Logging/Service_Logging.cs`

**Dependencies**: `Helper_LogPath`, `Model_Application_Variables`

### 2. Error Handler Service
**Files**:
- `Services/ErrorHandling/IService_ErrorHandler.cs`
- `Services/ErrorHandling/Service_ErrorHandler.cs`

**Dependencies**: `ILoggingService`, WinUI 3 ContentDialog

### 3. Database Helper (Multi-Database)
**Files**:
- `Helpers/Helper_Database_StoredProcedure.cs` (MySQL)
- `Helpers/Helper_SqlServer_StoredProcedure.cs` (SQL Server)

**Dependencies**: `MySql.Data`, `Microsoft.Data.SqlClient`, `Model_Dao_Result`

## Setup Instructions

See REUSABLE_SERVICES_SETUP.md for complete setup guide.

### Quick Start
1. Copy service files to new project
2. Update namespaces
3. Install NuGet packages
4. Configure database connection strings
5. Register in DI container

---

**Last Updated**: December 2025  
**Version**: 1.0.0
