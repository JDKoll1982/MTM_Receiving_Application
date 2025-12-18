# Database Administration - Developer Guide

## Database Schema

See DATABASE_ADMIN.md (original file) for complete schema definitions.

Key tables:
- `users` - Employee authentication data
- `workstation_config` - Workstation type configuration
- `departments` - Department options
- `user_activity_log` - Audit trail

## Stored Procedures

- `sp_GetUserByWindowsUsername` - Retrieve user by Windows username
- `sp_ValidateUserPin` - Validate PIN credentials
- `sp_CreateNewUser` - Create new user account
- `sp_GetDepartments` - Get active departments
- `sp_LogUserActivity` - Record activity events

## Backup & Recovery

```bash
# Full backup
mysqldump -u root -p mtm_receiving_application > backup.sql

# Restore
mysql -u root -p mtm_receiving_application < backup.sql
```

## Security Best Practices

1. Limit database access to authorized personnel
2. Audit logs regularly for suspicious activity
3. Daily backups with offsite storage
4. Monitor failed login patterns
5. Require authorization for PIN resets

---

**Last Updated**: December 2025  
**Version**: 1.0.0
