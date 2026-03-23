# Assumptions Requiring Confirmation

Date: 2026-03-22 06:27 PM
Feature: Update User Creation
Scope: New User Setup UI + credential storage behavior

1. Assumption: The new user setup dialog should replace the single `Full Name` input with separate `First Name` and `Last Name` inputs, and the application should compose `FullName` as title case before saving.
   Why this assumption is needed: The request specifies separate first and last name inputs and examples like `john` + `koll` becoming `John Koll`, but it does not define whether middle names, suffixes, apostrophes, hyphenated names, or multi-word last names need special handling.
   Potential impact if wrong: A too-simple formatter could incorrectly transform valid employee names or make later editing inconsistent with other user-management screens.
   Alternative interpretations considered:

- Preserve user-entered casing and only split the fields visually.
- Apply simple title casing only to basic alphabetic inputs.
- Introduce a more advanced name parser for punctuation and multi-word names.
  User confirmation received: Proceed with option 1 from the follow-up package discussion, meaning keep this dependency-free and implement a small in-repo formatter instead of adding a NuGet package.
  Requested confirmation: Confirm whether the small dependency-free formatter should handle only the basic examples provided now, or whether specific edge cases must also be included in this change.

2. Assumption: `PIN` should be protected at rest with one-way server-side hashing, not reversible encryption.
   Why this assumption is needed: The request says the user's PIN needs to be encrypted server side, but PIN authentication currently works by direct equality comparison in `sp_Auth_User_ValidatePin`, which implies raw storage today. For authentication, hashing is the safer design, but it changes the database validation contract.
   Potential impact if wrong: Choosing hashing changes login validation, user update flows, and any admin editing flow that currently expects the raw PIN value to round-trip. Choosing reversible encryption instead would preserve round-tripping but would weaken protection for the login credential.
   Alternative interpretations considered:

- Hash PIN values in MySQL procedures and compare hashes during login.
- Encrypt PIN values reversibly in MySQL and decrypt or compare after transformation.
- Leave PIN storage unchanged and encrypt only Visual credentials.
  User confirmation received: Use one-way protection for PIN. No migration for prior users is required because the database is flushed on each deployment until release.
  Revised implementation note: PIN hashing will be applied in the app layer before MySQL writes, and authentication will compare against the hashed value.

3. Assumption: `VisualUsername` and `VisualPassword` should be encrypted at rest in MySQL in a reversible way so the app can continue to use them for ERP integration.
   Why this assumption is needed: The app currently reads and edits these values in multiple places, including new-user setup and the settings users screen. ERP credentials typically need reversible protection, but the request does not define how MySQL should manage encryption keys or whether existing rows must be migrated.
   Additional investigation result: Workstations are persisted in MySQL through `auth_workstation_config` and `sp_Auth_Workstation_Upsert`, but this stores only workstation name, workstation type, active flag, and description. It is used for startup classification and does not provide a reusable shared secret or encryption key source.
   Potential impact if wrong: Picking the wrong key-management approach could make existing credentials unreadable, break ERP access, or require a wider deployment plan than a scoped feature fix. The workstation table does not remove the need for an explicit shared key source.
   Alternative interpretations considered:

- Perform reversible encryption in the application layer using the existing `ISettingsEncryptionService` and store ciphertext in MySQL.
- Perform reversible encryption inside MySQL stored procedures using a server-managed key.
- Encrypt only the password and leave the Visual username in plain text.
  User confirmation received: Use application-layer reversible encryption before MySQL. No migration for prior users is required because the database is flushed on each deployment until release.
  Revised conclusion after workstation review: application-layer encryption remains the correct direction, but it still requires an explicit shared key source because workstation metadata in MySQL is not a secret.
  User confirmation received: Use an environment variable on each client PC as the shared key source for reversible Visual credential encryption.

4. Assumption: This change should also update the settings user-management path, not only the first-run new user setup dialog.
   Why this assumption is needed: The same raw Visual credential storage is used in `Dao_User.UpdateAsync`, `Dao_User.UpdateVisualCredentialsAsync`, and `ViewModel_Settings_Users`. Updating only first-run setup would leave the same security gap elsewhere.
   Potential impact if wrong: A partial fix would remove the warning in the new user dialog while leaving credentials exposed when edited through settings, creating inconsistent behavior and a misleading security posture.
   Alternative interpretations considered:

- Limit the change strictly to first-run new user creation.
- Update all credential write paths in this repo for consistency.
- Update writes now and defer reads/edit UI to a separate migration task.
  User confirmation received: Apply the credential protection change to all auth user create/update paths.

Please confirm, correct, or clarify these assumptions before implementation continues.
