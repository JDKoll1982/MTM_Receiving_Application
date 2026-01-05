# {{module_name}} Module - Configurable Settings

**Module:** {{module_name}}  
**Purpose:** Centralized configuration for {{module_description}}  
**Target Implementation:** Settings Page in Application

---

## 📋 Settings Categories

{{#each categories}}
### {{category_number}}. {{category_name}}

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
{{#each settings}}
| **{{setting_name}}** | `{{current_value}}` | {{setting_type}} | {{description}} | {{validation_rule}} |
{{/each}}

**Implementation Notes:**
{{category_implementation_notes}}

**Current Code Location{{#if (gt settings.length 1)}}s{{/if}}:**
```csharp
{{#each settings}}
// {{file_path}}:{{line_number}}
{{code_snippet}}

{{/each}}
```

---

{{/each}}

## 🔧 Implementation Strategy

### Phase 1: Core Settings Infrastructure
1. Create `Model_{{module_name}}Settings.cs` with all properties
2. Create `Dao_{{module_name}}Settings` for database persistence
3. Create settings table in MySQL database
4. Add settings singleton to DI container

### Phase 2: Settings Service
1. Create `IService_{{module_name}}Settings` interface
2. Implement `Service_{{module_name}}Settings` with validation
3. Add default settings initialization on first run
4. Implement import/export for backup

### Phase 3: Settings UI
1. Create `SettingsPage.xaml` in Module_Settings
2. Group settings by category with ExpanderControls
3. Add validation with real-time feedback
4. Implement "Reset to Defaults" per category
5. **Icon Selection**: Integrate `Module_Shared\Views\View_Shared_IconSelectorWindow.xaml` for icon settings

### Phase 4: Integration
1. Refactor hardcoded values to use settings service
2. Add settings change notifications (INotifyPropertyChanged)
3. Update documentation with setting descriptions
4. Add tooltips in UI explaining each setting

---

## 📊 Database Schema

```sql
CREATE TABLE {{module_name_lower}}_settings (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value TEXT NOT NULL,
    setting_type ENUM('String','Integer','Boolean','Path','Enum') NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT,
    default_value TEXT NOT NULL,
    min_value INT NULL,
    max_value INT NULL,
    modified_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    modified_by VARCHAR(50),
    INDEX idx_category (category)
);
```

---

## 🎯 Priority Settings (Quick Wins)

These settings provide immediate value with minimal effort:

{{#each quick_wins}}
{{quick_win_number}}. **{{setting_name}}** - {{reason}}
{{/each}}

---

## ⚠️ Settings Requiring Careful Implementation

{{#each complex_settings}}
{{complex_setting_number}}. **{{setting_name}}** - {{reason}}
{{/each}}

---

## 📖 Related Documents

- [Module Settings Constitution](.specify/memory/constitution.md)
- [Service Infrastructure Guide](../../README.md)
- [{{module_name}} Workflow Specification](../../specs/{{module_spec_folder}}/)

---

**Document Version:** {{document_version}}  
**Created:** {{created_date}}  
**Last Updated:** {{updated_date}}  
**Status:** {{status}}
