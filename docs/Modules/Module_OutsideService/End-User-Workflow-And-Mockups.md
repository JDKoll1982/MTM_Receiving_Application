# Module_OutsideService — End-User Workflow And Mockups

Last Updated: 2026-03-27

This document explains how the new Outside Service waitlist is expected to work for day-to-day users.
It is written for coordinators, Shipping, and support staff.
Version 1 focuses on creating and viewing open requests.
Later scheduling and shipped-history screens are included below as future-state mockups so the full workflow is easy to understand.

---

## Who This Is For

- Outside Service Coordinator
- Shipping team
- Leads and support staff who need to understand the process

---

## What Version 1 Includes

- Create a new outside-service request
- Add one or more part lines to the request
- Enter number of packages and quantity per package for each line
- See vendor suggestions based on prior history for the selected part
- Enter a custom vendor when no suggestion fits
- Save the request into an active waitlist
- View the open waitlist for Shipping follow-up

## What Version 1 Does Not Include

- Scheduling the shipment
- Entering a BOL number
- Marking the request as shipped
- Editing a saved request after it is submitted
- Cancelling a saved request

---

## Workflow Overview

### Version 1 Flow

```mermaid
flowchart TD
  W1_1_Start([Coordinator opens Outside Service]) --> W1_1_NewRequest[Start new request]
  W1_1_NewRequest --> W1_1_AddLine[Add one or more part lines]
  W1_1_AddLine --> W1_1_EnterPackageData[Enter package count and quantity per package]
  W1_1_EnterPackageData --> W1_1_VendorSuggestion{Vendor suggestion found?}
  W1_1_VendorSuggestion -->|Yes| W1_1_ChooseSuggestion[Choose suggested vendor]
  W1_1_VendorSuggestion -->|No| W1_1_CustomVendor[Enter custom vendor]
  W1_1_ChooseSuggestion --> W1_1_SaveRequest[Save request]
  W1_1_CustomVendor --> W1_1_SaveRequest
  W1_1_SaveRequest --> W1_1_SaveValid{All required fields valid?}
  W1_1_SaveValid -->|No| W1_1_ShowValidation[Show validation message]
  W1_1_ShowValidation --> W1_1_ReturnToForm[Return to request form]
  W1_1_ReturnToForm --> W1_1_AddLine
  W1_1_SaveValid -->|Yes| W1_1_AddToWaitlist[Add request to active waitlist]
  W1_1_AddToWaitlist --> W1_1_SuccessEnd([Request ready for Shipping review])
```

### Future Full Lifecycle

```mermaid
flowchart TD
  W2_1_Start([Saved request in active waitlist]) --> W2_1_ShippingReview[Shipping reviews request]
  W2_1_ShippingReview --> W2_1_ScheduleDecision{Ready to schedule?}
  W2_1_ScheduleDecision -->|No| W2_1_StayOpen[Remain in active waitlist]
  W2_1_StayOpen --> W2_1_OpenEnd([Still waiting for action])
  W2_1_ScheduleDecision -->|Yes| W2_1_AddShipmentData[Enter BOL number and shipment details]
  W2_1_AddShipmentData --> W2_1_ScheduledState[Move request to Shipment Scheduled]
  W2_1_ScheduledState --> W2_1_ShipConfirm{Shipment leaves facility?}
  W2_1_ShipConfirm -->|No| W2_1_ScheduledEnd([Remain in scheduled queue])
  W2_1_ShipConfirm -->|Yes| W2_1_MarkShipped[Mark request as Shipped]
  W2_1_MarkShipped --> W2_1_HistoryMove[Move request to history]
  W2_1_HistoryMove --> W2_1_HistoryEnd([Request visible in shipped history])
```

---

## Screen Summary

| Screen | Version | Purpose |
| ------ | ------- | ------- |
| `Outside Service Request Entry` | Version 1 | Create a new request with part lines and vendor selection |
| `Outside Service Active Waitlist` | Version 1 | Show open requests waiting for Shipping action |
| `Outside Service Shipment Scheduled` | Future Phase | Capture BOL number and shipment scheduling details |
| `Outside Service Shipped History` | Future Phase | Review requests that have already left the facility |

---

## Screen 1 — Outside Service Request Entry

This is the main Version 1 screen for the Outside Service Coordinator.

### What The User Does

1. Starts a new request.
2. Adds one or more part lines.
3. Enters package count and quantity per package for each line.
4. Chooses a suggested vendor or enters a custom vendor.
5. Saves the request.

### Request Entry Mockup

```text
+----------------------------------------------------------------------------------+
| Outside Service - New Request                                                    |
+----------------------------------------------------------------------------------+
| Requested By: [Current User                     ]   Request Date: [2026-03-27]   |
| Vendor:       [Suggested Vendor v]              [Use Custom Vendor]              |
| Custom Name:  [__________________________________________]                       |
| Notes:        [______________________________________________________________]   |
|                                                                                  |
| Part Lines                                                                       |
| [Add Part Line]                                                                  |
|                                                                                  |
| Line | Part ID        | Packages | Qty / Package | Vendor Hint                   |
| 1    | [ABC-123_____] | [4_____] | [25_______]   | Metro Heat Treat              |
| 2    | [XYZ-900_____] | [2_____] | [10_______]   | No prior vendor found         |
|                                                                                  |
| Suggested Vendors For Selected Part                                              |
| ( ) Metro Heat Treat       Last used recently                                    |
| ( ) Allied Plating         Used on prior outside-service work                    |
| ( ) Custom Vendor          Type vendor name above                                |
|                                                                                  |
| [Save Request]   [Clear Form]                                                    |
+----------------------------------------------------------------------------------+
```

### Request Entry User Notes

- The request cannot be saved unless every part line has valid counts.
- If no vendor suggestion appears, the user can still complete the request with a custom vendor.
- The vendor selection is meant to help speed up entry, not block it.

---

## Screen 2 — Outside Service Active Waitlist

This is the Version 1 list that Shipping uses to see open work.

### What The User Sees

- newest requests first
- current vendor choice
- number of part lines in the request
- request status, which will be `Initial Entry` in Version 1

### Active Waitlist Mockup

```text
+------------------------------------------------------------------------------------------------+
| Outside Service - Active Waitlist                                                              |
+------------------------------------------------------------------------------------------------+
| Search: [__________________________]   Status: [Initial Entry v]   [Refresh]                  |
|                                                                                                |
| Request #   Created On   Created By        Vendor              Lines   Status                  |
| OS-000128   03/27/2026   J. Smith          Metro Heat Treat    2       Initial Entry           |
| OS-000127   03/27/2026   A. Lopez          Custom: Blue Star   1       Initial Entry           |
| OS-000126   03/26/2026   J. Smith          Allied Plating      3       Initial Entry           |
|                                                                                                |
| Request Details                                                                               |
| Request #: OS-000128                                                                          |
| Vendor: Metro Heat Treat                                                                      |
| Lines: 2                                                                                      |
| - ABC-123 | 4 packages | 25 each                                                              |
| - XYZ-900 | 2 packages | 10 each                                                              |
|                                                                                                |
| [View Request]                                                                                |
+------------------------------------------------------------------------------------------------+
```

### Active Waitlist User Notes

- In Version 1 this list is for visibility only.
- Scheduling actions are not part of the first release.
- The existing Ship/Rec Outside Service History screen remains separate and continues to serve historical lookup needs.

---

## Screen 3 — Outside Service Shipment Scheduled

**Future Phase**

This screen is not part of Version 1.
It shows how Shipping would later confirm that a request is scheduled to leave.

### Shipment Scheduled Mockup

```text
+----------------------------------------------------------------------------------------------+
| Outside Service - Shipment Scheduled                                                         |
+----------------------------------------------------------------------------------------------+
| Request #: [OS-000128]     Vendor: [Metro Heat Treat____________________]                    |
| BOL Number: [____________________________________]                                           |
| Scheduled Ship Date: [03/30/2026]   Pickup Window: [2:00 PM - 4:00 PM]                      |
| Shipping Contact: [________________________________]                                         |
| Notes: [__________________________________________________________________________________] |
|                                                                                              |
| [Save As Scheduled]   [Back To Waitlist]                                                     |
+----------------------------------------------------------------------------------------------+
```

### Shipment Scheduled Notes

- This phase is where Shipping would add the BOL number.
- Once saved, the request would move from `Initial Entry` to `Shipment Scheduled`.

---

## Screen 4 — Outside Service Shipped History

**Future Phase**

This screen is also not part of Version 1.
It shows what the end-state history screen may look like after requests are marked as shipped.

### Shipped History Mockup

```text
+------------------------------------------------------------------------------------------------+
| Outside Service - Shipped History                                                             |
+------------------------------------------------------------------------------------------------+
| Search: [________________________]   Vendor: [All v]   Date Range: [Last 30 Days v]          |
|                                                                                                |
| Request #   Ship Date    BOL Number      Vendor              Status                           |
| OS-000091   03/19/2026   BOL-445991      Metro Heat Treat    Shipped                          |
| OS-000087   03/17/2026   BOL-445870      Allied Plating      Shipped                          |
|                                                                                                |
| [Open History Record]                                                                         |
+------------------------------------------------------------------------------------------------+
```

### Shipped History Notes

- This is the screen that would replace manual tracking for completed shipments.
- It is intended for history and follow-up, not for new request entry.

---

## Daily Workflow Summary

| Role | Version 1 Action |
| ---- | ---------------- |
| Outside Service Coordinator | Create new requests and choose a vendor suggestion or custom vendor |
| Shipping | Review the active waitlist and prepare for future scheduling work |
| Support Staff | Use the screen layout and lifecycle summary to understand what is currently live versus future-state |

---

## Support Notes

- If a user asks where BOL number entry is, the correct answer for Version 1 is: not available yet.
- If a user asks why the request does not appear in Ship/Rec history, the answer is: the waitlist and the history lookup are separate tools.
- If a user asks why there is no vendor suggestion, the answer is: the part may not have prior outside-service history, and custom vendor entry should be used instead.
