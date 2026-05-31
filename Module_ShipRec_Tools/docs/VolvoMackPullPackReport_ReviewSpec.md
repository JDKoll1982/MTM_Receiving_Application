# Customer Pull n' Pack Tool — Review Spec

**Last Updated:** 2026-05-20

---

## 1. What This Is

This document describes a proposed **Ship/Rec Tool** that would reproduce the current **Volvo/Mack Pull n' Pack Report (By Due Date)** as the starting example, while expanding it into a **customer-selectable Pull n' Pack Tool** inside the MTM application.

This is intentionally written in a **plain-language review format** so it is easy to read and approve before it is turned into a formal development specification.

---

## 2. Goal

Create a new tool in **Ship/Rec Tools** that:

- shows the same operational information as the current Crystal report
- allows the user to choose **which customer** they want to run the report for
- lets users sort and filter the report instead of reading it as fixed pages
- adds a **waitlist** so departments can say what locations they want pulled
- lets material handlers record what has already been pulled
- allows the report to be printed from the MTM application
- keeps the Infor Visual data read-only while storing MTM-specific workflow updates in the MTM application

---

## 3. What The Current Crystal Report Appears To Show

Based on the three screenshots, the current report is a due-date-driven packing and pull report for Volvo/Mack work.

For the new tool, **Volvo/Mack should become just one selectable customer view**, not the only supported customer.

### Visible report header

- Title: **Volvo/Mack Pull n' Pack Report (By Due Date)**
- Legend:
  - **Red** = Not Enough Parts On Hand
  - **Yellow** = Late Order
- Page numbering is shown
- The report is currently formatted as a fixed multi-page print layout

### Visible row-level information

For each parent part / pack line, the report appears to show:

- customer order number
- part number
- location ID
- ship quantity
- pull date
- part header in the format `P/N: <part id>`
- large **QTY TO PACK** value
- **FG ON HAND** section for finished-goods availability and likely source location
- **SUB PARTS ON HAND** section showing component or related-part availability

### Behavior implied by the screenshots

- the report is grouped primarily by **due date / pull date**
- shortages are visually highlighted
- each parent part can display multiple scheduled lines underneath it
- this report is meant for warehouse or material-handling action, not just for review

---

## 4. What The New Tool Should Feel Like

The new tool should feel like a **live operational dashboard version** of the existing report.

Instead of paging through a static Crystal report, users should be able to:

- open one screen in the MTM application
- choose the customer they want to work with
- refresh the latest data from Infor Visual
- sort the report in different ways
- filter down to the lines they care about
- create or update waitlist entries from the report page
- work pull activity from a dedicated waitlist page
- print the current working view when needed

The tool should preserve the current report's business meaning, while removing the limitations of a fixed paper-style report.

---

## 5. Proposed Main Screen

### Recommended layout

The main report screen should be split into two clear areas:

| Area | Purpose |
|---|---|
| **Filters and sorting bar** | Lets the user choose customer, date range, shortage state, pulled state, and sort order |
| **Main pull/pack results grid** | Shows the same core rows the Crystal report shows today, but in a sortable grid |

### Waitlist surface model

The waitlist should be treated as a **separate entity from the report itself**.

That means:

- the report is the place where users review demand and create or update waitlist entries
- the waitlist is stored independently in the MTM application
- material handlers should use a dedicated **Waitlist page** as their primary working screen for pulling orders

### Report-page waitlist actions

From the report page, a user should be able to:

- create a new waitlist entry from a selected report row
- update an existing waitlist entry linked to a selected report row
- see whether a row already has a related waitlist entry
- jump from the report row to the related waitlist record when needed

### Dedicated waitlist page

The feature should also include a dedicated **Waitlist page** for material handlers.

That page should let a material handler:

- review all open waitlist entries
- sort and filter by status, location, customer, or requester
- open one waitlist entry at a time and work it to completion
- update handler status and notes
- complete, cancel, or flag a problem on the entry
- print pull-focused information from the waitlist side when needed

### Customer picker behavior

The screen should include a clear **Customer selector** near the top of the page.

The selector should:

- show both **Customer ID** and **Customer Name**
- allow quick searching by either ID or name
- default to a saved customer when one is configured in settings
- allow the user to switch customers without leaving the tool
- show the **full customer list** from Infor Visual, not just customers with current demand
- show a clear empty-state screen when the selected customer has no open demand instead of showing a blank report

Example display format:

- `VOLVO - Volvo Group`
- `MACK - Mack Trucks`
- `ABC123 - Customer Name`

### Recommended columns in the main grid

The interactive report should show, at minimum:

| Column | Plain-language meaning |
|---|---|
| **Pull Date** | When the material should be pulled |
| **Customer Order** | The order driving the request |
| **Parent Part** | The part being packed or staged |
| **Location ID** | The location the report is currently pointing the team to |
| **Ship Qty** | The shipment quantity tied to the order line |
| **Qty To Pack** | The sum of all ship-quantity values for the given part number |
| **FG On Hand** | Finished-goods quantity currently available |
| **FG Location** | Where that finished stock is located |
| **Sub Parts On Hand** | Related component availability summary |
| **Shortage Status** | Normal, Late Order, or not enough parts on hand |
| **Waitlist Status** | Not requested, requested, in progress, pulled, partial, short, blocked |
| **Pulled By** | Who marked the material as pulled |
| **Pulled Time** | When the pull was completed or updated |
| **Notes** | Optional handling notes or exceptions |

### Recommended visual behavior

- keep **red shortage highlighting** for urgent exceptions
- keep **yellow Late Order highlighting** for the same items the current report flags today
- allow expanding a parent part row to see the detailed schedule lines beneath it
- allow quick filtering to show only shortage rows, only waitlisted rows, or only incomplete pulls

---

## 6. Sorting And Filtering

This is one of the biggest improvements over the Crystal report.

### Users should be able to sort by

- pull date
- customer order
- parent part number
- location ID
- ship quantity
- qty to pack
- finished-goods on-hand quantity
- shortage severity
- waitlist status
- pulled status

### Users should be able to filter by

- customer
- date range
- customer or customer family
- shortage only
- late-order only
- part number
- location
- pulled / not pulled / partial
- department request
- handler name

### Default sort

The default sort should remain:

- **Pull Date ascending**
- then **Customer Order**
- then **Parent Part**

That keeps the screen aligned with how the current report is being consumed today.

---

## 7. Waitlist Function

This is the biggest business addition beyond recreating the report.

The waitlist is a **separate feature entity** that is fed by the report, but it is not just a temporary sub-panel inside the report.

### What the waitlist is for

The waitlist should let a department or internal requester tell the material-handling team:

- what location they want pulled from
- what part they need prioritized
- whether they want a full pull or a partial pull
- any notes that help the handler understand the request

### What the material handler should be able to do

For each waitlist entry, the handler should be able to mark:

- accepted
- completed
- cancelled
- problem

When a handler marks an item as `Problem`, they should be expected to add a useful note describing what went wrong, such as:

- part not found in the requested location
- quantity in the location does not match the report
- stock is short
- location needs review

### Waitlist permissions

- anyone can create a waitlist request
- users can create or update waitlist entries from the report page
- material handlers can edit someone else's waitlist entries when needed
- marking an item as completed does not require approval from another user
- completion status should reset automatically when the source report refreshes and the current part-location picture changes in Visual

### Waitlist data that should be captured

| Field | Purpose | Dev notes, update off this |
|---|---|---|
| **Waitlist Entry ID** | Uniquely identifies the waitlist record | Yes; hidden/internal system field needed so edits always update the correct row |
| **Customer ID** | Ties the waitlist entry to the selected customer | Yes; use the selected Infor Visual customer ID |
| **Customer Order** | Keeps the request linked to the order context shown on the report | Yes; helps distinguish the same part on multiple orders |
| **Source Report Row Key** | Links the waitlist entry back to the originating report row | Yes; hidden/internal field, likely built from customer, order, part, and location when no single source key exists |
| **Requested By Department** | Shows who asked for the pull | Not needed for this feature |
| **Requested By Person** | Name of the requester | Use the Receiving application's user's full name |
| **Requested Location** | The location the requester wants the handler to pull from | Yes |
| **Parent Part** | The requested part | Yes |
| **Requested Qty** | Quantity the department wants staged or pulled | Optional; default to the quantity currently shown in the selected location if not set |
| **Priority** | Normal, high, urgent | Not needed for this feature |
| **Request Time** | When the waitlist item was created | Yes |
| **Handler Status** | Current pull status | Yes; supported values should include Accepted, Completed, Cancelled, and Problem |
| **Handler Notes** | Notes about partials, shortages, or problems | Yes; material handlers should update this when the part is not in the location, quantities do not match, or other issues occur |
| **Pulled Qty** | Quantity actually pulled | Not needed for this feature |
| **Pulled By** | The handler who completed the work | Yes |
| **Pulled Time** | Timestamp of completion or last update | Yes |
| **Last Updated By** | Shows who last changed the waitlist row | Yes; additional audit/support field because not every update is a completed pull |
| **Last Updated Time** | Shows when the waitlist row was last changed | Yes; additional audit/support field for edits, status changes, and notes |

Some of these fields are user-facing and some are internal support fields. The internal fields are still important in the spec because the feature will need them to save, reopen, edit, filter, and print the correct waitlist records reliably.

### Important behavior

The waitlist should **NEVER** write anything back to Infor Visual.

It should live in the MTM application as an operational layer on top of the read-only report data.

The report and the waitlist should be related, but not treated as the same record set.

In plain language:

- the report shows what Visual says needs attention
- the waitlist stores what MTM users want pulled and how handlers are working that request

---

## 8. Printing

The tool should support printing because the current process clearly relies on a paper-friendly format.

The print output should **follow the same general HTML-styled reporting approach already used by the Material Availability Board feature**, rather than using a plain text dump or a basic grid export.

### Print format direction

The new feature's print reports should mimic the Material Availability Board print style by:

- generating a **browser-hosted HTML print document**
- using structured report sections instead of raw application-screen screenshots
- using print-friendly CSS for borders, spacing, headings, and tables
- preserving strong visual hierarchy so the printed result looks like a real warehouse report
- keeping shortage and status cues visually obvious in the print layout
- matching the provided reference screenshots as closely as practical
- targeting **landscape 8.5 x 11** paper output as the primary print layout

In plain language, the printout should feel like a polished internal HTML report that opens in the browser and prints cleanly, not like a simple exported table.

### Print options should include

- **Print current view**
- **Print floor copy**
- **Print pull list**
- **Print shortage-only view**
- **Print waitlist-only view**
- **Print one selected part or one selected customer order**

### Print pull list mode

The print pull list should produce a warehouse-focused location sheet for pulling activity.

Each unique sub-part should get its own table.

Each table should show:

- sub-part name in the table header
- total quantity needed
- total quantity on hand
- one row per selected location

The table columns should include:

- location
- quantity in location

### Print output should preserve

- report title
- run date and time
- selected filters
- shortage highlighting where possible
- grouped layout that remains easy for floor use

### Material Availability Board print traits to mimic

The new print reports should take visual and structural inspiration from the existing Material Availability Board output, including:

- a clear top-level report container
- bordered report sections or cards
- bold section headers
- full-width tables with print-friendly spacing
- consistent row and section styling
- print CSS that keeps colors visible on paper when possible
- browser print behavior that opens a ready-to-print HTML page

### Recommended print behavior

The printed output does not need to look exactly like Crystal Reports, and it does not need to be identical to the Material Availability Board report, but it should clearly follow the same HTML-report design language. It should:

- be easy to scan
- fit normal paper sizes cleanly
- not lose the shortage cues
- include enough detail that a printed copy is still useful on the floor
- use HTML report styling rather than a generic export style
- calculate and expand all printable fields before printing, even if some sections are lazily loaded during on-screen use

### Print decisions from review

- the printed layout should stay **close to the current Crystal report**
- color printing is expected
- the report should still remain understandable if a user ends up printing on a non-color printer
- the primary report print layout should match the provided screenshots as closely as practical
- the primary page setup should assume **landscape 8.5 x 11** paper
- the feature should include a separate **pull list** print mode for selected locations and sub-parts

---

## 9. Likely Infor Visual Data Sources

Using the CSV exports in `MTM_Waitlist_Application/Documents/InforVisualRelated/CSV_Documents`, the following sources appear to be the most likely building blocks for the new tool.

### Open order and schedule detail

| Table/View | Why it likely matters |
|---|---|
| **CUSTOMER** | Customer master list, including customer ID and customer display name |
| **CUSTOMER_ORDER** | Customer order header information |
| **CUST_ORDER_LINE** | Order-line part, quantity, and promise/ship detail |
| **CUST_BOOK_DEL** | Delivery schedule lines and date-driven booking detail |

### How customer IDs and names appear to work

Based on the CSV exports:

- `CUSTOMER.ID` appears to be the stable customer identifier used by Infor Visual
- `CUSTOMER.NAME` appears to be the readable customer name shown to users
- `CUSTOMER_ORDER.CUSTOMER_ID` points to `CUSTOMER.ID`

That means the new tool should treat the customer selector as:

- **stored value:** `CUSTOMER.ID`
- **display value:** `CUSTOMER.NAME`
- **working filter:** `CUSTOMER_ORDER.CUSTOMER_ID`

In plain language, the user should pick a customer by name, but the system should filter by the underlying customer ID.

### Part and inventory detail

| Table/View | Why it likely matters |
|---|---|
| **PART** | Part description and master data |
| **CR_PART_LOCATION** | Read-only location-level on-hand quantity view |
| **PART_LOCATION** | Base location inventory table behind on-hand/location detail |

### Likely source for sub-part availability

| Table/View | Why it likely matters |
|---|---|
| **REQUIREMENT** | Likely source for the bill-of-material / requirement explosion that drives the `SUB PARTS ON HAND` section |

### Plain-language interpretation

The visible report most likely combines:

- selected customer demand
- scheduled customer demand
- parent-part packing need
- qty-to-pack rollups calculated as the sum of ship quantities for the same part number
- current finished-goods stock by location
- component or related-part availability used to explain shortages

### Important constraint

Infor Visual remains **read-only** for this tool.

That means:

- the report data comes from Visual
- the waitlist and pull-status updates must be stored in the MTM application database, not in Visual

---

## 10. Settings Needed For This Feature

This feature should include a settings area so users do not have to reconfigure the tool every time they open it.

### Recommended settings section

The new feature should include a settings module or settings page that stores the user's preferred defaults for the tool.

### Recommended settings to support

| Setting | Purpose |
|---|---|
| **Default Customer** | Opens the tool to the most commonly used customer automatically |
| **Default Date Range Type** | Examples: today, next 3 days, next 7 days, custom rolling window |
| **Default Date Range Length** | Numeric value used with the chosen date range type |
| **Default Sort Order** | Example: pull date, then customer order, then parent part |
| **Default Filter: Show Shortages Only** | Lets some users open directly to shortage-only view |
| **Default Filter: Show Unpulled Only** | Useful for handlers who only care about open work |
| **Default Print Preset** | Example: floor copy or shortage review |
| **Auto Refresh Option** | Enables manual-only refresh or optional timed refresh |
| **Waitlist Default Status Filter** | Lets handlers default the waitlist page to open items such as Accepted or Problem |
| **Favorite Customers List** | Optional list of frequently used customers for quick access |

### Settings behavior

The settings should:

- be simple enough for end users to understand
- allow a sensible default customer to be preselected
- allow users to override the default customer at runtime
- never prevent a user from running the tool for a different customer
- support future expansion if more filters become important later
- remember the last good customer and date range used by the user

### Recommended defaults

Until the business chooses otherwise, the feature should assume these as the starting defaults:

- **Default customer:** user-configurable, blank if none chosen
- **Default date range:** next 7 days
- **Default sort:** pull date, then customer order, then parent part
- **Default filter:** show all rows
- **Default print preset:** floor copy

---

## 11. Business Rules To Preserve

The new tool should preserve these behaviors from the current report unless the business asks to change them.

### Preserve the current shortage meaning

- rows that are shortage conditions today should still be shortage conditions in the new tool
- red/yellow visual indicators should remain easy to understand

### Preserve due-date-driven planning

- users must still be able to work the list in pull-date order
- the report should remain useful as a daily pull sheet

### Preserve warehouse-first readability

- the screen should prioritize speed and clarity over technical detail
- a material handler should be able to understand the next action quickly

---

## 12. Recommended User Workflow

### Daily use flow

1. Open **Ship/Rec Tools**.
2. Open **Customer Pull n' Pack Tool**.
3. Confirm or change the selected customer.
4. Sort by pull date or shortage status.
5. Review shortage rows first.
6. Create or update waitlist requests for items that departments want pulled.
7. Material handler opens the dedicated **Waitlist page** to work pull orders.
8. Material handler updates waitlist status and notes from the waitlist page.
9. Print the current filtered view if a paper copy is needed.

### Shortage handling flow

1. User filters to shortage rows.
2. User reviews FG on hand and sub-parts on hand.
3. User adds notes or a waitlist request.
4. Handler marks the line as partial, short, or blocked as needed.

---

## 13. Review Decisions And Remaining Open Item

Most of the earlier review questions are now answered. The decisions below should be treated as the current approved direction unless you change them later.

### Confirmed report logic decisions

1. **QTY TO PACK** should be treated as the sum of all **Ship Qty** values for the given part number.
2. The **SUB PARTS ON HAND** section should be treated as a **bill-of-material / requirement explosion** result.
3. A row should be marked **Late Order** when the **Pull Date** is before today's date.
4. The new tool may improve the Crystal report's logic where the result becomes clearer, rather than copying every reporting quirk exactly.
5. Users should run the tool for **one customer at a time**.
6. The customer picker should show the **full customer master list**.
7. If the selected customer has no open demand, the tool should show a clear **no open demand** screen instead of rendering an empty report.

### Confirmed workflow decisions

1. Anyone can create waitlist requests.
2. Material handlers can edit someone else's waitlist entries.
3. Marking an item as pulled does not require approval or review by another user.
4. Pulled status should reset automatically when the source report refreshes and demand changes, since the part locations are coming from Visual.

### Confirmed printing decisions

1. The printed layout should stay close to the current Crystal report.
2. Color printing is expected, while the report should still remain readable if printed without color.
3. The primary report print layout should match the provided images as closely as practical.
4. The primary page setup should assume landscape 8.5 x 11 paper.
5. The feature should include a separate pull-list print mode for selected locations and sub-parts.
6. When printing, all printable sections should be calculated and expanded before the document is generated.

### Remaining open item

1. Is the **LOCATION ID** shown on the current report always the preferred pull location, or just the best currently available location?

---

## 14. Recommended Scope For Phase 1

To keep the first version practical, Phase 1 should focus on:

- matching the current report's core result set
- supporting one selected customer at a time
- adding sorting and filtering
- adding settings for default customer and default date range
- adding a simple waitlist
- allowing handlers to mark pulled / partial / short
- supporting print of the current view
- supporting a floor-copy print preset
- supporting a pull-list print mode

Phase 1 should avoid trying to solve every exception upfront.

---

## 15. Approved Review Additions

These points are now approved review direction to carry forward into the formal development specification.

1. **Keep the default screen very close to the current report.** Users already know the paper version, so the first screen should feel familiar.
2. **Make shortage rows the strongest visual element.** Those appear to be the most operationally important lines in the screenshots.
3. **Treat waitlist activity as MTM-only workflow data.** This avoids violating the read-only Infor Visual boundary.
4. **Add a quick toggle for `Show only items not yet pulled`.** That will likely be one of the most used daily filters.
5. **Build the print layout on the same HTML-report pattern used by Material Availability Board.** The print report should match the provided images as closely as practical and should target landscape 8.5 x 11 paper.
6. **Add a print preset called `Floor Copy`.** This should print the current working list with large readable columns and minimal clutter.
7. **Add a print pull list.** This should print all selected locations for pulling in table format. Each unique sub-part gets its own table, and each table should show the sub-part name plus total quantity needed versus total quantity on hand. The table columns should include location and quantity in location.
8. **Reuse the same browser-print workflow and CSS philosophy already proven in Material Availability Board.** That should make the result more maintainable and familiar.
9. **If possible, keep a one-click `Sort by pull date, then shortage` action.** That will likely match how the team triages work in real life.
10. **If the sub-parts section is expensive to calculate, lazy loading is acceptable for the on-screen experience.** When printing, all printable fields and sections must be fully calculated and expanded.
11. **Support one customer at a time only.** That keeps the tool easier to understand and closer to the existing report behavior.
12. **Let the settings page remember the last good customer and date range.** That will make the tool feel much faster for daily users.

---

## 16. Summary

The new tool should be a **live, sortable, printable, action-oriented version** of the current Pull n' Pack Crystal report, with the current Volvo/Mack report serving as the first known example.

It should keep the current report's meaning and shortage cues, while adding:

- customer selection
- interactive sorting
- filtering
- saved feature settings
- a department-driven waitlist
- handler pull-status tracking
- in-app printing using the same HTML-styled print-report direction as Material Availability Board

If this direction looks right, the next step would be to turn this into a **formal development spec** with user stories, acceptance criteria, technical boundaries, and a proposed data model for the MTM-managed waitlist layer.