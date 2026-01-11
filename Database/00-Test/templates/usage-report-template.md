# Stored Procedure Usage Analysis

**Generated:** {{TIMESTAMP}}
**Database:** {{DATABASE}}
**Total Stored Procedures:** {{TOTAL_SPS}}

## Summary

| Metric | Count | Percentage |
|--------|-------|------------|
| **Used Stored Procedures** | {{USED_SPS}} | {{USED_PERCENT}}% |
| **Unused Stored Procedures** | {{UNUSED_SPS}} | {{UNUSED_PERCENT}}% |
| **Total Usage Locations** | {{TOTAL_USAGES}} | - |

## Stored Procedures in Use

| SP Name | File | Class | Method | Line | Has Callers | Caller Count |
|---------|------|-------|--------|------|-------------|--------------|
{{USAGE_TABLE}}

## Unused Stored Procedures

{{UNUSED_LIST}}
