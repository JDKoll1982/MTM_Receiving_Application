# FAQ

Last Updated: 2026-11-11

## Does this page create a receipt in Infor VISUAL?

No. It reads the open receipt screen and, for three text fields, types a value and puts the original back.
It has no save path at all.

## Will my test values be saved if I forget to close the page?

No. Nothing the app types is committed. The values exist only in the open VISUAL window until that window
is closed without saving.

## Why do I have to enter three values just to read fields?

Current behaviour: the run is blocked until `BOL_ID`, `VEND_PACK_ID`, and `VEND_FREIGHT_ID` are all filled
in, and this applies to every run, including runs made only of read-only tests. Until that changes, fill in
three short test values before running.

## What is the difference between "Run selected probes" and "Run this probe"?

**Run selected probes** runs every card whose tick box is selected, in the order the cards appear.
**Run this probe**, inside an expanded card, runs only that card. Both need the three field values.

## Do I need a purchase order loaded?

For a meaningful result, yes. Several tests only pass when the receipt shows an Order ID and a Site ID.
Connection tests pass on an empty form but tell you much less.

## Can I run it while someone else is using VISUAL on my PC?

No. The run types with the real keyboard and mouse, so it needs the VISUAL window in front and nobody else
touching the machine until it finishes.

## Why did the VISUAL window jump to the front during a run?

The app must bring VISUAL to the front to type into it. That is expected for the two write tests. If
another window takes focus, the app stops and reports it instead of typing into the wrong place.

## What do the two icon buttons next to Reset do?

- The list icon opens **Probe details**: every test from the last run, with its reason and the values it
  captured.
- The pencil icon opens the **field values** dialog where you set the three test values.

## Why does a card say "Pass" but the details show an empty field?

Some tests pass as long as the field exists on screen, even when it is empty. Read the sentence under the
test name in the details dialog to see what that test actually checked.

## Is it safe to run on a real receipt that someone is about to post?

It never saves, but it does type into three text fields and restores them. Run it on a test or sample
receipt if you want zero interaction with live work, and never run it while a colleague is editing the
same receipt.
