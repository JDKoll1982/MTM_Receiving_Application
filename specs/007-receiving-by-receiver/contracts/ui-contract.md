# UI Contract: Receiving By Receiver Mode

The screen contract for the By Receiver mode. It fixes the mode label, the column order and headers,
the receiver list surface, and the Load Entry readout a consumer or test codes against, without
reopening the spec. Every string in this file is case-sensitive and must be matched exactly by the
view, its bindings, and any test that asserts them.

## Verbatim Constraint Identifiers

These identifiers are copied verbatim from the spec's Verbatim Constraints section. They are the
authoritative spellings a consumer or test codes against.

### Mode control label

- `By Receiver`

The mode control sits between `Guided` and `Manual` in the Receiving mode controls.

### Column headers

The receiver list shows these column headers, exactly as written, in this order:

| Order | Header |
| --- | --- |
| 1 | `Receiver Number` |
| 2 | `PO #` |
| 3 | `Line #` |
| 4 | `Part Number` |
| 5 | `Vendor` |
| 6 | `Location Received to` |
| 7 | `Qty Received` |
| 8 | `Qty Ordered` |
| 9 | `Qty Remaining on Line` |

## Receiver List Screen

- **Screen title**: `Receiving - By Receiver`
- **View**: `Module_Receiving/Views/View_Receiving_ReceiverSelection.xaml` (+ `.xaml.cs`)
- **View model**: `Module_Receiving/ViewModels/ViewModel_Receiving_ReceiverSelection.cs`
- **Reached from**: selecting `By Receiver` in the Receiving mode controls, or opening Receiving when
  the stored default is `by-receiver`.
- **Row model**: `Module_Core/Models/InforVisual/Model_InforVisualReceiverRow.cs`
- **Workflow step**: `Enum_ReceivingWorkflowStep.ReceiverSelection`
- **Query**: `Database/InforVisualScripts/Queries/35_GetRecentReceivers.sql`, read through
  `IService_InforVisual.GetRecentReceiversAsync` and `Dao_InforVisualReceiver`.

### Header controls

| Control | Default | Behaviour |
| --- | --- | --- |
| Receiver number search | empty | Contains match on the receiver number, narrowing as the operator types |
| Rows per page | `50` | Offers `25`, `50`, `100`, `200` |
| Refresh | n/a | Re-reads the source without leaving the mode |

Clearing the search box restores the full list at page one. Changing the page size or the search text
while on a later page returns the view to the first page.

### Pager

- A pager shows the current page and the total, paging rather than scrolling.
- When the filtered result fits on one page, the paging controls are present but inactive.
- The pager reflects the filtered count, not the unfiltered count.

### Row selection

Selecting a row carries these values into the receiving workflow state, in this order: receiver
number, PO number, line number, part number, vendor, location received to, quantity received,
quantity ordered, and quantity remaining. The next screen is Load Entry, with the location received
to and the quantity remaining already filled in.

Back from Load Entry returns to the receiver list with the previously selected row still highlighted.

## Load Entry Readout

- **Screen title**: `Receiving - Enter Load Information`
- **Readout label**: `Qty Remaining on Line`
- **Behaviour**: read-only, shows the chosen row's remaining quantity, hidden when the walk did not
  start from a receiver. The location received to field is prefilled from the chosen row and stays
  editable.

## Stored Default Token

- **Token**: `by-receiver`
- Recognised in `StartWorkflowAsync`, `NormalizeDefaultMode`, and the option lists of
  `ViewModel_Settings_Receiving_WorkflowDefaults` and `ViewModel_Settings_Receiving_BusinessRules`.
- `By Receiver` is one of the choices in the Receiving workflow default settings.
