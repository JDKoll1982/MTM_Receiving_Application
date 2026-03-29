# End-User Workflow And Mockups: Module_OutsideService

**Feature Branch**: `001-module-outside-service`
**Created**: 2026-03-27
**Last Updated**: 2026-03-29
**Version**: 1.0

---

## Quick Role Guide

| Who | What They Do In This Module |
| --- | --- |
| Outside Service Coordinator | Creates new outside-service requests and adds lines through the Add Line modal. |
| Shipping | Opens lines from the active waitlist, enters vendor and BOL information, and marks lines complete. |

---

## Lifecycle Overview

Every waitlist line follows this progression:

```
Initialize  -->  Setup  -->  Complete
(Coordinator)  (Shipping)  (Shipping)
```

Phase is tracked at the **line level**. Two lines in the same request can be in different phases at the same time.

---

## Per-Package Quantity Rule

A single line may contain one or more physical packages. Each package has its own quantity. The quantities do not have to be equal.

**Example** - Part A123, four packages:

| Package | Quantity |
| ------- | -------- |
| 1       | 10       |
| 2       | 15       |
| 3       | 8        |
| 4       | 20       |

The system stores all four values separately. The coordinator enters each one individually in the Add Line modal. They are never merged, averaged, or assumed equal.

---

## Screen-By-Screen Walkthrough

### Screen 1 - Request Entry

**Who uses it**: Outside Service Coordinator
**When**: Starting a new outside-service request

**Wireframe**: See `Mockups/OutsideService-Request-Entry.svg`

**Purpose**: The coordinator fills in the request header and reviews the lines already added to the current request before saving. Lines are added one at a time through the Add Line modal launched from this screen.

**What the user sees**:

- A request header area at the top with request notes and a Save Request button.
- A line list in the middle showing every line added so far. Each line row shows the part ID, package count, and a summary of packages.
- An Add Line button below the list that opens the Add Line modal.

**What the user does**:

1. Opens the screen. A new request starts empty.
2. Optionally types request notes in the header area.
3. Clicks "Add Line". The Add Line modal opens.
4. Repeats step three for each part to add.
5. Reviews the line list to confirm all lines are present.
6. Clicks "Save Request". The system saves the request and all lines to MySQL. Each line starts in `Initialize`.

**Important notes**:

- The Save Request button is disabled until at least one line has been added.
- The coordinator cannot save a request with zero lines.

---

### Screen 2 - Add Line Modal

**Who uses it**: Outside Service Coordinator
**When**: Adding a part line to the current request from the request-entry screen

**Wireframe**: See `Mockups/OutsideService-Add-Line-Modal.svg`

**Purpose**: The coordinator enters the Part ID, sets the number of packages, and then enters a quantity for each individual package.

**What the user sees**:

- A Part ID field at the top.
- A Package Count field showing how many physical packages will be on this line.
- A package-quantity table that dynamically adds or removes rows to match the Package Count. Each row represents one physical package and has its own quantity field.
- Save Line and Cancel buttons at the bottom.

**What the user does**:

1. Types the Part ID.
2. Enters Package Count. The table immediately adds or removes rows to match.
3. Types a quantity in each package row. Quantities can all be different.
4. Clicks "Save Line".

**What happens after Save Line**:

- The system validates the Part ID against Infor Visual.
- If the Part ID matches exactly, the line is added to the request and the modal closes.
- If the Part ID does not match, the Part Match Helper opens automatically. See Screen 3.

**Validation rules enforced before save**:

- Part ID must not be blank.
- Package Count must be a positive whole number.
- Every package row must have a quantity entered.
- All quantities must be positive numbers.
- The number of quantity rows must equal the Package Count.

**Example - four packages with different quantities**:

The coordinator enters Package Count = 4. The table shows four rows. They type 10, 15, 8, and 20 in those rows. Clicking Save Line stores four separate package records. The active waitlist later shows "4 pkgs" for this line.

---

### Screen 3 - Part Match Helper

**Who uses it**: Outside Service Coordinator
**When**: The entered Part ID in the Add Line modal does not exactly match any part in Infor Visual

**Wireframe**: See `Mockups/OutsideService-Part-Match-Helper.svg`

**Purpose**: Offers a plain-language list of Infor Visual parts that are similar to what was typed so the coordinator can pick the right one without memorizing exact formatting.

**What the user sees**:

- A header showing what was typed.
- A list of suggested matches. Each suggestion shows the part ID and a short description.
- A "Use This Part" button next to each suggestion.
- A "Go Back And Edit" option in case the suggestions are all wrong.

**What happens when the user picks a suggestion**:

- The Part Match Helper closes.
- The Add Line modal reappears with the Part ID field updated to the selected value.
- All other fields the coordinator had already filled in (Package Count and quantities) are still there exactly as left.

**What if there are no suggestions**:

- The helper shows a "No matches found" message.
- The coordinator clicks "Go Back And Edit" to return to the modal and correct the Part ID manually.

---

### Screen 4 - Active Waitlist

**Who uses it**: Outside Service Coordinator (view only) and Shipping (view and action)
**When**: Any time after at least one request has been saved

**Wireframe**: See `Mockups/OutsideService-Active-Waitlist.svg`

**Purpose**: Shows every open line grouped by phase so both roles can see what needs attention.

**What the user sees**:

- Two sections: "Initialize" and "Setup".
- Each line row shows: request number, part ID, package count, and for `Setup` lines also vendor and BOL number.
- A phase badge on each row.
- An "Open" button on each row.

**What Shipping does**:

- Clicks "Open" on an `Initialize` line to begin setup. The Setup screen opens.
- Clicks "Open" on a `Setup` line to review or complete it.

**Empty state**: When no open lines exist the screen shows "No open requests" instead of an empty grid.

---

### Screen 5 - Setup Screen

**Who uses it**: Shipping
**When**: Opening a line that is currently in `Initialize`

**Wireframe**: See `Mockups/OutsideService-Setup.svg`

**Purpose**: Shipping enters vendor and shipment details and moves the line to `Setup`.

**What the user sees**:

- A read-only header showing the part ID, package count, and the per-package quantity breakdown from Initialize. Each package row and its quantity are visible but not editable.
- A vendor section with suggested vendor names derived from prior outside-service history for this part ID. Each suggestion shows the vendor name and how recently it was used.
- A "Use a different vendor" text field for cases where none of the suggestions apply.
- A shipment section with BOL Number, Scheduled Ship Date, and Shipping Contact fields.
- A "Save Setup" button.

**What happens after Save Setup**:

- The system updates the line phase to `Setup`.
- The line moves into the "Setup" group on the active waitlist.
- The package-quantity breakdown is preserved unchanged.

**Vendor source tracking**:

When Shipping picks a suggestion the system records the source as "suggested". When they type a custom value the source is recorded as "custom". This distinction is stored with the line.

---

### Screen 6 - Complete Line (Within Setup Screen)

**Who uses it**: Shipping
**When**: The shipment has physically left the facility

**The "Complete Shipment" button appears at the bottom of the Setup Screen** once the line is in `Setup`.

**What the user does**:

1. Reviews the line details on the Setup Screen.
2. Types optional completion notes.
3. Clicks "Complete Shipment".

**What happens**:

- The system records the completion timestamp.
- The line phase changes to `Complete`.
- The line is removed from the active waitlist.
- The line becomes visible in the Completed History view.

---

### Screen 7 - Completed History

**Who uses it**: Both roles
**When**: Viewing lines that have already shipped

**Wireframe**: See `Mockups/OutsideService-Complete-History.svg`

**Purpose**: A read-only view of all completed lines for reference and audit.

**What the user sees**:

- A list of completed lines sorted by completion date, newest first.
- Each row shows: part ID, vendor, BOL number, package count, and completion timestamp.
- An "Open Details" button on each row.

**What opening a detail shows**:

- All shipment fields from Setup.
- The per-package quantity breakdown from Initialize showing each package and its individual quantity value.
- Completion notes.

---

## Common Questions

**Q: Can I change a quantity after saving the line?**
No. Once a request is saved the quantities are locked. Editing submitted lines is not supported in Version 1.

**Q: Do all packages on the same line have to have the same quantity?**
No. Each package can have a completely different quantity. The system stores them individually.

**Q: What if I type the wrong number for Package Count?**
You can change Package Count while the Add Line modal is still open. The quantity table adjusts automatically. Once the line is saved Package Count is locked.

**Q: What happens to the package quantities when Shipping does setup?**
They are preserved exactly as the coordinator entered them. Shipping can see them as read-only reference in the Setup screen.

**Q: Why does the Part Match Helper open automatically?**
Because typos and formatting differences in part numbers are common. The system catches mismatches immediately inside the modal so the coordinator can fix them without losing the other data already entered.

**Q: What if none of the vendor suggestions match?**
Shipping can type any vendor name in the custom field. The system saves it and records that it was a custom entry.

---

## Mockup File Index

| File | Screen |
| ---- | ------ |
| `Mockups/OutsideService-Request-Entry.svg` | Request Entry - coordinator starts a new request and reviews lines |
| `Mockups/OutsideService-Add-Line-Modal.svg` | Add Line Modal - coordinator enters Part ID and per-package quantities |
| `Mockups/OutsideService-Part-Match-Helper.svg` | Part Match Helper - shown when Part ID does not match Infor Visual |
| `Mockups/OutsideService-Active-Waitlist.svg` | Active Waitlist - grouped by Initialize and Setup |
| `Mockups/OutsideService-Setup.svg` | Setup Screen - Shipping enters vendor, BOL, and completion |
| `Mockups/OutsideService-Complete-History.svg` | Completed History - read-only view of finished lines |
