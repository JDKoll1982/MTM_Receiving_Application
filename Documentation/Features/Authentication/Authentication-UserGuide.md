# Authentication - User Guide

## Overview

The MTM Receiving Application uses different login methods depending on your workstation type. This guide explains how to log in and troubleshoot common authentication issues.

---

## How to Log In

### Personal Workstation (Automatic Login)

If you're using your **personal office computer**, the application logs you in automatically.

**What happens:**
1. Application opens
2. Reads your Windows username
3. Logs you in automatically
4. Main application window appears

**No action required** - you'll be logged in within seconds.

**Your computer is a personal workstation if:**
- It's assigned to you (office desk computer, laptop)
- You log into Windows with your own username
- It's not on the shop floor

---

### Shared Terminal (PIN Login)

If you're using a **shared shop floor computer**, you must log in with your username and PIN.

**What happens:**
1. Application opens
2. Login dialog appears
3. You enter your username and 4-digit PIN
4. Main application window appears

**Shop floor terminals include:** SHOP2, MTMDC, and other SHOP-FLOOR-* computers

**Steps:**
1. Type your username (same as your Windows login)
2. Type your 4-digit PIN
3. Click **Login** or press **Enter**

**Security:**
- You have **3 attempts** to enter the correct PIN
- After 3 failed attempts, the application closes for security
- All login attempts are logged

---

### First-Time Setup (New Users)

If you're using the application for the **first time on your personal workstation**, you'll see the New User Setup dialog.

**Steps:**
1. Application opens and detects you're a new user
2. New User Setup dialog appears
3. **Fill in required information:**
   - **Full Name**: Your first and last name (e.g., "John Smith")
   - **Windows Username**: Automatically filled (read-only)
   - **Shift**: Select your work shift (1st, 2nd, or 3rd)
   - **4-Digit PIN**: Create a unique 4-digit PIN for shop floor access
   - **Confirm PIN**: Re-enter your PIN to confirm
   - **Department**: Optional (e.g., "Receiving")
4. Click **Create Account**
5. Your employee number is assigned automatically
6. Application continues loading

**PIN Requirements:**
- Must be exactly 4 numeric digits
- Must be unique (cannot match another employee's PIN)
- Used for shop floor terminal access

**Important:** Contact your supervisor if you need help creating your account.

---

## Session Timeout

For security, your session automatically times out after a period of inactivity.

### Timeout Durations

- **Personal Workstation**: 30 minutes of inactivity
- **Shared Terminal**: 15 minutes of inactivity

### What Counts as Activity

The following actions reset the inactivity timer:
- Moving your mouse
- Typing on the keyboard
- Clicking buttons or controls
- Switching between application windows

### When Session Times Out

If your session times out:
1. Application closes automatically
2. Your work is saved (if auto-save is enabled)
3. You must log in again to continue

**Tip:** Keep the application active by performing actions periodically if working on long tasks.

---

## Optional: ERP Credentials

During account creation, you can optionally provide your Visual/Infor ERP credentials. This is for **future integration features only**.

**To configure ERP access:**
1. During New User Setup, check the box **"Configure Visual/Infor ERP Access"**
2. Enter your ERP username and password
3. Complete account creation

**Note:** 
- ERP credentials are stored in plain text (required for API integration)
- Only provide these if instructed by IT or your supervisor
- This feature is not currently used

---

## Troubleshooting

### Cannot Log In (Personal Workstation)

**Problem:** Windows username not recognized

**Possible causes:**
- You're a new user (no account created yet)
- Your account is inactive
- Computer is incorrectly configured as shared terminal

**Solutions:**
1. **New user:** New User Setup dialog should appear automatically
2. **Inactive account:** Contact supervisor or IT to reactivate your account
3. **Wrong workstation type:** Contact IT to update workstation configuration

---

### Cannot Log In (Shared Terminal)

**Problem:** Invalid username or PIN message

**Possible causes:**
- Incorrect PIN
- Account is inactive
- Typing error

**Solutions:**
1. **Verify your PIN:** Ask supervisor if you forgot your PIN
2. **Check for typos:** Retype carefully
3. **Inactive account:** Contact supervisor to reactivate
4. **Locked out after 3 attempts:** Wait and try again, or contact supervisor

---

### Session Timing Out Too Quickly

**Problem:** Application closes unexpectedly during work

**Possible causes:**
- Inactivity timeout triggered
- Mouse/keyboard not being detected
- Application issue

**Solutions:**
1. **Stay active:** Move mouse or type periodically
2. **Check timeout setting:** Personal (30 min) vs Shared (15 min)
3. **Contact IT:** If timing out during active use

---

### Forgot PIN

**Problem:** Cannot remember 4-digit PIN for shared terminal

**Solutions:**
1. **Try logging in on personal workstation:** Uses Windows authentication, no PIN required
2. **Contact supervisor:** They can look up or reset your PIN
3. **View employee records:** Supervisor can check database for your PIN

**Note:** PINs cannot be self-service reset for security reasons.

---

### Wrong Employee Number Shown

**Problem:** Application shows wrong employee number or name

**Possible causes:**
- Logged in with different Windows username
- Database record incorrect

**Solutions:**
1. **Verify Windows username:** Check who you're logged in as (Windows Start menu)
2. **Contact supervisor:** Have them verify database record
3. **Log out and back in:** Restart application to refresh session

---

## FAQ

### Q: Do I need to create an account?
**A:** If you're using your personal workstation for the first time, yes. The application walks you through account creation automatically.

### Q: Can I change my PIN?
**A:** Not currently. Contact your supervisor to update your PIN in the database.

### Q: Why do I need a PIN if I use a personal workstation?
**A:** Your PIN is used when accessing shared shop floor terminals. You won't use it on your personal computer.

### Q: What happens if I enter the wrong PIN 3 times?
**A:** The application closes for security. Wait a few minutes and try again, or contact your supervisor.

### Q: Can I use the same PIN as another employee?
**A:** No, each employee must have a unique 4-digit PIN.

### Q: Who can see my ERP credentials?
**A:** Only database administrators and IT staff have access to the database where credentials are stored.

### Q: What if the application won't open?
**A:** 
1. Check that you have network connectivity
2. Verify database server is running
3. Contact IT support

---

## Contact & Support

For authentication issues:
- **Forgot PIN / Account Issues:** Contact your supervisor
- **Technical Problems:** Contact IT Support
- **New Account Setup Help:** Contact your supervisor

---

**Last Updated**: December 2025  
**Version**: 1.0.0
