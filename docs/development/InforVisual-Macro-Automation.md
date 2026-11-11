# Infor VISUAL Automation from the MTM Receiving Application

Last Updated: 2026-11-11

How the app reads and drives the Infor VISUAL **Purchase Receipt Entry** window (`VMRCVENT.EXE`) directly,
without deploying or executing a macro. Field meanings, backing columns, and the evidence for them live in
[InforVisual-Macro-Variable-Reference.md](./InforVisual-Macro-Variable-Reference.md).

## Why the app does not use macros

VISUAL macros are VBScript files named `<PROGRAM_ID>_<NAME>.VMS` beside the module executable. Recovered from
the running module, the engine's own SQL decides where macros live:

```sql
select STORE_MACROS_IN_DB from APPLICATION_GLOBAL
```

On this install `STORE_MACROS_IN_DB = 'N'`, so macros are files on the shared VISUAL folder and the `MACRO`
table stays empty. Either way a deployed macro is **live for every user of the share** until someone removes
it, and the Macros menu is only re-read when the module window opens.

The app therefore performs the same work itself: it reads the window with UI Automation and writes fields
with real mouse and keyboard input. Nothing is added to the VISUAL share, and nobody else is affected.

## What a macro and this app do the same way

A macro gets every screen field as a read/write VBScript variable. This app gets the same values by reading
the Gupta window's automation tree, and changes them by clicking the field and typing - which is what a
person does, and effectively what a macro does.

| Concern | Macro | This app |
| ------- | ----- | -------- |
| Read a header field | `ORDER_ID` variable | Automation element beside the "Order ID" label |
| Write a header field | assign the variable | click, select-all, type, Tab, read back |
| Known required inputs | `ORDER_ID` + `SITE_ID` | same two, per the toolkit documentation |
| Report a result | `MACRO_SUCCESS` / `MACRO_MESSAGE` | the probe verdict on the card |
| Side effects on other users | file lives on the shared VISUAL folder | none |
| Save the receipt | macro can call save | **never** - no save path exists |

## How a field is located and read

The window publishes 101 automation elements with `PO-072040` loaded (61 on an empty form). Field *value*
boxes are Gupta panes: they expose no UI Automation patterns, are not keyboard focusable, and carry no
automation id, so they cannot be written through Automation APIs. They are matched positionally instead:

1. Find the static label whose text matches a known caption (`Order ID`, `Bill of Lading ID`, ...).
2. Take the widest control on the same row, to the right of that label (within +/-8 px of the label's
   vertical centre), taller than 18 px and wider than 40 px. This skips the small lookup buttons that sit
   between label and box.
3. Read its `Name`, which is the value currently shown.

Every mapped variable resolves this way - 16 of 16 on a loaded receipt.

## How a field is written

1. Activate the VISUAL window and confirm it actually holds focus.
2. Click the value box, `Ctrl+A`, type, press `Tab` to commit.
3. Re-read the field and compare with what was typed.
4. If the probe asked for a restore, type the original value back and re-read that too.

Only three fields are ever typed into - `BOL_ID`, `VEND_PACK_ID`, `VEND_FREIGHT_ID`. They are receipt-only
free text: writing one cannot change a purchase order, a quantity, or a posting state. The variable safety
classes are recorded in the reference document, and a test enforces that the writable set never widens past
that safe class.

## The write path is gated on focus

Keystrokes go to whatever window is in front, so the driver refuses to type unless VISUAL is the foreground
window, re-checks focus after activating, and returns an explanatory failure instead of typing blind. A run
that loses focus mid-probe reports that rather than corrupting a field.

## Table automation

The line grid is the part that matters for receiving work, and it is the part UI Automation cannot see: the
grid exposes no grid, table, or scroll patterns. The practical split is therefore:

- **Read the lines from the database.** `PURC_ORDER_LINE` joined to `RECEIVER_LINE` gives part, ordered
  quantity, previously received quantity, and the receipt's current values, exactly as the grid shows them.
  The grid's "Total Qty Recv" is the stored rollup `PURC_ORDER_LINE.TOTAL_USR_RECD_QTY`, verified to equal
  the sum of `RECEIVER_LINE.USER_RECEIVED_QTY` on all five lines of `PO-072040`.
- **Write through the grid.** Cells are reached by click plus keyboard navigation. The editable columns are
  `USER_RECEIVED_QTY`, `LOCATION_ID`, `WAREHOUSE_ID`, the Recv-to-Inspect checkbox, and the three date
  columns; the rest are display-only.

`LINES` is not a usable entry point: it is a count, and `LINES(1)` raises *Type mismatch* outside VISUAL.
The column-to-column mapping for every grid column is in the reference document.

## The Visual Macro Tester page

`Module_VisualAutomation` adds one navigation entry below **Volvo Dunnage Requisition**, separated by a
divider. Each probe is its own collapsible card, collapsed by default, stating what it proves, what it does
on the VISUAL window, and what a pass looks like. A card's status is only the word **Pass** or **Fail** -
the reason and the captured values are never printed on the page.

Two icon buttons beside **Reset** open the modals:

| Icon | Modal | Contents |
| ---- | ----- | -------- |
| Info | **Probe details** | Every probe from the last run: status, the reason for the verdict, and the values it captured from the live window |
| Edit | **Field values sent to Infor VISUAL** | The three values the app types into the receipt |

### Field values

The app sends exactly three values, and they start **blank**:

| Macro variable | Screen field |
| -------------- | ------------ |
| `BOL_ID` | Bill of Lading ID |
| `VEND_PACK_ID` | Vendor Packlist ID |
| `VEND_FREIGHT_ID` | Vendor Freight Bill ID |

All three are required. **A run will not start while any of them is blank** - the app shows what is missing
and opens the values dialog instead. The dialog keeps its primary button disabled until every value is
filled in, and cancelling leaves the live set untouched.

| Probe | What it establishes | Needs values |
| ----- | ------------------- | ------------ |
| `P01_Attach` | The live window is found and readable | no |
| `P02_ControlSurface` | The automation tree is stable across reads | no |
| `P03_IdentityFields` | `ORDER_ID`, `SITE_ID`, `RECEIVER_ID` read correctly | no |
| `P04_DateAndPartyFields` | Dates, buyer, FOB, ship via, carrier read correctly | no |
| `P05_ReferenceFields` | The reference fields resolve | no |
| `P06_VariableCoverage` | Which of the 87 macro variables exist on the visible surface | no |
| `P07_FieldWriteRestore` | Types `BOL_ID`, verifies, restores | yes |
| `P08_RoundTripFidelity` | Types `VEND_PACK_ID` and `VEND_FREIGHT_ID`, verifies character fidelity, restores | yes |
| `P09_NoSaveGuard` | No save action is ever performed | no |
| `P10_RestoreIntegrity` | Every field matches the pre-run snapshot | no |

Typical run: load a purchase order in Purchase Receipt Entry, open the page, enter the field values, then
**Run selected probes**. `P01` and `P02` answer immediately; `P07` and `P08` briefly take focus of the
VISUAL window.

## Safety rules the code enforces

- No save action exists anywhere in the module - no Save button is clicked and no save key sequence is sent.
- Only receipt-only free-text fields are typed into.
- Every write is read back, and every write probe restores the original value.
- Typing requires a confirmed foreground VISUAL window.
- Database access is read-only; `ApplicationIntent=ReadOnly` is set on the connection.

## See Also

- [Infor VISUAL Macro Variable Reference](./InforVisual-Macro-Variable-Reference.md)
- [Infor Visual database reference files](../../.github/instructions/architecture/infor-visual-database-reference.instructions.md)
