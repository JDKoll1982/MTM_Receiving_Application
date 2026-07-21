# Connected Services: Module Core

Last Updated: 2026-07-21

This document defines Module_Core contracts and services that should be reused or extended for scanner functionality delivery.

## Reuse As-Is

- IService_Dispatcher and Service_Dispatcher
Reason: safe UI-thread updates for progress and state transitions.

- IService_ErrorHandler and Service_ErrorHandler
Reason: consistent user-facing and log-facing error pathways.

- IService_LoggingUtility and Service_LoggingUtility
Reason: execution diagnostics, send traceability, and failure triage.

- IService_Notification and Service_Notification
Reason: standardized status broadcast and action-capable notifications.

- IService_Window and Service_Window
Reason: XamlRoot access and application window routing support.

- IService_Focus and Service_Focus
Reason: view focus ergonomics for high-throughput user input screens.

## Reuse With Extension

- IService_UIAutomation and Service_UIAutomation
Extension intent: add only generic capabilities such as stricter app focus verification or richer popup wait helpers.
Boundary: do not add scanner workflow policy, item retry workflow, or send states to this core service.

## Related Core Helpers To Reference

- Helper_KeyboardShortcuts
Reason: align user-facing keyboard shortcut formatting and modifier semantics when exposing scanner send and stop keys.

- Window helper utilities
Reason: maintain consistent window sizing and startup behavior for any scanner-specific window or dialog surface.

## Dependency Injection Touchpoints

- update core registration consumers in Infrastructure/DependencyInjection to register scanner services in Module_Scanner while preserving existing core singleton and transient lifetimes
- avoid registering scanner services directly in App.xaml.cs

## Not Recommended For Core Changes

- no scanner-specific state models in Module_Core models
- no scanner-specific command orchestration in core behaviors
- no module-specific keyboard shortcut command policy embedded directly in global core services
