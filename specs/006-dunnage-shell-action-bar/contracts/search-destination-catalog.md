# Search Destination Catalog Contract

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

The application's search box opens pages. This is the contract that decides which pages it may offer and
how a chosen result is opened, so a test can compare the catalog against the navigation pane and both
settings hubs without reading the shell (FR-030 to FR-033).

## Where the catalog lives

`MainWindow.xaml.cs`, in `CreateSearchDestinations()` for pages and `CreateSearchCommandDestinations()`
for the `/` filters. Entries are declared once and indexed by key into `_searchDestinationsByKey`. The
catalog is a hand-maintained list, kept deliberately: a generated one could not express "this page is an
inner guided step, do not offer it".

## `SearchDestination`

A private nested type with three kinds.

| Field | Type | Rule |
| --- | --- | --- |
| `Key` | `string` | The route tag, the settings page's full type name, or `command:<text>`. Unique across the catalog. |
| `Label` | `string` | The text the operator sees. |
| `Kind` | `SearchDestinationKind` | `FrameRoute`, `SettingsPage`, or `Command`. |
| `RouteTag` | `string?` | Required for `FrameRoute`; it must be a key of `_navRoutes`. |
| `SettingsPageType` | `Type?` | Required for `SettingsPage`; it must be registered in dependency injection. |
| `Detail` | `string` | The one-line description shown under the label. |
| `CommandText` | `string?` | Set for `Command`; the `/` filter it applies. |
| aliases | `string[]` | Extra search terms. Never displayed as behaviour. |

## Completeness Rules

**R-1**: every key of `_navRoutes` has a `FrameRoute` entry, except the module the catalog already
comments out as offline. Today `ReconciliationLauncherPage` (Receiving Validator) is missing and must be
added.

**R-2**: every settings page that a settings hub card opens has a `SettingsPage` entry. This holds today
for both category hubs once the Label Files card is added.

**R-3**: every `SettingsPage` entry names a type registered in
`Infrastructure/DependencyInjection/ModuleServicesExtensions.cs`. Today
`View_Settings_Dunnage_LabelPaths` is offered but not registered, so it cannot open; it must be
registered.

**R-4**: no entry names a step inside a guided workflow. A module's entry page is what is offered in its
place. The Non-Inventoried Labels entry is the module's entry page, and the alias `non-inventoried labels
mode selection` is retired with the screen it names.

**R-5**: no entry's `Label` or alias names `Mode Selection`, `Manual`, or `Manual Entry` after this
change.

## Access Rules

**R-6**: an entry behind an access rule is offered only to a user the rule allows. Today
`ScannerMainPage` is offered to everyone; it must be filtered out of the offered results for a user the
scanner allow-list refuses, and restored without a code change when the allow-list adds them.

**R-7**: opening a result passes through the same access check navigation uses. Today the navigation
pane checks the scanner allow-list at `MainWindow.xaml.cs:787` and the search path does not. Both must
call one opener that carries the check and shows the same refusal message
(`NotifyScannerAccessDenied`).

**R-8**: opening a result runs the same leave confirmation navigation uses. This already holds for both
kinds: `FrameRoute` goes through `ClearModuleDraftStateBeforeNavigationAsync`, which asks the Dunnage and
Receiving pages, and `SettingsPage` goes through `ConfirmEnterSettingsModeAsync`, which asks them too.
The audit confirms it rather than adding it.

## Opening Rules

| Kind | Opens through | Failure |
| --- | --- | --- |
| `FrameRoute` | the shared route opener: access check, leave confirmation, then the frame | the entry must not be offered if its route tag is absent; a failed open reports the page could not be opened |
| `SettingsPage` | `EnterSettingsModeAsync`, which runs the leave confirmation first | the entry must not be offered if its type is not registered |
| `Command` | re-fills the search box with the filter's matches | never fails |

## Recorded Exceptions

- `AppDocumentation` is a `FrameRoute` key that resolves through the documentation launcher rather than
  a frame page. It keeps that path.
- The disabled Outside Service module has no entry, by the existing comment in the catalog.
- Any search result for an inner guided step is deliberately absent. Searching for `detail` or `review`
  offers `Non-Inventoried Labels`, not the step itself.
