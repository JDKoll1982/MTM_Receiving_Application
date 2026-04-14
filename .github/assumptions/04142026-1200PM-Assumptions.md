Last Updated: 2026-04-14

# Assumptions For Receiving Location Reconciliation Aggregate Transfer Matching

1. Assumption: The intended rule is to treat multiple MTM receiving rows for the same `PO + Part + optional PO line + receipt day` as one receipt group when Infor Visual shows a later transfer whose total quantity matches the group's total quantity.
Why this assumption is needed: The request describes four 5,000-unit skids being transferred in one 20,000-unit transaction and expects all four rows to reconcile to the same destination location.
Potential impact if wrong: If grouping should be narrower or broader than this, the reconciliation could move too many or too few rows.
Alternative interpretations considered: Group by exact receipt timestamp; group by PO + part only; group by receipt session/load ID instead of day.

2. Assumption: The destination location should be applied to every affected MTM row in that group when one destination location has transfer evidence covering the full grouped quantity, even if no individual transfer row exactly matches each saved row quantity.
Why this assumption is needed: The current implementation only accepts exact per-row quantity matches such as `5000 -> 5000`, which prevents `5000 + 5000 + 5000 + 5000 -> 20000` from reconciling.
Potential impact if wrong: If per-row exact matches are still required, changing the logic would make reconciliation more permissive than intended.
Alternative interpretations considered: Only mark rows as suggested instead of updated; only update the first row; only apply the change when transaction history also contains per-row breakdown entries.

3. Assumption: A SQL-only change will not fully fix the reported issue, because the current service logic in the reconciliation layer still rejects buckets unless they match each row's quantity exactly or appear in exact per-row transaction history.
Why this assumption is needed: The query already returns `MatchedTransactionQuantity` aggregated by destination location, but the service still returns `NotFound` for a 5,000-unit row when the only transfer evidence is a single 20,000-unit movement.
Potential impact if wrong: If there is another code path consuming the SQL differently, expanding the fix into the service would be unnecessary scope.
Alternative interpretations considered: The SQL data returned today is incorrect; the service is not the blocker; only the evidence query needs a new aggregated flag.

4. Assumption: The correct implementation scope is to update the evidence/reconciliation logic together, which likely means touching `16_GetReceivingLocationEvidence.sql`, `Service_ReceivingLocationReconciliation.cs`, and the relevant unit tests.
Why this assumption is needed: The desired behavior spans both how evidence is surfaced and how that evidence is allocated across multiple saved rows.
Potential impact if wrong: Restricting the change to the SQL file alone may leave the bug unresolved; expanding the scope without approval may exceed what you wanted changed.
Alternative interpretations considered: Limit the change strictly to the SQL file and accept that the service may still need a later follow-up.

Please confirm, correct, or clarify these assumptions before implementation proceeds.
In particular, please confirm whether you want me to make the full fix across SQL plus reconciliation service and tests, since the current evidence query alone does not appear sufficient to resolve the scenario you described.