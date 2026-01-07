# MTM Receiving Application - Development Guide

## Prerequisites

### Development Environment
- **Operating System**: Windows 10 version 1809 (build 17763) or later
- **IDE**: Visual Studio 2022 (Community, Professional, or Enterprise)
  - Workload: .NET Desktop Development
  - Workload: Universal Windows Platform development
- **SDK**: .NET 8 SDK
- **Windows App SDK**: 1.8+ (installed via NuGet)

### Database Requirements
- **MySQL Server 8.x**: Application database server
  - Host: localhost:3306
  - Database: `mtm_receiving_application`
  - User: `root` (development)
  - Character Set: utf8mb4
- **SQL Server**: Infor Visual ERP (READ-ONLY access)
  - Server: `VISUAL`
  - Database: `MTMFG`
  - User: `SHOP2` (read-only account)
  - Connection: `ApplicationIntent=ReadOnly` required

### Database Development Standards
- **Stored Procedures**: All application logic must use stored procedures.
- **Idempotency**: All SQL scripts (schemas, seed data, migrations) must be idempotent (safe to run multiple times).
  - Use `IF NOT EXISTS` for table creation.
  - Use `INSERT IGNORE` or `ON DUPLICATE KEY UPDATE` for seed data.
  - Use `DROP PROCEDURE IF EXISTS` before `CREATE PROCEDURE`.
- **No Raw SQL**: C# code must strictly call stored procedures, not execute raw SQL queries.

### Additional Tools
- **MySQL CLI**: For database deployment and testing
- **LabelView 2022**: For printing receiving/dunnage labels (CSV-driven)
- **PowerShell 5.1+**: For build scripts
- **Git**: Version control

## Environment Setup

### 1. Clone Repository
```powershell
git clone https://github.com/JDKoll1982/MTM_Receiving_Application.git
cd MTM_Receiving_Application
```

### 2. Configure Database Connection
Edit `appsettings.json` to match your environment:

```json
{
    "AppSettings": {
        "UseInforVisualMockData": false,
        "Environment": "Development"
    },
    "ConnectionStrings": {
        "MySQL": "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;CharSet=utf8mb4;",
        "InforVisual": "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;"
    }
}
```

**Note**: For development without ERP access, set `UseInforVisualMockData: true` to use mock data.

### 3. Deploy MySQL Database
```powershell
# Navigate to Database directory
cd Database

# Deploy schemas in order
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Schemas/01_create_receiving_tables.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Schemas/02_create_authentication_tables.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Schemas/03_create_receiving_tables.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Schemas/04_create_package_preferences.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Schemas/06_create_dunnage_tables.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < Schemas/07_create_dunnage_tables_v2.sql

# Deploy stored procedures
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < StoredProcedures/Authentication/sp_GetUserByWindowsUsername.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < StoredProcedures/Authentication/sp_ValidateUserPin.sql
# ... (deploy all stored procedures)

# Load test data (optional)
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < TestData/insert_test_data.sql
mysql -h localhost -P 3306 -u root -p mtm_receiving_application < TestData/010_seed_dunnage_complete.sql
```

### 4. Restore NuGet Packages
```powershell
dotnet restore
```

## Build & Run

### Build Commands

#### Debug Build (Default)
```powershell
dotnet build
# or
dotnet build /p:Platform=x64
```

#### Release Build
```powershell
dotnet build -c Release /p:Platform=x64
```

#### Visual Studio Build (for XAML error diagnostics)
```powershell
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
& "$vs\Common7\IDE\devenv.com" MTM_Receiving_Application.slnx /Build "Debug|x64"
```

### Run Application

#### From Visual Studio
1. Open `MTM_Receiving_Application.slnx` in Visual Studio
2. Set platform to **x64**
3. Press F5 to run with debugging

#### From Command Line
```powershell
dotnet run
```

#### Packaged Application (for deployment testing)
```powershell
dotnet publish -c Release /p:Platform=x64 /p:PublishReadyToRun=false
```

## Testing

### Run All Tests
```powershell
dotnet test
```

### Run Specific Test Categories
```powershell
# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"

# Specific test class
dotnet test --filter "FullyQualifiedName~Dao_ReceivingLoad_Tests"
```

### Database Integration Tests
Integration tests require live MySQL connection to test database:
- Tests use separate test database schema
- Stored procedures are tested against actual MySQL
- Infor Visual integration uses mock data (no live ERP required)

## Development Workflow

### Adding a New Feature Module

1. **Create Module Folder Structure**
   ```
   Module_{FeatureName}/
   ├── Data/               # DAOs
   ├── Enums/              # Enumerations
   ├── Interfaces/         # Service contracts
   ├── Models/             # Domain models
   ├── Services/           # Business logic
   ├── ViewModels/         # MVVM ViewModels
   └── Views/              # XAML Views
   ```

2. **Register Services in DI** (`App.xaml.cs`)
   ```csharp
   // DAOs (Singleton)
   services.AddSingleton(sp => new Dao_MyFeature(mySqlConnectionString));
   
   // Services (Singleton or Transient)
   services.AddTransient<IService_MyFeature, Service_MyFeature>();
   
   // ViewModels (Transient)
   services.AddTransient<ViewModel_MyFeature>();
   ```

3. **Create Database Schema** (`Database/Schemas/`)
   - Follow naming convention: `{NN}_create_{feature}_tables.sql`
   - Version incrementally

4. **Create Stored Procedures** (`Database/StoredProcedures/{Feature}/`)
   - All MySQL operations must use stored procedures
   - Naming: `sp_{table}_{operation}.sql`

5. **Build and Test**
   ```powershell
   dotnet build
   dotnet test
   ```

### XAML Troubleshooting

If `dotnet build` fails with generic `XamlCompiler.exe exited with code 1` error:

```powershell
# Get detailed XAML errors
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
& "$vs\Common7\IDE\devenv.com" MTM_Receiving_Application.slnx /Rebuild "Debug|x64" 2>&1 | Select-String "error|warning" | Select-Object -Last 30
```

Common XAML issues:
- **WMC1121**: Type mismatch in binding (e.g., `DateTime` to `DateTimeOffset?`)
- **WMC1110**: Property not found on ViewModel (check spelling, ensure ViewModel is `partial`)
- **Invalid x:Bind**: Always specify `Mode=OneWay`, `Mode=TwoWay`, or `Mode=OneTime`

## Configuration Files

### appsettings.json
- **UseInforVisualMockData**: Set to `true` for development without ERP access
- **Environment**: `Development` or `Production`
- **ConnectionStrings**: MySQL and SQL Server connections

### App.xaml.cs
- Dependency Injection container setup
- Service registrations (DAOs, Services, ViewModels)
- Application lifecycle management

## Common Development Tasks

### Adding a New ViewModel
1. Create partial class inheriting from `BaseViewModel`
2. Use `[ObservableProperty]` for bindable properties
3. Use `[RelayCommand]` for commands
4. Inject dependencies via constructor
5. Register in `App.xaml.cs` DI

### Adding a New DAO
1. Create instance-based class (no static)
2. All methods return `Model_Dao_Result` or `Model_Dao_Result<T>`
3. Use `Helper_Database_StoredProcedure.ExecuteStoredProcedureAsync()`
4. Register as Singleton in `App.xaml.cs`

### Creating a Stored Procedure
1. Write SQL in `Database/StoredProcedures/{Module}/sp_{operation}.sql`
2. Deploy to MySQL via CLI
3. Create corresponding DAO method
4. Never write raw SQL in C# code

### Debugging Tips
- Use Visual Studio debugger for step-through debugging
- Check `IsBusy` flags in ViewModels to prevent re-entry
- Validate database connections via `appsettings.json`
- Use `IService_ErrorHandler` for user-friendly error messages
- Check MySQL logs for stored procedure errors

## Build Output

### Debug Build
- **Location**: `bin/x64/Debug/net8.0-windows10.0.22621.0/`
- **Executable**: `MTM_Receiving_Application.exe`
- **Symbols**: `.pdb` files included for debugging

### Release Build
- **Location**: `bin/x64/Release/net8.0-windows10.0.22621.0/`
- **Optimizations**: Disabled (WinUI 3 compatibility)
- **ReadyToRun**: Disabled (COM interop issues)
- **Trimming**: Disabled (XAML binding issues)

## Continuous Integration

Currently no CI/CD pipeline configured. Future considerations:
- GitHub Actions for automated builds
- Automated testing on pull requests
- Deployment to test environments

## Additional Resources

- **AI Agent Instructions**: `AGENTS.md` - Guidelines for AI-assisted development
- **Architecture Patterns**: `.github/instructions/mvvm-pattern.instructions.md`
- **DAO Patterns**: `.github/instructions/dao-pattern.instructions.md`
- **Window Sizing**: `.github/instructions/window-sizing.instructions.md`
