# Module_Settings.Receiving - Purpose and Overview

**Category**: Core Specification  
**Last Updated**: 2026-01-25  
**Related Documents**: [Settings Architecture](./settings-architecture.md), [Module_Receiving Purpose](../../Module_Receiving/00-Core/purpose-and-overview.md)

---

## Purpose

Module_Settings.Receiving provides centralized configuration and preference management for the Receiving workflow. It allows users and administrators to customize default behaviors, manage part-specific settings, and configure system-level options that govern how the Receiving module operates.

---

## Core Functionality

The settings module provides six primary categories of configuration:

### 1. Part Number Management
Configure part-specific settings that affect how individual parts behave during receiving:
- **Part Type Assignment** - Override automatic part type detection
- **Default Receiving Locations** - Set custom default locations per part
- **Quality Hold Flags** - Mark parts requiring quality inspection
- **Package Type Preferences** - Define default package types per part

**Access Level**: Administrators, Supervisors  
**Impact Scope**: All users receiving configured parts

### 2. Package Type Preferences
Manage associations between parts and their expected package types:
- **Default Package Types** - Set preferred package type per part
- **Package Type History** - View commonly used package types
- **Bulk Configuration** - Apply package preferences to multiple parts

**Access Level**: Administrators, Supervisors  
**Impact Scope**: All users receiving configured parts

### 3. Receiving Location Defaults
Configure default receiving locations and fallback behaviors:
- **Part-Specific Locations** - Override ERP default locations
- **Location Validation** - Enable/disable strict location validation
- **Fallback Location** - Set default when no location specified (currently "RECV")

**Access Level**: Administrators  
**Impact Scope**: All receiving locations auto-populated

### 4. Quality Hold Configuration
Manage quality inspection requirements and procedures:
- **Part-Level Quality Hold Flags** - Enable/disable quality hold per part
- **Quality Hold Procedure Text** - Configure procedure instructions (if configurable)
- **Acknowledgment Requirements** - Set acknowledgment behavior

**Access Level**: Administrators, Quality Control  
**Impact Scope**: All users receiving Quality Hold parts

### 5. Workflow Preferences
User-specific settings for workflow behavior:
- **Default Workflow Mode** - Set preferred mode (Wizard/Manual/Edit)
- **Auto-Save Preferences** - Configure automatic save behavior
- **Validation Strictness** - Set validation level (Strict/Warning/Permissive)
- **Session Recovery** - Enable/disable session data recovery

**Access Level**: Individual users (self-configuration)  
**Impact Scope**: Only the configuring user

### 6. Advanced Settings
System-level configuration options:
- **CSV Export Paths** - Configure local and network CSV destinations
- **Debug/Logging Options** - Enable verbose logging for troubleshooting
- **Integration Settings** - Configure ERP connection parameters
- **Performance Tuning** - Adjust grid rendering and validation timing

**Access Level**: Administrators only  
**Impact Scope**: System-wide

---

## Key Features

### Centralized Configuration
- **Single Source of Truth** - All receiving preferences in one location
- **Cross-Module Integration** - Settings apply across Wizard, Manual, and Edit modes
- **Real-Time Application** - Changes take effect immediately or on next session (configurable)

### User-Friendly Interface
- **Search and Filter** - Quickly find parts and settings
- **Bulk Operations** - Apply settings to multiple parts simultaneously
- **Validation Feedback** - Real-time validation of configuration values
- **Import/Export** - Migrate settings between environments (if implemented)

### Audit and Compliance
- **Change Tracking** - Record who changed what and when
- **Settings History** - View historical configuration values
- **Compliance Reporting** - Generate reports on Quality Hold configurations

### Integration with Receiving Workflow
- **Seamless Integration** - Settings automatically applied during receiving
- **Override Capability** - Users can override settings when appropriate
- **Session Management** - Session-scoped overrides don't affect base settings

---

## User Personas

### Administrator
- **Role**: System configuration and maintenance
- **Uses**: All settings categories
- **Permissions**: Full read/write access to all settings
- **Responsibilities**: Configure system defaults, manage part-level settings, maintain user preferences

### Supervisor
- **Role**: Department-level configuration
- **Uses**: Part Number Management, Package Type Preferences, Quality Hold
- **Permissions**: Read/write for part-specific settings
- **Responsibilities**: Configure new parts, update quality requirements, maintain location defaults

### Quality Control Manager
- **Role**: Quality inspection configuration
- **Uses**: Quality Hold Configuration
- **Permissions**: Read/write for Quality Hold settings
- **Responsibilities**: Mark parts requiring inspection, update procedures, monitor compliance

### Standard User (Receiving Clerk)
- **Role**: Daily receiving operations
- **Uses**: Workflow Preferences only
- **Permissions**: Read all settings, write only to personal workflow preferences
- **Responsibilities**: Configure personal workflow preferences, view part settings

---

## Integration Points

### Module_Receiving Integration
Settings are consumed by all Receiving workflow modes:

**Wizard Mode:**
- Auto-applies Part Type, Default Location, Package Type
- Displays Quality Hold prompts based on settings
- Respects user's Default Workflow Mode preference

**Manual Mode:**
- Pre-populates grid with default values from settings
- Validates entries against configured settings
- Applies bulk settings for efficiency

**Edit Mode:**
- Loads historical data with original settings applied
- Allows viewing current vs historical settings
- Applies validation based on current settings (with warnings)

### ERP (Infor Visual) Integration
Settings can override or supplement ERP data:
- **Override Precedence**: Settings > ERP > Hardcoded Defaults
- **Read-Only ERP**: Settings never write back to ERP
- **Validation**: Can validate settings against ERP data

### Database Integration
Settings persistence and retrieval:
- **Application Settings Database**: Stores all configuration
- **User-Specific Settings**: Linked to user accounts
- **Part-Specific Settings**: Indexed by part number
- **Audit Trail**: Change history with user/timestamp

---

## Success Metrics

### Configuration Efficiency
- Time to configure new part: <2 minutes
- Time to find and edit setting: <30 seconds
- Bulk configuration operations: 10+ parts in <5 minutes

### User Adoption
- 80%+ of users configure personal workflow preferences
- 100% of Quality Hold parts configured within 1 week of requirement
- Reduced support tickets related to default values

### Data Quality
- 0% invalid settings persisted (validation catches before save)
- 100% Quality Hold parts properly flagged
- Complete audit trail for all configuration changes

---

## Out of Scope

The following items are explicitly NOT part of Module_Settings.Receiving:

- **User Account Management** - Handled by Module_Core authentication
- **Role/Permission Management** - Handled by Module_Core security
- **ERP Settings Management** - ERP configuration remains in Infor Visual
- **Label Printer Configuration** - Handled separately (not receiving-specific)
- **Workflow Logic Changes** - Settings configure behavior, not define new workflows
- **Historical Data Migration** - Settings apply to future transactions only

---

## Settings Scope and Behavior

### Immediate vs Deferred Application

**Immediate Application (No session impact):**
- Workflow Preferences changes
- UI appearance settings
- Non-critical configuration

**Deferred Application (Next session):**
- Part Type changes
- Default Location changes
- Package Type Preference changes

**Session-Locked (Until save/exit):**
- Quality Hold status (once acknowledged in session)
- Session-scoped location overrides

### Override Hierarchy

Settings follow a clear precedence order:

```
1. User Session Override (highest priority - temporary)
   ↓
2. User Workflow Preferences (user-specific - permanent)
   ↓
3. Part-Specific Settings (administrator-configured)
   ↓
4. ERP Default Values (Infor Visual)
   ↓
5. Hardcoded Application Defaults (lowest priority - fallback)
```

**Example:**
- ERP says MMC0001000 location: V-C0-01
- Settings say MMC0001000 location: V-C0-02 (override)
- User in session changes to: RECV (session override)
- **Result**: RECV is used (highest priority)
- **Next Session**: V-C0-02 is used (settings win over ERP)

---

## Related Documentation

- [Settings Architecture](./settings-architecture.md) - Technical implementation details
- [Part Number Management](../01-Settings-Categories/part-number-management.md) - Detailed part settings spec
- [Module_Receiving Purpose](../../Module_Receiving/00-Core/purpose-and-overview.md) - Main receiving module overview
- [Business Rules](../../Module_Receiving/01-Business-Rules/) - Rules governed by settings

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-25 | Initial specification created |
