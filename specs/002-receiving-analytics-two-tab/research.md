# Phase 0 Research: Receiving Analytics App Data Tab

**Feature**: [spec.md](./spec.md) · **Plan**: [plan.md](./plan.md)

Everything below was settled by reading the existing page, the MySQL schema and stored procedures,
and the Infor Visual query that the ERP tab already runs. Two entries resolve the spec's
`[NEEDS CLARIFICATION]` markers.

## R1. The two clarification numbers

**Decision**: The on-time target is **95 per cent**, held as an administrator-editable setting with a
default of 95. The comparison counts **one app label against one ERP receipt line**, aligning both
sides by received date and by the app's normalised category; an ERP category with no app equivalent
counts toward the ERP side only.

**Rationale**: Both markers were business numbers the spec already carried as working assumptions,
not open design questions, and both are needed before any query can be written. Holding the target as
a setting rather than a constant means FR-014 is satisfied without the figure appearing in code.
Stating the alignment rule up front stops the coverage and mix panels being built on an unstated
assumption, which is where they would silently disagree with the ERP.

**Alternatives considered**: Leaving both deferred to a later clarify step was rejected: the plan and
the stored-procedure contracts cannot be written without a comparison unit and a pass mark. A target
of zero as "no target" was rejected because it would paint every period green rather than removing
the colour, which contradicts FR-014.

## R2. Where the App Data read lives

**Decision**: A new `IService_Tool_ReceivingAnalyticsApp` service, a new
`Dao_Tool_ReceivingAnalyticsApp`, and four new stored procedures. The existing
`IService_Tool_ReceivingAnalytics` is not modified.

**Rationale**: The existing service depends only on `IService_InforVisual` and returns exactly two
datasets, history and forecast. Giving it a MySQL dependency would hand the ERP tab a data source it
never uses and would put two unrelated queries behind one type. `Module_ShipRec_Tools/Data` already
holds two DAOs built the same way, so a third is the module's own pattern rather than a new one. The
new service depends on the DAO for its three app panels and on `IService_InforVisual` for the
comparison, which is the only part of the tab that needs both sides.

**Alternatives considered**: Adding app methods to the existing service was rejected as it changes an
interface the ERP tab depends on, for no gain. Reading MySQL through `IService_MySQL_Receiving` was
rejected: that service is the receiving workflow's read and write surface, not a reporting one.

## R3. How the page becomes two tabs

**Decision**: `View_Tool_ReceivingAnalytics` keeps its whole existing ERP markup and wraps it in the
first `TabViewItem` of a `TabView`. The second item hosts a new
`View_Tool_ReceivingAnalyticsApp` control with its own view model. The tab view's selected index is
not bound; ERP Data is first and open by default by declaration order.

**Rationale**: Wrapping rather than extracting means none of the ERP tab's four hundred lines of
markup moves, so the regression check in SC-008 is about runtime behaviour only. One view model per
tab makes FR-003 and SC-009 structural: there is no shared state a filter change could reach, and no
`SelectedTabIndex` mirroring is needed because nothing reads the selection. WinUI keeps both
`TabViewItem` contents alive, so switching tabs does not rebuild either view model and each tab's
filters survive as the user left them.

**Alternatives considered**: Extracting the ERP content into its own `UserControl` was rejected: it
moves every line of the existing markup, and any transcription slip is a silent ERP regression.
Adding a host view model for one `SelectedTabIndex` property was rejected as scaffolding with no
consumer. A `TabView` bound to `SelectedIndex` was rejected because no behaviour depends on knowing
which tab is showing.

## R4. Where totals are computed

**Decision**: Four stored procedures, each returning the finest grain a panel needs, which is one row
per received day (plus category, unit or status where the panel needs it). Weekly and monthly
bucketing happens in C#.

**Rationale**: Per-day grain is the only grain that lets daily, weekly and monthly views come from
one fetch, so FR-007's period switch costs nothing and cannot disagree with itself between periods.
It also keeps the date arithmetic in one C# helper instead of being repeated across four procedures
with a period parameter each. The existing page already caches its dataset and re-renders on a
setting change, so this follows the idiom rather than inventing one.

**Alternatives considered**: Passing a period into the procedures and letting SQL group was rejected:
it forces a re-query on every period change, duplicates week and month logic four times, and makes
the three panels' bucket boundaries a thing that could drift apart. Returning raw receipt rows and
aggregating everything in C# was rejected: it moves tens of thousands of rows across the wire for
figures the database can total in place.

## R5. No new tables, views, functions or triggers

**Decision**: Every panel totals from columns already on `receiving_history`. This change adds four
stored procedures and nothing else to the schema.

**Rationale**: FR-031 and FR-032 permit new objects; they do not require them. Each figure was traced
to a column that already exists: `received_date` with `transaction_date` as the fallback for the
bucket key, `part_type` for the app category, `unit_of_measure` for the unit, `weight_quantity` with
`quantity` behind it for the total, `po_due_date` for the classification, `vendor_name` for the
supplier, `load_id` and `label_number` for the two counts, and `load_number IS NULL` for the
disclosure. A generated view would sit between the procedure and the table without removing any work
from either.

**Alternatives considered**: Building the totals into a generated view was rejected for the reason
above. A new summary table was rejected outright: it would need a trigger or a refresh job to stay
true, and a stale analytics table is worse than a live aggregate.

## R6. What a load is, and why loads and labels agree

**Decision**: A label is `COUNT(*)` over completed receipt rows. A load is
`COUNT(DISTINCT COALESCE(load_id, load_guid, CAST(id AS CHAR)))`. On today's data both figures are
equal, and the count of receipts whose `load_number` is null is disclosed beside them.

**Rationale**: This is the one finding that changes how the Activity panel will read. In the schema,
`label_number` is written from the same per-session counter as `load_number`, and `load_id` is a fresh
GUID on every row, so one completed receipt row is one load and one label at once. Counting
`DISTINCT load_number` would be worse than useless, because the number restarts at one in every
session and unrelated loads would merge. The two measures are still defined differently, so they
diverge the day the app starts putting several labels under one load, and FR-035's disclosure is what
stops a reader mistaking today's agreement for a defect.

**Alternatives considered**: Reporting a single "receipts" figure and dropping the load count was
rejected: FR-008 asks for both measures. Reporting the load count as the label count was rejected as
an outright lie in the code, even though the numbers match.

## R7. The single category translation

**Decision**: One static type,
`Module_ShipRec_Tools/Helpers/Translation_ReceivingAnalyticsCategory.cs`, owns the five ERP category
names, the three app category names, and the mapping in both directions. The App Data procedures pass
`part_type` through untranslated; normalisation to `Standard`, `Coil` or `Sheet` and to the ERP's
names happens only in that type. The ERP tab's own three copies of the ERP category literals are
folded onto it.

**Rationale**: FR-027 asks for exactly one place and both tabs reading it, and today the ERP
vocabulary exists three times over: as constants in the view model, as a label-and-colour table in
the series builder, and inside the SQL Server query. The SQL Server derivation stays where it is,
because changing it would change the ERP tab's figures. What consolidates is the mapping, which is
the thing that must never diverge. Keeping the app category names out of MySQL means the vocabulary
has one home rather than two.

**Alternatives considered**: Putting the translation in a MySQL view was rejected: the ERP side is
SQL Server, so a MySQL object could only ever hold half of a cross-database mapping. Deriving app
categories again from `part_id` in SQL rather than reading the stored `part_type` was rejected: it
would duplicate the rule that `Model_ReceivingLoad` already applies, and disagree with it the moment
either changes.

## R8. How quantity and unit are read

**Decision**: A quantity total is `SUM(COALESCE(weight_quantity, quantity))` within one selected
unit. The procedures pass `unit_of_measure` through exactly as stored, including blank; C# turns
blank into `EA` in one helper and uses it in the label.

**Rationale**: `weight_quantity` is the number the operator typed and `quantity` is that same number
rounded, so preferring the unrounded column is more faithful without pretending two measurements
exist; FR-023 forbids presenting them as a comparison and this does not. Reading the blank unit as
`EA` in C# rather than in SQL keeps that pinned string in one place, so a later change to how blanks
are read cannot leave the database and the screen disagreeing.

**Alternatives considered**: Summing `quantity` alone was rejected because it discards the operator's
decimals on units like LB and FT. Coalescing the unit inside each procedure was rejected: four copies
of the same rule.

## R9. The on-time target as configuration

**Decision**: A `ReceivingAnalytics.OnTimeTarget` key under the `ShipRecTools` settings category,
declared in `Module_Settings.Core/Defaults/settings.manifest.json` as a decimal with a default of 95
and administrator permission, read through `IService_ShipRecToolsSettings`.

**Rationale**: This is the mechanism the module already uses for every other administrator-editable
value, it is stored in the database rather than the build, and the manifest entry is what makes the
key visible and editable in the settings surface. FR-014 is therefore satisfied by adding a key, not
by building a settings screen. A blank or missing value reads as no target, which is FR-014's plain
number with no colour.

**Alternatives considered**: `appsettings.json` was rejected: changing it needs a file edit and a
restart on the shop floor terminal, which is not what "without a rebuild" means in practice. A
constant with the 95 in code was rejected outright.

## R10. The ERP side of the comparison

**Decision**: The Comparison panel calls the existing
`IService_InforVisual.GetReceivingAnalyticsHistoryAsync` with the App Data tab's own date bounds and
all five category scopes enabled, then translates the app's categories and normalises both sides to
the same five names. The ERP query itself is not modified.

**Rationale**: FR-028 requires identical dates and filters on both sides, and the App tab holds no
filter that the ERP query cannot express, so the date bounds are the whole of it. Reusing the query
keeps the read-only guard, the mock-data switch and the column mappings in one place. Modifying the
query was ruled out by FR-002 and FR-033.

**Known limits of the comparison, inherited and not corrected here.** The ERP query counts one PO
line, anchored on its last received date, so a line received on two days counts once, on the later
day. It also excludes consignment and internal orders. Correcting either would change the ERP Data
tab, which FR-002 forbids. The comparison therefore says what it is: app labels against ERP receipt
lines, on the same dates, with the ERP counted as the ERP counts. This belongs in the panel's
supporting text and in the data model's notes, not in a silent adjustment.

**Alternatives considered**: Writing a second ERP query tuned to match the app's counting was
rejected: two ERP queries would drift, and the second one would be the one nobody checks. Counting
ERP quantities instead of lines was rejected: FR-025's own assumption says only a count can be
compared without resolving units.

## R11. Panels, layout and the chart control

**Decision**: A 2x2 grid of four panels inside the App Data tab, using the existing LiveCharts
`CartesianChart`. Panel one splits into a loads-and-labels chart beside a quantity stack; panel four
splits into the coverage lines above the two-sided category mix. Panels two and three carry one chart
plus a readout strip.

**Rationale**: The default window is 1450 by 900, so each quadrant has roughly 700 by 380 pixels
before headers. Two side-by-side charts at that width stay legible; two stacked charts of the same
content do not. LiveCharts is already referenced and already used by this page, so adding a second
charting library for six charts would be a new dependency for no capability. The page makes no use of
`ObservableCollection` for chart data; it replaces the `Series` list wholesale, which is the
convention to keep.

**Alternatives considered**: Pie or donut charts for the category mix were rejected: the panel's job
is comparing two splits across the same five names, and side-by-side bars do that without a legend on
each. A scrolling page with larger charts was rejected by FR-006 and SC-001.

## R12. Period bucketing

**Decision**: Weekly buckets run Monday to Sunday and monthly buckets follow the calendar month. A
bucket with no completed receipts is present in the axis and empty in the data, never skipped.

**Rationale**: A gap in the axis reads as missing data rather than as a quiet week, and the spec's
"range that spans years" edge case needs the axis to stay continuous. Monday starts match the shift
week the dock works. The ERP tab's Day, Week and Month options only shift the range start today and
never aggregate, so the App Data tab cannot borrow that behaviour and must bucket for real.

**Alternatives considered**: Letting SQL group into buckets was rejected in R4. Sunday-start weeks
were rejected: they would put a Monday receipt in the previous week and disagree with how the dock
talks about a week.

## R13. Unit selection and the dominant unit

**Decision**: The quantity panels gate on a selected unit. The selector is populated from the units
the range actually contains, and it opens on the unit carrying the most completed receipts; where two
units tie, the panel opens on each, showing a separate line per unit, and the selector shows no
choice.

**Rationale**: This is the spec's stated assumption and the only reading that satisfies both FR-021's
no-cross-unit rule and FR-006's no-scrolling rule. Naming the unit in every total (FR-020) means the
figure never stands alone without saying what it counts.

**Alternatives considered**: Summing everything and labelling it "mixed" was rejected: it is not a
total. Hiding the whole quantity panel when units disagree was rejected: it removes a panel the spec
asks for.

## R14. Reprints are not filtered out

**Decision**: No `is_reprint` predicate is applied to `receiving_history`.

**Rationale**: `is_reprint` on a history row means that row was re-queued for a label reprint, not
that it is a duplicate. The completion procedure skips the insert for re-queued rows, so a reprinted
label never produces a second history row. Filtering the flag out would therefore delete real work
from the totals. The queue table, which is where reprints genuinely duplicate, is never read.

**Alternatives considered**: Excluding `is_reprint = 1` was rejected for the reason above; it is the
obvious-looking choice and it would undercount.

## R15. Failure and concurrency behaviour

**Decision**: The four app reads run concurrently in the service and any one of them failing fails
the app dataset as a whole, with the reason shown. The ERP comparison is a separate call whose
failure is reported on that panel alone. The view model keeps its previous figures visible while a
new load runs.

**Rationale**: SC-004 asks for all four panels inside three seconds on a thirty-day range, and four
independent reads issued together meet that without caching. A half-loaded app dataset is worse than
a stated failure, because three panels agreeing and one quietly missing is exactly the kind of lie
this tab exists to catch. The comparison is different: its ERP half depends on another server, and
the spec's edge case says the remaining panels keep working.

**Alternatives considered**: Failing the whole tab when the ERP side is unreachable was rejected
against that edge case. Loading the four reads one after another was rejected: it multiplies four
round trips where the panels share nothing.

## R16. What the tests prove

**Decision**: Unit tests cover the category translation in both directions and the bucketing helper;
the App Data view model is tested with a mocked service for the empty range, the no-target case and
the ERP-unavailable case; the service is tested with a mocked DAO for the dataset and comparison
shapes. One integration test exercises the four procedures against the test database.

**Rationale**: These are the places where a wrong answer is silent. A translation that maps one
category wrongly, or a bucket that drops a day, produces a plausible-looking chart. The procedures
need a real database because their correctness is in the SQL, not in the C#.

**Alternatives considered**: Testing the series construction was rejected: the chart library owns
that, and asserting on `ISeries` shapes proves nothing a user would notice.
