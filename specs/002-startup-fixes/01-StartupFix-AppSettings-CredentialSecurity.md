# 01-StartupFix-AppSettings-CredentialSecurity

## Copy/Paste Prompt

You are a senior C# / WinUI 3 / MVVM engineer working in `MTM_Receiving_Application`.
Implement only this fix slice. Keep the change set focused, complete it end-to-end, and stop
only after the targeted validation passes.

## Feature

Remove plaintext credentials from source-controlled appsettings.json.

## Implementation Order

**Order:** 01

## Why This Runs First

This is a security fix. Plaintext database credentials are currently committed in `appsettings.json`
and visible to anyone with repository access. All other startup improvements are lower priority than
removing credential exposure from source control. This slice must complete before any other startup
changes ship.

## Confirmed Decisions

- `appsettings.json` must never contain real credentials in any committed form.
- `appsettings.Development.json` is gitignored and is the correct home for local development credentials.
- The committed `appsettings.json` must retain placeholder values so the configuration shape is
  discoverable without exposing real values.
- The `.gitignore` must explicitly list `appsettings.Development.json` if it does not already.
- Do not change any connection string consumption logic — only the storage location of the values.

## Current Repo State

- `appsettings.json` lines 17–20 contain plaintext credentials for both databases:
  - `"MySql": "Server=172.16.1.104;...Uid=root;Pwd=root;..."`
  - `"InforVisual": "...User Id=SHOP2;Password=SHOP;..."`
- `appsettings.json` is tracked by git and not listed in `.gitignore`.
- `appsettings.Development.json` exists in the repo root and is intended for dev-only overrides.
- `Helper_Database_Variables` reads connection strings from the configuration system.
- `Infrastructure/Logging/SerilogConfiguration.cs` may also read from `appsettings.json`.

## Required Outcome

The committed `appsettings.json` must contain only placeholder values for all connection string
credentials. Real credentials must live in `appsettings.Development.json` only, which must be
gitignored. The application must start normally in development using the Development override file.

## Primary Change Areas

- Replace real credential values in `appsettings.json` with safe placeholder strings.
- Ensure real credentials exist in `appsettings.Development.json` for local development.
- Verify `appsettings.Development.json` is listed in `.gitignore`.
- Verify the application reads configuration in the correct override order so Development values
  take precedence over base values in development environments.

## Files That Must Change

- `appsettings.json`
- `appsettings.Development.json`
- `.gitignore` (add entry if `appsettings.Development.json` is not already listed)

## Recommended Target Shape

Committed `appsettings.json` connection string entries should use safe placeholder tokens:

```json
"AppSettings": {
  "MySql": "#{MYSQL_CONNECTION_STRING}#",
  "InforVisual": "#{INFORVISUAL_CONNECTION_STRING}#"
}
```

`appsettings.Development.json` should contain the real local development values and must not be
committed. The `#{}#` placeholder pattern signals clearly that environment-specific substitution
is required before this config is usable.

## Implementation Steps

1. Open `appsettings.json` and locate the `MySql` and `InforVisual` connection string values.
2. Replace the real credential-bearing values with placeholder tokens such as
   `#{MYSQL_CONNECTION_STRING}#` and `#{INFORVISUAL_CONNECTION_STRING}#`.
3. Open `appsettings.Development.json` and confirm that the real development connection strings
   are present there. Add them if they are missing.
4. Open `.gitignore` at the repository root. Add `appsettings.Development.json` if it is not
   already listed. Do not add `appsettings.json`.
5. Build the solution to confirm the credential removal does not break compilation.
6. Verify locally that starting the application in Debug mode uses the Development file and
   connects successfully to both databases.
7. Run `git log --all -S "Pwd=root"` and `git log --all -S "Password=SHOP"` to confirm the
   credentials are not already embedded in git history. If they are, document the rotation
   requirement separately.

## Task Checklist

- [ ] Replace MySql plaintext credentials in `appsettings.json` with placeholder token.
- [ ] Replace InforVisual plaintext credentials in `appsettings.json` with placeholder token.
- [ ] Confirm `appsettings.Development.json` contains the real development credentials.
- [ ] Add `appsettings.Development.json` to `.gitignore` if not already present.
- [ ] Build solution — zero errors.
- [ ] Start application locally — both database connections succeed.
- [ ] Check git history for credential presence and note rotation requirement if found.

## Validation

- `appsettings.json` must contain no passwords, UIDs, or usernames in any connection string value.
- `appsettings.Development.json` must be listed in `.gitignore`.
- `dotnet build` must succeed with no errors.
- The application must start normally in a local development run.
- `git status` must not show `appsettings.Development.json` as a tracked or staged file after
  the `.gitignore` change is committed.

## Guardrails

- Do not change any code that reads connection strings — only the config file values.
- Do not commit real credentials in any form, including in comments or example files.
- Do not remove the `appsettings.Development.json` file — it is needed for local development.
- Do not introduce a secrets manager or environment variable system in this slice; that is a
  separate architectural concern. This slice's goal is simply to stop the immediate credential
  exposure in source control.

## Completion Criteria

- No committed file in the repository contains a plaintext database password or username.
- `appsettings.Development.json` is gitignored.
- The application builds and starts normally in development.
- The fix is verifiable by inspecting `appsettings.json` directly in the repository.
