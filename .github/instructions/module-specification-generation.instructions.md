# Module Specification Generation Prompt

**Version:** 1.0  
**Last Updated:** 2026-01-25  
**Purpose:** Comprehensive prompt for generating Module_{Name} and Module_Settings.{Name} specification folders with 100% accuracy

---

## 🎯 Overview

This prompt guides the creation of complete specification documentation for any module following the established structure used for Module_Receiving and Module_Settings.Receiving.

**Input Required:**
- Module Name (e.g., "Dunnage", "Routing", "Volvo")
- Existing module code location (if applicable)
- Business requirements or context

**Output Delivered:**
- `specs/Module_{Name}/` - Complete module specifications
- `specs/Module_Settings.{Name}/` - Complete settings specifications
- All cross-references, edge cases, and implementation blueprints

---

## 📋 Pre-Execution Checklist

Before starting, confirm:

- [ ] Module name is identified (e.g., "Dunnage")
- [ ] Access to existing module code (if any) at `Module_{Name}/`
- [ ] Access to existing settings code (if any) at `Module_Settings.{Name}/`
- [ ] Access to related specs documentation
- [ ] Understanding of module's core purpose
- [ ] List of configurable settings identified

---

## 🔄 Execution Phases

### PHASE 1: Discovery and Analysis
**Duration:** Initial pass through existing code/specs  
**Goal:** Complete understanding of module scope and settings

---

#### PHASE 1.1: Identify Module Structure

**Prompt:**
```
Analyze the codebase and provide:

1. Module location and structure:
   - Main module folder: Module_{Name}/
   - Settings module folder: Module_Settings.{Name}/ (if exists)
   - All subfolders in each

2. File inventory:
   - Total file count per folder
   - File types present (.cs, .xaml, .md, .sql, etc.)
   - Key architectural files (ViewModels, Views, Services, DAOs, Models)

3. Core functionality:
   - What does this module do?
   - What workflows does it support?
   - What are the main user scenarios?

Output Format:
Create a markdown report with folder tree, file counts, and functionality summary.
```

**Validation Checklist:**
- [ ] Folder structure documented
- [ ] All file types identified
- [ ] Core purpose clearly stated
- [ ] Workflows enumerated

---

#### PHASE 1.2: Extract Business Rules

**Prompt:**
```
From the Module_{Name}/ codebase, identify and document:

1. Data validation rules:
   - Required fields
   - Format constraints
   - Business logic validation
   - Cross-field dependencies

2. Workflow rules:
   - Process steps
   - Navigation logic
   - State transitions
   - User permissions

3. Integration points:
   - External systems (ERP, databases, etc.)
   - Other modules
   - Export/import mechanisms
   - API endpoints

4. Data flow:
   - User input → Processing → Persistence → Export
   - Key data transformations
   - State management

For each rule found, provide:
- Rule description
- Implementation location (file + line number)
- Related UI elements
- Validation or error messages

Output Format:
Create `Module_{Name}_BusinessRules_Analysis.md` with categorized rules.
```

**Validation Checklist:**
- [ ] All validation rules extracted
- [ ] Workflow steps documented
- [ ] Integration points identified
- [ ] Data flow mapped

---

#### PHASE 1.3: Identify Configurable Settings

**Prompt:**
```
Analyze Module_{Name}/ and Module_Settings.{Name}/ (if exists) to identify:

1. Part/Item-Level Settings:
   - Configuration stored per item/entity
   - Default assignments
   - Override capabilities
   - Item-specific flags

2. User Preference Settings:
   - User-specific preferences
   - Workflow mode preferences
   - UI customizations
   - Session behaviors

3. System-Level Settings:
   - Global configuration
   - Path configurations
   - Integration settings
   - Performance tuning

4. Settings Storage:
   - Database tables used
   - Settings files
   - Configuration classes
   - Settings keys/constants

For each setting found:
- Setting name and purpose
- Data type and valid values
- Default value
- Override hierarchy
- Storage location
- UI access path (if exists)

Output Format:
Create `Module_{Name}_Settings_Inventory.md` with categorized settings.
```

**Validation Checklist:**
- [ ] All settings categories identified
- [ ] Storage mechanisms documented
- [ ] Default values recorded
- [ ] UI access paths mapped

---

#### PHASE 1.4: Extract Edge Cases and Questions

**Prompt:**
```
Review Module_{Name}/ code for edge cases, TODOs, and unhandled scenarios:

1. Code comments containing:
   - TODO
   - FIXME
   - HACK
   - NOTE
   - Edge case
   - Known issue

2. Try-catch blocks and error handling:
   - What errors are caught?
   - What scenarios cause failures?
   - What validations are present?

3. Conditional logic branches:
   - Complex if/else structures
   - Switch statements with many cases
   - Null checks and fallbacks

4. Integration points:
   - What happens when external system is unavailable?
   - How are conflicts resolved?
   - What are timeout behaviors?

For each edge case:
- Scenario description
- Current handling (if any)
- Potential issues
- Questions for clarification

Output Format:
Create `Module_{Name}_EdgeCases_Draft.md` with scenarios and questions.
```

**Validation Checklist:**
- [ ] All TODOs extracted
- [ ] Error scenarios documented
- [ ] Edge cases identified
- [ ] Questions formulated

---

### PHASE 2: Module Specifications Creation
**Duration:** Creating specs/Module_{Name}/ structure  
**Goal:** Complete module specification documentation

---

#### PHASE 2.1: Create Folder Structure

**Prompt:**
```
Create the following folder structure in specs/Module_{Name}/:

specs/Module_{Name}/
├── index.md
├── CLARIFICATIONS.md
├── 00-Core/
├── 01-Business-Rules/
├── 02-Workflow-Modes/
├── 03-Implementation-Blueprint/
└── 99-Archive/

Confirm creation with:
- Folder tree output
- File count per folder (should all be 0 initially except for folders)
```

**Validation Checklist:**
- [ ] All folders created
- [ ] Naming follows convention (two-digit prefix)
- [ ] Archive folder present

---

#### PHASE 2.2: Create Index and Navigation

**Prompt:**
```
Create specs/Module_{Name}/index.md following this template:

# Module_{Name} Specifications - Navigation Index

**Version:** 1.0  
**Last Updated:** {TODAY}  
**Purpose:** Central navigation guide for Module_{Name} specifications

## 📋 Quick Reference
[Brief description of module]

## 🎯 Getting Started
Links to:
1. Implementation Blueprint (START HERE)
2. Purpose and Overview
3. Business Rules Overview
4. Workflow Modes Overview

## 📁 Directory Structure
[Full folder tree with descriptions]

## 🎨 Navigation by Category

### Core Concepts
| Document | Purpose | Key Topics |
|----------|---------|------------|
[Table with all core documents]

### Business Rules
| Document | Purpose | Key Topics |
|----------|---------|------------|
[Table with all business rule documents]

### Workflow Modes
| Document | Purpose | Key Topics |
|----------|---------|------------|
[Table with all workflow documents]

### Implementation Blueprint
| Document | Purpose | Content |
|----------|---------|---------|
[Table with implementation documents]

## 🔗 Related Documentation
[Links to other relevant specs]

Use content from Phase 1 analysis to populate all sections.
```

**Validation Checklist:**
- [ ] Index created
- [ ] All sections present
- [ ] Links valid (even if targets don't exist yet)
- [ ] Tables formatted correctly

---

#### PHASE 2.3: Create Core Specifications

**Prompt:**
```
Create the following core specification files in specs/Module_{Name}/00-Core/:

1. purpose-and-overview.md:
   - Module purpose statement
   - Core functionality (3-5 key features)
   - User personas (3-5 types)
   - Integration points
   - Success metrics
   - Out of scope items
   - Related documentation

2. data-flow.md:
   - Complete data flow diagrams (Mermaid format)
   - User input → Processing → Persistence → Export
   - State transitions
   - Validation points
   - Error handling flows

Template Structure:
# [Module Name] - [Document Type]

**Category**: Core Specification  
**Last Updated**: {TODAY}  
**Related Documents**: [Links]

## Overview
[High-level description]

## [Main Sections Based on Document Type]
[Detailed content from Phase 1 analysis]

## Related Documentation
[Cross-references]

Use data from Phase 1 analysis to populate all content.
```

**Validation Checklist:**
- [ ] purpose-and-overview.md created
- [ ] data-flow.md created
- [ ] All sections complete
- [ ] Mermaid diagrams valid
- [ ] Cross-references working

---

#### PHASE 2.4: Create Business Rules Documents

**Prompt:**
```
For EACH business rule identified in Phase 1.2, create a separate .md file in specs/Module_{Name}/01-Business-Rules/:

File naming: kebab-case describing the rule (e.g., load-number-dynamics.md)

Template for each file:
# [Rule Name]

**Category**: Business Rules  
**Last Updated**: {TODAY}  
**Related Documents**: [Links to related rules]

## Overview
[Brief description of what this rule governs]

## Rule Definition
[Detailed rule specification]

## Validation Rules
[How the rule is validated]

## Edge Cases
[Known edge cases for this rule]

## UI Implementation
[How this appears in UI]

## Integration Impact
[How this affects other modules/systems]

## Examples
[Clear examples of rule in action]

## Related Documentation
[Cross-references]

Create one file per rule. For Module_Receiving, there were 10 business rule files.
Expect similar count based on module complexity.
```

**Validation Checklist:**
- [ ] One file per business rule
- [ ] All rules from Phase 1.2 covered
- [ ] Naming convention followed
- [ ] Cross-references complete

---

#### PHASE 2.5: Create Workflow Mode Specifications

**Prompt:**
```
For EACH workflow mode identified in Phase 1.1, create specification in specs/Module_{Name}/02-Workflow-Modes/:

File naming: ###-mode-name-specification.md (e.g., 001-wizardmode-specification.md)

Template for each workflow mode:
# [Mode Name] - Workflow Specification

**Category**: Workflow Mode  
**Last Updated**: {TODAY}  
**Related Documents**: [Links]

## Purpose
[What this mode is for, when to use it]

## User Experience
[Step-by-step user flow]

## UI Components
[Screens, dialogs, controls]

## Validation
[Validation points in workflow]

## State Management
[How state is tracked]

## Success Criteria
[When workflow is complete]

## Error Handling
[How errors are managed]

## Related Documentation
[Cross-references]

If no distinct modes exist, document the single workflow comprehensively.
```

**Validation Checklist:**
- [ ] All workflow modes documented
- [ ] Numbered sequentially
- [ ] User flows complete
- [ ] UI mockups included (if applicable)

---

#### PHASE 2.6: Create Implementation Blueprint

**Prompt:**
```
Create implementation blueprint in specs/Module_{Name}/03-Implementation-Blueprint/:

1. index.md:
   - Blueprint overview
   - Architecture summary
   - Development phases
   - Quick reference

2. file-structure.md:
   - Complete file listing (all 228+ files expected)
   - Folder organization
   - File naming patterns
   - Dependencies

3. naming-conventions-extended.md (or reuse if same as Receiving):
   - 5-part naming standard
   - ViewModels: ViewModel_{Module}_{Mode}_{CategoryType}_{DescriptiveName}
   - Views: View_{Module}_{Mode}_{CategoryType}_{DescriptiveName}
   - Services: Service_{Module}_{Mode}_{CategoryType}_{DescriptiveName}
   - Models: Model_{Module}_{Mode}_{CategoryType}_{DescriptiveName}
   - DAOs: Dao_{Module}_{CategoryType}_{DescriptiveName}
   - Examples for each type

4. IMPLEMENTATION-SUMMARY.md:
   - Quick reference for implementation
   - Key architectural decisions
   - Critical paths

Use 5-part naming from .github/copilot-instructions.md
```

**Validation Checklist:**
- [ ] All blueprint files created
- [ ] File structure complete
- [ ] Naming conventions documented
- [ ] Summary useful for developers

---

#### PHASE 2.7: Create CLARIFICATIONS Document

**Prompt:**
```
Create specs/Module_{Name}/CLARIFICATIONS.md using edge cases from Phase 1.4:

Template:
# Module_{Name} - Clarifications and Edge Cases

**Version:** 1.0  
**Last Updated**: {TODAY}  
**Purpose:** Document unresolved questions and edge cases

## 🎯 Overview
[Brief description]

## 📋 [Category Name]

### Edge Case #: [Descriptive Name]

**Scenario:**
[Detailed scenario description]

**Questions:**
1. [Question 1]
2. [Question 2]

**Options:**
- **A**: [Option with pros/cons]
- **B**: [Option with pros/cons]
- **C**: [Option with pros/cons]

**Recommendation Needed:** [What decision is required]

---

[Repeat for each edge case, organized by category]

## ✅ Decisions Needed Summary

**Critical (Blocking Implementation):**
[List with links]

**High Priority:**
[List with links]

**Medium Priority:**
[List with links]

**Low Priority (UX Enhancement):**
[List with links]

Group edge cases by:
- Workflow Navigation
- Data Validation
- Integration Points
- UI/UX
- Security/Permissions

Aim for 15-25 edge cases total, prioritized.
```

**Validation Checklist:**
- [ ] All edge cases from Phase 1.4 included
- [ ] Categorized logically
- [ ] Priority assigned to each
- [ ] Options provided for each

---

### PHASE 3: Settings Specifications Creation
**Duration:** Creating specs/Module_Settings.{Name}/ structure  
**Goal:** Complete settings specification documentation

---

#### PHASE 3.1: Create Settings Folder Structure

**Prompt:**
```
Create the following folder structure in specs/Module_Settings.{Name}/:

specs/Module_Settings.{Name}/
├── index.md
├── CLARIFICATIONS.md
├── IMPLEMENTATION-STATUS.md
├── 00-Core/
├── 01-Settings-Categories/
├── 02-Implementation-Blueprint/
└── 99-Archive/

Confirm creation with folder tree output.
```

**Validation Checklist:**
- [ ] All folders created
- [ ] Naming follows Module_Settings.{Name} pattern
- [ ] Archive folder present

---

#### PHASE 3.2: Create Settings Index

**Prompt:**
```
Create specs/Module_Settings.{Name}/index.md:

# Module_Settings.{Name} Specifications - Navigation Index

**Version:** 1.0  
**Last Updated:** {TODAY}  
**Purpose:** Central navigation for {Name} Settings specifications

## 📋 Quick Reference
[Brief description of settings module]

## 🎯 Getting Started
1. Purpose and Overview
2. Settings Architecture
3. Settings Categories
4. Implementation Blueprint

## 📁 Directory Structure
[Full folder tree]

## 🎨 Navigation by Category

### Core Concepts
[Table with core documents]

### Settings Categories
[Table listing all settings categories identified in Phase 1.3]

### Implementation Blueprint
[Table with implementation documents]

## 🔗 Related Documentation
[Links to Module_{Name} specs and other relevant docs]

Populate using Phase 1.3 settings inventory.
```

**Validation Checklist:**
- [ ] Index created
- [ ] All settings categories listed
- [ ] Links to Module_{Name} specs present
- [ ] Quick reference useful

---

#### PHASE 3.3: Create Settings Core Documents

**Prompt:**
```
Create in specs/Module_Settings.{Name}/00-Core/:

1. purpose-and-overview.md:
   - Settings module purpose
   - Core functionality (list all setting categories from Phase 1.3)
   - User personas (Admin, Supervisor, Standard User, etc.)
   - Integration with Module_{Name}
   - Success metrics
   - Out of scope
   - Settings scope and behavior
   - Override hierarchy

2. settings-architecture.md:
   - Data storage schema
   - Settings precedence logic
   - Cache invalidation
   - Session management
   - Database table design
   - Settings load/save flow

Template:
# Module_Settings.{Name} - [Document Type]

**Category**: Core Specification  
**Last Updated**: {TODAY}  
**Related Documents**: [Links]

## Purpose
[Clear statement]

## Core Functionality
[List all 6+ settings categories]

## Key Features
[Centralized config, user-friendly interface, audit trail, etc.]

## User Personas
[Who uses settings and how]

## Integration Points
[How settings integrate with main module]

## Success Metrics
[Measurable goals]

## Out of Scope
[What settings DON'T do]

## Settings Scope and Behavior
[Immediate vs deferred application, override hierarchy]

## Related Documentation
[Cross-references]

Use Phase 1.3 settings inventory to populate all sections.
```

**Validation Checklist:**
- [ ] purpose-and-overview.md created
- [ ] settings-architecture.md created
- [ ] All settings categories listed
- [ ] Override hierarchy documented

---

#### PHASE 3.4: Create Settings Category Documents

**Prompt:**
```
For EACH settings category identified in Phase 1.3, create detailed specification in specs/Module_Settings.{Name}/01-Settings-Categories/:

File naming: kebab-case describing category (e.g., item-management.md, workflow-preferences.md)

Template for each category:
# [Category Name]

**Category**: Settings Category  
**Last Updated**: {TODAY}  
**Related Documents**: [Links]

## Overview
[What this category configures]

## Configurable Settings Per [Entity]
[List all settings in this category]

### 1. [Setting Name]

**Purpose**: [What this setting does]

**Configuration Options**:
[List all valid values]

**Default Behavior**: [What happens without override]

**Override Behavior**: [What happens with override]

**UI Component**: [Dropdown, checkbox, text input, etc.]

**Example**:
```
[Clear example showing before/after]
```

---

[Repeat for each setting in category]

## User Interface Design

### Main Settings View
[ASCII art or description of UI layout]

### Detail Panel
[ASCII art or description of detail view]

## Search and Filter Capabilities
[How users find settings]

## Bulk Operations
[What bulk edits are supported]

## Validation Rules
[Validation for this category]

## Audit Trail
[Change tracking for this category]

## Integration with {Name} Workflow
[How these settings affect main module workflows]

## Performance Considerations
[Caching, lazy loading, etc.]

## Error Handling
[Error scenarios and handling]

## Related Documentation
[Cross-references]

Expect 4-8 settings category files based on Phase 1.3 inventory.
Most modules will have:
1. Item/Entity Management (part-specific, route-specific, etc.)
2. Workflow Preferences
3. Advanced Settings

Some may have:
4. Package/Container Preferences
5. Location/Routing Defaults
6. Quality/Compliance Configuration
```

**Validation Checklist:**
- [ ] One file per settings category
- [ ] All settings from Phase 1.3 documented
- [ ] UI mockups included
- [ ] Validation rules specified

---

#### PHASE 3.5: Create Settings Implementation Blueprint

**Prompt:**
```
Create in specs/Module_Settings.{Name}/02-Implementation-Blueprint/:

1. index.md:
   - Blueprint overview
   - Settings architecture decisions
   - Technology stack
   - Development phases

2. file-structure.md:
   - Complete file listing for settings module
   - Folder organization
   - Naming conventions applied
   - Dependencies

3. naming-conventions.md (or link to Module_{Name} if same):
   - 5-part naming standard for settings-specific files
   - Examples for each file type
   - ViewModel examples
   - View examples
   - Service examples
   - Model examples

Use same 5-part standard from .github/copilot-instructions.md
```

**Validation Checklist:**
- [ ] All blueprint files created
- [ ] File structure complete
- [ ] Naming conventions consistent with main module
- [ ] Development phases realistic

---

#### PHASE 3.6: Create Settings CLARIFICATIONS

**Prompt:**
```
Create specs/Module_Settings.{Name}/CLARIFICATIONS.md:

# Module_Settings.{Name} - Clarifications and Edge Cases

**Version:** 1.0  
**Last Updated:** {TODAY}  
**Purpose:** Document settings-specific edge cases and questions

## 🎯 Overview
[Brief description]

## 📋 [Settings Category Name]

### Edge Case #: [Descriptive Name]

**Scenario:**
[Scenario involving settings configuration or application]

**Questions:**
1. [Question about settings behavior]
2. [Question about precedence or conflicts]

**Options:**
- **A**: [Option]
- **B**: [Option]
- **C**: [Option]

**Recommendation Needed:** [Decision required]

---

Focus edge cases on:
- Concurrent settings changes (user editing while settings change)
- Settings conflicts (multiple sources of truth)
- Invalid configuration handling
- Settings migration/export
- Cache invalidation
- Session vs persistent settings
- Permission-based access to settings

Aim for 10-20 settings-specific edge cases.
```

**Validation Checklist:**
- [ ] Settings-specific edge cases documented
- [ ] Categorized by settings category
- [ ] Priority assigned
- [ ] Options provided

---

#### PHASE 3.7: Create IMPLEMENTATION-STATUS

**Prompt:**
```
Create specs/Module_Settings.{Name}/IMPLEMENTATION-STATUS.md:

# Module_Settings.{Name} - Specification Creation Summary

**Date Created:** {TODAY}  
**Purpose:** Track specification completion and next steps

## ✅ Files Created
[List all files created in this phase]

## 📋 Files Still Needed
[List any deferred specifications]

## 🎯 Key Findings from Analysis
[Summary of settings identified]

## 📊 Specification Coverage
[What's documented vs what's pending]

## 🚀 Recommended Next Steps
[Prioritized implementation phases]

## 🔗 Cross-References Created
[Links between Module_{Name} and Module_Settings.{Name}]

## 📝 Edge Cases Requiring Decisions
[Critical/High/Medium/Low priority decisions]

## 🎨 UI Mockups Created
[List of UI mockups provided]

## 📚 Documentation Standards Followed
[Checklist of standards applied]

## 🔄 Integration with Existing Code
[Notes about existing implementations found]

## ✅ Success Criteria
[Completion checklist]

This provides a snapshot of specification completion status.
```

**Validation Checklist:**
- [ ] Status document created
- [ ] All created files listed
- [ ] Pending work identified
- [ ] Next steps clear

---

### PHASE 4: Cross-Referencing and Validation
**Duration:** Final pass to ensure completeness  
**Goal:** All links work, all references valid, no gaps

---

#### PHASE 4.1: Validate All Internal Links

**Prompt:**
```
Scan all created .md files in:
- specs/Module_{Name}/
- specs/Module_Settings.{Name}/

Check:
1. All markdown links work
2. All cross-references point to existing files
3. All relative paths are correct
4. All anchor links (#heading) resolve

Report:
- Broken links found
- Missing files referenced
- Suggested fixes

Fix all broken links before proceeding.
```

**Validation Checklist:**
- [ ] All internal links validated
- [ ] No broken references
- [ ] Anchor links working
- [ ] Relative paths correct

---

#### PHASE 4.2: Validate Cross-Module References

**Prompt:**
```
Verify cross-references between Module_{Name} and Module_Settings.{Name}:

1. Module_{Name} business rules should reference settings configuration
2. Module_Settings.{Name} should reference business rules they affect
3. Both clarifications documents should cross-reference each other
4. Implementation blueprints should reference each other

Check for:
- Missing cross-references
- One-way references (should be bidirectional)
- Inconsistent terminology

Create report:
- All cross-references found
- Missing bidirectional links
- Recommendations for additional cross-references
```

**Validation Checklist:**
- [ ] Cross-references validated
- [ ] Bidirectional links confirmed
- [ ] No orphaned documents

---

#### PHASE 4.3: Validate Completeness

**Prompt:**
```
Compare created specifications against template to ensure nothing missing:

Module_{Name} Checklist:
- [ ] index.md
- [ ] CLARIFICATIONS.md
- [ ] 00-Core/purpose-and-overview.md
- [ ] 00-Core/data-flow.md
- [ ] 01-Business-Rules/ (all rules documented)
- [ ] 02-Workflow-Modes/ (all modes documented)
- [ ] 03-Implementation-Blueprint/index.md
- [ ] 03-Implementation-Blueprint/file-structure.md
- [ ] 03-Implementation-Blueprint/naming-conventions-extended.md
- [ ] 03-Implementation-Blueprint/IMPLEMENTATION-SUMMARY.md

Module_Settings.{Name} Checklist:
- [ ] index.md
- [ ] CLARIFICATIONS.md
- [ ] IMPLEMENTATION-STATUS.md
- [ ] 00-Core/purpose-and-overview.md
- [ ] 00-Core/settings-architecture.md
- [ ] 01-Settings-Categories/ (all categories documented)
- [ ] 02-Implementation-Blueprint/index.md
- [ ] 02-Implementation-Blueprint/file-structure.md
- [ ] 02-Implementation-Blueprint/naming-conventions.md

Report any missing files or incomplete sections.
```

**Validation Checklist:**
- [ ] All required files present
- [ ] No incomplete sections
- [ ] All checklists completed

---

#### PHASE 4.4: Quality Review

**Prompt:**
```
Review all created documentation for:

1. Consistency:
   - Terminology used consistently
   - Naming conventions followed
   - Date stamps current
   - Version numbers consistent

2. Completeness:
   - All sections filled
   - No placeholder text
   - Examples provided
   - Edge cases documented

3. Clarity:
   - Technical terms defined
   - Examples clear
   - UI mockups understandable
   - Flows easy to follow

4. Accuracy:
   - Code references correct
   - File paths valid
   - Database schemas match code
   - Business rules match implementation

Generate quality report with:
- Issues found per document
- Severity (Critical/High/Medium/Low)
- Suggested improvements
- Overall quality score (0-100)

Fix all Critical and High severity issues before completion.
```

**Validation Checklist:**
- [ ] Consistency verified
- [ ] Completeness confirmed
- [ ] Clarity assessed
- [ ] Accuracy validated
- [ ] Quality score >= 85

---

### PHASE 5: Finalization and Handoff
**Duration:** Final documentation  
**Goal:** Package ready for development team

---

#### PHASE 5.1: Generate Summary Report

**Prompt:**
```
Create comprehensive summary report as:
specs/Module_{Name}_and_Settings_{Name}_SPECIFICATION_SUMMARY.md

Include:
1. Executive Summary
   - Module purpose
   - Key features
   - Settings overview

2. File Inventory
   - Total files created
   - Lines of documentation
   - Folder structure

3. Key Decisions Required
   - Critical edge cases
   - High priority clarifications
   - Business decisions needed

4. Implementation Roadmap
   - Suggested phases
   - Dependencies
   - Estimated effort

5. Success Metrics
   - How to measure completion
   - Quality gates
   - Acceptance criteria

6. Next Steps
   - Who needs to review
   - What decisions needed
   - When to start implementation
```

**Validation Checklist:**
- [ ] Summary report created
- [ ] Executive summary clear
- [ ] Key decisions highlighted
- [ ] Implementation roadmap realistic

---

#### PHASE 5.2: Create Handoff Checklist

**Prompt:**
```
Create HANDOFF_CHECKLIST.md in specs/ root:

# Specification Handoff Checklist - Module_{Name}

## Documentation Delivered
- [ ] Module_{Name} specifications complete
- [ ] Module_Settings.{Name} specifications complete
- [ ] All cross-references validated
- [ ] All edge cases documented
- [ ] UI mockups provided

## Decisions Required
- [ ] Review CLARIFICATIONS.md for Module_{Name}
- [ ] Review CLARIFICATIONS.md for Module_Settings.{Name}
- [ ] Prioritize edge cases
- [ ] Make architectural decisions

## Prerequisites for Development
- [ ] All critical decisions made
- [ ] Database schema approved
- [ ] UI/UX design approved
- [ ] Integration points confirmed

## Development Team Actions
- [ ] Review specifications
- [ ] Estimate effort
- [ ] Create development tasks
- [ ] Begin implementation

## Sign-Off
- [ ] Business Owner: _______________
- [ ] Technical Lead: _______________
- [ ] Development Team: _______________
- [ ] Date: _______________
```

**Validation Checklist:**
- [ ] Handoff checklist created
- [ ] All items relevant
- [ ] Sign-off section present

---

## 🎯 Completion Criteria

The specification generation is **complete** when:

### Module_{Name} Specifications
- [x] All folder structure created
- [x] index.md with full navigation
- [x] CLARIFICATIONS.md with prioritized edge cases
- [x] Core documents (purpose-and-overview.md, data-flow.md)
- [x] All business rules documented (1 file per rule)
- [x] All workflow modes documented
- [x] Complete implementation blueprint
- [x] All internal links validated

### Module_Settings.{Name} Specifications
- [x] All folder structure created
- [x] index.md with full navigation
- [x] CLARIFICATIONS.md with settings edge cases
- [x] IMPLEMENTATION-STATUS.md tracking progress
- [x] Core documents (purpose-and-overview.md, settings-architecture.md)
- [x] All settings categories documented (1 file per category)
- [x] Complete implementation blueprint
- [x] All cross-references to Module_{Name} working

### Cross-Validation
- [x] Bidirectional cross-references validated
- [x] No broken links
- [x] Terminology consistent
- [x] Quality score >= 85

### Deliverables
- [x] Summary report created
- [x] Handoff checklist created
- [x] All critical issues resolved

---

## 📊 Expected Output Metrics

| Metric | Expected Range | Module_Receiving Actual |
|--------|----------------|------------------------|
| Total .md files | 20-40 | 32 |
| Business rule files | 5-15 | 10 |
| Workflow mode files | 1-5 | 4 |
| Settings category files | 4-8 | 6 (planned) |
| Edge cases documented | 25-50 | 36 |
| Pages of documentation | 80-150 | ~120 (estimated) |
| Internal links | 100-200 | ~150 (estimated) |

Your module should fall within these ranges based on complexity.

---

## 🔧 Troubleshooting

### Issue: Module has no distinct workflow modes
**Solution:** Document single workflow comprehensively in 02-Workflow-Modes/001-primary-workflow.md

### Issue: Settings storage unclear
**Solution:** Focus Phase 1.3 on finding any configuration persistence (DB tables, JSON files, settings classes)

### Issue: Too many edge cases (>50)
**Solution:** Group related edge cases, prioritize ruthlessly, defer low priority to backlog

### Issue: No existing code to analyze
**Solution:** Work from business requirements, use Module_Receiving as template, create assumptions document

### Issue: Business rules overlap significantly
**Solution:** Create fewer, more comprehensive rule documents rather than many small ones

---

## 📝 Usage Example

### Example Command Sequence

```markdown
I need to create specifications for Module_Dunnage.

[AI executes PHASE 1.1]
→ Analyzes Module_Dunnage/ and Module_Settings.Dunnage/
→ Provides folder structure and file inventory

[AI executes PHASE 1.2]
→ Extracts business rules
→ Creates Module_Dunnage_BusinessRules_Analysis.md

[AI executes PHASE 1.3]
→ Identifies settings
→ Creates Module_Dunnage_Settings_Inventory.md

[AI executes PHASE 1.4]
→ Finds edge cases
→ Creates Module_Dunnage_EdgeCases_Draft.md

[VALIDATION GATE]
Review Phase 1 outputs, confirm accuracy, proceed to Phase 2

[AI executes PHASE 2.1 through 2.7]
→ Creates complete specs/Module_Dunnage/ structure

[AI executes PHASE 3.1 through 3.7]
→ Creates complete specs/Module_Settings.Dunnage/ structure

[AI executes PHASE 4.1 through 4.4]
→ Validates and quality checks all documentation

[AI executes PHASE 5.1 through 5.2]
→ Creates summary and handoff materials

[COMPLETION]
All specifications ready for business review and development
```

---

## 🎓 Best Practices

1. **Always start with Phase 1** - Understanding before creating
2. **Validate after each phase** - Catch issues early
3. **Use examples liberally** - Clarity over brevity
4. **Cross-reference religiously** - Create connected documentation
5. **Prioritize edge cases** - Not all questions are equally important
6. **Include UI mockups** - Visual understanding helps
7. **Keep terminology consistent** - Build glossary if needed
8. **Version everything** - Track changes over time
9. **Link to code** - Reference actual implementation when possible
10. **Think about maintainability** - Specs will be updated

---

## 📅 Estimated Timeline

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Phase 1: Discovery | 2-4 hours | Access to codebase |
| Phase 2: Module Specs | 4-8 hours | Phase 1 complete |
| Phase 3: Settings Specs | 3-6 hours | Phase 1 & 2 complete |
| Phase 4: Validation | 1-2 hours | Phase 2 & 3 complete |
| Phase 5: Finalization | 1 hour | Phase 4 complete |
| **Total** | **11-21 hours** | All phases sequential |

**Note:** Timeline assumes AI assistance. Manual creation would be 2-3x longer.

---

## ✅ Final Checklist

Before declaring completion:

- [ ] All Phase 1 analysis complete
- [ ] All Phase 2 files created
- [ ] All Phase 3 files created
- [ ] All Phase 4 validations passed
- [ ] All Phase 5 deliverables created
- [ ] Quality score >= 85
- [ ] No critical issues remaining
- [ ] Business stakeholders notified
- [ ] Development team has access
- [ ] Handoff meeting scheduled

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Maintainer:** Documentation Team  
**Based On:** Module_Receiving and Module_Settings.Receiving specification process
