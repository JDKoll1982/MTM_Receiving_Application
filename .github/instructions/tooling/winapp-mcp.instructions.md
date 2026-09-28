---
description: "Guidance for using WinApp MCP to launch, inspect, and automate WinUI and other Windows desktop applications when the user asks for UI interaction, verification, or desktop E2E workflows."
applyTo: "**"
---

<!-- 
[DOC-META-START]
- File Name: winapp-mcp.instructions.md
- Description: Guidance for using WinApp MCP to launch, inspect, and automate WinUI and other Windows desktop applications when the user asks for UI interaction, verification, or desktop E2E workflows.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-9: # WinApp MCP Usage Guide
  - Line 10-25: ## Purpose
  - Line 26-35: ## Prerequisites
  - Line 36-63: ## Installation And Configuration
  - Line 64-76: ## Default Operating Pattern
  - Line 77-114: ## Tooling Strategy
  - Line 115-139: ## WinUI-Specific Guidance
  - Line 140-152: ## Reliability Rules
  - Line 191-200: ## Safety Rules
  - Line 201-239: ## Recommended Workflows
  - Line 240-257: ## Decision Guidance
[DOC-META-END]
-->

# WinApp MCP Usage Guide

Use this guidance when the user asks to interact with, inspect, validate, test, or automate a running Windows desktop application through the WinApp MCP server.

## Purpose

WinApp MCP exposes Windows UI Automation tools to MCP-compatible assistants so they can work with compiled desktop applications, not just source code.

Use WinApp MCP for tasks such as:

- Launching or attaching to a running WinUI 3, WPF, WinForms, UWP, or Win32 app
- Inspecting the live UI tree and reading control state
- Clicking buttons, filling forms, choosing dropdown values, and sending key input
- Verifying dialogs, multi-window flows, and visual behavior
- Capturing screenshots and comparing UI before and after a change
- Running desktop E2E or smoke-test style validation after code changes
- Auditing AutomationId coverage and general UI accessibility exposure

Do not use WinApp MCP when the task is purely source-code editing, architecture analysis, or non-UI business logic work.

## Prerequisites

Before using WinApp MCP, confirm these constraints:

- Windows 10 version 1903+ or Windows 11
- Target application exposes a usable UI Automation tree
- If the target app runs elevated, the MCP server must also run elevated
- Only one active hands-on automation session should run at a time
- Sensitive applications should be closed before automation starts

## Installation And Configuration

### Preferred Runtime Option

Use the npm package:

```json
{
  "servers": {
    "winapp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "winapp-mcp"]
    }
  }
}
```

This is appropriate for `.vscode/mcp.json` when using VS Code / GitHub Copilot.

### VS Code Extension Option

If the user prefers VS Code integration, the WinApp MCP VSIX or marketplace extension can be installed instead. The extension auto-registers the server.

### Source Build Option

If npm or the extension is not suitable, the server can be built from source with .NET 10 and run directly as a stdio MCP server.

## Default Operating Pattern

When a user asks to use the WinApp MCP server, follow this sequence:

1. Confirm or establish that WinApp MCP is configured for the client.
2. Launch the target app or attach to an existing process.
3. Wait for input idle or the first expected element.
4. Inspect the UI tree before interacting.
5. Prefer stable selectors and targeted actions over coordinate clicks.
6. Synchronize after each UI-changing action.
7. Capture evidence when validation matters.
8. Use recovery tools if the session becomes stuck.

## Tooling Strategy

### Start With Discovery

Prefer discovery tools before action tools:

- `launch_app` or `attach_to_app` / `attach_to_pid`
- `wait_for_input_idle`
- `get_snapshot`
- `find_elements` or `find_all_elements`
- `read_element`

Treat `get_snapshot` as the desktop equivalent of DOM inspection. Use it first to understand structure, AutomationIds, names, and control types.

### Prefer Stable Element Resolution

When locating elements, use this priority:

1. `automationId`
2. `name`
3. `controlType`
4. `index` when multiple matches exist

Prefer `AutomationId` whenever available. It is the most stable selector across layout changes and localization.

### Prefer Targeted Actions Over Fragile Ones

Use these patterns:

- `fill_form` instead of many `type_text` calls
- `select_option` instead of click-expand-click for ComboBox flows
- `invoke_element` when normal clicking is unreliable, especially in dialogs
- `wait_for_element` or `wait_for_condition` after navigation or mutations
- `element_exists` for quick presence checks
- `get_all_values` to verify a form after filling it

Use `click_at_coordinates` only as a fallback when UIA element targeting is not viable.

## WinUI-Specific Guidance

WinUI applications need extra care because virtualization and popup behavior can hide elements from the immediate UIA tree.

### Virtualized Lists

For `ListView`, `GridView`, `ItemsRepeater`, and similar virtualized surfaces:

- Use `realize_virtualized_item` before reading or clicking off-screen list items
- Use `scroll_into_view` when the item exists but is not visible
- Use `find_item_by_property` for large or virtualized containers

### ContentDialogs And Secondary Windows

For dialogs, popups, and separate windows:

- Use `list_desktop_windows` to discover HWND values
- Use `get_snapshot_hwnd` to inspect the specific window
- Use `click_element_hwnd` or `set_value_hwnd` when the dialog is not behaving like the main app window
- Prefer HWND-targeted tools for ContentDialogs and system dialogs when normal app-scoped tools are unreliable

### Automation Properties

If a WinUI control cannot be found reliably, inspect whether the application exposes `AutomationProperties.AutomationId`. WinUI controls may not have useful AutomationIds unless they were explicitly set in XAML.

## Reliability Rules

Always apply these practices:

- Wait after app launch with `wait_for_input_idle`
- Wait after navigation with `wait_for_element`
- Wait after async state changes with `wait_for_condition`
- Use `invalidate_cache` if the UI changed without a tracked mutation
- Use `find_all_elements` when multiple matches may exist
- Use `get_tree_hash` to verify whether the UI actually changed after an action

Do not assume the UI is ready immediately after a click or navigation event.

## Screenshots And Visual Verification

Use WinApp MCP visual tools when the user asks for proof, UI validation, or regression comparison:

- `take_screenshot`
- `take_screenshot_optimized`
- `annotate_screenshot`
- `screenshot_diff`

Prefer `take_screenshot_optimized` when the screenshot will be consumed by an LLM and token budget matters.

Use `annotate_screenshot` when you need to confirm that the selected elements match the expected on-screen targets.

## Event-Driven Debugging

When the UI changes asynchronously or unpredictably:

- Start `start_event_monitor`
- Perform the action
- Use `wait_for_condition` for the expected UI state
- Read `get_event_log`
- Stop monitoring with `stop_event_monitor`

Use this flow for debugging delayed loads, background updates, or animation-driven UI changes.

## Locked And Minimized Sessions

WinApp MCP can continue to function in limited scenarios when the app is minimized or the desktop session is locked.

Use session-aware behavior:

- Check state with `check_session_status`
- Restore minimized windows with `restore_window` when needed
- Prefer UIA-pattern-based operations when the session is locked
- Expect raw mouse or keyboard simulation to be less reliable in locked-session mode

If the user wants unattended or long-running desktop automation, favor pattern-based operations over coordinate or raw input automation.

## Safety Rules

This server can control the live desktop. Follow these rules every time:

- Warn the user not to interfere with mouse or keyboard while automation is running
- Avoid automating with unrelated sensitive applications visible
- Use one automation session at a time to avoid conflicts
- Use `release_all` if modifier keys or mouse buttons become stuck
- Respect timeouts and fail safely instead of retrying blindly

## Recommended Workflows

### Launch And Inspect

Use this pattern when the user asks to inspect a desktop app:

1. `launch_app`
2. `wait_for_input_idle`
3. `get_snapshot`
4. `find_elements` or `read_element`

### Navigate And Fill A Form

Use this pattern when the user asks to create or edit records:

1. Attach or launch the app
2. Navigate with `click_element` or `invoke_element`
3. Wait for the form
4. Use `fill_form`
5. Verify with `get_all_values`
6. Save and wait for confirmation

### Dialog Handling

Use this pattern for save dialogs, confirmations, and ContentDialogs:

1. `list_desktop_windows`
2. `get_snapshot_hwnd`
3. `set_value_hwnd` or `click_element_hwnd`

### Visual Regression

Use this pattern when the user asks for before/after UI comparison:

1. Take baseline screenshot
2. Perform the change or workflow
3. Take follow-up screenshot
4. Run `screenshot_diff`

## Decision Guidance

Use WinApp MCP when the user asks things like:

- "Launch the app and verify the page loads"
- "Click through this WinUI workflow"
- "Fill this form in the running app"
- "Open the dialog and tell me what controls are there"
- "Take a screenshot after saving"
- "Run a desktop smoke test"
- "Check whether the app exposes AutomationIds correctly"

Do not default to WinApp MCP for tasks like:

- Pure code refactoring with no runtime validation requested
- Non-Windows UI targets
- Applications that do not expose a usable UI Automation tree

## Agent Behavior Expectations

When using WinApp MCP in response to a user request:

- Explain briefly what part of the UI workflow you are about to perform
- Prefer deterministic selectors and resilient waits
- Avoid unnecessary full-tree snapshots once the relevant selectors are known
- Capture evidence for important state transitions
- Escalate clearly if the app lacks usable UIA exposure or requires elevation

## External References

Primary source materials:

- `https://github.com/floatingbrij/desktop-pilot-mcp/blob/main/README.md`
- `https://github.com/floatingbrij/desktop-pilot-mcp/blob/main/DOCUMENTATION.md`
