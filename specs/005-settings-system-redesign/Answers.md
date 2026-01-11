# Settings System Design - Answers to Clarifying Questions

## 1. Settings Architecture & Permissions

### Q: Permission Levels - Should we implement role-based access control (RBAC)?

- Suggested roles: Regular User, Operator/Supervisor, Admin, Developer/IT, Super Admin?
- Should we store user roles in the database or integrate with Windows/Active Directory groups?

**A:** Yes, use the suggested roles: Regular User, Operator/Supervisor, Admin, Developer/IT, Super Admin. Store roles in the database (user settings are user-based, system settings are admin and above only).

---

### Q: Settings Inheritance - Should user preferences override system defaults, or should admins be able to "lock" certain settings to prevent user changes?

**A:** Admins should have the ability to lock settings to prevent user changes. Additionally, provide ability to reset all user settings or single user settings.

---

### Approved Schema Approach

- Use unified settings table for system-level settings
- Use `settings_personal` table with individual column per setting for user-specific settings
- All existing settings cards to be removed and replaced by settings from `SETTABLE_OBJECTS_REPORT.md`
- If there are existing setting values not incorporated into a SETTABLE_OBJECT file, add them

---

## 2. Settings Categories & Organization

### Q: Should we keep the 5 existing cards and nest sub-categories, use hierarchical tree navigation, or use a searchable list view?

**A:** Use grouped cards layout with the proposed reorganization from the SETTABLE_OBJECTS reports.

---

## 3. Template Settings Page Design

### Q: Layout preference - Two-column form, grouped cards, or expander/accordion style?

**A:** Grouped cards (sections with headers).

---

### Q: Save behavior - Auto-save on value change, manual save with buttons, or apply & revert?

**A:** Auto-save on value change (like modern apps).

---

### Q: Validation - Show inline validation errors? Require confirmation dialog for dangerous changes?

**A:** Yes to both - show inline validation errors and require confirmation for dangerous changes.

---

## 4. Data Model Design

### Q: Should we migrate all settings from appsettings.json to database? Or keep deployment-time settings in config?

**A:** Migrate all settings to database. `appsettings.json` is for deployment settings only where it's not safe to use a database. Move runtime-configurable settings to database.

---

### Q: Should settings support environment-specific overrides?

**A:** No, production only.

---

## 5. Security Concerns

### Q: Credential storage - Should we encrypt passwords/API keys in the database? Use Windows DPAPI, Azure Key Vault, or AES encryption?

**A:** Use the simplest method - mark settings as `is_sensitive` and hash/encrypt in database.

---

### Q: Sensitive settings visibility - Should passwords show as ******** in UI? Allow "Show password" toggle?

**A:** Password in textbox with show button in it (see WinUI3 gallery on GitHub). **DO NOT** show credentials in Settings UI for InforVisual:Password, etc. Instead: "Change Password" button → dialog with old/new password fields.

---

## 6. Migration Strategy

### Q: Should we migrate existing appsettings.json values → DB automatically on first run?

**A:** User will get defaults from database on first run.

---

### Q: Should we deprecate appsettings.json for these settings or keep it as fallback?

**A:** Deprecate. `appsettings.json` is for deployment settings only where it's not safe to use a database. Move runtime-configurable settings to database.

---

## 7. UI/UX Priorities

### Q: Should settings pages support bulk import/export (backup/restore configuration)?

**A:** No.

---

### Q: Should we show tooltips/help text explaining what each setting does?

**A:** Yes.

---

### Q: Do you want validation rules displayed?

**A:** Yes.

---

## 8. Specific Settings Questions

### Q: Infor Visual Warehouse/Site - Single global setting, multi-site support, or user-specific?

**A:** Single system global setting via database.

---

### Q: Package Type Mapping (Receiving) - Should this be a DataGrid where users can add/edit prefix mappings?

**A:** MMC for coil, MMF for Sheets, ability to add more type mappings, else Skids.

---

### Q: CSV Export Paths - Should users be able to test connection to paths before saving? Should we create directories automatically?

**A:** Yes (test connection), no (auto-create directories), yes (show validation).
