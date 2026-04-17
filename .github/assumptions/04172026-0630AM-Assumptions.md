Last Updated: 2026-04-17

# Assumptions Requiring Confirmation

The requested work covers three settings modules and includes destructive restructuring. Per repo rules, implementation must stop here until these assumptions are confirmed or corrected.

## 1. Reporting Placeholder Scope

Assumption:
`Module_Settings.Reporting` should be treated exactly like the recent Volvo settings cleanup: all existing Reporting settings child pages and their view models should be removed from runtime navigation and DI, and the Reporting navigation hub should be replaced by a single placeholder page stating that Reporting settings are not currently available.

Why this assumption is needed:
The request says to do the “exact same thing” as Volvo, but does not explicitly say whether the existing Reporting settings pages should be deleted, excluded from the project, or retained but unreachable.

Potential impact if wrong:
If the intent was only to hide the cards while preserving some pages, removing them from DI and navigation would over-disable the module.

Alternative interpretations considered:

1. Keep existing Reporting settings pages compiled but remove only the navigation cards.
2. Exclude the existing Reporting settings pages from the project, mirroring Volvo more strictly.
3. Keep one or more existing Reporting pages and only replace the landing page with a placeholder.

## 2. Dunnage Recreation Means New Runtime Files Only

Assumption:
For `Module_Settings.Dunnage`, “do not reuse existing files” means I should create a new replacement navigation hub and a new replacement set of settings pages/view models for the reorganized Dunnage workflow, then remove the current Dunnage settings files from runtime registration and navigation to avoid future confusion.

Why this assumption is needed:
The request explicitly says not to reuse existing files and to think of it as a full recreation, but does not specify whether the old files should be deleted from disk, excluded from the project, or left compiled but unused.

Potential impact if wrong:
Choosing the wrong retirement strategy could either leave future-maintainer confusion in the codebase or remove files the user intended to preserve for reference.

Alternative interpretations considered:

1. Physically delete old Dunnage settings files.
2. Leave old files on disk but exclude them from runtime/DI.
3. Rewrite existing files in place despite the instruction not to reuse them.

## 3. Receiving Recreation Uses the Same Pattern as Dunnage

Assumption:
For `Module_Settings.Receiving`, I should apply the same full-recreation approach as Dunnage: define new categories from currently implemented settings, build new replacement pages and a new replacement navigation hub, and retire the current Receiving settings files from runtime use.

Why this assumption is needed:
The request says to do the “EXACT same thing” to Receiving after Dunnage, but it does not explicitly state whether the same non-reuse rule applies to files or whether only the category redesign should be mirrored.

Potential impact if wrong:
If the intent was to preserve existing Receiving files and only regroup cards, a full replacement would be broader than requested.

Alternative interpretations considered:

1. Full replacement with new files only, matching Dunnage.
2. Keep current Receiving files and only reorganize the nav cards.
3. Replace only the nav hub and keep the detail pages.

## 4. Category Design Should Be Based on Currently Implemented Settings Behavior, Not Current Page Names

Assumption:
The new Dunnage and Receiving category groupings should be derived from what settings are actually implemented and persisted today, even if those groupings differ from the current page names such as “Workflow”, “User Preferences”, “Validation”, or “Business Rules”.

Why this assumption is needed:
The request says to “go through all the settings pages, categorize all settings” and rebuild the modules, but it does not specify whether categories should reflect current page boundaries or actual feature/setting ownership.

Potential impact if wrong:
The rebuilt IA could feel arbitrary or could split settings differently than the user expects.

Alternative interpretations considered:

1. Preserve current page names as the category model.
2. Group by actual setting purpose and persisted behavior.
3. Group by user persona or workflow stage.

## 5. Existing Setting Keys and Persistence Contracts Must Be Preserved Unless a Setting Is Truly Dead

Assumption:
For Dunnage and Receiving, the rebuilt pages should continue using existing implemented setting keys, services, and persistence flows wherever the setting is still live, and only remove cards/pages for settings that are demonstrably unused or placeholder-only.

Why this assumption is needed:
The request asks for a full recreation, but changing setting keys or persistence contracts would create avoidable migration risk and could silently reset user/system configuration.

Potential impact if wrong:
If the user wanted a clean break with new keys and storage layout, preserving the old contracts would miss that intent. If preservation is required and I change keys, existing saved settings would be lost or orphaned.

Alternative interpretations considered:

1. Preserve all live keys and storage contracts.
2. Introduce a new settings schema and migrate values.
3. Reset unused settings entirely during rebuild.

## 6. “Remaining No Longer Used Cards” Means Runtime-Dead or Placeholder Navigation Entries

Assumption:
When removing “remaining no longer used cards” in Dunnage and Receiving, I should remove cards whose destination pages are placeholder-only, runtime-dead, or no longer backed by real save/load behavior, rather than removing settings merely because they are infrequently used.

Why this assumption is needed:
The phrase “no longer used” is ambiguous without a functional definition.

Potential impact if wrong:
Removing low-frequency but still-needed settings would be a behavioral regression.

Alternative interpretations considered:

1. Remove only dead or placeholder cards.
2. Remove rarely used cards based on inferred UX value.
3. Preserve all cards and only regroup them.

## 7. UI Pattern Baseline Should Match Current Core/Volvo Placeholder Card Language

Assumption:
The rebuilt navigation hubs and settings pages should follow the card-based settings design already used in `Module_Settings.Core` and the new Volvo placeholder page: Border cards, current typography resources, card stroke/fill brushes, and the current spacing conventions visible in the existing settings modules.

Why this assumption is needed:
The request says to adhere to currently implemented UI/UX design patterns, but there are multiple settings UIs in the repo and no single explicit visual baseline was named.

Potential impact if wrong:
If a different module is intended as the exact visual source of truth, the rebuilt pages may be stylistically inconsistent with the desired standard.

Alternative interpretations considered:

1. Follow current Core settings pages and the recent Volvo placeholder pattern.
2. Match Receiving workflow pages as the primary baseline.
3. Match MainWindow navigation-card styling instead of settings-page styling.

## 8. Search Destinations and MainWindow Settings Routing Should Be Updated Along With DI

Assumption:
Because the rebuilt pages will change type names and runtime entry points, I should also update `MainWindow.xaml.cs` settings search destinations and any settings-window routing references, not just DI and the workflow hub files.

Why this assumption is needed:
These modules are referenced outside their own folders, and leaving stale search destinations would create broken deep links.

Potential impact if wrong:
If the user wanted this scoped only to visible settings navigation, touching app-level destination maps would broaden the change.

Alternative interpretations considered:

1. Update all app-level routing references to the new settings pages.
2. Limit the change to module-local navigation only.

## Request For Confirmation

Please confirm, correct, or clarify the assumptions above before implementation continues.

The most important points to answer are:

1. For Reporting, do you want all current settings pages removed from runtime and replaced with one placeholder page, exactly like Volvo?
2. For Dunnage and Receiving, should old settings files remain on disk but be retired from runtime, or do you want them physically deleted?
3. Should the new Dunnage and Receiving categories be based on actual implemented settings behavior rather than today’s page names?
4. Should existing live setting keys/persistence contracts be preserved wherever possible?
