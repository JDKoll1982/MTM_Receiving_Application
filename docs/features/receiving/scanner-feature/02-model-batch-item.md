# Batch Item Model

Last Updated: 2026-07-21

This model defines one staged item and its exact send outcome.

## Model Purpose

- hold one record payload prepared for scanner-speed injection
- preserve per-item navigation and pacing directives
- retain immutable send results for partial-send safety

## Required Contents

- ItemId: unique item identity
- SessionId: parent session identity
- SequenceNumber: send order within the session
- ExternalRecordKey: optional ERP-visible reference for troubleshooting
- PayloadPartId: part identifier sent to target system
- PayloadFromWarehouse: source warehouse value resolved from settings/profile defaults
- PayloadFromLocation: source location value sent to target system
- PayloadToWarehouse: destination warehouse value resolved from settings/profile defaults
- PayloadToLocation: destination location value sent to target system
- PayloadQuantity: quantity field value as normalized string for send accuracy
- PayloadUnitOfMeasure: optional UOM value if target workflow requires it
- PayloadLotOrSerial: optional lot or serial field
- PayloadReferenceText: optional memo or tag text
- NavigationPattern: declarative field movement sequence for this item
- PreSendDelayMs: optional item-specific delay before first field
- DelayBetweenFieldsMs: optional delay between fields
- PostSendDelayMs: optional delay after item commit
- ValidationState: NotValidated, Valid, Invalid
- ValidationMessage: first blocking validation message
- ValidationNotes: user-facing summary shown in the Manage Items notes column
- FuzzyMatchedPartId: resolved Infor Visual part id when fuzzy lookup is used
- FuzzyMatchedFromLocation: resolved Infor Visual source location when fuzzy lookup is used
- FuzzyMatchedToLocation: resolved Infor Visual destination location when fuzzy lookup is used
- ExecutionState: Waiting, Sending, Sent, Failed, Skipped
- SentUtc: timestamp when item entered Sent state
- FailedUtc: timestamp when item entered Failed state
- IssueType: Validation, FocusLoss, Integrity, Timeout, AppClosed, Unknown
- IssueMessage: user-visible reason
- RetryCount: number of manual retry attempts
- LastAttemptUtc: timestamp of most recent send attempt
- IsLockedAfterSend: true once item is marked sent to prevent accidental mutation

## Behavioral Rules

- SequenceNumber must be stable until user reorders items explicitly
- warehouse values are displayed with each item but are not editable per item in the approved direction
- item payload becomes immutable after Sent unless user performs explicit unlock workflow
- if item fails, all following waiting items remain waiting
- retries are manual and never automatic for already-sent predecessors

## Validation Rules

- required payload fields are profile-dependent but must be validated before send
- when a new item is added, part id, from location, and to location should be checked against Infor Visual using the existing fuzzy-check patterns
- add-line validation should also verify that the source location has sufficient quantity on hand for the requested transfer quantity
- SequenceNumber must be unique within a session
- PayloadQuantity must pass profile-defined numeric constraints
- NavigationPattern cannot be empty for rows eligible to run

## Persistence And Mapping Notes

- store payload and state fields in one item table for read efficiency
- store failure metadata for post-run analysis and support
- preserve full row history through append-only run log entries in history service
- persist normalized from and to warehouse/location values so replay and review match what was actually sent
- persist latest validation state and notes so the Manage Items dialog can reopen with the same status context

## Use Or Modify Guidance

- create a dedicated scanner item model in Module_Scanner.Models
- keep row payload typed for business semantics in memory, but persist normalized send text snapshots for deterministic replay
- avoid introducing UI-only properties into this model; keep those in view-model projections
