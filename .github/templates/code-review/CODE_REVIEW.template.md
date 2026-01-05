# {{module_name}} - Code Review Report

**Date:** {{review_date}}  
**Reviewer:** Code Review Sentinel  
**Version:** {{review_version}}  
**Scope:** All code in {{module_name}}

---

## Executive Summary

**Files Analyzed:** {{files_analyzed}}  
**Total Issues Found:** {{total_issues}}  
**Severity Breakdown:**
- 🔴 CRITICAL: {{critical_count}}
- 🟡 SECURITY: {{security_count}}
- 🟠 DATA: {{data_count}}
- 🔵 QUALITY: {{quality_count}}
- 🟣 PERFORMANCE: {{performance_count}}
- 🔧 MAINTAIN: {{maintain_count}}
- 🟢 EDGE CASE: {{edge_case_count}}
- 🎨 UI DESIGN: {{ui_design_count}}
- 👤 UX: {{ux_count}}
- 🟤 LOGGING/DOCS: {{logging_count}}

**Estimated Fix Time:** ~{{estimated_hours}} hours

---

## How to Use This Report

1. **Review issues** below and understand each finding
2. **Amend checkboxes:**
   - ✅ = "Apply this fix automatically"
   - ⬜ = "Skip this fix" or "I'll fix manually"
3. **Save this file** with your amendments
4. **Run fix command:** Use `[F]ix` command to apply checked fixes
5. **Review changes:** Check git diff after fixes applied
6. **Archive when complete:** Use `[V]archive` when all fixes done

---

## Implementation Plan Summary

### Files to Modify

| File | Modifications | Priority |
|------|---------------|----------|
{{#each files_to_modify}}
| {{file_path}} | {{modification_count}} changes | {{priority}} |
{{/each}}

### Methods to Add

| Service/Class | Method | Purpose |
|---------------|--------|---------|
{{#each methods_to_add}}
| {{class_name}} | {{method_name}} | {{purpose}} |
{{/each}}

### Methods to Remove

| File | Method | Reason |
|------|--------|--------|
{{#each methods_to_remove}}
| {{file_path}} | {{method_name}} | {{reason}} |
{{/each}}

### Database Changes Required

| Type | Name | Location |
|------|------|----------|
{{#each database_changes}}
| {{change_type}} | {{object_name}} | {{file_path}} |
{{/each}}

---

## Issue Location Reference

| ✓ | # | Issue | Severity | File | Method/Location | Lines | Recommended Fix |
|---|---|-------|----------|------|-----------------|-------|-----------------|
{{#each issues}}
| {{checkbox}} | {{issue_number}} | {{title}} | {{severity_icon}} {{severity_name}} | `{{file_path}}` | {{method_name}} | {{line_start}}-{{line_end}} | {{recommended_fix}} |
{{/each}}

---

## Detailed Issue Descriptions

{{#each severity_groups}}
### {{severity_icon}} {{severity_name}} Issues ({{issue_count}})

{{#each issues}}
#### Issue #{{issue_number}}: {{title}}

**Severity:** {{severity_icon}} {{severity_name}}  
**File:** [`{{file_path}}`]({{file_path}}#L{{line_start}})  
**Method:** `{{method_name}}`  
**Lines:** {{line_start}}-{{line_end}}

**Description:**
{{description}}

**Current Code:**
```{{code_language}}
{{current_code}}
```

**Recommended Fix:**
{{recommended_fix}}

**Proposed Code:**
```{{code_language}}
{{proposed_code}}
```

{{#if dependencies}}
**Dependencies:**
- {{#each dependencies}}Issue #{{dependency_number}} must be fixed first{{/each}}
{{/if}}

{{#if impact}}
**Impact:**
{{impact}}
{{/if}}

---

{{/each}}
{{/each}}

---

## Settings Discovered

The following hardcoded values should be moved to a settings system:

{{#each settings_categories}}
### {{category_name}}

| Setting Name | Current Value | Location | Type | Recommended Range |
|--------------|---------------|----------|------|-------------------|
{{#each settings}}
| {{setting_name}} | `{{current_value}}` | {{file_location}} | {{setting_type}} | {{validation_range}} |
{{/each}}

{{/each}}

**Full documentation:** [`Documentation/FutureEnhancements/Module_Settings/{{module_name}}Settings.md`](Documentation/FutureEnhancements/Module_Settings/{{module_name}}Settings.md)

---

## Service Documentation Status

{{#each services}}
| Service | Documentation Exists | Status | Action Required |
|---------|----------------------|--------|-----------------|
| {{service_name}} | {{has_docs}} | {{status}} | {{action}} |
{{/each}}

**Use `[D]ocs` command to generate/update service documentation**

---

## Metrics

**Code Quality Score:** {{quality_score}}/100

**Breakdown:**
- Security: {{security_score}}/100
- Maintainability: {{maintainability_score}}/100
- Performance: {{performance_score}}/100
- Architecture: {{architecture_score}}/100

---

## Next Steps

1. ✅ **Review this document carefully**
2. ⬜ **Amend checkboxes** (✅ = fix, ⬜ = skip)
3. ⬜ **Run `[F]ix` command** to apply checked fixes
4. ⬜ **Test changes** after fixes applied
5. ⬜ **Review service documentation** needs
6. ⬜ **Plan settings implementation** from {{module_name}}Settings.md
7. ⬜ **Archive this review** when complete with `[V]archive`

---

**Report Generated:** {{generation_timestamp}}  
**Agent Version:** {{agent_version}}  
**Template Version:** 1.0
