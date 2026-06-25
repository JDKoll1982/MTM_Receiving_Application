# MMC Coil Reports

Date: 2026-06-24

This report contains the commands used to query the `receiving_history` table for parts matching "MMC", the outputs captured during an automated run attempt, and next steps to obtain full result rows.

## Commands run (recommended — copy/paste into PowerShell)

Run the quantity-ranked report:

```powershell
C:\MAMP\bin\mysql\bin\mysql.exe -h 172.16.1.104 -u root -proot -B mtm_receiving_application -e "SELECT part_id AS part_number, COALESCE(part_description, part_id) AS description, SUM(quantity) AS total_received_qty FROM receiving_history WHERE LOWER(COALESCE(part_description, part_id)) LIKE '%mmc%' GROUP BY part_id, part_description ORDER BY total_received_qty DESC LIMIT 25;"
```0

Run the skid-ranked report:

```powershell
C:\MAMP\bin\mysql\bin\mysql.exe -h 172.16.1.104 -u root -proot -B mtm_receiving_application -e "SELECT part_id AS part_number, COALESCE(part_description, part_id) AS description, SUM(COALESCE(coils_on_skid, CEIL(quantity/NULLIF(packages_per_load,0)))) AS total_skids, SUM(quantity) AS total_qty FROM receiving_history WHERE LOWER(COALESCE(part_description, part_id)) LIKE '%mmc%' GROUP BY part_id, part_description ORDER BY total_skids DESC LIMIT 25;"
```

Run a sample-rows query (verify example rows):

```powershell
C:\MAMP\bin\mysql\bin\mysql.exe -h 172.16.1.104 -u root -proot -B mtm_receiving_application -e "SELECT PART_ID, part_description, quantity, coils_on_skid, packages_per_load, transaction_date FROM receiving_history WHERE LOWER(COALESCE(part_description, part_id)) LIKE '%mmc%' ORDER BY transaction_date DESC LIMIT 20;"
```

## Automated run attempt (what I tried)

- I attempted to execute the project's phpMyAdmin-friendly SQL scripts via the MAMP `mysql.exe` client from this environment and capture outputs to `reports/`.
- Commands executed produced only the mysql warning about using a password on the command line; no result rows were captured by the automation run here. Example captured stderr snippet:

```
mysql: [Warning] Using a password on the command line interface can be insecure.
```

I also attempted several quoting/streaming variants (piping the SQL file, invoking with `-e`, running via PowerShell Start-Process). Those runs returned the same warning-only output in the captured files.

## Why this happened (likely)

- Shell quoting and Windows cmd/PowerShell parsing made reliably sending multi-statement SQL or literal patterns to `mysql.exe` from this automated environment fragile.
- phpMyAdmin itself executes the scripts successfully when pasted into its SQL box; earlier manual runs from the MAMP client returned results (you reported that). The automated runner in this session could not reliably reproduce that interactive execution.

## Next steps (pick one)

1. I run the queries from this environment again using a PowerShell script file (avoids inline quoting), capture results, and update this report with actual rows. (I can do this if you approve.)
2. You run the three commands above in PowerShell/Command Prompt on your machine and paste the results here; I will convert them into a formatted table and commit.
3. I produce alternate SQL files that use a literal filter placeholder and a tiny PowerShell runner script that you can run locally to produce the report files; I will commit the runner and an example output template.

Which option do you prefer? If you want me to retry (1), say "Please retry" and I'll run a PowerShell script file to avoid quoting issues and capture results.
