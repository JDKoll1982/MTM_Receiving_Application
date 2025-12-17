# Database Administration Guide

## Overview

This guide provides instructions for database administrators to manage the MTM Receiving Application authentication system.

## Table of Contents

1. [Managing Shared Terminals](#managing-shared-terminals)
2. [Managing Departments](#managing-departments)
3. [Querying Activity Logs](#querying-activity-logs)
4. [User Account Management](#user-account-management)
5. [Troubleshooting](#troubleshooting)

---

## Managing Shared Terminals

### What is a Shared Terminal?

A shared terminal is a computer where multiple users log in using their username and PIN, rather than automatic Windows authentication. Typically used on shop floor computers.

### Adding a New Shared Terminal

When setting up a new shop floor computer, add it to the workstation configuration:

```sql
INSERT INTO workstation_config (computer_name, workstation_type, description)
VALUES ('SHOP3', 'shared_terminal', 'Shop floor terminal - Assembly area');
```

**Parameters**:
- `computer_name`: Exact Windows computer name (from `hostname` command)
- `workstation_type`: Must be `'shared_terminal'` for PIN login
- `description`: Human-readable description for identification

### Listing All Shared Terminals

```sql
SELECT 
    computer_name,
    workstation_type,
    description,
    created_date
FROM workstation_config
WHERE workstation_type = 'shared_terminal'
ORDER BY computer_name;
```

### Converting Terminal Type

**From Shared to Personal**:
```sql
UPDATE workstation_config
SET workstation_type = 'personal_workstation',
    description = 'Personal workstation - Office'
WHERE computer_name = 'SHOP2';
```

**From Personal to Shared**:
```sql
UPDATE workstation_config
SET workstation_type = 'shared_terminal',
    description = 'Shared terminal - Shop floor'
WHERE computer_name = 'OFFICE1';
```

### Removing a Terminal

```sql
DELETE FROM workstation_config
WHERE computer_name = 'OLD_COMPUTER';
```

**Note**: Computers not in `workstation_config` default to personal workstation behavior.

---

## Managing Departments

### What are Departments?

Departments are used in the New User Creation dialog dropdown. Users must select their department when creating an account.

### Adding a New Department

```sql
INSERT INTO departments (department_name, is_active, sort_order)
VALUES ('Quality Assurance', TRUE, 60);
```

**Parameters**:
- `department_name`: Display name (max 50 characters, unique)
- `is_active`: `TRUE` to show in dropdown, `FALSE` to hide
- `sort_order`: Display order (lower numbers appear first)

**Recommended Sort Order**:
- Production/Manufacturing: 10-19
- Quality/Inspection: 20-29
- Shipping/Receiving: 30-39
- Maintenance: 40-49
- Engineering: 50-59
- Office/Admin: 60-69
- Management: 70-79
- Other: 999

### Listing All Departments

```sql
SELECT 
    department_id,
    department_name,
    is_active,
    sort_order,
    created_date
FROM departments
ORDER BY sort_order, department_name;
```

### Deactivating a Department

```sql
UPDATE departments
SET is_active = FALSE
WHERE department_name = 'Obsolete Department';
```

**Note**: Deactivating a department hides it from new user creation but doesn't affect existing users.

### Reactivating a Department

```sql
UPDATE departments
SET is_active = TRUE
WHERE department_name = 'Department Name';
```

### Changing Department Sort Order

```sql
UPDATE departments
SET sort_order = 15
WHERE department_name = 'Production';
```

### Renaming a Department

```sql
UPDATE departments
SET department_name = 'Quality Control'
WHERE department_name = 'QC';
```

**Important**: This also updates all existing users:
```sql
UPDATE users
SET department = 'Quality Control'
WHERE department = 'QC';
```

---

## Querying Activity Logs

### Purpose

The `user_activity_log` table records all authentication-related events for security auditing and troubleshooting.

### Event Types

- `login_success`: Successful authentication
- `login_failed`: Failed authentication attempt
- `session_timeout`: Session expired due to inactivity
- `user_created`: New user account created
- `manual_close`: User manually closed application

### Recent Login Activity

```sql
SELECT 
    event_timestamp,
    event_type,
    username,
    workstation_name,
    event_details
FROM user_activity_log
WHERE event_type IN ('login_success', 'login_failed')
ORDER BY event_timestamp DESC
LIMIT 50;
```

### Failed Login Attempts

Useful for identifying security issues or user problems:

```sql
SELECT 
    DATE(event_timestamp) AS login_date,
    username,
    workstation_name,
    COUNT(*) AS failed_attempts
FROM user_activity_log
WHERE event_type = 'login_failed'
  AND event_timestamp >= DATE_SUB(NOW(), INTERVAL 7 DAY)
GROUP BY DATE(event_timestamp), username, workstation_name
HAVING failed_attempts >= 3
ORDER BY login_date DESC, failed_attempts DESC;
```

### Session Timeout Report

```sql
SELECT 
    DATE(event_timestamp) AS timeout_date,
    username,
    workstation_name,
    COUNT(*) AS timeout_count
FROM user_activity_log
WHERE event_type = 'session_timeout'
  AND event_timestamp >= DATE_SUB(NOW(), INTERVAL 30 DAY)
GROUP BY DATE(event_timestamp), username, workstation_name
ORDER BY timeout_date DESC, timeout_count DESC;
```

### New User Creation Audit

```sql
SELECT 
    event_timestamp,
    username AS new_username,
    SUBSTRING_INDEX(event_details, 'by ', -1) AS created_by,
    workstation_name,
    event_details
FROM user_activity_log
WHERE event_type = 'user_created'
ORDER BY event_timestamp DESC
LIMIT 20;
```

### User Login History

Track when specific user last accessed system:

```sql
SELECT 
    event_timestamp,
    event_type,
    workstation_name,
    event_details
FROM user_activity_log
WHERE username = 'johnk'
  AND event_type = 'login_success'
ORDER BY event_timestamp DESC
LIMIT 10;
```

### Export Activity Log to CSV

For external analysis or reporting:

```sql
SELECT 
    log_id,
    event_timestamp,
    event_type,
    username,
    workstation_name,
    event_details
INTO OUTFILE '/tmp/activity_log_export.csv'
FIELDS TERMINATED BY ','
ENCLOSED BY '"'
LINES TERMINATED BY '\n'
FROM user_activity_log
WHERE event_timestamp >= '2025-01-01'
ORDER BY event_timestamp DESC;
```

### Archive Old Logs

Keep database size manageable by archiving logs older than 1 year:

```sql
-- First, back up to archive table
CREATE TABLE IF NOT EXISTS user_activity_log_archive LIKE user_activity_log;

INSERT INTO user_activity_log_archive
SELECT * FROM user_activity_log
WHERE event_timestamp < DATE_SUB(NOW(), INTERVAL 1 YEAR);

-- Then delete from active table
DELETE FROM user_activity_log
WHERE event_timestamp < DATE_SUB(NOW(), INTERVAL 1 YEAR);
```

---

## User Account Management

### View All Active Users

```sql
SELECT 
    employee_number,
    windows_username,
    full_name,
    department,
    shift,
    created_date,
    CASE 
        WHEN visual_username IS NOT NULL THEN 'Yes'
        ELSE 'No'
    END AS has_erp_access
FROM users
WHERE is_active = TRUE
ORDER BY full_name;
```

### Find User by Employee Number

```sql
SELECT 
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    visual_username,
    created_date,
    created_by
FROM users
WHERE employee_number = 6229;
```

### Find User by Windows Username

```sql
SELECT 
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active
FROM users
WHERE windows_username = 'johnk';
```

### Deactivate User Account

```sql
UPDATE users
SET is_active = FALSE,
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Reactivate User Account

```sql
UPDATE users
SET is_active = TRUE,
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Reset User PIN

**Security Note**: This should only be done with proper authorization.

```sql
-- First, verify the new PIN is unique
SELECT COUNT(*) FROM users WHERE pin = '5678';

-- If count is 0, PIN is available
UPDATE users
SET pin = '5678',
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Update User Department

```sql
UPDATE users
SET department = 'Engineering',
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Update User Shift

```sql
UPDATE users
SET shift = '2nd Shift',
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Add ERP Credentials to Existing User

```sql
UPDATE users
SET visual_username = 'erp_username',
    visual_password = 'erp_password',
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Remove ERP Credentials

```sql
UPDATE users
SET visual_username = NULL,
    visual_password = NULL,
    modified_date = NOW()
WHERE employee_number = 1234;
```

### Check for Duplicate PINs

```sql
SELECT 
    pin,
    COUNT(*) AS duplicate_count,
    GROUP_CONCAT(employee_number) AS employee_numbers,
    GROUP_CONCAT(full_name) AS full_names
FROM users
WHERE is_active = TRUE
GROUP BY pin
HAVING duplicate_count > 1;
```

### List Users with ERP Access

```sql
SELECT 
    employee_number,
    full_name,
    department,
    visual_username,
    CASE 
        WHEN visual_password IS NOT NULL THEN '***SET***'
        ELSE 'Not Set'
    END AS password_status
FROM users
WHERE visual_username IS NOT NULL
  AND is_active = TRUE
ORDER BY full_name;
```

---

## Troubleshooting

### User Cannot Log In

**Problem**: User's Windows username or PIN not working

**Diagnostic Queries**:

1. Check if user exists:
   ```sql
   SELECT * FROM users WHERE windows_username = 'username' OR pin = '1234';
   ```

2. Check if account is active:
   ```sql
   SELECT is_active FROM users WHERE employee_number = 1234;
   ```

3. Check recent failed attempts:
   ```sql
   SELECT * FROM user_activity_log 
   WHERE username = 'username' 
     AND event_type = 'login_failed'
   ORDER BY event_timestamp DESC 
   LIMIT 5;
   ```

**Solutions**:
- If user not found: Create account via application (personal workstation)
- If account inactive: Reactivate with `UPDATE users SET is_active = TRUE WHERE...`
- If wrong PIN: Reset PIN (requires authorization)

### Workstation Not Detected Correctly

**Problem**: Computer showing wrong authentication method

**Diagnostic Queries**:

```sql
SELECT * FROM workstation_config WHERE computer_name = 'COMPUTERNAME';
```

**Solutions**:
- If not found: Computer defaults to personal workstation (Windows auth)
- If wrong type: Update with `UPDATE workstation_config SET workstation_type = ...`
- Add computer name to config if needed

### Department Not Appearing in Dropdown

**Problem**: Department not visible in new user creation

**Diagnostic Query**:

```sql
SELECT 
    department_name,
    is_active,
    sort_order
FROM departments
WHERE department_name = 'Department Name';
```

**Solutions**:
- If `is_active = FALSE`: Set to `TRUE`
- If not found: Add department with `INSERT INTO departments...`

### Session Timing Out Too Quickly

**Problem**: Application closes unexpectedly

**Note**: Timeout durations are hardcoded in application:
- Personal Workstation: 30 minutes
- Shared Terminal: 15 minutes

**Diagnostic Queries**:

```sql
-- Check recent timeout events
SELECT * FROM user_activity_log
WHERE event_type = 'session_timeout'
  AND username = 'username'
ORDER BY event_timestamp DESC
LIMIT 10;
```

**Solutions**:
- Verify activity tracking is working (mouse/keyboard should reset timer)
- Check if workstation type is correct (shared vs personal affects timeout)
- Review application logs for session management issues

---

## Maintenance Tasks

### Daily Tasks

1. **Monitor Failed Login Attempts**:
   ```sql
   SELECT username, COUNT(*) AS failures
   FROM user_activity_log
   WHERE event_type = 'login_failed'
     AND event_timestamp >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
   GROUP BY username
   HAVING failures > 5
   ORDER BY failures DESC;
   ```

2. **Check for Duplicate PINs** (run query from User Management section)

### Weekly Tasks

1. **Review New User Creation**:
   ```sql
   SELECT COUNT(*) AS new_users_this_week
   FROM users
   WHERE created_date >= DATE_SUB(NOW(), INTERVAL 7 DAY);
   ```

2. **Verify Workstation Configuration**:
   ```sql
   SELECT workstation_type, COUNT(*) AS count
   FROM workstation_config
   GROUP BY workstation_type;
   ```

### Monthly Tasks

1. **Archive Old Activity Logs** (see Archive Old Logs section)
2. **Review Department List**: Remove obsolete, add new departments
3. **Audit User Accounts**: Deactivate terminated employees

### Quarterly Tasks

1. **Security Audit**: Review failed login patterns
2. **Performance Review**: Check database query performance
3. **Backup Verification**: Ensure database backups are working

---

## Database Backup & Recovery

### Backup Commands

**Full Database Backup**:
```bash
mysqldump -u root -p mtm_receiving_application > mtm_receiving_backup_$(date +%Y%m%d).sql
```

**Tables Only (no data)**:
```bash
mysqldump -u root -p --no-data mtm_receiving_application > mtm_receiving_schema.sql
```

**Authentication Tables Only**:
```bash
mysqldump -u root -p mtm_receiving_application users workstation_config departments user_activity_log > auth_backup.sql
```

### Restore Commands

**Full Restore**:
```bash
mysql -u root -p mtm_receiving_application < mtm_receiving_backup_20251216.sql
```

**Selective Table Restore**:
```bash
mysql -u root -p mtm_receiving_application < auth_backup.sql
```

---

## Security Best Practices

1. **Access Control**: Limit database access to authorized personnel only
2. **Audit Regularly**: Review activity logs for suspicious patterns
3. **Backup Frequently**: Daily backups with offsite storage
4. **Monitor Failed Logins**: Investigate repeated failures
5. **PIN Resets**: Require proper authorization and documentation
6. **ERP Credentials**: Treat as sensitive data, limit access to `visual_password` column
7. **Deactivate Promptly**: Disable accounts for terminated employees immediately

---

## Contact & Support

For database issues or questions:
- **IT Support**: [Contact Information]
- **Application Developer**: [Contact Information]
- **Database Administrator**: [Contact Information]

---

**Document Version**: 1.0  
**Last Updated**: December 2025  
**Next Review**: March 2026
