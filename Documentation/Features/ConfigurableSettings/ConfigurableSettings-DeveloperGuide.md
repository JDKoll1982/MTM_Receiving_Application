# Configurable Settings - Developer Guide

## Overview

This document catalogs all configurable settings in the application and their implementation.

## Current Implementation

Settings are currently hardcoded in various files. Future implementation will move these to:
- Local JSON files for user settings
- Database tables for system settings

## Settings Categories

### User Settings (Per-User)
Stored in: `%APPDATA%\MTM_Receiving_Application\user-settings.json`

```json
{
  "WindowSize": { "Width": 1200, "Height": 800 },
  "WindowPosition": "Centered",
  "Theme": "System",
  "EnableMicaBackdrop": true,
  "TimerIntervalSeconds": 60
}
```

### System Settings (Global)
Stored in database tables with category-based organization.

#### Database Schema

```sql
CREATE TABLE system_settings_security (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value VARCHAR(255) NOT NULL,
    data_type VARCHAR(20) NOT NULL,
    description TEXT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by VARCHAR(100)
);

-- Similar tables for:
-- system_settings_database
-- system_settings_validation
-- system_settings_ui
-- system_settings_branding
-- system_settings_file_paths
```

## Implementation Plan

### Phase 1: User Settings Service
Create `IUserSettingsService` to manage per-user preferences using JSON file storage.

### Phase 2: System Settings Service  
Create `ISystemSettingsService` to load settings from database tables.

### Phase 3: Settings UI
Add settings pages to allow users to configure available options.

## Migration Strategy

Replace hardcoded values with setting service calls:
```csharp
// Before
const int MaxLoginAttempts = 3;

// After  
var maxAttempts = _systemSettings.GetInt("max_login_attempts", defaultValue: 3);
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0
