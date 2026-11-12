# Guardrails

Last Updated: 2026-11-12

## Do

- Keep the quantity proof as the only gate to a write, and re-run it when a line is accepted.
- Keep direction on a transfer from the transaction type.
- Keep the balance comparison as of the receipt day rather than using the live figure.
- Keep every outcome named, counted, and reachable from the day summary.
- Keep the module's own data access, procedures and settings keys.
- Add a test for any behaviour change, and keep the reference case green.

## Do not

- Do not reference `Module_Receiving` in any form.
- Do not add a table, a column, or an audit trail in this slice.
- Do not write to Infor Visual. Every Visual query is a `SELECT`, and the connection keeps read-only intent.
- Do not put business logic in code-behind, or a DAO call in a view model.
- Do not introduce a tolerance, an epsilon, or a percentage allowance in a quantity comparison.
- Do not reintroduce an ignored-locations list or an "ignored by settings" outcome.
- Do not add an export action to the day summary.
- Do not merge the collaborators back into one large class.

## Sensitive areas

| Area | Why it is sensitive |
| --- | --- |
| `Service_ReconciliationQuantityProof` | It is the write gate. A tolerance or a skipped check silently permits a wrong write. |
| `Service_ReconciliationWriteService` | It re-proves and then writes once. A retry loop here would write twice. |
| `18a` / `18b` | Direction decides source and destination. Get it wrong and the proof certifies a wrong answer. |
| `sp_Reconciliation_ReceivingLocation_Update` | The only write between the modules. It must reject a blank location and a blank operator. |
| `ModuleServicesExtensions.cs` | Registrations belong here, never in the host. |

## Validation

- Build the solution.
- Run the reconciliation tests.
- Grep for the other module's name in both directions.
- Confirm the reference case passes.
- Confirm a preview writes nothing.
