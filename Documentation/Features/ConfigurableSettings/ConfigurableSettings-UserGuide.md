# Configurable Settings - User Guide

## Overview

This guide explains the settings you can customize in the MTM Receiving Application.

## User Settings

### Window and UI Preferences
- **Window Size**: Main window default size (1200×800)
- **Window Position**: Save last position or always center
- **Theme**: Light, Dark, or System Default

### Display Settings
- **Backdrop Effect**: Enable/disable Mica material effect
- **Font Size**: Adjust text size for readability

##Session Settings
- **Timer Check Interval**: How often to check for session timeout (default: 60 seconds)

## System Settings (Admin Only)

These settings require administrator privileges and affect all users.

### Security
- **Max Login Attempts**: Maximum failed login attempts before lockout (default: 3)
- **Lockout Duration**: How long to show lockout message (default: 5 seconds)
- **Session Timeouts**: 
  - Personal workstation: 30 minutes
  - Shared terminal: 15 minutes

### Database
- **Connection Timeout**: Database command timeout (default: 60 seconds)
- **Max Retries**: Maximum database retry attempts (default: 3)
- **Retry Delays**: Exponential backoff delays (100ms, 200ms, 400ms)

### Validation
- **PIN Length**: Required PIN length (default: 4 digits)
- **Max Field Lengths**: Character limits for various input fields

---

**Last Updated**: December 2025  
**Version**: 1.0.0
