import sqlite3
import re
import os

DB_FILE = 'BarangayDB.sqlite'
if not os.path.exists(DB_FILE):
    print(f"ERROR: {DB_FILE} not found in this folder.")
    exit(1)

conn = sqlite3.connect(DB_FILE)
cursor = conn.cursor()

cursor.execute("SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'")
tables = cursor.fetchall()

fixed_count = 0

for name, schema in tables:
    if not schema:
        continue
    if re.search(r'PRIMARY KEY', schema, re.IGNORECASE):
        continue

    cursor.execute(f"PRAGMA table_info('{name}')")
    cols = cursor.fetchall()
    id_col = None
    for col in cols:
        cid, col_name, col_type, not_null, dflt, pk = col
        if pk:
            break
        if col_type.upper().startswith('INTEGER') and (not_null or col_name.lower().startswith('id')):
            id_col = col_name
            break

    if not id_col:
        print(f"  {name}: no id column detected, skipping")
        continue

    print(f"  {name}: making '{id_col}' a PRIMARY KEY...")

    new_schema = schema
    new_schema = re.sub(
        r'("?' + re.escape(id_col) + r'"?)\s+INTEGER(\s+NOT\s+NULL)?',
        r'\1 INTEGER PRIMARY KEY',
        new_schema, count=1, flags=re.IGNORECASE)
    if 'PRIMARY KEY' not in new_schema:
        new_schema = re.sub(
            r'("?' + re.escape(id_col) + r'"?)\s+\w+',
            r'\1 INTEGER PRIMARY KEY',
            new_schema, count=1, flags=re.IGNORECASE)

    new_schema_renamed = re.sub(
        r'CREATE\s+TABLE\s+"?' + re.escape(name) + r'"?',
        f'CREATE TABLE "{name}__new"',
        new_schema, count=1, flags=re.IGNORECASE)

    try:
        cursor.execute(new_schema_renamed)
        cursor.execute(f'INSERT INTO "{name}__new" SELECT * FROM "{name}"')
        cursor.execute(f'DROP TABLE "{name}"')
        cursor.execute(f'ALTER TABLE "{name}__new" RENAME TO "{name}"')
        fixed_count += 1
    except Exception as e:
        print(f"  ERROR fixing {name}: {e}")
        conn.rollback()
        conn.close()
        exit(1)

conn.commit()
conn.close()
print(f"\nDone! Fixed {fixed_count} table(s).")