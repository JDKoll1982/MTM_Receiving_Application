# Customer Pull n' Pack Handler Workflow Mockup Package 01

Last Updated: 2026-05-20

## Purpose

This document narrows the mockup scope to the **material-handler waitlist workflow**.

It is intended to help produce a second, more execution-focused round of mockups after the broader package. The emphasis here is on the screens a handler uses to review, work, and complete pull orders from the dedicated waitlist page.

---

## Flow Order

1. Waitlist queue page
2. Waitlist detail accepted state
3. Waitlist detail problem state
4. Completed update state
5. Pull-list print mode
6. Empty queue state

---

## Screen 01

### Screen name

Waitlist Queue Page

### Purpose

Show the handler's main work queue for open pull orders.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page

### Key visible controls

- page title for waitlist work area
- status filter controls
- location/customer/requester filters
- dense queue grid or master list
- open selected item action
- refresh action
- print pull list action

### Key user actions

- scan open work
- filter to current assignments or current states
- select an item to work
- print a pull list

### Short rationale

This page is the handler's home base. It should optimize for speed, scanning, and action initiation.

---

## Screen 02

### Screen name

Waitlist Detail - Accepted State

### Purpose

Show the state after a handler has accepted an item and is actively working it.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page -> Selected Item

### Key visible controls

- selected work summary
- customer/order/part/location fields
- requested quantity
- accepted status clearly shown
- notes area
- action buttons for completed, cancelled, and problem
- save/update action

### Key user actions

- confirm work context
- add notes
- move item forward to completed or problem

### Short rationale

Once work begins, the screen should reduce ambiguity and help the handler complete the job with minimal extra navigation.

---

## Screen 03

### Screen name

Waitlist Detail - Problem State

### Purpose

Show how the workflow behaves when the handler cannot complete the pull normally.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page -> Selected Item

### Key visible controls

- problem status emphasized
- notes entry made visually prominent
- helpful prompt text for common issues
- save/update action
- optional inline feedback after save

### Key user actions

- mark problem
- enter explanatory note
- save problem update

### Short rationale

Problem handling is critical operationally. The mockup should make good note-taking and clear status communication feel required.

---

## Screen 04

### Screen name

Waitlist Detail - Completed Update

### Purpose

Show what the handler sees when completing a waitlist item successfully.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page -> Selected Item

### Key visible controls

- completed status shown clearly
- pulled by / pulled time visible
- optional completion note field
- confirmation feedback area
- return to queue action

### Key user actions

- mark completed
- save
- return to queue

### Short rationale

This state should feel conclusive and fast, so handlers can move to the next item without confusion.

---

## Screen 05

### Screen name

Pull List Print Mode

### Purpose

Show the handler-oriented print mode used to pull material by location and sub-part.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page -> Print Pull List

### Key visible controls

- print preview header
- one table per unique sub-part
- sub-part name in header
- total quantity needed
- total quantity on hand
- location and quantity-in-location rows

### Key user actions

- preview pull list
- print
- return to waitlist queue

### Short rationale

This screen supports floor execution. It should read like an action sheet, not like a reporting dashboard.

---

## Screen 06

### Screen name

Empty Queue State

### Purpose

Show the waitlist page when no open items currently match the active filters.

### Where it lives in app navigation

Main application shell -> Ship/Rec Tools -> Customer Pull n' Pack -> Waitlist Page

### Key visible controls

- filter bar remains visible
- empty-state message
- quick actions to clear filters or refresh

### Key user actions

- understand there is no current work in view
- adjust filters
- refresh queue

### Short rationale

This helps the waitlist page feel dependable rather than broken when no matching items exist.

---

## Handler Workflow Notes

- This package is intentionally narrower than the broader feature package.
- It should focus on execution speed, state clarity, and problem handling.
- The waitlist queue and detail screens should feel more like a work console than a report.
