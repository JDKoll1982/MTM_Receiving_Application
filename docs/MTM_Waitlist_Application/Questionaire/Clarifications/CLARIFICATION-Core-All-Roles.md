# MTM Waitlist Application – Core Questions (All Roles)

**Date:** January 21, 2026  
**Version:** 1.0  
**Audience:** All stakeholders

---

## Instructions

This questionnaire covers features that affect everyone using the system, regardless of role. Please fill this out along with your role-specific questionnaire.

**How to Answer:**

- Keep it simple - explain like you're talking to someone on the floor
- If you're not sure, pick what seems best and say who needs to approve it
- Write down who decided and when
- **This is a mockup** - feel free to suggest different approaches if something doesn't fit your needs

---

## Current System Feedback

### What Works with tablesready.com?

**List the things that work well now that we should keep in the new system:**

---

### What Doesn't Work with tablesready.com?

**List the pain points, frustrations, or missing features:**

---

### Must-Have Features

**What features are absolutely critical for the new system to have?**

---

## Section 1: Login & Access

### 1.1 How Should People Log In?

**We're considering these options:**

- Windows/AD (automatic login using your computer login)
- Username and password (type it in each time)
- Hybrid (Windows where it works, username/password as backup)

> **Why This Matters:**  
> This choice affects setup cost and how much support is needed (like password resets). Windows/AD means no passwords to manage, but it needs the right network setup.

> **Example:**  
> If the floor PCs are shared kiosks without AD, we'll need username/password. If supervisors use office PCs with Windows login, that could work automatically.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 1.2 Can One Person Have Multiple Roles?

**Question:** Can someone be assigned multiple roles at once (like both Material Handler and Material Handler Lead)?

**Common combinations in your facility:**

> **Why This Matters:**  
> If people can have multiple roles, it changes what screens they see and what they can do.

> **Example:**  
> A Production Lead who also runs a press during slow times might need both the Lead dashboard and the Operator screens.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 1.3 Who Can Change User Roles?

**Question:** Who should be able to add, change, or remove someone's role? Can roles be temporary?

**Options we're thinking about:** HR, Department Managers, IT, or some combination

> **Why This Matters:**  
> This keeps people from getting access they shouldn't have and tracks who made changes.

> **Example:**  
> HR wants to give someone an "Acting Lead" role for two weeks to cover vacation. Who approves it, and does it expire automatically?

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 1.4 Should Higher Roles Include Lower Role Access?

**Question:** Should a Lead automatically have all the same access as the people they supervise?

**Example:** Should a Production Lead automatically be able to do everything an Operator can do?

> **Why This Matters:**  
> This makes setup easier, but you need to be careful not to give people access to things they shouldn't see.

> **Example:**  
> If Leads inherit Operator access, we only assign one role instead of two. But maybe there are things Operators can do that Leads shouldn't?

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 1.5 Should Access Be Limited by Zone/Department/Shift?

**Question:** Should people only see and work on tasks for their specific zone, department, or shift?

**Consider:** Do Material Handlers in Zone 1 need to see Zone 3 tasks? Do day shift people need to see night shift tasks?

> **Why This Matters:**  
> Limiting by area keeps people focused on their work and makes it clear who's responsible.

> **Example:**  
> A Material Handler assigned to Zone 1 probably shouldn't claim tasks for Zone 3 unless they're helping out.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 1.6 Auto-Logout Rules

**Question:** How long should someone be able to sit idle before they're automatically logged out? Should it be different for shared kiosks vs office PCs?

**Consider:**

- Shared floor kiosk timeout: _____ minutes
- Office PC timeout: _____ minutes
- Can someone be logged in on multiple devices at once? Yes / No

> **Why This Matters:**  
> Short timeouts are more secure but might annoy people. Long timeouts are convenient but risky on shared computers.

> **Example:**  
> A kiosk on the floor might log out after 5 minutes to prevent the next person from seeing someone else's stuff. An office PC could wait 30 minutes.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Section 2: Request Basics (All Users)

### 2.1 What Information Is Required to Submit a Request?

**Question:** Which fields must be filled out before someone can submit a request?

**Check all that should be required:**

- [ ] Work order number
- [ ] Part number  
- [ ] Zone/location
- [ ] Priority level
- [ ] Description/notes
- [ ] Other: _______________________

> **Why This Matters:**  
> Missing info means handlers have to hunt for details before they start. But too many required fields slow down creating requests.

> **Example:**  
> Without a zone, the handler doesn't know where to go. Without a work order, the request might not tie to the right job.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 2.2 Can Requests Be Changed After Assignment?

**Question:** After a request is assigned to someone, can the requester still edit or cancel it?

**Options we're considering:**

- No changes after assignment
- Can cancel only (not edit)
- Can change until the handler starts work
- Can change anytime (handler gets notified)
- Other idea: _______________________

> **Why This Matters:**  
> Blocking changes means handlers know the request won't shift on them. Allowing changes gives flexibility for quick fixes but might mean redoing work.

> **Example:**  
> An operator submits a request for coils, then realizes they typed the wrong part number after a handler already accepted it. Can they fix it?

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 2.3 Favorites and Recent Requests

**Question:** How should the "favorites" and "recents" feature work?

**Settings to decide:**

- Keep the last _____ requests in "recents"
- Keep recents for _____ days
- Can people share favorite templates with their team? Yes / No
- If yes, who approves shared templates? _____________

> **Why This Matters:**  
> Favorites and recents make it faster to submit common requests. But too many clutter the list.

> **Example:**  
> Keep the last 10 requests for 7 days so people can quickly resubmit "Coils for Press 5." Maybe let Leads approve templates everyone can use.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 2.4 What Happens When Infor Visual Is Down?

**Question:** When work order or part validation fails (or Visual is offline), what should happen?

**Options:**

- **Block:** Don't allow the request until validation works
- **Warn:** Show a warning but let it through with a flag
- **Queue:** Accept it and validate later when Visual comes back
- Other approach: _______________________

> **Why This Matters:**  
> Blocking keeps bad data out but could stop work if the system is down. Allowing it with a flag keeps things moving but might let wrong info in.

> **Example:**  
> If Visual is offline, operators could still submit but the request gets marked "Needs Validation." When Visual is back, it checks automatically.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Section 3: User Interface Preferences

### 3.1 How Should the Menu/Navigation Work?

**Question:** How should people move around in the app?

**Options we're considering:**

- Sidebar menu (always visible on the left)
- Top tabs (across the top)
- Different layout for different roles
- Other idea: _______________________

> **Why This Matters:**  
> Consistent navigation makes it easier to learn and use.

> **Example:**  
> Material Handlers might want a simple sidebar with "My Tasks" and "Available Tasks." Leads might want tabs for "Dashboard," "Team Tasks," "Reports."

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 3.2 How Should Task Lists Display?

**Question:** When looking at a list of tasks, what format works best?

**Options:**

- List view (rows, compact)
- Grid view (table with columns)
- Card view (boxes with more detail)
- Let users toggle between views
- Other idea: _______________________

**Key filters/sorts to include:**

- [ ] Filter by zone
- [ ] Filter by status
- [ ] Filter by priority
- [ ] Sort by wait time
- [ ] Sort by oldest first
- [ ] Other: _______________________

> **Example:**  
> Default to list view for office users. Let people switch to card view on mobile carts. Always include filters for zone and status.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 3.3 Long Lists: Pages or Scroll?

**Question:** When there are lots of tasks, should we use pages or infinite scroll?

**Options:**

- Pages (show 10, 25, 50, or 100 items per page)
- Infinite scroll (keeps loading as you scroll)
- Hybrid (load 50 initially, then scroll for more)

> **Why This Matters:**  
> Pages keep kiosks running fast. Scroll is smoother but might slow down with hundreds of items.

> **Example:**  
> Use 25 items per page on shared kiosks. Office users might get 50 items with a "load more" button.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 3.4 Autocomplete and Barcode Scanning

**Question:** Which fields should have autocomplete (type-ahead suggestions) or barcode scanning?

**Check what makes sense:**

- [ ] Request categories - Autocomplete / Scan / Neither
- [ ] Request types - Autocomplete / Scan / Neither
- [ ] Zones - Autocomplete / Scan / Neither
- [ ] Work order numbers - Autocomplete / Scan / Neither
- [ ] Part numbers - Autocomplete / Scan / Neither
- [ ] Other: _______________________

> **Why This Matters:**  
> Autocomplete and scanning cut down on typos and speed up entry. But each one takes more work to build.

> **Example:**  
> Scan work orders and part numbers (most error-prone). Autocomplete zones (short list). Categories might just be dropdowns.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Section 4: Data & History

### 4.1 What Should We Track?

**Question:** What information should the system keep for each request/task?

**Check what you need:**

- [ ] Who assigned it (auto or manual, and who)
- [ ] Timestamps (created, accepted, ETA, completed)
- [ ] Location (Zone → specific station/press)
- [ ] Photos/attachments
- [ ] Task dependencies (one task blocks another)
- [ ] Recurring templates
- [ ] Other: _______________________

> **Why This Matters:**  
> This data drives reports, routing, and tracking. It also helps find problems.

> **Example:**  
> Photo proves damaged parts were received. Dependencies prevent starting "Install new die" before "Remove old die" is done.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 4.2 How Long Should We Keep Old Data?

**Question:** How long should tasks stay in the system?

**Timeframes:**

- Keep active/recent tasks for: _____ days
- Archive completed tasks for: _____ years
- Must archived tasks be searchable? Yes / No
- Delete very old data after: _____ years (or never)

> **Why This Matters:**  
> Keeping data longer helps with reports and investigations but uses more space and slows things down.

> **Example:**  
> Keep tasks in the main system for 90 days after completion. Archive for 2 years in a searchable form. After that, export to backup if regulations allow.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 4.3 What Changes Should Be Logged?

**Question:** What actions/changes should the system log for review?

**Check what to track:**

- [ ] Who changed priority and when
- [ ] Who reassigned tasks
- [ ] Who changed status
- [ ] Who cancelled tasks
- [ ] Who viewed what (for sensitive stuff)
- [ ] Login/logout activity
- [ ] Role/permission changes
- [ ] Other: _______________________

**How long to keep logs:** _____ months/years

**Should logs be searchable?** Yes / No

> **Why This Matters:**  
> Logs help investigate problems and meet compliance requirements. But more logging uses more space.

> **Example:**  
> Track who changed priority and when. Keep searchable for 1 year. After that, archive for another year in read-only storage.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Section 5: Reporting & Alerts

### 5.1 What Are the Top Daily Metrics?

**Question:** What numbers matter most day-to-day? Pick your top 5.

**Available metrics (check 5):**

- [ ] Average wait time (request to handler acceptance)
- [ ] Average completion time (acceptance to done)
- [ ] SLA breaches (tasks over target time)
- [ ] Current backlog (open task count)
- [ ] Handler utilization (% of time on tasks)
- [ ] Cancellation rate (% cancelled)
- [ ] Rework/error rate (tasks redone)
- [ ] Zone load (task count by zone)
- [ ] Other: _______________________

> **Why This Matters:**  
> Focusing on key metrics drives the right behavior. Too many overwhelms people.

> **Example:**  
> Top 5: Wait time, SLA breaches, backlog by category, handler utilization, cancellation rate. Show these on the Lead dashboard.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 5.2 Who Can See Performance Data?

**Question:** Should we track individual performance, and if so, who can see it?

**Options:**

- Track performance: Yes / No
- If yes, who can see their own stats: Everyone / Leads Only / Managers Only
- Who can see others' stats: Leads / Managers / HR / No One

> **Why This Matters:**  
> Performance data can drive improvement but can also create competition or resentment if not handled carefully.

> **Example:**  
> Track handler stats (average time, task count). Handlers see only their own. Leads see their team's individual stats for coaching. Managers see team totals, not individuals.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 5.3 Standard Reports

**Question:** What reports should run automatically?

**Schedule:**

- Daily report to: _____________ (who?) at: _____ AM/PM
- Weekly report to: _____________ (who?) on: _____________ (day)
- Monthly report to: _____________ (who?) on: _____________ (day of month)

**Delivery:**

- [ ] Email
- [ ] In-app only
- [ ] Both

**Format:**

- [ ] PDF (can't edit)
- [ ] Excel (can edit/analyze)
- [ ] CSV (raw data)
- [ ] Multiple formats

> **Example:**  
> Daily PDF to Leads at 6 AM (yesterday's stats). Weekly Excel to Plant Manager on Monday (allows custom analysis). Monthly PDF for management meetings.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 5.4 Alerts - When and How?

**Question:** When should the system send alerts, and how?

**Alert conditions (check what applies):**

- [ ] Task waiting more than _____ minutes
- [ ] Backlog over _____ tasks
- [ ] No handler available in a zone
- [ ] Handler utilization under ____%
- [ ] SLA breach

**How to notify:**

- [ ] In-app toast/popup
- [ ] Email
- [ ] SMS/text
- [ ] Other: _______________________

**Who gets alerts:**

- For operational issues: _______________________
- For strategic issues: _______________________

**Can thresholds be adjusted?** Yes / No  
**If yes, by whom:** _______________________

> **Example:**  
> Alert Production Lead (in-app + email) when task waits 20+ minutes or backlog >15. Alert Plant Manager (email) when SLA breach rate >10% for the day. Leads can adjust thresholds ±25%.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Section 6: Security & Privacy

### 6.1 Password Rules (If Not Using Windows Login)

**If using username/password, what are the requirements?**

- Minimum length: _____ characters
- Must include: [ ] Letters [ ] Numbers [ ] Special characters
- Prevent reusing last _____ passwords
- Expire after _____ days (or never)
- Lock after _____ failed attempts for _____ minutes
- Reset method: [ ] Self-service [ ] Requires supervisor [ ] IT only

> **Example:**  
> Minimum 8 characters, letters + numbers. No expiration. Lock after 5 tries for 15 minutes. Supervisors can reset or self-service with security questions.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 6.2 Who Can Delete What?

**Question:** How should deletion work?

**Options:**

- **Soft delete:** Mark as deleted but keep in database (can recover)
- **Hard delete:** Permanently remove (can't recover)

**Who can delete:**

- Tasks: _______________________
- Users: _______________________
- What happens to history when deleted: _______________________

> **Example:**  
> Soft delete only. Deleted items hidden from normal views but stay in database and logs. Only IT Admins can hard-delete after 2 years.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Section 7: System Performance

### 7.1 Expected Volume

**Question:** Roughly how many tasks and users do you expect?

- Tasks per day: _____________
- Peak simultaneous users: _____________
- Busiest time: _____________

> **Why This Matters:**  
> This helps size the system properly. Underestimating means slow performance; overestimating wastes money.

> **Example:**  
> 500 tasks/day across 3 shifts. Peak of 60 users at shift change (7 AM, 3 PM, 11 PM). 150 tasks/hour surge at day shift start.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

### 7.2 Speed Expectations

**Question:** How fast should things load?

- Task list: _____ seconds max
- Dashboard refresh: _____ seconds max
- Search results: _____ seconds max
- Reports: _____ seconds max

> **Example:**  
> Task list: 2 seconds. Dashboard: 1 second. Search: 3 seconds. Daily report: 10 seconds. Monthly report with charts: 30 seconds.

**Your Answer:**

**Decided By:** _________________ **Date:** _____________

---

## Notes & Suggestions

**Use this space for any ideas, concerns, or suggestions not covered above:**

---

**Completed By:** _________________________ **Date:** _____________  
**Department:** _________________________  
**Role:** _________________________

---

**Next Steps:**

1. Fill out your role-specific questionnaire
2. Return both forms by: _____________
3. Questions? Contact: _____________
