# MTM Receiving Application

Modern WinUI 3 desktop application for manufacturing receiving operations with integrated authentication and label printing capabilities.

## Overview

The MTM Receiving Application streamlines the receiving process for manufacturing facilities by providing:
- **Multi-tier authentication system** with automatic workstation detection
- **Label generation** for receiving, dunnage, and routing operations
- **Database integration** with MySQL for data persistence and audit trails
- **Session management** with automatic timeout for security

## Features

### Authentication System

The application implements a comprehensive authentication system that automatically adapts to the workstation type:

#### Personal Workstation (Auto-Login)
- Automatic Windows username authentication
- No credentials required for office/supervisor computers
- 30-minute inactivity timeout

#### Shared Terminal (PIN Login)
- Username + 4-digit PIN authentication
- Designed for shop floor communal computers
- 15-minute inactivity timeout
- 3-attempt lockout security

#### New User Creation
- Self-service account creation for new employees
- Department and shift assignment
- Optional Visual/Infor ERP credential storage
- PIN uniqueness validation

**[Full Authentication Documentation](Documentation/AUTHENTICATION.md)**

### Session Management

- Automatic activity tracking (mouse, keyboard, window activation)
- Configurable timeout durations based on workstation type
- Graceful session termination with audit logging
- Comprehensive activity log for security auditing

### Receiving Workflow

A guided, step-by-step workflow for receiving materials:
- **PO Entry**: Validate PO numbers against Infor Visual ERP.
- **Part Selection**: Select parts from PO or enter non-PO items.
- **Load Entry**: Specify number of loads/pallets.
- **Weight & Quantity**: Enter weight and quantity per load.
- **Heat/Lot Entry**: Track heat and lot numbers for traceability.
- **Package Type**: Select package types with user preferences.
- **Review**: Verify all data before saving.
- **Persistence**: Saves to local CSV, network CSV, and MySQL database.

### Label Generation

_(Coming soon)_
- Receiving labels with barcode integration
- Dunnage/packing material labels
- Internal routing labels

### Help System

The application includes a centralized, context-sensitive help system:

#### Features
- **Contextual Help Dialogs**: Step-by-step guidance for each workflow step
- **Tooltips**: Informative button and field tooltips throughout the UI
- **Placeholders**: Dynamic placeholder text for input fields
- **Tips**: Workflow-specific tips and best practices
- **Related Topics**: Navigation to related help content
- **Search**: Full-text search across all help content

#### Using the Help System

**In Workflow Views**:
- Click the **Help** button (❓) in any workflow to see context-specific guidance
- Hover over buttons and fields for tooltips
- Look for tip icons (💡) for workflow-specific advice

**Help Content Categories**:
- **Dunnage Workflow**: Complete guidance for dunnage receiving process
- **Receiving Workflow**: PO-based receiving with Infor Visual integration
- **Admin**: Dunnage type and part management instructions

**Keyboard Shortcuts** _(coming soon)_:
- `F1` - Context-sensitive help for current screen

#### For Developers

The help system is implemented as a centralized service:

```csharp
// Inject help service in ViewModel
public MyViewModel(IService_Help helpService, ...)
{
    _helpService = helpService;
}

// Show contextual help dialog
await _helpService.ShowContextualHelpAsync(currentStep);

// Get tooltip/placeholder text
string tooltip = _helpService.GetTooltip("Button.Save");
string placeholder = _helpService.GetPlaceholder("Field.PONumber");
```

**Architecture**:
- **Service**: `Services/Help/Service_Help.cs` (Singleton)
- **Dialog**: `Views/Shared/Shared_HelpDialog.xaml` (Transient)
- **Model**: `Models/Core/Model_HelpContent.cs`
- **Content**: 100+ help entries loaded at startup

See [Help System Architecture memory](Serena: `help_system_architecture`) for complete documentation.

## Technology Stack

- **Framework**: WinUI 3 (.NET 8.0)
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Database**: MySQL 8.0+ (application data), SQL Server (Infor Visual - READ ONLY)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **UI Toolkit**: Windows App SDK 1.8
- **Icons**: Material.Icons.WinUI3

## Development Guidelines

This project follows a formal constitution that defines core principles and development standards:

📋 **[Project Constitution](.specify/memory/constitution.md)** - Required reading for all contributors

**Key Principles**:
1. **MVVM Architecture** - Strict separation of concerns (ViewModels, Views, Models, Services)
2. **Database Layer Consistency** - Model_Dao_Result pattern, stored procedures only, async operations
3. **Dependency Injection** - All services registered with interfaces, constructor injection
4. **Error Handling & Logging** - Centralized error handling and CSV-based logging
5. **Security & Authentication** - Multi-tier auth with audit trails
6. **WinUI 3 Modern Practices** - x:Bind, ObservableCollection, async/await
7. **Specification-Driven Development** - Speckit workflow for all features

**Critical Constraints**:
- ⚠️ **Infor Visual Database is STRICTLY READ ONLY** - No writes allowed
- MySQL 5.7.24+ compatibility required (no JSON functions, CTEs, window functions)
- See constitution for complete guidelines

## Prerequisites

- Windows 10 (version 1809) or later / Windows 11
- .NET 8.0 SDK or later
- MySQL 8.0+ database server
- Visual Studio 2022 (recommended) or VS Code

## Installation

### 1. Clone Repository

```powershell
git clone https://github.com/JDKoll1982/MTM_Receiving_Application.git
cd MTM_Receiving_Application
```

### 2. Database Setup

Run the database schema scripts in order:

```powershell
# From MySQL command line or MySQL Workbench
mysql -u root -p < Database/Schemas/01_create_receiving_tables.sql
mysql -u root -p < Database/Schemas/02_create_authentication_tables.sql

# Deploy stored procedures
cd Database/Deploy
powershell -ExecutionPolicy Bypass -File Deploy-Database.ps1
```

### 3. Configure Connection String

Update the database connection in `Helpers/Database/Helper_Database_Variables.cs`:

```csharp
private static readonly string ProductionConnectionString = 
    "Server=172.16.1.104;Database=mtm_receiving_application;Uid=your_username;Pwd=your_password;";
```

### 4. Build and Run

```powershell
dotnet restore
dotnet build
dotnet run
```

Or open `MTM_Receiving_Application.slnx` in Visual Studio 2022 and press F5.

## Configuration

### Workstation Setup

Configure workstations in the database:

**Personal Workstation** (Windows auto-login):
```sql
-- Computer defaults to personal workstation if not in config
-- No configuration needed unless changing to shared terminal
```

**Shared Terminal** (PIN login):
```sql
INSERT INTO workstation_config (computer_name, workstation_type, description)
VALUES ('SHOP2', 'shared_terminal', 'Shop floor terminal - Receiving area');
```

### Department Management

Add departments for user assignment:

```sql
INSERT INTO departments (department_name, is_active, sort_order)
VALUES ('Production', TRUE, 10),
       ('Quality Control', TRUE, 20),
       ('Shipping & Receiving', TRUE, 30);
```

**[Full Database Administration Guide](Documentation/DATABASE_ADMIN.md)**

## Project Structure

```
MTM_Receiving_Application/
├── Assets/                          # Images, icons, resources
├── Contracts/                       # Service interfaces
│   └── Services/                    # IService_Authentication, IService_SessionManager, etc.
├── Data/                            # Data Access Objects (DAOs)
│   ├── Authentication/              # User, session data access
│   └── Receiving/                   # Label data access
├── Database/                        # SQL scripts and deployment
│   ├── Schemas/                     # Table definitions
│   ├── StoredProcedures/            # Stored procedure scripts
│   └── Deploy/                      # Deployment scripts
├── Documentation/                   # Technical documentation
│   ├── AUTHENTICATION.md            # Authentication system guide
│   └── DATABASE_ADMIN.md            # Database administration guide
├── Helpers/                         # Utility classes
│   ├── Database/                    # Database helpers
│   ├── Formatting/                  # Data formatting utilities
│   └── Validation/                  # Input validation
├── Models/                          # Data models and business logic
│   ├── Enums/                       # Enumeration types
│   └── Receiving/                   # Label models
├── Services/                        # Business logic services
│   ├── Authentication/              # Auth and session services
│   ├── Database/                    # Error handling, logging
│   └── Startup/                     # Application lifecycle
├── Tests/                           # Manual test files
├── ViewModels/                      # MVVM ViewModels
│   ├── Shared/                      # Login, user setup VMs
│   └── Receiving/                   # Label VMs
└── Views/                           # XAML views
    ├── Shared/                      # Login dialogs
    └── Receiving/                   # Label views
```

## Usage

### First Launch

1. **Personal Workstation**: Application detects Windows username and attempts auto-login
   - If username not found: New User Setup dialog appears
   - Fill in full name, department, shift, PIN
   - Click "Create Account"

2. **Shared Terminal**: PIN Login dialog appears
   - Enter username and 4-digit PIN
   - Click "Login"

### Session Management

- Application monitors user activity automatically
- Idle timeout: 30 minutes (personal) / 15 minutes (shared)
- Application closes automatically on timeout
- All activity logged to database for audit trail

### Creating New Users

On personal workstations, new users can create accounts:
1. Launch application with unknown Windows username
2. New User Setup dialog appears automatically
3. Fill required fields (full name, department, shift, PIN)
4. Optionally configure ERP credentials
5. Click "Create Account" - employee number assigned
6. Automatic authentication with new account

## Testing

### Manual Tests

Manual test classes are available in the `Tests/` folder:

```csharp
// Test database connectivity
await Phase1_Manual_Tests.RunAllTests();

// Test model validation
Phase5_Model_Verification.RunAllTests();
```

### Runtime Testing

- **T120-T122**: Shared terminal PIN login tests
- **T165-T170**: New user creation tests
- **T180-T182**: ERP credential storage tests

See [tasks.md](specs/002-user-login/tasks.md) for complete test scenarios.

## Troubleshooting

### Application Won't Start

**Check**:
1. .NET 8.0 runtime installed
2. MySQL service running
3. Database connection string correct
4. Database schema deployed

### Authentication Fails

**Check**:
1. User exists in database: `SELECT * FROM users WHERE windows_username = 'username';`
2. Account is active: `SELECT is_active FROM users WHERE ...;`
3. Workstation configured correctly: `SELECT * FROM workstation_config WHERE ...;`

### Database Connection Errors

**Check**:
1. MySQL service running: `services.msc` → MySQL80
2. Connection string correct in `Helper_Database_Variables.cs`
3. Database exists: `SHOW DATABASES;`
4. Stored procedures deployed

**[Complete Troubleshooting Guide](Documentation/DATABASE_ADMIN.md#troubleshooting)**

## Development

### Building from Source

```powershell
# Restore NuGet packages
dotnet restore

# Build application
dotnet build --configuration Debug

# Run application
dotnet run

# Build for release
dotnet build --configuration Release
```

### Code Style

- **Architecture**: MVVM pattern with ViewModels and Views separated
- **DI**: Constructor injection for all services
- **Async**: Use async/await for all I/O operations
- **Naming**: PascalCase for public members, _camelCase for private fields
- **Documentation**: XML comments for all public APIs

### Adding New Features

1. Create service interface in `Contracts/Services/`
2. Implement service in `Services/`
3. Create models in `Models/`
4. Create ViewModel in `ViewModels/`
5. Create View in `Views/`
6. Register service in `App.xaml.cs` ConfigureServices()

## Contributing

### Branch Structure

- `001-phase1-infrastructure`: Core infrastructure (default)
- `002-user-login`: Authentication system (current)
- Feature branches: Follow `###-feature-name` pattern

### Pull Request Process

1. Create feature branch from appropriate base
2. Implement feature with tests
3. Update documentation
4. Submit PR with description
5. Code review and approval
6. Merge to base branch

## Security

### Authentication Security

- PINs: 4-digit numeric, plain text storage (physical facility access)
- 3-attempt lockout prevents brute force attacks
- All authentication events logged for audit
- Session timeout ensures automatic logout

### Database Security

- Parameterized queries prevent SQL injection
- Stored procedures enforce business logic
- Database access controls limit exposure
- Activity log tracks all operations

### ERP Credentials

- Stored in plain text (required for ERP API)
- Database access controls protect data
- User warned during account creation
- Optional feature, not required

## License

[Specify License]

## Support

For issues, questions, or feature requests:
- **GitHub Issues**: [Project Issues Page]
- **Documentation**: See `Documentation/` folder
- **Email**: [Contact Email]

## Acknowledgments

- **WinUI 3**: Microsoft Windows App SDK team
- **CommunityToolkit**: .NET Community
- **MySQL**: Oracle Corporation

---

**Version**: 1.0.0  
**Last Updated**: December 2025  
**Platform**: Windows 10/11  
**.NET Version**: 8.0
