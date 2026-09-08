import re
import sys
import pymysql

LIVE = "mtm_receiving_application"        # read-only source
TEST = "mtm_receiving_application_test"   # write target (user-approved)

HISTORY_PATTERN = re.compile(
    r"(history|activity|label_data|_session|_item|_run|waitlist|line_data)$",
    re.IGNORECASE,
)
EXECUTE = "--execute" in sys.argv

c = pymysql.connect(host="172.16.1.104", port=3306, user="root", password="root",
                    charset="utf8mb4", autocommit=False, cursorclass=pymysql.cursors.DictCursor)
cur = c.cursor()


def tables_in(db):
    cur.execute("SELECT TABLE_NAME AS name FROM information_schema.TABLES "
                "WHERE TABLE_SCHEMA=%s AND TABLE_TYPE='BASE TABLE'", (db,))
    return {r["name"] for r in cur.fetchall()}


def meta(db, table):
    cur.execute("SELECT COLUMN_NAME AS name, EXTRA AS extra FROM information_schema.COLUMNS "
                "WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s", (db, table))
    cols, gen, auto = [], set(), set()
    for r in cur.fetchall():
        cols.append(r["name"])
        ex = (r["extra"] or "").lower()
        if "auto_increment" in ex:
            auto.add(r["name"])
        if "generated" in ex:
            gen.add(r["name"])
    return cols, gen, auto


def unique_key_cols(db, table, content):
    """Pick the natural unique key: a unique index whose columns are all writable
    content columns (never auto-increment/generated). None => no natural key."""
    cur.execute(
        "SELECT INDEX_NAME AS ix, COLUMN_NAME AS col, SEQ_IN_INDEX AS seq "
        "FROM information_schema.STATISTICS WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s "
        "AND NON_UNIQUE=0 ORDER BY INDEX_NAME, SEQ_IN_INDEX", (db, table))
    by_ix = {}
    for r in cur.fetchall():
        by_ix.setdefault(r["ix"], []).append((r["seq"], r["col"]))
    allowed = set(content)
    for ix in sorted(by_ix):
        cols_ix = [col for _, col in sorted(by_ix[ix])]
        if cols_ix and all(col in allowed for col in cols_ix):
            return cols_ix
    return None


def _n(v):
    if v is None:
        return "<NULL>"
    if isinstance(v, bytes):
        return v.hex()
    if hasattr(v, "isoformat"):
        return v.isoformat()
    return str(v)


def fetch(db, table, colnames):
    if not colnames:
        return []
    sel = ", ".join("`%s`" % col for col in colnames)
    cur.execute(f"SELECT {sel} FROM `{db}`.`{table}`")
    return cur.fetchall()


def ktuple(row, kcols):
    return tuple(_n(row[col]) for col in kcols)


live_tables = tables_in(LIVE)
test_tables = tables_in(TEST)
targets = sorted(t for t in live_tables & test_tables if HISTORY_PATTERN.search(t))
# parents before children for FK-linked scanner tables (session -> item)
_priority = {"receiving_scanner_session": 0, "receiving_scanner_item": 1}
targets.sort(key=lambda t: (_priority.get(t, 2), t))

print(f"mode: {'EXECUTE' if EXECUTE else 'WHATIF (read-only)'}   "
      f"source={LIVE} target={TEST}")
print()
hdr = (f"{'table':<34}{'key':<28}{'live':>6}{'toAdd':>7}{'dupSkip':>8}{'ins':>6}{'err':>5}")
print(hdr)
print("-" * 96)

tot_add = tot_dup = tot_ins = tot_err = 0
for table in targets:
    lc, lg, la = meta(LIVE, table)
    tc, tg, ta = meta(TEST, table)
    shared = sorted(set(lc) & set(tc))
    excluded = lg | la | tg | ta
    content = [col for col in shared if col not in excluded]
    if not content:
        continue
    key_cols = unique_key_cols(TEST, table, content) or content
    lrows = fetch(LIVE, table, content)
    trows = fetch(TEST, table, content)
    tkeys = {ktuple(r, key_cols) for r in trows}
    missing = [r for r in lrows if ktuple(r, key_cols) not in tkeys]
    dups = len(lrows) - len(missing)

    ins = err = 0
    if EXECUTE and missing:
        ins_sql = "INSERT INTO `%s`.`%s` (%s) VALUES (%s)" % (
            TEST, table,
            ", ".join("`%s`" % col for col in content),
            ", ".join(["%s"] * len(content)))
        seen = set()
        for row in missing:
            k = ktuple(row, key_cols)
            if k in seen:
                dups += 1
                continue
            seen.add(k)
            cur.execute("SAVEPOINT sp")
            try:
                cur.execute(ins_sql, [row[col] for col in content])
                ins += 1
            except pymysql.err.IntegrityError:
                cur.execute("ROLLBACK TO SAVEPOINT sp")
                err += 1
        c.commit()
        # idempotency recheck
        trows2 = fetch(TEST, table, content)
        tkeys2 = {ktuple(r, key_cols) for r in trows2}
        still = sum(1 for r in lrows if ktuple(r, key_cols) not in tkeys2)
        if still:
            print(f"  !! residual (would still be missing after run): {still}")

    key_disp = ",".join(key_cols)
    print(f"{table:<34}{key_disp:<28}{len(lrows):>6}{len(missing):>7}{dups:>8}"
          f"{ins:>6}{err:>5}")
    tot_add += len(missing)
    tot_dup += dups
    tot_ins += ins
    tot_err += err

print("-" * 96)
print(f"TOTALS -> live={tot_add+tot_dup}  genuinelyNew(toAdd)={tot_add}  "
      f"alreadyInTest(skip as dup)={tot_dup}  inserted={tot_ins}  errors(skip)={tot_err}")
