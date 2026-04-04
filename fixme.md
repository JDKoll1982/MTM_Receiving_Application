---

## Fix 8 — Rework New User First-Login Settings Initialization

**Priority:** High  
**Effort:** Medium  
**Files:**  
- `Module_Core/Services/Authentication/Service_UserLoginCoordinator.cs`  
- `Module_Settings.Core/Services/Service_SettingsCoreFacade.cs`  
- `Module_Settings.Core/Services/GetSettingQueryHandler.cs`  
- `Module_Settings.Core/Defaults/settings.manifest.json`

**Problem:**  
During first login for a brand-new user, `InitializeAuthenticatedSessionAsync` waits for `InitializeSettingsDefaultsAsync`, which calls `InitializeDefaultsAsync`. That method loops through every settings definition in the settings manifest and sends a `GetSettingQuery` for each one. For a new user this causes a cache miss, a user-settings lookup, and then a default write for each missing setting. Because this is done sequentially for every setting, the user waits through hundreds of serial database operations before entering the application.

**Recommended Alternatives:**  
1. Preferred: return manifest defaults in memory and only persist settings when the user explicitly changes them.  
2. Add a bulk seed stored procedure that inserts all missing defaults for a user in one database call.  
3. Move settings default initialization off the critical login path and run it in the background after the session is created.  
4. As a short-term mitigation, parallelize the existing per-setting initialization flow.

**Expected Outcome:**  
First login for new users will no longer block on hundreds of per-setting serial database round-trips. New users should reach the main application much faster, and settings persistence can be handled lazily, in bulk, or off the critical path.