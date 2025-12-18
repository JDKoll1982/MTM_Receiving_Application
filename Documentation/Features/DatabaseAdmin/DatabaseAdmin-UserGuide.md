# Database Administration - User Guide

## Overview

This guide provides instructions for database administrators to manage the MTM Receiving Application.

## Managing Shared Terminals

### Adding a New Shared Terminal

```sql
INSERT INTO workstation_config (computer_name, workstation_type, description)
VALUES ('SHOP3', 'shared_terminal', 'Shop floor terminal - Assembly area');
```

### Listing All Shared Terminals

```sql
SELECT computer_name, workstation_type, description, created_date
FROM workstation_config
WHERE workstation_type = 'shared_terminal'
ORDER BY computer_name;
```

## Managing Departments

### Adding a New Department

```sql
INSERT INTO departments (department_name, is_active, sort_order)
VALUES ('Quality Assurance', TRUE, 60);
```

### Listing Departments

```sql
SELECT department_id, department_name, is_active, sort_order
FROM departments
ORDER BY sort_order, department_name;
```

## Querying Activity Logs

### Recent Login Activity

```sql
SELECT event_timestamp, event_type, username, workstation_name
FROM user_activity_log
WHERE event_type IN ('login_success', 'login_failed')
ORDER BY event_timestamp DESC
LIMIT 50;
```

### Failed Login Attempts

```sql
SELECT DATE(event_timestamp) AS login_date, username, COUNT(*) AS failed_attempts
FROM user_activity_log
WHERE event_type = 'login_failed'
  AND event_timestamp >= DATE_SUB(NOW(), INTERVAL 7 DAY)
GROUP BY DATE(event_timestamp), username
HAVING failed_attempts >= 3;
```

## User Account Management

### View All Active Users

```sql
SELECT employee_number, full_name, department, shift, created_date
FROM users
WHERE is_active = TRUE
ORDER BY full_name;
```

### Deactivate User Account

```sql
UPDATE users
SET is_active = FALSE, modified_date = NOW()
WHERE employee_number = 1234;
```

### Reset User PIN

```sql
UPDATE users
SET pin = '5678', modified_date = NOW()
WHERE employee_number = 1234;
```

## Maintenance Tasks

### Daily: Monitor Failed Logins

```sql
SELECT username, COUNT(*) AS failures
FROM user_activity_log
WHERE event_type = 'login_failed'
  AND event_timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
GROUP BY username
HAVING failures > 5;
```

### Monthly: Archive Old Logs

```sql
-- Back up to archive table
INSERT INTO user_activity_log_archive
SELECT * FROM user_activity_log
WHERE event_timestamp < DATE_SUB(NOW(), INTERVAL 1 YEAR);

-- Delete from active table
DELETE FROM user_activity_log
WHERE event_timestamp < DATE_SUB(NOW(), INTERVAL 1 YEAR);
```

---

**Last Updated**: December 2025  
**Version**: 1.0.0
