# Common Issues - Module_Receiving

**Last Updated: 2025-01-15**

## Error: "PO Not Found" or "Invalid PO Number"

**Symptoms**:
- Enter PO number and click Next
- Red error message appears
- System won't proceed to next step

**Quick Fixes**:
1. **Check for typos**: Re-enter PO number carefully
2. **Remove leading zeros**: Try entering 12345 instead of 00012345
3. **Check PO prefix**: Some facilities use PO-12345 format, try with or without prefix
4. **Verify PO exists in ERP**: Ask supervisor to check Infor Visual

**When to escalate**:
- PO definitely exists in ERP but system can't find it
- Error mentions "database connection" or "timeout"
- Issue persists for multiple different POs

**IT needs to know**:
- Exact PO number you tried
- Screenshot of error message
- Whether other users have same issue

---

## Error: "CSV Path Not Found" or "Access Denied"

**Symptoms**:
- Save completes but shows warning about CSV file
- Message says "Could not write to [path]"
- Local or network CSV creation failed

**Quick Fixes**:
1. **Check network connection**: Can you access other network drives?
2. **Verify local disk space**: Is your C: drive full?
3. **Close file if open**: Is the CSV currently open in Excel or other program?
4. **Check file permissions**: Do you have write access to the folder?

**Temporary workaround**:
- If only network CSV fails: Proceed anyway—local CSV and database are saved
- Manually copy local CSV to network location later
- Labels can be printed from local copy

**When to escalate**:
- Both local and network CSV fail
- Error says "Access Denied" or "Permission"
- Issue started suddenly (was working before)

**IT needs to know**:
- Which path is failing (local or network)
- Full path shown in error message
- Your Windows username
- Time when it started failing

---

## Error: "Database Connection Failed" or "MySQL Error"

**Symptoms**:
- CSV saves successfully
- Database save fails with red error
- Data not visible in reports or history

**Quick Fixes**:
1. **Check network**: Can you access other systems?
2. **Retry**: Click save again after a few seconds
3. **Session is preserved**: Your data is still in session, not lost

**Do NOT**:
- Re-enter all data from scratch
- Close the application (you'll lose session)

**When to escalate**:
- Database error persists after 2-3 retries
- Error mentions "timeout," "connection refused," or "authentication"
- Other modules also have database issues

**IT needs to know**:
- Exact error message (copy text or screenshot)
- Were you able to save earlier today?
- Are other users affected?

---

## Labels Print with Wrong Data

**Symptoms**:
- Labels print but show incorrect PO, part, or quantity
- Labels show data from previous receive
- Blank labels with no data

**Quick Fixes**:
1. **Check CSV file**:
   - Open the local CSV in Notepad
   - Verify data matches what you entered
2. **Verify label template**: Label printer might be using wrong template
3. **Clear label queue**: Cancel stale print jobs

**If CSV is wrong**:
- Use Edit Mode to reload and correct
- Re-save to regenerate CSV

**If CSV is correct but labels are wrong**:
- **Escalate to label system support**—not a receiving module issue
- Provide sample CSV file and sample label

**When to escalate**:
- CSV file content doesn't match what you entered
- Every save creates wrong CSV
- Labels are correct sometimes but wrong other times

**IT needs to know**:
- Sample CSV file (attach to ticket)
- What you expected vs. what printed
- Screenshot of data entered in receiving screen

---

## Session Won't Restore After Crash

**Symptoms**:
- App crashed or you closed it mid-receive
- Restart app but no "Restore Session" option appears
- Session recovery fails with error

**Quick Fixes**:
1. **Check session file exists**:
   - Path: `%APPDATA%\MTM_Receiving_Application\session.json`
   - Open in File Explorer (paste path into address bar)
   - If file is 0 bytes or missing, session is lost
2. **Try manual recovery**:
   - Look for auto-saved CSV in local path
   - Use Edit Mode to reload from CSV

**When to escalate**:
- Session file exists but won't load
- Error message when clicking "Restore Session"
- Happens frequently (session save is failing)

**IT needs to know**:
- Did crash happen during specific step?
- Error message from restore attempt
- Copy of session.json file (if it exists)

**Prevention**:
- Save frequently instead of completing everything in one session
- Session only saves when you complete a step and click Next

---

## Validation Errors: "Quantity Required" or "Invalid Weight"

**Symptoms**:
- Try to proceed or save
- Red text appears under specific fields
- Can't move forward until fixed

**Quick Fixes**:
1. **Read the error message carefully**: It tells you exactly what's wrong
2. **Common fixes**:
   - **"Quantity required"**: Can't be blank or zero
   - **"Invalid weight"**: Must be a positive number (no letters)
   - **"Heat/lot format"**: Check if facility has specific format requirements
   - **"Package type required"**: Must select from dropdown

**When to escalate**:
- Field is filled but still shows error
- Error message doesn't make sense
- No way to fix the specific validation

**IT needs to know**:
- Which field shows the error
- What value you're trying to enter
- Screenshot showing error message

---

## System Slow or Freezes During Save

**Symptoms**:
- Click Save button
- Screen freezes or shows "Saving..." for several minutes
- Eventually completes or times out

**Quick Fixes**:
1. **Wait it out**: If it's just slow, it may complete
2. **Check task manager**: Is MTM application "Not Responding"?
3. **Don't click Save multiple times**: Creates duplicate entries

**When to escalate**:
- Save takes more than 60 seconds consistently
- System freezes every time you save
- Other users report same slow behavior

**IT needs to know**:
- How long save typically takes
- Does it complete or error out?
- Time of day when slowness occurs
- How many loads you're saving at once

---

## Can't See Past Receives in Edit Mode

**Symptoms**:
- Go to Edit Mode → Load from Database
- Search for recent receives
- No results or missing expected entries

**Quick Fixes**:
1. **Widen date range**: May be filtering too narrowly
2. **Check spelling**: PO numbers or part IDs might be typed wrong
3. **Clear filters**: Remove all search criteria and search again

**Common causes**:
- Receive was never saved to database (check with user who entered it)
- Receive is there but under different user or date
- Database replication lag (just saved, not searchable yet)

**When to escalate**:
- Definitely saved but can't find it even with no filters
- Can see it in reports but not in Edit Mode
- Missing entire days of receiving data

**IT needs to know**:
- PO number you're searching for
- Date it was received
- User who entered it (if not you)
- Can you find it via reports or only Edit Mode fails?

---

## Part Number Shows "Quality Hold" Unexpectedly

**Symptoms**:
- Part appears in red or highlighted
- Message says "Quality Hold" or warning indicator
- Part was previously OK to receive

**Quick Fixes**:
1. **Contact Quality Control**: Part may have been recently flagged
2. **Check part master in ERP**: Quality hold status comes from there
3. **Do not bypass**: Receiving quality hold parts as normal stock is a compliance issue

**When to escalate**:
- Part is definitely not on quality hold (verified by QC)
- System shows quality hold for all parts (not just one)
- Quality hold flag won't clear even after QC updates ERP

**IT needs to know**:
- Part number showing incorrect status
- Who at QC confirmed it's not on hold
- Screenshot of quality hold message

---

## "Session Expired" Error

**Symptoms**:
- Working in receiving for extended time
- Get logged out or "session expired" message
- Lose unsaved work

**Quick Fixes**:
1. **Save frequently**: Don't leave app idle for long periods
2. **Log back in**: Session should be preserved if you saved before timeout
3. **Check session timeout settings**: May be configurable

**Prevention**:
- Complete receives in shorter time windows
- Save to database (not just session) before taking breaks

**When to escalate**:
- Session expires very quickly (less than 15 minutes)
- No timeout warning before losing work
- Login page doesn't appear (just crashes)

**IT needs to know**:
- How long you were idle before timeout
- Were you actively working or stepped away?
- Did you lose data or was it recovered?

---

## Receiving Screen Doesn't Load or Blank Page

**Symptoms**:
- Click on Receiving module
- Screen stays blank or shows loading spinner forever
- No error message

**Quick Fixes**:
1. **Wait 30 seconds**: Sometimes slow to render
2. **Close and reopen app**: Full restart
3. **Check network**: Module may require network data to load

**When to escalate**:
- Happens every time
- Other modules load fine, only Receiving fails
- Error appears in Windows Event Viewer

**IT needs to know**:
- Does it happen immediately or after waiting?
- Other modules working normally?
- Any recent Windows updates or system changes?

---

## When to Call for Help

**Call supervisor for**:
- PO or part number issues
- Quality or compliance questions
- Process exceptions or special receives

**Call IT for**:
- Application errors or crashes
- Network or database connectivity
- CSV file path or permission issues
- Missing data or report problems

**Emergency contact**:
- If receiving is completely down: [Contact manufacturing IT hotline]
- If blocking production: Notify supervisor AND IT immediately
