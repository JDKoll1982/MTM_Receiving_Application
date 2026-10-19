# Overdue and Due Today Labels

Last Updated: 2026-10-19

Open work that has already blown its date used to be called a past job, which sounds like
finished history. The board now says `Overdue` and `Due Today` instead.

## What You Saw Before

For `MMC0000990` (Coil, 7Ga X 22.200) the card reported:

```text
Past Job: WO-074826 / SRE-30481 on 10/16/2026
```

`WO-074826` was released, scheduled to run 10/15-10/16, and had issued zero of the 16,339 units
of coil the job needs. It was not a past job. It was three days overdue and still starving.

A job scheduled to start exactly today also had no label it could land on, because the date test
only counted dates strictly after today. Those rows fell to the bottom with the undated ones.

## What You See Now

```text
Overdue: WO-074826 / SRE-30481 on 10/16/2026
Due Today: WO-075089 / 13042-0804 on 10/19/2026
```

Labels you can now see on an Associated Parts row:

| Label | Meaning |
| ----- | ------- |
| `Currently Running` | The run window covers today. |
| `Overdue` | Still open, but the date has already passed. |
| `Due Today` | Still open and due today. |
| `Future Run` | Still open and due later. |
| `No scheduled job` | Still open but Infor Visual has no usable date. |
| `Past Job` | Closed or cancelled — history, not demand. |

Only `Past Job` means the job is done.

## Why It Matters

`Overdue` and `Past Job` lead to opposite decisions. One means "chase this, the line needs it
now". The other means "nothing to do here". Before this change both cases printed the same words.

## See Also

- [01 — Closed jobs no longer appear as the next run](01-Closed-Jobs-Shown-As-Upcoming-User-Guide.md)
- [02 — Jobs without a schedule now show a real date](02-Missing-Job-Run-Date-Fallback-User-Guide.md)
