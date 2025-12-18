# Database Administration - Copilot Guide

## Quick SQL Queries

### Add Shared Terminal
```sql
INSERT INTO workstation_config (computer_name, workstation_type, description)
VALUES ('SHOP3', 'shared_terminal', 'Description');
```

### Add Department
```sql
INSERT INTO departments (department_name, is_active, sort_order)
VALUES ('New Dept', TRUE, 60);
```

### Deactivate User
```sql
UPDATE users SET is_active = FALSE WHERE employee_number = 1234;
```

### View Recent Activity
```sql
SELECT * FROM user_activity_log ORDER BY event_timestamp DESC LIMIT 50;
```

### Check Duplicate PINs
```sql
SELECT pin, COUNT(*) as count FROM users GROUP BY pin HAVING count > 1;
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0
