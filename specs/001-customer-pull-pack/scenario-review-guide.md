# Customer Pull n' Pack Scenario Review Guide

Last Updated: 2026-05-21

## Purpose

This document is a plain-language review guide for the Customer Pull n' Pack waitlist workflow.

It is written for end users and process reviewers, not developers.

Use it to review how the tool should behave in normal day-to-day work, in unusual situations, and in edge cases where the team needs the workflow to be clear before implementation continues.

## How To Use This Guide

For each scenario:

- read the situation
- picture what the user is trying to do
- decide whether the expected outcome matches how the team wants the workflow to behave
- mark anything that feels wrong, confusing, or incomplete

## Quick Navigation

- [Daily Use Scenarios](#daily-use-scenarios)
- [Ownership And Queue Scenarios](#ownership-and-queue-scenarios)
- [Problem And Exception Scenarios](#problem-and-exception-scenarios)
- [Completed Work And Recheck Scenarios](#completed-work-and-recheck-scenarios)
- [Open Review Questions](#open-review-questions)

## Daily Use Scenarios

### 1. Open The Tool For A Customer With Work

**Situation**

A user opens Customer Pull n' Pack and chooses a customer that has current demand.

**Expected end-user outcome**

- the report opens for one customer only
- the user sees due-date-driven lines
- shortage cues are visible
- any lines that already have waitlist work are clearly marked

**Why this matters**

This is the basic entry point for the whole feature. If this feels confusing, everything after it becomes harder to trust.

---

### 2. Open The Tool For A Customer With No Work

**Situation**

A user chooses a valid customer, but that customer has no current open demand.

**Expected end-user outcome**

- the tool does not show a blank report
- the user sees a clear no-open-demand screen
- the screen gives an obvious next step, such as changing customer or filters

**Why this matters**

Blank screens look broken. Users need a clear answer about whether there is truly no work or whether something failed.

---

### 3. Create Waitlist Work From Multiple Selected Lines

**Situation**

A requester selects several compatible customer-order lines for the same customer and parent part.

**Expected end-user outcome**

- the requester can save them in one action
- the system creates one waitlist item per selected source line
- the queue does not end up with one mixed record holding several different source lines inside it

**Why this matters**

This keeps queue ownership, status, and history easy to follow line by line.

---

### 4. Try To Create A Duplicate Waitlist Item For The Same Source Line

**Situation**

A requester sees a report line that already has open waitlist work and tries to create another request for the same source line.

**Expected review point**

This still needs a business decision.

**Review question**

Should the tool force the user into the existing waitlist item instead of allowing a duplicate open request?

---

## Ownership And Queue Scenarios

### 5. New Waitlist Item Before A Handler Takes It

**Situation**

A requester creates a new waitlist item and no material handler has touched it yet.

**Expected end-user outcome**

- the item starts in `Requested`
- no handler owns it yet
- it appears in the default open-work queue

**Why this matters**

This separates requester-created work from handler-owned work.

---

### 6. Default Open Queue At The Start Of A Shift

**Situation**

A material handler opens the queue at the start of a shift.

**Expected end-user outcome**

- the default queue shows `Requested`, `Accepted`, and `Problem`
- it does not show `Completed` and `Cancelled` unless the user changes the filter

**Why this matters**

This keeps the queue focused on work that still needs action.

---

### 7. Handler Accepts A Line And Becomes The Owner

**Situation**

A handler opens a `Requested` line and marks it `Accepted`.

**Expected end-user outcome**

- ownership is assigned at the moment of acceptance
- the line now shows the handler as the owner
- the line still has only one current status

**Why this matters**

Ownership needs one clear starting point so there is no argument about who is responsible for the line.

---

### 8. Requester Edits A Line After A Handler Has Accepted It

**Situation**

A requester notices they forgot an important note after a handler already owns the line.

**Expected end-user outcome**

- the requester can still add or edit requester-facing notes or context
- the requester cannot change handler-owned execution fields
- ownership does not change

**Why this matters**

The requester can still communicate, but they cannot accidentally interfere with active floor work.

---

### 9. Handler Is Reassigned Elsewhere In The Plant

**Situation**

A handler owns an `Accepted` line, but they are pulled away to another task elsewhere in the plant.

**Expected end-user outcome**

- the handler can un-assign themselves
- the line becomes unowned again
- another handler can take it by accepting it later

**Why this matters**

Ownership should be easy to hand off without creating confusion or hidden work.

---

### 10. Two People Save Changes To The Same Waitlist Item

**Situation**

Two users edit the same waitlist item close together.

**Expected end-user outcome**

- the most recent save becomes the saved state
- the line still shows who currently owns it
- audit fields show the latest updater

**Why this matters**

Users need to know what final state the system keeps when edits overlap.

---

## Problem And Exception Scenarios

### 11. No Selectable Sub-Part Locations Exist

**Situation**

A requester wants to create a waitlist item, but there are no selectable sub-part locations available for the chosen line.

**Expected end-user outcome**

- the requester is still allowed to create the item
- the requester must provide a note
- the new item is flagged for location review
- the location-review flag is separate from the normal line status

**Why this matters**

Work should not disappear just because the location data is incomplete.

---

### 12. Handler Marks A Line As Problem

**Situation**

A handler gets to the location and something is wrong.

**Expected end-user outcome**

- the handler changes the line to `Problem`
- the new status replaces the old status rather than stacking on top of it
- the handler must provide a reason

**Valid preset reasons**

- `Not in location`
- `Incorrect Qty`
- `Incorrect Part Number`

**Expected note behavior**

- if one of the preset reasons fits, the user can choose it
- if none of the preset reasons fits the scenario, the user can explain the situation in a note field

**Why this matters**

The team needs consistent reasons for common problems without blocking uncommon cases.

---

### 13. Current Review Issue: Who Owns A Problem Line?

**Situation**

A handler owns a line, marks it `Problem`, and saves it.

**Why this needs review**

The system still needs a final business decision on whether a `Problem` line keeps the same owner or immediately becomes unowned.

**Review options in plain language**

- the current owner keeps responsibility until they un-assign themselves
- the line becomes unowned immediately
- some other ownership rule is used

**Why this matters**

This affects accountability, reassignment, and how quickly problem lines move through the queue.

---

### 14. A Problem Reason Does Not Fit The Real Situation

**Situation**

A handler finds a real-world problem that is not well described by the preset reasons.

**Expected end-user outcome**

- the handler can still save `Problem`
- the handler explains the situation in the note field
- the workflow does not force the user into a wrong reason just to continue

**Why this matters**

Preset lists are useful, but they cannot cover every floor scenario.

---

## Completed Work And Recheck Scenarios

### 15. Source Data Changes After A Line Was Completed

**Situation**

A handler completed a line earlier, but later the source demand or location picture changes in Visual.

**Expected end-user outcome**

- the completed waitlist item stays completed
- the system does not reopen it automatically
- the requester sees a recheck indicator outside the waitlist status
- if needed, the requester creates a new waitlist item instead of reusing the old completed one

**Why this matters**

Completed work should remain historical truth, while still warning the requester that the source picture changed afterward.

---

### 16. Print While Some On-Screen Detail Was Never Expanded

**Situation**

The user prints a report or pull list without opening every detail section on screen first.

**Expected end-user outcome**

- the print output still includes all required printable information
- the print job is not limited to what happened to be visible on screen

**Why this matters**

Printed floor documents need to be complete even when the on-screen view is partially collapsed.

---

## Open Review Questions

These are the main scenarios that still need business review or confirmation:

### A. Duplicate Open Waitlist Items For The Same Source Line

Should the system allow more than one open waitlist item for the same source line?

Current concern:

- duplicates can split ownership and confuse status tracking

### B. Ownership Of A Line In Problem Status

When a handler marks a line `Problem`, should the current owner stay assigned until they explicitly un-assign themselves, or should the line become unowned automatically?

Current concern:

- this affects accountability and how problem lines get handed off

## Suggested Review Method

1. Review the Daily Use scenarios first.
2. Review the Ownership and Queue scenarios second.
3. Review the Problem and Exception scenarios third.
4. Decide the Open Review Questions last.

## Review Notes

Use this space during review:

- What feels correct?
- What feels risky?
- What feels too strict?
- What feels too loose?
- What still needs a business decision?