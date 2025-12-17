# Configurable Settings for Future Settings Menu

This document catalogs all hard-coded values in the application that should be moved to a configurable settings system.

## Overview

Settings are divided into two categories:
- **User Settings**: Per-user preferences stored in local storage or user-specific database records
- **System Settings**: Global settings configurable by administrators, stored in system-wide database tables

---

## User Settings

These settings are configurable per user and stored in user preferences.

### Window and UI Preferences

| File | Variable/Value | Current Value | Description |
|------|---------------|---------------|-------------|
| `MainWindow.xaml.cs` | Window Size | 1200×800 | Main application window default size |
| `MainWindow.xaml.cs` | Window Position | Centered | Main window opening position (could save last position) |

### Display and Appearance

| File | Variable/Value | Current Value | Description |
|------|---------------|---------------|-------------|
| `App.xaml` | Theme | System Default | Application theme (Light/Dark/System) |
| `MainWindow.xaml` | `MicaBackdrop` | Enabled | Mica material backdrop effect |

### Session and Activity

| File | Variable/Value | Current Value | Description |
|------|---------------|---------------|-------------|
| `Service_SessionManager.cs` | `TimerIntervalSeconds` | 60 seconds | How often to check for session timeout |

---

## System Settings (Admin-Configurable)

These settings are global and can only be changed by administrators. They should be stored in database configuration tables.

### Security Settings

| File | Variable/Value | Current Value | Description | Database Table |
|------|---------------|---------------|-------------|----------------|
| `SharedTerminalLoginDialog.xaml.cs` | `MaxAttempts` | 3 | Maximum login attempts before lockout | `system_settings.security` |
| `SharedTerminalLoginDialog.xaml.cs` | Lockout Display Duration | 5000ms (5 sec) | How long to show lockout message before closing | `system_settings.security` |
| `Model_WorkstationConfig.cs` | Shared Terminal Timeout | 15 minutes | Inactivity timeout for shared terminals | `system_settings.security` |
| `Model_WorkstationConfig.cs` | Personal Workstation Timeout | 30 minutes | Inactivity timeout for personal workstations | `system_settings.security` |

### Database Configuration

| File | Variable/Value | Current Value | Description | Database Table |
|------|---------------|---------------|-------------|----------------|
| `DeployAuthenticationSchema.cs` | `ConnectionString` | Hard-coded | Database connection string (should be encrypted in config) | `system_settings.database` |
| `DeployAuthenticationSchema.cs` | `CommandTimeout` | 60 seconds | Database command timeout for deployment | `system_settings.database` |
| `Helper_Database_StoredProcedure.cs` | `MaxRetries` | 3 | Maximum retry attempts for failed database operations | `system_settings.database` |
| `Helper_Database_StoredProcedure.cs` | `RetryDelaysMs` | [100, 200, 400] | Exponential backoff delays for database retries (milliseconds) | `system_settings.database` |

### Input Validation

| File | Variable/Value | Current Value | Description | Database Table |
|------|---------------|---------------|-------------|----------------|
| `NewUserSetupDialog.xaml` | Full Name MaxLength | 100 | Maximum characters for user full name | `system_settings.validation` |
| `NewUserSetupDialog.xaml` | Employee Number MaxLength | 10 | Maximum characters for employee number | `system_settings.validation` |
| `NewUserSetupDialog.xaml` | Custom Department MaxLength | 50 | Maximum characters for custom department name | `system_settings.validation` |
| `NewUserSetupDialog.xaml` | PIN MaxLength | 4 | Required PIN length (4 digits) | `system_settings.validation` |
| `NewUserSetupDialog.xaml` | ERP Username MaxLength | 50 | Maximum characters for ERP username | `system_settings.validation` |
| `NewUserSetupDialog.xaml` | ERP Password MaxLength | 100 | Maximum characters for ERP password | `system_settings.validation` |
| `SharedTerminalLoginDialog.xaml` | Username MaxLength | 50 | Maximum characters for login username | `system_settings.validation` |
| `SharedTerminalLoginDialog.xaml` | PIN MaxLength | 4 | PIN length for shared terminal login | `system_settings.validation` |

### UI Behavior

| File | Variable/Value | Current Value | Description | Database Table |
|------|---------------|---------------|-------------|----------------|
| `NewUserSetupDialog.xaml.cs` | Success Message Display Duration | 2000ms (2 sec) | How long to show success message before closing | `system_settings.ui` |
| `SplashScreenWindow.xaml.cs` | Window Size | 850×700 | Splash screen window size | `system_settings.ui` |

### Application Branding

| File | Variable/Value | Current Value | Description | Database Table |
|------|---------------|---------------|-------------|----------------|
| `SplashScreenWindow.xaml` | Application Title | "MTM Receiving Application" | Main application title displayed on splash screen | `system_settings.branding` |
| `SplashScreenWindow.xaml` | Version Number | "Version 1.0.0" | Application version displayed on splash screen | `system_settings.branding` |
| `SplashScreenWindow.xaml` | Company Name | "MTM Manufacturing" | Company name displayed on splash screen | `system_settings.branding` |
| `SplashScreenWindow.xaml` | Copyright Text | "© 2025 MTM Manufacturing. All rights reserved." | Copyright notice | `system_settings.branding` |
| `MainWindow.xaml` | Navigation Pane Header | "MTM Receiving" | Title shown in navigation pane | `system_settings.branding` |

---

## Implementation Plan

### Phase 1: User Settings
1. Create `UserSettings` service to manage per-user preferences
2. Store settings in local application data folder (JSON or SQLite)
3. Add settings page to UI with user-configurable options
4. Load user preferences on application startup
5. Save preferences when changed

### Phase 2: System Settings (Admin)
1. Create `system_settings` database schema with category tables:
   - `system_settings.security`
   - `system_settings.database`
   - `system_settings.validation`
   - `system_settings.ui`
   - `system_settings.branding`
2. Create `SystemSettings` service to load from database
3. Add admin-only settings page (requires elevated permissions)
4. Implement setting change auditing
5. Cache system settings in memory with periodic refresh

### Phase 3: Migration
1. Replace all hard-coded constants with setting service calls
2. Provide default values as fallback
3. Create migration script to populate database with current defaults
4. Test all settings are properly loaded and applied

---

## Database Schema (Proposed)

### system_settings.security
```sql
CREATE TABLE system_settings_security (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value VARCHAR(255) NOT NULL,
    data_type VARCHAR(20) NOT NULL, -- 'int', 'string', 'bool', 'timespan'
    description TEXT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by VARCHAR(100)
);

-- Example data
INSERT INTO system_settings_security VALUES
('max_login_attempts', '3', 'int', 'Maximum login attempts before lockout', NOW(), 'system'),
('shared_terminal_timeout_minutes', '15', 'int', 'Session timeout for shared terminals', NOW(), 'system'),
('personal_workstation_timeout_minutes', '30', 'int', 'Session timeout for personal workstations', NOW(), 'system'),
('lockout_display_duration_ms', '5000', 'int', 'Lockout message display duration', NOW(), 'system');
```

### system_settings.database
```sql
CREATE TABLE system_settings_database (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value VARCHAR(255) NOT NULL,
    data_type VARCHAR(20) NOT NULL,
    description TEXT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by VARCHAR(100)
);

-- Example data
INSERT INTO system_settings_database VALUES
('max_retry_attempts', '3', 'int', 'Maximum database retry attempts', NOW(), 'system'),
('retry_delays_ms', '100,200,400', 'int_array', 'Retry delay intervals in milliseconds', NOW(), 'system'),
('command_timeout_seconds', '60', 'int', 'Default command timeout', NOW(), 'system');
```

### system_settings.validation
```sql
CREATE TABLE system_settings_validation (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value VARCHAR(255) NOT NULL,
    data_type VARCHAR(20) NOT NULL,
    description TEXT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by VARCHAR(100)
);

-- Example data
INSERT INTO system_settings_validation VALUES
('pin_length', '4', 'int', 'Required PIN length', NOW(), 'system'),
('max_employee_number_length', '10', 'int', 'Maximum employee number length', NOW(), 'system'),
('max_full_name_length', '100', 'int', 'Maximum full name length', NOW(), 'system');
```

### system_settings.branding
```sql
CREATE TABLE system_settings_branding (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value TEXT NOT NULL,
    data_type VARCHAR(20) NOT NULL,
    description TEXT,
    last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by VARCHAR(100)
);

-- Example data
INSERT INTO system_settings_branding VALUES
('application_title', 'MTM Receiving Application', 'string', 'Main application title', NOW(), 'system'),
('application_version', '1.0.0', 'string', 'Current version number', NOW(), 'system'),
('company_name', 'MTM Manufacturing', 'string', 'Company name', NOW(), 'system'),
('copyright_text', '© 2025 MTM Manufacturing. All rights reserved.', 'string', 'Copyright notice', NOW(), 'system');
```

---

## Notes

- All timeout values should be configurable but have sensible defaults
- PIN length changes should trigger validation in existing UI components
- Connection strings should be encrypted in database/config files
- System settings changes should be logged for audit purposes
- Consider caching frequently accessed settings
- IT support email from `REUSABLE_SERVICES_SETUP.md` should also be system setting

---

**Last Updated**: December 16, 2025  
**Version**: 1.0
