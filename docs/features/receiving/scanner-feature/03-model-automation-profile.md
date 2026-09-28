# Sending Profile Model

Last Updated: 2026-07-21

This model stores execution settings that define how items are validated, targeted, paced, and sent.

## Model Purpose

- parameterize runtime behavior without changing item payload
- support user-scoped defaults and safe profile reuse
- separate operational settings from business record data

## Required Contents

- ProfileId: unique profile identity
- OwnerUserId: profile owner user id for user-scoped persistence
- ProfileName: user-selected profile label
- IsDefaultForUser: indicates default profile selection behavior
- TargetExecutableName: expected process name, currently `VMINVENT.exe`
- AppWindowTitle: expected foreground window title or title fragment
- TargetChildWindowTitle: expected child screen title, currently `Inventory Transfers`
- AppWindowClass: optional class name requirement for stronger targeting
- TargetFrameworkFamily: target client framework descriptor, currently Gupta/Centura
- RequireExactTitleMatch: strict versus contains matching mode
- ActivateAppBeforeSend: whether focus acquisition is required before each send
- FromWarehouseDefault: source warehouse variable value, default `002`
- ToWarehouseDefault: destination warehouse variable value, default `002`
- AllowPerItemWarehouseOverride: false in approved direction
- ActivationDelayMs: delay before focus check after activation attempt
- PauseAfterItemMs: base delay between item sends
- DelayBetweenFieldsMs: base delay between field injections
- PopupTimeoutMs: maximum wait for expected popup appearance
- PopupCloseTimeoutMs: maximum wait for popup dismissal
- SendShortcutLabel: friendly text label for user instruction
- SendShortcutChord: configured send keyboard shortcut, default Ctrl+Alt+M
- StopBetweenSendsOnly: always true in baseline design
- AllowAdvancedTiming: optional safety feature flag, default false
- MaxItemsPerSend: optional guard to constrain long runs
- EnforceFocusEveryItem: true when advanced timing mode is enabled
- Notes: optional user notes
- CreatedUtc: creation timestamp
- LastUpdatedUtc: update timestamp

## Behavioral Rules

- profile changes do not mutate existing sent-item outcomes
- profile snapshot is captured into session at send start
- stop keyboard shortcut is honored between sends even if profile changes during send
- advanced timing mode is off by default and requires explicit opt-in and warning
- from and to warehouse defaults are settings-controlled and not edited on individual workbench items

## Validation Rules

- ProfileName required and unique per user
- timing values must remain inside safe bounds defined by settings policy
- send and stop keyboard shortcuts cannot be identical
- TargetWindowTitle required for any runnable profile
- TargetExecutableName and TargetChildWindowTitle required for any runnable profile in the current approved workflow

## Persistence And Mapping Notes

- persist per user through existing settings and DAO patterns
- keep profile metadata queryable for UI pickers and audit
- optionally mirror critical values into settings facade for quick defaults
- include target executable, child screen title, optional class match, and warehouse defaults in the persisted profile snapshot

## Use Or Modify Guidance

- create scanner-specific profile model in Module_Scanner.Models
- use existing settings subsystem for ownership and default selection behavior
- extend settings keys and stored procedures rather than creating a parallel configuration store
