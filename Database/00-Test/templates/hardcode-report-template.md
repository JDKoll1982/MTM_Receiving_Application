# Hardcoded SQL Query Analysis

**Generated:** {{TIMESTAMP}}
**Total Queries Found:** {{TOTAL_QUERIES}}

## Summary

| Database Type             | Count             |
| ------------------------- | ----------------- |
| MySQL                     | {{MYSQL_COUNT}}   |
| Infor Visual (SQL Server) | {{VISUAL_COUNT}}  |
| Unknown                   | {{UNKNOWN_COUNT}} |

## Infor Visual (SQL Server) Queries

These queries access the read-only Infor Visual ERP database.

| File | Class | Method | Line | Query Preview |
| ---- | ----- | ------ | ---- | ------------- |
{{VISUAL_QUERIES}}

## MySQL Queries

⚠️ **These should ideally be converted to stored procedures.**

| File | Class | Method | Line | Query Preview |
| ---- | ----- | ------ | ---- | ------------- |
{{MYSQL_QUERIES}}

## Unknown/Other Queries

| File | Class | Method | Line | Query Preview |
| ---- | ----- | ------ | ---- | ------------- |
{{UNKNOWN_QUERIES}}

---

**Recommendations:**

1. Convert MySQL queries to stored procedures for better security and maintainability
2. Ensure Infor Visual queries are read-only (SELECT statements only)
3. Review unknown queries for proper database targeting
4. Consider parameterized queries or ORM for complex data access patterns
