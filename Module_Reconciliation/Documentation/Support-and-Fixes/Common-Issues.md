# Common Issues

Last Updated: 2026-11-12

## The day will not load

The application database is unreachable. The page states the reason and produces no lines. Check the database connection, then reload the day.

## Every line on a day says NotFound

No Infor Visual transfer was found for the part around that day. The lines are logged with the part, the day and the group, and counted in the summary. Confirm in Infor Visual that the material was actually transferred; if it was not, the saved location is already right and nothing needs changing.

## Many lines say Not Enough or Extra Surplus

Two causes are common.

1. **The quantity field.** The proof uses the integer quantity column. Weight-based parts whose entered weight rounds differently from the movement will diverge. This is a known limit of the current choice and is measured on a live day; see `Decisions.md`.
2. **Another receipt at the same location.** The residual check compares the whole location balance, so stock from a different receipt at the same location makes the residual disagree.

## The Needs Attention button is greyed out

There are no unresolved lines on the day. It is disabled whenever the count is zero, by design.

## A line will not accept

Read the reason in the pane. The usual ones are a destination that ran negative, a bucket that no longer fills exactly, or a residual that does not agree. The proof is re-run when you accept, so a balance that changed since the preview is enough to refuse the write.

## Nobody can be seen as the operator

Every write records the signed-in Windows user as the operator. There is no supervisor gate in this version; a role gate is a future change and would be enforced in the stored procedure.
