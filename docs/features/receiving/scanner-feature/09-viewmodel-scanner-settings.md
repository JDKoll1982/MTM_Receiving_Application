# Scanner Settings ViewModel

Last Updated: 2026-07-28

This view-model manages sending profile editing, keyboard shortcut policy, and safety defaults.

## ViewModel Purpose

- create and edit sending profiles
- enforce keyboard shortcut and timing validation
- persist user-scoped defaults

## Current Implementation State

- Profiles collection
- SelectedProfile
- EditableProfileDraft
- IsDirty
- ValidationErrors collection
- CanSaveProfile
- CanDeleteProfile
- IsTestingTargetWindow
- LastTargetTestResult

## Required State

- Profiles collection
- SelectedProfile
- EditableProfileDraft
- IsDirty
- ValidationErrors collection
- CanSaveProfile
- CanDeleteProfile
- IsTestingTargetWindow
- LastTargetTestResult

## Required Commands

- LoadProfiles
- CreateProfile
- DuplicateProfile
- SaveProfile
- DeleteProfile
- SetAsDefaultProfile
- TestTargetWindowMatch
- ResetTimingDefaults

## Validation Responsibilities

- ensure send and stop shortcuts are distinct
- ensure timing values remain within configured safe limits
- require target window title before save
- enforce unique profile name per user

## Integration Responsibilities

- use settings and persistence services for user-scoped profile storage
- publish keyboard shortcut display values for user reference
- provide warnings when advanced timing options are enabled

## Use Or Modify Guidance

Use existing:

- shared base view-model dependencies for status and errors
- keyboard shortcut helper behavior model concepts from Module_Core
- settings core facade patterns for user-scoped settings reads and writes

Modify or extend:

- settings navigation path to include scanner profile configuration surface
- settings key inventory to include scanner profile defaults

Do not modify:

- global shared shortcut behavior in unrelated modules without explicit cross-module approval
