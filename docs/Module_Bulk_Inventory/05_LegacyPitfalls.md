# Module_Bulk_Inventory — Legacy Pitfalls

> Specific bugs, anti-patterns, and fragile behaviours found in the legacy WinForms app
> (`MTM Receiving Manager / Visual_Inventory_Assistant`) that must **not** be repeated.
> Each entry names the source file and line range where the problem lives.

---

## CRITICAL — Show-stopper class bugs

### PF-01  `VisualLogger` inherits `Form` — a God Class

**Source:** `Classes/VisualLogger.cs` — class declaration `internal class VisualLogger : Form`

**What's wrong:** A data-access / automation class inherits `Form` so it can reach UI controls directly
(`Application.OpenForms["MainForm"]`).  This completely couples the automation logic to the WinForms
host and makes it impossible to unit test, reuse, or run headless.

**Do not repeat:** `IService_UIAutomation` (and `Service_VisualInventoryAutomation` that wraps it)
must be plain classes with no UI inheritance, injected via constructor DI.  They communicate
results and status back through return values, not through direct form field reads.

---

### PF-02  Global static credential/form references

**Source:** `Classes/VisualLogger.cs` lines ~20-25

```csharp
private static MainForm mainForm = Application.OpenForms["MainForm"] as MainForm;
internal static string visualUserName = ApplicationVariables.VisualUserName;
internal static string visualPassword = ApplicationVariables.VisualPassword;
private static string argument = "-d MTMFG -u " + visualUserName + " -p " + visualPassword;
```

**What's wrong:**
1. `Application.OpenForms["MainForm"]` returns `null` if the form isn't open or is renamed — silent NRE.
2. Static fields are set once at class load time; if credentials change mid-session the class uses stale values.
3. The password is concatenated into a static `argument` string that persists in memory indefinitely.

**Do not repeat:** Credentials must be fetched fresh from `IService_UserSessionManager` at the point of
use, held only in local variables, and never stored as static or instance fields outside the call that
needs them.

---

### PF-03  `Thread.Sleep` throughout the fill sequence

**Source:** `Classes/VisualLogger.cs` — `FillTransferPartID` (~line 310), `FillTransferQuantity`
(~line 400), `FillTransferToLocation` (~line 500), `SendKeysToVisual` (~line 245)

```csharp
Thread.Sleep(1000);  // repeated many times
Thread.Sleep(250);
Thread.Sleep(100);
```

**What's wrong:** Blocks the **UI thread** in a WinForms app (WinForms calls this from event handlers);
arbitrary delays are fragile on slower machines and wasteful on faster ones.

**Do not repeat:** Use `await Task.Delay(ms)` inside `async Task` methods.  Better: use
`AutomationElement.WaitForInputIdle()` or an active polling loop with a `CancellationToken`
rather than fixed sleeps.

---

### PF-04  Busy-wait infinite loop with no exit condition

**Source:** `Classes/VisualLogger.cs` — `FillTransferCheckForInventoryTransactionEntryWindow` (~line 575)

```csharp
while (true)
{
    if (FindWindow("Gupta:AccFrame", "Inventory Transaction Entry") == IntPtr.Zero)
    {
        break;
    }
    mainForm.MainForm_StatusText_Loading.Text = "Waiting...";
    // no Sleep!  Pure 100% CPU spin
}
```

**What's wrong:** Pure CPU spin — no sleep.  Will saturate one CPU core until the user dismisses the
dialog.  On older machines this can cause the dialog itself to become unresponsive.

**Do not repeat:** Polling loops must always include `await Task.Delay(100)` and a `CancellationToken`
check.  The pattern for "wait for a window to disappear" should be:
```csharp
while (FindWindow(null, windowTitle) != IntPtr.Zero)
{
    ct.ThrowIfCancellationRequested();
    await Task.Delay(100, ct);
}
```

---

### PF-05  `MessageBox.Show` for every error condition

**Source:** `Classes/VisualLogger.cs` — appears in ~25 catch blocks and ~10 null checks

```csharp
catch (Exception ex)
{
    MessageBox.Show($"An error occurred while filling Part ID: {ex.Message}", ...);
}
```

**What's wrong:**
1. Interrupts execution mid-automation, leaving Visual in a partially-filled state.
2. Blocks the calling thread until the user dismisses the dialog.
3. Focus shifts away from the Visual window, breaking subsequent `SendKeys` calls.
4. No logging — errors vanish after the user clicks OK.

**Do not repeat:** Errors during automation must be:
1. Logged via `IService_LoggingUtility.LogError()`
2. Stored in the row's `ErrorMessage` field
3. Row status set to `Failed`
4. Control returned to the ViewModel, which updates the UI non-blockingly

---

### PF-06  `FillTransferToLocation` has a logic error with `windowOpen` flag

**Source:** `Classes/VisualLogger.cs` — `FillTransferToLocation` (~line 490)

```csharp
if (windowOpen == false)
{
    // fill To Location and send TAB
}
else
{
    FillTransferWaitForSaveButtonClick(); // called even though window is still open!
}
```

**What's wrong:** When `windowOpen == true` (the "Inventory Transaction Entry" duplicate popup appeared),
the code calls `FillTransferWaitForSaveButtonClick` — which enables the "Next" button and signals
success — even though the transaction was **not** actually submitted.  The user can then advance to
the next row without realising the current row was not saved.

**Do not repeat:** A row must only be marked `Success` after Visual has actually committed the
transaction (e.g., the Visual grid row count increases, or the window returns to a clean state).
A pending popup = the row status stays `InProgress`; the UI must show "Waiting for confirmation"
and block the "Next" action until the popup is resolved by the user.

---

## IMPORTANT — Significant quality issues

### PF-07  `FillTransferCheckForPartsWindow` uses `SendWait` with no window-open guard

**Source:** `Classes/VisualLogger.cs` — `FillTransferCheckForPartsWindow` (~line 540)

```csharp
var hwndParts = FindWindow("Gupta:AccFrame", "Parts");
if (hwndParts != IntPtr.Zero)
{
    SetForegroundWindow(hwndParts);
    SendKeys.SendWait("{UP}");
    SendKeys.SendWait("{ENTER}");
}
```

**What's wrong:** If the window exists but is minimised or behind another window,
`SetForegroundWindow` is not guaranteed to succeed on Windows 10/11 (foreground focus rules).
The `{UP}{ENTER}` keys could go to a different focused window.

**Do not repeat:** After `SetForegroundWindow` call, verify the target window is actually in the
foreground (check using `GetForegroundWindow` P/Invoke) before sending keys.

---

### PF-08  `argument` string embeds password as plain text in process args

**Source:** `Classes/VisualLogger.cs` line ~25

```csharp
private static string argument = "-d MTMFG -u " + visualUserName + " -p " + visualPassword;
```

**What's wrong:** Command-line arguments are visible to all users via Task Manager → Details →
right-click → "Command line" column.  The Visual password is exposed to any user on the workstation.

**Mitigation options for new module (pick one):**
1. Pass credentials via `Process.StartInfo.EnvironmentVariables` instead of CLI args (if Visual
   supports it — needs investigation)
2. Accept this limitation as unavoidable (Visual's CLI is vendor-defined) and document it explicitly
3. Use Windows DPAPI to at least avoid in-memory plain text

---

### PF-09  `SendKeysToVisual` does not verify the element is enabled before filling

**Source:** `Classes/VisualLogger.cs` — `SendKeysToVisual` (~line 240)

```csharp
AutomationElement element = FindTextBoxByAutomationId(handle, id);
if (element != null)
{
    SetForegroundWindow(new IntPtr(element.Current.NativeWindowHandle));
    Thread.Sleep(250);
    SendKeys.Send("^a");
    SendKeys.Send("{DEL}");
    SendKeys.Send(text);
    ...
}
```

**What's wrong:** No check that the element is `IsEnabled = true` or `IsKeyboardFocusable = true`.
If Visual has disabled the field (e.g., grayed out due to prior validation failure), the keystrokes
are silently swallowed and the field remains at its old value.

**Do not repeat:** Before filling a field, check:
```csharp
if (!element.Current.IsEnabled || !element.Current.IsKeyboardFocusable)
{
    throw new InvalidOperationException($"Field {automationId} is not enabled/focusable.");
}
```

---

### PF-10  `FindTextBoxByAutomationId` iterates ALL descendants every call

**Source:** `Classes/VisualLogger.cs` — `FindTextBoxByAutomationId` (~line 600)

```csharp
AutomationElementCollection textBoxes = mainWindow.FindAll(
    TreeScope.Descendants,
    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

foreach (AutomationElement textBox in textBoxes)
{
    if (textBox.Current.AutomationId == automationId)
        return textBox;
}
```

**What's wrong:** `FindAll(Descendants, ...)` with `ControlType.Edit` returns every Edit control in the
entire window hierarchy.  For a Visual window with hundreds of controls this is slow (~200-500ms per call).
Called 8+ times per transaction row = 1.6-4 seconds just in element lookups.

**Do not repeat:** Use a compound `AndCondition` to filter by both ControlType AND AutomationId in a single
call:
```csharp
var condition = new AndCondition(
    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
    new PropertyCondition(AutomationElement.AutomationIdProperty, automationId));
return mainWindow.FindFirst(TreeScope.Descendants, condition);
```
This stops at the first match and is ~10x faster.

---

### PF-11  No check that `VMINVENT.exe` is accessible before launching

**Source:** `Classes/VisualLogger.cs` — `OpenVisualInventoryCommand` (~line 125)

```csharp
var cmd2 = Process.Start(@"\\visual\visual908$\VMFG\VMINVENT.exe", argument);
```

**What's wrong:** If the network share is unavailable (Visual server offline, VPN disconnected),
`Process.Start` throws a `Win32Exception` with a generic access denied / file not found message.
This bubbles up to a `MessageBox` with no guidance on what went wrong.

**Do not repeat:** Before launching the process:
```csharp
if (!File.Exists(exePath))
{
    return Model_Dao_Result.Failure(
        $"Infor Visual executable not found at '{exePath}'. " +
        "Verify the Visual server is online and the path is correct in Settings.");
}
```

---

### PF-12  Google Sheets service account JSON bundled in binary directory

**Source:** `MTM Receiving Manager/` root — `service_account.json` (committed to git history)

**What's wrong:** A Google Cloud service account private key was present in the project directory.
It was committed to local git history (confirmed in the discovery session conversation above).
The key must be considered compromised and rotated.

**Do not repeat:**
- No secrets, keys, or tokens in any source or binary directory
- Add `*.json` exclusion rules to `.gitignore` for any credential files  
- The new module has **no Google Sheets integration** — this attack surface is eliminated

---

### PF-13  `MainForm._currentRowIndex` is zero-based but UI displays 1-based without comment

**Source:** `Classes/VisualLogger.cs` — `FillTransferWaitForSaveButtonClick` (~line 620)

```csharp
mainForm.MainForm_StatusText_Loading.Text =
    "Transaction " + (mainForm._currentRowIndex + 1).ToString() + " Saved.";
```

**What's wrong:** The `+ 1` offset is silently applied in two separate places.  If a future
developer adds a third location that reads `_currentRowIndex` they will likely display the wrong
number.

**Do not repeat:** Use a single computed property `CurrentRowDisplayNumber => _currentRowIndex + 1`
on the ViewModel to centralise the offset.

---

## NICE-TO-KNOW — Minor issues worth noting

### PF-14  `IsNewTransaction` reads the TextBox text, not the data model

**Source:** `Classes/VisualLogger.cs` — `IsNewTransaction` (~line 75)

```csharp
string columnBValue = mainForm.MainForm_TextBox_From.Text.Trim();
bool isNew = columnBValue.Equals("NEW", StringComparison.OrdinalIgnoreCase);
```

`"From"` field containing `"NEW"` is the signal for New Transaction mode.  This is an undocumented
convention that only lives in this one branch.  A future developer who renamed or repurposed the
"From" field would silently break the transaction type detection.

**Do not repeat:** Use an explicit `TransactionType` enum field on the row model.

---

### PF-15  No logging — errors silently discarded after MessageBox

**Source:** `Classes/VisualLogger.cs` — every catch block ends with `MessageBox.Show` but no write
to any log file.  If the user dismisses the dialog there is no trace of the error.

**Do not repeat:** Every exception in the automation service must be logged with
`IService_LoggingUtility.LogError(message, exception)` before (or instead of) surfacing to the user.

---

### PF-16  `closeVisualTransferCommand` sends `WM_CLOSE` with no guard

**Source:** `Classes/VisualLogger.cs` — `closeVisualTransferCommand` (~line 645)

If the user has unsaved manual edits in the Transfer window, `WM_CLOSE` closes it without prompting —
discarding their work silently.

**Do not repeat:** Before sending `WM_CLOSE`, either:
1. Only close windows the *app itself* opened (track this in a boolean flag per automation session)
2. Or send `Escape` first to check for a "Save Changes?" dialog and handle it

---

### PF-17  Every row pushed to Visual individually — no pre-consolidation

**Source:** `Classes/VisualLogger.cs` — `FillTransferPartID` and the outer push loop in
`Windows/MainForm.cs`

**What's wrong:** The legacy app sends every row in the Google Sheet as a separate Visual
automation cycle, even when multiple rows share the same Part ID, From Location, and To Location.
A batch of 20 rows for the same part to the same bin produces 20 Visual automation cycles
instead of 1.  Each cycle requires waiting for window focus, filling ~6 fields, and waiting for
Visual to confirm — roughly 4-8 seconds per row.  A consolidatable 20-row batch takes
80-160 seconds instead of ~8 seconds.

**Do not repeat:** Before the push loop begins, consolidate all rows that share the same
**(PartID, FromLocation, ToLocation)** key by summing their `Quantity` values.  Only the
consolidated rows are pushed to Visual.  The original source rows remain in the audit table
with a `Consolidated` status that references the consolidated row ID.

---

*Last Updated: 2026-03-08 — Updated to reflect approved corrections*
