# Editing Brief

Last Updated: 2026-11-12

## What this module is

One day of receiving rows, the Infor Visual movements for the parts on that day, and a two-part quantity proof that decides whether a line may be written. The entry point is the working page; the only write is one accepted line.

## The three things not to break

1. **The module boundary.** Nothing here may name `Module_Receiving`, and nothing in Receiving may name this module. No shared model, enum, constant, data access class, procedure name or settings key. The shared fact is the physical table, never the code.
2. **The single write procedure.** `sp_Reconciliation_ReceivingLocation_Update` is the only write over receiving rows from this module. Do not add a second procedure, a bulk path, or an inline `UPDATE`.
3. **The proof as the only write gate.** A line is writable only when every destination bucket fills exactly and the source residual agrees with the as-of balance. Never write around it.

## Where the logic lives

- `Service_ReconciliationPreviewOrchestrator` is the only type that composes collaborators. Nobody else calls another service.
- Each collaborator takes the data it needs as a parameter and returns data. They are pure and directly testable.
- The outcome derivation is one ordered set of checks in the orchestrator. Keep it ordered and keep it exhaustive.
- The view model depends only on the three published services, never on a data access class or a collaborator.

## Conventions

- Quantities are `decimal` everywhere. No `double`, no tolerance, no epsilon.
- Every async method ends in `Async` and returns `Model_Dao_Result<T>` rather than throwing for an expected failure.
- Display strings for outcomes exist in one place: the two converters.
- Views use `x:Bind` with an explicit mode, and every interactive control has an accessible name.
- Writes format every figure with the invariant culture.

## Validation steps before you call a change done

1. `dotnet build MTM_Receiving_Application.slnx`
2. `dotnet test MTM_Receiving_Application.Tests/MTM_Receiving_Application.Tests.csproj --filter "FullyQualifiedName~Module_Reconciliation"`
3. Grep the module folder for `Module_Receiving` and confirm nothing matches.
4. Confirm the `MMC0001000` reference case still passes.
