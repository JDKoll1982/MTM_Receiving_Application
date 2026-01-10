# Module Shared — Settable Objects Report

This report lists **settable objects** (configuration, tunables, and hardcoded values that should be configurable) discovered in `Module_Shared`.

## Settable objects

## Security

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `SharedTerminalLogin:MaxAttempts` | int | System | NumberBox | Maximum failed PIN attempts before lockout (currently 3). | Admin (Security) | `View_Shared_SharedTerminalLoginDialog.xaml.cs` | Externalize to security policy config; consider per-workstation overrides. |
| `SharedTerminalLogin:PinDigits` | int | System | NumberBox | PIN length expectation (documented as 4-digit). | Admin (Security) | `View_Shared_SharedTerminalLoginDialog.xaml.cs` (docstring) | Ensure enforced consistently; externalize and validate. |
| `SharedTerminalLogin:LockoutDelayMs` | int | System | NumberBox | Delay before closing dialog after lockout (currently 5000ms). | Admin (Security) | `View_Shared_SharedTerminalLoginDialog.xaml.cs` | Externalize to policy; consider immediate close on high-security terminals. |

## UI/UX

| Settable object | Type | Scope (User/System) | Recommended UI control | Description | Recommended permission to set | Current source | Recommendation |
|---|---:|---|---|---|---|---|---|
| `NewUserSetup:SuccessMessageDelayMs` | int | System | NumberBox | Delay before closing after successful account creation (currently 2000ms). | Admin (UI) | `View_Shared_NewUserSetupDialog.xaml.cs` | Externalize if training scenarios need longer visibility. |

## Hardcoded values that should not be hardcoded (high priority)

- Login attempt limits and lockout behavior should be centrally configurable (security policy).
