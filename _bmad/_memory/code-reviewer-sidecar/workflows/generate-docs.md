# Workflow: Generate Documentation

**Purpose:** Create/update service instruction files and settings documentation

---

## Prerequisites

- Module has been analyzed (CODE_REVIEW.md exists)
- Hardcoded values discovered during analysis (stored in memories.md)

---

## Execution Steps

### 1. Service Documentation Assessment

**Scan module Services folder:**

```
{Module}/Services/
├── Service_{Name}.cs
├── Service_{Name2}.cs
...
```

**For each service:**

#### Step 1.1: Check for Existing Documentation

**Path:** `.github/instructions/service-{name}.instructions.md`

**Search pattern:** Lowercase service name, with hyphens

- Example: `Service_Volvo` → `service-volvo.instructions.md`
- Example: `Service_VolvoMasterData` → `service-volvo-masterdata.instructions.md`

#### Step 1.2: Compare Service with Documentation

**If documentation exists:**

**Read both files and compare:**

1. Extract method signatures from `Service_{Name}.cs`
2. Extract documented methods from `.instructions.md`
3. Detect changes:
   - New methods added
   - Method signatures changed
   - Methods removed
   - Constructor dependencies changed

**If changes detected:**

```
⚠️ Service_{Name} has changed since last documentation:
- Added methods: {list}
- Modified methods: {list}
- Removed methods: {list}

Documentation needs update.
Version will increment: v{current} → v{next}
```

**Archive old version:**

- Rename: `service-{name}.instructions.md` → `service-{name}_v{#}.instructions.md`
- Create new: `service-{name}.instructions.md` (updated content)

**If no changes:**

```
✅ Service_{Name} documentation is current
No update needed
```

#### Step 1.3: Generate New Documentation

**If no documentation exists:**

**Use GitHub Copilot instruction format:**

Reference: `https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions`

**Template structure:**

```markdown
# Service_{Name} - Implementation Guide

**Service Name:** Service_{Name}
**Interface:** IService_{Name}
**Module:** {Module}
**Purpose:** {Brief description of service role}

---

## Overview

{Detailed description of what this service does, its place in the architecture}

---

## Architecture

### Dependencies (Constructor Injection)
```csharp
{List all injected dependencies with types}
```

**Registration** (App.xaml.cs):

```csharp
{Show DI registration code}
```

---

## Core Methods

{For each public method:}

### {MethodName}

**Purpose:** {What it does}

**Parameters:**

- `{param}` - {description}

**Returns:** `{type}` - {description}

**Business Rules:**

- {Rule 1}
- {Rule 2}

**Example:**

```csharp
{Usage example}
```

**Error Handling:**

- {Error scenario 1}
- {Error scenario 2}

---

## Common Patterns

{Show recurring code patterns, templates}

---

## Configuration Points

{List hardcoded values that should be configurable}

---

## Testing Checklist

{Validation points for this service}

---

## Related Documentation

{Links to related instruction files}

---

**Version:** {version}
**Last Updated:** {date}
**Maintained By:** Development Team

```

**Save to:** `.github/instructions/service-{name}.instructions.md`

---

### 2. Settings Documentation Generation

**Source:** Hardcoded values from memories.md (discovered during analysis)

**Template:** `.github/templates/code-review/ModuleSettings.template.md`

**Generate:** `Documentation/FutureEnhancements/Module_Settings/{Module}Settings.md`

**Structure:**

```markdown
# {Module} Module - Configurable Settings

**Module:** {Module}
**Purpose:** Centralized configuration for {module description}
**Target Implementation:** Settings Page in Application

---

## Settings Categories

### 1. {Category Name}

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **{Setting}** | `{value}` | {type} | {description} | {validation rule} |

**Implementation Notes:**
{How to implement this setting}

**Current Code Location:**
```csharp
// {File}:{LineNumber}
{Code snippet showing hardcoded value}
```

---

{Repeat for each category}

---

## Implementation Strategy

{Step-by-step guide for implementing settings system}

---

## Database Schema

{Proposed table structure for storing settings}

---

## Priority Settings (Quick Wins)

{List easiest/most valuable settings to implement first}

---

**Document Version:** 1.0
**Created:** {date}
**Last Updated:** {date}
**Status:** Proposed for Implementation

```

**Categories to include:**

1. File System Paths
2. CSV/File Generation Limits
3. Validation Rules
4. Email/Communication Formatting
5. AutoSuggest/UI Behavior
6. Data Retention
7. Workflow Behavior
8. Logging & Diagnostics
9. **UI Text** (Labels, Placeholders, Icons)

**For each hardcoded value found:**

Extract from memories.md:
- Setting name
- Current value
- File location
- Type (Integer, String, Boolean, Path, Enum)
- Recommended validation range

Categorize appropriately

---

### 3. Icon Selector Integration

**For UI Text category:**

**Add note:**
```markdown
### UI Text Settings

Icons can be selected using the icon selector:
`Module_Shared\Views\View_Shared_IconSelectorWindow.xaml`

This should be integrated into the settings page for visual icon selection.
```

---

### 4. Update Memories

**Record in memories.md:**

```
Documentation Generated:

Service Instruction Files:
- service-{name}.instructions.md (created/updated to v{#})
- {List all services}

Settings Documentation:
- {Module}Settings.md (created)
- Hardcoded values documented: {count}
- Categories: {list}

Timestamp: {datetime}
```

---

### 5. Present Results

**Report to user:**

```
✅ Documentation Generation Complete

Service Documentation:
- Created: {count} new files
- Updated: {count} to new versions
- No changes: {count} files

Files Created/Updated:
{List all .instructions.md files}

Settings Documentation:
- {Module}Settings.md created
- {count} configurable settings documented
- {count} categories

Location: Documentation/FutureEnhancements/Module_Settings/

Next Steps:
1. Review service documentation for accuracy
2. Review settings documentation
3. Plan settings page implementation
4. Update .gitignore if needed (for versioned docs)
```

---

## Service Documentation Best Practices

**Always include:**

- Constructor dependencies with purpose
- DI registration example
- Each public method with:
  - Purpose
  - Parameters
  - Return type
  - Business rules
  - Usage example
  - Error scenarios
- Common patterns/templates
- Configuration points (hardcoded values)
- Testing checklist
- Related documentation links

**Format:**

- Use markdown headers for structure
- Code blocks with language hints
- Tables for parameter lists
- Bullet points for rules/checklists

**Version control:**

- Track changes across versions
- Archive old versions with timestamp
- Maintain version number in file

---

## Settings Documentation Best Practices

**Always include:**

- Current hardcoded value with location
- Proposed type and validation
- Implementation notes
- Database schema if persistent
- Priority rating (quick win vs complex)
- Category grouping

**Categories standard:**

- File System Paths
- Limits & Thresholds
- Validation Rules
- Communication Templates
- UI Behavior
- Data Retention
- Workflow Options
- Logging & Diagnostics
- UI Text & Icons

---

## Error Handling

**If service file missing:**

- Warn user
- Skip that service
- Continue with others

**If template missing:**

- Use built-in template
- Generate basic structure
- Note in output

**If permissions error:**

- Report error
- Suggest alternative location
- Ask user to fix permissions

---

**End of Workflow**
