# MySQL Deployment Errors and Solutions

## Error: Duplicate entry for key 'idx_unique_...'

**Symptoms:**
When running deployment scripts (e.g., `Deploy-Database-GUI.ps1` or `Deploy-Database.ps1`), you encounter an error like:
```
ERROR 1062 (23000) at line X in file: '.../Database/TestData/XX_seed_data.sql': Duplicate entry 'VALUE' for key 'idx_unique_key_name'
```

**Cause:**
This occurs when a seed data script tries to insert a record that already exists in the database, violating a unique constraint. This often happens when re-running deployment scripts on a database that has already been seeded.

**Solution:**
Ensure that seed data scripts are **idempotent** (can be run multiple times without error).

Use `INSERT IGNORE INTO` or `ON DUPLICATE KEY UPDATE` instead of simple `INSERT INTO` for seed data.

**Example Fix:**

Change:
```sql
INSERT INTO table_name (code, description) VALUES ('VAL', 'Value');
```

To:
```sql
INSERT IGNORE INTO table_name (code, description) VALUES ('VAL', 'Value');
```

**Affected Files:**
- `Database/TestData/*.sql`
- `Database/Schemas/*.sql` (if they contain initialization data)
