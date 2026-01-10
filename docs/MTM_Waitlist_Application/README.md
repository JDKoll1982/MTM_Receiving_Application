# MTM Waitlist Application

## Overview

This folder contains the complete project skeleton and planning documentation for a new **module-based waitlist application** that will replace and enhance the existing "Tables Ready" system.

> **Note**: This is a standalone project structure within the MTM Receiving Application documentation. The actual implementation will be a separate application project.

## Purpose

The waitlist application is designed to:
- Reduce operator training burden through guided wizards
- Minimize data entry errors with dropdown selections
- Provide role-based analytics for production leads
- Support zone-based material handling routing
- Integrate with Visual ERP (read-only) for work order validation
- Store all application state in MySQL

## Key Principles

### Module-Based Architecture
Each department/role has its own self-contained module:
- **Core**: Shell UI, navigation, error handling, logging, session management
- **Login**: Authentication and role-based access
- **Operator**: Guided wizards, favorites/recents
- **Wait List**: Create/update/close tasks, analytics (lead-only)
- **Material Handling**: Zone assignment, quick add for credit
- **Quality**: Quality-only queue with alerts
- **System Integration**: Visual ERP (read-only), site/IP detection
- **Training**: Guided training flows
- **Admin/Analytics**: Time standards, audit trail

### Hard Constraints
- **Visual ERP is READ ONLY** - SELECT queries only, never INSERT/UPDATE/DELETE
- **MySQL writes use stored procedures** - No raw SQL in application code
- **In-house only** - Not a web app, stays on company servers
- **User-friendly** - Minimize typing, maximize guided choices

## Folder Contents

### Project Skeleton
The complete folder structure for the waitlist application has been created. See [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) for a detailed breakdown of all folders and their purposes.

**Module Folders**:
- `Module_Core/` - Foundation services
- `Module_Login/` - Authentication
- `Module_Operator/` - Press operator workflows
- `Module_SetupTech/` - Setup technician tools
- `Module_Leads/` - Production lead analytics
- `Module_WaitList/` - Central task queue
- `Module_MaterialHandling/` - Material handler operations
- `Module_Quality/` - Quality control workflows
- `Module_Logistics/` - Truck and van logistics
- `Module_Training/` - Training programs
- `Module_SystemIntegration/` - Backend integration

**Support Folders**:
- `Assets/` - Icons, images, fonts
- `Database/` - Schemas, stored procedures, test data
- `Tests/` - Unit and integration tests
- `Documentation/` - Architecture and user guides
- `Scripts/` - Build and deployment scripts
- `.github/` - CI/CD and instructions
- `.vscode/` - Editor configuration

### Planning Documentation
### Planning Documentation

All planning documents are located in [Documentation/Planning/](Documentation/Planning/):

**Meeting Notes**:
- [meeting-transcript.md](Documentation/Planning/meeting-transcript.md) - Full meeting transcript from stakeholder discussion
- [meeting-summary.md](Documentation/Planning/meeting-summary.md) - Executive summary of meeting outcomes
- [meeting-outline.md](Documentation/Planning/meeting-outline.md) - Structured outline of topics discussed
- [module-breakdown.md](Documentation/Planning/module-breakdown.md) - Detailed module descriptions and features
- [file-structure-breakdown.md](Documentation/Planning/file-structure-breakdown.md) - Proposed MVVM folder structure

**Kickoff Documents**:
- [kickoff.md](Documentation/Planning/kickoff.md) - Original comprehensive kickoff blueprint
- [kickoff-revised-core-first.md](Documentation/Planning/kickoff-revised-core-first.md) - Core-first implementation approach
- [kickoff-stakeholder-version.md](Documentation/Planning/kickoff-stakeholder-version.md) - Stakeholder-aligned summary

## Technology Stack

- **Framework**: WinUI 3 on .NET 8
- **Architecture**: Strict MVVM with CommunityToolkit.Mvvm
- **Databases**: 
  - MySQL (mtm_receiving_application) - Full READ/WRITE access
  - SQL Server (Infor Visual - VISUAL/MTMFG) - **READ ONLY**
- **Dependency Injection**: Built-in .NET DI
- **Key Packages**: 
  - CommunityToolkit.Mvvm
  - Microsoft.Extensions.DependencyInjection
  - MySql.Data
  - Microsoft.Data.SqlClient

## Related Documentation

- [MCP Installation Guide](../mcp/01-install-and-configure.md) - MCP server setup for development
- [MCP Tool Catalog](../mcp/02-tool-catalog.md) - Available development tools
- [MCP Recipes](../mcp/03-recipes-cheat-sheet.md) - Quick reference for common tasks
- [MTM Receiving Application](../../README.md) - Parent project documentation

## Next Steps

1. Review all meeting documentation to understand stakeholder requirements
2. Review kickoff documents to understand technical approach
3. Begin implementation with Core module (foundation services)
4. Add feature modules incrementally based on priority
5. Gather feedback after each module rollout

## Status

**Planning Phase** - Documentation complete, awaiting implementation kickoff.

Last Updated: January 10, 2026
