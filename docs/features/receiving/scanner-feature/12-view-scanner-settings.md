# Scanner Settings View

Last Updated: 2026-07-21

This view allows user-scoped profile management and safety configuration for scanner execution.

## View Purpose

- provide user-scoped profile management
- enforce safe timing and keyboard shortcut configuration
- provide clear visibility into app window matching expectations
- manage fixed from and to warehouse variables used by scanner transfers

## Required Regions

- Profile list region: existing profiles with default marker
- Profile editor region: app settings, timing settings, keyboard shortcut settings
- Validation panel: blocking and advisory issues
- Test panel: app window match test results

## Textual Mockup

Left Sidebar:

- Profiles list
- New Profile
- Duplicate Profile
- Delete Profile

Main Editor:

- Profile Name
- Target Executable Name (`VMINVENT.exe`)
- App Window Title
- Target Child Screen Title (`Inventory Transfers`)
- Optional App Window Class
- Match Mode toggle
- From Warehouse Variable (default `002`)
- To Warehouse Variable (default `002`)
- Timing inputs for startup wait, delay between fields, pause after item
- Keyboard shortcut inputs for send and stop
- Optional advanced timing checkbox with warning banner

Bottom Actions:

- Save Profile
- Set As Default
- Safe Defaults
- Test App Window Match

## Interaction Notes

- Save disabled until validation passes
- conflicting keyboard shortcut values show immediate blocking feedback
- advanced timing always shows warning text and confirmation requirement
- from and to warehouse variables are settings-controlled and feed the workbench as read-only values on each item
- executable, child screen title, and optional class matching support Gupta/Centura target verification before any send begins

## Binding And UI Rules

- use x:Bind for all inputs and commands
- avoid direct settings persistence in view code-behind
- reflect final save outcomes via shared status mechanism

## Use Or Modify Guidance

Use existing:

- settings page layout conventions already used in Module_Settings.* pages
- shared keyboard shortcut display semantics from core helper guidance

Modify or extend:

- settings navigation to include scanner settings page
- scanner settings key map and settings core facade integration

Do not modify:

- existing module settings behavior unrelated to scanner profiles
