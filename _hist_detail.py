import pymysql
from collections import Counter

LIVE = "mtm_receiving_application"
TEST = "mtm_receiving_application_test"
TABLES = ["dunnage_history", "receiving_history"]

c = pymysql.connect(host="172.16.1.104", port=3306, user="root", password="root",
                    charset="utf8mb4", cursorclass=pymysql.cursors.DictCursor)
cur = c.cursor()


def _n(v):
    if v is None:
        return "<NULL>"
    if isinstance(v, bytes):
        return v.hex()
    if hasattr(v, "isoformat"):
        return v.isoformat()
    return str(v)


def schema(db, table):
    cur.execute(
        "SELECT COLUMN_NAME AS name, COLUMN_TYPE AS typ, IS_NULLABLE AS nul, "
        "COLUMN_KEY AS keycol, EXTRA AS extra "
        "FROM information_schema.COLUMNS "
        "WHERE TABLE_SCHEMA=%s AND TABLE_NAME=%s ORDER BY ORDINAL_POSITION", (db, table))
    return cur.fetchall()


def fetch(db, table, cols):
    if not cols:
        return []
    sel = ", ".join("`%s`" % col for col in cols)
    cur.execute(f"SELECT {sel} FROM `{db}`.`{table}`")
    return cur.fetchall()


def is_excl(r):
    ex = (r.get("extra") or "").lower()
    return "auto_increment" in ex or "generated" in ex


for table in TABLES:
    lmap = {r["name"]: r for r in schema(LIVE, table)}
    tmap = {r["name"]: r for r in schema(TEST, table)}
    only_test = sorted(set(tmap) - set(lmap))
    only_live = sorted(set(lmap) - set(tmap))
    shared = sorted(set(lmap) & set(tmap))
    content = [col for col in shared if not (is_excl(lmap[col]) or is_excl(tmap[col]))]

    lrows = fetch(LIVE, table, content)
    trows = fetch(TEST, table, content)

    def key(row):
        return tuple((col, _n(row[col])) for col in content)

    lk = Counter(key(r) for r in lrows)
    tk = Counter(key(r) for r in trows)
    overlap = sorted(k for k in lk if k in tk)
    toadd = sorted(k for k in lk if k not in tk)
    dup_live = sum(1 for k, n in lk.items() if n > 1)
    dup_test = sum(1 for k, n in tk.items() if n > 1)

    print("=" * 100)
    print(f"TABLE {table}")
    print(f"  live cols: " + ", ".join(
        f"{n}{'(auto)' if is_excl(lmap[n]) else ''}" for n in sorted(lmap)))
    print(f"  test cols: " + ", ".join(
        f"{n}{'(auto)' if is_excl(tmap[n]) else ''}" for n in sorted(tmap)))
    if only_test:
        print(f"  columns ONLY in test (live lacks): {only_test}")
    if only_live:
        print(f"  columns ONLY in live (test lacks): {only_live}")
    print(f"  shared content columns used: {content}")
    print(f"  rows: live={len(lrows)}  test={len(trows)}")
    print(f"  duplicate content rows WITHIN live={dup_live}  within test={dup_test}")
    print(f"  live content rows ALREADY in test (exact content match): {len(overlap)}")
    print(f"  live content rows MISSING from test (would be merged): {len(toadd)}")
    if toadd:
        print("  sample rows that would be merged (first 3):")
        for k in toadd[:3]:
            print("    " + " | ".join(f"{col}={val}" for (col, val) in k))
    print()
