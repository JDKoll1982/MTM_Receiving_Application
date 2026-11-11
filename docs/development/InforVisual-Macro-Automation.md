# Infor VISUAL Macro Automation

Last Updated: 2026-11-10

How Infor VISUAL exposes its Purchase Receipt Entry window (`VMRCVENT.EXE`) to VBScript macros, what
was verified on this installation, and how the app's **Visual Macro Tester** page
(`Module_VisualAutomation`) uses that behaviour to self-test.

## What VISUAL macros are

A VISUAL macro is a VBScript file that lives beside the module executable and is named
`<MODULENAME>_<MACRONAME>.VMS`. When the module window opens it enumerates those files and lists each
one under its **Macros** menu. Inside the script every screen field is available as a read/write
variable, so a macro drives the same controls a user drives - business logic still applies.

Verified on this installation (2026-11-10):

| Item | Value |
| ---- | ----- |
| Module executable | `\\visual\visual908$\VMFG\VMRCVENT.EXE` |
| Window title | `Purchase Receipt Entry - Infor VISUAL - MTMFG` |
| VISUAL version | 9.0.8 |
| Macro folder | `\\visual\visual908$\VMFG` (same folder as the executable) |
| Menu bar | File, Edit, View, Info, Options, Notes, **Macros**, Help |
| Macros menu contents with no `.VMS` files present | `Edit...` only |

## Event macros run without the Macros menu

Special macro names make VISUAL run the script by itself:

| Macro name | Fires when |
| ---------- | ---------- |
| `OnLoad` | a record is loaded into the window |
| `OnNew` | the New button is pressed |
| `OnSave` | before a save is committed - *not deployed by this app* |
| `OnAfterSave` | after a save is committed - *not deployed by this app* |

Because a file named `VMRCVENT_OnLoad.VMS` is executed by VISUAL on every record load, nothing has to
drive the menu bar. That is why the harness offers an **OnLoad probe** as its non-menu route. The
trade-off is reach: an event macro is live for every user of the shared VISUAL share until it is
removed, so the harness backs up any file it displaces and removes its own file when the deployment is
cleared.

## Fields a macro can see

The macro editor's **Available Fields** list is the authoritative surface. On the Purchase Receipt
Entry window it contains header fields, user defined fields, and status fields:

```text
BOL_ID  BUYER  CARRIER_ID  CONTACT_*  DATABASE  DESIRED_RECV_DATE  DISPATCH_ID  FREE_ON_BOARD
LINES  LONG_DESCRIPTION  MACRO_MESSAGE  MACRO_SUCCESS  ORDER_ID  PO_USER_1..10
POSTING_CANDIDATE  PROMISE_DATE  PROMISE_SHIP_DATE  RECEIVED_DATE  RECEIVER_ID
RECVR_LONG_DESC  SHIP_VIA  SITE_ID  STATUS  USER  VEND_FREIGHT_ID  VEND_PACK_DATE  VEND_PACK_ID
VENDOR_ID  udfBoolean1..5  udfDate1..5  udfNumber1..10  udfStrCombo1..10  udfString1..10  udfURL1..5
```

Notes that matter when writing a probe:

- `MACRO_MESSAGE` and `MACRO_SUCCESS` exist so a macro can report its own outcome back to whatever
  started it.
- The `udf*` slots are not shown on the General tab, which makes them safe scratch space for a
  read/write round-trip.
- `LINES` reports a line count. The per-line access shape is **not** documented in the Available
  Fields list; the `T04_LineGridRead` probe tries the plausible shapes (`LINES(1)`,
  `LINES(1).PART_ID`, `LINE_COUNT`, `ITEM_COUNT`) and reports which one answers.
- The grid columns shown in the window (`colLINE_NO`, `colPART_ID`, ...) come from `VMRCVENT.INI`
  under `[tblReturnLineItem]`, not from the macro field list. They configure the grid, they are not
  macro variables.

## Getting results back out

A macro has no return value the caller can read, so the harness uses a file as the result channel:

1. The app writes `VMRCVENT_MTMProbeAll.VMS` with an absolute result path embedded in it.
2. The macro creates the file with `Scripting.FileSystemObject` and writes `KEY=value` lines inside
   `#TEST=<probe>` / `#END` sections, finishing with a `#TRAILER` marker.
3. The app polls the file until the trailer appears, then parses each section and decides pass/fail
   per probe card.

The generated script was executed against the real VBScript engine (VBScript 10.8) and produced all
sections cleanly, including `CreateObject` success for `Scripting.FileSystemObject`,
`WScript.Shell`, and `ADODB.Connection`, plus working `Now`, `DateAdd`, and `DateSerial` calls.

## Constraints this app respects

- **No saves.** The harness never deploys `OnSave` or `OnAfterSave`, and the generated script contains
  no save call. The `T09_NoSaveGuard` card fails the run if either becomes true. Saving stays a human
  action on the VISUAL Save button.
- **Restore what you touch.** Every write probe captures the original value first and writes it back
  before the section ends.
- **Macro list is read at module start.** A newly deployed `.VMS` shows up under Macros only after the
  Purchase Receipt Entry window is closed and reopened once.
- **Shared share.** The macro folder is a network share used by every VISUAL workstation, so probe
  files are removed when the deployment is cleared.

## The Visual Macro Tester page

`Module_VisualAutomation` adds one navigation entry below *Volvo Dunnage Requisition* in the shell's
left pane, separated by a divider. Each probe is its own card that states what it proves, what it does
to the VISUAL window, and what a pass looks like.

| Card | Route |
| ---- | ----- |
| `T01_TargetDiscovery` | Resolve the running process, window title, and macro folder |
| `T02_MacroDeployment` | Write `VMRCVENT_MTMProbeAll.VMS` and check the naming contract |
| `T03_HeaderFieldRead` | Read Order ID, Site ID, Buyer, FOB, Status, Posting Candidate |
| `T04_LineGridRead` | Discover the receipt line collection shape |
| `T05_ScratchUdfRoundTrip` | Write, read back, and restore `udfString1` / `udfNumber1` / `udfDate1` |
| `T06_HeaderFieldWriteRestore` | Write and restore Bill of Lading ID |
| `T07_MacroStatusChannel` | Drive `MACRO_MESSAGE` and `MACRO_SUCCESS` |
| `T08_RuntimeEnvironment` | Script engine version, COM objects, date arithmetic |
| `T09_NoSaveGuard` | Prove the run cannot commit anything |
| `T10_EventAutoRun` | Run `VMRCVENT_OnLoad.VMS` with no menu interaction |

Typical run:

1. Open the page. `T01` settles immediately from the live process.
2. Press **Deploy selected probes**, then run `MTMProbeAll` from **Macros** in Purchase Receipt Entry.
   First time only: close and reopen the Purchase Receipt Entry window so the menu re-reads the folder.
3. Cards flip to **Passed** or **Failed** as the result file is parsed. The activity log keeps the full
   trail and the result file path is shown so it can be opened directly.

## See Also

- [Infor VISUAL macro overview](https://www.gingerhelp.com/infor-visual-blog/developing-and-using-macros-to-automate-common-tasks)
- [Running VISUAL macros from outside VISUAL](https://community.infor.com/discussion/3127/running-visual-macros-from-outside-visual)
- The macro folder also ships the VISUAL API Toolkit DLLs and PDFs under `VMFG\API Toolkit Documents`.
  That toolkit is .NET Framework x86 only, so it cannot be loaded by this .NET 10 x64 app directly.
