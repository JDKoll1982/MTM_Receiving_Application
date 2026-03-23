## Assumptions Requiring Confirmation

1. The request means the application should automatically create the `MTM_AUTH_USER_SECRET_KEY` value the first time the app runs on a PC if it does not already exist.

Why this assumption is needed:
The request says "when the app first runs on a pc i want the app to create this," and the screenshot shows the missing environment variable error. That strongly suggests "this" means the missing secret key.

Potential impact if wrong:
If you meant a different value or a different storage location, the startup behavior would be implemented in the wrong place.

Alternative interpretations considered:

- You want the app to create the environment variable automatically.
- You want the app to create a local key file instead of an environment variable.
- You want IT deployment to create the key, and the app should only guide the user when it is missing.

2. The app should generate a machine-specific random secret on first run and save it locally on that PC.

Why this assumption is needed:
If the app is the thing creating the secret automatically, it needs to know whether that secret should be unique per PC or shared across all PCs.

Potential impact if wrong:
This is the most important risk. If each PC generates its own secret, data encrypted on PC A may not be readable on PC B. That would break cross-machine use of stored Visual credentials and PIN validation assumptions.

Alternative interpretations considered:

- Generate a unique secret on each PC automatically.
- Require one shared secret for all PCs and provision it outside the app.
- Store a shared secret centrally in the database or another managed secret store.

3. The app should save the secret in the Windows machine-level environment variable store rather than a user-level variable or a local configuration file.

Why this assumption is needed:
The current implementation explicitly reads an environment variable, so I need to know whether you want to preserve that storage approach or switch to something else.

Potential impact if wrong:
Saving it in the wrong scope could make the app work for one Windows user account but fail for another, or could create permission and support issues.

Alternative interpretations considered:

- Machine-level environment variable.
- User-level environment variable.
- Protected local config file.
- Windows credential or secret storage.

4. If the app is allowed to auto-create the secret, it should do so silently during first-run startup instead of stopping and asking the user or IT for a value.

Why this assumption is needed:
The user experience is very different depending on whether startup is silent, guided, or blocked pending admin action.

Potential impact if wrong:
The app could either be too disruptive at startup or too silent for an operation that should be controlled by IT.

Alternative interpretations considered:

- Silent auto-create.
- Prompt the user with instructions.
- Block startup until an admin-configured secret exists.

## Explicit Confirmation Needed Before Work Continues

Please confirm or correct these assumptions before I implement anything.

The key design question is this:

- Do you want one shared secret across all PCs, or a different auto-generated secret on each PC?

That choice changes whether encrypted auth data remains readable across machines.

## Additional Implementation Assumption Used For The Deployment Script

5. The deployment script may use configurable default file locations for the secret source when an exact path was not specified.

Why this assumption is needed:
The latest request defined local-versus-shared behavior, but it still did not give an exact local file path or exact shared UNC path for the key file.

Potential impact if wrong:
The script could create or read the secret from a location that does not match your normal deployment workflow.

Alternative interpretations considered:

- Hard-code one exact local path and one exact shared path.
- Stop and ask for the exact paths before implementing anything.
- Make both paths configurable in the script with sensible defaults.

Implementation choice:
Use configurable defaults in the deployment script so the behavior works immediately but can still be adjusted without changing the rest of the logic.
