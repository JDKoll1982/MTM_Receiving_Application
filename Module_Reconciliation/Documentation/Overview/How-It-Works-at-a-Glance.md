# How It Works at a Glance

Last Updated: 2026-11-12

## The one day

Pick a day. The module reads that day's receiving rows from the label queue and the history archive, and drops reprint rows from the set while counting them.

The stepper only moves to days that hold receiving rows. It asks a cheap question, not a full preview, so it stays quick. Today is always one button away.

## The group

Rows that share a purchase order, a part and a receipt day compete for the same movement evidence, so they form one group. Two purchase orders of one part on one day are two groups. A receipt with no purchase order is grouped by part, day and vendor instead, and is never skipped for lacking a purchase order.

## The movements

For the group's part, the module reads the Infor Visual transfer movements around the receipt day. It keeps only the movements it can reach: those that left a location the group already knows about, or those booked against the group's purchase order. It repeats until nothing more is reachable, so a transfer that chains through a staging location is followed to its end.

Direction comes from the transaction type, never from the order of transaction ids. The outbound leg is the source, the inbound leg is the destination.

## The buckets

Each location that received net quantity becomes a destination bucket. A location that forwarded everything nets to zero and disappears. One that kept some quantity stays a bucket for the remainder.

## The allocation

Rows are placed largest first, then by load number, then by load id. A row whose saved location is already a bucket with enough left over is bound there first, so a line that is already right is never moved. A row that finds no bucket is not written.

## The proof

Two checks decide whether a line may be written.

1. Every destination bucket must be filled exactly by the rows bound to it. Nothing left over, nothing too many.
2. Whatever is left in the receiving location must equal that location's balance as of the receipt day. That balance is reconstructed as today's on-hand minus everything that moved at the location after the receipt, because Infor Visual only shows a live figure.

A line is writable only when both checks pass. A line whose saved location already matches is reported as unchanged and never enters the review.

## The write

Accepting a line re-runs the proof over a freshly read balance and then writes through one stored procedure. One line, one write. There is no bulk apply.

## The counts

Every row loaded gets exactly one outcome. The seven counts in the summary are those outcomes, so they always add up to the rows loaded.
