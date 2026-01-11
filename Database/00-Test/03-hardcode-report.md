# Hardcoded SQL Query Analysis

**Generated:** 2026-01-11 14:13:05
**Total Queries Found:** 3

## Summary

| Database Type | Count |
|---------------|-------|
| MySQL | 3 |
| Infor Visual (SQL Server) | 0 |
| Unknown | 0 |

## Infor Visual (SQL Server) Queries

These queries access the read-only Infor Visual ERP database.

| File | Class | Method | Line | Query Preview |
|------|-------|--------|------|---------------|

## MySQL Queries

⚠️ **These should ideally be converted to stored procedures.**

| File | Class | Method | Line | Query Preview |
|------|-------|--------|------|---------------|
| Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `IsWindowsUsernameUniqueAsync` | 177 | `SELECT COUNT(*) FROM users WHERE windows_username = @username` |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestTablesAsync` | 448 | `SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'mtm_receiving_application' AND table_name = '{tableName}'` |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 562 | `SELECT COUNT(*) FROM information_schema.routines WHERE routine_schema = 'mtm_receiving_application' AND routine_name = '{procName}'` |

## Unknown/Other Queries

| File | Class | Method | Line | Query Preview |
|------|-------|--------|------|---------------|

---

**Recommendations:**
1. Convert MySQL queries to stored procedures for better security and maintainability
2. Ensure Infor Visual queries are read-only (SELECT statements only)
3. Review unknown queries for proper database targeting
4. Consider parameterized queries or ORM for complex data access patterns
