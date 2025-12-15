# MTM Receiving Application - Complete Setup Guide

**Project**: MTM Receiving Label Application  
**Platform**: WinUI 3 (.NET 8.0)  
**Pattern**: MVVM with Dependency Injection  
**Database**: MySQL 5.7.24  
**Created**: December 15, 2025

---

## Overview

This guide establishes the complete setup process for the MTM Receiving Application, designed to work alongside the `speckit` workflow for feature-driven development.

**Two-Phase Setup**:
1. **Phase 1**: Infrastructure (database, services, helpers) - **SETUP_PHASE_1_INFRASTRUCTURE.md**
2. **Phase 2**: MVVM Features (ViewModels, Views, navigation) - **SETUP_PHASE_2_MVVM_FEATURES.md**

---

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- MySQL 5.7.24 Server (localhost:3306)
- Visual Studio 2022 or VS Code
- Git

### Initial Setup

```powershell
# 1. Clone/Create project
cd C:\Users\johnk\source\repos\MTM_Receiving_Application

# 2. Create folder structure (already done)
# 27 folders created for MVVM organization

# 3. Follow Phase 1 setup
# See SETUP_PHASE_1_INFRASTRUCTURE.md

# 4. Verify Phase 1 complete
# Check verification checklist in Phase 1 doc

# 5. Follow Phase 2 setup
# See SETUP_PHASE_2_MVVM_FEATURES.md
```

---

## Documentation Structure

### Setup Documents (Execute in Order)
1. **SETUP_PHASE_1_INFRASTRUCTURE.md** - Core infrastructure setup
   - Copy templates from MTM WIP Application
   - Create models, enums, helpers
   - Configure database layer
   - Set up services (error handling, logging)
   - Create stored procedures
   - **Duration**: 2-3 hours
   - **Status**: ✅ Must complete before Phase 2

2. **SETUP_PHASE_2_MVVM_FEATURES.md** - MVVM feature development
   - Configure dependency injection
   - Create ViewModels and Views
   - Integrate with speckit workflow
   - Build features iteratively
   - **Duration**: 4-6 hours per feature
   - **Status**: ⏳ After Phase 1 complete

### GitHub Instructions (.github/instructions/)

Created during Phase 1, referenced throughout development:

- **database-layer.instructions.md** - DAO patterns, stored procedures, Model_Dao_Result
- **mvvm-pattern.instructions.md** - ViewModels, Views, data binding, commands
- **service-layer.instructions.md** - Service patterns, DI, error handling *(to be created)*
- **error-handling.instructions.md** - Service_ErrorHandler usage, logging *(to be created)*
- **dao-pattern.instructions.md** - Model_Dao_Result, async patterns *(to be created)*

### Speckit Configuration (.specify/)

- **config.json** - Project metadata, branch format, spec location
- **templates/spec-template.md** - Feature specification template
- **templates/tasks-template.md** - Technical task breakdown template
- **scripts/powershell/create-new-feature.ps1** - Branch creation script

### Final Documentation (Created LAST)

**Only after ALL features complete**:

- **AGENTS.md** - Agent configuration, context, workflows
- **.github/copilot-instructions.md** - Core Copilot instructions

---

## Workflow Integration

### Speckit Feature Development

```powershell
# 1. Create feature specification
/speckit.specify Add receiving label entry with Material ID lookup from Infor Visual

# Outputs:
# - Branch: feature/receiving-label-entry-1
# - Spec: specs/receiving-label-entry-1/spec.md
# - Checklist: specs/receiving-label-entry-1/checklists/requirements.md

# 2. Create technical plan
/speckit.plan Create a plan for the spec

# Outputs:
# - Tasks: specs/receiving-label-entry-1/tasks.md
# - MVVM breakdown (ViewModels, Views, DAOs, SPs)

# 3. Implement following tasks.md

# 4. Test and verify

# 5. Create pull request
```

### Development Flow

```
Phase 1: Infrastructure Setup (One-Time)
    ↓
Phase 2: Feature Development (Iterative)
    ↓
Speckit Feature Loop (For Each Feature):
    1. /speckit.specify → Create spec
    2. /speckit.plan → Create tasks
    3. Implement (ViewModels, Views, DAOs, SPs)
    4. Test
    5. Pull Request
    6. Merge
    ↓
Repeat for Next Feature
    ↓
All Features Complete → Create AGENTS.md & copilot-instructions.md
```

---

## Folder Structure

```
MTM_Receiving_Application/
├── SETUP_PHASE_1_INFRASTRUCTURE.md    ← START HERE (Phase 1)
├── SETUP_PHASE_2_MVVM_FEATURES.md     ← THEN THIS (Phase 2)
├── SETUP_GUIDE.md                     ← THIS FILE (Overview)
│
├── .github/
│   ├── instructions/                  ← Copilot instruction files
│   │   ├── database-layer.instructions.md
│   │   ├── mvvm-pattern.instructions.md
│   │   ├── service-layer.instructions.md *(to create)*
│   │   └── error-handling.instructions.md *(to create)*
│   ├── agents/                        ← Speckit agent definitions *(to create)*
│   └── prompts/                       ← Speckit prompts *(to create)*
│
├── .specify/
│   ├── config.json                    ← Speckit configuration
│   ├── templates/
│   │   ├── spec-template.md           ← Feature spec template
│   │   └── tasks-template.md          ← Task breakdown template
│   └── scripts/powershell/
│       └── create-new-feature.ps1     ← Branch creation script
│
├── Models/
│   ├── Receiving/                     ← Core models
│   │   ├── Model_Dao_Result.cs
│   │   ├── Model_Application_Variables.cs
│   │   └── Model_ReceivingLine.cs
│   ├── Labels/
│   ├── Enums/
│   └── Lookup/
│
├── ViewModels/                        ← MVVM ViewModels
│   ├── Receiving/
│   │   └── ReceivingLabelViewModel.cs
│   ├── Labels/
│   └── Shared/
│       └── BaseViewModel.cs
│
├── Views/                             ← XAML Pages
│   ├── Receiving/
│   │   ├── ReceivingLabelPage.xaml
│   │   └── ReceivingLabelPage.xaml.cs
│   ├── Labels/
│   └── Shared/
│
├── Services/                          ← Business logic services
│   ├── Database/
│   │   ├── Service_ErrorHandler.cs
│   │   └── LoggingUtility.cs
│   ├── Export/
│   ├── InforVisual/
│   └── LabelView/
│
├── Helpers/                           ← Helper utilities
│   ├── Database/
│   │   ├── Helper_Database_StoredProcedure.cs
│   │   └── Helper_Database_Variables.cs
│   ├── Formatting/
│   └── Validation/
│
├── Data/                              ← DAOs (Data Access Objects)
│   ├── Receiving/
│   │   └── Dao_ReceivingLine.cs
│   ├── Labels/
│   └── Lookup/
│
├── Contracts/                         ← Interfaces
│   ├── Services/
│   │   ├── IService_ErrorHandler.cs
│   │   └── ILoggingService.cs
│   └── Data/
│
├── Database/                          ← SQL Scripts
│   ├── Schemas/
│   │   └── 01_create_receiving_tables.sql
│   └── StoredProcedures/
│       ├── Receiving/
│       │   └── receiving_line_Insert.sql
│       └── Labels/
│
├── specs/                             ← Speckit feature specs
│   └── receiving-label-entry-1/
│       ├── spec.md
│       ├── tasks.md
│       └── checklists/
│           └── requirements.md
│
└── AGENTS.md                          ← Agent config (CREATE LAST)
└── .github/copilot-instructions.md    ← Copilot instructions (CREATE LAST)
```

---

## Key Patterns & Principles

### From MTM WIP Application Constitution

1. **Model_Dao_Result Pattern** - ALL DAO methods return this type
2. **Async-First** - ALL database operations are async
3. **Stored Procedures Only** - NO inline SQL in code
4. **Service_ErrorHandler** - Centralized error handling (NO MessageBox.Show)
5. **Structured Logging** - CSV format via LoggingUtility
6. **MySQL 5.7.24 Only** - NO 8.0+ features (JSON, CTEs, window functions)

### New for WinUI 3 / MVVM

7. **MVVM Pattern** - Strict separation: View → ViewModel → Service/DAO → Database
8. **CommunityToolkit.Mvvm** - Use `ObservableObject`, `RelayCommand`, `[ObservableProperty]`
9. **Dependency Injection** - ALL services and ViewModels registered in DI
10. **x:Bind** - Compile-time binding (NOT `Binding` runtime binding)
11. **Speckit Workflow** - Feature-driven development with specs and tasks

---

## Verification Checklists

### Phase 1 Complete Checklist

- [ ] All 16 template files copied
- [ ] Core models created (Model_Dao_Result, Model_Application_Variables, Model_ReceivingLine)
- [ ] Enums created (Enum_ErrorSeverity, Enum_LabelType)
- [ ] Database helpers configured
- [ ] Services configured (Service_ErrorHandler, LoggingUtility)
- [ ] DAO pattern established (at least one DAO)
- [ ] MySQL database created
- [ ] Tables created
- [ ] At least one stored procedure created and tested
- [ ] GitHub instruction files created (database-layer, mvvm-pattern)
- [ ] Project builds without errors
- [ ] Can connect to MySQL database successfully

### Phase 2 Complete Checklist (Per Feature)

- [ ] Feature spec created via `/speckit.specify`
- [ ] Technical plan created via `/speckit.plan`
- [ ] ViewModel implemented
- [ ] View (XAML) created
- [ ] ViewModels registered in DI
- [ ] Views registered in DI
- [ ] Commands use `RelayCommand`
- [ ] Error handling uses `Service_ErrorHandler`
- [ ] Logging uses `ILoggingService`
- [ ] DAO methods return `Model_Dao_Result<T>`
- [ ] All database calls are async
- [ ] Feature tested end-to-end
- [ ] Pull request created and reviewed

---

## Next Steps

### Immediate Actions

1. **Read SETUP_PHASE_1_INFRASTRUCTURE.md**
2. **Execute all Phase 1 steps** (2-3 hours)
3. **Verify Phase 1 checklist** (must be 100% complete)
4. **Read SETUP_PHASE_2_MVVM_FEATURES.md**
5. **Create first feature with speckit**: `/speckit.specify Add receiving label entry`

### Long-Term Roadmap

- **Weeks 1-2**: Phase 1 + First 3 features (Receiving, Dunnage, Routing labels)
- **Weeks 3-4**: Data management features (Save to History, Auto-Fill, Sort)
- **Weeks 5-6**: Label generation and export (Preview, CSV, Multi-Coil)
- **Week 7**: Supporting features (Search, Infor Visual, Settings)
- **Week 8**: Polish, testing, documentation (AGENTS.md, copilot-instructions.md)

---

## Support Resources

### Documentation Files
- **Database Layer**: `.github/instructions/database-layer.instructions.md`
- **MVVM Pattern**: `.github/instructions/mvvm-pattern.instructions.md`
- **Phase 1 Setup**: `SETUP_PHASE_1_INFRASTRUCTURE.md`
- **Phase 2 Features**: `SETUP_PHASE_2_MVVM_FEATURES.md`

### Reference Materials
- **Google Sheets Logic**: `RECEIVINGLABELLOGIC-GOOGLEAPPSCRIPTS.MD`, `DUNNAGELABELLOGIC-GOOGLEAPPSCRIPTS.md`, `UPSFEDEXLABELLOGIC-GOOGLEAPPSCRIPTS.md`
- **MTM WIP Templates**: `_TEMPLATE_*.txt` files in various folders
- **Speckit Workflow**: `.specify/` directory

### External Resources
- **WinUI 3 Docs**: https://learn.microsoft.com/en-us/windows/apps/winui/
- **CommunityToolkit.Mvvm**: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
- **MySQL 5.7 Docs**: https://dev.mysql.com/doc/refman/5.7/en/

---

## Troubleshooting

### Common Issues

**Issue**: Can't connect to MySQL
- **Solution**: Verify MAMP running, check connection string in `Helper_Database_Variables.cs`

**Issue**: Template files not found
- **Solution**: Run copy script from Phase 1 instructions

**Issue**: Model_Dao_Result not recognized
- **Solution**: Ensure Phase 1 complete, namespace updated to `MTM_Receiving_Application`

**Issue**: ViewModel not updating View
- **Solution**: Check `Mode=TwoWay` in XAML, ensure `[ObservableProperty]` attribute on properties

**Issue**: Speckit commands not working
- **Solution**: Verify `.specify/config.json` exists, check PowerShell execution policy

---

**Ready to Begin!**

Start with **SETUP_PHASE_1_INFRASTRUCTURE.md** and work through systematically. Phase 1 is foundational - do NOT skip to Phase 2!

🚀 **Happy Building!**

---

**Document Version**: 1.0  
**Last Updated**: December 15, 2025  
**Author**: GitHub Copilot + MTM Development Team
