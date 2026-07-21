# Native Input Engine Service

Last Updated: 2026-07-21

This service encapsulates Win32 and UI Automation operations required to inject scanner-speed item input into the target app window safely.

The current approved direction assumes FlaUI as the primary UI Automation layer for target discovery,
child-window verification, and popup handling, while still allowing direct `SendInput` interop or a
small helper wrapper when FlaUI does not cover raw input emission needs cleanly.

## Service Purpose

- isolate all native input and app window targeting behavior
- provide deterministic item sends from normalized payload instructions
- enforce desktop safety checks before injection

## Inputs

- app window identity constraints from profile, including `VMINVENT.exe`, `Inventory Transfers`, and optional class hints for Gupta/Centura targets
- normalized item payload and navigation semantics
- timing configuration values
- send mode and stop coordination context

## Outputs

- execution result per item with detailed issue type
- focus verification evidence used by orchestration logs
- optional popup handling results when detected

## Required Responsibilities

- resolve app window handle by executable name, title, optional class, and expected child screen title
- verify app window is in focus before each send
- construct and emit ordered input events for item payload and navigation
- apply delay between fields and post-send pause
- detect and classify failures such as integrity restriction, focus loss, timeout
- expose bounded popup wait and dismissal operations when needed
- use FlaUI first for window discovery and child-screen verification; supplement with direct Win32 or another narrow helper only where FlaUI does not provide the required low-level behavior

## Safety And Integrity Rules

- never inject when app verification fails
- never auto-retry already-sent items
- never interrupt an active item mid-send for stop requests
- keep advanced timing mode disabled by default
- if advanced timing mode is introduced, enforce strict timeout and recovery controls

## Use Or Modify Guidance

Use existing:

- IService_UIAutomation as the baseline native automation abstraction
- IService_Window where WinUI window-root context is required
- IService_LoggingUtility for deep native diagnostics

Modify or extend:

- IService_UIAutomation and Service_UIAutomation only for generic capabilities that benefit other modules, such as stronger focus verification helpers
- add a scanner-focused adapter service in Module_Scanner.Services that uses FlaUI for target resolution and popup handling, instead of overloading the generic core service
- review InputSimulatorCore only as a convenience wrapper if direct `SendInput` boilerplate becomes excessive; do not let it replace MTM-owned pacing and safety logic

Do not modify:

- core service to include scanner workflow states, item retry policy, or business validation
