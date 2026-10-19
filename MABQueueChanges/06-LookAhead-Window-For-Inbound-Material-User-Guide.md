# Look Ahead Is Now a Highlight Window

Last Updated: 2026-10-19

Status: **Applied** (Option B). The control is now called `Highlight window`, the empty-state text
no longer promises a filter, and dates outside the window carry a visible marker.

The `Highlight window` selector (30 / 60 / 90 / All) changes ordering and wording. It deliberately
does not hide anything, so long-range visibility is kept.

## What You See Before

With `Look Ahead: 30` selected, a part whose only inbound purchase order arrives in five months
still shows that purchase order:

```text
Incoming Material
Received 0   Ordered 13,000   Remaining 13,000
1 purchase order, 1 PO line
```

and the card can be sorted on that far-off date. Nothing on the card explains that the material is
outside the window you picked.

When no inbound quantity exists, the empty state says:

```text
No qualifying incoming material was found for the selected look-ahead window.
```

That sentence promises a filter that is not applied. The window is also not applied to the
Associated Parts list; it only decides whether a future job is grouped as "inside" or
"outside" the window for ordering purposes.

## What You See Now

With `Highlight window: 30` selected, a part whose only inbound purchase order arrives in five
months still shows that purchase order, and the date is labeled so you know it is outside the
window you picked:

```text
Incoming Material
Received 0   Ordered 13,000   Remaining 13,000
Expected delivery
Requested: 02/14/2027 (beyond 30-day highlight)
```

In the incoming detail dialog the same line shows a note under its date label:

```text
Beyond highlight window
```

A future work order that sits beyond the window is marked the same way on its Associated Parts row:

```text
Future Run
03/15/2027
beyond 30-day highlight
```

When no inbound quantity exists at all, the card now says:

```text
No inbound material found for this part.
```

## Why It Matters

`Remaining` is read as "what is coming", and management uses it to decide whether to expedite.
The board now tells you plainly when a balance arrives later than the window you selected, instead
of leaving you to compare dates by hand or wonder why the empty state mentioned a filter.

## What You Should Know

- Nothing is hidden by the window. `All` therefore looks the same as `90` except that no
  beyond-window markers are shown.
- The markers are informational only; they do not change quantities, totals, or the run date.
- If you later want the quantity totals limited to the window, that is a separate change and would
also change what `Remaining` means.

## See Also

- [Implementation note](06-LookAhead-Window-For-Inbound-Material-Implementation.md)
- [05 — Next delivery date tells you what it really means](05-Next-Delivery-Date-Accuracy-User-Guide.md)
