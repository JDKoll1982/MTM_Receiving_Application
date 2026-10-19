# Inbound Purchase Orders With No Warehouse Now Show

Last Updated: 2026-10-19

Four coil part numbers showed `No inbound material found for this part` while over 1.4 million
pounds of that coil was on order. The board was hiding purchase orders that carry no warehouse.

## What You Saw Before

Searching by part for `MMC0000364`, `MMC0000366`, `MMC0000659`, or `MMC0000850`, with the highlight
window set to `All`, the Incoming Material section stayed empty:

```text
Incoming Material
No inbound material found for this part.
```

Yet Infor Visual held 17 open purchase-order lines with a balance for those four parts:

| Part | Open lines | Remaining balance |
| ---- | ---------- | ----------------- |
| MMC0000364 | 6 | 652,000 |
| MMC0000366 | 5 | 381,534 |
| MMC0000659 | 4 | 346,197 |
| MMC0000850 | 2 | 95,000 |

The same was true far beyond those four parts: 899 of the 969 open purchase-order lines in MTMFG
have no warehouse on the line or on the order, and every one of them was invisible.

## Why It Happened

The board only accepted a purchase-order line when its warehouse matched the warehouse you were
looking at. Coil purchase orders are normally written without a warehouse, because the receiving
location decides where the coil lands. Because neither the line nor the order header had a
warehouse, the line matched no warehouse at all and was discarded. Parts whose receiving location
did carry a warehouse matched nothing else, so their cards came up empty.

## What You See Now

```text
Incoming Material
Received 0   Ordered 40,000   Remaining 40,000
1 purchase order, 1 PO line
Expected delivery
Confirmed: 11/02/2026
```

The four parts now report 6, 5, 4, and 2 open lines respectively.

Inbound material is no longer filtered by warehouse at all. Warehouse `002` is the only warehouse
on the board, the board is already limited to `002` by configuration, and the parts being searched
are already the parts at that location (or the part you typed). A purchase-order line is therefore
included whenever it belongs to a part on the card and still has a balance.

## Blanket Releases Now Show Their Date

Blanket purchase orders used to be reduced to a `Last received on` history line, and a blanket
release with a balance but no receipt ever shown no date at all. That affected 87 open lines. Those
lines now carry their promise or requested date, and the last-received date is kept alongside it as
history.

## What You Should Know

- The card now shows inbound material for far more parts than before. Expect the incoming totals on
  a location search to be larger, because they include material that previously matched no
  warehouse. For example, location `RECV` went from 0 to 10 inbound lines.
- The only non-`002` warehouse values found anywhere are a handful of zero-quantity stock rows and
  two closed `DC-FG` purchase-order lines, so removing the filter cannot mix in material from
  another warehouse.

## See Also

- [Implementation note](08-Inbound-Without-Warehouse-Implementation.md)
- [05 — Next delivery date tells you what it really means](05-Next-Delivery-Date-Accuracy-User-Guide.md)
