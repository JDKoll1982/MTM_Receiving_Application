# MTM Receiving Application Development Guide

## Project Overview
- **Purpose:** WinUI 3 desktop application for manufacturing receiving workflows, label generation, ERP lookups, settings, reporting, and support tooling.
- **Key Technologies:**
  - .NET 10
  - C# 13
  - WinUI 3
  - CommunityToolkit.Mvvm
  - MediatR and FluentValidation
  - MySQL 5.7 for application writes
  - SQL Server / Infor Visual for read-only ERP queries

## Getting Started
### Prerequisites
- Required software: .NET SDK 10, Visual Studio 2022 with Windows App SDK workload, MySQL 5.7
- Dependencies: Install dependencies using `dotnet restore`

### Installation Instructions
1. Clone the repository.
2. Open the solution in Visual Studio 2022.
3. Restore NuGet packages by running `dotnet restore` in the terminal.

### Basic Usage Examples
- Build and test:
  ```powershell
  dotnet build MTM_Receiving_Application.slnx
  dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj
  ```

## Project Structure
### Main Directories
- `Module_Core/`: Shared infrastructure, behaviors, converters, helpers, base models, services.
- `Module_Receiving/`: Receiving workflow and label-related features.
- `Module_Dunnage/`, `Module_Reporting/`, `Module_Volvo/`: Domain modules.
- `Module_Settings.*`: Settings subsystems and settings UI.
- `Module_Shared/`: Shared UI and cross-module presentation assets.
- `Infrastructure/`: Dependency injection, configuration, logging, app-wide plumbing.
- `Database/`: SQL scripts, schema assets, test data, and database deployment resources.
- `docs/`: Project documentation, CopilotForms assets, database references, and historical notes.
- `.github/`: Active AI customization files, prompt library, instructions, agents, and archive.
- `MTM_Receiving_Application.Tests/`: Unit and integration tests.

### Key Configuration Files
- `appsettings.json`
- `appsettings.Development.json`

## Development Workflow
### Coding Standards
- Follow MVVM architecture: View → ViewModel → Service → DAO → Database.
- Use `x:Bind` in XAML.
- Keep DAOs instance-based.
- Use stored procedures for MySQL access.
- Keep Infor Visual access read-only.

### Testing Approach
- Unit tests located in `MTM_Receiving_Application.Tests/`
- Run tests using:
  ```powershell
  dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj
  ```

### Build and Deployment Process
- Build solution:
  ```powershell
  dotnet build MTM_Receiving_Application.slnx
  ```
- Deploy database resources as needed.

### Contribution Guidelines
- Ensure all changes comply with MVVM and architecture rules.
- Update relevant documentation when changes invalidate it.

## Key Concepts
- **Domain-Specific Terminology:** Refer to domain-specific terms within modules.
- **Core Abstractions:** Shared infrastructure services, base models, converters.
- **Design Patterns Used:** MVVM pattern.

## Common Tasks
### Step-by-Step Guides
1. **Adding a New Module:**
   - Create a new directory under the appropriate module category (e.g., `Module_New/`).
   - Implement ViewModels and Views following MVVM rules.
   - Add necessary services and DAOs.
2. **Updating Database Scripts:**
   - Modify SQL scripts in the `Database/` directory.
   - Redeploy stored procedures as needed.

## Troubleshooting
### Common Issues and Solutions
- **Build Errors:** Ensure all dependencies are installed and restored.
- **Testing Failures:** Verify test configurations and data setup.

### Debugging Tips
- Use Visual Studio's debugging tools for step-by-step analysis.
- Check logs and console output for error messages.

## References
- [Project Documentation](docs/)
- [.github Instructions](.github/README.md)
- [Repository Rules](.github/copilot-instructions.md)
