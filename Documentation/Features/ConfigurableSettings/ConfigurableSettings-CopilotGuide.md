# Configurable Settings - Copilot Guide

## Quick Reference

Settings are cataloged for future implementation. Currently hardcoded.

## Future: Load User Setting

```csharp
var userSettings = App.GetService<IUserSettingsService>();
var windowWidth = userSettings.GetInt("WindowWidth", defaultValue: 1200);
```

## Future: Load System Setting

```csharp
var systemSettings = App.GetService<ISystemSettingsService>();  
var maxAttempts = systemSettings.GetInt("max_login_attempts", defaultValue: 3);
```

## Hardcoded Values to Replace

| Location | Variable | Default Value |
|----------|----------|---------------|
| SharedTerminalLoginDialog | MaxAttempts | 3 |
| Model_WorkstationConfig | SharedTimeout | 15 min |
| Model_WorkstationConfig | PersonalTimeout | 30 min |
| Helper_Database_StoredProcedure | MaxRetries | 3 |
| Helper_Database_StoredProcedure | RetryDelays | [100, 200, 400] ms |

---

**Last Updated**: December 2025  
**Version**: 1.0.0
