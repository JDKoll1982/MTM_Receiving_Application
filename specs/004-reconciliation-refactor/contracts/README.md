# Contracts: Receiving Validator

**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md) | **Data model**: [data-model.md](../data-model.md)
**Created**: 2026-11-12

These are the interfaces a consumer or a test codes against. They are the boundary of `Module_Reconciliation`, not an implementation sketch.

- [service-contracts.md](service-contracts.md): the module's own services, which the ViewModels and the collaborators depend on.
- [data-access-contracts.md](data-access-contracts.md): the module's DAOs, the MySQL stored procedures they call, and the settings keys they read.
- [visual-query-contracts.md](visual-query-contracts.md): the read-only Infor Visual query files and what each returns.

## Pinned Identifiers

Every string below is copied from the specification's Verbatim Constraints table. None may be renamed, re-cased, pluralised, or paraphrased.

| What | Exact value | Where this contract uses it |
| --- | --- | --- |
| Module name | `Module_Reconciliation` | every namespace, every type prefix |
| Feature label | `Receiving Validator` | page title and navigation entry |
| Supporting text | `from Infor Visual` | beside the feature label |
| Outcome names | `Unchanged`, `Updated`, `Extra Surplus`, `Not Enough`, `NotFound`, `Skipped`, `Error`, `Manually Set` | finding outcome, filters, summary labels |
| Reference part | `MMC0001000` | the calibration fixture |
| Reference locations | `RECV`, `SHIP`, `V-C0-01`, `R-04`, `R-05` | the calibration fixture |
| Query folder | `Queries/Reconciliation/` | where the moved and new Visual queries live |
| Settings key prefix | `Reconciliation.` | every settings key the module owns |

## Calibration Fixture

The reference case every quantity rule is checked against, with live on-hand at `RECV` of 41,000:

| Row | Part | Saved location | Quantity | Expected outcome | Expected destination |
| --- | --- | --- | --- | --- | --- |
| 1 | `MMC0001000` | `RECV` | 5,000 | `Updated` | `SHIP` |
| 2 | `MMC0001000` | `RECV` | 2,500 | `Updated` | `V-C0-01` |
| 3 | `MMC0001000` | `RECV` | 500 | `Updated` | `R-04` |
| 4 | `MMC0001000` | `RECV` | 1,000 | `Updated` | `R-05` |
| 5..n | `MMC0001000` | `RECV` | 41,000 total | `Unchanged` | none |

Every bucket nets to zero and the residual of 41,000 equals the as-of balance at `RECV`. Change the balance and rows 5..n become `Not Enough` while the four destination writes still stand, because the destination proof and the residual proof are independent (FR-024, FR-031).

The existing documented example also survives unchanged: five rows of 4,500, 5,000, 7,500, 4,350 and 2,000 with transfers of 12,000 to `V-A0-01`, 5,000 to `V-B0-01` and 6,350 to `V-C0-01` allocate as 4,500 plus 7,500 to `V-A0-01`, 5,000 to `V-B0-01`, and 4,350 plus 2,000 to `V-C0-01`.
