# Material Icons Migration - Master Checklist Index

**Purpose**: Complete task tracking for migration from Segoe Fluent Icons to Material.Icons.WinUI3  
**Created**: 2025-12-29  
**Feature**: Icon System Migration to Material Design Icons  
**Total Phases**: 9  
**Estimated Total Time**: 4.5-6 hours  
**Status**: Not Started

---

## ⚠️ CRITICAL: Legacy Code Removal is MANDATORY

All legacy FontIcon code, converters, and references **MUST** be removed. This is **NOT OPTIONAL**.

---

## Checklist Files (All Required)

### ✅ Phase 1: Discovery & Analysis (1 hour)
**File**: [checklist-01-discovery-analysis.md](checklist-01-discovery-analysis.md)  
**Status**: ✅ Complete  
**Tasks**: 33 discovery items + 7 analysis sections = **40 total**  
**Output**: icon-inventory.md with 200+ icon locations catalogued

**Key Deliverables**:
- Complete XAML icon catalog (33 files)
- Code-behind dynamic icon usage documentation
- Converter analysis (Converter_IconCodeToGlyph.cs)
- IconPickerControl structure documentation
- Database icon storage validation

---

### ✅ Phase 2: Package Installation & Configuration (15 minutes)
**File**: [checklist-02-package-installation.md](checklist-02-package-installation.md)  
**Status**: ✅ Complete  
**Tasks**: 12 installation items + 7 documentation items = **19 total**  
**Output**: Material.Icons.WinUI3 NuGet package installed and configured

**Key Deliverables**:
- NuGet package installed
- App.xaml resource dictionary updated
- Test page created and verified (then deleted)
- Build verification passed
- Documentation updated

---

### ✅ Phase 3: Icon Mapping Strategy (1 hour)
**File**: [checklist-03-icon-mapping.md](checklist-03-icon-mapping.md)  
**Status**: ✅ Complete  
**Tasks**: 60 icon mappings + 15 compatibility checks = **75 total**  
**Output**: icon-mapping.md with Fluent → Material equivalents

**Key Deliverables**:
- 60+ unique glyph codes mapped
- Visual comparison screenshots
- Size compatibility table (10 sizes)
- Color/theme compatibility verification
- Design review for ambiguous icons

---

### ✅ Phase 4: IconPickerControl Refactor (2 hours)
**File**: [checklist-04-icon-picker-refactor.md](checklist-04-icon-picker-refactor.md)  
**Status**: ✅ Complete  
**Tasks**: ~50 (XAML refactor + C# logic + enum integration + search functionality)

**Key Deliverables**:
- IconPickerControl.xaml updated to GridView with ItemTemplate
- IconPickerControl.xaml.cs refactored with MaterialIconKind enum
- Searchable 5000+ icons
- Recently used icons persistence
- Parent dialog integration (3 locations)

---

### ✅ Phase 5: XAML File Migration (1.5 hours)
**File**: [checklist-05-xaml-migration.md](TO BE CREATED)  
    **NOTE**: You are to use the serena mcp server to get a full list of xaml files that need to be updated from the old standard icon to the new material.winui3 icons.
**Status**: ⬜ Not Started  
**Tasks**: ~200+ (one per icon instance found in discovery)

**Key Deliverables**:
- All 33 XAML files updated
- FontIcon → MaterialIcon replacements
- Namespace additions (xmlns:materialIcons)
- Visual regression testing
- Size/alignment adjustments

---

### ✅ Phase 6: Code-Behind Migration (45 minutes)
**File**: [checklist-06-code-behind-migration.md](TO BE CREATED)  
**Status**: ⬜ Not Started  
**Tasks**: ~30 (converter replacement + dynamic icon creation)

**Key Deliverables**:
- Converter_IconCodeToGlyph.cs **DELETED** (MANDATORY)
- New Converter_IconCodeToMaterialKind.cs created
- All `new FontIcon` replaced with `new MaterialIcon`
- Legacy glyph fallback logic (migration period only)

---

### ✅ Phase 7: Database Migration (30 minutes)
**File**: [checklist-07-database-migration.md](TO BE CREATED)  
**Status**: ⬜ Not Started  
**Tasks**: ~25 (SQL script + data validation + seed data update)

**Key Deliverables**:
- Database backup created
- Migration script: Database/Migrations/011-material-icons-migration.sql
- Unicode glyphs → MaterialIconKind enum names
- Seed data updated (010-dunnage-complete-seed.sql)
- Migration validation (0 legacy glyphs remaining)

---

### ✅ Phase 8: Testing & Validation (1 hour)
**File**: [checklist-08-testing-validation.md](TO BE CREATED)  
**Status**: ⬜ Not Started  
**Tasks**: ~40 (visual regression + functional + performance + cross-theme)

**Key Deliverables**:
- Visual regression test cases passed (20+ scenarios)
- Functional testing completed (icon picker, search, selection)
- Performance benchmarks met (<100ms load time)
- Cross-theme compatibility verified (Light/Dark/High Contrast)

---

### ✅ Phase 9: Documentation & Cleanup (30 minutes) **MANDATORY**
**File**: [checklist-09-documentation-cleanup.md](TO BE CREATED)  
**Status**: ⬜ Not Started  
**Tasks**: ~35 (documentation + **MANDATORY legacy code removal**)

**Key Deliverables**:
- Documentation/ICONS.md created (developer guide)
- Architecture documentation updated
- **Converter_IconCodeToGlyph.cs DELETED** (MANDATORY)
- **All FontIcon references removed** (MANDATORY)
- **Unicode glyph constants/helpers deleted** (MANDATORY)
- tasks.md updated with completion status

---

## Overall Progress Tracking

**Total Tasks Across All Phases**: ~550 checklist items

### Completion Status by Phase

| Phase | Name | Tasks | Status | Time |
|-------|------|-------|--------|------|
| 1 | Discovery & Analysis | 40 | ⬜ Not Started | 1h |
| 2 | Package Installation | 19 | ⬜ Not Started | 15m |
| 3 | Icon Mapping | 75 | ⬜ Not Started | 1h |
| 4 | IconPicker Refactor | ~50 | ⬜ Not Started | 2h |
| 5 | XAML Migration | ~200 | ⬜ Not Started | 1.5h |
| 6 | Code-Behind Migration | ~30 | ⬜ Not Started | 45m |
| 7 | Database Migration | ~25 | ⬜ Not Started | 30m |
| 8 | Testing & Validation | ~40 | ⬜ Not Started | 1h |
| 9 | **Cleanup (MANDATORY)** | ~35 | ⬜ Not Started | 30m |

**Total Estimated Time**: 4.5-6 hours

---

## Critical Success Criteria

Before marking migration as COMPLETE, verify:

- [ ] All 550+ checklist items completed across all 9 phases
- [ ] Zero FontIcon elements remain in XAML files
- [ ] Zero Unicode glyph references in code (except fallback converter during migration)
- [ ] Converter_IconCodeToGlyph.cs **DELETED** (non-negotiable)
- [ ] All icon data in database migrated to MaterialIconKind enum names
- [ ] Build succeeds with zero errors
- [ ] All visual regression tests passed
- [ ] Performance benchmarks met
- [ ] Documentation complete
- [ ] **Legacy code removal verified** (MANDATORY)

---

## How to Use These Checklists

### 1. Sequential Execution
Checklists **MUST** be completed in order (Phase 1 → Phase 9). Do not skip phases.

### 2. Task Completion
- Mark tasks with `[X]` when complete: `- [X] CHK001-01 Task description`
- Add notes inline if issues encountered
- Link to relevant files/commits for traceability

### 3. Phase Sign-Off
After completing all tasks in a phase:
- Update phase status in this master index
- Update phase "Completion Criteria" section
- Commit changes before proceeding to next phase

### 4. Rollback Procedures
Each checklist includes rollback instructions if critical issues arise. Execute rollback, document reason, and reassess approach.

---

## Dependencies & Prerequisites

### Before Starting
- [ ] Git branch created: `011-material-icons-migration`
- [ ] Database backup created: `mtm_receiving_application_backup_YYYYMMDD.sql`
- [ ] All pending PRs merged to avoid conflicts
- [ ] Team notified of upcoming migration

### Technical Requirements
- [ ] .NET 8 SDK installed
- [ ] Visual Studio 2022 (or VS Code with C# extensions)
- [ ] MySQL client for database migration
- [ ] Git for version control

---

## Risks & Mitigation

| Risk | Impact | Mitigation | Checklist Reference |
|------|--------|------------|---------------------|
| Icon size mismatch | Medium | Size compatibility table in Phase 3 | checklist-03 CHK024-* |
| Missing Material equivalent | Low | Design review in Phase 3 | checklist-03 CHK022-* |
| Database migration failure | Critical | Backup before migration | checklist-07 |
| Performance degradation | Medium | Benchmark testing in Phase 8 | checklist-08 |
| Theme color incompatibility | Medium | Cross-theme testing in Phase 8 | checklist-08 |

---

## Communication Plan

### Stakeholder Updates
- **Daily**: Update master checklist status
- **Per Phase**: Create progress summary in CHAT_SUMMARY_REPORT.md
- **Completion**: Demo updated UI with Material icons

### Issue Escalation
If critical blockers arise:
1. Document issue in plan.md "Issues Encountered" section
2. Execute rollback if necessary
3. Notify team lead
4. Reassess approach before resuming

---

## Post-Migration Verification

After all 9 phases complete:

- [ ] Run full application smoke test (all workflows)
- [ ] Verify database integrity (all icon values valid MaterialIconKind enums)
- [ ] Verify no console errors related to icons
- [ ] Verify icon accessibility (screen reader compatibility)
- [ ] Capture "after" screenshots for comparison
- [ ] Update project README.md with Material Icons badge
- [ ] Close GitHub issue/PR for migration

---

## Files to Create (Remaining Checklists)

Generate these checklist files based on plan.md details:

1. ✅ checklist-01-discovery-analysis.md (CREATED)
2. ✅ checklist-02-package-installation.md (CREATED)
3. ✅ checklist-03-icon-mapping.md (CREATED)
4. ⬜ checklist-04-icon-picker-refactor.md
5. ⬜ checklist-05-xaml-migration.md
6. ⬜ checklist-06-code-behind-migration.md
7. ⬜ checklist-07-database-migration.md
8. ⬜ checklist-08-testing-validation.md
9. ⬜ checklist-09-documentation-cleanup.md (MANDATORY LEGACY REMOVAL)

---

**Last Updated**: 2025-12-29  
**Migration Status**: Ready to Begin  
**Next Action**: Complete Phase 1 Discovery & Analysis
