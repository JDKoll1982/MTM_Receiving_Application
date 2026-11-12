# Frequently Asked Questions

Last Updated: 2026-11-12

## Why can I not change every line?

Because the module only lets you write a line when the material can be fully accounted for. If the movements and the lines do not add up, writing a location would be a guess.

## What does "Not Enough" mean?

The lines are asking for more material than the movements deliver. The pane shows the raw difference.

## What does "Extra Surplus" mean?

The movements delivered more material to a destination than the lines account for. Again, the raw difference is shown.

## What does "NotFound" mean?

No Infor Visual transfer was found for that part around that day. It is counted in the summary and logged, and it does not ask you for anything.

## Why does a line I fixed yesterday still show up?

It reports as **Unchanged**, because the saved location now matches where the material went. You will not be asked to change it again.

## Can I skip a line?

Yes, with **Escape**. The skip lasts for the session; the line comes back the next time the day is loaded. It is still counted as skipped in the summary.

## Can I write several lines at once?

No. One line, one write. There is no bulk apply, on purpose.

## Can I export the summary?

No. Export is not part of this version.

## Who is recorded as making a change?

The signed-in Windows user. There is no supervisor approval step in this version.

## Why is a day taking a while?

The module asks Infor Visual once per group of rows. A day with many parts asks more times. There is one retry per request.

## What happens if Infor Visual is down?

A warning appears at the top of the page and the affected lines are marked **Error**. The rest of the day still works. Reload the day when Visual is back.
