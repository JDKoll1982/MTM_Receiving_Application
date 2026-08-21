# Next Version Patch Notes

Last Updated: 2026-08-21

## Overview
This patch focuses on better day-to-day usability and much stronger user preference persistence across Receiving, Dunnage, Reporting, and Ship/Recv Tools.

## New: Reprint Labels
- Added a dedicated "Reprint Labels" page (accessible from the footer navigation, directly above Scanner) that replaces the old reprint button inside Receiving Edit Mode.
- Choose a module on the landing page: Receiving, Dunnage, or Volvo.
- Each module shows its label history with a date range filter (Today, Yesterday, Week, Month, Quarter), a search-by dropdown, and a search box.
- Select the history rows to reprint; rows that are already queued for reprint are shown in red and cannot be selected.
- Reprint Selected re-queues the checked rows back into that module's active label queue with `is_reprint = 1` (same behavior Receiving already used).
- After reprinting, a summary dialog reports how many rows were queued, how many were already queued, and offers Start Over or Mode Selection.
- Search By preference is saved per module and restored on return.
- Receiving, Dunnage, and Volvo reprint-capable label tables now all support `is_reprint`.

## New: PO Line Specification Search (Ship/Recv Tools)
- Added a new PO line specification search workflow in Ship/Recv Tools.
- Added dedicated options and text viewer dialogs to make result review easier.
- Added multiple search strategies to improve match quality:
  - Weighted ranking
  - Tokenized partial match
  - Exact phrase search
- Added status-filtered query variants to support cleaner operational filtering.

## Receiving Improvements
- Edit Mode preferences now save more reliably and automatically when changed:
  - Visible columns
  - Search By column
  - Sort column and sort direction
  - Page size
- These preferences now restore correctly on return, reducing repeated setup work.
- Guided workflow focus behavior was hardened so the active step reliably regains keyboard focus when navigating between steps and when a step becomes visible.

## Dunnage Improvements
- Improved consistency of location behavior across Dunnage workflow screens.
- Improved Dunnage history view behavior and supporting SQL view alignment.
- Added/validated user-scoped persistence for Dunnage preferences, including:
  - Preferred thumbnail size
  - Part image preference
  - Type selection sort preference
- Dunnage location picker now supports deeper location discovery:
  - Initial load can include a larger location set.
  - If local filtering returns no matches, it performs a fallback lookup using the typed text before showing "No matches".
- Improved Dunnage image-search return flow and part preselection reliability:
  - Back navigation now correctly returns to Image Search when Part Selection was opened from that flow.
  - Part selection restore logic now includes additional matching and deferred reassertion to reduce intermittent selection loss during UI refresh timing.

## Reporting Improvements
- Added persistent Reporting Preview settings so users keep their preferred setup between sessions.
- Row Display Mode now saves and restores automatically.
- Per-module preview preferences now save and restore, including:
  - Whether each module is included
  - Included columns
  - Sort key
  - Sort direction
- Improved module selection synchronization when loading saved preferences.
- Improved grouped Location display for Reporting preview rows:
  - Multi-location values now render with line breaks after commas, improving readability in the preview table.
- Fixed the End of Day Report preview so Dunnage and Volvo content renders correctly when Receiving is not included in the report.
- Fixed an app crash when clicking Back from the End of Day Report preview page (the preview TabView no longer clears its items during page teardown).

## Window and App Icon Fixes
- Fixed the main window showing a default/white icon on the Windows taskbar instead of the app icon.
- The shared window icon is now re-applied once when each window is first activated, so the taskbar reliably shows the app icon for every window (splash, main, settings, icon selector, and Volvo detail windows).

## Settings Reliability Hardening
- Added missing user-scoped settings manifest keys for preferences that were previously not consistently persisted.
- Added a dedicated Reporting settings contract and service for cleaner load/save behavior.
- Wired new settings services into dependency injection and updated related test construction.

## Infor Visual Data Access Enhancements
- Added new DAO/service-level support for PO line spec search data retrieval.
- Added supporting SQL query assets and validation scripts used to verify search quality and behavior.

## Documentation and Data Reference Organization
- Reorganized Infor Visual schema CSV reference assets into categorized folder structure for easier navigation and maintenance.

## User Impact Summary
- Less repeated setup every time you reopen screens.
- More predictable saved preferences across key workflows.
- Improved search tooling for PO line specs in Ship/Recv operations.
- Better consistency across Receiving, Dunnage, and Reporting user experiences.
- Faster, more reliable location lookup behavior in Dunnage when searching outside initially visible results.
- Cleaner Reporting location readability for multi-location grouped rows.

## Additional Navigation Update
- Main navigation Scanner entry is now accessible with an updated caution tooltip instead of being disabled.
