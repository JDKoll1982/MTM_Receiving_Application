# Batch Session Model

Last Updated: 2026-08-24

This model is the root aggregate for one scanner context. It represents ownership, lifecycle state, and the ordered set of items.

## Model Purpose

- represent one user-owned scanner draft or active send
- track send-level progress and stop reasons
- provide the parent identity for items and send history

## Required Contents

- SessionId: unique identifier for the session record
- OwnerUserId: current MTM user identity that owns the draft
- OwnerDisplayName: friendly user name for UI lists and audit visibility
- CreatedUtc: initial creation timestamp
- LastUpdatedUtc: last mutation timestamp
- Status: Draft, Ready, Running, Stopped, Completed, Failed, Archived
- ActiveProfileId: selected sending profile used at send time
- AppWindowTitleSnapshot: resolved title captured before first send
- AppWindowClassSnapshot: optional class name snapshot for stronger verification
- TotalItems: count of items
- SentItems: count of successfully sent items
- FailedItems: count of failed items
- WaitingItems: count of items not yet sent
- StopRequested: persisted from earlier sessions; no longer raised by the UI since Send All / Stop After This were removed (2026-08-24), still honored by SendNextItemAsync when present
- StopReason: UserStop, ValidationFailure, AppNotInFocus, IntegrityBlock, Unknown
- LastSendStartedUtc: when current or last send started
- LastSendEndedUtc: when current or last send ended
- LastFailureMessage: latest user-facing failure summary
- Notes: optional user notes for handoff between shifts
- Items: ordered collection of Batch Item records

## Behavioral Rules

- item order is authoritative and must be preserved
- status cannot move directly from Draft to Completed without Running
- stop request does not interrupt active item send mid-cycle
- sent counts must never decrease once marked sent
- a failed item freezes subsequent waiting items until user action

## Validation Rules

- OwnerUserId required for every persisted session
- ActiveProfileId required before transition to Ready or Running
- TotalItems must match item collection count
- SentItems plus FailedItems plus WaitingItems must equal TotalItems

## Persistence And Mapping Notes

- map 1-to-many with Batch Item records by SessionId
- persist all lifecycle counters to support resume and audit
- persist snapshots of target window identity to support replay diagnostics

## Use Or Modify Guidance

- create new scanner-specific model in Module_Scanner.Models
- do not overload generic shared models with scanner-only fields
- if a generic execution-status enum already exists in shared layers, reuse only if it preserves scanner stop semantics without compromise
