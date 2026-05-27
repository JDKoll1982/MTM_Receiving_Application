# Customer Pull n' Pack Implementation Notes

Last Updated: 2026-05-26

## Purpose

This note summarizes the current in-repo implementation of Customer Pull n' Pack in Module_ShipRec_Tools.

It is intended for maintainers who need a quick map of what is already built, which surfaces are connected, and what still needs manual validation.

## Delivered Surfaces

- Ship/Rec tool-selection routing includes both the Customer Pull n' Pack report entry and the standalone `Customer Pull n' Pack Waitlist` entry.
- The main report page supports one-customer demand review, filter/sort controls, empty-state messaging, duplicate-open-waitlist warnings, and report-side waitlist actions including `Show Waitlist` navigation.
- The filter panel is collapsed by default and keeps the primary actions visible in the expander header.
- The Ship/Rec host widens the shell while Customer Pull n' Pack is active and restores the standard MainWindow size when the user leaves the tool.
- The embedded crystal-style report control is bound into the report workflow and is used to review grouped request lines, finished-goods context, and selectable SUB PARTS ON HAND rows.
- The old bottom location-selection card has been removed, leaving the crystal-style sub-part rows as the only report-side location-selection surface.
- The waitlist editor dialog supports create and update flows, requester-facing note edits, and post-acceptance field restrictions.
- The dedicated waitlist queue page supports Requested, Accepted, Problem, Completed, and Cancelled filtering, single-item review, status updates, problem reasons, unassign-owner behavior, and print-preview launch.
- The defaults dialog supports default customer, favorite customers, saved date range, saved filters, saved waitlist status set, and saved print preset management.
- Floor-copy and pull-list preview pages exist and are launched from report or queue print actions.

## Current Data And Selection Flow

- Report demand and location context are loaded through the Customer Pull n' Pack report query and demand DAO.
- Linked waitlist state is overlaid into the report so duplicate open requests can be blocked and reopened.
- The report ViewModel owns the live selection state used by waitlist creation and update.
- The embedded crystal-style control reflects report grouping and forwards selection changes back into the report ViewModel instead of maintaining an isolated fake selection model.
- The report now enforces one active parent-part selection group at a time, and refresh/filter actions clear invalidated selections with a warning only when the user had active selections.
- `QTY SELECTED` now reflects selected sub-part quantities for the active parent-part group instead of selected request-line quantities.
- Request-line status is rendered as simplified colored notes (`Normal`, `Shortage`, `Late Order`, `Waitlist`) instead of the earlier badge-style treatment.
- The selectable sub-part rows now use a one-line `PART_ID • LOCATION_ID` presentation inside the crystal-style control.
- Queue updates flow through the queue query, update-status command, and unassign-owner command over the existing waitlist DAO seam.
- Defaults load and save flow through the defaults query and save-defaults command over the user-defaults DAO seam.
- Print preview contexts are built through the print-context query so preview pages do not depend on whatever happens to be expanded on screen.

## Validation Completed In Code

- Application build succeeds for the Customer Pull n' Pack slice.
- Focused Customer Pull n' Pack tests currently pass for the report slice, waitlist queue slice, main-host routing, and tool-selection routing.
- Queue activation is resilient when no saved defaults row is returned.

## Remaining Manual Validation

- Run the full quickstart walkthrough against live or seeded data and capture the 30-second load and 60-second print timing checks.
- Confirm the report-side row highlight is visually strong enough for real users during end-to-end review.
- Confirm the floor-copy and pull-list preview layouts are acceptable for production printing.

## Key Files

- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackReport.cs`
- `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackReport.xaml`
- `Module_ShipRec_Tools/Views/Controls/View_CustomerPullPack_CrystalReportLines.xaml`
- `Module_ShipRec_Tools/ViewModels/ViewModel_Tool_CustomerPullPackQueue.cs`
- `Module_ShipRec_Tools/Views/View_Tool_CustomerPullPackQueue.xaml`
- `Module_ShipRec_Tools/Views/View_ShipRecTools_Main.xaml`
- `Module_ShipRec_Tools/ViewModels/ViewModel_ShipRecTools_Main.cs`
- `Module_ShipRec_Tools/Services/Service_ShipRecTools_Navigation.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackReportHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackWaitlistQueueHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackDefaultsHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Queries/Query_CustomerPullPackPrintContextHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackBatchUpsertHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUpdateStatusHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackUnassignOwnerHandler.cs`
- `Module_ShipRec_Tools/Services/CustomerPullPack/Commands/Command_CustomerPullPackSaveDefaultsHandler.cs`