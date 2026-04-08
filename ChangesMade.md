# Changes Made

This file gives a quick, plain-English summary of the work currently waiting to be committed.

## Dunnage

- The Dunnage type selection screen now uses a 3 by 3 card layout instead of the larger previous layout, so each page shows 9 type cards at a time.
- Dunnage type cards now behave better when a type has its own image:
  - the type name still stays visible,
  - the extra icon is hidden when an image is being used,
  - the card is taller so the image and label are not cut off.
- When adding or editing a Dunnage part or Dunnage type, the image area now includes a Rotate 90° button.
- The Rotate 90° button only becomes usable after an image has been selected.
- Rotating an image updates the preview so the user can see the new orientation before saving.

## Receiving

- In Receiving Load Entry, recommended location cards can now be clicked.
- Clicking one of those location cards copies that location directly into the Location box for faster entry.

## Material Availability Board

- The Material Availability Board now uses the label Incoming Material instead of Incoming PO progress.
- The board now includes a look-ahead selector with 30, 60, 90, and All options.
- Search results now change based on the selected look-ahead window.
- The board now shows which larger associated parts are expected to use the selected material next.
- Incoming material is shown more reliably, even when some date details are missing.
- The board text and card layout were updated so the results are easier to read across the full width of the page.

## Behind The Scenes

- Supporting data work was added so the Material Availability Board can find upcoming associated-part demand.
- Plain-language help data for Dunnage, Receiving, and the Material Availability Board was refreshed so future guidance matches the new behavior.
- Check coverage was added for the new image rotation, recommended-location click behavior, and the updated Material Availability Board results.
