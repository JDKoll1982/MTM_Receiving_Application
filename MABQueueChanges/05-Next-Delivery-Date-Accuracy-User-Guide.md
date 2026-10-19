# Next Delivery Date Tells You What It Really Means

Last Updated: 2026-10-19

Status: **Applied** — confirmed and requested dates are now shown as two separate, clearly worded
lines, and the order-date fallback has been removed.

The inbound date on the board is almost never a supplier-confirmed date, so the card now says which
kind of date it is showing and where it came from.

## What You See Before

Infor Visual holds four different kinds of date on an open purchase order line, and the board
picks the first one it finds in this order:

1. Line promise delivery date.
2. Line promise ship date.
3. Purchase-order header promise delivery date.
4. Purchase-order header promise ship date.
5. Line desired receive date.
6. Purchase-order header desired receive date.
7. Purchase-order order date.

Three problems come with that:

- Almost every line skips steps 1 and 2. Of the 949 open PO lines in MTMFG on 10/19/2026, 947 had
  no line-level promise date, and 388 had no promise date anywhere on the line or header. The
  board therefore shows a *requested* date, not a confirmed one, but the card text reads
  `Next material date: PO desired receive 12/11/2026`, which sounds confirmed.
- The card header only has room for one summary. When a part has any associated work order, the
  header shows the job summary and the next delivery date disappears from the card. You have to
  open `View incoming details` or print the board to see it.
- If a purchase order had neither a promise date nor a desired receive date, the board would fall
  back to the order date and present the day the PO was written as if it were the delivery date.
  No live line hits this today, but it is one data-entry gap away from printing a false date.

## What You See Now

The Incoming Material section always carries an `Expected delivery` block with one line per kind
of date:

```text
Incoming Material
Received 0   Ordered 13,000   Remaining 13,000
Expected delivery
Confirmed: 12/11/2026
Requested: 12/14/2026 (beyond 30-day highlight)
```

- `Confirmed` means the date comes from a PO or PO-line promise date — the supplier agreed to it.
- `Requested` means the date is an internal desired-receive date — we asked for it, nobody
  promised it.
- A past date with quantity still outstanding is marked `(overdue 5 days)`.
- A date beyond the highlight window is marked `(beyond 30-day highlight)`.
- If neither kind of date exists, the block says `No delivery date on the PO` instead of quietly
  showing the day the purchase order was written.

The purchase-order order date is no longer used as a delivery date anywhere: not on the card, not
in the incoming detail dialog, and not in the printed board. The order date is still available in
Infor Visual; it is simply not reported as an arrival date.

In the incoming detail dialog each line now shows a note beside it, for example
`Overdue by 3 days` or `Beyond highlight window`.

## Why It Matters

Receiving decides what to expedite from this date. Before this change a requested date that was
three weeks out looked identical to a confirmed promise, a past date looked future, and a purchase
order with no dates at all showed the day it was written.

## What You Should Know

- No open purchase order in MTMFG currently reaches the old order-date fallback, so no existing
  line changes its date today. The change removes the false date before it can appear.
- The date table on the printed board now lists each calendar date once instead of once per label
  that shares the date.

## See Also

- [Implementation note](05-Next-Delivery-Date-Accuracy-Implementation.md)
- [06 — Look Ahead actually filters inbound material](06-LookAhead-Window-For-Inbound-Material-User-Guide.md)
