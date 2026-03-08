# Infor Visual Receiving Integration Approaches

**Last Updated: 2026-03-08**  
**Status: Research / Decision-Pending**  
**Author: MTM Development Team**

---

## Purpose

This document evaluates multiple strategies to bridge the data gap between **Module_Receiving** (MTM Receiving Application) and the **Infor Visual Receiver Entry** window, so that operators do not have to re-enter data they have already captured in the MTM app.

### Hard Constraints (Non-Negotiable)

| Constraint | Reason |
|---|---|
| ❌ No direct writes to SQL Server (`MTMFG`) | Infor Visual's database enforces complex transactional triggers, fiscal period locks, and GL journal preparation. Writing rows directly corrupts costing, inventory balances, and AP accruals. |
| ✅ All data entry must pass through Infor Visual's UI | The Visual application processes business rules, generates `INVENTORY_TRANS` records, updates `PURC_ORDER_LINE.TOTAL_RECEIVED_QTY`, fires costing services, and writes to `RECEIVER` / `RECEIVER_LINE` in a coordinated way that cannot be safely replicated externally. |
| ✅ SQL Server connection remains `ApplicationIntent=ReadOnly` | This is enforced in the connection string and guarded by the `Guardrails.md` for `Module_Receiving`. It stays that way under every approach below. |

### Data Available from Module_Receiving

The following fields are already captured on `Model_ReceivingLoad` and `Model_ReceivingLine` — these are exactly the fields Infor Visual's Receiver Entry window requires:

| MTM Field | Visual Field | Screen Location |
|---|---|---|
| `PONumber` | PO Number | Receiver Entry header |
| `POLineNumber` | Line No | Receiver Entry grid |
| `PartID` | Part ID | Receiver Entry grid |
| `WeightQuantity` | Received Qty | Receiver Entry grid |
| `HeatLotNumber` | Lot / Heat Number | Receiver Entry grid |
| `ReceivedDate` | Received Date | Receiver Entry header |
| `InitialLocation` / `WarehouseID` | Warehouse / Location | Receiver Entry grid |
| `EmployeeNumber` | User / Employee | Receiver Entry header |
| `VendorName` | (auto-populated from PO) | Read-only on screen |

---

## Approach 1 — Reference Printout / Structured Display Panel

### Description

The MTM app generates a **formatted summary** of each receiving session — either printed to paper or displayed in an on-screen overlay panel — organized in the exact field order used by Infor Visual's Receiver Entry window. The warehouse operator keeps the printout visible while entering the data into Visual one time.

This is not automation; it is **data organization and presentation**. The operator still types into Visual, but every field is pre-validated and pre-organized, eliminating lookups, PO searches, and calculation errors.

### What Would Be Built

- A `View_Receiving_VisualHandoff` page in the MTM app
- Displays a receiver-ready summary per load/PO, in Visual's entry order
- Optional: prints a formatted sheet with a barcode per PO line that the operator scans in Visual

### Complexity: Low

### Pros

✅ **Zero integration risk** — no dependency on Visual's internal structure, version, or API  
✅ **No IT / network infrastructure** required beyond what already exists  
✅ **Data still goes through Visual's full business rule chain** exactly as designed  
✅ **Trivially maintainable** — immune to Visual upgrades, patches, or configuration changes  
✅ **Fully audit-compliant** — Visual's own audit trail is the only record  
✅ **Immediate to implement** — only requires building a display view  
✅ **Works even when Visual is on a different machine, version, or user session**

### Cons

❌ **Still requires manual keyboard entry** into Visual — only reduces, does not eliminate it  
❌ **Operator error still possible** during transcription  
❌ Scales poorly if a single session covers 20+ PO lines  

### Implementation Notes

```
Module_Receiving/
  Views/View_Receiving_VisualHandoff.xaml
  ViewModels/ViewModel_Receiving_VisualHandoff.cs
  Services/Service_VisualHandoffSummary.cs
```

The summary would group loads by PO number, sub-sort by PO line, and format in the same top-to-bottom order used by Infor Visual's Receiver Entry screen.

---

## Approach 2 — Infor Visual Built-in Macro Engine (VBA/VBScript)

### Description

Infor Visual ships with a built-in macro scripting engine that can automate its own windows. Macro scripts are written in VBScript or VBA and execute within the Visual process context, giving them direct access to Visual's form objects (fields, grids, buttons).

Evidence this is already in use: `docs/InforVisual/TransactionsMacro.txt` is a VBScript HTA that queries MTMFG using `ADODB.Connection` and renders transaction lifecycle data — proof the macro infrastructure exists at MTM.

A receiving macro would:
1. Accept data from an MTM-generated CSV/JSON file (or read from a shared location)
2. Open or focus the Receiver Entry window in Visual
3. Populate header fields (PO number, date, employee)
4. Iterate PO lines and fill the grid rows (part, qty, lot, location)
5. Stop before posting — allowing the user to review, then click **Add** or **Post**

### What Would Be Built

- MTM app exports a structured file (e.g., `receiver_handoff_YYYYMMDD_HHMM.csv` or `.json`) to a mapped network path or local folder
- A VBScript `.vbs` or `.hta` macro that reads the file and drives Visual's Receiver Entry form
- The macro is launched from within the MTM app using `Process.Start`

### Complexity: Medium

### Pros

✅ **Uses Visual's own processing engine** — all business rules fire normally  
✅ **User sees every field being populated** in Visual's UI before committing  
✅ **No external automation framework** needed beyond VBScript (already on every Windows machine)  
✅ **Precedent exists** — the `TransactionsMacro.txt` pattern is already familiar and working in this environment  
✅ **Low dependency** — only requires a file share or local temp folder  
✅ **Non-destructive by design** — macro only fills fields, user posts or cancels  

### Cons

❌ **Fragile** — macro must know Visual's exact control names and tab order. If Infor upgrades the Receiver Entry form, the macro breaks  
❌ **Visual must be open and on the correct screen** when the macro runs (or the macro must navigate to Receiver Entry itself, which is harder)  
❌ **Limited to VBScript capabilities** — error handing is minimal, and debugging macro failures is difficult  
❌ **Requires Visual's macro security settings** to permit script execution  
❌ **Visual's macro API is not publicly documented by Infor** — reverse engineering required  
❌ **One instance only** — macros cannot run in parallel for multi-user scenarios  

### Implementation Notes

```
docs/InforVisual/Macros/
  receiver_autofill.vbs      -- The macro script
  receiver_handoff.csv       -- Generated by MTM app (per session)
```

The MTM app would need a new service:

```csharp
// Service_VisualMacroExport.cs
public async Task<Model_Dao_Result> ExportHandoffFileAsync(
    List<Model_ReceivingLoad> loads,
    string outputPath)
```

---

## Approach 3 — Windows UI Automation (Microsoft UIA / FlaUI)

### Description

Microsoft's **UI Automation** framework (`System.Windows.Automation` or the open-source `FlaUI` library) can programmatically read and write UI controls in any Windows application — including Infor Visual — by interacting with the accessibility tree of the target window.

The MTM app would:
1. Detect or launch the Infor Visual process
2. Find the Receiver Entry window using its accessibility tree path
3. Locate each field control by its `AutomationId` or `Name` property
4. Set values directly into those controls
5. Stop before the final **Post** action so the user reviews and confirms

### Complexity: High

### Pros

✅ **Entirely through the UI** — Visual's application processes every value as if typed by a human  
✅ **No dependency on Visual's internal macro engine** — works even if macros are disabled  
✅ **Programmatic access is deterministic** — unlike SendKeys, UIA sets values directly  
✅ **The MTM app controls the flow entirely** — no separate macro runtime or script  
✅ **Can confirm successful field population** by reading the control's value back  
✅ **FlaUI is a well-maintained .NET library** compatible with .NET 8  

### Cons

❌ **High brittleness** — Infor Visual does not publish `AutomationId` values for its controls. These must be discovered using tools like `Inspect.exe` or `FlaUI Inspector`, and they may change between Visual versions or patches  
❌ **No official support from Infor** for this approach  
❌ **Complex navigation logic** — the entry screen involves multiple tabs, combo boxes, and a data grid; driving these via UIA is significantly more code than it appears  
❌ **Timing-sensitive** — Visual must be running, unlocked, and on the correct screen; modal dialogs can interrupt the automation  
❌ **Requires FlaUI NuGet package** (`FlaUI.Core`, `FlaUI.UIA3`), adding a dependency  
❌ **Maintenance burden** is high — every Visual upgrade requires re-testing and potentially re-mapping all control IDs  
❌ **Visual's grid control** (where PO lines are entered) may not expose individual cells via UIA, making line-level automation impossible or unreliable  

### Implementation Notes

```csharp
// NuGet: FlaUI.Core, FlaUI.UIA3
using FlaUI.Core;
using FlaUI.UIA3;

// Attach to running Visual process
var app = Application.Attach(Process.GetProcessesByName("VMFG")[0]);
using var automation = new UIA3Automation();
var window = app.GetMainWindow(automation);
// ... navigate to Receiver Entry window, find PO field, set value
```

The `VMFG.exe` process name or equivalent would need to be verified against the actual production Visual executable.

---

## Approach 5 — EDI / XML Import File (Infor Visual's Import Facility)

### Description

Infor Visual supports batch import of purchase receipts through its **transaction import** or **EDI import facility**. The application can accept structured data files (typically fixed-width or delimited text following EDI 850/856 conventions, or Infor's proprietary XML schema) that represent one or more receiver transactions.

The MTM app would:
1. Generate a properly formatted import file (XML or delimited) at the end of a receiving session
2. The file would be placed in a monitored import folder or the operator manually imports it from within Visual's **Receiver Entry** or **Purchase Order Receipts** import menu

### Complexity: High

### Pros

✅ **Officially supported by Infor** — this is a documented, published integration path  
✅ **Does not bypass any business rules** — Visual processes the import through its full application layer  
✅ **Batch-friendly** — handles 1 or 100 PO lines with equal effort  
✅ **No fragile UI control mapping** — data goes in through a defined file schema, not screen automation  
✅ **Supports unattended operation** if a monitored folder is configured in Visual  

### Cons

❌ **Infor's import file format for Receivers is not standardized across versions** — the exact schema for purchase receipt import must be validated against the specific version of Visual 9.0.8 installed at MTM  
❌ **Requires configuration in Visual** to enable and map the import facility — this may require Infor technical support or partner involvement  
❌ **EDI setup is complex** — EDI 856 (ASN) is for shipment notification, not PO receipts; the correct EDI transaction for receipts (if any) must be identified  
❌ **Error reporting is limited** — import failures often produce generic error logs rather than field-level feedback  
❌ **User must still complete the import step** in Visual (unless fully unattended with a monitored folder) — reduces but does not eliminate manual work  
❌ **File format research required** — the team must obtain and read Infor Visual's import file specification document for Receiver Entry, which may require a support contract  

### Implementation Notes

At a minimum, the following would need to be resolved before implementation:

1. Obtain Infor Visual 9.0.8 Receiver Entry import file specification from Infor's support portal
2. Determine whether Visual at MTM has the import facility licensed and configured
3. Identify the correct file format (Infor's proprietary XML vs. delimited text)
4. Validate the format against a test import in a non-production environment

The MTM app service would be:

```csharp
// Service_VisualImportFileGenerator.cs
public async Task<Model_Dao_Result> GenerateReceiverImportFileAsync(
    List<Model_ReceivingLoad> loads,
    string outputPath)
```

---

## Approach 6 — Infor ION API / BOD Message Bus

### Description

Infor's **ION (Intelligent Open Network)** middleware platform allows external applications to send **BOD (Business Object Document)** XML messages that ION routes into connected Infor applications, including Infor Visual. The relevant BOD for creating a purchase receipt is `ReceiveDelivery.Process` or `PurchaseOrder.Process`.

The MTM app would send a formatted BOD XML message to the ION API Gateway. ION validates and routes the message to Visual, which processes it through its normal business layer.

### Complexity: Very High

### Pros

✅ **The gold standard of Infor integration** — explicitly designed for this use case  
✅ **Fully decoupled** — the MTM app does not need to know anything about Visual's UI or database  
✅ **Supports async/retry** — ION handles delivery, retries on failure, and logs all messages  
✅ **Enterprise-grade audit trail** — ION keeps a complete message log separate from Visual  
✅ **Official Infor support** — Infor documentation for BOD-based integrations is available  

### Cons

❌ **Requires ION to be licensed, installed, and configured** at MTM — this is a significant additional product  
❌ **ION is typically part of Infor OS / CloudSuite** — on-premise Visual 9.x installations may not have it, or it may require an add-on purchase  
❌ **Very high implementation complexity** — requires BOD schema knowledge, ION Connect configuration, and API authentication setup  
❌ **Long lead time** — a new ION integration can take weeks to months to implement and test properly  
❌ **BOD routing for Receivers may still require configuration** inside Visual's ION adapter  
❌ **Not practical for an independent WinUI app** without a middleware team and infrastructure budget  

### When to Revisit

If MTM ever migrates to Infor CloudSuite Manufacturing or upgrades to a SaaS version of Visual, ION becomes the natural path and this approach should be re-evaluated at that time.

---

## Comparison Matrix

| Approach | Data Enters Via Visual UI | Complexity | Brittleness | Requires Infrastructure | Blocks on Visual Version | Effort Estimate |
|---|:---:|---|---|---|---|---|
| **1. Reference Printout** | ✅ Manual | Low | None | None | None | 1–2 days |
| **2. Built-in Macro (VBScript)** | ✅ Automated | Medium | Medium | Shared file path | Medium | 1–2 weeks |
| **3. Windows UI Automation (FlaUI)** | ✅ Automated | High | High | None | High | 3–6 weeks |
| **5. EDI / XML Import File** | ✅ Via import | High | Low | Import config in Visual | Medium | 4–8 weeks |
| **6. Infor ION / BOD** | ✅ Via API | Very High | Low | ION license + infra | Low | Months |

---

## Recommended Approach

### Phase 1 — Immediate (Approach 1)

**Build the Reference Printout / Structured Display Panel.**

This can be completed in days and delivers immediate value: operators see all validated, pre-organized data on screen before touching Visual. Transcription errors are dramatically reduced because lookups (part descriptions, vendor names, PO remaining quantities) are already resolved by the MTM app.

Implementation path:
- `View_Receiving_VisualHandoff.xaml` — summary display in Visual's field order
- `Service_VisualHandoffSummary.cs` — groups loads by PO/line, formats for display
- Optional: print button to generate a formatted receiving sheet

### Phase 2 — Near-Term (Approach 2)

**Prototype the VBScript Built-in Macro.**

Given that `TransactionsMacro.txt` is already in use as a VBScript HTA against the MTMFG database, the team already has the expertise and infrastructure to extend this pattern. A macro that reads a session export file from MTM and auto-populates the Receiver Entry header fields would eliminate the majority of keystrokes even if grid (line-level) automation proves difficult.

Implementation path:
- `Service_VisualHandoffExport.cs` — exports session data to a structured CSV/JSON file
- `docs/InforVisual/Macros/receiver_autofill.vbs` — VBScript macro that reads the file and drives Visual

### Defer

Approaches 3, 5, and 6 should be deferred until:
- The Phase 1/2 solution is in production and its limitations are measured
- Infor's import file specification for Visual 9.0.8 has been obtained (prerequisite for Approach 5)
- An assessment of ION licensing availability is completed (prerequisite for Approach 6)

---

## Open Questions

| # | Question | Owner | Priority |
|---|---|---|---|
| 1 | Does Infor Visual 9.0.8 at MTM have a Receiver Entry import file facility? What format? | IT / Infor Support | High |
| 2 | What is the exact process name and window title of the running Visual executable? | IT | Medium |
| 3 | Can `VMFG.exe` UI controls be inspected with `Inspect.exe` to discover `AutomationId` values for Receiver Entry fields? | Developer | Medium |
| 4 | Is ION licensed or available in the current Visual installation? | IT / Management | Low |
| 5 | What is the exact tab/field order and field names in Visual's Receiver Entry screen? | Visual user / screenshots | High (for any automation) |
| 6 | Are VBScript macros permitted to run on production workstations (AV/MDM policy)? | IT | Medium |

---

## Related Files in This Repository

| File | Relevance |
|---|---|
| [Module_Receiving/Models/Model_ReceivingLoad.cs](../../Module_Receiving/Models/Model_ReceivingLoad.cs) | Source data model — maps to Visual Receiver Entry fields |
| [Module_Receiving/Models/Model_ReceivingLine.cs](../../Module_Receiving/Models/Model_ReceivingLine.cs) | PO line data — maps to Visual Receiver Entry grid rows |
| [Module_Receiving/Documentation/AI-Handoff/Guardrails.md](../../Module_Receiving/Documentation/AI-Handoff/Guardrails.md) | Hard constraint documentation — no writes to SQL Server |
| [docs/InforVisual/TransactionsMacro.txt](TransactionsMacro.txt) | Existing VBScript HTA — precedent for Approach 2 |
| [Database/InforVisualScripts/Queries/01_GetPOWithParts.sql](../../Database/InforVisualScripts/Queries/01_GetPOWithParts.sql) | Read-only PO query — confirms RECEIVER/RECEIVER_LINE table structure |
| [docs/InforVisual/DatabaseReferenceFiles/MTMFG_Schema_Tables.csv](DatabaseReferenceFiles/MTMFG_Schema_Tables.csv) | Schema reference — confirms RECEIVER, RECEIVER_LINE, INVENTORY_TRANS columns |
