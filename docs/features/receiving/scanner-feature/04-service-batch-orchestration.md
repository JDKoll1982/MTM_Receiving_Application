# Batch Orchestration Service

Last Updated: 2026-07-21

This service is the execution coordinator for scanner sends. It owns validation, item ordering, state transitions, and stop-on-failure behavior.

## Service Purpose

- accept send requests from scanner view-models
- validate session readiness and item integrity
- coordinate focus acquisition and native input emission per item
- enforce stop-on-first-failure with partial-send preservation
- publish progress and final send summaries

## Inputs

- session aggregate with ordered items
- selected sending profile snapshot
- send mode: send next item or send all items
- cancellation or stop signals from keyboard shortcut or UI command

## Outputs

- per-item execution results with timestamps
- updated session status and counters
- send summary object for UI and history persistence
- user-facing warnings when environment constraints block execution

## Required Responsibilities

- transition session states through Ready, Running, Stopped, Completed, Failed
- verify `VMINVENT.exe` and the intended `Inventory Transfers` child screen are in focus before each item send
- coordinate pre-send item validation results produced during item entry so invalid items remain visible but do not enter the send pipeline
- invoke native input service with item payload and navigation pattern
- apply configured pause delays between sends
- stop immediately after first failed item
- keep already-sent items immutable and visible
- maintain waiting state for unsent items after failure or stop
- queue persistence updates asynchronously
- never auto-finalize or auto-save inside the ERP; the user manually validates the entered data and performs save/commit actions themselves

## Failure Handling Policy

- validation failures prevent send start
- focus or integrity failures fail current item and stop further sending
- persistence failures are logged and surfaced without corrupting in-memory state
- stop requests are honored between item sends only
- completion of an input burst does not imply an ERP save; orchestration stops at data entry and returns control to the user

## Concurrency And Threading

- one active send per session
- synchronize state mutations to prevent duplicate item execution
- publish UI-safe progress via dispatcher abstraction

## Use Or Modify Guidance

Use existing:

- IService_Dispatcher for UI-safe callback publication
- IService_ErrorHandler for standardized user and log error handling
- IService_LoggingUtility for execution diagnostics
- IService_Notification for status messaging

Modify or extend:

- scanner module service registration to add scanner orchestration service
- any scanner workflow coordinator to route item-entry validation and send commands into this service without bypassing MVVM

Do not modify:

- generic shared base view-model logic for scanner-specific state transitions
- core UI automation service with scanner business rules
