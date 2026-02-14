# Database Scripts (MySQL 5.7)

This folder contains the inferred schema for the MTM Waitlist Application 2.0 database.

Execution order (recommended):
1. schema/000_create_database.sql
2. schema/tables/*.sql (in numeric order)
3. schema/views/*.sql
4. routines/procedures/*.sql
5. routines/functions/*.sql
6. triggers/*.sql
7. seed/*.sql

Notes:
- The schema is inferred from application code and may need adjustments if your live schema differs.
- The application uses SELECT * with ordinal reads on waitlist_active. Column order in that table matters.
  Verify the order against your runtime expectations before deploying.
