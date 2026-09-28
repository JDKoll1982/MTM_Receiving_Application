# Scanner History ViewModel

Last Updated: 2026-07-28

This view-model provides searchable history of previous sends and item-level results after execution.

## ViewModel Purpose

- display previous sends with status and timing summaries
- expose filtering by date, status, and operator
- provide drill-down into sent, failed, and waiting item results

## Current Implementation State

- Runs collection
- SelectedRun
- SelectedRunItems collection
- DateFromUtc and DateToUtc filters
- StatusFilter
- OwnerUserId
- MaxResults
- RefreshHistoryAsync and ClearFilters

## Required State

- SendHistory collection
- SelectedSend
- SelectedSendItems collection
- DateFrom and DateTo filters
- StatusFilter
- UserFilter
- IsLoadingHistory
- LoadErrorMessage
- IsDetailsVisible

## Required Commands

- LoadRecentHistory
- ApplyFilters
- ClearFilters
- OpenSendDetails
- ExportSendSummary
- ReloadSelectedSendItems

## Data Responsibilities

- call persistence/history service for paged history retrieval
- load item-level details on demand to reduce initial page load
- preserve immutable historical values and failure reasons
- preserve from and to warehouse/location values so retry decisions can be made from the history screen without reopening the original batch

## UX Responsibilities

- highlight incomplete sends
- show which items were sent before a stop or failure
- show clear retry guidance without auto-modifying historical records
- show the exact source and destination warehouse/location values that were used when the item was sent or failed

## Use Or Modify Guidance

Use existing:

- ViewModel_Shared_Base status and notification plumbing
- shared logging and error handling services

Modify or extend:

- receiving module menu or tab host to surface history page
- persistence query services for efficient filtering and detail retrieval

Do not modify:

- active session scanner state from history context
