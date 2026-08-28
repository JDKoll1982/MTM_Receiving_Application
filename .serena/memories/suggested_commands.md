<!-- 
[DOC-META-START]
- File Name: suggested_commands.md
- Description: Build, test, run, deploy, and architecture-validation commands.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 16-19: # Suggested Commands
  - Line 20-38: ## Build & Test
  - Line 39-42: ## Run the Application
  - Line 43-50: ## Database Deployment
  - Line 51-64: ## Architecture Validation Searches
- Critical Notes: Tests run via the test csproj, not the .slnx.
[DOC-META-END]
-->

# Suggested Commands

Last Updated: 2026-03-21

## Build & Test

```powershell
# Build the solution
dotnet build MTM_Receiving_Application.slnx

# Build full release
dotnet build MTM_Receiving_Application.slnx -c Release /p:Platform=x64

# Run all tests
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj

# Run only unit tests
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Integration"
```

## Run the Application

- Use Visual Studio "Start Debugging" (F5) or "Start Without Debugging" (Ctrl+F5).

## Database Deployment

Deploy stored procedures to MySQL by running individual `.sql` files in `Database/StoredProcedures/`:

```powershell
mysql -h 172.16.1.104 -P 3306 -u root -p mtm_receiving_application_test < Database/StoredProcedures/sp_example.sql
```

## Architecture Validation Searches

Run these to check for forbidden patterns after changes:

```powershell
# Find ViewModels calling DAOs directly (should be 0 results)
Select-String -Path "Module_*/ViewModels/**/*.cs" -Pattern "Dao_" -Recurse

# Find static DAO classes (should be 0 results)
Select-String -Path "**/*.cs" -Pattern "static class Dao_" -Recurse

# Find runtime Binding in XAML (should be 0 results)
Select-String -Path "**/*.xaml" -Pattern "{Binding " -Recurse
```
