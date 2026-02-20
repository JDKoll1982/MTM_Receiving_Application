# MTM Receiving Application - Project Brief

**Last Updated:** 2026-02-19

## Project Overview

MTM Receiving Application is a manufacturing receiving operations desktop application for streamlined label generation, workflow management, and ERP integration.

## Core Purpose

Enable efficient receiving workflows for manufacturing operations with:
- Label generation for incoming materials
- Integration with Infor Visual ERP system
- Workflow management for receiving processes
- Multi-module architecture supporting various receiving scenarios

## Technology Stack

- **Framework:** WinUI 3 (Windows App SDK 1.8+)
- **Language:** C# 12
- **Platform:** .NET 10
- **Architecture:** MVVM with CommunityToolkit.Mvvm + CQRS with MediatR
- **Database:** MySQL 8.0 (READ/WRITE), SQL Server/Infor Visual (READ ONLY)
- **Testing:** xUnit with FluentAssertions, Moq
- **DI Container:** Microsoft.Extensions.DependencyInjection

## Key Architectural Constraints

1. **MVVM Layer Flow:** View (XAML) → ViewModel → Service → DAO → Database
2. **ViewModels:** Partial classes inheriting from `ViewModel_Shared_Base`
3. **Services:** Interface-based with dependency injection
4. **DAOs:** Instance-based, return `Model_Dao_Result`
5. **XAML Bindings:** Use `x:Bind` (compile-time binding)
6. **Database Access:** MySQL via stored procedures, SQL Server READ ONLY

## Modules

- `Module_Core` - Shared infrastructure, helpers, base classes, CQRS behaviors
- `Module_Shared` - Shared ViewModels, Views, models
- `Module_Receiving` - Receiving workflow and label generation
- `Module_Dunnage` - Dunnage management
- `Module_Reporting` - Report generation
- `Module_Settings` - Configuration UI
- `Module_Volvo` - Volvo-specific integration

## Success Criteria

- All tests pass (unit and integration)
- Build succeeds without warnings
- Follows MTM architecture patterns (documented in copilot-instructions.md)
- Code is maintainable and well-documented

## Current Milestone

- `Module_Receiving` spreadsheet-removal lifecycle is implemented with MySQL queue/archive flow:
  - Save path: `receiving_label_data`
  - Clear path: transactional archive to `receiving_history`
