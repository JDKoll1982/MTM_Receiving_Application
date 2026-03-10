# Method 3: Direct SQL Writes to MTMFG — Human Developer Tasks

Last Updated: 2026-03-10

This method requires the most demanding human work of all three alternatives. You must
capture what VISUAL actually writes to the SQL Server database during a real inventory
transaction, because the check constraints and application logic are encrypted and cannot
be read from the schema files.

**Do not skip any step here. Incorrect writes to MTMFG can corrupt inventory counts.**

---

## Prerequisites

- [ ] You have access to **SQL Server Management Studio (SSMS)** connected to the VISUAL
      (`VISUAL`) server.
- [ ] You have permissions to run **SQL Server Profiler** (or Extended Events) against
      the MTMFG database. Ask the DBA if you are unsure.
- [ ] A **VISUAL test company** (separate from MTMFG production) is available.
      If one does not exist, you must create it or get the DBA to help — **never run
      test transactions in MTMFG production**.
- [ ] You can log in to VISUAL against the test company.
- [ ] You know the credentials for the MTMFG SQL Server; the current `appsettings.json`
      uses `User Id=SHOP2;Password=SHOP` — confirm with the DBA that these credentials
      have INSERT permission on MTMFG (they currently do NOT — they have `ReadOnly`).

---

## Step 1 — Start SQL Server Profiler

1. Open **SSMS → Tools → SQL Server Profiler**.
2. Connect to server `VISUAL`.
3. Create a new trace with **only these event classes** to avoid log flooding:
   - `SQL:BatchCompleted`
   - `RPC:Completed`
   - `SP:StmtCompleted` (optional — add if you want stored proc detail)
4. Add a **filter**: `DatabaseName = 'MTMFG'` (or whichever test company database name is).
5. Add a filter for the VISUAL login: `LoginName = 'SHOP2'` — this shows only VISUAL's writes.
6. Start the trace. Leave it running.

> If SSMS Profiler is unavailable, use **Extended Events** as an alternative. Ask the DBA.

---

## Step 2 — Perform a Manual Inventory Transfer in VISUAL (Test Company)

1. Log in to VISUAL → test company.
2. Open **Inventory → Inventory Transfers**.
3. **Before filling any fields**, switch to Profiler and clear the trace buffer (Start → Clear).
4. Switch back to VISUAL.
5. Fill in:
   - Part ID: a real part that exists in the test company (e.g. one you know has stock)
   - Quantity: `1`
   - From Warehouse: your warehouse code
   - From Location: a location with stock
   - To Warehouse: same warehouse code
   - To Location: a different location
6. Click **Post** (or whatever the commit button is in the Inventory Transfers window).
7. Wait for VISUAL to confirm the transaction.
8. Switch back to **Profiler** immediately.

---

## Step 3 — Capture and Save the Profiler Trace

1. Stop the trace (Profiler → File → Stop Trace).
2. Look at the captured SQL statements. You are looking for `INSERT`, `UPDATE`, and any
   stored procedure calls that ran after you clicked Post.
3. Save the full trace as a `.trc` or `.xml` file:
   **File → Save As → Trace File** → save to `docs/InforVisual/` in this repo.
   Name it: `profiler_inventory_transfer_<date>.trc`
4. Also copy the text of every `INSERT` and `UPDATE` statement into a new file:
   `docs/InforVisual/INVENTORY_TRANSFER_WRITE_PATH.md`
   Include the full SQL text, table names, column names, and actual values used in the test.

---

## Step 4 — Repeat for Inventory Transaction Entry

1. Clear the Profiler trace buffer again.
2. In VISUAL, open **Inventory → Inventory Transaction Entry**.
3. Fill in:
   - Work Order: a real open work order in the test company
   - Lot No: `1`
   - Quantity: `1`
   - To Warehouse
   - To Location
4. Post the transaction.
5. Capture the Profiler trace.
6. Save to `docs/InforVisual/INVENTORY_TRANSACTION_ENTRY_WRITE_PATH.md`.

---

## Step 5 — Identify the Exact Write Path

From the Profiler output, answer these questions and document the answers in
`docs/InforVisual/INVENTORY_TRANSFER_WRITE_PATH.md`:

- [ ] Which tables does VISUAL INSERT into?
- [ ] Which tables does VISUAL UPDATE?
- [ ] Does VISUAL call any stored procedures? What are their names?
- [ ] What value does it use for the "transaction type" column (e.g. `TRANS_TYPE`,
      `TRANSACTION_CODE`, or similar)?
- [ ] Are there any sequence/identity columns auto-generated, or does VISUAL manually
      compute the next ID?
- [ ] Does VISUAL update the PART table's on-hand quantity directly, or does a trigger handle it?
- [ ] Does VISUAL write to a history/audit table?
- [ ] Are all the writes in a single SQL transaction (BEGIN TRAN … COMMIT)?

---

## Step 6 — Verify Trigger Side-Effects

1. In SSMS, query the affected tables before and after a test transfer:
   ```sql
   -- Before snapshot
   SELECT * FROM INVENTORY_TRANS WHERE PART_ID = 'your_test_part' ORDER BY ... DESC;
   SELECT QTY_ON_HAND FROM PART WHERE ID = 'your_test_part';

   -- (Run the VISUAL transfer here)

   -- After snapshot
   SELECT * FROM INVENTORY_TRANS WHERE PART_ID = 'your_test_part' ORDER BY ... DESC;
   SELECT QTY_ON_HAND FROM PART WHERE ID = 'your_test_part';
   ```
2. Verify `QTY_ON_HAND` changed as expected.
3. If the PART table updated but you did NOT see an UPDATE in the Profiler, a **trigger**
   is doing it. That is fine — triggers fire automatically when we INSERT into `INVENTORY_TRANS`,
   so we do NOT need to update PART ourselves.

---

## Step 7 — Check That SHOP2 Credentials Can Write

1. In SSMS (connected as `sa` or DBA account), run:
   ```sql
   USE MTMFG;
   EXECUTE AS LOGIN = 'SHOP2';
   -- Try inserting a test row and immediately rolling back
   BEGIN TRAN;
   INSERT INTO INVENTORY_TRANS (PART_ID, ...) VALUES ('TEST', ...);
   ROLLBACK;
   REVERT;
   ```
2. If you get a permissions error, contact the DBA to grant `SHOP2` INSERT on the required
   tables. **Do not grant blanket write on all tables** — request table-specific INSERT only.
3. Alternatively, ask the DBA to create a dedicated write-only account (e.g. `MTM_BULK_WRITE`)
   with INSERT on only `INVENTORY_TRANS` (and any other tables identified in Step 5).

---

## Step 8 — Update Connection String Configuration

Once the DBA has confirmed credentials and permissions:

1. Add the write connection string to `appsettings.json`:
   ```json
   "InforVisualBulkWrite": "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;"
   ```
2. **Remove `ApplicationIntent=ReadOnly`** from this string only. The existing `InforVisual`
   connection string for all read DAOs keeps `ReadOnly` unchanged.
3. Inform the developer of the exact table names and column names from your Profiler capture
   so they can implement `Dao_InforVisualInventoryWrite`.

---

## Step 9 — Validation Test Against Test Company

After the developer implements the DAO:

1. Run ONE transaction via the MTM app pointing at the test company DB.
2. In SSMS, query `INVENTORY_TRANS` for your test part — confirm the row exists with the
   correct quantities, warehouse codes, and date.
3. Compare it side-by-side with a row inserted by the VISUAL UI for the same inputs.
   **They must be identical in structure and values (except time and auto-generated IDs).**
4. Verify `QTY_ON_HAND` updated on PART table.

---

## Step 10 — Get Inventory Manager Sign-Off

Before any live MTMFG production writes:

- [ ] Show the inventory manager the comparison from Step 9.
- [ ] Get written (email) sign-off that the direct writes are accurate.
- [ ] Document the sign-off date in `Changes-and-Decisions/Decisions.md`.

---

## Documents to Create and Save to Repo

| File | Contents |
|---|---|
| `docs/InforVisual/INVENTORY_TRANSFER_WRITE_PATH.md` | Full Profiler SQL, table names, column values |
| `docs/InforVisual/INVENTORY_TRANSACTION_ENTRY_WRITE_PATH.md` | Same for Transaction Entry |
| `docs/InforVisual/profiler_inventory_transfer_<date>.trc` | Raw Profiler trace file |
| `docs/InforVisual/profiler_entry_<date>.trc` | Raw trace for Transaction Entry |
| `Module_Bulk_Inventory/Documentation/Changes-and-Decisions/Decisions.md` | Constitutional amendment approval |

---

## ⚠️ Warning: Do Not Rush This

Getting the write path wrong will silently corrupt MTMFG inventory. VISUAL has no
"undo" for inventory transactions — they must be manually reversed via VISUAL itself.
The schema research (Steps 1–6) is the entire foundation of this method. Budget
adequate time to do it carefully.
