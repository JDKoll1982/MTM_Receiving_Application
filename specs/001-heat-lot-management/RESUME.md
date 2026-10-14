# Resume Here — Heat / Lot Management

**Last updated:** 2026-10-14
**Status:** COMPLETE. **60 of 60 tasks checked. Build green, 802 of 802 tests pass.**
The spec is at `status: completed`.
**Feature dir:** `specs/001-heat-lot-management`

## How to continue

Nothing is outstanding. Both tasks that were blocked are closed: **T059** was unblocked by
installing PowerShell 7 and regenerating the catalog, and **T045** by adding a shared
`ViewModel_Receiving_StepBase` for the four guided-step view models.

The sections below are kept as a record of the blockers and the environment traps that cost time.
Read them before touching this feature again; the two named tasks are done, not pending.

## What is already done and verified

- **Database is live.** `mtm_receiving_application` at **172.16.1.104** (MySQL 5.7.24) has
  `lot_markings`, `lot_marking_part_scope`, the four `heat_*` columns, and 11 new routines, all
  verified at 2/2 tables, 4/4 columns, 11 routines. Identical on `mtm_receiving_application_test`.
- **Smoke tested on core:** 6/6 procedures execute. `sp_Receiving_LabelData_GetAll` returned 175
  real rows, proving the modified column lists work against live data.
- **Backup:** `%USERPROFILE%\MTM_DB_Backups\mtm_receiving_application-20261014-121429.sql` (4.36 MB).
- **Service layer compiles:** `Dao_Tool_LotMarking`, `IService_Tool_LotMarking`,
  `Service_Tool_LotMarking`, plus `ShipRecToolsSettingsKeys.LotMarking` (5 constants).

## The two tasks that were blocked, both now closed

1. **T045 (blocked) — the marking on the per-step receipt header card.** T043, T044 and T046 are
done, so lines and the review card already show markings and both receiving view models expose
`ReceiptLotMarkings`. What is missing is the four guided steps:
`View_Receiving_HeatLot.xaml`, `View_Receiving_LoadEntry.xaml`,
`View_Receiving_WeightQuantity.xaml`, `View_Receiving_PackageType.xaml`. Each binds only to its own
step view model, and those four share nothing that carries receipt markings:
`ViewModel_Receiving_HeatLot`, `_WeightQuantity` and `_PackageType` each derive straight from
`ViewModel_Shared_Base` and expose their own `Loads`, while `ViewModel_Receiving_LoadEntry` exposes
**no loads collection at all** — it creates them. So a `<TextBlock>` bound to `ViewModel.Loads`
compiles in three of the four files and fails the XAML compile in the fourth.

   The fix is one member, then four bindings. Either give the four step view models a shared base
they do not currently have, or put a `ReceiptLotMarkings` collection on `ViewModel_Shared_Base`.
The second is one file but puts a receiving concept on every view model in the app, which is a
layering smell; the first is cleaner and costs four class declarations. Decide this deliberately.

2. **T059 (blocked) — regenerate the SQL object catalog.** `Build-SqlObjectCatalog.ps1` calls
`[System.IO.Path]::GetRelativePath` in `SqlCodebaseReferenceTools.ps1` line 281, which exists only
on PowerShell 7 / .NET Core. This machine has Windows PowerShell 5.1 and no `pwsh`, so the script
fails partway through and **truncates** `Database/Database_Deployment/sql-object-catalog.json`.
That damage was already repaired once from git. Install PowerShell 7 (`winget install Microsoft.PowerShell`)
before running it, and check `git diff --stat` on the catalog afterwards either way.

## Gotchas that will cost time if rediscovered

- **`python3` is not on every process's PATH, but it is on this machine.** A subagent that reports
  `python3` missing should fall back to `.venv\Scripts\python.exe`; `dotnet` and `git` likewise
  resolve only after refreshing PATH from Machine+User:
  `$env:Path = (Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Environment').Path + ';' + (Get-ItemProperty 'HKCU:\Environment').Path`
- **`dotnet build` writes to a shared `obj/` tree.** Two agents building at once produce `CS2012`
  locks and `CS2001` missing-generated-file errors, and the WinUI XAML compiler then cascades
  `WMC9999`. Wait and retry; do not "fix" it, and never work around it with
  `-p:BaseIntermediateOutputPath` — that breaks the XAML compiler outright.
- **The settings manifest is strict-parsed.** `dataType`, `scope` and `permissionLevel` use bare
  `Enum.Parse` with no fallback and `isSensitive` uses `GetBoolean`. A single typo throws while the
  registry is constructed, which crashes startup for every setting, not just the bad one.
- **`ExecuteListAsync<int>` returns `Model_Dao_Result<List<int>>`**, not a single int. Single-row
  procedures must unwrap `.Data[0]`.
- **MySQL procedures cannot have default parameter values.** That is why the four write procedures
  were left with their original signatures and markings are written by the additive
  `sp_Receiving_LabelData_SetMarking` instead. Do not add parameters to them.
- **A blank heat number is stored as the literal `"Nothing Entered"`** by three call sites. The
  normalizer treats it as blank. Treating it as a real lot number collapses every blank-heat line
  into one shared marking.
- **Shapes are emoji and the emoji is the persisted value** (10 shapes, 12 colors, 120 combinations).
  There is no shape enum. Never store display names or hex.
- **Markings must never be recomputed.** Written once at save time, then copied verbatim through
  promotion and reprint.
- **`contracts/service.md` is stale** against the implemented `IService_Tool_LotMarking`: it names
  `GetPaletteAsync`, `GetMarkingsAsync`, `FindByHeatAsync` and `SetPartEnabledAsync(partId, …)`, while
  the code has `GetAllMarkingsAsync`, `SearchMarkingsByHeatAsync` and `SetPartEnabledAsync(id, …)`.
  Trust the code.
- **Windows PowerShell 5.1 strips double quotes** from native command arguments, so prefer
  single-quoted arguments (or a here-string) for the Companion capture calls rather than inline JSON.
- **`.spec-context.json` writes intermittently fail** with `WinError 5` (transient lock). Retry after
  a short pause; do not hand-edit the file.

## Deliberately not done

- **T018's promotion path** is verified by construction only (both column lists grew symmetrically);
  no row has been archived through it since the change. A real receive-then-archive test is the last
  proof for that path.
- **The migration and the two new tables were never added** to `Database/SyncTool/sync_policy.yml`
  (default-deny), nor to `sql-object-catalog.json`, `StoredProcedure-Test-Checklist.md`, or the
  generated `docs/database/` pages. Those are T055–T059.
- **No tests exist yet.** T034, T038, T039, T047, T051.
