# Unusual Situations

Last Updated: 2026-11-11

What the code does in each case, and what to check next.

## VISUAL is not running

`DiscoverWindow` returns `IsRunning = false` with the diagnostic
`No VMRCVENT.EXE window is open. Open Purchase Receipt Entry in Infor VISUAL first.`.

The page shows that text in the target line, logs it as `WARN`, and `RunProbesAsync` returns before
starting. Nothing is typed. If you run a single probe anyway, `P01_Attach` fails with the same text.

## The window is found but the element count is low or zero

`P02_ControlSurface` fails when the two reads disagree or the count is below 30, with
`Element count was X then Y; expected a stable count of at least 30.`

If reading elements threw, the driver stores
`The window was found but UI Automation could not read it: ...` in the window diagnostic. A count of 0
with a running process usually means the wrong window is in front, or that the process is still starting.

## A macro variable has no visible control

Expected for 71 of the 87 macro variables - for example `LINES`, `MACRO_MESSAGE`, `CONTACT_FAX`, and all
45 `udf*` slots. `LabelFor` returns null, so `KeyForLabel` never resolves them and `CaptureSnapshot`
records `HasControl = false`.

`P06_VariableCoverage` reports this as `n of 87 ... the other m have no control on the visible surface`.
A mapped variable that stops resolving is different: `P04`/`P05` fail and name it, and `P06` fails with
`n of m mapped variables resolved on the window.` Then check the caption text and the row layout.

## A write does not round-trip

`P07_FieldWriteRestore` compares trimmed text; `P08_RoundTripFidelity` compares character for character.
The failing verdict names the field and both strings, for example
`VEND_PACK_ID: VISUAL did not keep the typed value (typed '...', read back '...')`.

Likely causes: the field rejected the characters (length limit, or a validating field), the click landed
on a lookup button instead of the box, or the window lost focus between click and type. The driver refuses
to type when the window is not foreground and returns `Another window took focus before typing.` instead.

## A write worked but the original value was not restored

Verdict: `the write worked but the original value was not restored (left as '...')`. Follow up with
`P10_RestoreIntegrity`, which compares every readable field against the baseline snapshot.

## A probe left the window changed

Two things in the current code can produce this:

- **Cancellation during a write.** The write path does `Ctrl+A`, type, Tab, then
  `await Task.Delay(320, cancellationToken)`. If the run is cancelled inside that delay, the exception
  leaves `WriteValueAsync` before the restore block runs. The restore is not in a `finally`.
- **Focus stolen mid-write.** The driver re-checks focus only before typing the new value. If focus moves
  after that, the restore attempt can also fail.

Recovery today is manual: compare the field with its original value, which the verdict and the details
dialog both report (`KEY_ORIGINAL` and `KEY_RESTORED_TO`), and retype it in VISUAL if needed. Nothing was
saved, so closing the receipt discards the edit.

## Elevation mismatch

`Process.MainModule` access fails when the two applications run at different elevation levels. The driver
records `The window was found but its executable path could not be read (...). Run both applications at
the same elevation level.` Reads may still work; the executable path is empty.

## More than one VMRCVENT process

`DiscoverWindow` orders candidates by "has a main window handle" and takes the first. It does not ask
which one you meant. If the attached window is not the receipt you are looking at, close the extra
receipt windows and press **Check VISUAL window** again.
