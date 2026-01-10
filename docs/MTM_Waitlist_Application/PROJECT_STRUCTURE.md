# MTM Waitlist Application - Project Structure

## Overview

This folder contains the complete project skeleton for the **MTM Waitlist Application**, a module-based Windows desktop application designed to replace and enhance the existing "Tables Ready" system.

## Project Structure

```
MTM_Waitlist_Application/
│
├── Module_Core/                      # Core foundation services
│   ├── Models/                       # Core data models
│   ├── Views/                        # Core UI components (status bar, etc.)
│   ├── ViewModels/                   # Core coordinators and base ViewModels
│   ├── Services/                     # Foundation services (logging, error handling, etc.)
│   ├── Contracts/                    # Service interfaces
│   ├── Helpers/                      # Utility classes and helpers
│   ├── Themes/                       # Application-wide themes and styles
│   └── Converters/                   # Value converters for XAML bindings
│
├── Module_Login/                     # Authentication module
│   ├── Models/                       # User account models
│   ├── Views/                        # Login forms and UI
│   └── ViewModels/                   # Login handlers and authentication logic
│
├── Module_Operator/                  # Press operator module
│   ├── Models/                       # Operator profiles and tasks
│   ├── Views/                        # Operator dashboards and wizards
│   ├── ViewModels/                   # Operator workflow logic
│   ├── Services/                     # Operator-specific services
│   └── Data/                         # Operator data access
│
├── Module_SetupTech/                 # Setup technician module
│   ├── Models/                       # Setup tasks and change requests
│   ├── Views/                        # Setup tech dashboards
│   └── ViewModels/                   # Setup workflow logic
│
├── Module_Leads/                     # Production leads module
│   ├── Models/                       # Lead profiles and analytics rights
│   ├── Views/                        # Lead dashboards
│   └── ViewModels/                   # Analytics and supervisor logic
│
├── Module_WaitList/                  # Waitlist queue module
│   ├── Models/                       # Waitlist items and analytics models
│   ├── Views/                        # Queue displays and quick add UI
│   ├── ViewModels/                   # Waitlist management logic
│   ├── Services/                     # Waitlist services
│   └── Data/                         # Waitlist data access
│
├── Module_MaterialHandling/          # Material handler module
│   ├── Models/                       # Material tasks and zone assignments
│   ├── Views/                        # Material handler panels
│   ├── ViewModels/                   # Zone-based task logic
│   ├── Services/                     # Material handling services
│   └── Data/                         # Material data access
│
├── Module_Quality/                   # Quality control module
│   ├── Models/                       # Quality tasks and alerts
│   ├── Views/                        # Quality dashboards
│   ├── ViewModels/                   # Quality workflow logic
│   ├── Services/                     # Quality alert services
│   └── Data/                         # Quality data access
│
├── Module_Logistics/                 # Logistics module
│   ├── Models/                       # Truck/van tasks and location mapping
│   ├── Views/                        # Logistics dashboards
│   └── ViewModels/                   # Logistics workflow logic
│
├── Module_Training/                  # Training module
│   ├── Models/                       # Training programs and sessions
│   ├── Views/                        # Training step displays
│   └── ViewModels/                   # Training flow logic
│
├── Module_SystemIntegration/         # System integration module
│   ├── Models/                       # Integration configs and IP mapping
│   ├── Views/                        # Integration status displays
│   └── ViewModels/                   # Integration monitoring logic
│
├── Assets/                           # Application assets
│   ├── Icons/                        # Application icons
│   ├── Images/                       # Images and graphics
│   └── Fonts/                        # Custom fonts
│
├── Database/                         # Database scripts
│   ├── Schemas/                      # Table definitions
│   ├── StoredProcedures/             # MySQL stored procedures
│   ├── TestData/                     # Sample data for testing
│   └── Migrations/                   # Database migration scripts
│
├── Tests/                            # Test projects
│   ├── Unit/                         # Unit tests
│   └── Integration/                  # Integration tests
│
├── Documentation/                    # Technical documentation
│   ├── Architecture/                 # Architecture diagrams and decisions
│   ├── UserGuides/                   # End-user documentation
│   ├── API/                          # API documentation
│   └── Planning/                     # Project planning documents
│       ├── meeting-transcript.md     # Stakeholder meeting transcript
│       ├── meeting-summary.md        # Meeting summary
│       ├── meeting-outline.md        # Meeting outline
│       ├── module-breakdown.md       # Module descriptions
│       ├── file-structure-breakdown.md # Original file structure
│       ├── kickoff.md                # Original kickoff
│       ├── kickoff-revised-core-first.md # Core-first approach
│       └── kickoff-stakeholder-version.md # Stakeholder summary
│
├── Scripts/                          # Build and deployment scripts
│
├── .github/                          # GitHub configuration
│   ├── workflows/                    # CI/CD workflows
│   └── instructions/                 # Copilot instructions
│
├── .vscode/                          # VS Code configuration
│
└── [Planning Documents]              # Project planning and meeting notes
    ├── README.md                     # Project overview
    ├── PROJECT_STRUCTURE.md          # This file
    └── Documentation/Planning/       # All planning documents
        ├── meeting-transcript.md     # Meeting transcript
        ├── meeting-summary.md        # Meeting summary
        ├── meeting-outline.md        # Meeting outline
        ├── module-breakdown.md       # Module descriptions
        ├── file-structure-breakdown.md # Original file structure
        ├── kickoff.md                # Original kickoff
        ├── kickoff-revised-core-first.md # Core-first approach
        └── kickoff-stakeholder-version.md # Stakeholder summary
```

## Module Descriptions

### Module_Core
Foundation services used by all other modules:
- Logging and error handling
- Messaging/event bus
- Application state management
- Base ViewModels and UI components
- Themes and styling

### Module_Login
User authentication and authorization:
- Badge + PIN authentication
- Role-based access control
- User session management

### Module_Operator
Press operator workflows:
- Guided wizards for task requests
- Favorites and recents for quick access
- Dashboard for operator-specific tasks
- Work order integration

### Module_SetupTech
Setup technician tools:
- Change request management
- Module configuration
- Setup task tracking

### Module_Leads
Production lead analytics and oversight:
- Analytics dashboards
- Time tracking and reporting
- Supervisor controls
- Team performance metrics

### Module_WaitList
Central task queue:
- Task creation and management
- Priority-based sorting
- Quick add functionality
- Analytics (lead-only)
- Auto-assignment logic

### Module_MaterialHandling
Material handler operations:
- Zone-based task assignment
- Project tracking
- Quick add for non-waitlist tasks
- Material movement logging

### Module_Quality
Quality control workflows:
- Quality-only task queue
- Alert configuration
- Notification management
- Quality issue tracking

### Module_Logistics
Truck and van logistics:
- Truck task assignment
- Van task management
- Location mapping
- Red-flag validation

### Module_Training
Operator and technician training:
- Training program management
- Session tracking
- Guided training flows
- Progress monitoring

### Module_SystemIntegration
Backend system integration:
- Visual ERP (read-only) integration
- Site/IP detection and mapping
- MySQL data storage
- Configuration management

## Architecture Principles

### MVVM Pattern
- **Models**: Pure data classes matching database schemas
- **Views**: XAML-only, no business logic
- **ViewModels**: Business logic, inherit from BaseViewModel, use `[ObservableProperty]` and `[RelayCommand]`

### Module Independence
- Each module is self-contained
- Changes to one module do not affect others
- Modules communicate through Core services/events
- Modules can be developed and tested independently

### Database Strategy
- **MySQL**: Application data (full CRUD via stored procedures)
- **SQL Server (Infor Visual)**: READ ONLY for validation and lookups
- All MySQL operations use stored procedures (no raw SQL in code)

### Deployment
- Copy-folder deployment via MTM Application Loader
- Installed folder is read-only at runtime
- Runtime artifacts stored in %LOCALAPPDATA%
- Shared settings stored in MySQL

## Technology Stack

- **Framework**: WPF on .NET 8 (or WinUI 3 alternative)
- **Language**: C# 12
- **MVVM**: CommunityToolkit.Mvvm
- **DI**: Microsoft.Extensions.DependencyInjection
- **Databases**:
  - MySQL 8+ (application data)
  - SQL Server 2019+ (Infor Visual ERP - read-only)
- **Testing**: xUnit
- **Logging**: Microsoft.Extensions.Logging

## Implementation Strategy

### Phase 1: Core Foundation
1. Set up Module_Core with base services
2. Implement authentication (Module_Login)
3. Create base UI shell and navigation

### Phase 2: Essential Modules
1. Module_Operator (guided wizards)
2. Module_WaitList (basic queue)
3. Module_MaterialHandling (zone assignment)

### Phase 3: Enhanced Features
1. Module_Quality (alerts and notifications)
2. Module_Leads (analytics)
3. Module_Training (guided flows)

### Phase 4: Advanced Modules
1. Module_SetupTech
2. Module_Logistics
3. Module_SystemIntegration enhancements

## Next Steps

1. Review all planning documents in this folder
2. Set up development environment
3. Initialize project structure with actual files
4. Begin Core module implementation
5. Iteratively add modules based on priority

## References

- [Main README](README.md) - Project overview and specifications
- [Planning Documents](Documentation/Planning/) - All meeting notes and kickoff documents
- [Module Breakdown](Documentation/Planning/module-breakdown.md) - Detailed module feature lists
- [Kickoff Documents](Documentation/Planning/kickoff.md) - Implementation blueprints
- [Meeting Notes](Documentation/Planning/meeting-transcript.md) - Stakeholder requirements

---

Last Updated: January 10, 2026
