# Stored Procedure Analysis Report

**Generated:** {{TIMESTAMP}}
**Database:** {{DATABASE}}
**Server:** {{SERVER}}

## Summary

- **Total Stored Procedures:** {{TOTAL_SPS}}
- **Execution Groups:** {{EXECUTION_GROUPS}}
- **Categories:** {{CATEGORY_COUNT}}

## Parameter Statistics

| Type                   | Count            |
| ---------------------- | ---------------- |
| Total Parameters       | {{TOTAL_PARAMS}} |
| IN Parameters          | {{IN_PARAMS}}    |
| OUT Parameters         | {{OUT_PARAMS}}   |
| INOUT Parameters       | {{INOUT_PARAMS}} |
| SPs without Parameters | {{NO_PARAMS}}    |

## Stored Procedures by Category

{{CATEGORY_BREAKDOWN}}

## Execution Order Groups

| Order | Category | Count | Stored Procedures |
| ----- | -------- | ----- | ----------------- |
{{EXECUTION_ORDER_TABLE}}

## Notes

- **Execution Order:** Lower numbers execute first (10-999)
- **Read Operations:** Offset by +1000 (can run anytime)
- **Delete Operations:** Offset by +2000 (run last)
- **Mock Data:** Review `01-mock-data.json` to customize parameter values
- **Dependencies:** Use `-UseExecutionOrder` flag when testing to avoid FK constraint errors

{{COMPLEX_SPS_SECTION}}

## 🔧 Fix Checklist

### Stored Procedures Requiring Updates

| Fixed | SP Name | Current Params | Expected Params | Missing Params | File Path | DAO Class | Issue Type | Priority | Fix Notes |
|-------|---------|----------------|-----------------|----------------|-----------|-----------|------------|----------|-----------|
{{SP_FIX_CHECKLIST}}

### DAO Files Requiring Updates

| Fixed | DAO Class | File Path | Related SPs | Param Mismatches | Methods to Update | Issue Summary | Priority |
|-------|-----------|-----------|-------------|------------------|-------------------|---------------|----------|
{{DAO_FIX_CHECKLIST}}

### Database Schema Issues

| Fixed | SP Name | Missing Column/Table | Schema File | Table Name | Expected Column | Current Status | SQL Fix Required | Priority | Fix Notes |
|-------|---------|----------------------|-------------|------------|-----------------|----------------|------------------|----------|-----------|
{{SCHEMA_FIX_CHECKLIST}}
