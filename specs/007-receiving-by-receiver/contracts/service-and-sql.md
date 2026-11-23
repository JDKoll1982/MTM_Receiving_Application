# Service and Query Contract: Recent Receivers

The read side a consumer or a test codes against. Names and types here are what tasks will implement;
the UI identifiers are pinned in [ui-contract.md](./ui-contract.md).

## Row model

`MTM_Receiving_Application.Module_Core.Models.InforVisual.Model_InforVisualReceiverRow`

| Member | Type |
| --- | --- |
| `ReceiverNumber` | `string` |
| `ReceivedDate` | `DateTime` |
| `PONumber` | `string` |
| `POLineNumber` | `string` |
| `PartNumber` | `string` |
| `VendorName` | `string` |
| `Warehouse` | `string` |
| `LocationReceivedTo` | `string` |
| `QtyReceived` | `decimal` |
| `QtyOrdered` | `decimal` |
| `QtyRemaining` | `decimal` |

The selection key is `ReceiverNumber` + `PartNumber` + `POLineNumber`.

## Service method

On `IService_InforVisual`:

```text
Task<Model_Dao_Result<List<Model_InforVisualReceiverRow>>> GetRecentReceiversAsync(int maxRows = 1000)
```

- `Service_InforVisualConnect` implements it as a pass-through to `Dao_InforVisualReceiver`.
- When `GetUseInforVisualMockData()` is true it returns a fixed sample list rather than the source.
- The repository's standard doc comment applies: read-only Infor Visual, no writes.

On `Dao_InforVisualReceiver` (constructor takes the Infor Visual connection string, optional logger):

```text
Task<Model_Dao_Result<List<Model_InforVisualReceiverRow>>> GetRecentReceiversAsync(int maxRows)
```

The constructor rejects any connection string whose `ApplicationIntent` is not `ReadOnly`, exactly as
`Dao_InforVisualPO` does. Failures come back as `Model_Dao_Result` failures, never exceptions.

## Query

`Database/InforVisualScripts/Queries/35_GetRecentReceivers.sql`, loaded by name through
`Helper_SqlQueryLoader.LoadAndPrepareQuery`.

| Parameter | Type | Meaning |
| --- | --- | --- |
| `@MaxRows` | `INT` | safety cap on returned rows; default 1000 |

Result columns, in this order: `ReceiverNumber`, `ReceivedDate`, `PONumber`, `POLineNumber`, `PartNumber`,
`VendorName`, `Warehouse`, `LocationReceivedTo`, `QtyReceived`, `QtyOrdered`, `QtyRemaining`.

Rules the query must apply (each row here is a testable predicate):

| Rule | Predicate |
| --- | --- |
| Receipt has a date | `RECEIVER.RECEIVED_DATE IS NOT NULL` |
| Part present | `PURC_ORDER_LINE.PART_ID` is not null and not blank |
| Inventory added | `RECEIVER_LINE.WAREHOUSE_ID` is not null and not blank |
| Not removed | `RECEIVER.MARKED_FOR_PURGE` is not `'Y'` |
| PO line resolves | inner joins from `RECEIVER_LINE` to `PURC_ORDER_LINE` on `PURC_ORDER_ID` + `PURC_ORDER_LINE_NO` |
| One row per receiver and part | group by receiver, part, PO line, vendor |
| Quantity received is the sum | `SUM(RECEIVER_LINE.RECEIVED_QTY)` |
| Remaining never negative | `CASE WHEN ORDER_QTY - TOTAL_RECEIVED_QTY < 0 THEN 0 ELSE ORDER_QTY - TOTAL_RECEIVED_QTY END` |
| Newest first | `ORDER BY RECEIVED_DATE DESC, RECEIVER.ID DESC` |
| Bounded | `SELECT TOP (@MaxRows)` |

The location shown is the receiving line's `LOCATION_ID`, which may be blank; the warehouse then shows
alone. `VENDOR.NAME` falls back to `PURCHASE_ORDER.VENDOR_ID` when it is null.

## Workflow state

`Service_ReceivingWorkflow` exposes, for the list view model and the Load Entry readout:

| Member | Type | Meaning |
| --- | --- | --- |
| `IsByReceiverFlow` | `bool` | the walk was opened from the By Receiver mode |
| `SelectedReceiver` | `Model_InforVisualReceiverRow?` | the row the walk was started from |
| `SelectedReceiverKey` | `string?` | the highlight key, `receiver|part|line` |
| `CurrentReceiverRemainingQty` | `decimal` | feeds the Load Entry readout |

`ResetWorkflowAsync` clears all four along with the existing PO, part, and location state.
