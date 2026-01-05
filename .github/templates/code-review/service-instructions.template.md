# Service_{{service_name}} - Implementation Guide

**Service Name:** Service_{{service_name}}  
**Interface:** IService_{{service_name}}  
**Module:** {{module_name}}  
**Purpose:** {{service_purpose}}

---

## Overview

{{service_overview}}

---

## Architecture

### Dependencies (Constructor Injection)
```csharp
{{#each dependencies}}
private readonly {{dependency_type}} {{dependency_name}};
{{/each}}
```

**Registration** (App.xaml.cs):
```csharp
services.Add{{registration_lifetime}}<IService_{{service_name}}>(sp =>
{
{{#each dependencies}}
    var {{dependency_var}} = sp.GetRequiredService<{{dependency_type}}>();
{{/each}}
    return new Service_{{service_name}}({{dependency_params}});
});
```

---

## Core Methods

{{#each methods}}
### {{method_name}}
**Purpose:** {{method_purpose}}

{{#if parameters}}
**Parameters:**
{{#each parameters}}
- `{{param_name}}` ({{param_type}}) - {{param_description}}
{{/each}}
{{/if}}

**Returns:** `{{return_type}}` - {{return_description}}

{{#if business_rules}}
**Business Rules:**
{{#each business_rules}}
- ✅ {{rule}}
{{/each}}
{{/if}}

{{#if example_usage}}
**Example:**
```csharp
{{example_usage}}
```
{{/if}}

{{#if error_scenarios}}
**Error Handling:**
{{#each error_scenarios}}
- {{scenario}}
{{/each}}
{{/if}}

---

{{/each}}

## Common Patterns

{{#each patterns}}
### {{pattern_name}}
```csharp
{{pattern_code}}
```

{{#if pattern_notes}}
**Notes:**
{{pattern_notes}}
{{/if}}

---

{{/each}}

## Configuration Points

The following values are currently hardcoded and should be moved to settings (see {{module_name}}Settings.md):

{{#each config_points}}
| Hardcoded Value | Location | Setting Name |
|----------------|----------|--------------|
| `{{value}}` | {{location}} | {{setting_name}} |
{{/each}}

---

## Testing Checklist

When modifying Service_{{service_name}}:

{{#each test_checklist}}
- [ ] {{test_item}}
{{/each}}

---

## Common Issues & Solutions

{{#each common_issues}}
### Issue: {{issue_title}}
**Cause:** {{issue_cause}}  
**Solution:** {{issue_solution}}

{{/each}}

---

## Related Documentation

{{#each related_docs}}
- [{{doc_title}}]({{doc_path}})
{{/each}}

---

**Version:** {{doc_version}}  
**Last Updated:** {{updated_date}}  
**Maintained By:** Development Team

---

## GitHub Copilot Instructions

**File Matching Pattern:** `{{file_pattern}}`

**Apply these instructions when working on Service_{{service_name}}:**

### Core Principles
{{#each copilot_principles}}
- {{principle}}
{{/each}}

### Method Templates
{{#each copilot_templates}}

**{{template_name}}:**
```csharp
{{template_code}}
```
{{/each}}

### Common Mistakes to Avoid
{{#each copilot_mistakes}}
- ❌ {{mistake}}
{{/each}}

### Validation Rules
{{#each copilot_validations}}
- ✅ {{validation}}
{{/each}}

---

*This file follows GitHub Copilot custom instruction format: https://docs.github.com/en/copilot/how-tos/configure-custom-instructions/add-repository-instructions*
