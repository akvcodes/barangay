import re
import sqlite3
import os

print("Reading MySQL dump...")
try:
    with open('my_database_dump.sql', 'r', encoding='utf-8') as f:
        sql = f.read()
except UnicodeDecodeError:
    with open('my_database_dump.sql', 'r', encoding='latin-1') as f:
        sql = f.read()

print("Cleaning MySQL-specific syntax...")

sql = re.sub(r'/\*!.*?\*/', '', sql, flags=re.DOTALL)
sql = re.sub(r'/\*.*?\*/', '', sql, flags=re.DOTALL)
sql = re.sub(r'(?m)^\s*--.*$', '', sql)
sql = re.sub(r'(?mi)^\s*SET\s+.*?;', '', sql)
sql = re.sub(r'(?mi)^\s*(UN)?LOCK\s+TABLES.*?;?$', '', sql)
sql = sql.replace('`', '')

# --- Replace ALL binary blobs with NULL (photos etc.) ---
sql = re.sub(r"_binary\s*'(?:[^'\\]|\\.)*'", "NULL", sql, flags=re.DOTALL)
# Some dumps use 0x... form
sql = re.sub(r"\b0x[0-9A-Fa-f]+\b", "NULL", sql)

# Replace current_timestamp and ON UPDATE clauses
sql = re.sub(r'DEFAULT\s+current_timestamp\s*\(\s*\)', 'DEFAULT NULL', sql, flags=re.IGNORECASE)
sql = re.sub(r'DEFAULT\s+current_timestamp\b(?!\s*\()', 'DEFAULT NULL', sql, flags=re.IGNORECASE)
sql = re.sub(r'\s+ON\s+UPDATE\s+CURRENT_TIMESTAMP(\(\d*\))?', '', sql, flags=re.IGNORECASE)

# Remove CONSTRAINT and FOREIGN KEY clauses (multi-line tolerant)
sql = re.sub(
    r"CONSTRAINT\s+\w+\s+FOREIGN\s+KEY\s*\([^)]*\)\s*REFERENCES\s+\w+\s*\([^)]*\)"
    r"(\s+ON\s+(DELETE|UPDATE)\s+(CASCADE|SET\s+NULL|SET\s+DEFAULT|RESTRICT|NO\s+ACTION))*",
    '', sql, flags=re.IGNORECASE | re.DOTALL)
sql = re.sub(
    r"FOREIGN\s+KEY\s*\([^)]*\)\s*REFERENCES\s+\w+\s*\([^)]*\)"
    r"(\s+ON\s+(DELETE|UPDATE)\s+(CASCADE|SET\s+NULL|SET\s+DEFAULT|RESTRICT|NO\s+ACTION))*",
    '', sql, flags=re.IGNORECASE | re.DOTALL)

# Line-by-line cleaning inside CREATE TABLE
lines = sql.split('\n')
clean_lines = []
in_create = False
for line in lines:
    stripped = line.strip()
    if re.match(r'^CREATE\s+TABLE', stripped, re.IGNORECASE):
        in_create = True
        clean_lines.append(line)
        continue
    if in_create and stripped.startswith(')'):
        in_create = False
        line = re.sub(r'\)\s*ENGINE\s*=[^;]*;', ');', line, flags=re.IGNORECASE)
        clean_lines.append(line)
        continue
    if in_create:
        if re.match(r'^(PRIMARY|UNIQUE|FULLTEXT|SPATIAL)?\s*KEY\b', stripped, re.IGNORECASE):
            continue
        if re.match(r'^CONSTRAINT\b', stripped, re.IGNORECASE):
            continue
        if re.match(r'^FOREIGN\s+KEY\b', stripped, re.IGNORECASE):
            continue
        if re.match(r'^(UNIQUE|FULLTEXT|SPATIAL)\s+(INDEX|KEY)\b', stripped, re.IGNORECASE):
            continue
        if re.match(r'^USING\s+(BTREE|HASH)', stripped, re.IGNORECASE):
            continue
        line = re.sub(r'\s+USING\s+(BTREE|HASH)', '', line, flags=re.IGNORECASE)
        # Strip inline constraint/foreign key phrases on this line
        line = re.sub(r'\bCONSTRAINT\s+\w+\s+', '', line, flags=re.IGNORECASE)
        line = re.sub(r'\bFOREIGN\s+KEY\s*\([^)]*\)\s*REFERENCES\s+\w+\s*\([^)]*\)'
                      r'(\s+ON\s+(DELETE|UPDATE)\s+\w+(\s+\w+)?)*',
                      '', line, flags=re.IGNORECASE)
    clean_lines.append(line)
sql = '\n'.join(clean_lines)

# Convert data types
sql = re.sub(r'\b(tiny|small|medium|big)?int\s*\(\s*\d+\s*\)', 'INTEGER', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(tiny|small|medium|big)?int\b', 'INTEGER', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(varchar|char|nvarchar)\s*\(\s*\d+\s*\)', 'TEXT', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(long|medium|tiny)?text\b', 'TEXT', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(decimal|numeric|double|float)\s*\(\s*\d+\s*,\s*\d+\s*\)', 'REAL', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(decimal|numeric|double|float)\b', 'REAL', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(datetime|timestamp|date|time)\b', 'TEXT', sql, flags=re.IGNORECASE)
sql = re.sub(r'\b(long|medium|tiny)?blob\b', 'BLOB', sql, flags=re.IGNORECASE)
sql = re.sub(r'\benum\s*\([^)]*\)', 'TEXT', sql, flags=re.IGNORECASE)
sql = re.sub(r'\bset\s*\([^)]*\)', 'TEXT', sql, flags=re.IGNORECASE)
sql = re.sub(r'\byear\b', 'INTEGER', sql, flags=re.IGNORECASE)

sql = re.sub(r'\s+AUTO_INCREMENT\b', '', sql, flags=re.IGNORECASE)
sql = re.sub(r'\s+UNSIGNED\b', '', sql, flags=re.IGNORECASE)
sql = re.sub(r'\s+ZEROFILL\b', '', sql, flags=re.IGNORECASE)
sql = re.sub(r'\s+CHARACTER SET\s+\w+', '', sql, flags=re.IGNORECASE)
sql = re.sub(r'\s+COLLATE[= ]\w+', '', sql, flags=re.IGNORECASE)
# Strip any lingering "ON UPDATE ..." or "ON DELETE ..." fragments
sql = re.sub(r'\bON\s+(UPDATE|DELETE)\s+\w+(\s+\w+)?', '', sql, flags=re.IGNORECASE)
sql = re.sub(r',\s*\)', ')', sql)

sql = sql.replace("'0000-00-00 00:00:00'", "NULL")
sql = sql.replace("'0000-00-00'", "NULL")
sql = sql.replace("0000-00-00 00:00:00", "NULL")

# --- Splitter that respects quoted strings ---
def split_sql(sql_text):
    statements = []
    current = []
    i = 0
    n = len(sql_text)
    while i < n:
        c = sql_text[i]
        if c == "'":
            j = i + 1
            buf = ["'"]
            while j < n:
                if sql_text[j] == '\\' and j + 1 < n:
                    buf.append(sql_text[j])
                    buf.append(sql_text[j+1])
                    j += 2
                    continue
                if sql_text[j] == "'":
                    buf.append("'")
                    j += 1
                    break
                buf.append(sql_text[j])
                j += 1
            current.append(''.join(buf))
            i = j
            continue
        if c == ';':
            statements.append(''.join(current))
            current = []
            i += 1
            continue
        current.append(c)
        i += 1
    if current:
        statements.append(''.join(current))
    return statements

statements = split_sql(sql)

if os.path.exists('BarangayDB.sqlite'):
    os.remove('BarangayDB.sqlite')

print("Connecting to SQLite...")
conn = sqlite3.connect('BarangayDB.sqlite')
cursor = conn.cursor()

success = 0
failed = 0
errors = []
for i, stmt in enumerate(statements):
    stmt = stmt.strip()
    if not stmt:
        continue
    try:
        cursor.execute(stmt)
        success += 1
    except Exception as e:
        failed += 1
        errors.append((i, str(e)[:200], stmt[:300]))
conn.commit()

cursor.execute("SELECT name FROM sqlite_master WHERE type='table'")
tables = cursor.fetchall()

print("\n--- Table row counts ---")
for t in tables:
    try:
        cursor.execute(f"SELECT COUNT(*) FROM {t[0]}")
        count = cursor.fetchone()[0]
        print(f"  {t[0]}: {count} rows")
    except Exception as e:
        print(f"  {t[0]}: (error: {e})")
conn.close()

print(f"\nSuccess: {success} statements")
print(f"Failed:  {failed} statements")
print(f"Tables created: {len(tables)}")

if errors:
    print("\n--- First 5 errors ---")
    for i, err, stmt in errors[:5]:
        print(f"\n#{i}: {err}")
        print(f"   {stmt[:200]}")

print("\nDone!")