# About This Module

Last Updated: 2026-11-12

The Receiving Validator checks where material that was received actually went, and lets one person correct the saved location one line at a time.

## What it is for

Receiving records where a load was put when it was booked in. That location is a guess made at the dock. The truth is in Infor Visual: the material is transferred to another location, or it stays where it was put.

This module compares the two. It works one calendar day at a time, reads the day's receiving rows from the application database, reads the transfer movements for the parts on that day from Infor Visual, and proposes a destination for each line.

## What makes it different from the old screen

The old reconciliation screen ran inside Receiving, worked over a whole span, and could only tell an operator that something was "ambiguous". This module:

- works exactly one day at a time, so a backlog is cleared a day at a time rather than in one long pass
- writes only when the material can be fully accounted for, checked by a two-part quantity proof
- names what went wrong: extra surplus, not enough, not found, skipped or error
- writes through one stored procedure of its own, so it shares no code with Receiving

## Where it lives

One navigation entry in the Reporting group, labelled **Receiving Validator** with **from Infor Visual** beneath it. The entry opens a launcher page, which opens the working page.

## What it does not do

- It does not print or verify labels.
- It does not export anything.
- It does not keep a run or change history: there are no reconciliation tables, and runs are recorded in logs only.
- It does not correct a line automatically. A person accepts every write.
