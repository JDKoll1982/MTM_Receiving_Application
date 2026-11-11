# Quick Start

Last Updated: 2026-11-11

For the person testing this page on the shop floor. No programming knowledge needed.

## Before you start

1. Log in to Infor VISUAL on the same PC as this app.
2. Open **Inventory > Purchase Receipt Entry**.
3. Load a purchase order so the receipt screen shows an Order ID and a Site ID.
4. Leave the receipt open and do not close it while you test.

You also need three test values written down: a bill of lading number, a vendor packlist number, and a
vendor freight bill number. Any short text works; use values you can recognise, such as `TEST-BOL-1`.

## Running the test

1. In this app, open **Visual Macro Tester** in the navigation menu.
2. Press **Check VISUAL window**. The top line should show the window title, a PID number, an element
   count, and a screen position. If it says the window is not running, go back to step 2 above.
3. Press the **pencil icon** next to Reset.
4. Fill in **BOL_ID**, **VEND_PACK_ID**, and **VEND_FREIGHT_ID**, then press **Use these values**.
   All three are required before any test can start.
5. Press **Run selected probes**. Every card starts with its tick box selected.
6. Do not click or type anywhere until the run finishes. Some tests briefly take control of the VISUAL
   window and type into it.

## Reading the result cards

- Each card is one test. Press the arrow on a card to read what it does on the VISUAL window, what it
  proves, and what a pass looks like.
- **Not run** (grey) means it has not run yet.
- **Running** (blue) means it is working.
- **Pass** (green) means the test found what it expected.
- **Fail** (red) means it did not. Expand the card, then open the **details icon** (the list icon next to
  the pencil) to read the reason and the values the test read from VISUAL.

## What the app does to your receipt

- It reads the fields on the screen.
- For three text fields it types a test value, reads it back, then puts the original value back.
- It never presses Save and never sends a save command, so an open receipt cannot be committed by a test
  run. If you want to be certain, look at the VISUAL window after a run: the receipt is still open and
  unsaved.

## If something looks wrong

- **VISUAL window not found**: the receipt screen is closed or minimised. Open it again and press
  **Check VISUAL window**.
- **Nothing happens when you press Run**: the three field values are not all filled in. Press the pencil
  icon and complete them.
- **A test fails**: open the details icon and read the sentence under the test name. It usually names the
  field and the value that was expected.
- **A field on screen shows a test value**: close the receipt without saving, then reopen it. Nothing was
  saved to the database.
