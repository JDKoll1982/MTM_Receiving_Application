# Playbook: DAO and Database Compliance

Goal: Ensure every DAO uses stored procedures (MySQL) or read-only queries (Infor Visual) and returns Model_Dao_Result without throwing.

## Prep

- Docent `AM` on each module to list Dao_* classes and methods.
- Cross-check with [repomix-output-code-only.xml](repomix-output-code-only.xml) for Dao_* files.

## Checks

- MySQL: stored procedure names passed to Helper_Database_StoredProcedure; no inline SQL strings.
- SQL Server: connection strings include ApplicationIntent=ReadOnly; operations are SELECT-only.
- Return types: async methods returning Model_Dao_Result or Model_Dao_Result<T>; no exceptions thrown to callers.
- Parameters: use typed parameters; avoid string concatenation.

## Steps

1) **Inventory**: For each Dao_* method, document SP name, parameters, return model.
2) **Fix violations**: Replace inline SQL with stored procedure calls; wrap errors in Model_Dao_Result.Failure.
3) **Tests**: Add integration tests against test DB; validate success and failure paths.
4) **Resilience**: Add Polly policies at caller (handlers) for transient faults if needed.

## Agents & Tools

- dev-developer for DAO fixes; dev-test-engineer for integration tests.
- plan-architect for DB access mapping; Docent `AM` outputs for traceability.

## Done Criteria

- No inline SQL for MySQL paths; SQL Server paths read-only.
- All DAO methods return Model_Dao_Result variants and are async.
- Tests cover happy/exceptional paths.
